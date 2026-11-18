# TabletStatusBarView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Tablet/TabletStatusBarView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Tablet.TabletStatusBarView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/StatusBarSnapViewTests.cs:24)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs:109)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs:134)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs:150)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs:168)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs:184)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs:216)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:34 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:34)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): x:DataType (src/PressToShape.App/Views/Tablet/TabletStatusBarView.axaml:1)

## Bindings

- line 23: TextBlock.Text = Binding `SelectionSummary` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.SelectionSummary` (verified)
- line 26: TextBlock.IsVisible = Binding `IsBusy` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.IsBusy` (verified)
- line 26: TextBlock.Text = Binding `BusyText` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.BusyText` (verified)
- line 29: ToggleButton.IsChecked = Binding `ShowGrid` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ShowGrid` (verified)
- line 34: ToggleButton.IsChecked = Binding `SnapToGrid` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.SnapToGrid` (verified)
- line 39: ToggleButton.IsChecked = Binding `SnapToObjects` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.SnapToObjects` (verified)
- line 45: GridSpacingBox.IsEnabled = Binding `GridSpacingField.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 45: GridSpacingBox.Tag = Binding `GridSpacingField` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.GridSpacingField` (verified)
- line 45: GridSpacingBox.Text = Binding `GridSpacingField.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 51: Button.Command = Binding `ZoomOutCommand` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ZoomOutCommand` (verified), command
- line 55: ZoomButton.Content = Binding `ZoomText` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ZoomText` (verified)
- line 60: MenuFlyout.ItemsSource = Binding `ZoomPresets` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ZoomPresets` (verified)
- line 65: Setter.Header = Binding `Text` -> `P:PressToShape.App.ViewModels.Editor.ZoomPresetViewModel.Text` (verified)
- line 66: Setter.Command = Binding `SelectCommand` -> `P:PressToShape.App.ViewModels.Editor.ZoomPresetViewModel.SelectCommand` (verified), command
- line 72: Button.Command = Binding `ZoomInCommand` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ZoomInCommand` (verified), command
- line 76: Button.Command = Binding `FitToWindowCommand` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.FitToWindowCommand` (verified), command

## Resource references

- line 14: Border.Background = DynamicResource `ToolbarBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Light` (verified)
- line 15: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 17: Border.Height = DynamicResource `StatusBarHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/StatusBarHeight` (verified)
- line 22: TextBlock.FontSize = DynamicResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)
- line 22: TextBlock.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 25: TextBlock.FontSize = DynamicResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)
- line 25: TextBlock.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 30: ToggleButton.MinHeight = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 30: ToggleButton.MinWidth = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 32: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 32: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 35: ToggleButton.MinHeight = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 35: ToggleButton.MinWidth = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 37: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 37: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 40: ToggleButton.MinHeight = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 40: ToggleButton.MinWidth = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 42: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 42: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 53: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 53: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 56: ZoomButton.MinHeight = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 57: ZoomButton.FontSize = DynamicResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)
- line 63: ControlTheme.BasedOn = StaticResource `{x:Type MenuItem}` -> unresolved: not defined in this solution (may come from the Avalonia theme)
- line 74: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 74: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 78: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 78: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
