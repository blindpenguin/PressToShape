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
internal sealed class CanvasInteraction
{
    /// <summary>A new shape needs at least this many screen pixels in x and in y.</summary>
    private const double MinimumDragPixels = 2;

    /// <summary>A selection box smaller than this in both directions is a click on empty space.</summary>
    private const double ClickPixels = 3;

    private readonly EditorState _state;

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

    public CanvasInteraction(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    private enum DragType
    {
        None,
        Drawing,
        Moving,
        Marquee,
        Resizing,
        Rotating,
    }

    /// <summary>Whether a drag is in progress.</summary>
    public bool IsDragging => _drag != DragType.None;

    /// <summary>The handle that is being dragged, or <c>null</c> if no resize or rotate drag is in progress.</summary>
    public HandleType? DraggedHandle => _drag is DragType.Resizing or DragType.Rotating ? _handle : null;

    /// <summary>The primary button went down at this document position. <paramref name="shift"/> adds to or removes from the selection.</summary>
    public void PointerPressed(double x, double y, bool shift = false)
    {
        Cancel();

        switch (_state.ActiveTool)
        {
            case Tool.Rectangle:
            case Tool.Ellipse:
                if (CanDrawInActiveLayer())
                {
                    BeginDrag(DragType.Drawing, x, y);
                }

                break;
            case Tool.Select:
                PressWithSelectTool(x, y, shift);
                break;
        }
    }

    /// <summary>The pointer moved to this document position. Does nothing without a drag.</summary>
    public void PointerMoved(double x, double y, bool shift = false, bool alt = false)
    {
        if (!ContinueDrag())
        {
            return;
        }

        switch (_drag)
        {
            case DragType.Drawing:
                _state.Draft = CreateShape(x, y, shift, alt);
                break;
            case DragType.Marquee:
                _state.SelectionBox = BoxTo(x, y);
                break;
            default:
                Show(TransformedShapes(x, y, shift, alt));
                break;
        }
    }

    /// <summary>The button went up at this document position. Ends the drag in every case.</summary>
    public void PointerReleased(double x, double y, bool shift = false, bool alt = false)
    {
        if (!ContinueDrag())
        {
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

        if (_drag == DragType.Marquee)
        {
            var box = BoxTo(x, y);
            EndDrag();
            SelectInBox(box);
            return;
        }

        EndDrag();
        if (CanDrawInActiveLayer() && CreateShape(x, y, shift, alt) is { } shape && IsBigEnough(shape))
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

    /// <summary>Removes all selected shapes from the document as one step. Does nothing without a selection.</summary>
    public void DeleteSelection()
    {
        var shapes = SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds);
        if (shapes.Count == 0)
        {
            return;
        }

        Cancel();
        _state.Edit(HistoryStepType.DeleteShape, document => shapes.Aggregate(document, (d, shape) => d.RemoveShape(shape.Id)));
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

    private SelectionFrame? CurrentFrame() =>
        SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds));

    private void PressWithSelectTool(double x, double y, bool shift)
    {
        var document = _state.Document;
        if (CurrentFrame() is { } frame && SelectionGeometry.HitHandle(frame, x, y, _state.ZoomPercent) is { } handle)
        {
            BeginHandleDrag(frame, handle, x, y);
            return;
        }

        foreach (var hit in document.HitTest(x, y))
        {
            if (!SelectionQueries.IsSelectable(document, hit.Id))
            {
                continue;
            }

            if (shift)
            {
                // Shift only changes the selection: nothing moves.
                _state.ToggleSelected(hit.Id);
                return;
            }

            if (!_state.SelectedShapeIds.Contains(hit.Id))
            {
                _state.SelectOnly(hit.Id);
            }

            _startShapes = SelectionQueries.SelectedShapes(document, _state.SelectedShapeIds);
            _startDocument = document;
            BeginDrag(DragType.Moving, x, y);
            return;
        }

        // Empty space: a box starts. If it stays tiny it counts as a click that clears the selection.
        _startShift = shift;
        BeginDrag(DragType.Marquee, x, y);
    }

    private void BeginHandleDrag(SelectionFrame frame, HandleType handle, double x, double y)
    {
        _startShapes = SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds);
        _startDocument = _state.Document;
        _startFrame = frame;
        _handle = handle;

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
            if (!_startShift)
            {
                _state.ClearSelection();
            }

            return;
        }

        var touched = SelectionQueries.ShapesTouching(_state.Document, box.X, box.Y, box.Width, box.Height).Select(s => s.Id);
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
            _ => (HistoryStepType?)null,
        };
        _drag = DragType.None;
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
                return SelectionTransforms.Resize(_startShapes, frame, _handle, x + _grabOffsetX, y + _grabOffsetY, shift, alt);
            case DragType.Rotating when _startFrame is { } frame:
                return SelectionTransforms.Rotate(_startShapes, frame, _startX, _startY, x, y, shift);
            default:
                return SelectionTransforms.Move(_startShapes, x - _startX, y - _startY);
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

        var document = original;
        for (var i = 0; i < shapes.Count; i++)
        {
            if (!Equals(shapes[i], _startShapes[i]))
            {
                var shape = shapes[i];
                document = document.UpdateShape(shape.Id, _ => shape);
            }
        }

        if (!ReferenceEquals(document, _state.Document))
        {
            _state.Preview(document);
        }
    }

    // Shift makes the shape square (a circle for the ellipse), Alt draws it around the press point as its center.
    private Shape? CreateShape(double x, double y, bool shift, bool alt)
    {
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

        return _dragTool switch
        {
            Tool.Rectangle => new RectangleShape { X = left, Y = top, Width = width, Height = height },
            Tool.Ellipse => new EllipseShape { X = left, Y = top, Width = width, Height = height },
            _ => null,
        };
    }

    private bool IsBigEnough(Shape shape)
    {
        var scale = _state.ZoomPercent / 100;
        return shape.Width * scale >= MinimumDragPixels && shape.Height * scale >= MinimumDragPixels;
    }
}
