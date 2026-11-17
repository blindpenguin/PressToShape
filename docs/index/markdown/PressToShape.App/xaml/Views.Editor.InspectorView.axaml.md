# InspectorView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/InspectorView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.InspectorView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.InspectorViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/LayersViewTests.cs:152)
- `T:PressToShape.App.ViewModels.Editor.InspectorViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:47 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:47)
- `T:PressToShape.App.ViewModels.Editor.InspectorViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/InspectorView.axaml:1)

## Bindings

- line 18: TabControl.SelectedIndex = Binding `SelectedTabIndex` -> `P:PressToShape.App.ViewModels.Editor.InspectorViewModel.SelectedTabIndex` (verified)
- line 26: ContentControl.Content = Binding `Properties` -> `P:PressToShape.App.ViewModels.Editor.InspectorViewModel.Properties` (verified)
- line 29: ContentControl.Content = Binding `Layers` -> `P:PressToShape.App.ViewModels.Editor.InspectorViewModel.Layers` (verified)
- line 32: ContentControl.Content = Binding `Assets` -> `P:PressToShape.App.ViewModels.Editor.InspectorViewModel.Assets` (verified)
- line 35: ContentControl.Content = Binding `History` -> `P:PressToShape.App.ViewModels.Editor.InspectorViewModel.History` (verified)

## Resource references

- line 13: Setter.Theme = StaticResource `InspectorTabItemTheme` -> `R:src/PressToShape.App/Styles/Controls.axaml#/Styles/Styles.Resources[1]/InspectorTabItemTheme` (verified)

## Styles

- line 12: `TabItem`, sets Theme, uses InspectorTabItemTheme
