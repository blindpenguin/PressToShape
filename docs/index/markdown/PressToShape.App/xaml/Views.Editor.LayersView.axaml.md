# LayersView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/LayersView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.LayersView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (inferred): ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.LayersViewModel maps to PressToShape.App.Views.Editor.LayersView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:29 (src/PressToShape.App/Views/Editor/InspectorView.axaml:29)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/LayersViewLayerTests.cs:35)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/LayersViewTests.cs:42)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:244)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/LayersView.axaml:1)

## Bindings

- line 56: Button.Command = Binding `AddLayerCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.AddLayerCommand` (verified), command
- line 60: Button.Command = Binding `DuplicateLayerCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.DuplicateLayerCommand` (verified), command
- line 64: Button.Command = Binding `DeleteLayerCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.DeleteLayerCommand` (verified), command
- line 68: Button.Command = Binding `MoveLayerUpCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.MoveLayerUpCommand` (verified), command
- line 72: Button.Command = Binding `MoveLayerDownCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.MoveLayerDownCommand` (verified), command
- line 76: Button.Command = Binding `MergeLayerDownCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.MergeLayerDownCommand` (verified), command
- line 84: RowList.ItemsSource = Binding `Rows` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.Rows` (verified)
- line 93: Border.Classes.selected = Binding `IsSelected` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsSelected` (verified)
- line 94: Border.Classes.current = Binding `IsCurrent` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsCurrent` (verified)
- line 95: Border.Classes.activeLayer = Binding `IsActiveLayer` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsActiveLayer` (verified)
- line 96: Border.Classes.drop = Binding `IsDropTarget` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsDropTarget` (verified)
- line 97: Border.Classes.inactive = Binding `IsEffectivelyHidden` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsEffectivelyHidden` (verified)
- line 99: Border.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Name` (verified)
- line 101: Border.Width = Binding `Depth` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Depth` (verified), converter StaticResource IndentConverter
- line 103: Button.Command = Binding `ToggleExpandedCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleExpandedCommand` (verified), command
- line 103: Button.IsVisible = Binding `HasChildren` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.HasChildren` (verified)
- line 106: materialIcons:MaterialIcon.IsVisible = Binding `IsExpanded` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsExpanded` (verified)
- line 107: materialIcons:MaterialIcon.IsVisible = Binding `!IsExpanded` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsExpanded` (verified)
- line 112: materialIcons:MaterialIcon.IsVisible = Binding `IsLayer` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLayer` (verified)
- line 113: materialIcons:MaterialIcon.IsVisible = Binding `IsRectangle` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRectangle` (verified)
- line 114: materialIcons:MaterialIcon.IsVisible = Binding `IsEllipse` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsEllipse` (verified)
- line 115: materialIcons:MaterialIcon.IsVisible = Binding `IsGroup` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsGroup` (verified)
- line 116: materialIcons:MaterialIcon.IsVisible = Binding `IsPath` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsPath` (verified)
- line 117: materialIcons:MaterialIcon.IsVisible = Binding `IsText` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsText` (verified)
- line 120: TextBlock.IsVisible = Binding `!IsRenaming` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRenaming` (verified)
- line 120: TextBlock.Text = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.Name` (verified)
- line 121: TextBox.IsVisible = Binding `IsRenaming` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsRenaming` (verified)
- line 121: TextBox.Text = Binding `EditText` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.EditText` (verified)
- line 124: KeyBinding.Command = Binding `CommitRenameCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CommitRenameCommand` (verified), command
- line 125: KeyBinding.Command = Binding `CancelRenameCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CancelRenameCommand` (verified), command
- line 129: Button.Command = Binding `ToggleLockCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleLockCommand` (verified), command
- line 132: materialIcons:MaterialIcon.IsVisible = Binding `IsLocked` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLocked` (verified)
- line 133: materialIcons:MaterialIcon.IsVisible = Binding `!IsLocked` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsLocked` (verified)
- line 136: Button.Command = Binding `ToggleVisibilityCommand` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleVisibilityCommand` (verified), command
- line 139: materialIcons:MaterialIcon.IsVisible = Binding `IsVisible` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsVisible` (verified)
- line 140: materialIcons:MaterialIcon.IsVisible = Binding `!IsVisible` -> `P:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.IsVisible` (verified)

## Resource references

- line 20: Setter.MinWidth = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 21: Setter.MinHeight = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 29: Setter.Background = DynamicResource `ControlHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Light` (verified)
- line 33: Setter.Background = DynamicResource `ControlPressedBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlPressedBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlPressedBrush@Light` (verified)
- line 36: Setter.Background = DynamicResource `SelectedItemBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectedItemBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectedItemBrush@Light` (verified)
- line 43: Setter.BorderBrush = DynamicResource `AppAccentBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Light` (verified)
- line 46: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 50: DockPanel.Margin = StaticResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 58: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 58: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 62: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 62: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 66: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 66: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 70: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 70: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 74: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 74: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 78: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 78: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 98: Border.MinHeight = DynamicResource `LayerRowMinHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/LayerRowMinHeight` (verified)
- line 102: Panel.MinWidth = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 106: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 106: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 107: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 107: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 112: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 112: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 113: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 113: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 114: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 114: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 115: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 115: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 116: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 116: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 117: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 117: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 132: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 132: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 133: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 133: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 139: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 139: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 140: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 140: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)

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

- line 91: DataTemplate for `vm:ObjectRowViewModel` (`T:PressToShape.App.ViewModels.Editor.ObjectRowViewModel`)
