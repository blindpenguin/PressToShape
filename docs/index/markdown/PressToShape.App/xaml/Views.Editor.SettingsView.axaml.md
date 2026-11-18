# SettingsView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/SettingsView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.SettingsView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/SettingsViewHeadlessTests.cs:55)
- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/SettingsViewHeadlessTests.cs:69)
- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/SettingsViewHeadlessTests.cs:94)
- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/SettingsView.axaml:1)

## Bindings

- line 17: ListBox.ItemsSource = Binding `Categories` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.Categories` (verified)
- line 17: ListBox.SelectedItem = Binding `SelectedCategory` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.SelectedCategory` (verified)
- line 22: materialIcons:MaterialIcon.Kind = Binding `Icon` -> `P:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel.Icon` (verified)
- line 23: TextBlock.Text = Binding `Title` -> `P:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel.Title` (verified)
- line 24: materialIcons:MaterialIcon.IsVisible = Binding `HasError` -> `P:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel.HasError` (verified)
- line 36: ContentControl.Content = Binding `SelectedCategory.Page` -> `P:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel.Page` (verified)
- line 42: Button.Command = Binding `OkCommand` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.OkCommand` (verified), command
- line 43: Button.Command = Binding `CancelCommand` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.CancelCommand` (verified), command

## Resource references

- line 16: ListBox.Width = DynamicResource `SettingsCategoryListWidth` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SettingsCategoryListWidth` (verified)
- line 22: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 22: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 25: materialIcons:MaterialIcon.Foreground = DynamicResource `ErrorTextBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ErrorTextBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ErrorTextBrush@Light` (verified)
- line 26: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 26: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 33: Border.Background = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 39: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 40: Border.Background = DynamicResource `PanelBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Light` (verified)
- line 40: Border.Padding = DynamicResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 41: StackPanel.Spacing = DynamicResource `PanelSpacing` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelSpacing` (verified)

## Templates

- line 20: DataTemplate for `vm:SettingsCategoryViewModel` (`T:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel`)
