namespace PressToShape.App.Workspace;

/// <summary>
/// On Android one press of the Back key arrives twice: as the Escape key, which closes an open menu or flyout by itself, and as the back request.
/// If the Escape half already closed something, the back request must not do more (it would go on to ask about unsaved changes). The layout tells
/// this guard when a menu or flyout closed; a back request right after that (within <see cref="WindowMs"/>) is the second half of the same press.
/// The caller passes the time, so tests need no real time.
/// </summary>
internal sealed class BackRequestGuard
{
    /// <summary>How long after a close a back request still counts as the second half of the same key press, in milliseconds.</summary>
    public const long WindowMs = 300;

    private long? _closedAt;

    /// <summary>A menu or flyout was closed now.</summary>
    public void NoteClosed(long nowMs) => _closedAt = nowMs;

    /// <summary>Whether a back request now is the second half of a press that already closed something. Answers true once for each close.</summary>
    public bool ConsumeIfSamePress(long nowMs)
    {
        if (_closedAt is not { } closedAt || nowMs - closedAt > WindowMs)
        {
            return false;
        }

        _closedAt = null;
        return true;
    }
}
