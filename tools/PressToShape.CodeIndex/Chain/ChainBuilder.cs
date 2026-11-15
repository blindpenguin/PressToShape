using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Query;
using PressToShape.CodeIndex.Xaml;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Chain;

/// <summary>
/// Builds the chain of a view, a view model or a command from the committed index files. It reads only index files and never compiles anything.
/// Every link says verified or inferred, and anything that is missing or not provable is a node that says so.
/// </summary>
internal sealed partial class ChainBuilder
{
    public const int MaxDepth = 5;

    private readonly NameResolver _resolver;
    private readonly ReaderIndexView _view;
    private readonly IndexAnalysis _analysis;

    public ChainBuilder(IndexReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _resolver = new NameResolver(reader);
        _view = new ReaderIndexView(reader);
        _analysis = new IndexAnalysis(_view);
    }

    /// <param name="query">A name or an id of a view (class or XAML file), a view model or a command property.</param>
    /// <param name="depth">1 lists the child views of a view. More levels expand them (at most <see cref="MaxDepth"/>).</param>
    public ChainResult Build(string query, int depth = 1)
    {
        var resolution = _resolver.Resolve(query);
        if (resolution.Status == QueryStatus.NotFound)
        {
            return new ChainResult(ChainStatus.NotFound, null, [], $"No symbol named '{query}'. Try: find {query}");
        }

        if (resolution.Status == QueryStatus.Ambiguous)
        {
            return new ChainResult(ChainStatus.Ambiguous, null, resolution.Matches, $"'{query}' is ambiguous. Use one of these ids.");
        }

        var entry = resolution.Matches[0];
        var depthLimit = Math.Clamp(depth, 1, MaxDepth);
        var (type, id) = Classify(entry);
        return type switch
        {
            SubjectType.View => Found(ViewNode(id!, depthLimit, [id!])),
            SubjectType.ViewModel => Found(ViewModelSubject(id!)),
            SubjectType.Command => Found(CommandSubject(id!)),
            _ => new ChainResult(ChainStatus.NotSupported, null, [], $"'{entry.Name}' is a {entry.SymbolType}. chain accepts a view (class or XAML file), a view model that a view links to, or a command property."),
        };
    }

    private enum SubjectType
    {
        View,
        ViewModel,
        Command,
        Other,
    }

    private static ChainResult Found(ChainNode root) => new(ChainStatus.Found, root, [], null);

    private (SubjectType Type, string? Id) Classify(NameEntry entry)
    {
        if (XamlIds.IsXaml(entry.Id))
        {
            return entry.SymbolType == "View" && _view.XamlFileOf(entry.Id)?.ClassId is { } viewClass ? (SubjectType.View, viewClass) : (SubjectType.Other, null);
        }

        var symbol = _view.SymbolOf(entry.Id);
        if (symbol is null)
        {
            return (SubjectType.Other, null);
        }

        if (symbol.SymbolType is SymbolType.Class or SymbolType.Record or SymbolType.Struct)
        {
            if (XamlFileOfClass(symbol.Id) is not null || _analysis.ViewModelsOf(symbol.Id).Count > 0)
            {
                return (SubjectType.View, symbol.Id);
            }

            return _analysis.ViewsOf(symbol.Id).Count > 0 ? (SubjectType.ViewModel, symbol.Id) : (SubjectType.Other, null);
        }

        return symbol.SymbolType == SymbolType.Property && _analysis.IsCommand(symbol.Id) ? (SubjectType.Command, symbol.Id) : (SubjectType.Other, null);
    }

    private ChainNode ViewNode(string classId, int depth, HashSet<string> visited)
    {
        var symbol = _view.SymbolOf(classId);
        var xaml = XamlFileOfClass(classId);
        var children = new List<ChainNode>();
        if (xaml is not null)
        {
            children.Add(ChainNode.Of($"XAML file {xaml.File}", xaml.Id, $"{xaml.File}:1", Confidence.Verified, "x:Class"));
        }

        var links = _analysis.ViewModelsOf(classId);
        if (links.Count == 0)
        {
            children.Add(ChainNode.Of("No view model link found", reason: "no x:DataType, DataContext binding or DataContext assignment names one"));
        }

        foreach (var group in links.GroupBy(l => l.ViewModelId, StringComparer.Ordinal))
        {
            children.Add(ViewModelNode(group.ToList(), xaml, classId, depth, visited));
        }

        if (xaml is not null)
        {
            children.Add(LookAndFeel(xaml));
        }

        children.Add(TestsNode([classId]));
        return ChainNode.Of($"View {symbol?.Name ?? classId}", classId, symbol is null ? null : $"{symbol.File}:{symbol.StartLine}", children: children);
    }

    private ChainNode ViewModelNode(IReadOnlyList<ViewModelLink> links, XamlFileRecord? xaml, string viewId, int depth, HashSet<string> visited)
    {
        var primary = links.OrderBy(l => l.Confidence).First();
        var viewModelId = primary.ViewModelId;
        var symbol = _view.SymbolOf(viewModelId);
        var children = new List<ChainNode>();
        children.AddRange(links.Where(l => l != primary).Select(l => ChainNode.Of("Also linked", null, LocationOf(l.File, l.Line), l.Confidence, l.Reason)));

        children.Add(BindingsNode(xaml, viewModelId));
        children.Add(ServicesNode(viewModelId));
        children.Add(ModelsNode(viewModelId));
        if (xaml is not null)
        {
            children.Add(ChildViewsNode(xaml, viewId, depth, visited));
        }

        children.Add(TestsNode([viewModelId]));
        return ChainNode.Of($"View model {symbol?.Name ?? viewModelId}", viewModelId, symbol is null ? null : $"{symbol.File}:{symbol.StartLine}", primary.Confidence, primary.Reason, children);
    }

    private ChainNode ChildViewsNode(XamlFileRecord xaml, string viewId, int depth, HashSet<string> visited)
    {
        var edges = _view.Relationships
            .Where(e => e.Type == RelationshipType.UsesViewModel && e.File == xaml.File && e.FromId != viewId)
            .GroupBy(e => e.FromId, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        var nodes = new List<ChainNode>();
        foreach (var group in edges)
        {
            var edge = group.OrderBy(e => e.Confidence).First();
            var children = new List<ChainNode> { ChainNode.Of($"View model {_analysis.NameOf(edge.ToId)}", edge.ToId, null, edge.Confidence, edge.Reason) };
            if (depth >= 2 && visited.Add(group.Key))
            {
                children.AddRange(ViewNode(group.Key, depth - 1, visited).Children);
            }

            nodes.Add(ChainNode.Of($"Child view {_analysis.NameOf(group.Key)}", group.Key, LocationOf(edge.File, edge.Line), edge.Confidence, edge.Reason, children));
        }

        return Section("Child views", nodes);
    }
}
