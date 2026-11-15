# ToolboxView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/ToolboxView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.ToolboxView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.ToolboxViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:41 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:41)
- `T:PressToShape.App.ViewModels.Editor.ToolboxViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/ToolboxView.axaml:1)

## Bindings

- line 15: ToggleButton.IsChecked = Binding `IsActive` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.IsActive` (verified)
- line 16: ToggleButton.Command = Binding `SelectCommand` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.SelectCommand` (verified), command
- line 17: ToggleButton.ToolTip.Tip = Binding `ToolTip` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.ToolTip` (verified)
- line 18: ToggleButton.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.Name` (verified)
- line 20: materialIcons:MaterialIcon.Kind = Binding `Icon` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.Icon` (verified)
- line 43: Button.Command = Binding `SwapColorsCommand` -> `P:PressToShape.App.ViewModels.Editor.ToolboxViewModel.SwapColorsCommand` (verified), command
- line 49: Button.Command = Binding `ResetColorsCommand` -> `P:PressToShape.App.ViewModels.Editor.ToolboxViewModel.ResetColorsCommand` (verified), command
- line 55: ItemsControl.ItemsSource = Binding `Tools` -> `P:PressToShape.App.ViewModels.Editor.ToolboxViewModel.Tools` (verified)

## Resource references

- line 25: Border.Background = DynamicResource `ToolColumnBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolColumnBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolColumnBackgroundBrush@Light` (verified)
- line 26: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 34: Border.Background = DynamicResource `ControlBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Light` (verified)
- line 35: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 38: Border.Background = DynamicResource `AppAccentBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Light` (verified)
- line 39: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 55: ItemsControl.ItemTemplate = StaticResource `ToolButtonTemplate` -> `R:src/PressToShape.App/Views/Editor/ToolboxView.axaml#/UserControl/UserControl.Resources[1]/ToolButtonTemplate` (verified)

## Resources defined

- line 13: `ToolButtonTemplate` (DataTemplate)

## Templates

- line 13: DataTemplate `ToolButtonTemplate` for `vm:ToolItemViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolItemViewModel`)
