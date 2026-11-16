using System;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

// The drag of a radius handle of the one selected rectangle (Select and Direct Select). Like the other drags, every move is computed from
// the rectangle at the press, and the release records one history step.
internal sealed partial class CanvasInteraction
{
    private Guid _radiusShapeId;
    private Corner _radiusCorner;
    private double _radiusGrabOffset;
    private bool _radiusByTouch;

    /// <summary>Whether a radius handle is being dragged.</summary>
    public bool IsRadiusDragging => _drag == DragType.RoundingCorners;

    // Starts the drag if the press hits a radius handle. The handles are tested before every other handle and shape, so they win where
    // they overlap the resize handles. Returns false when nothing was hit, and the caller goes on with its own press.
    private bool TryBeginRadiusDrag(double x, double y, InputPointerType inputType)
    {
        if (SelectionQueries.SelectedSelectable(_state.Document, _state.SelectedShapeIds) is not [RectangleShape rectangle]
            || !RadiusHandles.IsShown(rectangle, _state.ZoomPercent))
        {
            return false;
        }

        var byTouch = inputType == InputPointerType.Touch;
        var grab = byTouch ? RadiusHandles.TouchGrabSize : RadiusHandles.GrabSize;
        if (RadiusHandles.Hit(rectangle, x, y, _state.ZoomPercent, grab) is not { } corner)
        {
            return false;
        }

        _startDocument = _state.Document;
        _startShapes = [rectangle];
        _radiusShapeId = rectangle.Id;
        _radiusCorner = corner;
        _radiusByTouch = byTouch;

        // The pointer may be a few pixels off the handle's center: keep that distance so the radius does not jump.
        _radiusGrabOffset = RadiusHandles.GrabOffset(rectangle, corner, x, y, _state.ZoomPercent);
        BeginDrag(DragType.RoundingCorners, x, y);
        return true;
    }

    // With a mouse or a pen Shift decides: without it all four radii change, with it only the radius of the dragged corner. By touch there
    // is no Shift key, so the link toggle decides.
    private void ShowRadius(double x, double y, bool shift)
    {
        if (_startShapes is not [RectangleShape start])
        {
            return;
        }

        // Rounded, so the rounding noise of the geometry never turns a press without a move into a change of the radius.
        var radius = Math.Round(RadiusHandles.RadiusAt(start, _radiusCorner, x, y, _state.ZoomPercent, _radiusGrabOffset), 4);
        var onlyThisCorner = _radiusByTouch ? !_state.LinkCornerRadii : shift;
        var radii = onlyThisCorner ? start.Radii.With(_radiusCorner, radius) : CornerRadii.Uniform(radius);
        Show([start with { Radii = radii }]);
    }
}
