using System.ComponentModel;
using System.Collections.Generic;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

// What the Pen tool does on the canvas: it draws a path point by point. The gesture logic is in PenDrawing, this part
// converts the distances from screen pixels, keeps the draft and the overlay current and puts the finished path in the document.
internal sealed partial class CanvasInteraction
{
    // A press this close to the last node (a double click or a double tap) finishes the path, in screen pixels.
    private const double PenSamePointPixels = 4;
    private const double PenSamePointTouchPixels = 12;

    // A drag shorter than this leaves a corner point, in screen pixels.
    private const double PenDragPixels = 3;

    private readonly PenDrawing _pen = new();

    // The path that is being continued. Its nodes are in the pen, in screen positions, and it is replaced when the drawing ends.
    private PathShape? _continued;
    private int _continuedSubPath;

    /// <summary>Whether the Pen has an unfinished path.</summary>
    public bool IsPenDrawing => _pen.IsDrawing;

    /// <summary>
    /// Finishes the path of the Pen as an open path (the Enter key). A path with fewer than two nodes is dropped.
    /// Returns whether there was a path to finish.
    /// </summary>
    public bool FinishPen()
    {
        if (!_pen.IsDrawing)
        {
            return false;
        }

        FinishPath(close: false);
        return true;
    }

    /// <summary>Drops the unfinished path of the Pen (the Escape key). Returns whether there was one.</summary>
    public bool DiscardPen()
    {
        if (!_pen.IsDrawing)
        {
            return false;
        }

        _pen.Discard();
        ClearPenView();
        return true;
    }

    // A second finger arrived: the press that added the last node was a mistake. The nodes before it stay.
    private void AbortPenPress()
    {
        if (!_pen.IsPressing)
        {
            return;
        }

        _pen.AbortPress();
        _state.IsDragInProgress = false;
        ShowPen();
    }

    private void PenPressed(double x, double y, InputPointerType inputType)
    {
        if (!CanDrawInActiveLayer())
        {
            DiscardPen();
            return;
        }

        if (!_pen.IsDrawing && TryEditSelectedPath(x, y, inputType))
        {
            return;
        }

        var scale = _state.ZoomPercent / 100;
        var grab = inputType == InputPointerType.Touch ? TouchGrabSize : SelectionGeometry.GrabSize;
        var samePoint = (inputType == InputPointerType.Touch ? PenSamePointTouchPixels : PenSamePointPixels) / scale;
        var closeDistance = grab / 2 / scale;
        var (nodeX, nodeY) = PenSnapped(x, y, samePoint, closeDistance);
        var result = _pen.Press(x, y, samePoint, closeDistance, nodeX, nodeY);
        switch (result)
        {
            case PenPressResult.ClosedPath:
                FinishPath(close: true);
                break;
            case PenPressResult.FinishedPath:
                FinishPath(close: false);
                break;
            default:
                _state.IsDragInProgress = true;
                ShowPen();
                break;
        }
    }

    private void PenMoved(double x, double y)
    {
        if (!_pen.IsDrawing)
        {
            return;
        }

        var scale = _state.ZoomPercent / 100;
        var closeDistance = SelectionGeometry.GrabSize / 2 / scale;
        if (_pen.IsPressing)
        {
            // The end of the handle snaps. The node that was just added is no target for it.
            var (handleX, handleY) = PenSnapped(x, y, 0, 0);
            _pen.Drag(handleX, handleY, PenDragPixels / scale);
        }
        else
        {
            var (hoverX, hoverY) = PenSnapped(x, y, PenSamePointPixels / scale, closeDistance);
            _pen.Hover(hoverX, hoverY, closeDistance);
        }

        ShowPen();
    }

    private void PenReleased()
    {
        if (!_pen.IsPressing)
        {
            return;
        }

        _pen.Release();
        _state.IsDragInProgress = false;
        ShowPen();
    }

    // Choosing another tool ends the path where it is, if it has two nodes.
    private void OnStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorState.ActiveTool) && _state.ActiveTool != Tool.Pen && _pen.IsDrawing)
        {
            FinishPath(close: false);
        }
    }

    private void FinishPath(bool close)
    {
        var continued = _continued;
        var continuedSubPath = _continuedSubPath;
        var path = _pen.Finish(close);
        ClearPenView();
        if (path is null || (continued is null && !CanDrawInActiveLayer()))
        {
            return;
        }

        if (continued is not null)
        {
            ReplaceContinuedPath(continued, path, continuedSubPath, close);
            return;
        }

        var shape = path with { Appearance = _state.DefaultAppearance };
        _state.Edit(HistoryStepType.DrawPath, document => document.AddShape(_state.ActiveLayerId, shape));
        _state.Select(shape.Id);
    }

    // The pen drew one sub-path (the continued one, with its new nodes). The new path keeps the id, the look and the flags of the old
    // one, and its other sub-paths, with the rotation of the old path baked into every node because the rotation is part of the node
    // positions now. Finishing straight away, without a new node and without closing, changes nothing and makes no step.
    private void ReplaceContinuedPath(PathShape original, PathShape drawn, int subPathIndex, bool close)
    {
        var drawnSubPath = drawn.ToDocumentSubPaths()[0];
        if (!close && drawnSubPath.Nodes.Count == original.SubPaths[subPathIndex].Nodes.Length)
        {
            return;
        }

        var subPaths = new List<DocumentSubPath>(EditablePath.AsOnScreen(original).ToDocumentSubPaths())
        {
            [subPathIndex] = drawnSubPath,
        };
        var shape = PathShape.Create(subPaths, original.FillRule) with
        {
            Id = original.Id,
            Appearance = original.Appearance,
            Name = original.Name,
            IsVisible = original.IsVisible,
            IsLocked = original.IsLocked,
        };
        _state.Edit(HistoryStepType.DrawPath, document => document.UpdateShapes(new Dictionary<System.Guid, Shape> { [shape.Id] = shape }));
        _state.Select(shape.Id);
    }

    private void ShowPen()
    {
        var appearance = _continued?.Appearance ?? _state.DefaultAppearance;
        _state.Draft = _pen.PreviewShape is { } preview ? preview with { Appearance = appearance } : null;
        _state.PenOverlay = _pen.Overlay;
        _state.PenContinuedId = _continued?.Id;
    }

    private void ClearPenView()
    {
        ClearSnap();
        _continued = null;
        _state.Draft = null;
        _state.PenOverlay = null;
        _state.PenContinuedId = null;
        _state.IsDragInProgress = false;
    }
}
