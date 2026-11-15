# LayersView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/LayersView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.LayersView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (inferred): ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.LayersViewModel maps to PressToShape.App.Views.Editor.LayersView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:29 (src/PressToShape.App/Views/Editor/InspectorView.axaml:29)
- `T:PressToShape.App.ViewModels.Editor.LayersViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/LayersView.axaml:1)

## Bindings

- line 24: Button.Command = Binding `AddLayerCommand` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.AddLayerCommand` (verified), command
- line 35: ListBox.ItemsSource = Binding `Items` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.Items` (verified)
- line 35: ListBox.SelectedItem = Binding `SelectedItem` -> `P:PressToShape.App.ViewModels.Editor.LayersViewModel.SelectedItem` (verified)
- line 40: TextBlock.Text = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.Name` (verified)
- line 41: Button.Command = Binding `ToggleLockCommand` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.ToggleLockCommand` (verified), command
- line 45: materialIcons:MaterialIcon.IsVisible = Binding `IsLocked` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.IsLocked` (verified)
- line 46: materialIcons:MaterialIcon.IsVisible = Binding `!IsLocked` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.IsLocked` (verified)
- line 49: Button.Command = Binding `ToggleVisibilityCommand` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.ToggleVisibilityCommand` (verified), command
- line 53: materialIcons:MaterialIcon.IsVisible = Binding `IsVisible` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.IsVisible` (verified)
- line 54: materialIcons:MaterialIcon.IsVisible = Binding `!IsVisible` -> `P:PressToShape.App.ViewModels.Editor.LayerItemViewModel.IsVisible` (verified)

## Resource references

- line 19: DockPanel.Margin = StaticResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)

## Styles

- line 13: `ListBoxItem`, sets MinHeight, Padding

## Templates

- line 37: DataTemplate for `vm:LayerItemViewModel` (`T:PressToShape.App.ViewModels.Editor.LayerItemViewModel`)
