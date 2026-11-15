using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Resolves one XAML document against the compilation of its project: elements, code-behind, view model links, bindings, resources,
/// styles, templates, handlers and includes. Everything it cannot prove is recorded as unresolved or inferred, never as verified.
/// </summary>
internal sealed class XamlResolver
{
    private readonly XamlWorld _world;
    private readonly Compilation _compilation;
    private readonly IReadOnlySet<string> _knownIds;
    private readonly XamlTypeResolver _types;
    private readonly BindingResolver _bindings;
    private readonly ResourceResolver _resources;
    private readonly ViewLocatorConvention? _convention;

    public XamlResolver(XamlWorld world, Compilation compilation, IReadOnlySet<string> knownIds, ViewLocatorConvention? convention)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(compilation);
        ArgumentNullException.ThrowIfNull(knownIds);
        _world = world;
        _compilation = compilation;
        _knownIds = knownIds;
        _convention = convention;
        _types = new XamlTypeResolver(compilation, knownIds);
        _bindings = new BindingResolver(knownIds);
        _resources = new ResourceResolver(world);
    }

    public XamlResolution Resolve(XamlDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var context = new XamlContext(_world, document, _compilation, _knownIds, _types, _bindings, _resources, _convention);
        LinkCodeBehind(context);
        ViewModelLinker.Link(context);
        var bindings = BindingCollector.Collect(context);
        var resourceRefs = ResourceReferenceCollector.Collect(context);
        var structure = XamlStructureCollector.Collect(context);

        var findings = document.Findings.Concat(context.Findings).ToList();
        var root = document.Root;
        var dataTypeText = XamlContext.DataTypeText(root);
        var file = new XamlFileRecord(
            context.FileId,
            document.Project,
            document.Path,
            root.TypeText,
            document.XClass,
            context.ClassId,
            dataTypeText,
            dataTypeText is null ? null : _types.Resolve(document, root, dataTypeText).SolutionId,
            Namespaces(document),
            BuildElements(context),
            bindings,
            structure.Resources,
            resourceRefs,
            structure.Styles,
            structure.Templates,
            structure.Handlers,
            findings);
        return new XamlResolution(file, context.Edges.OrderBy(e => e.Type).ThenBy(e => e.FromId, StringComparer.Ordinal).ThenBy(e => e.ToId, StringComparer.Ordinal).ThenBy(e => e.Line).ToList());
    }

    private static void LinkCodeBehind(XamlContext context)
    {
        var document = context.Document;
        if (document.XClass is not { } name)
        {
            return;
        }

        if (context.ClassId is { } classId)
        {
            context.AddEdge(RelationshipType.CodeBehind, context.FileId, classId, document.Root, Confidence.Verified, "x:Class");
        }
        else
        {
            context.Findings.Add($"x:Class '{name}' is not a class of this solution (line {document.Root.Line}).");
        }
    }

    private List<XamlElementRecord> BuildElements(XamlContext context)
    {
        var document = context.Document;
        return document.Nodes.Select(node => new XamlElementRecord(
            context.ElementId(node),
            node.Parent is null ? null : context.ElementId(node.Parent),
            node.TypeText,
            node.IsPropertyElement ? null : _types.Resolve(document, node, node.TypeText).SolutionId,
            node.Name,
            node.Line,
            node.PropertyName)).ToList();
    }

    private static SortedDictionary<string, string> Namespaces(XamlDocument document)
    {
        var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var node in document.Nodes)
        {
            foreach (var (prefix, uri) in node.Namespaces)
            {
                map.TryAdd(prefix.Length == 0 ? "(default)" : prefix, uri);
            }
        }

        return map;
    }
}
