using System;
using Avalonia.Controls;
using PressToShape.App.Controls;

namespace PressToShape.App.Views.Tablet;

/// <summary>
/// The status bar of the tablet layout. All behavior is in <see cref="ViewModels.Editor.StatusBarViewModel"/>. The view only knows whether its zoom
/// list is open, so the Back button can close it first: a menu is not part of the view model.
/// </summary>
public partial class TabletStatusBarView : UserControl
{
    private readonly OpenFlyoutTracker _menus = new();

    public TabletStatusBarView()
    {
        InitializeComponent();

        _menus.Track(ZoomButton.Flyout);
        _menus.Closed += (_, _) => MenuClosed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raised when the zoom list closed, however it was closed.</summary>
    public event EventHandler? MenuClosed;

    /// <summary>Closes the zoom list if it is open.</summary>
    /// <returns>Whether the list was open and is now closing.</returns>
    public bool TryCloseMenu() => _menus.TryCloseLast();
}
