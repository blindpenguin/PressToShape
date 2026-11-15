# ToolbarView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Desktop/ToolbarView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Desktop.ToolbarView`.

## View model links

- `T:PressToShape.App.ViewModels.Desktop.ToolbarViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:38 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:38)
- `T:PressToShape.App.ViewModels.Desktop.ToolbarViewModel` (verified): x:DataType (src/PressToShape.App/Views/Desktop/ToolbarView.axaml:1)

## Bindings

- line 15: Button.Command = Binding `Command` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Command` (verified)
- line 16: Button.IsEnabled = Binding `IsEnabled` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.IsEnabled` (verified)
- line 17: Button.ToolTip.Tip = Binding `Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 18: Button.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 19: materialIcons:MaterialIcon.Kind = Binding `Icon` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Icon` (verified)
- line 39: ItemsControl.ItemsSource = Binding `FileActions` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarViewModel.FileActions` (verified)
- line 49: ItemsControl.ItemsSource = Binding `HistoryActions` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarViewModel.HistoryActions` (verified)
- line 60: ToggleButton.IsChecked = Binding `IsSelectToolActive` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarViewModel.IsSelectToolActive` (verified)
- line 66: ToggleButton.IsChecked = Binding `IsNodeToolActive` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarViewModel.IsNodeToolActive` (verified)
- line 74: ItemsControl.ItemsSource = Binding `HorizontalAlignActions` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarViewModel.HorizontalAlignActions` (verified)
- line 84: ItemsControl.ItemsSource = Binding `TransformActions` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarViewModel.TransformActions` (verified)
- line 94: ItemsControl.ItemsSource = Binding `VerticalAlignActions` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarViewModel.VerticalAlignActions` (verified)
- line 104: ItemsControl.ItemsSource = Binding `OtherActions` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarViewModel.OtherActions` (verified)

## Resource references

- line 28: Setter.Background = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 32: Border.Background = DynamicResource `ToolbarBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Light` (verified)
- line 33: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 35: Border.Height = StaticResource `ToolbarHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarHeight` (verified)
- line 39: ItemsControl.ItemTemplate = StaticResource `ToolbarButtonTemplate` -> `R:src/PressToShape.App/Views/Desktop/ToolbarView.axaml#/UserControl/UserControl.Resources[1]/ToolbarButtonTemplate` (verified)
- line 49: ItemsControl.ItemTemplate = StaticResource `ToolbarButtonTemplate` -> `R:src/PressToShape.App/Views/Desktop/ToolbarView.axaml#/UserControl/UserControl.Resources[1]/ToolbarButtonTemplate` (verified)
- line 74: ItemsControl.ItemTemplate = StaticResource `ToolbarButtonTemplate` -> `R:src/PressToShape.App/Views/Desktop/ToolbarView.axaml#/UserControl/UserControl.Resources[1]/ToolbarButtonTemplate` (verified)
- line 84: ItemsControl.ItemTemplate = StaticResource `ToolbarButtonTemplate` -> `R:src/PressToShape.App/Views/Desktop/ToolbarView.axaml#/UserControl/UserControl.Resources[1]/ToolbarButtonTemplate` (verified)
- line 94: ItemsControl.ItemTemplate = StaticResource `ToolbarButtonTemplate` -> `R:src/PressToShape.App/Views/Desktop/ToolbarView.axaml#/UserControl/UserControl.Resources[1]/ToolbarButtonTemplate` (verified)
- line 104: ItemsControl.ItemTemplate = StaticResource `ToolbarButtonTemplate` -> `R:src/PressToShape.App/Views/Desktop/ToolbarView.axaml#/UserControl/UserControl.Resources[1]/ToolbarButtonTemplate` (verified)

## Resources defined

- line 13: `ToolbarButtonTemplate` (DataTemplate)

## Styles

- line 25: `Border.group-separator`, sets Background, Margin, Width, uses BorderSubtleBrush

## Templates

- line 13: DataTemplate `ToolbarButtonTemplate` for `vm:ToolbarButtonViewModel` (`T:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel`)
