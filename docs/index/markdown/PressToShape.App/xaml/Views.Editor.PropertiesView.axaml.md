# PropertiesView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.PropertiesView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.PropertiesViewModel` (inferred): ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.PropertiesViewModel maps to PressToShape.App.Views.Editor.PropertiesView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:26 (src/PressToShape.App/Views/Editor/InspectorView.axaml:26)
- `T:PressToShape.App.ViewModels.Editor.PropertiesViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/PropertiesViewColorTests.cs:28)
- `T:PressToShape.App.ViewModels.Editor.PropertiesViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/PropertiesView.axaml:1)

## Bindings

- line 46: TextBlock.IsVisible = Binding `ShowHeader` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ShowHeader` (verified)
- line 46: TextBlock.Text = Binding `Header` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Header` (verified)
- line 49: TextBlock.IsVisible = Binding `ShowNoPropertiesHint` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ShowNoPropertiesHint` (verified)
- line 55: controls:CollapsibleSection.IsExpanded = Binding `IsTransformExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsTransformExpanded` (verified)
- line 55: controls:CollapsibleSection.IsVisible = Binding `ShowTransform` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ShowTransform` (verified)
- line 57: Grid.IsEnabled = Binding `HasSelection` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.HasSelection` (verified)
- line 59: TextBox.IsEnabled = Binding `X.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 59: TextBox.Tag = Binding `X` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.X` (verified)
- line 59: TextBox.Text = Binding `X.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 63: TextBox.IsEnabled = Binding `Y.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 63: TextBox.Tag = Binding `Y` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Y` (verified)
- line 63: TextBox.Text = Binding `Y.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 67: ToggleButton.IsChecked = Binding `KeepProportions` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.KeepProportions` (verified)
- line 73: TextBox.IsEnabled = Binding `Width.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 73: TextBox.Tag = Binding `Width` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Width` (verified)
- line 73: TextBox.Text = Binding `Width.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 77: TextBox.IsEnabled = Binding `Height.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 77: TextBox.Tag = Binding `Height` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Height` (verified)
- line 77: TextBox.Text = Binding `Height.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 82: TextBox.IsEnabled = Binding `Rotation.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 82: TextBox.Tag = Binding `Rotation` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Rotation` (verified)
- line 82: TextBox.Text = Binding `Rotation.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 86: Button.Command = Binding `ResetTransformCommand` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ResetTransformCommand` (verified), command
- line 93: controls:CollapsibleSection.IsExpanded = Binding `IsAppearanceExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsAppearanceExpanded` (verified)
- line 93: controls:CollapsibleSection.IsVisible = Binding `ShowAppearance` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ShowAppearance` (verified)
- line 96: FillSwatchButton.IsEnabled = Binding `Fill.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsEnabled` (verified)
- line 108: ItemsControl.ItemsSource = Binding `RecentColors` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.RecentColors` (verified)
- line 115: Border.Background = Binding `.` -> unresolved: x:DataType coreShapes:Color, converter x:Static local:CoreColorBrushConverter.Instance
- line 124: Border.Background = Binding `Fill.Color` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.Color` (verified), converter x:Static local:CoreColorBrushConverter.Instance
- line 125: Border.IsVisible = Binding `Fill.HasColor` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.HasColor` (verified)
- line 126: Border.IsVisible = Binding `!Fill.HasColor` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.HasColor` (verified)
- line 127: TextBlock.IsVisible = Binding `Fill.IsMixed` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsMixed` (verified)
- line 132: CheckBox.IsChecked = Binding `Fill.IsNone` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsNone` (verified)
- line 132: CheckBox.IsEnabled = Binding `Fill.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsEnabled` (verified)
- line 136: StrokeSwatchButton.IsEnabled = Binding `Stroke.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsEnabled` (verified)
- line 148: ItemsControl.ItemsSource = Binding `RecentColors` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.RecentColors` (verified)
- line 155: Border.Background = Binding `.` -> unresolved: x:DataType coreShapes:Color, converter x:Static local:CoreColorBrushConverter.Instance
- line 164: Border.Background = Binding `Stroke.Color` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.Color` (verified), converter x:Static local:CoreColorBrushConverter.Instance
- line 165: Border.IsVisible = Binding `Stroke.HasColor` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.HasColor` (verified)
- line 166: Border.IsVisible = Binding `!Stroke.HasColor` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.HasColor` (verified)
- line 167: TextBlock.IsVisible = Binding `Stroke.IsMixed` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsMixed` (verified)
- line 172: CheckBox.IsChecked = Binding `Stroke.IsNone` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsNone` (verified)
- line 172: CheckBox.IsEnabled = Binding `Stroke.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.ColorFieldViewModel.IsEnabled` (verified)
- line 177: TextBox.IsEnabled = Binding `StrokeWidth.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 177: TextBox.Tag = Binding `StrokeWidth` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.StrokeWidth` (verified)
- line 177: TextBox.Text = Binding `StrokeWidth.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 199: PaletteStrip.ItemsSource = Binding `Palette.Swatches` -> `P:PressToShape.App.ViewModels.Editor.PaletteViewModel.Swatches` (verified)
- line 211: Border.Background = Binding `.` -> unresolved: x:DataType coreShapes:Color, converter x:Static local:CoreColorBrushConverter.Instance
- line 216: TextBlock.IsVisible = Binding `Palette.IsEmpty` -> `P:PressToShape.App.ViewModels.Editor.PaletteViewModel.IsEmpty` (verified)
- line 222: OpacitySlider.IsEnabled = Binding `OpacityPercent.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 222: OpacitySlider.Value = Binding `OpacitySliderValue` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.OpacitySliderValue` (verified)
- line 225: TextBox.IsEnabled = Binding `OpacityPercent.IsEnabled` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.IsEnabled` (verified)
- line 225: TextBox.Tag = Binding `OpacityPercent` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.OpacityPercent` (verified)
- line 225: TextBox.Text = Binding `OpacityPercent.Text` -> `P:PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Text` (verified)
- line 233: controls:CollapsibleSection.IsExpanded = Binding `IsPathExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsPathExpanded` (verified)
- line 233: controls:CollapsibleSection.IsVisible = Binding `ShowPath` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ShowPath` (verified)
- line 238: controls:CollapsibleSection.IsExpanded = Binding `IsAlignExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsAlignExpanded` (verified)
- line 238: controls:CollapsibleSection.IsVisible = Binding `ShowAlign` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ShowAlign` (verified)
- line 239: StackPanel.IsEnabled = Binding `HasSelection` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.HasSelection` (verified)
- line 265: controls:CollapsibleSection.IsExpanded = Binding `IsQuickActionsExpanded` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.IsQuickActionsExpanded` (verified)
- line 265: controls:CollapsibleSection.IsVisible = Binding `ShowQuickActions` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ShowQuickActions` (verified)
- line 266: WrapPanel.IsEnabled = Binding `HasSelection` -> `P:PressToShape.App.ViewModels.Editor.PropertiesViewModel.HasSelection` (verified)

## Resource references

- line 17: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 18: Setter.FontSize = StaticResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)
- line 32: Setter.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 39: Setter.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 126: Border.Background = DynamicResource `ControlBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Light` (verified)
- line 166: Border.Background = DynamicResource `ControlBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Light` (verified)
- line 252: Border.Background = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)

## Styles

- line 16: `TextBlock.label`, sets FontSize, Foreground, VerticalAlignment, uses TextSecondaryBrush, UiFontSize
- line 21: `Button.swatchButton`, sets Background, MinHeight, MinWidth, Padding
- line 27: `Border.smallSwatch`, sets BorderBrush, BorderThickness, CornerRadius, Height, Width, uses BorderStrongBrush
- line 34: `Border.swatch`, sets BorderBrush, BorderThickness, CornerRadius, Height, Width, uses BorderStrongBrush

## Templates

- line 113: DataTemplate for `coreShapes:Color` (`T:PressToShape.Core.Shapes.Color`)
- line 153: DataTemplate for `coreShapes:Color` (`T:PressToShape.Core.Shapes.Color`)
- line 204: DataTemplate for `coreShapes:Color` (`T:PressToShape.Core.Shapes.Color`)

## Event handlers

- line 99: Flyout.Closed = `OnFillFlyoutClosed` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillFlyoutClosed(System.Object,System.EventArgs)` (verified)
- line 99: Flyout.Opened = `OnFillFlyoutOpened` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillFlyoutOpened(System.Object,System.EventArgs)` (verified)
- line 102: FillColorView.ColorChanged = `OnFillColorChanged` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillColorChanged(System.Object,Avalonia.Controls.ColorChangedEventArgs)` (verified)
- line 104: Button.Click = `OnFillAddToPaletteClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillAddToPaletteClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 105: Button.Click = `OnFillPickClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillPickClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 114: Button.Click = `OnFillRecentClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillRecentClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 133: CheckBox.Click = `OnFillNoneClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnFillNoneClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 139: Flyout.Closed = `OnStrokeFlyoutClosed` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeFlyoutClosed(System.Object,System.EventArgs)` (verified)
- line 139: Flyout.Opened = `OnStrokeFlyoutOpened` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeFlyoutOpened(System.Object,System.EventArgs)` (verified)
- line 142: StrokeColorView.ColorChanged = `OnStrokeColorChanged` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeColorChanged(System.Object,Avalonia.Controls.ColorChangedEventArgs)` (verified)
- line 144: Button.Click = `OnStrokeAddToPaletteClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeAddToPaletteClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 145: Button.Click = `OnStrokePickClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokePickClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 154: Button.Click = `OnStrokeRecentClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeRecentClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 173: CheckBox.Click = `OnStrokeNoneClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnStrokeNoneClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 205: Button.Click = `OnSwatchClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnSwatchClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
- line 208: MenuItem.Click = `OnRemoveSwatchClick` -> `M:PressToShape.App.Views.Editor.PropertiesView.OnRemoveSwatchClick(System.Object,Avalonia.Interactivity.RoutedEventArgs)` (verified)
