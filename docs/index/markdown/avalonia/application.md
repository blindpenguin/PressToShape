# Application

## Application class

### PressToShapeApp

`T:PressToShape.App.PressToShapeApp` in src/PressToShape.App/PressToShapeApp.axaml.cs.

- `Initialize` at line 21: creates `JsonFileSettingsStore` (line 24), `Localizer` (line 25), `EmbeddedCatalogSource` (line 26)
- `OnFrameworkInitializationCompleted` at line 34: creates nothing from this solution
  - when the lifetime is `IClassicDesktopStyleApplicationLifetime` (line 36): creates `FileServices` (line 39), `LocalFileStore` (line 41), `AvaloniaFileDialogs` (line 42), `AvaloniaMessageDialogs` (line 43), `MainWindow` (line 44), `DesktopAppShutdown` (line 46), `DesktopShellViewModel` (line 46)
  - when the lifetime is `IActivityApplicationLifetime` (line 49): creates `MainViewModel` (line 51), `MainView` (line 51)
  - when the lifetime is `ISingleViewApplicationLifetime` (line 53): creates `MainView` (line 55), `MainViewModel` (line 57)

### HeadlessUiApp

`T:PressToShape.App.Tests.HeadlessUiApp` in tests/PressToShape.App.Tests/HeadlessUi.cs.

- `Initialize` at line 21: creates nothing from this solution

## View locator convention

- ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name

## Explicit data templates

- `vm:ToolbarButtonViewModel` at src/PressToShape.App/Views/Desktop/ToolbarView.axaml:13
- `vm:HistoryItemViewModel` at src/PressToShape.App/Views/Editor/HistoryView.axaml:33
- `vm:LayerItemViewModel` at src/PressToShape.App/Views/Editor/LayersView.axaml:37
- `coreShapes:Color` at src/PressToShape.App/Views/Editor/PropertiesView.axaml:113
- `coreShapes:Color` at src/PressToShape.App/Views/Editor/PropertiesView.axaml:153
- `coreShapes:Color` at src/PressToShape.App/Views/Editor/PropertiesView.axaml:204
- `vm:ToolItemViewModel` at src/PressToShape.App/Views/Editor/ToolboxView.axaml:13
- `vm:MessageDialogButtonViewModel` at src/PressToShape.App/Views/MessageDialogWindow.axaml:29

## Content hosts

XAML places that show a view model through a content control, so the view is chosen at run time. These links are `inferred`.

- src/PressToShape.App/Views/Editor/InspectorView.axaml:26 shows `PropertiesViewModel` as `PropertiesView` (inferred)
- src/PressToShape.App/Views/Editor/InspectorView.axaml:29 shows `LayersViewModel` as `LayersView` (inferred)
- src/PressToShape.App/Views/Editor/InspectorView.axaml:32 shows `PlaceholderPanelViewModel` as `PlaceholderPanelView` (inferred)
- src/PressToShape.App/Views/Editor/InspectorView.axaml:35 shows `HistoryViewModel` as `HistoryView` (inferred)

## Navigation

no navigation service found: the index has no data about navigation, so nothing is claimed about how views are switched.
