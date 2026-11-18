# TabletSettingsView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Tablet/TabletSettingsView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Tablet.TabletSettingsView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/SettingsViewHeadlessTests.cs:135)
- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/SettingsViewHeadlessTests.cs:157)
- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/SettingsViewHeadlessTests.cs:174)
- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:38 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:38)
- `T:PressToShape.App.ViewModels.Editor.SettingsViewModel` (verified): x:DataType (src/PressToShape.App/Views/Tablet/TabletSettingsView.axaml:1)

## Bindings

- line 18: Button.Command = Binding `BackCommand` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.BackCommand` (verified), command
- line 18: Button.IsVisible = Binding `IsPageOpen` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.IsPageOpen` (verified)
- line 27: ScrollViewer.IsVisible = Binding `!IsPageOpen` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.IsPageOpen` (verified)
- line 28: ItemsControl.ItemsSource = Binding `Categories` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.Categories` (verified)
- line 32: Button.Command = Binding `$parent[ItemsControl].((vm:SettingsViewModel)DataContext).OpenCategoryCommand` -> unresolved: at the start the path has an attached property or cast ((vm:SettingsViewModel)DataContext).OpenCategoryCommand, which is not modelled, source $parent[ItemsControl]
- line 33: Button.CommandParameter = Binding `` -> unresolved: x:DataType vm:SettingsCategoryViewModel
- line 35: materialIcons:MaterialIcon.Kind = Binding `Icon` -> `P:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel.Icon` (verified)
- line 36: TextBlock.Text = Binding `Title` -> `P:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel.Title` (verified)
- line 37: materialIcons:MaterialIcon.IsVisible = Binding `HasError` -> `P:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel.HasError` (verified)
- line 49: ContentControl.Content = Binding `SelectedCategory.Page` -> `P:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel.Page` (verified)
- line 49: ContentControl.IsVisible = Binding `IsPageOpen` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.IsPageOpen` (verified)
- line 55: Button.Command = Binding `OkCommand` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.OkCommand` (verified), command
- line 56: Button.Command = Binding `CancelCommand` -> `P:PressToShape.App.ViewModels.Editor.SettingsViewModel.CancelCommand` (verified), command

## Resource references

- line 13: Grid.Background = DynamicResource `WindowBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/WindowBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/WindowBackgroundBrush@Light` (verified)
- line 15: Border.Background = DynamicResource `ToolbarBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Light` (verified)
- line 15: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 16: Border.Height = DynamicResource `ToolbarHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarHeight` (verified)
- line 17: StackPanel.Margin = DynamicResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 17: StackPanel.Spacing = DynamicResource `PanelSpacing` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelSpacing` (verified)
- line 20: materialIcons:MaterialIcon.Height = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 20: materialIcons:MaterialIcon.Width = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 31: Button.Margin = DynamicResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 35: materialIcons:MaterialIcon.Height = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 35: materialIcons:MaterialIcon.Width = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 38: materialIcons:MaterialIcon.Foreground = DynamicResource `ErrorTextBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ErrorTextBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ErrorTextBrush@Light` (verified)
- line 39: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 39: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 52: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 53: Border.Background = DynamicResource `PanelBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Light` (verified)
- line 53: Border.Padding = DynamicResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 54: StackPanel.Spacing = DynamicResource `PanelSpacing` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelSpacing` (verified)

## Templates

- line 30: DataTemplate for `vm:SettingsCategoryViewModel` (`T:PressToShape.App.ViewModels.Editor.SettingsCategoryViewModel`)
