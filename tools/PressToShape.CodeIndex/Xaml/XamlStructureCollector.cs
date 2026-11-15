using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Collects resource definitions, styles, templates, event handlers, uses of solution classes and includes.
/// Events are recognized from the Roslyn symbol of the control, not from a list of names. A handler is verified only when exactly one method of the <c>x:Class</c> has that name.
/// </summary>
internal static class XamlStructureCollector
{
    public static XamlStructure Collect(XamlContext context)
    {
        var resources = CollectResources(context);
        var styles = CollectStyles(context);
        var templates = CollectTemplates(context);
        var handlers = CollectHandlers(context);
        LinkSolutionTypes(context);
        LinkIncludes(context);
        return new XamlStructure(resources, styles, templates, handlers);
    }

    private static List<XamlResourceRecord> CollectResources(XamlContext context) =>
        ResourceDictionaries.Definitions(context.Document)
            .Select(e => new XamlResourceRecord(
                e.Id,
                e.Key,
                e.Node.TypeText,
                context.Types.Resolve(context.Document, e.Node, e.Node.TypeText).SolutionId,
                e.Variant,
                e.Node.Line))
            .ToList();

    private static List<XamlStyleRecord> CollectStyles(XamlContext context)
    {
        var styles = new List<XamlStyleRecord>();
        foreach (var node in context.Document.Nodes.Where(n => XamlNames.IsType(n, "Style")))
        {
            var key = node.AttributeValue("x:Key");
            var selector = node.AttributeValue("Selector") ?? "(no selector)";
            var setters = new SortedSet<string>(StringComparer.Ordinal);
            var keys = new SortedSet<string>(StringComparer.Ordinal);
            Gather(node, setters, keys);
            styles.Add(new XamlStyleRecord(XamlIds.Style(context.Document.Path, key ?? selector, node.Line), selector, key, setters.ToList(), keys.ToList(), node.Line));
        }

        return styles;
    }

    /// <summary>Setter properties and referenced resource keys of a style, without the parts that belong to nested styles.</summary>
    private static void Gather(XamlNode style, SortedSet<string> setters, SortedSet<string> keys)
    {
        foreach (var attribute in style.Attributes.Where(a => a.Parsed is { Name: "StaticResource" or "DynamicResource" }))
        {
            if (attribute.Parsed!.FirstPositionalText is { } key)
            {
                keys.Add(key);
            }
        }

        if (XamlNames.IsType(style, "Setter") && style.AttributeValue("Property") is { } property)
        {
            setters.Add(property);
        }

        foreach (var child in style.Children.Where(c => !XamlNames.IsType(c, "Style")))
        {
            Gather(child, setters, keys);
        }
    }

    private static List<XamlTemplateRecord> CollectTemplates(XamlContext context)
    {
        var templates = new List<XamlTemplateRecord>();
        foreach (var node in context.Document.Nodes.Where(n => !n.IsPropertyElement && XamlNames.Local(n.TypeText) is "ControlTemplate" or "DataTemplate" or "TreeDataTemplate" or "HierarchicalDataTemplate"))
        {
            var target = node.AttributeValue("TargetType") ?? XamlContext.DataTypeText(node);
            templates.Add(new XamlTemplateRecord(
                context.ElementId(node),
                XamlNames.Local(node.TypeText),
                target,
                target is null ? null : context.Types.Resolve(context.Document, node, target).SolutionId,
                node.AttributeValue("x:Key"),
                node.Line));
        }

        return templates;
    }

    private static List<XamlHandlerRecord> CollectHandlers(XamlContext context)
    {
        var handlers = new List<XamlHandlerRecord>();
        foreach (var node in context.Document.Nodes.Where(n => !n.IsPropertyElement))
        {
            var candidates = node.Attributes.Where(a => a.Role == XamlAttributeRole.HandlerCandidate).ToList();
            if (candidates.Count == 0 || context.Types.Resolve(context.Document, node, node.TypeText).Symbol is not { } type)
            {
                continue;
            }

            foreach (var attribute in candidates.Where(a => FindEvent(type, a.Name) is not null))
            {
                handlers.Add(CollectHandler(context, node, attribute));
            }
        }

        return handlers;
    }

    private static XamlHandlerRecord CollectHandler(XamlContext context, XamlNode node, XamlAttribute attribute)
    {
        var elementId = context.ElementId(node);
        var id = XamlIds.Binding(elementId, attribute.Name);
        var (targetId, reason) = ResolveHandler(context, attribute.Value);

        if (targetId is not null)
        {
            context.AddEdge(RelationshipType.HandledBy, id, targetId, node, Confidence.Verified, $"event {attribute.Name} handler {attribute.Value}");
        }

        return new XamlHandlerRecord(id, elementId, attribute.Name, attribute.Value, targetId, targetId is null ? Confidence.Inferred : Confidence.Verified, reason, attribute.Line);
    }

    private static (string? TargetId, string? Reason) ResolveHandler(XamlContext context, string handler)
    {
        if (context.ClassSymbol is not { } classSymbol)
        {
            return (null, "the file has no x:Class code-behind class in this solution");
        }

        var methods = classSymbol.GetMembers(handler).OfType<IMethodSymbol>().Where(m => !m.IsImplicitlyDeclared).ToList();
        return methods.Count switch
        {
            0 => (null, $"no method '{handler}' in {classSymbol.ToDisplayString()}"),
            1 => (methods[0].GetDocumentationCommentId(), null),
            _ => (null, $"{methods.Count} methods named '{handler}' in {classSymbol.ToDisplayString()}, so the handler is ambiguous"),
        };
    }

    private static IEventSymbol? FindEvent(INamedTypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.GetMembers(name).OfType<IEventSymbol>().FirstOrDefault() is { } found)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>R9.3: elements and resources whose type is a class of the solution (custom controls, converters, views).</summary>
    private static void LinkSolutionTypes(XamlContext context)
    {
        foreach (var node in context.Document.Nodes.Where(n => !n.IsPropertyElement && n.Parent is not null))
        {
            if (context.Types.Resolve(context.Document, node, node.TypeText).SolutionId is { } id)
            {
                context.AddEdge(RelationshipType.UsesType, context.ElementId(node), id, node, Confidence.Verified, "element type");
            }
        }
    }

    /// <summary>R6.4: <c>StyleInclude</c>, <c>ResourceInclude</c> and <c>MergeResourceInclude</c> that point at a file of the solution.</summary>
    private static void LinkIncludes(XamlContext context)
    {
        foreach (var (node, source, target) in context.World.IncludesOf(context.Document))
        {
            if (target is not null)
            {
                context.AddEdge(RelationshipType.Includes, context.FileId, XamlIds.File(target.Path), node, Confidence.Verified, $"{XamlNames.Local(node.TypeText)} {source}");
            }
        }
    }
}
