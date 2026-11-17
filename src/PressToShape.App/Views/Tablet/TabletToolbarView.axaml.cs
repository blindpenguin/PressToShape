using System;
using Avalonia.Controls;
using PressToShape.App.Controls;

namespace PressToShape.App.Views.Tablet;

/// <summary>
/// The compact toolbar of the tablet layout. All behavior is in <see cref="ViewModels.Tablet.TabletToolbarViewModel"/>. The view only knows whether
/// its menu (the overflow menu) is open, so the Back button can close it first: a menu is not part of the view model.
/// </summary>
public partial class TabletToolbarView : UserControl
{
    private readonly OpenFlyoutTracker _menus = new();

    public TabletToolbarView()
    {
        InitializeComponent();

        _menus.Track(OverflowButton.Flyout);
        _menus.Closed += (_, _) => MenuClosed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raised when the overflow menu closed, however it was closed.</summary>
    public event EventHandler? MenuClosed;

    /// <summary>Closes the menu that is open, if there is one.</summary>
    /// <returns>Whether a menu was open and is now closing.</returns>
    public bool TryCloseMenu() => _menus.TryCloseLast();
}
