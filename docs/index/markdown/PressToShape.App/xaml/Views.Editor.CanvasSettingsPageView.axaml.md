# CanvasSettingsPageView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/CanvasSettingsPageView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.CanvasSettingsPageView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.CanvasSettingsPageViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/CanvasSettingsPageView.axaml:1)

## Bindings

- line 16: CheckBox.IsChecked = Binding `Settings.ShowRulers` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.ShowRulers` (verified)
- line 17: CheckBox.IsChecked = Binding `Settings.ShowGrid` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.ShowGrid` (verified)
- line 22: GridSpacingBox.Text = Binding `Settings.GridSpacingText` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.GridSpacingText` (verified)
- line 28: TextBlock.IsVisible = Binding `Settings.HasGridSpacingError` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.HasGridSpacingError` (verified)
- line 28: TextBlock.Text = Binding `Settings.GridSpacingError` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.GridSpacingError` (verified)
- line 32: Button.Command = Binding `Settings.RestoreDefaultsCommand` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.RestoreDefaultsCommand` (verified), command

## Resource references

- line 12: StackPanel.Margin = DynamicResource `SettingsPagePadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SettingsPagePadding` (verified)
- line 12: StackPanel.Spacing = DynamicResource `SettingsRowSpacing` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SettingsRowSpacing` (verified)
- line 21: GridSpacingBox.Width = DynamicResource `SettingsFieldWidth` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SettingsFieldWidth` (verified)
