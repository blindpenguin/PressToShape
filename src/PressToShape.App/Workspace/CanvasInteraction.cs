using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// What the mouse does on the canvas: draw rectangles and ellipses, select, move and delete shapes.
/// Works on document positions and edits the document through <see cref="EditorState"/>. It has no UI types.
/// A drag that is in progress ends with <see cref="PointerReleased"/> or <see cref="Cancel"/>.
/// </summary>
internal sealed partial class CanvasInteraction
{
    /// <summary>A new shape needs at least this many screen pixels in x and in y.</summary>
    private const double MinimumDragPixels = 2;

    /// <summary>A selection box smaller than this in both directions is a click on empty space.</summary>
    private const double ClickPixels = 3;

    private readonly EditorState _state;
    private readonly ITextMeasurer? _textMeasurer;

    private DragType _drag = DragType.None;
    private Tool _dragTool;
    private double _startX;
    private double _startY;
    private bool _startShift;
    private IReadOnlyList<Shape> _startShapes = [];
    private Document? _startDocument;
    private SelectionFrame? _startFrame;
    private HandleType _handle;
    private double _grabOffsetX;
    private double _grabOffsetY;

    // Whether snapping is suspended for the pointer event that is handled right now (the Ctrl key). Set by every pointer method.
    private bool _suspendSnap;

    /// <summary>Whether the last pointer event suspended snapping (the Ctrl key was held).</summary>
    internal bool IsSnapSuspended => _suspendSnap;

    /// <param name="state">The editor state.</param>
    /// <param name="textMeasurer">Measures a text when its editing ends, so a point text gets its box. Without one the box is left as it is.</param>
    public CanvasInteraction(EditorState state, ITextMeasurer? textMeasurer = null)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _textMeasurer = textMeasurer;
        _state.PropertyChanged += OnStatePropertyChanged;
    }

    private enum DragType
    {
        None,
        Drawing,
        Moving,
        Marquee,
        Resizing,
        Rotating,
        MovingNodes,
        MovingHandle,
        Bending,
        RoundingCorners,
        MovingGuide,
        CreatingGuide,
        CreatingText,
    }

    /// <summary>Whether a drag is in progress.</summary>
    public bool IsDragging => _drag != DragType.None;

    /// <summary>The handle that is being dragged, or <c>null</c> if no resize or rotate drag is in progress.</summary>
    public HandleType? DraggedHandle => _drag is DragType.Resizing or DragType.Rotating ? _handle : null;

    /// <summary>The size of the area that grabs a selection handle for a finger, in screen pixels. A press by mouse or pen uses <see cref="SelectionGeometry.GrabSize"/>.</summary>
    public double TouchGrabSize { get; set; } = SelectionGeometry.TouchGrabSize;

    /// <summary>
    /// The primary button went down at this document position. <paramref name="shift"/> adds to or removes from the selection.
    /// <paramref name="suspendSnap"/> (the Ctrl key) turns snapping off for this event.
    /// <paramref name="inputType"/> is the device: a finger grabs a handle from a larger area than a mouse or a pen.
    /// </summary>
    public void PointerPressed(double x, double y, bool shift = false, InputPointerType inputType = InputPointerType.Mouse, bool suspendSnap = false)
    {
        _suspendSnap = suspendSnap;
        Cancel();

        // A press ends the editing of a text, whatever it hits. The text tool can then start a new one with the same press.
        CommitTextEdit();

        if (_state.ColorPick != ColorPickTarget.None)
        {
            PickColorAt(x, y);
            return;
        }

        if (_state.ActiveTool == Tool.Eyedropper)
        {
            // The tool is a pick that starts with the click: the fill, or the stroke with Shift.
            _state.BeginColorPick(shift ? ColorPickTarget.Stroke : ColorPickTarget.Fill);
            PickColorAt(x, y);
            return;
        }

        if (TryBeginGuideDrag(x, y, inputType))
        {
            return;
        }

        switch (_state.ActiveTool)
        {
            case Tool.Rectangle:
            case Tool.Ellipse:
                if (CanDrawInActiveLayer())
                {
                    BeginSnapSession([]);
                    var (startX, startY) = SnappedPoint(x, y);
                    BeginDrag(DragType.Drawing, startX, startY);
                }

                break;
            case Tool.Text:
                PressWithTextTool(x, y);
                break;
            case Tool.Pen:
                PenPressed(x, y, inputType);
                break;
            case Tool.DirectSelect:
            case Tool.Node:
                PressWithNodeTool(x, y, shift, inputType);
                break;
            case Tool.Select:
                if (!TryBeginRadiusDrag(x, y, inputType))
                {
                    PressWithSelectTool(x, y, shift, inputType == InputPointerType.Touch ? TouchGrabSize : SelectionGeometry.GrabSize);
                }

                break;
        }
    }

    /// <summary>The pointer moved to this document position. Does nothing without a drag.</summary>
    public void PointerMoved(double x, double y, bool shift = false, bool alt = false, bool suspendSnap = false)
    {
        _suspendSnap = suspendSnap;
        if (_state.ActiveTool == Tool.Pen && !IsGuideDragging)
        {
            PenMoved(x, y);
            return;
        }

        if (!ContinueDrag())
        {
            return;
        }

        switch (_drag)
        {
            case DragType.MovingGuide:
            case DragType.CreatingGuide:
                ShowGuideDrag(x, y);
                break;
            case DragType.Drawing:
                _state.Draft = CreateShape(x, y, shift, alt);
                ShowSmart([.. _drawMarkers, .. LabelsForDraft(_state.Draft)]);
                break;
            case DragType.Marquee:
                _state.SelectionBox = BoxTo(x, y);
                break;
            case DragType.CreatingText:
                _state.SelectionBox = TextBoxTo(x, y);
                break;
            case DragType.RoundingCorners:
                ShowRadius(x, y, shift);
                break;
            case DragType.MovingNodes:
            case DragType.MovingHandle:
            case DragType.Bending:
                _nodeDragMoved |= x != _startX || y != _startY;
                ShowNodeDrag(x, y, shift, alt);
                break;
            default:
                Show(TransformedShapes(x, y, shift, alt));
                break;
        }
    }

    /// <summary>The button went up at this document position. Ends the drag in every case.</summary>
    public void PointerReleased(double x, double y, bool shift = false, bool alt = false, bool suspendSnap = false)
    {
        _suspendSnap = suspendSnap;
        if (IsGuideDragging)
        {
            // A release that reaches here is inside the canvas. The view model ends a drag that left it with EndGuideDrag.
            EndGuideDrag(x, y, overCanvas: true);
            return;
        }

        if (_state.ActiveTool == Tool.Pen)
        {
            PenReleased();
            return;
        }

        NodeToolReleased();
        if (!ContinueDrag())
        {
            return;
        }

        if (_drag is DragType.MovingNodes or DragType.MovingHandle or DragType.Bending)
        {
            ShowNodeDrag(x, y, shift, alt);
            EndDrag();
            return;
        }

        if (_drag == DragType.RoundingCorners)
        {
            ShowRadius(x, y, shift);
            EndDrag();
            return;
        }

        if (_drag == DragType.Moving)
        {
            // The final position counts, then the whole drag is recorded as one step.
            Show(TransformedShapes(x, y, shift, alt));
            EndDrag();
            return;
        }

        if (_drag is DragType.Resizing or DragType.Rotating)
        {
            Show(TransformedShapes(x, y, shift, alt));
            EndDrag();
            return;
        }

        if (_drag == DragType.CreatingText)
        {
            EndTextDrag(x, y);
            return;
        }

        if (_drag == DragType.Marquee)
        {
            var box = BoxTo(x, y);
            var nodeBox = _nodeMarquee;
            EndDrag();
            if (nodeBox)
            {
                SelectNodesInBox(box);
            }
            else
            {
                SelectInBox(box);
            }

            return;
        }

        // The shape is made before the drag ends, because ending it drops the snap targets.
        var created = CreateShape(x, y, shift, alt);
        EndDrag();
        if (CanDrawInActiveLayer() && created is { } shape && IsBigEnough(shape))
        {
            var type = shape is EllipseShape ? HistoryStepType.DrawEllipse : HistoryStepType.DrawRectangle;
            _state.Edit(type, document => document.AddShape(_state.ActiveLayerId, shape));
            _state.Select(shape.Id);
        }
    }

    /// <summary>
    /// Ends a drag without a result: the draft is dropped and a moved shape keeps the position it has,
    /// which then counts as one step if the shape moved.
    /// </summary>
    public void Cancel()
    {
        EndDrag();
    }

    /// <summary>
    /// Ends a drag and undoes it: the draft and the selection box are dropped, and a move, resize or rotate puts the document back to the
    /// one from the press. Nothing is recorded in the history. Used when the drag was a mistake, for example when a second finger arrives.
    /// Does nothing without a drag. <see cref="Cancel"/> differs: it keeps what the drag reached.
    /// </summary>
    public void Abort()
    {
        AbortPenPress();
        if (_drag is DragType.Moving or DragType.Resizing or DragType.Rotating or DragType.MovingNodes or DragType.MovingHandle or DragType.Bending
            or DragType.RoundingCorners or DragType.MovingGuide or DragType.CreatingGuide
            && _startDocument is { } original
            && !ReferenceEquals(original, _state.Document))
        {
            _state.Preview(original);
        }

        EndDrag();
    }

    /// <summary>
    /// Removes all selected shapes from the document as one step. With Direct Select or the Node tool and selected nodes it removes the
    /// nodes instead (see <see cref="NodeEditor.DeleteSelected"/>). Does nothing without a selection.
    /// </summary>
    public void DeleteSelection()
    {
        // A selected ruler guide is deleted when no shape is selected.
        if (_state.SelectedGuideId is not null && !_state.HasSelection)
        {
            DeleteSelectedGuide();
            return;
        }

        // With a node tool and selected nodes, Delete removes the nodes instead of the shape.
        if (IsNodeTool && _state.HasSelectedNodes && Nodes.HasEditableNodes)
        {
            Cancel();
            Nodes.DeleteSelected();
            return;
        }

        var shapes = SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds);
        if (shapes.Count == 0)
        {
            return;
        }

        Cancel();
        var ids = shapes.Select(shape => shape.Id).ToHashSet();
        _state.Edit(HistoryStepType.DeleteShape, document => document.RemoveShapes(ids));
        _state.ClearSelection();
    }

    /// <summary>
    /// The handle under this document position, for the cursor. <c>null</c> during a drag, for other tools than Select, without a
    /// selection, or away from the handles.
    /// </summary>
    public HandleType? HoverHandle(double x, double y)
    {
        if (IsDragging || _state.ActiveTool != Tool.Select || CurrentFrame() is not { } frame)
        {
            return null;
        }

        return SelectionGeometry.HitHandle(frame, x, y, _state.ZoomPercent);
    }

    // The topmost visible shape gives its fill, or its stroke if it has no fill. The shape's own opacity does not count.
    private void PickColorAt(double x, double y)
    {
        var appearance = _state.Document.HitTest(x, y).FirstOrDefault()?.Appearance;
        _state.CompleteColorPick(appearance?.Fill ?? appearance?.Stroke);
    }

    private SelectionFrame? CurrentFrame() =>
        SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds));

    private void PressWithSelectTool(double x, double y, bool shift, double grabSize)
    {
        var document = _state.Document;
        if (CurrentFrame() is { } frame && SelectionGeometry.HitHandle(frame, x, y, _state.ZoomPercent, grabSize) is { } handle)
        {
            BeginHandleDrag(frame, handle, x, y);
            return;
        }

        if (SelectableTargetAt(document, x, y) is { } target)
        {
            if (shift)
            {
                // Shift only changes the selection: nothing moves.
                _state.ToggleSelected(target.Id);
                return;
            }

            if (!_state.SelectedShapeIds.Contains(target.Id))
            {
                _state.SelectOnly(target.Id);
            }

            _startShapes = SelectionQueries.SelectedLeaves(document, _state.SelectedShapeIds);
            _startDocument = document;
            BeginSnapSession(_startShapes);
            BeginDrag(DragType.Moving, x, y);
            return;
        }

        // Empty space: a box starts. If it stays tiny it counts as a click that clears the selection.
        _startShift = shift;
        BeginDrag(DragType.Marquee, x, y);
    }

    private void BeginHandleDrag(SelectionFrame frame, HandleType handle, double x, double y)
    {
        _startShapes = SelectionQueries.SelectedLeaves(_state.Document, _state.SelectedShapeIds);
        _startDocument = _state.Document;
        _startFrame = frame;
        _handle = handle;
        if (handle != HandleType.Rotate)
        {
            BeginSnapSession(_startShapes);
        }

        // The pointer may be a few pixels off the handle's center: keep that distance so the handle does not jump.
        var (hx, hy) = SelectionGeometry.HandlePosition(frame, handle, _state.ZoomPercent);
        _grabOffsetX = hx - x;
        _grabOffsetY = hy - y;
        BeginDrag(handle == HandleType.Rotate ? DragType.Rotating : DragType.Resizing, x, y);
    }

    private SelectionBox BoxTo(double x, double y) =>
        new(Math.Min(_startX, x), Math.Min(_startY, y), Math.Abs(x - _startX), Math.Abs(y - _startY));

    private void SelectInBox(SelectionBox box)
    {
        var scale = _state.ZoomPercent / 100;
        if (box.Width * scale < ClickPixels && box.Height * scale < ClickPixels)
        {
            ClickOnEmptySpace();
            return;
        }

        var touched = SelectionQueries.ShapesTouching(_state.Document, box.X, box.Y, box.Width, box.Height, _state.EnteredGroupId).Select(s => s.Id);
        _state.SetSelection(_startShift ? _state.SelectedShapeIds.Concat(touched) : touched);
    }

    private bool CanDrawInActiveLayer() =>
        _state.Document.Layers.FirstOrDefault(l => l.Id == _state.ActiveLayerId) is { IsVisible: true, IsLocked: false };

    private void BeginDrag(DragType drag, double x, double y)
    {
        _drag = drag;
        _dragTool = _state.ActiveTool;
        _startX = x;
        _startY = y;
        _state.IsDragInProgress = true;
    }

    // A drag only goes on while the tool is the one it started with.
    private bool ContinueDrag()
    {
        if (_drag == DragType.None)
        {
            return false;
        }

        if (_state.ActiveTool != _dragTool)
        {
            EndDrag();
            return false;
        }

        return true;
    }

    private void EndDrag()
    {
        var stepType = _drag switch
        {
            DragType.Moving => HistoryStepType.MoveShape,
            DragType.Resizing => HistoryStepType.ResizeShapes,
            DragType.Rotating => HistoryStepType.RotateShapes,
            DragType.MovingNodes => HistoryStepType.MovePathPoints,
            DragType.MovingHandle => HistoryStepType.MovePathHandle,
            DragType.Bending => HistoryStepType.BendSegment,
            DragType.RoundingCorners => HistoryStepType.ChangeCornerRadius,
            DragType.MovingGuide => _guideDeleted ? HistoryStepType.DeleteGuide : HistoryStepType.MoveGuide,
            DragType.CreatingGuide => HistoryStepType.CreateGuide,
            _ => (HistoryStepType?)null,
        };
        _drag = DragType.None;
        ClearSnap();
        _nodeMarquee = false;
        _state.Draft = null;
        _state.SelectionBox = null;
        _startShapes = [];
        _startDocument = null;
        _startFrame = null;
        _state.IsDragInProgress = false;

        if (stepType is { } type)
        {
            // The pointer moves only previewed the document. If the shapes changed, the whole drag is one step.
            _state.Commit(type);
        }
    }

    // Always computed from the shapes at the press, so many small moves never add up to a drift.
    private IReadOnlyList<Shape> TransformedShapes(double x, double y, bool shift, bool alt)
    {
        switch (_drag)
        {
            case DragType.Resizing when _startFrame is { } frame:
                var keepsRatio = shift || (!frame.IsSingle && _startShapes.Any(s => s.Rotation != 0)) || SelectionTransforms.HasPointText(_startShapes);
                var (handleX, handleY) = SnappedHandle(frame, x + _grabOffsetX, y + _grabOffsetY, keepsRatio, alt);
                var resized = FitPointTexts(SelectionTransforms.Resize(_startShapes, frame, _handle, handleX, handleY, shift, alt));
                ShowSmart([.. _resizeMarkers, .. LabelsForResize(frame, resized)]);
                return resized;
            case DragType.Rotating when _startFrame is { } frame:
                return SelectionTransforms.Rotate(_startShapes, frame, _startX, _startY, x, y, shift);
            default:
                var (dx, dy) = SnappedMove(x - _startX, y - _startY);
                return SelectionTransforms.Move(_startShapes, dx, dy);
        }
    }

    // Shows the shapes on top of the document from the press. Equal shapes are left alone, and if nothing differs from the
    // start the very same document comes back, so a click or a drag back to the start makes no step.
    private void Show(IReadOnlyList<Shape> shapes)
    {
        if (_startDocument is not { } original)
        {
            return;
        }

        var changed = new Dictionary<Guid, Shape>();
        for (var i = 0; i < shapes.Count; i++)
        {
            if (!Equals(shapes[i], _startShapes[i]))
            {
                changed[shapes[i].Id] = shapes[i];
            }
        }

        var document = original.UpdateShapes(changed);

        if (!ReferenceEquals(document, _state.Document))
        {
            _state.Preview(document);
        }
    }

    // Shift makes the shape square (a circle for the ellipse), Alt draws it around the press point as its center.
    private Shape? CreateShape(double pointerX, double pointerY, bool shift, bool alt)
    {
        var (left, top, width, height) = DragBox(pointerX, pointerY, shift, alt);
        return _dragTool switch
        {
            Tool.Rectangle => new RectangleShape { X = left, Y = top, Width = width, Height = height, Appearance = _state.DefaultAppearance },
            Tool.Ellipse => new EllipseShape { X = left, Y = top, Width = width, Height = height, Appearance = _state.DefaultAppearance },
            _ => null,
        };
    }

    // The box of a drag from the press point to the pointer.
    private (double Left, double Top, double Width, double Height) DragBox(double pointerX, double pointerY, bool shift, bool alt)
    {
        // The corner snaps first, then Shift and Alt shape the result.
        var (x, y) = SnappedDrawPoint(pointerX, pointerY, shift, alt);
        var dx = x - _startX;
        var dy = y - _startY;
        if (shift)
        {
            var side = Math.Max(Math.Abs(dx), Math.Abs(dy));
            dx = (dx < 0 ? -1 : 1) * side;
            dy = (dy < 0 ? -1 : 1) * side;
        }

        var width = alt ? Math.Abs(dx) * 2 : Math.Abs(dx);
        var height = alt ? Math.Abs(dy) * 2 : Math.Abs(dy);
        var left = alt ? _startX - Math.Abs(dx) : Math.Min(_startX, _startX + dx);
        var top = alt ? _startY - Math.Abs(dy) : Math.Min(_startY, _startY + dy);
        return (left, top, width, height);
    }

    private bool IsBigEnough(Shape shape)
    {
        var scale = _state.ZoomPercent / 100;
        return shape.Width * scale >= MinimumDragPixels && shape.Height * scale >= MinimumDragPixels;
    }
}
