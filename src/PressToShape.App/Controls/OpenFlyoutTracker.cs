using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Primitives;

namespace PressToShape.App.Controls;

/// <summary>
/// Knows which of some flyouts are open, so the Back button can close the last one that was opened before anything else happens. A menu is not part
/// of a view model, so this is a view-only helper. Used by the tablet toolbar (overflow menu) and the tablet status bar (zoom list).
/// </summary>
internal sealed class OpenFlyoutTracker
{
    private readonly List<FlyoutBase> _open = [];

    /// <summary>Raised when a tracked flyout closed, however it was closed.</summary>
    public event EventHandler? Closed;

    /// <summary>Starts to watch the flyouts. Flyouts that are <c>null</c> are skipped.</summary>
    public void Track(params FlyoutBase?[] flyouts)
    {
        foreach (var flyout in flyouts.OfType<FlyoutBase>())
        {
            flyout.Opened += (_, _) => _open.Add(flyout);
            flyout.Closed += (_, _) =>
            {
                _open.Remove(flyout);
                Closed?.Invoke(this, EventArgs.Empty);
            };
        }
    }

    /// <summary>Closes the flyout that was opened last and is still open, if there is one.</summary>
    /// <returns>Whether a flyout was open and is now closing.</returns>
    public bool TryCloseLast()
    {
        var flyout = _open.LastOrDefault();
        if (flyout is null)
        {
            return false;
        }

        flyout.Hide();
        return true;
    }
}
