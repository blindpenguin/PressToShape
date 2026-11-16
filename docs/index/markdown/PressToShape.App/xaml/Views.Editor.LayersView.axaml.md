# LayersView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/LayersView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.LayersView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (inferred): ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.LayersViewModel maps to PressToShape.App.Views.Editor.LayersView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:29 (src/PressToShape.App/Views/Editor/InspectorView.axaml:29)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/LayersViewTests.cs:42)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:244)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/LayersView.axaml:1)

## Bindings

- line 55: Button.Command = Binding `AddLayerCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.AddLayerCommand` (verified), command
- line 62: RowList.ItemsSource = Binding `Rows` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.Rows` (verified)
- line 71: Border.Classes.selected = Binding `IsSelected` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsSelected` (verified)
- line 72: Border.Classes.current = Binding `IsCurrent` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsCurrent` (verified)
- line 73: Border.Classes.activeLayer = Binding `IsActiveLayer` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsActiveLayer` (verified)
- line 74: Border.Classes.drop = Binding `IsDropTarget` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsDropTarget` (verified)
- line 75: Border.Classes.inactive = Binding `IsEffectivelyHidden` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsEffectivelyHidden` (verified)
- line 77: Border.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Name` (verified)
- line 79: Border.Width = Binding `Depth` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Depth` (verified), converter StaticResource IndentConverter
- line 81: Button.Command = Binding `ToggleExpandedCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleExpandedCommand` (verified), command
- line 81: Button.IsVisible = Binding `HasChildren` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.HasChildren` (verified)
- line 84: materialIcons:MaterialIcon.IsVisible = Binding `IsExpanded` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsExpanded` (verified)
- line 85: materialIcons:MaterialIcon.IsVisible = Binding `!IsExpanded` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsExpanded` (verified)
- line 90: materialIcons:MaterialIcon.IsVisible = Binding `IsLayer` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLayer` (verified)
- line 91: materialIcons:MaterialIcon.IsVisible = Binding `IsRectangle` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRectangle` (verified)
- line 92: materialIcons:MaterialIcon.IsVisible = Binding `IsEllipse` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsEllipse` (verified)
- line 93: materialIcons:MaterialIcon.IsVisible = Binding `IsGroup` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsGroup` (verified)
- line 96: TextBlock.IsVisible = Binding `!IsRenaming` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRenaming` (verified)
- line 96: TextBlock.Text = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Name` (verified)
- line 97: TextBox.IsVisible = Binding `IsRenaming` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRenaming` (verified)
- line 97: TextBox.Text = Binding `EditText` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.EditText` (verified)
- line 100: KeyBinding.Command = Binding `CommitRenameCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CommitRenameCommand` (verified), command
- line 101: KeyBinding.Command = Binding `CancelRenameCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CancelRenameCommand` (verified), command
- line 105: Button.Command = Binding `ToggleLockCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleLockCommand` (verified), command
- line 108: materialIcons:MaterialIcon.IsVisible = Binding `IsLocked` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLocked` (verified)
- line 109: materialIcons:MaterialIcon.IsVisible = Binding `!IsLocked` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLocked` (verified)
- line 112: Button.Command = Binding `ToggleVisibilityCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleVisibilityCommand` (verified), command
- line 115: materialIcons:MaterialIcon.IsVisible = Binding `IsVisible` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsVisible` (verified)
- line 116: materialIcons:MaterialIcon.IsVisible = Binding `!IsVisible` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsVisible` (verified)

## Resource references

- line 20: Setter.MinWidth = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 21: Setter.MinHeight = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 29: Setter.Background = DynamicResource `ControlHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Light` (verified)
- line 33: Setter.Background = DynamicResource `ControlPressedBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlPressedBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlPressedBrush@Light` (verified)
- line 36: Setter.Background = DynamicResource `SelectedItemBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectedItemBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectedItemBrush@Light` (verified)
- line 43: Setter.BorderBrush = DynamicResource `AppAccentBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Light` (verified)
- line 46: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 50: DockPanel.Margin = StaticResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 57: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 57: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 76: Border.MinHeight = DynamicResource `LayerRowMinHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/LayerRowMinHeight` (verified)
- line 80: Panel.MinWidth = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 84: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 84: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 85: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 85: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 90: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 90: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 91: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 91: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 92: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 92: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 93: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 93: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 108: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 108: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 109: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 109: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 115: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 115: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 116: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 116: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)

## Resources defined

- line 14: `IndentConverter` (views:ObjectIndentConverter)

## Styles

- line 19: `Button.rowButton`, sets MinHeight, MinWidth, uses RowButtonSize
- line 23: `Border.objectRow`, sets Background, BorderBrush, BorderThickness
- line 28: `Border.objectRow:pointerover`, sets Background, uses ControlHoverBrush
- line 32: `Border.objectRow.current`, sets Background, uses ControlPressedBrush
- line 35: `Border.objectRow.selected`, sets Background, uses SelectedItemBrush
- line 39: `Border.objectRow.activeLayer TextBlock`, sets FontWeight
- line 42: `Border.objectRow.drop`, sets BorderBrush, uses AppAccentBrush
- line 45: `Border.objectRow.inactive TextBlock`, sets Foreground, uses TextSecondaryBrush

## Templates

- line 69: DataTemplate for `vm:ObjectRowViewModel` (`T:PressToShape.App.ViewModels.Editor.ObjectRowViewModel`)
