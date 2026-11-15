namespace PressToShape.App.Workspace;

/// <summary>
/// Decides whether a press became a long press. The caller feeds it the pointer events and the time (so there is no clock in here and
/// tests need no real time). It fires once, when the pointer has been down for <see cref="DelayMs"/> without moving farther than
/// <see cref="Slop"/>. <see cref="ConsumeFired"/> tells the caller to ignore the click that follows the release.
/// </summary>
internal sealed class LongPressTracker
{
    /// <summary>How long the pointer must stay down, in milliseconds.</summary>
    public const long DelayMs = 500;

    /// <summary>How far the pointer may move and still count as holding, in the units of the caller (dp).</summary>
    public const double Slop = 10;

    private bool _isDown;
    private bool _hasFired;
    private bool _fired;
    private double _startX;
    private double _startY;
    private long _startTime;

    /// <summary>Whether a press is being watched, so the caller knows to keep ticking.</summary>
    public bool IsWatching => _isDown && !_hasFired;

    public void Press(double x, double y, long timestampMs)
    {
        _isDown = true;
        _hasFired = false;
        _fired = false;
        _startX = x;
        _startY = y;
        _startTime = timestampMs;
    }

    /// <summary>The pointer moved. Moving too far ends the watching.</summary>
    public void Move(double x, double y)
    {
        if (!_isDown)
        {
            return;
        }

        var dx = x - _startX;
        var dy = y - _startY;
        if (dx * dx + dy * dy > Slop * Slop)
        {
            _isDown = false;
        }
    }

    public void Release() => _isDown = false;

    public void Cancel()
    {
        _isDown = false;
        _fired = false;
    }

    /// <summary>Time passed. Returns true exactly once, at the moment the long press happens.</summary>
    public bool Tick(long timestampMs)
    {
        if (!IsWatching || timestampMs - _startTime < DelayMs)
        {
            return false;
        }

        _hasFired = true;
        _fired = true;
        return true;
    }

    /// <summary>Whether the last press was a long press. True once, then false until the next long press.</summary>
    public bool ConsumeFired()
    {
        var fired = _fired;
        _fired = false;
        return fired;
    }
}
