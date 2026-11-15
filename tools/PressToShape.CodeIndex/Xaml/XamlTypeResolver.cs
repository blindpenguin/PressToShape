using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Maps <c>prefix:Name</c> to a C# type. <c>using:</c> and <c>clr-namespace:</c> namespaces map directly. XML namespace URIs (the default Avalonia namespace)
/// are mapped with the <c>XmlnsDefinition</c> attributes of the assemblies that the compilation references, so no namespace list is hardcoded.
/// </summary>
internal sealed class XamlTypeResolver
{
    private const string UsingPrefix = "using:";
    private const string ClrNamespacePrefix = "clr-namespace:";

    private readonly Compilation _compilation;
    private readonly IReadOnlySet<string> _knownIds;
    private Dictionary<string, List<string>>? _xmlnsDefinitions;

    public XamlTypeResolver(Compilation compilation, IReadOnlySet<string> knownIds)
    {
        ArgumentNullException.ThrowIfNull(compilation);
        ArgumentNullException.ThrowIfNull(knownIds);
        _compilation = compilation;
        _knownIds = knownIds;
    }

    /// <summary>Resolves an element or attribute type name such as <c>desktop:MenuBarView</c> or <c>Button</c> at the given node.</summary>
    public ResolvedType Resolve(XamlDocument document, XamlNode node, string typeText)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(typeText);

        var (prefix, local) = SplitPrefix(typeText);
        var uri = document.ResolvePrefix(node, prefix);
        var symbol = uri is null || local.Length == 0 ? null : Find(uri, local);
        return Result(symbol, typeText);
    }

    /// <summary>
    /// Splits <c>prefix:Type.Member</c> (an attached property or an <c>x:Static</c> member) into the owner type and the member name.
    /// The member is null when the text has no dot after the prefix.
    /// </summary>
    public (ResolvedType Owner, string? Member) ResolveMember(XamlDocument document, XamlNode node, string text)
    {
        var (prefix, rest) = SplitPrefix(text);
        var dot = rest.LastIndexOf('.');
        if (dot <= 0)
        {
            return (Resolve(document, node, text), null);
        }

        var ownerText = prefix.Length == 0 ? rest[..dot] : $"{prefix}:{rest[..dot]}";
        return (Resolve(document, node, ownerText), rest[(dot + 1)..]);
    }

    private INamedTypeSymbol? Find(string uri, string local)
    {
        if (ClrNamespaceOf(uri) is { } clrNamespace)
        {
            return _compilation.GetTypeByMetadataName(Qualify(clrNamespace, local));
        }

        return XmlnsDefinitions().TryGetValue(uri, out var namespaces)
            ? namespaces.Select(ns => _compilation.GetTypeByMetadataName(Qualify(ns, local))).FirstOrDefault(t => t is not null)
            : null;
    }

    private ResolvedType Result(INamedTypeSymbol? symbol, string typeText)
    {
        if (symbol is null)
        {
            return new ResolvedType(null, typeText, null);
        }

        var id = symbol.GetDocumentationCommentId();
        return new ResolvedType(symbol, symbol.ToDisplayString(SymbolFormats.Qualified), id is not null && _knownIds.Contains(id) ? id : null);
    }

    private Dictionary<string, List<string>> XmlnsDefinitions()
    {
        if (_xmlnsDefinitions is not null)
        {
            return _xmlnsDefinitions;
        }

        var map = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var assemblies = _compilation.SourceModule.ReferencedAssemblySymbols.Prepend(_compilation.Assembly);
        foreach (var attribute in assemblies.SelectMany(a => a.GetAttributes()))
        {
            if (attribute.AttributeClass?.Name == "XmlnsDefinitionAttribute"
                && attribute.AttributeClass.ContainingNamespace.ToDisplayString() is "Avalonia.Metadata" or "System.Windows.Markup"
                && attribute.ConstructorArguments is [{ Value: string uri }, { Value: string clrNamespace }, ..])
            {
                if (!map.TryGetValue(uri, out var list))
                {
                    map[uri] = list = [];
                }

                list.Add(clrNamespace);
            }
        }

        foreach (var list in map.Values)
        {
            list.Sort(StringComparer.Ordinal);
        }

        return _xmlnsDefinitions = map;
    }

    /// <summary>The CLR namespace of a <c>using:</c> or <c>clr-namespace:</c> declaration, or null for an XML namespace URI.</summary>
    private static string? ClrNamespaceOf(string uri)
    {
        if (uri.StartsWith(UsingPrefix, StringComparison.Ordinal))
        {
            return uri[UsingPrefix.Length..].Trim();
        }

        if (uri.StartsWith(ClrNamespacePrefix, StringComparison.Ordinal))
        {
            var rest = uri[ClrNamespacePrefix.Length..];
            var semicolon = rest.IndexOf(';', StringComparison.Ordinal);
            return (semicolon < 0 ? rest : rest[..semicolon]).Trim();
        }

        return null;
    }

    private static string Qualify(string clrNamespace, string local) => clrNamespace.Length == 0 ? local : $"{clrNamespace}.{local}";

    private static (string Prefix, string Local) SplitPrefix(string text)
    {
        var colon = text.IndexOf(':', StringComparison.Ordinal);
        return colon < 0 ? (string.Empty, text) : (text[..colon], text[(colon + 1)..]);
    }
}
