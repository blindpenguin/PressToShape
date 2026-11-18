using System;
using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

// The three drags of the node tools: nodes, one handle, and a segment (bend). Like the drags of the Select tool, every move is computed
// from the state at the press, so many small moves never add up to a drift, and the release records one history step. A rectangle or an
// ellipse is converted by the first change of the preview, so the conversion and the edit are one step.
internal sealed partial class CanvasInteraction
{
    // A handle dragged to within this many screen pixels of its anchor is put on the anchor, which removes it.
    private const double HandleSnapPixels = 2;

    private NodeEditor? _nodeEditor;
    private IReadOnlyList<NodeTarget> _nodeTargets = [];
    private PathShape? _nodePath;
    private NodeHit _nodeHit;

    /// <summary>The editor of the selected nodes (type, segments, delete, nudge, position). Shared with the keys of the canvas.</summary>
    internal NodeEditor Nodes => _nodeEditor ??= new NodeEditor(_state);

    /// <summary>Whether a node, handle or bend drag is in progress.</summary>
    public bool IsNodeDragging => _drag is DragType.MovingNodes or DragType.MovingHandle or DragType.Bending;

    // Starts the drag. The paths are taken from the document as it is now, so a preview never builds on a preview.
    private void BeginNodeDrag(DragType drag, double x, double y, NodeHit hit)
    {
        _startDocument = _state.Document;
        _nodeHit = hit;
        _nodeTargets = [];
        _nodePath = null;

        if (drag == DragType.MovingNodes)
        {
            _nodeTargets = Nodes.Targets();
        }
        else if (_state.Document.FindShape(hit.ShapeId)?.Shape is { } shape && EditablePath.For(shape) is { } path)
        {
            _nodePath = path;
            if (drag == DragType.MovingHandle)
            {
                // The pointer may be a few pixels off the handle: keep that distance so the handle does not jump.
                var node = EditablePath.AsOnScreen(path).ToDocumentNodes()[hit.Index];
                var handleX = hit.Side == NodeHandleSide.In ? node.InX : node.OutX;
                var handleY = hit.Side == NodeHandleSide.In ? node.InY : node.OutY;
                _grabOffsetX = (handleX ?? node.X) - x;
                _grabOffsetY = (handleY ?? node.Y) - y;
            }
        }
        else
        {
            return;
        }

        BeginNodeSnapSession(drag);
        BeginDrag(drag, x, y);
    }

    // Shows the edit that the pointer position gives. If nothing differs from the start, the very same document comes back, so a click or
    // a drag back to the start makes no step.
    private void ShowNodeDrag(double x, double y, bool shift, bool alt)
    {
        if (_startDocument is not { } original)
        {
            return;
        }

        var replacements = new Dictionary<Guid, Shape>();
        switch (_drag)
        {
            case DragType.MovingNodes:
                var (dx, dy) = SnappedNodeMove(x - _startX, y - _startY);
                MoveSelectedNodes(dx, dy, shift, replacements);
                break;
            case DragType.MovingHandle when _nodePath is { } handlePath:
                AddIfChanged(replacements, handlePath, MovedHandle(handlePath, x, y, alt));
                break;
            case DragType.Bending when _nodePath is { } bendPath:
                var local = bendPath.ToLocalDelta(x - _startX, y - _startY);
                AddIfChanged(replacements, bendPath, PathNodeEditing.BendSegment(bendPath, _nodeHit.Index, _nodeHit.T, local.Dx, local.Dy));
                break;
        }

        var document = replacements.Count == 0 ? original : original.UpdateShapes(replacements);
        if (!ReferenceEquals(document, _state.Document))
        {
            _state.Preview(document);
        }
    }

    private void MoveSelectedNodes(double dx, double dy, bool shift, Dictionary<Guid, Shape> replacements)
    {
        if (shift)
        {
            // One axis only, the one with the larger distance.
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                dy = 0;
            }
            else
            {
                dx = 0;
            }
        }

        if (dx == 0 && dy == 0)
        {
            return;
        }

        foreach (var target in _nodeTargets)
        {
            var local = target.Path.ToLocalDelta(dx, dy);
            AddIfChanged(replacements, target.Path, PathNodeEditing.MoveNodes(target.Path, target.Indices, local.Dx, local.Dy));
        }
    }

    private PathShape MovedHandle(PathShape path, double x, double y, bool breakLink)
    {
        var scale = _state.ZoomPercent / 100;
        var target = (X: x + _grabOffsetX, Y: y + _grabOffsetY);
        var snap = Snap([target.X], [target.Y], null);
        target = (target.X + snap.OffsetX, target.Y + snap.OffsetY);
        var anchor = PathNodeEditing.NodePosition(path, _nodeHit.Index);
        if (Math.Sqrt(Math.Pow(target.X - anchor.X, 2) + Math.Pow(target.Y - anchor.Y, 2)) * scale <= HandleSnapPixels)
        {
            target = anchor;
        }

        var local = path.ToLocal(target.X, target.Y);
        return PathNodeEditing.MoveHandle(path, _nodeHit.Index, _nodeHit.Side, local.X, local.Y, breakLink);
    }

    private static void AddIfChanged(Dictionary<Guid, Shape> replacements, PathShape start, PathShape result)
    {
        if (!NodeEditor.IsSame(result, start))
        {
            replacements[start.Id] = result;
        }
    }
}
