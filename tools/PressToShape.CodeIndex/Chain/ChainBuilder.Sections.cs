using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Chain;

internal sealed partial class ChainBuilder
{
    private XamlFileRecord? XamlFileOfClass(string classId)
    {
        var edge = _view.Relationships.FirstOrDefault(e => e.Type == RelationshipType.CodeBehind && e.ToId == classId);
        return edge is null ? null : _view.XamlFileOf(edge.FromId);
    }

    /// <summary>A section node with a count in its label. An empty section says "none" and has no children.</summary>
    private static ChainNode Section(string title, IReadOnlyList<ChainNode> nodes) =>
        ChainNode.Of(nodes.Count == 0 ? $"{title} (none)" : $"{title} ({nodes.Count})", children: nodes);

    private static string? LocationOf(string? file, int? line) => file is null ? null : line is { } l ? $"{file}:{l}" : file;

    private ChainNode BindingsNode(XamlFileRecord? xaml, string viewModelId)
    {
        if (xaml is null)
        {
            return Section("Bindings", []);
        }

        var elements = xaml.Elements.ToDictionary(e => e.Id, StringComparer.Ordinal);
        var nodes = new List<ChainNode>();
        foreach (var binding in xaml.Bindings.Where(b => b.TargetId is not null && _view.SymbolOf(b.TargetId)?.ContainingId == viewModelId).OrderBy(b => b.Line).ThenBy(b => b.Id, StringComparer.Ordinal))
        {
            var element = elements.TryGetValue(binding.ElementId, out var e) ? e.Type : "?";
            var children = binding.IsCommand ? [HandlerNode(_analysis.HandlerOf(binding.TargetId!))] : new List<ChainNode>();
            nodes.Add(ChainNode.Of($"{element}.{binding.Property} = {binding.BindingType} {binding.Path}", binding.Id, $"{xaml.File}:{binding.Line}", binding.Confidence, binding.Reason, children));
        }

        return Section("Bindings", nodes);
    }

    private static ChainNode HandlerNode(HandlerInfo handler) => handler.IsKnown
        ? ChainNode.Of($"Handler {handler.MethodName}", handler.MethodId, handler.Location, handler.Confidence, handler.Reason)
        : ChainNode.Of("Handler not statically known", reason: handler.Reason);

    private ChainNode ServicesNode(string viewModelId)
    {
        var nodes = _analysis.Services(viewModelId).Select(ServiceNode).ToList();
        return Section("Services", nodes);
    }

    private ChainNode ServiceNode(ServiceDependency service)
    {
        var symbol = _view.SymbolOf(service.TypeId);
        var children = new List<ChainNode>();
        if (service.IsInterface)
        {
            if (service.Implementations.Count == 0)
            {
                children.Add(ChainNode.Of("No implementation in the solution"));
            }

            children.AddRange(service.Implementations.Select(i => ChainNode.Of(
                $"Implemented by {i.Name}",
                i.TypeId,
                LocationOfSymbol(i.TypeId),
                Confidence.Verified,
                "Implements",
                CreatedAt(i.CreatedAt))));
        }
        else
        {
            children.AddRange(CreatedAt(service.CreatedAt));
        }

        var kind = service.IsInterface ? "interface" : "class";
        return ChainNode.Of($"Service {service.Name} ({kind})", service.TypeId, symbol is null ? null : $"{symbol.File}:{symbol.StartLine}", Confidence.Verified, "InjectedDependency", children);
    }

    private static IEnumerable<ChainNode> CreatedAt(IReadOnlyList<Place> places) => places.Count == 0
        ? [ChainNode.Of("No creation site found")]
        : places.Select(p => ChainNode.Of($"Created in {p.FromName}", p.FromId, p.Location, Confidence.Verified, "Calls"));

    private string? LocationOfSymbol(string id) => _view.SymbolOf(id) is { } symbol ? $"{symbol.File}:{symbol.StartLine}" : null;

    private ChainNode ModelsNode(string viewModelId)
    {
        var nodes = _analysis.Models(viewModelId)
            .Select(m => ChainNode.Of($"Model {m.Name} ({m.Project}, {m.References} references)", m.TypeId, LocationOfSymbol(m.TypeId), Confidence.Verified, "References"))
            .ToList();
        return Section("Models", nodes);
    }

    private ChainNode TestsNode(IEnumerable<string> ids)
    {
        var nodes = _analysis.TestsFor(ids)
            .Select(t => ChainNode.Of($"Test class {t.Name}", t.ClassId, $"{t.File}:{t.Line}", Confidence.Verified, $"{t.References} references"))
            .ToList();
        return Section("Tests", nodes);
    }

    /// <summary>The resources, converters, styles and templates of the view's XAML file, with where they are defined.</summary>
    private ChainNode LookAndFeel(XamlFileRecord xaml)
    {
        var nodes = new List<ChainNode>();
        foreach (var group in xaml.ResourceRefs.GroupBy(r => (r.RefType, r.Key)).OrderBy(g => g.Key.Key, StringComparer.Ordinal).ThenBy(g => g.Key.RefType, StringComparer.Ordinal))
        {
            var first = group.OrderBy(r => r.Line).First();
            var targets = group.SelectMany(r => r.TargetIds).Distinct(StringComparer.Ordinal).OrderBy(t => t, StringComparer.Ordinal).ToList();
            nodes.Add(ResourceNode(first, targets));
        }

        var converters = _view.Relationships
            .Where(e => e.Type == RelationshipType.UsesConverter && e.FromId.StartsWith(xaml.Id + "#", StringComparison.Ordinal))
            .Select(e => e.ToId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal);
        nodes.AddRange(converters.Select(id => ChainNode.Of($"Converter {_analysis.NameOf(id)}", id, LocationOfSymbol(id), Confidence.Verified, "UsesConverter")));
        nodes.AddRange(xaml.Styles.OrderBy(s => s.Line).Select(s => ChainNode.Of($"Style {s.Selector}", s.Id, $"{xaml.File}:{s.Line}")));
        nodes.AddRange(xaml.Templates.OrderBy(t => t.Line).Select(t => ChainNode.Of($"Template {t.TemplateType}{(t.Target is null ? string.Empty : " for " + t.Target)}", t.Id, $"{xaml.File}:{t.Line}")));
        return Section("Look and feel", nodes);
    }

    private ChainNode ResourceNode(XamlResourceRefRecord reference, IReadOnlyList<string> targets)
    {
        var label = $"{reference.RefType} {reference.Key}";
        if (targets.Count == 0)
        {
            return ChainNode.Of(label, null, $"line {reference.Line}", Confidence.Inferred, reference.Reason);
        }

        var definitions = targets.Select(t => ChainNode.Of(XamlIds.Describe(t), t, DefinitionLocation(t), Confidence.Verified, "ReferencesResource")).ToList();
        return targets.Count == 1
            ? ChainNode.Of(label, targets[0], definitions[0].Location, Confidence.Verified, "ReferencesResource")
            : ChainNode.Of(label, null, null, Confidence.Verified, "ReferencesResource", definitions);
    }

    /// <summary>The file and line of a resource definition (<c>R:path#...</c>).</summary>
    private string? DefinitionLocation(string resourceId)
    {
        var hash = resourceId.IndexOf('#', StringComparison.Ordinal);
        if (hash < 0)
        {
            return null;
        }

        var path = resourceId[2..hash];
        var file = _view.XamlFileOf(XamlIds.File(path));
        var resource = file?.Resources.FirstOrDefault(r => r.Id == resourceId);
        return resource is null ? path : $"{path}:{resource.Line}";
    }
}
