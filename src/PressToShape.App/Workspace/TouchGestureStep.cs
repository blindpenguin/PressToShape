namespace PressToShape.App.Workspace;

/// <summary>One decision of the <see cref="TouchGestureTracker"/>. All positions are screen positions of the canvas.</summary>
/// <param name="Type">What to do.</param>
/// <param name="X">The position of the press, move or release, or the midpoint of the two fingers for a gesture.</param>
/// <param name="Y">See <paramref name="X"/>.</param>
/// <param name="TargetZoom">For a gesture: the zoom in percent, within the zoom range.</param>
/// <param name="PreviousMidX">For a gesture: the midpoint of the two fingers before this event.</param>
/// <param name="PreviousMidY">For a gesture: see <paramref name="PreviousMidX"/>.</param>
internal readonly record struct TouchGestureStep(
    TouchGestureStepType Type,
    double X = 0,
    double Y = 0,
    double TargetZoom = 0,
    double PreviousMidX = 0,
    double PreviousMidY = 0)
{
    public static TouchGestureStep Ignored => new(TouchGestureStepType.Ignore);
}
