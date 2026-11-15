using System.Linq;
using Microsoft.CodeAnalysis;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Resolves a binding against the right source: the data type of the scope, a named element, an ancestor type or the element itself.</summary>
internal static class BindingSourceResolver
{
    /// <summary>The resolution, a short text for the source column ("ElementName=box"), and the evidence for a verified result.</summary>
    public static (BindingResolution Resolution, string? Source, string Evidence) Resolve(XamlContext context, XamlNode node, MarkupExtension binding)
    {
        var path = binding.NamedText("Path") ?? binding.FirstPositionalText;
        var parsed = BindingPathParser.Parse(path);

        if (binding.Named.ContainsKey("Source"))
        {
            return (BindingResolution.Unresolved("a Source=... binding is not modelled"), "Source", string.Empty);
        }

        var elementName = binding.NamedText("ElementName") ?? (parsed.Source == BindingSource.ElementName ? parsed.SourceText : null);
        if (elementName is not null)
        {
            return (FromElement(context, node, elementName, parsed), $"ElementName={elementName}", $"ElementName {elementName}");
        }

        if (binding.NamedExtension("RelativeSource") is { } relative)
        {
            return (FromRelative(context, node, relative, parsed), "RelativeSource " + Describe(relative), "RelativeSource " + Describe(relative));
        }

        switch (parsed.Source)
        {
            case BindingSource.Self:
                return (FromType(context, context.Types.Resolve(context.Document, node, node.TypeText).Symbol, parsed, "$self"), "$self", "$self");
            case BindingSource.Parent:
                return (FromParent(context, node, parsed), "$parent[" + parsed.SourceText + "]", "$parent[" + parsed.SourceText + "]");
        }

        var (inScope, symbol, text) = context.DataTypeAt(node);
        if (!inScope)
        {
            return (BindingResolution.Unresolved("no x:DataType"), null, string.Empty);
        }

        return symbol is null
            ? (BindingResolution.Unresolved($"the data type '{text}' could not be resolved to a C# type"), null, string.Empty)
            : (context.Bindings.Resolve(symbol, path), null, $"x:DataType {text}");
    }

    private static BindingResolution FromElement(XamlContext context, XamlNode node, string name, BindingPath parsed)
    {
        var target = context.Document.Nodes.FirstOrDefault(n => n.Name == name);
        if (target is null)
        {
            return BindingResolution.Unresolved($"no element named '{name}' in this file");
        }

        var symbol = context.Types.Resolve(context.Document, target, target.TypeText).Symbol;
        return FromType(context, symbol, parsed, $"the element '{name}' ({target.TypeText})");
    }

    private static BindingResolution FromRelative(XamlContext context, XamlNode node, MarkupExtension relative, BindingPath parsed)
    {
        var mode = relative.NamedText("Mode") ?? relative.FirstPositionalText;
        if (mode == "Self")
        {
            return FromType(context, context.Types.Resolve(context.Document, node, node.TypeText).Symbol, parsed, "RelativeSource Self");
        }

        var typeText = relative.NamedText("AncestorType") ?? relative.NamedExtension("AncestorType")?.FirstPositionalText;
        if (typeText is null)
        {
            return BindingResolution.Unresolved($"RelativeSource{(mode is null ? string.Empty : " " + mode)} has no AncestorType");
        }

        return FromType(context, context.Types.Resolve(context.Document, node, typeText).Symbol, parsed, $"AncestorType '{typeText}'");
    }

    private static BindingResolution FromParent(XamlContext context, XamlNode node, BindingPath parsed)
    {
        var typeText = parsed.SourceText?.Split(',')[0].Trim();
        if (string.IsNullOrEmpty(typeText) || char.IsDigit(typeText[0]))
        {
            return BindingResolution.Unresolved("$parent without an ancestor type has no known type here");
        }

        return FromType(context, context.Types.Resolve(context.Document, node, typeText).Symbol, parsed, $"$parent[{typeText}]");
    }

    private static BindingResolution FromType(XamlContext context, INamedTypeSymbol? symbol, BindingPath parsed, string what) =>
        symbol is null
            ? BindingResolution.Unresolved($"the source type of {what} is not known")
            : context.Bindings.ResolveSegments(symbol, parsed.Segments, parsed.Unsupported);

    private static string Describe(MarkupExtension relative)
    {
        var type = relative.NamedText("AncestorType") ?? relative.NamedExtension("AncestorType")?.FirstPositionalText;
        var mode = relative.NamedText("Mode") ?? relative.FirstPositionalText;
        return string.Join(' ', new[] { mode, type is null ? null : "AncestorType=" + type }.Where(p => p is not null));
    }
}
