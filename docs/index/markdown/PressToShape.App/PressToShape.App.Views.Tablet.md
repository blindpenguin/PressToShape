# PressToShape.App.Views.Tablet

Project: PressToShape.App

## TabletColorSwatchView (class)

`public partial class TabletColorSwatchView : UserControl`

Source: `src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml.cs:6-12`

The small fill and stroke swatch of the tablet tool column. All behavior is in PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.

**Members**

- `public TabletColorSwatchView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## TabletPanelView (class)

`public partial class TabletPanelView : UserControl`

Source: `src/PressToShape.App/Views/Tablet/TabletPanelView.axaml.cs:6-12`

The tabbed panel of the tablet layout, with an icon for each tab. All state is in the inspector view model.

**Members**

- `public TabletPanelView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## TabletShellView (class)

`public partial class TabletShellView : UserControl`

Source: `src/PressToShape.App/Views/Tablet/TabletShellView.axaml.cs:19-139`

The root of the tablet layout. The code-behind has three view-only jobs that need the platform or the pointer: it hands the Back button to the view model (after closing an open menu of the toolbar or the status bar, which only the view knows about), it closes the panel when the canvas is touched, and it keeps the layout above the on-screen keyboard. Avalonia already keeps the content out from under the system bars, and the keyboard does not resize the view but only reports the area it covers (found on the device), so the bottom margin is set here.

**Members**

- `internal Grid LayoutRoot` _(generated)_ - _(no documentation)_
- `internal TabletToolbarView ToolbarView` _(generated)_ - _(no documentation)_
- `internal CanvasAreaView CanvasArea` _(generated)_ - _(no documentation)_
- `internal TabletStatusBarView StatusBarView` _(generated)_ - _(no documentation)_
- `internal Border DialogScrim` _(generated)_ - _(no documentation)_
- `public TabletShellView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).
- `protected void OnDataContextChanged(EventArgs e)` - _(no documentation)_
- `protected void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)` - _(no documentation)_
- `protected void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)` - _(no documentation)_

## TabletStatusBarView (class)

`public partial class TabletStatusBarView : UserControl`

Source: `src/PressToShape.App/Views/Tablet/TabletStatusBarView.axaml.cs:11-29`

The status bar of the tablet layout. All behavior is in PressToShape.App.ViewModels.Editor.StatusBarViewModel. The view only knows whether its zoom list is open, so the Back button can close it first: a menu is not part of the view model.

**Members**

- `internal DropDownButton ZoomButton` _(generated)_ - _(no documentation)_
- `public TabletStatusBarView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).
- `public EventHandler? MenuClosed` - Raised when the zoom list closed, however it was closed.
- `public bool TryCloseMenu()` - Closes the zoom list if it is open.

## TabletToolColumnView (class)

`public partial class TabletToolColumnView : UserControl`

Source: `src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml.cs:5-11`

_(no documentation)_

**Members**

- `public TabletToolColumnView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## TabletToolbarView (class)

`public partial class TabletToolbarView : UserControl`

Source: `src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml.cs:11-29`

The compact toolbar of the tablet layout. All behavior is in PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel. The view only knows whether its menu (the overflow menu) is open, so the Back button can close it first: a menu is not part of the view model.

**Members**

- `internal Button OverflowButton` _(generated)_ - _(no documentation)_
- `public TabletToolbarView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).
- `public EventHandler? MenuClosed` - Raised when the overflow menu closed, however it was closed.
- `public bool TryCloseMenu()` - Closes the menu that is open, if there is one.
