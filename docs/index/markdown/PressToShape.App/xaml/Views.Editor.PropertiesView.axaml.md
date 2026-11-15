# PropertiesView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.PropertiesView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.PropertiesViewModel` (inferred): ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.PropertiesViewModel maps to PressToShape.App.Views.Editor.PropertiesView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:26 (src/PressToShape.App/Views/Editor/InspectorView.axaml:26)
- `T:PressToShape.App.ViewModels.Editor.PropertiesViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/PropertiesView.axaml:1)

## Bindings

- line 38: TextBlock.Text = Binding `Header` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Header` (verified)
- line 44: controls:CollapsibleSection.IsExpanded = Binding `IsTransformExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsTransformExpanded` (verified)
- line 46: Grid.IsEnabled = Binding `HasSelection` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.HasSelection` (verified)
- line 48: TextBox.IsEnabled = Binding `X.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 48: TextBox.Tag = Binding `X` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.X` (verified)
- line 48: TextBox.Text = Binding `X.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 52: TextBox.IsEnabled = Binding `Y.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 52: TextBox.Tag = Binding `Y` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Y` (verified)
- line 52: TextBox.Text = Binding `Y.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 56: ToggleButton.IsChecked = Binding `KeepProportions` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.KeepProportions` (verified)
- line 62: TextBox.IsEnabled = Binding `Width.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 62: TextBox.Tag = Binding `Width` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Width` (verified)
- line 62: TextBox.Text = Binding `Width.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 66: TextBox.IsEnabled = Binding `Height.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 66: TextBox.Tag = Binding `Height` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Height` (verified)
- line 66: TextBox.Text = Binding `Height.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 71: TextBox.IsEnabled = Binding `Rotation.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 71: TextBox.Tag = Binding `Rotation` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Rotation` (verified)
- line 71: TextBox.Text = Binding `Rotation.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 75: Button.Command = Binding `ResetTransformCommand` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ResetTransformCommand` (verified), command
- line 82: controls:CollapsibleSection.IsExpanded = Binding `IsAppearanceExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsAppearanceExpanded` (verified)
- line 84: Grid.IsEnabled = Binding `HasSelection` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.HasSelection` (verified)
- line 86: Button.IsEnabled = Binding `Fill.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsEnabled` (verified)
- line 95: Border.Background = Binding `Fill.Color` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.Color` (verified), converter x:Static local:CoreColorBrushConverter.Instance
- line 96: Border.IsVisible = Binding `Fill.HasColor` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.HasColor` (verified)
- line 97: Border.IsVisible = Binding `!Fill.HasColor` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.HasColor` (verified)
- line 98: TextBlock.IsVisible = Binding `Fill.IsMixed` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsMixed` (verified)
- line 103: CheckBox.IsChecked = Binding `Fill.IsNone` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsNone` (verified)
- line 103: CheckBox.IsEnabled = Binding `Fill.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsEnabled` (verified)
- line 107: Button.IsEnabled = Binding `Stroke.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsEnabled` (verified)
- line 116: Border.Background = Binding `Stroke.Color` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.Color` (verified), converter x:Static local:CoreColorBrushConverter.Instance
- line 117: Border.IsVisible = Binding `Stroke.HasColor` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.HasColor` (verified)
- line 118: Border.IsVisible = Binding `!Stroke.HasColor` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.HasColor` (verified)
- line 119: TextBlock.IsVisible = Binding `Stroke.IsMixed` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsMixed` (verified)
- line 124: CheckBox.IsChecked = Binding `Stroke.IsNone` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsNone` (verified)
- line 124: CheckBox.IsEnabled = Binding `Stroke.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsEnabled` (verified)
- line 129: TextBox.IsEnabled = Binding `StrokeWidth.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 129: TextBox.Tag = Binding `StrokeWidth` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.StrokeWidth` (verified)
- line 129: TextBox.Text = Binding `StrokeWidth.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 152: OpacitySlider.IsEnabled = Binding `OpacityPercent.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 152: OpacitySlider.Value = Binding `OpacitySliderValue` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.OpacitySliderValue` (verified)
- line 155: TextBox.IsEnabled = Binding `OpacityPercent.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 155: TextBox.Tag = Binding `OpacityPercent` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.OpacityPercent` (verified)
- line 155: TextBox.Text = Binding `OpacityPercent.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 163: controls:CollapsibleSection.IsExpanded = Binding `IsPathExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsPathExpanded` (verified)
- line 168: controls:CollapsibleSection.IsExpanded = Binding `IsAlignExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsAlignExpanded` (verified)
- line 169: StackPanel.IsEnabled = Binding `HasSelection` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.HasSelection` (verified)
- line 195: controls:CollapsibleSection.IsExpanded = Binding `IsQuickActionsExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsQuickActionsExpanded` (verified)
- line 196: WrapPanel.IsEnabled = Binding `HasSelection` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.HasSelection` (verified)

## Resource references

- line 16: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 17: Setter.FontSize = StaticResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)
- line 31: Setter.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 97: Border.Background = DynamicResource `ControlBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Light` (verified)
- line 118: Border.Background = DynamicResource `ControlBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Light` (verified)
- line 182: Border.Background = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)

## Styles

- line 15: `TextBlock.label`, sets FontSize, Foreground, VerticalAlignment, uses TextSecondaryBrush, UiFontSize
- line 20: `Button.swatchButton`, sets Background, MinHeight, MinWidth, Padding
- line 26: `Border.swatch`, sets BorderBrush, BorderThickness, CornerRadius, Height, Width, uses BorderStrongBrush

## Event handlers

- line 89: Flyout.Closed = `OnFillFlyoutClosed` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillFlyoutClosed(System.Object,System.EventArgs)` (verified)
- line 89: Flyout.Opened = `OnFillFlyoutOpened` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillFlyoutOpened(System.Object,System.EventArgs)` (verified)
- line 91: FillColorView.ColorChanged = `OnFillColorChanged` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillColorChanged(System.Object,Avalonia.Controls.ColorChangedEventArgs)` (verified)
- line 104: CheckBox.Click = `OnFillNoneClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillNoneClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 110: Flyout.Closed = `OnStrokeFlyoutClosed` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeFlyoutClosed(System.Object,System.EventArgs)` (verified)
- line 110: Flyout.Opened = `OnStrokeFlyoutOpened` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeFlyoutOpened(System.Object,System.EventArgs)` (verified)
- line 112: StrokeColorView.ColorChanged = `OnStrokeColorChanged` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeColorChanged(System.Object,Avalonia.Controls.ColorChangedEventArgs)` (verified)
- line 125: CheckBox.Click = `OnStrokeNoneClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeNoneClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
