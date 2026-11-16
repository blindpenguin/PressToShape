using System;

namespace PressToShape.App.Workspace;

/// <summary>
/// Recognises a double-tap: two taps close in time and place. It holds no clock: the caller says when a tap happened, so a test can say it
/// with plain numbers. A tap that moved too far, or any other input in between, is not a tap and starts the count again.
/// </summary>
internal sealed class DoubleTapDetector
{
    /// <summary>The most time between two taps of a double-tap, in milliseconds.</summary>
    public const long MaxIntervalMilliseconds = 400;

    /// <summary>The most distance, in screen pixels, between two taps of a double-tap, and the most a finger may move during one tap.</summary>
    public const double MaxDistance = 12;

    private long _lastTime;
    private double _lastX;
    private double _lastY;
    private bool _hasLast;

    /// <summary>
    /// A tap ended at this place and time. Returns whether it completes a double-tap. A completed double-tap uses up both taps, so a third
    /// tap right after does not count as another one.
    /// </summary>
    /// <param name="timeMilliseconds">When the tap ended, in milliseconds on any clock that only goes forward.</param>
    /// <param name="x">Screen position of the tap.</param>
    /// <param name="y">Screen position of the tap.</param>
    public bool Tap(long timeMilliseconds, double x, double y)
    {
        var isDouble = _hasLast
            && timeMilliseconds - _lastTime is >= 0 and <= MaxIntervalMilliseconds
            && Math.Abs(x - _lastX) <= MaxDistance
            && Math.Abs(y - _lastY) <= MaxDistance;

        if (isDouble)
        {
            _hasLast = false;
            return true;
        }

        (_lastTime, _lastX, _lastY, _hasLast) = (timeMilliseconds, x, y, true);
        return false;
    }

    /// <summary>The last tap does not count any more: something else happened (a drag, two fingers, another tool).</summary>
    public void Reset() => _hasLast = false;
}
