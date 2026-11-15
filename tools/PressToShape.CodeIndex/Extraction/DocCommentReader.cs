using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Extraction;

/// <summary>Turns the XML documentation comment of a symbol into a <see cref="DocRecord"/>. Nothing is invented for undocumented symbols.</summary>
internal static class DocCommentReader
{
    private const int MaxInheritDepth = 8;

    public static DocRecord Read(ISymbol symbol, Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        ArgumentNullException.ThrowIfNull(compilation);

        return Resolve(symbol, compilation, 0).Doc;
    }

    private static (DocRecord Doc, string? SourceId) Resolve(ISymbol symbol, Compilation compilation, int depth)
    {
        var parsed = Parse(symbol);
        if (parsed is null)
        {
            return (DocRecord.None, null);
        }

        var (own, inheritCref) = parsed.Value;
        if (inheritCref is null || depth >= MaxInheritDepth)
        {
            return (own, null);
        }

        foreach (var candidate in InheritCandidates(symbol, inheritCref, compilation))
        {
            var (inherited, sourceId) = Resolve(candidate, compilation, depth + 1);
            if (inherited.HasDocumentation)
            {
                return (Merge(own, inherited, sourceId ?? candidate.GetDocumentationCommentId()), sourceId ?? candidate.GetDocumentationCommentId());
            }
        }

        return (own, null);
    }

    /// <summary>Parses the symbol's own comment. The second value is the inheritdoc marker: null when absent, otherwise the cref or an empty string.</summary>
    private static (DocRecord Doc, string? InheritCref)? Parse(ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml(expandIncludes: true);
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        XElement root;
        try
        {
            root = XDocument.Parse(xml).Root ?? throw new XmlException("No root element.");
        }
        catch (XmlException)
        {
            return null;
        }

        var summary = TextOf(root.Element("summary"));
        var remarks = TextOf(root.Element("remarks"));
        var returns = TextOf(root.Element("returns"));
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var param in root.Elements("param"))
        {
            var name = (string?)param.Attribute("name");
            var text = TextOf(param);
            if (name is not null && text is not null)
            {
                parameters[name] = text;
            }
        }

        var exceptions = root.Elements("exception")
            .Select(e => new ExceptionDoc(CrefName((string?)e.Attribute("cref")), TextOf(e) ?? string.Empty))
            .ToList();
        var inherit = root.Element("inheritdoc");
        var hasText = summary is not null || remarks is not null || returns is not null || parameters.Count > 0 || exceptions.Count > 0;

        var doc = new DocRecord(hasText, summary, remarks, parameters, returns, exceptions, null);
        return (doc, inherit is null ? null : (string?)inherit.Attribute("cref") ?? string.Empty);
    }

    private static DocRecord Merge(DocRecord own, DocRecord inherited, string? sourceId)
    {
        var parameters = new Dictionary<string, string>(inherited.Params, StringComparer.Ordinal);
        foreach (var pair in own.Params)
        {
            parameters[pair.Key] = pair.Value;
        }

        return new DocRecord(
            true,
            own.Summary ?? inherited.Summary,
            own.Remarks ?? inherited.Remarks,
            parameters,
            own.Returns ?? inherited.Returns,
            own.Exceptions.Count > 0 ? own.Exceptions : inherited.Exceptions,
            sourceId);
    }

    private static IEnumerable<ISymbol> InheritCandidates(ISymbol symbol, string cref, Compilation compilation)
    {
        if (cref.Length > 0)
        {
            if (DocumentationCommentId.GetFirstSymbolForDeclarationId(cref, compilation) is { } target)
            {
                yield return target;
            }

            yield break;
        }

        switch (symbol)
        {
            case IMethodSymbol { OverriddenMethod: { } overridden }:
                yield return overridden;
                break;
            case IPropertySymbol { OverriddenProperty: { } overriddenProperty }:
                yield return overriddenProperty;
                break;
            case IEventSymbol { OverriddenEvent: { } overriddenEvent }:
                yield return overriddenEvent;
                break;
        }

        if (symbol is INamedTypeSymbol type)
        {
            if (type.BaseType is { } baseType)
            {
                yield return baseType;
            }

            foreach (var iface in type.Interfaces)
            {
                yield return iface;
            }

            yield break;
        }

        if (symbol.ContainingType is not { } containing)
        {
            yield break;
        }

        foreach (var iface in containing.AllInterfaces)
        {
            foreach (var member in iface.GetMembers())
            {
                if (SymbolEqualityComparer.Default.Equals(containing.FindImplementationForInterfaceMember(member), symbol))
                {
                    yield return member;
                }
            }
        }
    }

    private static string? TextOf(XElement? element)
    {
        if (element is null)
        {
            return null;
        }

        var paragraphs = new List<string>();
        var current = new StringBuilder();
        Append(element, current, paragraphs);
        Flush(current, paragraphs);
        return paragraphs.Count == 0 ? null : string.Join("\n\n", paragraphs);
    }

    private static void Append(XElement element, StringBuilder current, List<string> paragraphs)
    {
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XText text:
                    current.Append(text.Value);
                    break;
                case XElement child when child.Name == "para":
                    Flush(current, paragraphs);
                    Append(child, current, paragraphs);
                    Flush(current, paragraphs);
                    break;
                case XElement child when child.Name.LocalName is "see" or "seealso":
                    current.Append((string?)child.Attribute("langword") ?? (child.IsEmpty ? CrefName((string?)child.Attribute("cref")) : child.Value));
                    break;
                case XElement child when child.Name.LocalName is "paramref" or "typeparamref":
                    current.Append((string?)child.Attribute("name"));
                    break;
                case XElement child:
                    Append(child, current, paragraphs);
                    break;
            }
        }
    }

    private static void Flush(StringBuilder current, List<string> paragraphs)
    {
        var text = string.Join(' ', current.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        current.Clear();
        if (text.Length > 0)
        {
            paragraphs.Add(text);
        }
    }

    /// <summary>Shows a documentation id such as <c>M:N.C.M(System.Int32)</c> as the member name <c>N.C.M</c>.</summary>
    private static string CrefName(string? cref)
    {
        if (string.IsNullOrEmpty(cref))
        {
            return string.Empty;
        }

        var name = cref.Length > 2 && cref[1] == ':' ? cref[2..] : cref;
        var paren = name.IndexOf('(', StringComparison.Ordinal);
        return paren >= 0 ? name[..paren] : name;
    }
}
