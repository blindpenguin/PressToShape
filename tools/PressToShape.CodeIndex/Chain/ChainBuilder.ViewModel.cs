using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Chain;

internal sealed partial class ChainBuilder
{
    /// <summary>The chain of a view model: the views that use it, its commands with handlers and bindings, services, models and tests.</summary>
    private ChainNode ViewModelSubject(string viewModelId)
    {
        var symbol = _view.SymbolOf(viewModelId);
        var views = _analysis.ViewsOf(viewModelId)
            .Select(l => ChainNode.Of($"View {_analysis.NameOf(l.ViewId)}", l.ViewId, LocationOf(l.File, l.Line), l.Confidence, l.Reason))
            .ToList();

        var children = new List<ChainNode>
        {
            Section("Views using it", views),
            CommandsNode(viewModelId),
            ServicesNode(viewModelId),
            ModelsNode(viewModelId),
            TestsNode([viewModelId]),
        };
        return ChainNode.Of($"View model {symbol?.Name ?? viewModelId}", viewModelId, symbol is null ? null : $"{symbol.File}:{symbol.StartLine}", children: children);
    }

    /// <summary>The chain of a command property: its handler, the XAML places that bind it, and the tests.</summary>
    private ChainNode CommandSubject(string propertyId)
    {
        var command = _analysis.CommandOf(propertyId)!;
        var symbol = _view.SymbolOf(propertyId);
        var owner = symbol?.ContainingId is { } containing ? _analysis.NameOf(containing) + "." : string.Empty;
        var children = new List<ChainNode>
        {
            HandlerNode(command.Handler),
            BoundAtNode(command),
            TestsNode([propertyId]),
        };
        return ChainNode.Of($"Command {owner}{command.Name}", propertyId, symbol is null ? null : $"{symbol.File}:{symbol.StartLine}", children: children);
    }

    private ChainNode CommandsNode(string viewModelId)
    {
        var nodes = new List<ChainNode>();
        foreach (var command in _analysis.Commands(viewModelId))
        {
            var children = new List<ChainNode> { HandlerNode(command.Handler) };
            children.AddRange(command.BoundAt.Select(p => ChainNode.Of($"Bound at {p.FromName}", p.FromId, p.Location, Confidence.Verified, "BindsTo")));
            nodes.Add(ChainNode.Of($"Command {command.Name}", command.PropertyId, LocationOfSymbol(command.PropertyId), children: children));
        }

        return Section("Commands", nodes);
    }

    private static ChainNode BoundAtNode(CommandInfo command) => Section(
        "Bound at",
        command.BoundAt.Select(p => ChainNode.Of(p.FromName, p.FromId, p.Location, Confidence.Verified, "BindsTo")).ToList());
}
