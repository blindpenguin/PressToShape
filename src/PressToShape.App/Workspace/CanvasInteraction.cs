using System;
using System.Linq;
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

    private readonly EditorState _state;

    private DragType _drag = DragType.None;
    private Tool _dragTool;
    private double _startX;
    private double _startY;
    private Guid _movedShapeId;
    private double _movedShapeStartX;
    private double _movedShapeStartY;

    public CanvasInteraction(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    private enum DragType
    {
        None,
        Drawing,
        Moving,
    }

    /// <summary>Whether a drag is in progress.</summary>
    public bool IsDragging => _drag != DragType.None;

    /// <summary>The primary button went down at this document position.</summary>
    public void PointerPressed(double x, double y)
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
                PressWithSelectTool(x, y);
                break;
        }
    }

    /// <summary>The pointer moved to this document position. Does nothing without a drag.</summary>
    public void PointerMoved(double x, double y)
    {
        if (!ContinueDrag())
        {
            return;
        }

        if (_drag == DragType.Drawing)
        {
            _state.Draft = CreateShape(x, y);
        }
        else
        {
            MoveShape(x, y);
        }
    }

    /// <summary>The button went up at this document position. Ends the drag in every case.</summary>
    public void PointerReleased(double x, double y)
    {
        if (!ContinueDrag())
        {
            return;
        }

        var drag = _drag;
        EndDrag();

        if (drag == DragType.Moving)
        {
            MoveShape(x, y);
        }
        else if (CanDrawInActiveLayer() && CreateShape(x, y) is { } shape && IsBigEnough(shape))
        {
            _state.Document = _state.Document.AddShape(_state.ActiveLayerId, shape);
            _state.Select(shape.Id);
        }
    }

    /// <summary>Ends a drag without a result: the draft is dropped and a moved shape keeps the position it has.</summary>
    public void Cancel()
    {
        EndDrag();
    }

    /// <summary>Removes the selected shape from the document. Does nothing without a selection.</summary>
    public void DeleteSelection()
    {
        if (_state.SelectedShapeId is not { } id || _state.Document.FindShape(id) is null)
        {
            return;
        }

        Cancel();
        _state.Document = _state.Document.RemoveShape(id);
        _state.Select(null);
    }

    private void PressWithSelectTool(double x, double y)
    {
        var document = _state.Document;
        foreach (var hit in document.HitTest(x, y))
        {
            if (document.FindShape(hit.Id) is not { } location || location.Layer.IsLocked)
            {
                continue;
            }

            _state.Select(hit.Id);
            _movedShapeId = hit.Id;
            _movedShapeStartX = hit.X;
            _movedShapeStartY = hit.Y;
            BeginDrag(DragType.Moving, x, y);
            return;
        }

        _state.Select(null);
    }

    private bool CanDrawInActiveLayer() =>
        _state.Document.Layers.FirstOrDefault(l => l.Id == _state.ActiveLayerId) is { IsVisible: true, IsLocked: false };

    private void BeginDrag(DragType drag, double x, double y)
    {
        _drag = drag;
        _dragTool = _state.ActiveTool;
        _startX = x;
        _startY = y;
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
        _drag = DragType.None;
        _state.Draft = null;
    }

    // Absolute from the press point, so many small moves never add up to a drift.
    private void MoveShape(double x, double y)
    {
        var shapeId = _movedShapeId;
        if (_state.Document.FindShape(shapeId) is null)
        {
            return;
        }

        var newX = _movedShapeStartX + (x - _startX);
        var newY = _movedShapeStartY + (y - _startY);
        _state.Document = _state.Document.UpdateShape(shapeId, s => s switch
        {
            RectangleShape r => r with { X = newX, Y = newY },
            EllipseShape e => e with { X = newX, Y = newY },
            _ => s,
        });
    }

    private Shape? CreateShape(double x, double y)
    {
        var left = Math.Min(_startX, x);
        var top = Math.Min(_startY, y);
        var width = Math.Abs(x - _startX);
        var height = Math.Abs(y - _startY);

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
