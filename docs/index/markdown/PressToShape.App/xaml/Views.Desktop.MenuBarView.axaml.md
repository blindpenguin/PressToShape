# MenuBarView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Desktop/MenuBarView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Desktop.MenuBarView`.

## View model links

- `T:PressToShape.App.ViewModels.Desktop.MenuBarViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:35 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:35)
- `T:PressToShape.App.ViewModels.Desktop.MenuBarViewModel` (verified): x:DataType (src/PressToShape.App/Views/Desktop/MenuBarView.axaml:1)

## Bindings

- line 20: Setter.Header = Binding `Header` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Header` (verified)
- line 21: Setter.ItemsSource = Binding `Children` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Children` (verified)
- line 22: Setter.Command = Binding `Command` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Command` (verified)
- line 23: Setter.IsEnabled = Binding `IsEnabled` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.IsEnabled` (verified)
- line 32: Setter.Header = Binding `Header` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Header` (verified)
- line 33: Setter.ItemsSource = Binding `Children` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Children` (verified)
- line 34: Setter.Command = Binding `Command` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Command` (verified)
- line 35: Setter.IsEnabled = Binding `IsEnabled` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.IsEnabled` (verified)
- line 36: Setter.ToolTip.Tip = Binding `ToolTip` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.ToolTip` (verified)
- line 37: Setter.InputGesture = Binding `Gesture` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Gesture` (verified), converter StaticResource GestureTextConverter
- line 38: Setter.ToggleType = Binding `IsCheckable` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.IsCheckable` (verified), converter StaticResource CheckableToggleTypeConverter
- line 39: Setter.IsChecked = Binding `IsChecked` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.IsChecked` (verified)
- line 44: Menu.ItemsSource = Binding `Menus` -> `P:PressToShape.App.ViewModels.Desktop.MenuBarViewModel.Menus` (verified)

## Resource references

- line 18: ControlTheme.BasedOn = StaticResource `FluentTopLevelMenuItem` -> unresolved: not defined in this solution (may come from the Avalonia theme)
- line 24: Setter.ItemContainerTheme = DynamicResource `AppMenuItemTheme` -> `R:src/PressToShape.App/Views/Desktop/MenuBarView.axaml#/UserControl/UserControl.Resources[1]/AppMenuItemTheme` (verified)
- line 30: ControlTheme.BasedOn = StaticResource `{x:Type MenuItem}` -> unresolved: not defined in this solution (may come from the Avalonia theme)
- line 40: Setter.ItemContainerTheme = DynamicResource `AppMenuItemTheme` -> `R:src/PressToShape.App/Views/Desktop/MenuBarView.axaml#/UserControl/UserControl.Resources[1]/AppMenuItemTheme` (verified)
- line 45: Menu.ItemContainerTheme = StaticResource `AppTopLevelMenuItemTheme` -> `R:src/PressToShape.App/Views/Desktop/MenuBarView.axaml#/UserControl/UserControl.Resources[1]/AppTopLevelMenuItemTheme` (verified)

## Resources defined

- line 12: `GestureTextConverter` (views:GestureTextConverter)
- line 13: `CheckableToggleTypeConverter` (views:CheckableToggleTypeConverter)
- line 16: `AppTopLevelMenuItemTheme` (ControlTheme)
- line 28: `AppMenuItemTheme` (ControlTheme)
