# ToolboxView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/ToolboxView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.ToolboxView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.ToolboxViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:67)
- `T:PressToShape.App.ViewModels.Editor.ToolboxViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42)
- `T:PressToShape.App.ViewModels.Editor.ToolboxViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/ToolboxView.axaml:1)

## Bindings

- line 17: controls:ToolToggleButton.IsChecked = Binding `IsActive` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.IsActive` (verified)
- line 18: controls:ToolToggleButton.Command = Binding `SelectCommand` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.SelectCommand` (verified), command
- line 19: controls:ToolToggleButton.ToolTip.Tip = Binding `ToolTip` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.ToolTip` (verified)
- line 20: controls:ToolToggleButton.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.Name` (verified)
- line 22: materialIcons:MaterialIcon.Kind = Binding `Icon` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.Icon` (verified)
- line 36: ItemsControl.ItemsSource = Binding `Tools` -> `P:PressToShape.App.ViewModels.Editor.ToolboxViewModel.Tools` (verified)

## Resource references

- line 21: controls:ToolToggleButton.Margin = DynamicResource `ToolButtonMargin` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolButtonMargin` (verified)
- line 22: materialIcons:MaterialIcon.Height = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 22: materialIcons:MaterialIcon.Width = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 27: Border.Background = DynamicResource `ToolColumnBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolColumnBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolColumnBackgroundBrush@Light` (verified)
- line 28: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 30: Border.Padding = DynamicResource `ToolColumnPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolColumnPadding` (verified)
- line 36: ItemsControl.ItemTemplate = StaticResource `ToolButtonTemplate` -> `R:src/PressToShape.App/Views/Editor/ToolboxView.axaml#/UserControl/UserControl.Resources[1]/ToolButtonTemplate` (verified)

## Resources defined

- line 15: `ToolButtonTemplate` (DataTemplate)

## Templates

- line 15: DataTemplate `ToolButtonTemplate` for `vm:ToolItemViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolItemViewModel`)
