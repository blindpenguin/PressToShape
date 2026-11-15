using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Collects the bindings of a XAML file as records and as <c>BindsTo</c> relationships, and links converters. A binding is verified only when its path resolved
/// against a known source type. <c>ReflectionBinding</c> is resolved at run time, so a match is only inferred. <c>TemplateBinding</c> targets the templated control.
/// </summary>
internal static class BindingCollector
{
    private static readonly string[] BindingNames = ["Binding", "CompiledBinding", "ReflectionBinding", "TemplateBinding"];

    public static List<XamlBindingRecord> Collect(XamlContext context)
    {
        var records = new List<XamlBindingRecord>();
        foreach (var node in context.Document.Nodes.Where(n => !n.IsPropertyElement))
        {
            foreach (var attribute in node.Attributes.Where(a => a.Role == XamlAttributeRole.MarkupExtension))
            {
                if (attribute.Parsed is not { } extension)
                {
                    context.Findings.Add($"The markup '{attribute.Value}' of '{attribute.Name}' at line {attribute.Line} could not be read.");
                    continue;
                }

                if (BindingNames.Contains(extension.Name))
                {
                    records.Add(CollectBinding(context, node, attribute, extension));
                }
            }
        }

        return records;
    }

    private static XamlBindingRecord CollectBinding(XamlContext context, XamlNode node, XamlAttribute attribute, MarkupExtension extension)
    {
        var elementId = context.ElementId(node);
        var property = TargetProperty(node, attribute);
        var id = XamlIds.Binding(elementId, property);
        var path = extension.NamedText("Path") ?? extension.FirstPositionalText;

        string? source = null;
        BindingResolution resolution;
        var evidence = string.Empty;
        if (extension.Name == "TemplateBinding")
        {
            resolution = BindingResolution.Unresolved("a TemplateBinding targets the templated control, not the data context");
        }
        else
        {
            (resolution, source, evidence) = BindingSourceResolver.Resolve(context, node, extension);
        }

        var verified = resolution.Status == BindingStatus.Verified && extension.Name != "ReflectionBinding";
        var reason = resolution.Status == BindingStatus.Verified
            ? (extension.Name == "ReflectionBinding" ? $"a ReflectionBinding is resolved at run time; the path matches {evidence}" : evidence)
            : resolution.Reason;
        var targetId = resolution.Status == BindingStatus.Verified ? resolution.PropertyId : null;

        if (targetId is not null)
        {
            context.AddEdge(RelationshipType.BindsTo, id, targetId, node, verified ? Confidence.Verified : Confidence.Inferred, reason);
        }

        if (resolution.SegmentNotFound && extension.Name is "Binding" or "CompiledBinding" && context.DataTypeAt(node).InScope)
        {
            context.Findings.Add($"Binding '{path}' at line {node.Line} does not resolve: {resolution.Reason}.");
        }

        var converter = LinkConverter(context, node, id, extension);
        return new XamlBindingRecord(
            id,
            elementId,
            property,
            extension.Name,
            path,
            converter,
            extension.NamedText("Mode"),
            source,
            targetId,
            resolution.Status == BindingStatus.Verified ? resolution.TargetTypeId : null,
            resolution.IsCommand && targetId is not null,
            verified ? Confidence.Verified : Confidence.Inferred,
            reason,
            attribute.Line);
    }

    /// <summary>The property that the markup sets. For <c>&lt;Setter Property="X" Value="{...}"&gt;</c> that is X, not Value.</summary>
    internal static string TargetProperty(XamlNode node, XamlAttribute attribute) =>
        attribute.Name == "Value" && XamlNames.IsType(node, "Setter") && node.AttributeValue("Property") is { } setterProperty ? setterProperty : attribute.Name;

    private static string? LinkConverter(XamlContext context, XamlNode node, string bindingId, MarkupExtension binding)
    {
        if (binding.NamedExtension("Converter") is not { } converter)
        {
            return binding.NamedText("Converter");
        }

        var description = $"{converter.Name} {converter.FirstPositionalText}".Trim();
        switch (converter.Name)
        {
            case "StaticResource" or "DynamicResource":
                LinkResourceConverter(context, node, bindingId, converter, description);
                break;
            case "x:Static":
                LinkStaticConverter(context, node, bindingId, converter, description);
                break;
            default:
                context.Findings.Add($"The converter '{description}' at line {node.Line} could not be resolved to a class.");
                break;
        }

        return description;
    }

    private static void LinkResourceConverter(XamlContext context, XamlNode node, string bindingId, MarkupExtension converter, string description)
    {
        var key = converter.FirstPositionalText ?? converter.NamedText("ResourceKey");
        var lookup = key is null ? ResourceLookup.NotFound() : context.Resources.Resolve(context.Document, node, key);
        var hit = lookup.Hits.FirstOrDefault();
        var type = hit is null ? null : context.Types.Resolve(hit.Document, hit.Node, hit.Node.TypeText);
        if (hit is null || type?.SolutionId is null)
        {
            var why = hit is null ? lookup.Reason : $"the resource is a '{hit.Node.TypeText}', not a class of this solution";
            context.Findings.Add($"The converter '{description}' at line {node.Line} could not be resolved to a class: {why}.");
            return;
        }

        context.AddEdge(RelationshipType.UsesConverter, bindingId, type.SolutionId, node, Confidence.Verified, $"{description} defined at {hit.Document.Path}:{hit.Node.Line}");
    }

    private static void LinkStaticConverter(XamlContext context, XamlNode node, string bindingId, MarkupExtension converter, string description)
    {
        var text = converter.FirstPositionalText;
        var (owner, member) = text is null ? (null, null) : context.Types.ResolveMember(context.Document, node, text);
        if (owner?.SolutionId is not { } id || member is null || !(owner.Symbol?.GetMembers(member).Any() ?? false))
        {
            context.Findings.Add($"The converter '{description}' at line {node.Line} could not be resolved to a class of this solution.");
            return;
        }

        context.AddEdge(RelationshipType.UsesConverter, bindingId, id, node, Confidence.Verified, description);
    }
}
