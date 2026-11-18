# GeneralSettingsPageView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/GeneralSettingsPageView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.GeneralSettingsPageView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.GeneralSettingsPageViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/GeneralSettingsPageView.axaml:1)

## Bindings

- line 17: ListBox.ItemsSource = Binding `Settings.Languages` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.Languages` (verified)
- line 17: ListBox.SelectedItem = Binding `Settings.SelectedLanguage` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.SelectedLanguage` (verified)
- line 21: TextBlock.Text = Binding `DisplayName` -> `P:PressToShape.App.Localization.LanguageInfo.DisplayName` (verified)
- line 27: Button.Command = Binding `Settings.RestoreDefaultsCommand` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.RestoreDefaultsCommand` (verified), command

## Resource references

- line 12: StackPanel.Margin = DynamicResource `SettingsPagePadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SettingsPagePadding` (verified)
- line 12: StackPanel.Spacing = DynamicResource `SettingsRowSpacing` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SettingsRowSpacing` (verified)

## Templates

- line 20: DataTemplate for `loc:LanguageInfo` (`T:PressToShape.App.Localization.LanguageInfo`)
