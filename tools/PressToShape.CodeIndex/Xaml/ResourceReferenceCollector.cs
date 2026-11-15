using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Collects <c>StaticResource</c> and <c>DynamicResource</c> uses. A found key gives verified <c>ReferencesResource</c> edges. A missing key is recorded with the reason.</summary>
internal static class ResourceReferenceCollector
{
    public static List<XamlResourceRefRecord> Collect(XamlContext context)
    {
        var records = new List<XamlResourceRefRecord>();
        foreach (var node in context.Document.Nodes.Where(n => !n.IsPropertyElement))
        {
            foreach (var attribute in node.Attributes.Where(a => a.Parsed is { Name: "StaticResource" or "DynamicResource" }))
            {
                var extension = attribute.Parsed!;
                var key = extension.FirstPositionalText ?? extension.NamedText("ResourceKey") ?? TypeKey(extension);
                if (key is null)
                {
                    context.Findings.Add($"The {extension.Name} at line {attribute.Line} has no key.");
                    continue;
                }

                records.Add(Collect(context, node, attribute, extension.Name, key));
            }
        }

        return records;
    }

    /// <summary>A resource keyed by a type, as in <c>{StaticResource {x:Type MenuItem}}</c>. The key is written the way it would be in an <c>x:Key</c>.</summary>
    private static string? TypeKey(MarkupExtension extension) =>
        extension.Positional.Count > 0 && extension.Positional[0] is MarkupNested { Extension: { Name: "x:Type", FirstPositionalText: { } type } }
            ? "{x:Type " + type + "}"
            : null;

    private static XamlResourceRefRecord Collect(XamlContext context, XamlNode node, XamlAttribute attribute, string refType, string key)
    {
        var elementId = context.ElementId(node);
        var property = BindingCollector.TargetProperty(node, attribute);
        var id = XamlIds.Binding(elementId, property);
        var lookup = context.Resources.Resolve(context.Document, node, key);

        foreach (var hit in lookup.Hits)
        {
            var variant = hit.Variant is null ? string.Empty : $" ({hit.Variant})";
            context.AddEdge(RelationshipType.ReferencesResource, id, hit.Id, node, Confidence.Verified, $"{refType} {key}{variant}");
        }

        return new XamlResourceRefRecord(
            id,
            elementId,
            property,
            key,
            refType,
            lookup.Hits.Select(h => h.Id).OrderBy(i => i, System.StringComparer.Ordinal).ToList(),
            lookup.IsFound ? Confidence.Verified : Confidence.Inferred,
            lookup.Reason,
            attribute.Line);
    }
}
