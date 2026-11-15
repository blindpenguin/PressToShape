# Views and view models

Links come from `x:DataType`, DataContext bindings and assignments, and the ViewLocator convention. `inferred` links are not proven, the reason is given.

## Views

### DesktopShellView

`T:PressToShape.App.Views.Desktop.DesktopShellView` at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml.cs:8. XAML: `src/PressToShape.App/Views/Desktop/DesktopShellView.axaml`, 20 bindings.
_(no documentation)_

- view model `DesktopShellViewModel` (`T:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`), verified: x:DataType (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:1); 20 commands, 2 test classes

### MenuBarView

`T:PressToShape.App.Views.Desktop.MenuBarView` at src/PressToShape.App/Views/Desktop/MenuBarView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Desktop/MenuBarView.axaml`, 13 bindings.
_(no documentation)_

- view model `MenuBarViewModel` (`T:PressToShape.App.ViewModels.Desktop.MenuBarViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:35 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:35); 0 commands, 3 test classes
- view model `MenuBarViewModel` (`T:PressToShape.App.ViewModels.Desktop.MenuBarViewModel`), verified: x:DataType (src/PressToShape.App/Views/Desktop/MenuBarView.axaml:1); 0 commands, 3 test classes

### StatusBarView

`T:PressToShape.App.Views.Desktop.StatusBarView` at src/PressToShape.App/Views/Desktop/StatusBarView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Desktop/StatusBarView.axaml`, 11 bindings.
_(no documentation)_

- view model `StatusBarViewModel` (`T:PressToShape.App.ViewModels.Desktop.StatusBarViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:49 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:49); 5 commands, 3 test classes
- view model `StatusBarViewModel` (`T:PressToShape.App.ViewModels.Desktop.StatusBarViewModel`), verified: x:DataType (src/PressToShape.App/Views/Desktop/StatusBarView.axaml:1); 5 commands, 3 test classes

### ToolbarView

`T:PressToShape.App.Views.Desktop.ToolbarView` at src/PressToShape.App/Views/Desktop/ToolbarView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Desktop/ToolbarView.axaml`, 13 bindings.
_(no documentation)_

- view model `ToolbarViewModel` (`T:PressToShape.App.ViewModels.Desktop.ToolbarViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:38 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:38); 0 commands, 2 test classes
- view model `ToolbarViewModel` (`T:PressToShape.App.ViewModels.Desktop.ToolbarViewModel`), verified: x:DataType (src/PressToShape.App/Views/Desktop/ToolbarView.axaml:1); 0 commands, 2 test classes

### CanvasAreaView

`T:PressToShape.App.Views.Editor.CanvasAreaView` at src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs:10. XAML: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`, 26 bindings.
_(no documentation)_

- view model `CanvasAreaViewModel` (`T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42); 0 commands, 3 test classes
- view model `CanvasAreaViewModel` (`T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/CanvasAreaView.axaml:1); 0 commands, 3 test classes

### HistoryView

`T:PressToShape.App.Views.Editor.HistoryView` at src/PressToShape.App/Views/Editor/HistoryView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/HistoryView.axaml`, 7 bindings.
_(no documentation)_

- view model `HistoryViewModel` (`T:PressToShape.App.ViewModels.Editor.HistoryViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/HistoryView.axaml:1); 0 commands, 4 test classes
- view model `HistoryViewModel` (`T:PressToShape.App.ViewModels.Editor.HistoryViewModel`), inferred: ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.HistoryViewModel maps to PressToShape.App.Views.Editor.HistoryView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:35 (src/PressToShape.App/Views/Editor/InspectorView.axaml:35); 0 commands, 4 test classes

### InspectorView

`T:PressToShape.App.Views.Editor.InspectorView` at src/PressToShape.App/Views/Editor/InspectorView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/InspectorView.axaml`, 5 bindings.
_(no documentation)_

- view model `InspectorViewModel` (`T:PressToShape.App.ViewModels.Editor.InspectorViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:46 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:46); 0 commands, 3 test classes
- view model `InspectorViewModel` (`T:PressToShape.App.ViewModels.Editor.InspectorViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/InspectorView.axaml:1); 0 commands, 3 test classes

### LayersView

`T:PressToShape.App.Views.Editor.LayersView` at src/PressToShape.App/Views/Editor/LayersView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/LayersView.axaml`, 10 bindings.
_(no documentation)_

- view model `LayersViewModel` (`T:PressToShape.App.ViewModels.Editor.LayersViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/LayersView.axaml:1); 1 commands, 2 test classes
- view model `LayersViewModel` (`T:PressToShape.App.ViewModels.Editor.LayersViewModel`), inferred: ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.LayersViewModel maps to PressToShape.App.Views.Editor.LayersView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:29 (src/PressToShape.App/Views/Editor/InspectorView.axaml:29); 1 commands, 2 test classes

### PlaceholderPanelView

`T:PressToShape.App.Views.Editor.PlaceholderPanelView` at src/PressToShape.App/Views/Editor/PlaceholderPanelView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/PlaceholderPanelView.axaml`, 1 bindings.
_(no documentation)_

- view model `PlaceholderPanelViewModel` (`T:PressToShape.App.ViewModels.Editor.PlaceholderPanelViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/PlaceholderPanelView.axaml:1); 0 commands, 2 test classes
- view model `PlaceholderPanelViewModel` (`T:PressToShape.App.ViewModels.Editor.PlaceholderPanelViewModel`), inferred: ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.PlaceholderPanelViewModel maps to PressToShape.App.Views.Editor.PlaceholderPanelView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:32 (src/PressToShape.App/Views/Editor/InspectorView.axaml:32); 0 commands, 2 test classes

### PropertiesView

`T:PressToShape.App.Views.Editor.PropertiesView` at src/PressToShape.App/Views/Editor/PropertiesView.axaml.cs:12. XAML: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, 49 bindings.
The Properties tab. The code-behind only forwards: focus, Enter and Escape of the number fields, the pickers' open and close, and the opacity slider's drag. Every decision is in PressToShape.App.ViewModels.Editor.PropertiesViewModel.

- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/PropertiesView.axaml:1); 1 commands, 5 test classes
- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), inferred: ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.PropertiesViewModel maps to PressToShape.App.Views.Editor.PropertiesView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:26 (src/PressToShape.App/Views/Editor/InspectorView.axaml:26); 1 commands, 5 test classes

### ToolboxView

`T:PressToShape.App.Views.Editor.ToolboxView` at src/PressToShape.App/Views/Editor/ToolboxView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/ToolboxView.axaml`, 8 bindings.
_(no documentation)_

- view model `ToolboxViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolboxViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:41 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:41); 3 commands, 2 test classes
- view model `ToolboxViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolboxViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/ToolboxView.axaml:1); 3 commands, 2 test classes

### MainView

`T:PressToShape.App.Views.MainView` at src/PressToShape.App/Views/MainView.axaml.cs:5. XAML: `src/PressToShape.App/Views/MainView.axaml`, 1 bindings.
_(no documentation)_

- view model `MainViewModel` (`T:PressToShape.App.ViewModels.MainViewModel`), verified: DataContext assignment (src/PressToShape.App/PressToShapeApp.axaml.cs:51); 0 commands, 2 test classes
- view model `MainViewModel` (`T:PressToShape.App.ViewModels.MainViewModel`), verified: DataContext assignment (src/PressToShape.App/PressToShapeApp.axaml.cs:57); 0 commands, 2 test classes
- view model `MainViewModel` (`T:PressToShape.App.ViewModels.MainViewModel`), verified: x:DataType (src/PressToShape.App/Views/MainView.axaml:1); 0 commands, 2 test classes
- view model `MainViewModel` (`T:PressToShape.App.ViewModels.MainViewModel`), inferred: design-time only (Design.DataContext), not a runtime link (src/PressToShape.App/Views/MainView.axaml:10); 0 commands, 2 test classes

### MainWindow

`T:PressToShape.App.Views.MainWindow` at src/PressToShape.App/Views/MainWindow.axaml.cs:6. XAML: `src/PressToShape.App/Views/MainWindow.axaml`, 1 bindings.
_(no documentation)_

- view model `DesktopShellViewModel` (`T:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`), verified: DataContext assignment (src/PressToShape.App/PressToShapeApp.axaml.cs:46); 20 commands, 2 test classes
- view model `DesktopShellViewModel` (`T:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`), verified: x:DataType (src/PressToShape.App/Views/MainWindow.axaml:1); 20 commands, 2 test classes

### MessageDialogWindow

`T:PressToShape.App.Views.MessageDialogWindow` at src/PressToShape.App/Views/MessageDialogWindow.axaml.cs:7. XAML: `src/PressToShape.App/Views/MessageDialogWindow.axaml`, 7 bindings.
_(no documentation)_

- view model `MessageDialogViewModel` (`T:PressToShape.App.ViewModels.MessageDialogViewModel`), verified: DataContext assignment (src/PressToShape.App/Services/AvaloniaMessageDialogs.cs:43); 0 commands, 1 test classes
- view model `MessageDialogViewModel` (`T:PressToShape.App.ViewModels.MessageDialogViewModel`), verified: x:DataType (src/PressToShape.App/Views/MessageDialogWindow.axaml:1); 0 commands, 1 test classes

## View models without a view

Types that derive from a base class of a linked view model but that no view links to. A view may still create them in code.

- `MenuItemViewModel` (`T:PressToShape.App.ViewModels.Desktop.MenuItemViewModel`) at src/PressToShape.App/ViewModels/Desktop/MenuItemViewModel.cs:15
- `ToolbarButtonViewModel` (`T:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel`) at src/PressToShape.App/ViewModels/Desktop/ToolbarButtonViewModel.cs:8
- `ColorFieldViewModel` (`T:PressToShape.App.ViewModels.Editor.ColorFieldViewModel`) at src/PressToShape.App/ViewModels/Editor/ColorFieldViewModel.cs:12
- `HistoryItemViewModel` (`T:PressToShape.App.ViewModels.Editor.HistoryItemViewModel`) at src/PressToShape.App/ViewModels/Editor/HistoryItemViewModel.cs:11
- `LayerItemViewModel` (`T:PressToShape.App.ViewModels.Editor.LayerItemViewModel`) at src/PressToShape.App/ViewModels/Editor/LayerItemViewModel.cs:14
- `NumberFieldViewModel` (`T:PressToShape.App.ViewModels.Editor.NumberFieldViewModel`) at src/PressToShape.App/ViewModels/Editor/NumberFieldViewModel.cs:13
- `ToolItemViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolItemViewModel`) at src/PressToShape.App/ViewModels/Editor/ToolItemViewModel.cs:10

## Views without a view model

none found
