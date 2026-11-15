# TabletToolColumnView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Tablet.TabletToolColumnView`.

## View model links

- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:588)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:604)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:620)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:636)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:649)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:667)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:688)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:706)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:725)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:765)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:779)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:797)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:813)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:828)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:858)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:883)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:902)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:24 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:24)
- `T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel` (verified): x:DataType (src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml:1)

## Bindings

- line 33: GroupButton.IsChecked = Binding `IsActive` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.IsActive` (verified)
- line 34: GroupButton.Command = Binding `SelectCurrentCommand` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.SelectCurrentCommand` (verified), command
- line 35: GroupButton.LongPressCommand = Binding `OpenFlyoutCommand` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.OpenFlyoutCommand` (verified), command
- line 36: GroupButton.ToolTip.Tip = Binding `ToolTip` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.ToolTip` (verified)
- line 37: GroupButton.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.Name` (verified)
- line 39: materialIcons:MaterialIcon.Kind = Binding `Icon` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.Icon` (verified)
- line 42: Path.IsVisible = Binding `IsGroup` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.IsGroup` (verified)
- line 48: FlyoutPopup.IsOpen = Binding `IsFlyoutOpen` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.IsFlyoutOpen` (verified)
- line 49: FlyoutPopup.PlacementTarget = Binding `#GroupButton` -> unresolved: ElementName GroupButton, source ElementName=GroupButton
- line 54: ItemsControl.ItemsSource = Binding `Members` -> `P:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.Members` (verified)
- line 58: controls:ToolToggleButton.IsChecked = Binding `IsActive` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.IsActive` (verified)
- line 59: controls:ToolToggleButton.Command = Binding `$parent[Popup].((vm:ToolGroupViewModel)DataContext).SelectMemberCommand` -> unresolved: at the start the path has an attached property or cast ((vm:ToolGroupViewModel)DataContext).SelectMemberCommand, which is not modelled, source $parent[Popup]
- line 60: controls:ToolToggleButton.CommandParameter = Binding `` -> unresolved: x:DataType editorVm:ToolItemViewModel
- line 61: controls:ToolToggleButton.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.Name` (verified)
- line 63: materialIcons:MaterialIcon.Kind = Binding `Icon` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.Icon` (verified)
- line 64: TextBlock.Text = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ToolItemViewModel.Name` (verified)
- line 82: local:TabletColorSwatchView.DataContext = Binding `ColorSwatch` -> `P:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel.ColorSwatch` (verified)
- line 85: ItemsControl.ItemsSource = Binding `Groups` -> `P:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel.Groups` (verified)

## Resource references

- line 19: Setter.Height = DynamicResource `ToolFlyoutItemHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolFlyoutItemHeight` (verified)
- line 20: Setter.MinHeight = DynamicResource `ToolFlyoutItemHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolFlyoutItemHeight` (verified)
- line 38: GroupButton.Margin = DynamicResource `ToolButtonMargin` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolButtonMargin` (verified)
- line 39: materialIcons:MaterialIcon.Height = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 39: materialIcons:MaterialIcon.Width = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 44: Path.Height = DynamicResource `ToolGroupMarkSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolGroupMarkSize` (verified)
- line 44: Path.Width = DynamicResource `ToolGroupMarkSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolGroupMarkSize` (verified)
- line 46: Path.Fill = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 51: Border.Background = DynamicResource `PanelBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Light` (verified)
- line 52: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 53: Border.CornerRadius = StaticResource `ControlCornerRadius` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlCornerRadius` (verified)
- line 63: materialIcons:MaterialIcon.Height = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 63: materialIcons:MaterialIcon.Width = DynamicResource `ToolIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolIconSize` (verified)
- line 64: TextBlock.FontSize = DynamicResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)
- line 76: Border.Background = DynamicResource `ToolColumnBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolColumnBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolColumnBackgroundBrush@Light` (verified)
- line 77: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 79: Border.Padding = DynamicResource `ToolColumnPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolColumnPadding` (verified)
- line 85: ItemsControl.ItemTemplate = StaticResource `ToolGroupTemplate` -> `R:src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml#/UserControl/UserControl.Resources[1]/ToolGroupTemplate` (verified)

## Resources defined

- line 29: `ToolGroupTemplate` (DataTemplate)

## Styles

- line 16: `ToggleButton.tool.flyoutTool`, sets Height, HorizontalAlignment, HorizontalContentAlignment, MinHeight, MinWidth, Padding, Width, uses ToolFlyoutItemHeight

## Templates

- line 29: DataTemplate `ToolGroupTemplate` for `vm:ToolGroupViewModel` (`T:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel`)
- line 56: DataTemplate for `editorVm:ToolItemViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolItemViewModel`)
