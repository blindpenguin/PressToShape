namespace PressToShape.App.Workspace;

/// <summary>
/// How far an arrow key moves the selection. Without Snap to Grid it is one document unit, ten with Shift. With Snap to Grid the top left
/// corner of the selection moves to the next grid line in the direction of the key, and with Shift ten grid spacings further. Pure function.
/// </summary>
internal static class GridNudge
{
    private const int ShiftSteps = 10;

    /// <summary>The distance to move for an arrow key. Any other key gives (0, 0).</summary>
    /// <param name="key">The key.</param>
    /// <param name="shift">Whether Shift is held.</param>
    /// <param name="snapToGrid">Whether Snap to Grid is on.</param>
    /// <param name="spacing">The grid spacing in document units.</param>
    /// <param name="left">The x of the left edge of the selection box. Only used with <paramref name="snapToGrid"/>.</param>
    /// <param name="top">The y of the top edge of the selection box. Only used with <paramref name="snapToGrid"/>.</param>
    public static (double Dx, double Dy) Delta(CanvasKey key, bool shift, bool snapToGrid, double spacing, double left, double top)
    {
        var steps = shift ? ShiftSteps : 1;
        return key switch
        {
            CanvasKey.Left => (snapToGrid ? GridMath.NudgeDelta(left, spacing, -1, steps) : -steps, 0),
            CanvasKey.Right => (snapToGrid ? GridMath.NudgeDelta(left, spacing, 1, steps) : steps, 0),
            CanvasKey.Up => (0, snapToGrid ? GridMath.NudgeDelta(top, spacing, -1, steps) : -steps),
            CanvasKey.Down => (0, snapToGrid ? GridMath.NudgeDelta(top, spacing, 1, steps) : steps),
            _ => (0, 0),
        };
    }
}
