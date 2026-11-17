using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

// The Pen on the one selected path: a press on an end point continues an open path, a press on another node deletes it and a
// press on a segment adds a node there. The hit tests use the path as it appears on the screen, so a rotated path works too.
internal sealed partial class CanvasInteraction
{
    private bool TryEditSelectedPath(double x, double y, InputPointerType inputType)
    {
        if (_state.SelectedShapeId is not { } id
            || !SelectionQueries.IsSelectable(_state.Document, id)
            || _state.Document.FindShape(id)?.Shape is not PathShape path)
        {
            return false;
        }

        var scale = _state.ZoomPercent / 100;
        var radius = (inputType == InputPointerType.Touch ? TouchGrabSize : SelectionGeometry.GrabSize) / 2 / scale;
        var onScreen = EditablePath.AsOnScreen(path);

        var nodes = onScreen.ToDocumentNodes();
        var nearest = -1;
        var nearestDistance = radius;
        for (var i = 0; i < nodes.Count; i++)
        {
            var distance = Math.Sqrt(Math.Pow(nodes[i].X - x, 2) + Math.Pow(nodes[i].Y - y, 2));
            if (distance <= nearestDistance)
            {
                nearest = i;
                nearestDistance = distance;
            }
        }

        if (nearest >= 0)
        {
            var (subPath, local) = onScreen.Locate(nearest);
            var ends = onScreen.SubPaths[subPath];
            if (!ends.IsClosed && (local == 0 || local == ends.Nodes.Length - 1))
            {
                BeginContinue(path, onScreen, nearest);
            }
            else
            {
                DeletePathNode(path, nearest);
            }

            return true;
        }

        var segments = onScreen.Segments();
        for (var i = 0; i < segments.Count; i++)
        {
            var t = Bezier.NearestParameter(segments[i], x, y);
            var point = Bezier.Point(segments[i], t);
            if (t > 0 && t < 1 && Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2)) <= radius)
            {
                var changed = PathEditing.InsertNode(path, i, t);
                _state.Edit(HistoryStepType.AddPathNode, document => document.UpdateShapes(new Dictionary<Guid, Shape> { [path.Id] = changed }));
                return true;
            }
        }

        return false;
    }

    private void DeletePathNode(PathShape path, int index)
    {
        if (PathEditing.RemoveNode(path, index) is { } changed)
        {
            _state.Edit(HistoryStepType.DeletePathNode, document => document.UpdateShapes(new Dictionary<Guid, Shape> { [path.Id] = changed }));
            return;
        }

        // Fewer than two nodes are no path.
        _state.Edit(HistoryStepType.DeletePathNode, document => document.RemoveShapes(new HashSet<Guid> { path.Id }));
        _state.ClearSelection();
    }

    // The pen works with the positions the user sees, so the rotation of the old path is baked into the nodes it continues from.
    private void BeginContinue(PathShape path, PathShape onScreen, int endIndex)
    {
        _continued = path;
        _continuedSubPath = onScreen.Locate(endIndex).SubPath;
        _pen.Begin(PathEditing.Continue(onScreen, endIndex));
        ShowPen();
    }
}
