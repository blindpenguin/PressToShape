# TabletToolbarView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Tablet.TabletToolbarView`.

## View model links

- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:108)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:124)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:138)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:157)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:171)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:192)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:216)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:233)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:252)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:72)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:745)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:85)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:21 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:21)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel` (verified): x:DataType (src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml:1)

## Bindings

- line 31: Setter.Header = Binding `PlainHeader` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.PlainHeader` (verified)
- line 32: Setter.ItemsSource = Binding `Children` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Children` (verified)
- line 33: Setter.Command = Binding `Command` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.Command` (verified)
- line 34: Setter.IsEnabled = Binding `IsEnabled` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.IsEnabled` (verified)
- line 35: Setter.ToggleType = Binding `IsCheckable` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.IsCheckable` (verified), converter StaticResource CheckableToggleTypeConverter
- line 36: Setter.IsChecked = Binding `IsChecked` -> `P:PressToShape.App.ViewModels.Desktop.MenuItemViewModel.IsChecked` (verified)
- line 52: TextBlock.Text = Binding `DocumentTitle` -> `P:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel.DocumentTitle` (verified)
- line 56: Button.Command = Binding `UndoButton.Command` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Command` (verified)
- line 56: Button.IsEnabled = Binding `UndoButton.IsEnabled` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.IsEnabled` (verified)
- line 57: Button.AutomationProperties.Name = Binding `UndoButton.Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 57: Button.ToolTip.Tip = Binding `UndoButton.Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 58: materialIcons:MaterialIcon.Kind = Binding `UndoButton.Icon` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Icon` (verified)
- line 60: Button.Command = Binding `RedoButton.Command` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Command` (verified)
- line 60: Button.IsEnabled = Binding `RedoButton.IsEnabled` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.IsEnabled` (verified)
- line 61: Button.AutomationProperties.Name = Binding `RedoButton.Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 61: Button.ToolTip.Tip = Binding `RedoButton.Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 62: materialIcons:MaterialIcon.Kind = Binding `RedoButton.Icon` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Icon` (verified)
- line 66: Button.Command = Binding `DuplicateButton.Command` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Command` (verified)
- line 66: Button.IsEnabled = Binding `DuplicateButton.IsEnabled` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.IsEnabled` (verified)
- line 67: Button.AutomationProperties.Name = Binding `DuplicateButton.Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 67: Button.ToolTip.Tip = Binding `DuplicateButton.Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 68: materialIcons:MaterialIcon.Kind = Binding `DuplicateButton.Icon` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Icon` (verified)
- line 70: Button.Command = Binding `DeleteButton.Command` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Command` (verified)
- line 70: Button.IsEnabled = Binding `DeleteButton.IsEnabled` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.IsEnabled` (verified)
- line 71: Button.AutomationProperties.Name = Binding `DeleteButton.Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 71: Button.ToolTip.Tip = Binding `DeleteButton.Name` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Name` (verified)
- line 72: materialIcons:MaterialIcon.Kind = Binding `DeleteButton.Icon` -> `P:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.Icon` (verified)
- line 78: ItemsControl.ItemsSource = Binding `PanelButtons` -> `P:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel.PanelButtons` (verified)
- line 86: controls:ToolToggleButton.Command = Binding `ToggleCommand` -> `P:PressToShape.App.ViewModels.Tablet.PanelButtonViewModel.ToggleCommand` (verified), command
- line 86: controls:ToolToggleButton.IsChecked = Binding `IsActive` -> `P:PressToShape.App.ViewModels.Tablet.PanelButtonViewModel.IsActive` (verified)
- line 87: controls:ToolToggleButton.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Tablet.PanelButtonViewModel.Name` (verified)
- line 87: controls:ToolToggleButton.ToolTip.Tip = Binding `Name` -> `P:PressToShape.App.ViewModels.Tablet.PanelButtonViewModel.Name` (verified)
- line 88: materialIcons:MaterialIcon.Kind = Binding `Icon` -> `P:PressToShape.App.ViewModels.Tablet.PanelButtonViewModel.Icon` (verified)
- line 98: Button.Command = Binding `Zoom.ZoomOutCommand` -> `P:PressToShape.App.ViewModels.Editor.ZoomViewModel.ZoomOutCommand` (verified), command
- line 102: ZoomButton.Content = Binding `Zoom.ZoomText` -> `P:PressToShape.App.ViewModels.Editor.ZoomViewModel.ZoomText` (verified)
- line 106: MenuFlyout.ItemsSource = Binding `Zoom.ZoomPresets` -> `P:PressToShape.App.ViewModels.Editor.ZoomViewModel.ZoomPresets` (verified)
- line 111: Setter.Header = Binding `Text` -> `P:PressToShape.App.ViewModels.Editor.ZoomPresetViewModel.Text` (verified)
- line 112: Setter.Command = Binding `SelectCommand` -> `P:PressToShape.App.ViewModels.Editor.ZoomPresetViewModel.SelectCommand` (verified), command
- line 118: Button.Command = Binding `Zoom.ZoomInCommand` -> `P:PressToShape.App.ViewModels.Editor.ZoomViewModel.ZoomInCommand` (verified), command
- line 122: Button.Command = Binding `Zoom.FitToWindowCommand` -> `P:PressToShape.App.ViewModels.Editor.ZoomViewModel.FitToWindowCommand` (verified), command
- line 134: MenuFlyout.ItemsSource = Binding `OverflowMenu.Items` -> `P:PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel.Items` (verified)

## Resource references

- line 19: Setter.MaxHeight = DynamicResource `FlyoutMaxHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/FlyoutMaxHeight` (verified)
- line 29: ControlTheme.BasedOn = StaticResource `{x:Type MenuItem}` -> unresolved: not defined in this solution (may come from the Avalonia theme)
- line 37: Setter.ItemContainerTheme = DynamicResource `TabletMenuItemTheme` -> `R:src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml#/UserControl/UserControl.Resources[1]/TabletMenuItemTheme` (verified)
- line 41: Border.Background = DynamicResource `ToolbarBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Light` (verified)
- line 42: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 43: Border.MinHeight = DynamicResource `ToolbarHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarHeight` (verified)
- line 49: TextBlock.FontSize = DynamicResource `SectionHeaderFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SectionHeaderFontSize` (verified)
- line 51: TextBlock.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 58: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 58: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 62: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 62: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 68: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 68: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 72: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 72: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 75: Border.Background = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 88: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 88: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 94: Border.Background = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 100: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 100: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 103: ZoomButton.FontSize = DynamicResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)
- line 109: ControlTheme.BasedOn = StaticResource `{x:Type MenuItem}` -> unresolved: not defined in this solution (may come from the Avalonia theme)
- line 120: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 120: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 124: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 124: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 131: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 131: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 135: MenuFlyout.ItemContainerTheme = StaticResource `TabletMenuItemTheme` -> `R:src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml#/UserControl/UserControl.Resources[1]/TabletMenuItemTheme` (verified)

## Resources defined

- line 24: `CheckableToggleTypeConverter` (desktopViews:CheckableToggleTypeConverter)
- line 27: `TabletMenuItemTheme` (ControlTheme)

## Styles

- line 18: `MenuFlyoutPresenter`, sets MaxHeight, uses FlyoutMaxHeight

## Templates

- line 85: DataTemplate for `vm:PanelButtonViewModel` (`T:PressToShape.App.ViewModels.Tablet.PanelButtonViewModel`)
