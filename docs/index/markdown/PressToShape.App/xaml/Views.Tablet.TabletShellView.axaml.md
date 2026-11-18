# TabletShellView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Tablet/TabletShellView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Tablet.TabletShellView`.

## View model links

- `T:PressToShape.App.ViewModels.Tablet.TabletShellViewModel` (verified): DataContext assignment (src/PressToShape.App/AppLayout.cs:75)
- `T:PressToShape.App.ViewModels.Tablet.TabletShellViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/CanvasAreaViewSnapTests.cs:108)
- `T:PressToShape.App.ViewModels.Tablet.TabletShellViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs:86)
- `T:PressToShape.App.ViewModels.Tablet.TabletShellViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:366)
- `T:PressToShape.App.ViewModels.Tablet.TabletShellViewModel` (verified): x:DataType (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:1)

## Bindings

- line 21: ToolbarView.DataContext = Binding `Toolbar` -> `P:PressToShape.App.ViewModels.Tablet.TabletShellViewModel.Toolbar` (verified)
- line 24: tablet:TabletToolColumnView.DataContext = Binding `Column` -> `P:PressToShape.App.ViewModels.Tablet.TabletShellViewModel.Column` (verified)
- line 28: CanvasArea.DataContext = Binding `Canvas` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Canvas` (verified)
- line 29: tablet:TabletPanelView.DataContext = Binding `Panel` -> `P:PressToShape.App.ViewModels.Tablet.TabletShellViewModel.Panel` (verified)
- line 34: StatusBarView.DataContext = Binding `StatusBar` -> `P:PressToShape.App.ViewModels.Tablet.TabletShellViewModel.StatusBar` (verified)
- line 37: Panel.IsVisible = Binding `SettingsHost.IsOpen` -> `P:PressToShape.App.ViewModels.Tablet.SettingsDialogHost.IsOpen` (verified)
- line 38: tablet:TabletSettingsView.DataContext = Binding `SettingsHost.Current` -> `P:PressToShape.App.ViewModels.Tablet.SettingsDialogHost.Current` (verified)
- line 44: DialogScrim.IsVisible = Binding `Dialogs.IsOpen` -> `P:PressToShape.App.ViewModels.Tablet.MessageDialogHost.IsOpen` (verified)
- line 53: TextBlock.Text = Binding `Dialogs.Current.Title` -> `P:PressToShape.App.ViewModels.MessageDialogViewModel.Title` (verified)
- line 54: views:MessageDialogView.DataContext = Binding `Dialogs.Current` -> `P:PressToShape.App.ViewModels.Tablet.MessageDialogHost.Current` (verified)

## Resource references

- line 16: Panel.Background = DynamicResource `WindowBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/WindowBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/WindowBackgroundBrush@Light` (verified)
- line 44: DialogScrim.Background = DynamicResource `ScrimBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ScrimBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ScrimBrush@Light` (verified)
- line 46: Border.Background = DynamicResource `PanelBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Light` (verified)
- line 47: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 48: Border.CornerRadius = StaticResource `ControlCornerRadius` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlCornerRadius` (verified)
- line 51: TextBlock.FontSize = DynamicResource `SectionHeaderFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SectionHeaderFontSize` (verified)
- line 52: TextBlock.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
