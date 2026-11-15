# LayersView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/LayersView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.LayersView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (inferred): ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.LayersViewModel maps to PressToShape.App.Views.Editor.LayersView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:29 (src/PressToShape.App/Views/Editor/InspectorView.axaml:29)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:244)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/LayersView.axaml:1)

## Bindings

- line 29: Button.Command = Binding `AddLayerCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.AddLayerCommand` (verified), command
- line 40: ListBox.ItemsSource = Binding `Items` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.Items` (verified)
- line 40: ListBox.SelectedItem = Binding `SelectedItem` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.SelectedItem` (verified)
- line 45: TextBlock.Text = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.Name` (verified)
- line 46: Button.Command = Binding `ToggleLockCommand` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.ToggleLockCommand` (verified), command
- line 49: materialIcons:MaterialIcon.IsVisible = Binding `IsLocked` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.IsLocked` (verified)
- line 50: materialIcons:MaterialIcon.IsVisible = Binding `!IsLocked` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.IsLocked` (verified)
- line 53: Button.Command = Binding `ToggleVisibilityCommand` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.ToggleVisibilityCommand` (verified), command
- line 56: materialIcons:MaterialIcon.IsVisible = Binding `IsVisible` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.IsVisible` (verified)
- line 57: materialIcons:MaterialIcon.IsVisible = Binding `!IsVisible` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.IsVisible` (verified)

## Resource references

- line 14: Setter.MinHeight = DynamicResource `LayerRowMinHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/LayerRowMinHeight` (verified)
- line 19: Setter.MinWidth = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 20: Setter.MinHeight = DynamicResource `RowButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowButtonSize` (verified)
- line 24: DockPanel.Margin = StaticResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 31: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 31: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 35: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 35: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 44: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 44: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 49: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 49: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 50: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 50: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 56: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 56: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 57: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 57: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)

## Styles

- line 13: `ListBoxItem`, sets MinHeight, Padding, uses LayerRowMinHeight
- line 18: `Button.rowButton`, sets MinHeight, MinWidth, uses RowButtonSize

## Templates

- line 42: DataTemplate for `vm:LayerItemViewModel` (`T:PressToShape.App.ViewModels.Editor.LayerItemViewModel`)
