using System;
using System.Collections.Generic;

namespace PressToShape.App.Workspace;

/// <summary>
/// Decides what the fingers on the canvas mean. One finger acts with the active tool, like the mouse. A second finger cancels that action
/// (so nothing stray stays in the drawing) and the two fingers then move and zoom the view around their midpoint. When a finger of the two
/// lifts, the rest is ignored until every finger is up, so a lifting hand does not draw. Mouse and pen are not tracked.
/// The tracker works on plain numbers and holds no document or view state. A pointer id only has to be unique from a finger's press to its release.
/// </summary>
internal sealed class TouchGestureTracker
{
    private enum Phase
    {
        Idle,
        OneFinger,
        TwoFingers,
        WaitingForRelease,
    }

    // Two fingers that touch at the same spot have no distance to compare with, so the first distance counts as at least one pixel.
    private const double MinimumStartDistance = 1;

    private readonly HashSet<int> _down = [];
    private Phase _phase = Phase.Idle;
    private int _firstId;
    private double _firstX;
    private double _firstY;
    private int _secondId;
    private double _secondX;
    private double _secondY;
    private double _startDistance;
    private double _startZoom;
    private double _midX;
    private double _midY;

    /// <summary>Whether two fingers are moving the view right now.</summary>
    public bool IsGestureActive => _phase == Phase.TwoFingers;

    /// <param name="id">The pointer id.</param>
    /// <param name="type">The device. Only <see cref="InputPointerType.Touch"/> is tracked.</param>
    /// <param name="x">Screen position.</param>
    /// <param name="y">Screen position.</param>
    /// <param name="currentZoomPercent">The zoom now. A second finger starts the gesture from this zoom.</param>
    public TouchGestureStep PointerDown(int id, InputPointerType type, double x, double y, double currentZoomPercent)
    {
        if (type != InputPointerType.Touch)
        {
            return new TouchGestureStep(TouchGestureStepType.NotTracked);
        }

        if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(currentZoomPercent) || !_down.Add(id))
        {
            return TouchGestureStep.Ignored;
        }

        switch (_phase)
        {
            case Phase.Idle:
                _phase = Phase.OneFinger;
                (_firstId, _firstX, _firstY) = (id, x, y);
                return new TouchGestureStep(TouchGestureStepType.Press, x, y);
            case Phase.OneFinger:
                _phase = Phase.TwoFingers;
                (_secondId, _secondX, _secondY) = (id, x, y);
                _startDistance = Math.Max(Distance(), MinimumStartDistance);
                _startZoom = ZoomLevels.Clamp(currentZoomPercent);
                (_midX, _midY) = Midpoint();
                return new TouchGestureStep(TouchGestureStepType.CancelPress);
            default:
                // A third finger, or any finger while the hand is lifting, does nothing.
                return TouchGestureStep.Ignored;
        }
    }

    public TouchGestureStep PointerMove(int id, double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y))
        {
            return TouchGestureStep.Ignored;
        }

        switch (_phase)
        {
            case Phase.OneFinger when id == _firstId:
                (_firstX, _firstY) = (x, y);
                return new TouchGestureStep(TouchGestureStepType.Move, x, y);
            case Phase.TwoFingers when id == _firstId:
                (_firstX, _firstY) = (x, y);
                return Gesture();
            case Phase.TwoFingers when id == _secondId:
                (_secondX, _secondY) = (x, y);
                return Gesture();
            default:
                return TouchGestureStep.Ignored;
        }
    }

    public TouchGestureStep PointerUp(int id, double x, double y)
    {
        if (!_down.Remove(id))
        {
            return TouchGestureStep.Ignored;
        }

        switch (_phase)
        {
            case Phase.OneFinger when id == _firstId:
                _phase = Phase.Idle;
                var (releaseX, releaseY) = double.IsFinite(x) && double.IsFinite(y) ? (x, y) : (_firstX, _firstY);
                return new TouchGestureStep(TouchGestureStepType.Release, releaseX, releaseY);
            case Phase.TwoFingers when id == _firstId || id == _secondId:
                _phase = Phase.WaitingForRelease;
                break;
        }

        if (_phase == Phase.WaitingForRelease && _down.Count == 0)
        {
            _phase = Phase.Idle;
        }

        return TouchGestureStep.Ignored;
    }

    /// <summary>The system took the fingers away (for example the pointer capture was lost). Forgets every finger. A running one-finger action must be cancelled.</summary>
    public TouchGestureStep Cancel()
    {
        var wasOneFinger = _phase == Phase.OneFinger;
        _phase = Phase.Idle;
        _down.Clear();
        return wasOneFinger ? new TouchGestureStep(TouchGestureStepType.CancelPress) : TouchGestureStep.Ignored;
    }

    private TouchGestureStep Gesture()
    {
        var (previousX, previousY) = (_midX, _midY);
        (_midX, _midY) = Midpoint();
        var target = ZoomLevels.Clamp(_startZoom * Distance() / _startDistance);
        return new TouchGestureStep(TouchGestureStepType.Gesture, _midX, _midY, target, previousX, previousY);
    }

    private double Distance() => Math.Sqrt(((_secondX - _firstX) * (_secondX - _firstX)) + ((_secondY - _firstY) * (_secondY - _firstY)));

    private (double X, double Y) Midpoint() => ((_firstX + _secondX) / 2, (_firstY + _secondY) / 2);
}
