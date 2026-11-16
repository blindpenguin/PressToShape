using System;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

// The long press of a finger or a pen opens the ring menu. The view ticks the clock while a press is watched, because nothing else
// tells the view model that time has passed.
internal sealed partial class CanvasAreaViewModel
{
    private readonly LongPressTracker _longPress = new();
    private double _longPressX;
    private double _longPressY;
    private InputPointerType _longPressType;

    /// <summary>Whether a press may still become a long press, so the view keeps calling <see cref="Tick"/>.</summary>
    public bool IsWatchingLongPress => _longPress.IsWatching;

    private long ElapsedMilliseconds => (long)_time.GetElapsedTime(_clockStart).TotalMilliseconds;

    /// <summary>
    /// Time passed. When a finger or a pen has stayed down long enough without moving, the action the press started is undone and the ring
    /// menu opens at the press. The release that follows is no tap.
    /// </summary>
    public void Tick()
    {
        if (!_longPress.IsWatching || !_longPress.Tick(ElapsedMilliseconds))
        {
            return;
        }

        AbortAction();
        _doubleTap.Reset();
        OpenContextMenu(_longPressX, _longPressY, _longPressType);
    }

    // Only a finger or a pen can long press, only with the Select tool, and not while the ring is open or a color is picked.
    private void WatchLongPress(double screenX, double screenY, InputPointerType inputType)
    {
        _longPress.Cancel();
        if (inputType == InputPointerType.Mouse
            || Ring is not { IsOpen: false }
            || _state.ActiveTool != Tool.Select
            || _state.ColorPick != ColorPickTarget.None)
        {
            return;
        }

        (_longPressX, _longPressY, _longPressType) = (screenX, screenY, inputType);
        _longPress.Press(screenX, screenY, ElapsedMilliseconds);
    }
}
