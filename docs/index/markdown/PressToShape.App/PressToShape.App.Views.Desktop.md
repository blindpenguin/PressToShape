# PressToShape.App.Views.Desktop

Project: PressToShape.App

## CheckableToggleTypeConverter (class)

`internal sealed class CheckableToggleTypeConverter : IValueConverter`

Source: `src/PressToShape.App/Views/Desktop/CheckableToggleTypeConverter.cs:9-16`

Maps the view model flag IsCheckable to a radio toggle for menu items. Other items show no toggle.

**Members**

- `public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_
- `public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_

## DesktopShellView (class)

`public partial class DesktopShellView : UserControl`

Source: `src/PressToShape.App/Views/Desktop/DesktopShellView.axaml.cs:8-72`

_(no documentation)_

**Members**

- `public DesktopShellView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).
- `protected void OnKeyDown(KeyEventArgs e)` - Single-key tool shortcuts. The view only decides whether the key press is meant for the shell (no modifiers, no text field focused). The view model decides which tool it selects.

## GestureTextConverter (class)

`internal sealed class GestureTextConverter : IValueConverter`

Source: `src/PressToShape.App/Views/Desktop/GestureTextConverter.cs:9-48`

Turns a shortcut text such as "Ctrl+Shift+S" into a Avalonia.Input.KeyGesture so the menu can display it.

**Members**

- `public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_
- `public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_

## MenuBarView (class)

`public partial class MenuBarView : UserControl`

Source: `src/PressToShape.App/Views/Desktop/MenuBarView.axaml.cs:5-11`

_(no documentation)_

**Members**

- `public MenuBarView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## StatusBarView (class)

`public partial class StatusBarView : UserControl`

Source: `src/PressToShape.App/Views/Desktop/StatusBarView.axaml.cs:5-11`

_(no documentation)_

**Members**

- `public StatusBarView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## ToolbarView (class)

`public partial class ToolbarView : UserControl`

Source: `src/PressToShape.App/Views/Desktop/ToolbarView.axaml.cs:5-11`

_(no documentation)_

**Members**

- `public ToolbarView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).
