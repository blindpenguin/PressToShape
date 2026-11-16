# HistoryView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/HistoryView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.HistoryView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.HistoryViewModel` (inferred): ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.HistoryViewModel maps to PressToShape.App.Views.Editor.HistoryView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:38 (src/PressToShape.App/Views/Editor/InspectorView.axaml:38)
- `T:PressToShape.App.ViewModels.Editor.HistoryViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:246)
- `T:PressToShape.App.ViewModels.Editor.HistoryViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/HistoryView.axaml:1)

## Bindings

- line 29: StepList.ItemsSource = Binding `Items` -> `P:PressToShape.App.ViewModels.Editor.HistoryViewModel.Items` (verified)
- line 30: StepList.SelectedItem = Binding `SelectedItem` -> `P:PressToShape.App.ViewModels.Editor.HistoryViewModel.SelectedItem` (verified)
- line 35: Grid.Classes.redoable = Binding `IsRedoable` -> `P:PressToShape.App.ViewModels.Editor.HistoryItemViewModel.IsRedoable` (verified)
- line 36: Grid.AutomationProperties.Name = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.HistoryItemViewModel.Name` (verified)
- line 38: materialIcons:MaterialIcon.IsVisible = Binding `IsCurrent` -> `P:PressToShape.App.ViewModels.Editor.HistoryItemViewModel.IsCurrent` (verified)
- line 39: materialIcons:MaterialIcon.IsVisible = Binding `!IsCurrent` -> `P:PressToShape.App.ViewModels.Editor.HistoryItemViewModel.IsCurrent` (verified)
- line 41: TextBlock.Text = Binding `Name` -> `P:PressToShape.App.ViewModels.Editor.HistoryItemViewModel.Name` (verified)

## Resource references

- line 14: Setter.MinHeight = DynamicResource `HistoryRowMinHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/HistoryRowMinHeight` (verified)
- line 23: DockPanel.Margin = StaticResource `PanelPadding` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding` (verified)
- line 37: Panel.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 37: Panel.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 38: materialIcons:MaterialIcon.Height = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)
- line 38: materialIcons:MaterialIcon.Width = DynamicResource `RowIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RowIconSize` (verified)

## Styles

- line 13: `ListBoxItem`, sets MinHeight, Padding, uses HistoryRowMinHeight
- line 18: `Grid.redoable`, sets Opacity

## Templates

- line 33: DataTemplate for `vm:HistoryItemViewModel` (`T:PressToShape.App.ViewModels.Editor.HistoryItemViewModel`)
