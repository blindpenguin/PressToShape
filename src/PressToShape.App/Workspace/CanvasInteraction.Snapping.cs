using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

// Snapping during a drag. The targets of the other objects are collected once at the press (the objects that are not dragged do not change
// during the drag), every pointer move then only searches them. Guides are published through the state and cleared when the drag ends,
// is aborted or is cancelled. Holding Ctrl (suspendSnap) turns all snapping off for that event.
internal sealed partial class CanvasInteraction
{
    // The targets of the other objects for the drag in progress. Null when Snap to Objects is off or no drag has a session.
    private SnapTargets? _snapTargets;

    // Starts the session for a drag of these shapes: collects the targets of everything else. The dragged shapes and every group that
    // holds one of them give no target, because they move with the drag. The other members of such a group stay targets.
    private void BeginSnapSession(IReadOnlyList<Shape> draggedLeaves)
    {
        ClearSnap();
        var excluded = ExcludedIds(_state.Document, draggedLeaves.Select(shape => shape.Id).ToHashSet());
        BeginSmartSession(excluded);
        if (_state.Snapping.SnapToObjects)
        {
            _snapTargets = SnapTargets.Collect(_state.Document, excluded, _state.Snapping.ShowGuides);
        }
    }

    // The position on the screen of the node that the drag grabbed, at the press. Null when a handle or a segment is dragged.
    private (double X, double Y)? _nodeGrabStart;

    // The paths that the node or handle drag edits. Their segments give no path point targets.
    private IReadOnlySet<Guid> _nodeEditedIds = new HashSet<Guid>();

    // Starts the session for a node or handle drag. The edited paths give no box of their own and no node that moves, but their other
    // nodes are targets. For a handle drag the node that owns the handle is no target either, because the handle is anchored there.
    private void BeginNodeSnapSession(DragType drag)
    {
        ClearSnap();
        _nodeGrabStart = null;
        var edited = new HashSet<Guid>();
        _nodeEditedIds = edited;
        var others = new List<(double X, double Y)>();
        if (drag == DragType.MovingNodes)
        {
            foreach (var target in _nodeTargets)
            {
                edited.Add(target.Path.Id);
                for (var i = 0; i < target.Path.NodeCount; i++)
                {
                    if (!target.Indices.Contains(i))
                    {
                        others.Add(PathNodeEditing.NodePosition(target.Path, i));
                    }
                }

                if (target.Path.Id == _nodeHit.ShapeId && target.Indices.Contains(_nodeHit.Index))
                {
                    _nodeGrabStart = PathNodeEditing.NodePosition(target.Path, _nodeHit.Index);
                }
            }
        }
        else if (drag == DragType.MovingHandle && _nodePath is { } path)
        {
            edited.Add(path.Id);
            for (var i = 0; i < path.NodeCount; i++)
            {
                if (i != _nodeHit.Index)
                {
                    others.Add(PathNodeEditing.NodePosition(path, i));
                }
            }
        }

        if (_state.Snapping.SnapToObjects && edited.Count > 0)
        {
            _snapTargets = SnapTargets.Collect(_state.Document, edited, _state.Snapping.ShowGuides).WithNodes(others);
        }
    }

    // Ends the session and removes the guide lines and the smart guide markers.
    private void ClearSnap()
    {
        _snapTargets = null;
        ClearSmart();
        ShowNoSnap();
    }

    // Snaps the candidates of the dragged thing and shows the guide lines. The extras are the snaps of the smart guides, they need no other snap
    // to be on. Without an active snap nothing changes and no guide stays.
    private SnapResult Snap(IReadOnlyList<double> xCandidates, IReadOnlyList<double> yCandidates, Bounds? dragged, IReadOnlyList<SnapCandidate>? extras = null)
    {
        var settings = SnapSettings.From(_state.Snapping);
        var active = !_suspendSnap && (settings.SnapToGrid || settings.SnapToObjects || extras is { Count: > 0 });
        var result = active
            ? Snapper.Snap(xCandidates, yCandidates, _snapTargets, settings, _state.ZoomPercent, dragged, extras)
            : SnapResult.None;
        ShowGuides(result.Guides);
        ShowActiveGuides(result.ActiveGuideIds);
        return result;
    }

    // No snap: no guide line and no active ruler guide.
    private void ShowNoSnap()
    {
        ShowGuides([]);
        ShowActiveGuides([]);
    }

    // The ruler guides that the drag is snapped to, drawn as active.
    private void ShowActiveGuides(IReadOnlyList<Guid> ids)
    {
        if (!_state.ActiveGuideIds.AsSpan().SequenceEqual(ids.ToArray()))
        {
            _state.ActiveGuideIds = [.. ids];
        }
    }

    private void ShowGuides(IReadOnlyList<SnapGuide> guides)
    {
        if (!_state.SnapGuides.AsSpan().SequenceEqual(guides.ToArray()))
        {
            _state.SnapGuides = [.. guides];
        }
    }

    // The shapes and the groups around them that move with the drag. Their own boxes are no targets.
    private static HashSet<Guid> ExcludedIds(Document document, IReadOnlySet<Guid> draggedLeafIds)
    {
        var excluded = new HashSet<Guid>(draggedLeafIds);
        foreach (var layer in document.Layers)
        {
            foreach (var shape in layer.Shapes)
            {
                MarkGroupsAround(shape, draggedLeafIds, excluded);
            }
        }

        return excluded;
    }

    // Adds the group when one of its leaves is dragged. Returns whether the shape holds a dragged leaf.
    private static bool MarkGroupsAround(Shape shape, IReadOnlySet<Guid> draggedLeafIds, HashSet<Guid> excluded)
    {
        if (shape is not GroupShape group)
        {
            return draggedLeafIds.Contains(shape.Id);
        }

        var holdsDragged = false;
        foreach (var child in group.Children)
        {
            holdsDragged |= MarkGroupsAround(child, draggedLeafIds, excluded);
        }

        if (holdsDragged)
        {
            excluded.Add(group.Id);
        }

        return holdsDragged;
    }

    // The distance the selection moves for a pointer delta, corrected so the box of the dragged shapes lines up with a target.
    private (double Dx, double Dy) SnappedMove(double dx, double dy)
    {
        if (SelectionBounds.Union(_startShapes) is not { } start)
        {
            return (dx, dy);
        }

        var moved = new Bounds(start.X + dx, start.Y + dy, start.Width, start.Height);
        var extras = SmartActive ? SmartSnap.EqualSpacing(moved, _smartTargets!, SmartTolerance) : null;
        var snap = Snap(Snapper.CandidatesX(moved), Snapper.CandidatesY(moved), moved, extras);
        var snapped = new Bounds(moved.X + snap.OffsetX, moved.Y + snap.OffsetY, moved.Width, moved.Height);
        ShowSmart([.. snap.Markers, .. LabelsFor(snapped)]);
        return (dx + snap.OffsetX, dy + snap.OffsetY);
    }

    // The targets of a Pen drawing: everything except the path that is continued, and the nodes of the path so far. While the button is
    // down the node just added is left out, because the end of its handle is what moves. Rebuilt when the document or the nodes change.
    private Document? _penTargetsDocument;
    private int _penTargetsNodes = -1;
    private bool _penTargetsPressing;

    private SnapTargets? PenTargets()
    {
        if (!_state.Snapping.SnapToObjects)
        {
            return null;
        }

        var pressing = _pen.IsPressing;
        if (_snapTargets is null || !ReferenceEquals(_penTargetsDocument, _state.Document) || _penTargetsNodes != _pen.Nodes.Count || _penTargetsPressing != pressing)
        {
            var excluded = _continued is { } continued ? new HashSet<Guid> { continued.Id } : [];
            var count = pressing ? _pen.Nodes.Count - 1 : _pen.Nodes.Count;
            _snapTargets = SnapTargets.Collect(_state.Document, excluded, _state.Snapping.ShowGuides).WithNodes(_pen.Nodes.Take(count).Select(node => (node.X, node.Y)));
            _penTargetsDocument = _state.Document;
            _penTargetsNodes = _pen.Nodes.Count;
            _penTargetsPressing = pressing;
        }

        return _snapTargets;
    }

    // A Pen position after snapping. A position that finishes or closes the path (within the distances of the last and the first node)
    // is not snapped: that action wins, and snapping must not move the pointer away from the node it aims at.
    private (double X, double Y) PenSnapped(double x, double y, double lastNodeDistance, double closeDistance)
    {
        var nodes = _pen.Nodes;
        if ((nodes.Count > 0 && Distance(nodes[^1], x, y) <= lastNodeDistance) || (nodes.Count >= 3 && Distance(nodes[0], x, y) <= closeDistance))
        {
            ShowNoSnap();
            ShowSmart([]);
            return (x, y);
        }

        _snapTargets = PenTargets();
        var excluded = _continued is { } continued ? new HashSet<Guid> { continued.Id } : [];
        return WithPathPoint(excluded, x, y, SnappedPoint(x, y));
    }

    private static double Distance(DocumentNode node, double x, double y) => Math.Sqrt(Math.Pow(node.X - x, 2) + Math.Pow(node.Y - y, 2));

    // The distance the dragged nodes move for a pointer delta, corrected so the node that was grabbed lines up with a target. The other
    // nodes move by the same distance.
    private (double Dx, double Dy) SnappedNodeMove(double dx, double dy)
    {
        if (_nodeGrabStart is not { } start)
        {
            return (dx, dy);
        }

        var x = start.X + dx;
        var y = start.Y + dy;
        var snap = Snap([x], [y], null);
        var (snappedX, snappedY) = WithPathPoint(_nodeEditedIds, x, y, (x + snap.OffsetX, y + snap.OffsetY), _startDocument);
        return (snappedX - start.X, snappedY - start.Y);
    }

    // A single point (the corner of a new shape) after snapping. The point has no extent, so the guides only show the target.
    private (double X, double Y) SnappedPoint(double x, double y)
    {
        var snap = Snap([x], [y], null);
        return (x + snap.OffsetX, y + snap.OffsetY);
    }

    // The markers of the smart snaps that won in the resize in progress. The labels are added from the shapes that the resize gives.
    private IReadOnlyList<SmartMarker> _resizeMarkers = [];

    // The position of a resize handle after snapping. Only the edges that the handle moves are candidates: a corner gives x and y, a
    // side gives one of them. A frame that is turned has slanted edges and does not snap. With the ratio kept only the axis that is
    // nearer to a target snaps, because the other side follows the ratio. The equal spacing of the smart guides applies to the gap
    // on the side of the edge that moves, unless both edges move (Alt).
    private (double X, double Y) SnappedHandle(SelectionFrame frame, double x, double y, bool keepsRatio, bool fromCenter)
    {
        _resizeMarkers = [];
        if (Math.Abs(frame.Rotation % 360) > 1e-9)
        {
            ShowNoSnap();
            return (x, y);
        }

        var (directionX, directionY) = SelectionGeometry.Direction(_handle);
        var box = new Bounds(frame.CenterX - frame.Width / 2, frame.CenterY - frame.Height / 2, frame.Width, frame.Height);
        IReadOnlyList<double> xs = directionX == 0 ? [] : [x];
        IReadOnlyList<double> ys = directionY == 0 ? [] : [y];
        var extras = ResizeExtras(box, directionX, directionY, x, y, fromCenter);
        var snap = Snap(xs, ys, box, extras);
        if (keepsRatio && snap.OffsetX != 0 && snap.OffsetY != 0)
        {
            snap = Math.Abs(snap.OffsetX) <= Math.Abs(snap.OffsetY)
                ? Snap(xs, [], box, OnAxis(extras, SnapAxis.X))
                : Snap([], ys, box, OnAxis(extras, SnapAxis.Y));
        }

        _resizeMarkers = snap.Markers;
        return (x + snap.OffsetX, y + snap.OffsetY);
    }

    // The box that the pointer gives: the edges that the handle drags are at the pointer, the others stay. With fromCenter both edges move.
    private static Bounds ResizedBox(Bounds box, int directionX, int directionY, double x, double y, bool fromCenter)
    {
        double left = box.X, right = box.X + box.Width, top = box.Y, bottom = box.Y + box.Height;
        var centerX = box.X + box.Width / 2;
        var centerY = box.Y + box.Height / 2;
        if (directionX != 0)
        {
            (left, right) = fromCenter ? (centerX - Math.Abs(x - centerX), centerX + Math.Abs(x - centerX)) : directionX > 0 ? (left, x) : (x, right);
        }

        if (directionY != 0)
        {
            (top, bottom) = fromCenter ? (centerY - Math.Abs(y - centerY), centerY + Math.Abs(y - centerY)) : directionY > 0 ? (top, y) : (y, bottom);
        }

        return new Bounds(Math.Min(left, right), Math.Min(top, bottom), Math.Abs(right - left), Math.Abs(bottom - top));
    }

    // Which gap changes when the edge in the direction moves: the right and bottom edges change the gap after the box, the left and top edges the one before it.
    private static SpacingSides SidesOf(int direction) => direction switch
    {
        > 0 => SpacingSides.AfterOnly,
        < 0 => SpacingSides.BeforeOnly,
        _ => SpacingSides.None,
    };

    private static IReadOnlyList<SnapCandidate>? OnAxis(IReadOnlyList<SnapCandidate>? candidates, SnapAxis axis) =>
        candidates?.Where(c => c.Axis == axis).ToList();
}
