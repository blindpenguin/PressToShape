using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// The edits of the selected nodes that need no mouse: change the type, make a segment straight or curved, delete, nudge and set the
/// position. The properties, the keys and the Delete button of the tablet use it, and so does nothing else, so the rules exist once.
/// A rectangle or an ellipse with selected nodes becomes a path in the same history step as the edit. Hidden and locked shapes are left
/// alone. Every call that changes something is one history step.
/// </summary>
internal sealed class NodeEditor
{
    private readonly EditorState _state;
    private Document? _lastNudgeDocument;
    private IReadOnlySet<NodeRef> _lastNudgeNodes = new HashSet<NodeRef>();

    public NodeEditor(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>Whether at least one selected node can be edited.</summary>
    public bool HasEditableNodes => Targets().Count > 0;

    /// <summary>The only selected node, or <c>null</c> when none or several nodes are selected.</summary>
    public NodeRef? SingleNode => Targets().SelectMany(t => t.Indices.Select(i => new NodeRef(t.Path.Id, i))).Take(2).ToList() is [var only] ? only : null;

    /// <summary>Whether exactly two neighboring nodes of one path are selected, so a segment can be made straight or curved.</summary>
    public bool CanChangeSegment => SelectedSegment() is not null;

    /// <summary>The type of the selected nodes, or <c>null</c> when none are selected or their types differ.</summary>
    public NodeType? TypeOfSelection
    {
        get
        {
            var types = Targets().SelectMany(t => t.Indices.Select(i => t.Path.Nodes[i].Type)).Distinct().Take(2).ToList();
            return types.Count == 1 ? types[0] : null;
        }
    }

    /// <summary>The position of the only selected node on the screen (the rotation applied), or <c>null</c> without exactly one node.</summary>
    public (double X, double Y)? PositionOfSingleNode =>
        SingleNode is { } node && Targets().FirstOrDefault(t => t.Path.Id == node.ShapeId) is { } target
            ? PathNodeEditing.NodePosition(target.Path, node.Index)
            : null;

    /// <summary>Gives the selected nodes the type, and the handles that fit it (see <see cref="PathNodeEditing.SetNodeType"/>).</summary>
    public void SetType(NodeType type) =>
        Apply(HistoryStepType.ChangeNodeType, target => PathNodeEditing.SetNodeType(target.Path, target.Indices, type));

    /// <summary>Removes the handles of the segment between the two selected nodes. Does nothing unless <see cref="CanChangeSegment"/>.</summary>
    public void StraightenSelectedSegment()
    {
        if (SelectedSegment() is { } segment)
        {
            Apply(HistoryStepType.ChangeSegment, target => target.Path.Id == segment.Target.Path.Id ? PathNodeEditing.StraightenSegment(target.Path, segment.Index) : target.Path);
        }
    }

    /// <summary>Gives the straight segment between the two selected nodes handles. Does nothing unless <see cref="CanChangeSegment"/>.</summary>
    public void CurveSelectedSegment()
    {
        if (SelectedSegment() is { } segment)
        {
            Apply(HistoryStepType.ChangeSegment, target => target.Path.Id == segment.Target.Path.Id ? PathNodeEditing.CurveSegment(target.Path, segment.Index) : target.Path);
        }
    }

    /// <summary>
    /// Removes the selected nodes, the neighbors connect. A path with fewer than two nodes left is deleted. The node selection is cleared
    /// afterwards, because the indices no longer fit.
    /// </summary>
    public void DeleteSelected()
    {
        var targets = Targets();
        if (targets.Count == 0)
        {
            return;
        }

        var replacements = new Dictionary<Guid, Shape>();
        var removed = new HashSet<Guid>();
        foreach (var target in targets)
        {
            if (PathNodeEditing.RemoveNodes(target.Path, target.Indices) is { } changed)
            {
                replacements[target.Path.Id] = changed;
            }
            else
            {
                removed.Add(target.Path.Id);
            }
        }

        _state.Edit(HistoryStepType.DeletePathPoints, document => document.UpdateShapes(replacements).RemoveShapes(removed));
        _state.ClearNodeSelection();
    }

    /// <summary>
    /// Moves the selected nodes by the distance on the screen, in document units. Repeated nudges of the same nodes become one history step,
    /// as long as nothing else changed the document in between.
    /// </summary>
    public void Nudge(double dx, double dy) => Move(dx, dy, extendRepeated: true);

    private void Move(double dx, double dy, bool extendRepeated)
    {
        if (dx == 0 && dy == 0)
        {
            return;
        }

        var extend = extendRepeated && ReferenceEquals(_lastNudgeDocument, _state.Document) && _state.SelectedNodes.SetEquals(_lastNudgeNodes);
        var before = _state.Document;
        Apply(HistoryStepType.MovePathPoints, target =>
        {
            var local = target.Path.ToLocalDelta(dx, dy);
            return PathNodeEditing.MoveNodes(target.Path, target.Indices, local.Dx, local.Dy);
        }, extend);
        if (!ReferenceEquals(before, _state.Document))
        {
            // A typed position is a step of its own, so the next nudge does not join it.
            _lastNudgeDocument = extendRepeated ? _state.Document : null;
            _lastNudgeNodes = _state.SelectedNodes;
        }
    }

    /// <summary>Moves the only selected node to the position on the screen. A value that is <c>null</c> keeps that coordinate. Does nothing without exactly one node.</summary>
    public void SetPosition(double? x, double? y)
    {
        if (PositionOfSingleNode is not { } current || (x is null && y is null))
        {
            return;
        }

        var dx = (x ?? current.X) - current.X;
        var dy = (y ?? current.Y) - current.Y;
        Move(dx, dy, extendRepeated: false);
    }

    // The edits of one kind, applied to every path that has selected nodes, as one step. A shape that does not change stays as it is
    // (a rectangle is not converted for nothing).
    private void Apply(HistoryStepType type, Func<NodeTarget, PathShape> change, bool extend = false)
    {
        var replacements = new Dictionary<Guid, Shape>();
        foreach (var target in Targets())
        {
            var result = change(target);
            if (!IsSame(result, target.Path))
            {
                replacements[target.Path.Id] = result;
            }
        }

        if (replacements.Count > 0)
        {
            _state.Edit(type, document => document.UpdateShapes(replacements), extend);
        }
    }

    // Whether two paths look the same: the same box, openness and nodes.
    internal static bool IsSame(PathShape a, PathShape b) =>
        ReferenceEquals(a, b)
        || (a.Bounds == b.Bounds && a.IsClosed == b.IsClosed && a.Nodes.AsSpan().SequenceEqual(b.Nodes.AsSpan()));

    // The editable shapes that have selected nodes: visible and not locked, as a path (a rectangle or ellipse converted), with the
    // indices that exist.
    internal IReadOnlyList<NodeTarget> Targets()
    {
        var targets = new List<NodeTarget>();
        foreach (var group in _state.SelectedNodes.GroupBy(n => n.ShapeId))
        {
            if (!SelectionQueries.IsSelectable(_state.Document, group.Key)
                || _state.Document.FindShape(group.Key)?.Shape is not { } shape
                || EditablePath.For(shape) is not { } path)
            {
                continue;
            }

            var indices = group.Select(n => n.Index).Where(i => i >= 0 && i < path.Nodes.Length).Distinct().OrderBy(i => i).ToList();
            if (indices.Count > 0)
            {
                targets.Add(new NodeTarget(path, indices));
            }
        }

        return targets;
    }

    private (NodeTarget Target, int Index)? SelectedSegment()
    {
        if (Targets() is not [{ Indices: [var first, var second] } target])
        {
            return null;
        }

        var count = target.Path.Nodes.Length;
        if (second == first + 1)
        {
            return (target, first);
        }

        // The closing segment of a closed path runs from the last node to the first one.
        return target.Path.IsClosed && first == 0 && second == count - 1 ? (target, second) : null;
    }

}
