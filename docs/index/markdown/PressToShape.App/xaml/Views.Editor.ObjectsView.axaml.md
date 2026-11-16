# ObjectsView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/ObjectsView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.ObjectsView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.ObjectsViewModel` (inferred): ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.ObjectsViewModel maps to PressToShape.App.Views.Editor.ObjectsView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:32 (src/PressToShape.App/Views/Editor/InspectorView.axaml:32)
- `T:PressToShape.App.ViewModels.Editor.ObjectsViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/ObjectsViewTests.cs:42)
- `T:PressToShape.App.ViewModels.Editor.ObjectsViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/ObjectsView.axaml:1)

## Bindings

- line 51: RowList.ItemsSource = Binding `Rows` -> `P:PressToShape.App.ViewModels.Editor.ObjectsViewModel.Rows` (verified)
- line 60: Border.Classes.selected = Binding `IsSelected` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsSelected` (verified)
- line 61: Border.Classes.current = Binding `IsCurrent` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsCurrent` (verified)
- line 62: Border.Classes.drop = Binding `IsDropTarget` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsDropTarget` (verified)
- line 63: Border.Classes.inactive = Binding `IsEffectivelyHidden` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsEffectivelyHidden` (verified)
- line 65: Border.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Name` (verified)
- line 67: Border.Width = Binding `Depth` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Depth` (verified), converter StaticResource IndentConverter
- line 69: Button.Command = Binding `ToggleExpandedCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleExpandedCommand` (verified), command
- line 69: Button.IsVisible = Binding `HasChildren` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.HasChildren` (verified)
- line 72: materialIcons:MaterialIcon.IsVisible = Binding `IsExpanded` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsExpanded` (verified)
- line 73: materialIcons:MaterialIcon.IsVisible = Binding `!IsExpanded` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsExpanded` (verified)
- line 78: materialIcons:MaterialIcon.IsVisible = Binding `IsLayer` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLayer` (verified)
- line 79: materialIcons:MaterialIcon.IsVisible = Binding `IsRectangle` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRectangle` (verified)
- line 80: materialIcons:MaterialIcon.IsVisible = Binding `IsEllipse` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsEllipse` (verified)
- line 81: materialIcons:MaterialIcon.IsVisible = Binding `IsGroup` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsGroup` (verified)
- line 84: TextBlock.IsVisible = Binding `!IsRenaming` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRenaming` (verified)
- line 84: TextBlock.Text = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Name` (verified)
- line 85: TextBox.IsVisible = Binding `IsRenaming` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRenaming` (verified)
- line 85: TextBox.Text = Binding `EditText` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.EditText` (verified)
- line 88: KeyBinding.Command = Binding `CommitRenameCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CommitRenameCommand` (verified), command
- line 89: KeyBinding.Command = Binding `CancelRenameCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CancelRenameCommand` (verified), command
- line 93: Button.Command = Binding `ToggleLockCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleLockCommand` (verified), command
- line 93: Button.IsVisible = Binding `HasToggles` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.HasToggles` (verified)
- line 96: materialIcons:MaterialIcon.IsVisible = Binding `IsLocked` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLocked` (verified)
- line 97: materialIcons:MaterialIcon.IsVisible = Binding `!IsLocked` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLocked` (verified)
- line 100: Button.Command = Binding `ToggleVisibilityCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleVisibilityCommand` (verified), command
- line 100: Button.IsVisible = Binding `HasToggles` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.HasToggles` (verified)
- line 103: materialIcons:MaterialIcon.IsVisible = Binding `IsVisible` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsVisible` (verified)
- line 104: materialIcons:MaterialIcon.IsVisible = Binding `!IsVisible` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsVisible` (verified)

## Resource references

- line 20: Setter.MinWidth = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 21: Setter.MinHeight = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 29: Setter.Background = DynamicResource `ControlHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Light` (verified)
- line 33: Setter.Background = DynamicResource `ControlPressedBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlPressedBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlPressedBrush@Light` (verified)
- line 36: Setter.Background = DynamicResource `SelectedItemBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectedItemBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectedItemBrush@Light` (verified)
- line 39: Setter.BorderBrush = DynamicResource `AppAccentBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Light` (verified)
- line 42: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 46: DockPanel.Margin = StaticResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 64: Border.MinHeight = DynamicResource `LayerRowMinHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/LayerRowMinHeight` (verified)
- line 68: Panel.MinWidth = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 72: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 72: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 73: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 73: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 78: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 78: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 79: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 79: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 80: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 80: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 81: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 81: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 96: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 96: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 97: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 97: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 103: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 103: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 104: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 104: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)

## Resources defined

- line 14: `IndentConverter` (views:ObjectIndentConverter)

## Styles

- line 19: `Button.rowButton`, sets MinHeight, MinWidth, uses RowButtonSize
- line 23: `Border.objectRow`, sets Background, BorderBrush, BorderThickness
- line 28: `Border.objectRow:pointerover`, sets Background, uses ControlHoverBrush
- line 32: `Border.objectRow.current`, sets Background, uses ControlPressedBrush
- line 35: `Border.objectRow.selected`, sets Background, uses SelectedItemBrush
- line 38: `Border.objectRow.drop`, sets BorderBrush, uses AppAccentBrush
- line 41: `Border.objectRow.inactive TextBlock`, sets Foreground, uses TextSecondaryBrush

## Templates

- line 58: DataTemplate for `vm:ObjectRowViewModel` (`T:PressToShape.App.ViewModels.Editor.ObjectRowViewModel`)
