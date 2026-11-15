using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PressToShape.App.Views.Tablet;

/// <summary>
/// The compact toolbar of the tablet layout. All behavior is in <see cref="ViewModels.Tablet.TabletToolbarViewModel"/>. The view only knows which of
/// its two menus (the overflow menu and the zoom list) is open, so the Back button can close it first: a menu is not part of the view model.
/// </summary>
public partial class TabletToolbarView : UserControl
{
    private readonly List<FlyoutBase> _open = [];

    public TabletToolbarView()
    {
        InitializeComponent();

        foreach (var flyout in new[] { OverflowButton.Flyout, ZoomButton.Flyout }.OfType<FlyoutBase>())
        {
            flyout.Opened += (_, _) => _open.Add(flyout);
            flyout.Closed += (_, _) =>
            {
                _open.Remove(flyout);
                MenuClosed?.Invoke(this, EventArgs.Empty);
            };
        }
    }

    /// <summary>Raised when the overflow menu or the zoom list closed, however it was closed.</summary>
    public event EventHandler? MenuClosed;

    /// <summary>Closes the menu that is open, if there is one.</summary>
    /// <returns>Whether a menu was open and is now closing.</returns>
    public bool TryCloseMenu()
    {
        var menu = _open.LastOrDefault();
        if (menu is null)
        {
            return false;
        }

        menu.Hide();
        return true;
    }
}
