# StatusBarView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Desktop/StatusBarView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Desktop.StatusBarView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs:216)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:49 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:49)
- `T:PressToShape.App.ViewModels.Editor.StatusBarViewModel` (verified): x:DataType (src/PressToShape.App/Views/Desktop/StatusBarView.axaml:1)

## Bindings

- line 32: TextBlock.Text = Binding `SelectionSummary` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.SelectionSummary` (verified)
- line 34: TextBlock.IsVisible = Binding `ShowPathLength` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ShowPathLength` (verified)
- line 34: TextBlock.Text = Binding `PathLengthText` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.PathLengthText` (verified)
- line 36: TextBlock.IsVisible = Binding `IsBusy` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.IsBusy` (verified)
- line 36: TextBlock.Text = Binding `BusyText` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.BusyText` (verified)
- line 39: Button.Command = Binding `ActivateHandToolCommand` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ActivateHandToolCommand` (verified), command
- line 43: Button.Command = Binding `ZoomOutCommand` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ZoomOutCommand` (verified), command
- line 48: DropDownButton.Content = Binding `ZoomText` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ZoomText` (verified)
- line 56: MenuFlyout.ItemsSource = Binding `ZoomPresets` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ZoomPresets` (verified)
- line 61: Setter.Header = Binding `Text` -> `P:PressToShape.App.ViewModels.Editor.ZoomPresetViewModel.Text` (verified)
- line 62: Setter.Command = Binding `SelectCommand` -> `P:PressToShape.App.ViewModels.Editor.ZoomPresetViewModel.SelectCommand` (verified), command
- line 69: Button.Command = Binding `ZoomInCommand` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.ZoomInCommand` (verified), command
- line 73: Button.Command = Binding `FitToWindowCommand` -> `P:PressToShape.App.ViewModels.Editor.StatusBarViewModel.FitToWindowCommand` (verified), command

## Resource references

- line 15: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 25: Border.Background = DynamicResource `ToolbarBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Light` (verified)
- line 26: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 28: Border.Height = StaticResource `StatusBarHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/StatusBarHeight` (verified)
- line 59: ControlTheme.BasedOn = StaticResource `{x:Type MenuItem}` -> unresolved: not defined in this solution (may come from the Avalonia theme)

## Styles

- line 14: `TextBlock.status`, sets FontSize, Foreground, VerticalAlignment, uses TextSecondaryBrush
- line 19: `Button.icon.small`, sets MinHeight, MinWidth
