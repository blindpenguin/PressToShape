# Services

Types that other types take as constructor parameters, and where they are provided.

## Injected types

### ColorFieldViewModel (class)

`T:PressToShape.App.ViewModels.Editor.ColorFieldViewModel`. A color of the Properties tab (fill or stroke). While the color picker is open the user's changes are previewed on the canvas (PressToShape.App.ViewModels.Editor.ColorFieldViewModel.Update) and the whole edit becomes one undo step when the picker closes (PressToShape.App.ViewModels.Editor.ColorFieldViewModel.EndEdit). "None" and a color chosen elsewhere take effect at once as one step.

- used by: `PaletteViewModel`
- created at `PropertiesViewModel.PropertiesViewModel` (src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs:48), `PropertiesViewModel.PropertiesViewModel` (src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs:52)

### Document (class)

`T:PressToShape.Core.Documents.Document`. A drawing: an artboard and a stack of layers. Immutable: every edit method returns a new document and leaves this one unchanged. Unchanged layers are shared between the two. A document always has at least one layer.

- used by: `DocumentHistory`, `EditorState`, `ProjectFileResult`
- created at `Document.Create` (src/PressToShape.Core/Documents/Document.cs:61)

### DocumentSession (class)

`T:PressToShape.App.Workspace.DocumentSession`. Which file the open document belongs to and whether it has unsaved changes. Documents are immutable, so "unsaved" means: the editor's document is no longer the instance that was last saved or opened. A change that is changed back still counts as unsaved.

- used by: `DocumentFileService`
- created at `DesktopShellViewModel.DesktopShellViewModel` (src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:31)

### EditMenuCommands (class)

`T:PressToShape.App.ViewModels.Desktop.EditMenuCommands`. What the clipboard, delete and selection items of the Edit menu do. Undo and Redo are passed on their own.

- used by: `MenuBarViewModel`
- created at `DesktopShellViewModel.DesktopShellViewModel` (src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:47)

### EditorState (class)

`T:PressToShape.App.Workspace.EditorState`. State shared by all regions of the editor shell: the document, its undo history, active tool, zoom and selection summary. View models observe it through PropertyChanged, so every route that changes a value stays in sync. This class is the only place that changes the document: edits go through PressToShape.App.Workspace.EditorState.Edit, a drag through PressToShape.App.Workspace.EditorState.Preview and PressToShape.App.Workspace.EditorState.Commit, and a new or opened document through PressToShape.App.Workspace.EditorState.ReplaceDocument.

- used by: `CanvasAreaViewModel`, `CanvasInteraction`, `DocumentFileService`, `DocumentSession`, `HistoryViewModel`, `InspectorViewModel`, `LayerItemViewModel`, `LayersViewModel`, `MenuBarViewModel`, `PaletteViewModel`, `PropertiesViewModel`, `SelectionEditor`, `StatusBarViewModel`, `ToolbarViewModel`, `ToolboxViewModel`
- created at `DesktopShellViewModel.DesktopShellViewModel` (src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:30), `ShapeOperationBenchmarks.Setup` (tests/PressToShape.Benchmarks/ShapeOperationBenchmarks.cs:44)

### FileMenuCommands (class)

`T:PressToShape.App.ViewModels.Desktop.FileMenuCommands`. What the File menu items do. OpenRecent creates the command for one entry of the recent list.

- used by: `MenuBarViewModel`
- created at `DesktopShellViewModel.DesktopShellViewModel` (src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:40)

### FileServices (class)

`T:PressToShape.App.Services.FileServices`. What the file actions need from the outside: settings, files, file pickers and message dialogs.

- used by: `DesktopShellViewModel`
- created at `PressToShapeApp.OnFrameworkInitializationCompleted` (src/PressToShape.App/PressToShapeApp.axaml.cs:39)

### IAppShutdown (interface)

`T:PressToShape.App.Services.IAppShutdown`. Lets view models close the application without referencing Avalonia types.

- used by: `DesktopShellViewModel`, `MenuBarViewModel`
- implemented by `DesktopAppShutdown`, created at `PressToShapeApp.OnFrameworkInitializationCompleted` (src/PressToShape.App/PressToShapeApp.axaml.cs:46)

### ICatalogSource (interface)

`T:PressToShape.App.Localization.ICatalogSource`. Finds the translation catalog of a language. English has no catalog, because the source text is English.

- used by: `Localizer`
- implemented by `EmbeddedCatalogSource`, created at `EmbeddedCatalogSource.EmbeddedCatalogSource` (src/PressToShape.App/Localization/EmbeddedCatalogSource.cs:20), `PressToShapeApp.Initialize` (src/PressToShape.App/PressToShapeApp.axaml.cs:26)
- implemented by `NoCatalogSource`, created at `LocalizationHost.Localizer` (src/PressToShape.App/Localization/LocalizationHost.cs:14)

### IFileDialogs (interface)

`T:PressToShape.App.Services.IFileDialogs`. The file pickers. Each returns a local file path, or null if the user cancelled.

- used by: `DocumentFileService`, `FileServices`
- implemented by `AvaloniaFileDialogs`, created at `PressToShapeApp.OnFrameworkInitializationCompleted` (src/PressToShape.App/PressToShapeApp.axaml.cs:42)

### IFileStore (interface)

`T:PressToShape.App.Services.IFileStore`. Reads and writes whole files. Lets the file actions be tested without a real file system.

- used by: `DocumentFileService`, `FileServices`
- implemented by `LocalFileStore`, created at `PressToShapeApp.OnFrameworkInitializationCompleted` (src/PressToShape.App/PressToShapeApp.axaml.cs:41)

### ILocalizer (interface)

`T:PressToShape.App.Localization.ILocalizer`. Translates user-visible texts. The English source text is the message id.

- used by: `AvaloniaFileDialogs`, `AvaloniaMessageDialogs`, `DesktopShellViewModel`, `DocumentFileService`, `HistoryItemViewModel`, `HistoryViewModel`, `InspectorViewModel`, `LanguagePreference`, `LocalizedTextObservable`, `LocalizedViewModel`, `MenuBarViewModel`, `MenuItemViewModel`, `NumberFieldViewModel`, `PlaceholderPanelViewModel`, `PropertiesViewModel`, `StatusBarViewModel`, `ToolItemViewModel`, `ToolbarButtonViewModel`, `ToolbarViewModel`, `ToolboxViewModel`
- implemented by `Localizer`, created at `LocalizationHost.Localizer` (src/PressToShape.App/Localization/LocalizationHost.cs:14), `PressToShapeApp.Initialize` (src/PressToShape.App/PressToShapeApp.axaml.cs:25)

### IMessageDialogs (interface)

`T:PressToShape.App.Services.IMessageDialogs`. The questions and messages shown to the user. The texts are translated by the caller.

- used by: `DocumentFileService`, `FileServices`
- implemented by `AvaloniaMessageDialogs`, created at `PressToShapeApp.OnFrameworkInitializationCompleted` (src/PressToShape.App/PressToShapeApp.axaml.cs:43)

### ISettingsStore (interface)

`T:PressToShape.App.Settings.ISettingsStore`. Loads and saves PressToShape.App.Settings.AppSettings. Neither operation throws.

- used by: `FileServices`, `LanguagePreference`, `RecentFiles`
- implemented by `JsonFileSettingsStore`, created at `PressToShapeApp.Initialize` (src/PressToShape.App/PressToShapeApp.axaml.cs:24)

### Layer (class)

`T:PressToShape.Core.Documents.Layer`. A named group of shapes that can be hidden or locked. Immutable: change it with with. Records compare PressToShape.Core.Documents.Layer.Shapes by reference, not by content.

- used by: `LayerItemViewModel`, `ShapeLocation`
- created at `Document.Create` (src/PressToShape.Core/Documents/Document.cs:61), `ProjectFile.Build` (src/PressToShape.Core/IO/ProjectFile.cs:130)

### Mixed (class)

`T:PressToShape.App.Workspace.Mixed`1`. A value that is the same for every selected shape, or marked as mixed when the shapes differ.

- used by: `SelectionValues`
- created at `Mixed.Of` (src/PressToShape.App/Workspace/SelectionValues.cs:31), `Mixed.Of` (src/PressToShape.App/Workspace/SelectionValues.cs:35)

### RecentFiles (class)

`T:PressToShape.App.Workspace.RecentFiles`. The list of recently used files, most recent first, kept in the settings file. It holds each path once and at most PressToShape.App.Workspace.RecentFiles.MaxCount paths. The stored value is untrusted: anything that is not a list of paths is ignored.

- used by: `DocumentFileService`, `MenuBarViewModel`
- created at `DesktopShellViewModel.DesktopShellViewModel` (src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:32)

### ScreenPoint (class)

`T:PressToShape.App.Workspace.ScreenPoint`. A point in screen pixels.

- used by: `ScreenHandle`, `ScreenLine`
- created at `ScreenPolygon.FromBox` (src/PressToShape.App/Workspace/ScreenPolygon.cs:19), `SelectionOverlay.Build` (src/PressToShape.App/Workspace/SelectionOverlay.cs:84)

### ScreenPolygon (class)

`T:PressToShape.App.Workspace.ScreenPolygon`. A closed shape of straight lines in screen pixels, for example the turned outline of a shape. Always four points here.

- used by: `SelectionOverlay`
- created at `ScreenPolygon.FromBox` (src/PressToShape.App/Workspace/ScreenPolygon.cs:22)

### SelectionEditor (class)

`T:PressToShape.App.Workspace.SelectionEditor`. Edits that work on the selected shapes and do not need the mouse: select all, deselect, copy, cut, paste, duplicate (and, in the same class, nudging and property edits). Every change of the document is one history step.

- used by: `CanvasAreaViewModel`, `InspectorViewModel`, `PropertiesViewModel`
- created at `DesktopShellViewModel.DesktopShellViewModel` (src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:35), `CanvasAreaViewModel.CanvasAreaViewModel` (src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs:31), `InspectorViewModel.InspectorViewModel` (src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs:20), `ShapeOperationBenchmarks.Setup` (tests/PressToShape.Benchmarks/ShapeOperationBenchmarks.cs:46)

### Shape (class)

`T:PressToShape.Core.Shapes.Shape`. A drawable shape with a position, a size and an appearance. Immutable: change it with with. Values are validated on creation and on with.

- used by: `ShapeLocation`
- created at no creation site found

### ToolDefinition (class)

`T:PressToShape.App.Workspace.ToolDefinition`. Display data for one tool: name, optional single-key shortcut and icon.

- used by: `ToolItemViewModel`
- created at `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:13), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:14), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:15), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:16), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:17), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:18), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:19), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:20), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:21), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:22), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:23), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:24)

## Container registrations

no container registrations found; services are created by hand in `CanvasAreaViewModel.CanvasAreaViewModel`, `DesktopShellViewModel.DesktopShellViewModel`, `Document.Create`, `EmbeddedCatalogSource.EmbeddedCatalogSource`, `InspectorViewModel.InspectorViewModel`, `LocalizationHost.Localizer`, `Mixed.Of`, `PressToShapeApp.Initialize`, `PressToShapeApp.OnFrameworkInitializationCompleted`, `ProjectFile.Build`, `PropertiesViewModel.PropertiesViewModel`, `ScreenPolygon.FromBox`, `SelectionOverlay.Build`, `ShapeOperationBenchmarks.Setup`, `ToolDefinitions.All`.
