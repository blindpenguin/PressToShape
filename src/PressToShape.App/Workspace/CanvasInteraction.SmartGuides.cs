using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;

namespace PressToShape.App.Workspace;

// The smart guides of a drag: distance labels, equal spacing and equal size. What they compare with (the boxes, gaps and sizes of the other
// objects) is collected once at the press, only when Smart Guides is on, and dropped with the snap session. The markers they produce go to
// EditorState.SmartMarkers and are cleared when the drag ends, is aborted or is cancelled. Holding Ctrl turns them off for that event.
internal sealed partial class CanvasInteraction
{
    // The other objects for the drag in progress. Null when Smart Guides is off or no drag has a session.
    private SmartTargets? _smartTargets;

    /// <summary>Whether smart guides apply to the pointer event that is handled now: the switch was on at the press and Ctrl is not held.</summary>
    private bool SmartActive => !_suspendSnap && _smartTargets is not null;

    // How far a smart snap reaches, in document units. The same 8 screen pixels as every other snap.
    private double SmartTolerance => _state.ZoomPercent > 0 && double.IsFinite(_state.ZoomPercent) ? Snapper.TolerancePixels / (_state.ZoomPercent / 100) : 0;

    // Starts the smart session of a drag. The excluded shapes and groups are the ones that move with the drag.
    private void BeginSmartSession(IReadOnlySet<Guid> excluded)
    {
        _smartTargets = _state.Snapping.SmartGuides ? SmartTargets.Collect(_state.Document, excluded) : null;
    }

    // Ends the smart session and removes its markers.
    private void ClearSmart()
    {
        _smartTargets = null;
        _pathPoints = null;
        _pathPointsDocument = null;
        if (!_state.SmartMarkers.IsEmpty)
        {
            _state.SmartMarkers = ImmutableArray<SmartMarker>.Empty;
        }
    }

    // Shows the markers. Nothing changes when they are the same as before, so the canvas does not redraw for nothing.
    private void ShowSmart(IReadOnlyList<SmartMarker> markers)
    {
        if (!_state.SmartMarkers.AsSpan().SequenceEqual(markers.ToArray()))
        {
            _state.SmartMarkers = [.. markers];
        }
    }

    // The distance labels of a box, or none when smart guides are off for this event.
    private IReadOnlyList<SmartMarker> LabelsFor(Bounds box) => SmartActive ? DistanceLabels.Build(box, _smartTargets!) : [];

    // The distance labels of the box that a resize gives. A turned frame has no labels.
    private IReadOnlyList<SmartMarker> LabelsForResize(SelectionFrame frame, IReadOnlyList<PressToShape.Core.Shapes.Shape> resized) =>
        Math.Abs(frame.Rotation % 360) <= 1e-9 && SelectionBounds.Union(resized) is { } box ? LabelsFor(box) : [];

    // The smart snaps of a resize: equal spacing of the gap on the side of the dragged edge (not when both edges move) and equal size.
    private List<SnapCandidate>? ResizeExtras(Bounds box, int directionX, int directionY, double x, double y, bool fromCenter)
    {
        if (!SmartActive)
        {
            return null;
        }

        var pointerBox = ResizedBox(box, directionX, directionY, x, y, fromCenter);
        var extras = new List<SnapCandidate>();
        if (!fromCenter)
        {
            extras.AddRange(SmartSnap.EqualSpacing(pointerBox, _smartTargets!, SmartTolerance, SidesOf(directionX), SidesOf(directionY)));
        }

        extras.AddRange(SmartSnap.EqualSize(pointerBox, directionX, directionY, fromCenter, _smartTargets!, SmartTolerance));
        return extras;
    }

    // The markers of the smart snaps that won while a shape is drawn. The labels are added from the shape.
    private IReadOnlyList<SmartMarker> _drawMarkers = [];

    // The corner of a shape that is drawn, after snapping. Equal size snaps the width and the height to those of other objects. With Shift the
    // shape is a square, so only the larger side is snapped. With Alt the shape grows from the press point on both sides.
    private (double X, double Y) SnappedDrawPoint(double x, double y, bool shift, bool alt)
    {
        List<SnapCandidate>? extras = null;
        if (SmartActive)
        {
            var dx = x - _startX;
            var dy = y - _startY;
            var width = Math.Abs(dx);
            var height = Math.Abs(dy);
            var directionX = dx >= 0 ? 1 : -1;
            var directionY = dy >= 0 ? 1 : -1;
            if (shift)
            {
                var side = Math.Max(width, height);
                (width, height) = (side, side);
                if (width > Math.Abs(dx))
                {
                    directionX = 0;
                }
                else
                {
                    directionY = 0;
                }
            }

            var box = alt
                ? new Bounds(_startX - width, _startY - height, width * 2, height * 2)
                : new Bounds(dx >= 0 ? _startX : _startX - width, dy >= 0 ? _startY : _startY - height, width, height);
            extras = [.. SmartSnap.EqualSize(box, directionX, directionY, alt, _smartTargets!, SmartTolerance)];
        }

        var snap = Snap([x], [y], null, extras);
        _drawMarkers = snap.Markers;
        return (x + snap.OffsetX, y + snap.OffsetY);
    }

    // The labels of the shape that is drawn.
    private IReadOnlyList<SmartMarker> LabelsForDraft(PressToShape.Core.Shapes.Shape? draft) =>
        draft is null ? [] : LabelsFor(draft.RotatedBounds);

    // The segment middles and crossings near the pointer, for the Pen and the node tools. Built when the first point is asked for and again
    // when the document changed or the pointer is more than 100 screen pixels from the center of the last build.
    private const double PathPointRadiusPixels = 200;
    private const double PathPointRebuildPixels = 100;

    private PathPointTargets? _pathPoints;
    private Document? _pathPointsDocument;

    // The source is the document the targets are built from. A drag that previews its edit asks with the document from the press, so the
    // previews do not make the targets be built again at every move.
    private PathPointTargets PathPointsAt(IReadOnlySet<Guid> excluded, double x, double y, Document? source = null)
    {
        var document = source ?? _state.Document;
        var scale = _state.ZoomPercent / 100;
        if (_pathPoints is not { } points
            || !ReferenceEquals(_pathPointsDocument, document)
            || Math.Sqrt(Math.Pow(points.CenterX - x, 2) + Math.Pow(points.CenterY - y, 2)) > PathPointRebuildPixels / scale)
        {
            _pathPoints = points = PathPointTargets.Build(document, excluded, x, y, PathPointRadiusPixels / scale);
            _pathPointsDocument = document;
        }

        return points;
    }

    // A point after the axis snaps, moved onto a segment middle or a crossing when one is near. A point target that is nearer than the axis
    // snaps wins, a node or an edge at the same distance wins over it. Shows the marker of the target, or none.
    private (double X, double Y) WithPathPoint(IReadOnlySet<Guid> excluded, double x, double y, (double X, double Y) snapped, Document? source = null)
    {
        if (_suspendSnap || !_state.Snapping.SmartGuides || !double.IsFinite(_state.ZoomPercent) || _state.ZoomPercent <= 0)
        {
            ShowSmart([]);
            return snapped;
        }

        var target = PathPointsAt(excluded, x, y, source).Nearest(x, y, SmartTolerance);
        var axisDistance = Math.Sqrt(Math.Pow(snapped.X - x, 2) + Math.Pow(snapped.Y - y, 2));
        var snappedByAxis = snapped.X != x || snapped.Y != y;
        if (target is { } point && (!snappedByAxis || Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2)) < axisDistance))
        {
            ShowSmart([new SmartMarker(SmartMarkerType.PathPoint, SnapAxis.X, point.X, point.X, point.Y, double.NaN)]);
            ShowNoSnap();
            return (point.X, point.Y);
        }

        ShowSmart([]);
        return snapped;
    }
}
