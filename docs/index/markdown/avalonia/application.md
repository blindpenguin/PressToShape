# Application

## Application class

### PressToShapeApp

`T:PressToShape.App.PressToShapeApp` in src/PressToShape.App/PressToShapeApp.axaml.cs.

- `Initialize` at line 22: creates `JsonFileSettingsStore` (line 25), `Localizer` (line 26), `EmbeddedCatalogSource` (line 27)
- `OnFrameworkInitializationCompleted` at line 35: creates nothing from this solution

### HeadlessUiApp

`T:PressToShape.App.Tests.HeadlessUiApp` in tests/PressToShape.App.Tests/HeadlessUi.cs.

- `Initialize` at line 21: creates `ViewLocator` (line 26)

## View locator convention

- ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name

## Explicit data templates

- `vm:ToolbarButtonViewModel` at src/PressToShape.App/Views/Desktop/ToolbarView.axaml:13
- `vm:HistoryItemViewModel` at src/PressToShape.App/Views/Editor/HistoryView.axaml:33
- `vm:ObjectRowViewModel` at src/PressToShape.App/Views/Editor/LayersView.axaml:69
- `coreShapes:Color` at src/PressToShape.App/Views/Editor/PropertiesView.axaml:145
- `coreShapes:Color` at src/PressToShape.App/Views/Editor/PropertiesView.axaml:185
- `coreShapes:Color` at src/PressToShape.App/Views/Editor/PropertiesView.axaml:236
- `vm:ToolItemViewModel` at src/PressToShape.App/Views/Editor/ToolboxView.axaml:15
- `vm:MessageDialogButtonViewModel` at src/PressToShape.App/Views/MessageDialogView.axaml:22
- `vm:ToolGroupViewModel` at src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml:29
- `editorVm:ToolItemViewModel` at src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml:56
- `vm:PanelButtonViewModel` at src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml:85

## Content hosts

XAML places that show a view model through a content control, so the view is chosen at run time. These links are `inferred`.

- src/PressToShape.App/Views/Editor/InspectorView.axaml:26 shows `PropertiesViewModel` as `PropertiesView` (inferred)
- src/PressToShape.App/Views/Editor/InspectorView.axaml:29 shows `LayersViewModel` as `LayersView` (inferred)
- src/PressToShape.App/Views/Editor/InspectorView.axaml:32 shows `PlaceholderPanelViewModel` as `PlaceholderPanelView` (inferred)
- src/PressToShape.App/Views/Editor/InspectorView.axaml:35 shows `HistoryViewModel` as `HistoryView` (inferred)

## Navigation

no navigation service found: the index has no data about navigation, so nothing is claimed about how views are switched.
