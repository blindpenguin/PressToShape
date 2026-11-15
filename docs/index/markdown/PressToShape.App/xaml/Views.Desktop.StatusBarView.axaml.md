# StatusBarView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Desktop/StatusBarView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Desktop.StatusBarView`.

## View model links

- `T:PressToShape.App.ViewModels.Desktop.StatusBarViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:49 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:49)
- `T:PressToShape.App.ViewModels.Desktop.StatusBarViewModel` (verified): x:DataType (src/PressToShape.App/Views/Desktop/StatusBarView.axaml:1)

## Bindings

- line 31: TextBlock.Text = Binding `SelectionSummary` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.SelectionSummary` (verified)
- line 33: TextBlock.IsVisible = Binding `ShowPathLength` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ShowPathLength` (verified)
- line 33: TextBlock.Text = Binding `PathLengthText` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.PathLengthText` (verified)
- line 36: Button.Command = Binding `ActivateHandToolCommand` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ActivateHandToolCommand` (verified), command
- line 40: Button.Command = Binding `ZoomOutCommand` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ZoomOutCommand` (verified), command
- line 45: DropDownButton.Content = Binding `ZoomText` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ZoomText` (verified)
- line 53: MenuFlyout.ItemsSource = Binding `ZoomPresets` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ZoomPresets` (verified)
- line 58: Setter.Header = Binding `Text` -> `P:PressToShape.App.ViewModels.Desktop.ZoomPresetViewModel.Text` (verified)
- line 59: Setter.Command = Binding `SelectCommand` -> `P:PressToShape.App.ViewModels.Desktop.ZoomPresetViewModel.SelectCommand` (verified), command
- line 66: Button.Command = Binding `ZoomInCommand` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ZoomInCommand` (verified), command
- line 70: Button.Command = Binding `FitToWindowCommand` -> `P:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.FitToWindowCommand` (verified), command

## Resource references

- line 14: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 24: Border.Background = DynamicResource `ToolbarBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Light` (verified)
- line 25: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 27: Border.Height = StaticResource `StatusBarHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/StatusBarHeight` (verified)
- line 56: ControlTheme.BasedOn = StaticResource `{x:Type MenuItem}` -> unresolved: not defined in this solution (may come from the Avalonia theme)

## Styles

- line 13: `TextBlock.status`, sets FontSize, Foreground, VerticalAlignment, uses TextSecondaryBrush
- line 18: `Button.icon.small`, sets MinHeight, MinWidth
