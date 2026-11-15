# TabletColorSwatchView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Tablet.TabletColorSwatchView`.

## View model links

- `T:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml:82 (src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml:82)
- `T:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel` (verified): x:DataType (src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml:1)

## Bindings

- line 27: Button.Command = Binding `OpenCommand` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.OpenCommand` (verified), command
- line 28: Button.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.Name` (verified)
- line 28: Button.ToolTip.Tip = Binding `Name` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.Name` (verified)
- line 31: Border.Background = Binding `StrokeColor` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.StrokeColor` (verified), converter x:Static editor:CoreColorBrushConverter.Instance
- line 32: Border.IsVisible = Binding `HasStroke` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.HasStroke` (verified)
- line 34: Border.IsVisible = Binding `!HasStroke` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.HasStroke` (verified)
- line 39: Border.Background = Binding `FillColor` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.FillColor` (verified), converter x:Static editor:CoreColorBrushConverter.Instance
- line 40: Border.IsVisible = Binding `HasFill` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.HasFill` (verified)
- line 45: Border.IsVisible = Binding `!HasFill` -> `P:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.HasFill` (verified)

## Resource references

- line 14: Setter.Width = DynamicResource `TouchTargetSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TouchTargetSize` (verified)
- line 15: Setter.Height = DynamicResource `TouchTargetSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TouchTargetSize` (verified)
- line 16: Setter.MinWidth = DynamicResource `TouchTargetSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TouchTargetSize` (verified)
- line 17: Setter.MinHeight = DynamicResource `TouchTargetSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TouchTargetSize` (verified)
- line 29: Panel.Height = DynamicResource `ColorSwatchRingSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ColorSwatchRingSize` (verified)
- line 29: Panel.Width = DynamicResource `ColorSwatchRingSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ColorSwatchRingSize` (verified)
- line 30: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 33: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 36: Border.Height = DynamicResource `ColorSwatchFillSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ColorSwatchFillSize` (verified)
- line 36: Border.Width = DynamicResource `ColorSwatchFillSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ColorSwatchFillSize` (verified)
- line 38: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 41: Border.Height = DynamicResource `ColorSwatchFillSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ColorSwatchFillSize` (verified)
- line 41: Border.Width = DynamicResource `ColorSwatchFillSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ColorSwatchFillSize` (verified)
- line 43: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 44: Border.Background = DynamicResource `ControlBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Light` (verified)

## Styles

- line 13: `Button.colorSwatch`, sets Background, BorderThickness, Height, HorizontalContentAlignment, MinHeight, MinWidth, Padding, VerticalContentAlignment, Width, uses TouchTargetSize
