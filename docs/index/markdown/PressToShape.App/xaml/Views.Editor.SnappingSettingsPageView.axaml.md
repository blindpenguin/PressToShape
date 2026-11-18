# SnappingSettingsPageView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/SnappingSettingsPageView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.SnappingSettingsPageView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.SnappingSettingsPageViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/SnappingSettingsPageView.axaml:1)

## Bindings

- line 16: CheckBox.IsChecked = Binding `Settings.SnapToGrid` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.SnapToGrid` (verified)
- line 17: CheckBox.IsChecked = Binding `Settings.SnapToObjects` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.SnapToObjects` (verified)
- line 18: CheckBox.IsChecked = Binding `Settings.SmartGuides` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.SmartGuides` (verified)
- line 19: CheckBox.IsChecked = Binding `Settings.ShowGuides` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.ShowGuides` (verified)
- line 22: Button.Command = Binding `Settings.RestoreDefaultsCommand` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.RestoreDefaultsCommand` (verified), command

## Resource references

- line 12: StackPanel.Margin = DynamicResource `SettingsPagePadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SettingsPagePadding` (verified)
- line 12: StackPanel.Spacing = DynamicResource `SettingsRowSpacing` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SettingsRowSpacing` (verified)
