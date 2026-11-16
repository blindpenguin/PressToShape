namespace PressToShape.App.Workspace;

// What the context menu ring acts on: the selection, which a right-click or a long press may first change.
internal sealed partial class CanvasInteraction
{
    /// <summary>
    /// Prepares the selection for the ring menu at this document position, with the same hit rules as a click. A selected object, or a handle of
    /// the selection, keeps the selection as it is. Another object that can be selected (a group counts as one object, and inside an entered
    /// group a member counts) becomes the only selected one. Empty canvas, and objects that are hidden or locked, clear the selection.
    /// Returns which slots the ring shows. The selection changes, but no history step is recorded.
    /// </summary>
    public RingMenuMode SelectForContextMenu(double x, double y, double grabSize = SelectionGeometry.GrabSize)
    {
        if (CurrentFrame() is { } frame && SelectionGeometry.HitHandle(frame, x, y, _state.ZoomPercent, grabSize) is not null)
        {
            return RingMenuMode.Selection;
        }

        if (SelectableTargetAt(_state.Document, x, y) is not { } target)
        {
            _state.ClearSelection();
            return RingMenuMode.EmptyCanvas;
        }

        if (!_state.SelectedShapeIds.Contains(target.Id))
        {
            _state.SelectOnly(target.Id);
        }

        return RingMenuMode.Selection;
    }
}
