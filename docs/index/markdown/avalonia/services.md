# Services

Types that other types take as constructor parameters, and where they are provided.

## Injected types

### ColorFieldViewModel (class)

`T:PressToShape.App.ViewModels.Editor.ColorFieldViewModel`. A color of the Properties tab (fill or stroke). While the color picker is open the user's changes are previewed on the canvas (PressToShape.App.ViewModels.Editor.ColorFieldViewModel.Update) and the whole edit becomes one undo step when the picker closes (PressToShape.App.ViewModels.Editor.ColorFieldViewModel.EndEdit). "None" and a color chosen elsewhere take effect at once as one step.

- used by: `PaletteViewModel`
- created at `PropertiesViewModel.PropertiesViewModel` (src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs:51), `PropertiesViewModel.PropertiesViewModel` (src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs:55)

### Document (class)

`T:PressToShape.Core.Documents.Document`. A drawing: an artboard and a stack of layers. Immutable: every edit method returns a new document and leaves this one unchanged. Unchanged layers are shared between the two. A document always has at least one layer.

- used by: `DocumentHistory`, `EditorState`, `ProjectFileResult`
- created at `Document.Create` (src/PressToShape.Core/Documents/Document.cs:61)

### DocumentSession (class)

`T:PressToShape.App.Workspace.DocumentSession`. Which file the open document belongs to and whether it has unsaved changes. Documents are immutable, so "unsaved" means: the editor's document is no longer the instance that was last saved or opened. A change that is changed back still counts as unsaved.

- used by: `DocumentFileService`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:30)

### EditMenuCommands (class)

`T:PressToShape.App.ViewModels.Editor.EditMenuCommands`. What the clipboard, delete and selection items of the Edit menu do. Undo and Redo are passed on their own.

- used by: `MenuBarViewModel`, `OverflowMenuViewModel`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:45)

### EditorShellViewModel (class)

`T:PressToShape.App.ViewModels.Editor.EditorShellViewModel`. What every layout shell needs. Creates the shared PressToShape.App.Workspace.EditorState and the view model of every region that the layouts share (tool column, canvas, inspector), and owns the file, edit, undo and redo commands, so the regions of a layout always agree. The desktop and the tablet layout derive from it and add their own regions.

- used by: `TabletToolbarViewModel`
- created at `DesktopShellViewModel.DesktopShellViewModel` (src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:17), `TabletShellViewModel.TabletShellViewModel` (src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs:23)

### EditorState (class)

`T:PressToShape.App.Workspace.EditorState`. State shared by all regions of the editor shell: the document, its undo history, active tool, zoom and selection summary. View models observe it through PropertyChanged, so every route that changes a value stays in sync. This class is the only place that changes the document: edits go through PressToShape.App.Workspace.EditorState.Edit, a drag through PressToShape.App.Workspace.EditorState.Preview and PressToShape.App.Workspace.EditorState.Commit, and a new or opened document through PressToShape.App.Workspace.EditorState.ReplaceDocument.

- used by: `CanvasAreaViewModel`, `CanvasInteraction`, `DocumentFileService`, `DocumentSession`, `HistoryViewModel`, `InspectorViewModel`, `LayersViewModel`, `MenuBarViewModel`, `ObjectEditor`, `PaletteViewModel`, `PropertiesViewModel`, `SelectionEditor`, `StatusBarViewModel`, `TabletToolColumnViewModel`, `ToolGroupViewModel`, `ToolbarViewModel`, `ToolboxViewModel`, `ZoomViewModel`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:29), `ShapeOperationBenchmarks.Setup` (tests/PressToShape.Benchmarks/ShapeOperationBenchmarks.cs:44)

### FileMenuCommands (class)

`T:PressToShape.App.ViewModels.Editor.FileMenuCommands`. What the File menu items do. OpenRecent creates the command for one entry of the recent list.

- used by: `MenuBarViewModel`, `OverflowMenuViewModel`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:38)

### FileServices (class)

`T:PressToShape.App.Services.FileServices`. What the file actions need from the outside: settings, files, file pickers and message dialogs.

- used by: `DesktopShellViewModel`, `EditorShellViewModel`, `TabletShellViewModel`
- created at `AppLayout.Choose` (src/PressToShape.App/AppLayout.cs:33), `AppLayout.CreateTabletShell` (src/PressToShape.App/AppLayout.cs:66)

### GroupShape (class)

`T:PressToShape.Core.Shapes.GroupShape`. A group of shapes that is selected, moved and styled as one. Children keep their own document coordinates, so a group has no transform. Its box is the union of the rotated bounds of its children, always upright (PressToShape.Core.Shapes.Shape.Rotation is 0), and follows PressToShape.Core.Shapes.GroupShape.Children by itself. Do not set the box, the rotation or the appearance of a group: change its children. Immutable: change it with with.

- used by: `ShapeLocation`
- created at `GroupOperations.Group` (src/PressToShape.Core/Documents/GroupOperations.cs:43), `ProjectFile.BuildGroup` (src/PressToShape.Core/IO/ProjectFile.cs:214)

### IAppShutdown (interface)

`T:PressToShape.App.Services.IAppShutdown`. Lets view models close the application without referencing Avalonia types.

- used by: `DesktopShellViewModel`, `MenuBarViewModel`, `TabletShellViewModel`
- implemented by `DelegateAppShutdown`, created at `AppLayout.CreateTabletShell` (src/PressToShape.App/AppLayout.cs:72)
- implemented by `DesktopAppShutdown`, created at `AppLayout.Choose` (src/PressToShape.App/AppLayout.cs:40)

### ICatalogSource (interface)

`T:PressToShape.App.Localization.ICatalogSource`. Finds the translation catalog of a language. English has no catalog, because the source text is English.

- used by: `Localizer`
- implemented by `EmbeddedCatalogSource`, created at `EmbeddedCatalogSource.EmbeddedCatalogSource` (src/PressToShape.App/Localization/EmbeddedCatalogSource.cs:20), `PressToShapeApp.Initialize` (src/PressToShape.App/PressToShapeApp.axaml.cs:27)
- implemented by `NoCatalogSource`, created at `LocalizationHost.Localizer` (src/PressToShape.App/Localization/LocalizationHost.cs:14)

### IFileDialogs (interface)

`T:PressToShape.App.Services.IFileDialogs`. The file pickers. Each returns a local file path, or null if the user cancelled.

- used by: `DocumentFileService`, `FileServices`
- implemented by `AvaloniaFileDialogs`, created at `AppLayout.Choose` (src/PressToShape.App/AppLayout.cs:36), `AppLayout.CreateTabletShell` (src/PressToShape.App/AppLayout.cs:69)

### IFileStore (interface)

`T:PressToShape.App.Services.IFileStore`. Reads and writes whole files. Lets the file actions be tested without a real file system, and lets a platform without file paths supply its own storage.

- used by: `DocumentFileService`, `FileServices`
- implemented by `LocalFileStore`, created at `AppLayout.Choose` (src/PressToShape.App/AppLayout.cs:35)
- implemented by `StorageFileStore`, created at `AppLayout.CreateTabletShell` (src/PressToShape.App/AppLayout.cs:68)

### ILocalizer (interface)

`T:PressToShape.App.Localization.ILocalizer`. Translates user-visible texts. The English source text is the message id.

- used by: `AvaloniaFileDialogs`, `AvaloniaMessageDialogs`, `DesktopShellViewModel`, `DocumentFileService`, `EditorShellViewModel`, `HistoryItemViewModel`, `HistoryViewModel`, `InspectorViewModel`, `LanguagePreference`, `LayersViewModel`, `LocalizedTextObservable`, `LocalizedViewModel`, `MenuBarViewModel`, `MenuItemViewModel`, `NumberFieldViewModel`, `OverflowMenuViewModel`, `OverlayMessageDialogs`, `PanelButtonViewModel`, `PlaceholderPanelViewModel`, `PropertiesViewModel`, `StatusBarViewModel`, `TabletColorSwatchViewModel`, `TabletPanelViewModel`, `TabletShellViewModel`, `TabletToolColumnViewModel`, `TabletToolbarViewModel`, `ToolGroupViewModel`, `ToolItemViewModel`, `ToolbarButtonViewModel`, `ToolbarViewModel`, `ToolboxViewModel`, `ZoomViewModel`
- implemented by `Localizer`, created at `LocalizationHost.Localizer` (src/PressToShape.App/Localization/LocalizationHost.cs:14), `PressToShapeApp.Initialize` (src/PressToShape.App/PressToShapeApp.axaml.cs:26)

### IMessageDialogs (interface)

`T:PressToShape.App.Services.IMessageDialogs`. The questions and messages shown to the user. The texts are translated by the caller.

- used by: `DocumentFileService`, `FileServices`
- implemented by `AvaloniaMessageDialogs`, created at `AppLayout.Choose` (src/PressToShape.App/AppLayout.cs:37)
- implemented by `OverlayMessageDialogs`, created at `AppLayout.CreateTabletShell` (src/PressToShape.App/AppLayout.cs:70)

### ISettingsStore (interface)

`T:PressToShape.App.Settings.ISettingsStore`. Loads and saves PressToShape.App.Settings.AppSettings. Neither operation throws.

- used by: `FileServices`, `LanguagePreference`, `RecentFiles`
- implemented by `JsonFileSettingsStore`, created at `PressToShapeApp.Initialize` (src/PressToShape.App/PressToShapeApp.axaml.cs:25)

### InspectorViewModel (class)

`T:PressToShape.App.ViewModels.Editor.InspectorViewModel`. State of the inspector: the selected tab and the view models of its tabs.

- used by: `TabletPanelViewModel`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:37)

### Layer (class)

`T:PressToShape.Core.Documents.Layer`. A named group of shapes that can be hidden or locked. Immutable: change it with with. Records compare PressToShape.Core.Documents.Layer.Shapes by reference, not by content.

- used by: `ShapeLocation`
- created at `Document.Create` (src/PressToShape.Core/Documents/Document.cs:61), `ProjectFile.Build` (src/PressToShape.Core/IO/ProjectFile.cs:130)

### LayersViewModel (class)

`T:PressToShape.App.ViewModels.Editor.LayersViewModel`. State of the Layers tab: the layers of the document as header rows, and below each layer its shapes and groups as a tree, topmost first. PressToShape.App.ViewModels.Editor.LayersViewModel.Rows is the flat list that the view shows: the rows whose groups and layer are expanded. The list follows the document. When only names, flags or the selection changed, the existing rows are updated, so the list is not rebuilt under the user's click, and which groups are open survives undo and redo as far as the groups still exist.

- used by: `ObjectRowViewModel`
- created at `InspectorViewModel.InspectorViewModel` (src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs:22)

### MessageDialogHost (class)

`T:PressToShape.App.ViewModels.Tablet.MessageDialogHost`. Shows message dialogs inside the layout, for a platform without windows. The layout shows PressToShape.App.ViewModels.Tablet.MessageDialogHost.Current over everything else while it is not null. Only one dialog is shown at a time: a second request waits until the first is closed.

- used by: `OverlayMessageDialogs`, `TabletShellViewModel`
- created at `AppLayout.CreateTabletShell` (src/PressToShape.App/AppLayout.cs:64)

### Mixed (class)

`T:PressToShape.App.Workspace.Mixed`1`. A value that is the same for every selected shape, or marked as mixed when the shapes differ.

- used by: `SelectionValues`
- created at `Mixed.Of` (src/PressToShape.App/Workspace/SelectionValues.cs:31), `Mixed.Of` (src/PressToShape.App/Workspace/SelectionValues.cs:35)

### ObjectEditor (class)

`T:PressToShape.App.Workspace.ObjectEditor`. Edits of the structure of the drawing: group and ungroup, arrange, lock and hide single objects, and names. Each change of the document is one history step. A command that would change nothing records no step, and its Can... property is false.

- used by: `InspectorViewModel`, `LayersViewModel`, `PropertiesViewModel`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:35), `InspectorViewModel.InspectorViewModel` (src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs:22), `PropertiesViewModel.PropertiesViewModel` (src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs:41)

### ObjectMenuCommands (class)

`T:PressToShape.App.ViewModels.Editor.ObjectMenuCommands`. What the Group, Arrange, Lock and Hide items of the Object menu do. A command is disabled while it would change nothing.

- used by: `MenuBarViewModel`, `OverflowMenuViewModel`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:55)

### OverflowMenuViewModel (class)

`T:PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel`. The "..." menu of the tablet toolbar: the file actions, the edit actions, group, arrange, lock and hide, and the language. It uses the same texts as the desktop menus (so the translations are shared) and the same commands, shown without Alt keys and shortcut texts through PressToShape.App.ViewModels.Desktop.MenuItemViewModel.PlainHeader. There is no Quit (an Android app is left with the Back button) and no list of recent files (their references do not survive a restart). An item without a command, or whose command cannot run, is disabled.

- used by: `TabletToolbarViewModel`
- created at `TabletShellViewModel.TabletShellViewModel` (src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs:28)

### PropertiesViewModel (class)

`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`. The Properties tab: the position, size, rotation and appearance of the selected shapes, and the section expansion. The fields show the current values (updated in place while shapes are dragged or edited elsewhere) and every committed edit is one undo step.

- used by: `TabletColorSwatchViewModel`
- created at `InspectorViewModel.InspectorViewModel` (src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs:21)

### RecentFiles (class)

`T:PressToShape.App.Workspace.RecentFiles`. The list of recently used files, most recent first, kept in the settings file. It holds each path once and at most PressToShape.App.Workspace.RecentFiles.MaxCount paths. The stored value is untrusted: anything that is not a list of paths is ignored. A list that does not persist stays empty: the paths of a platform whose file references do not survive a restart are of no use later.

- used by: `DocumentFileService`, `MenuBarViewModel`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:31)

### ScreenPoint (class)

`T:PressToShape.App.Workspace.ScreenPoint`. A point in screen pixels.

- used by: `ScreenHandle`, `ScreenLine`
- created at `ScreenPolygon.FromBox` (src/PressToShape.App/Workspace/ScreenPolygon.cs:19), `SelectionOverlay.Build` (src/PressToShape.App/Workspace/SelectionOverlay.cs:98)

### ScreenPolygon (class)

`T:PressToShape.App.Workspace.ScreenPolygon`. A closed shape of straight lines in screen pixels, for example the turned outline of a shape. Always four points here.

- used by: `SelectionOverlay`
- created at `ScreenPolygon.FromBox` (src/PressToShape.App/Workspace/ScreenPolygon.cs:22)

### SelectionEditor (class)

`T:PressToShape.App.Workspace.SelectionEditor`. Edits that work on the selected shapes and do not need the mouse: select all, deselect, copy, cut, paste, duplicate (and, in the same class, nudging and property edits). Every change of the document is one history step.

- used by: `CanvasAreaViewModel`, `InspectorViewModel`, `PropertiesViewModel`
- created at `CanvasAreaViewModel.CanvasAreaViewModel` (src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs:42), `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:34), `InspectorViewModel.InspectorViewModel` (src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs:21), `ShapeOperationBenchmarks.Setup` (tests/PressToShape.Benchmarks/ShapeOperationBenchmarks.cs:46)

### Shape (class)

`T:PressToShape.Core.Shapes.Shape`. A drawable shape with a position, a size and an appearance. Immutable: change it with with. Values are validated on creation and on with.

- used by: `Found`, `Node`, `ShapeLocation`
- created at no creation site found

### StorageFileRegistry (class)

`T:PressToShape.App.Services.StorageFileRegistry`. Remembers the files that the platform's file picker returned, and hands out a text reference for each. The rest of the app works with file paths as text, but the Android picker gives no path, only a handle. A reference looks like storage:<id>/<file name>, so code that shows "the name after the last slash" shows the real file name. References only live as long as the app runs.

- used by: `AvaloniaFileDialogs`, `StorageFileStore`
- created at `AppLayout.CreateTabletShell` (src/PressToShape.App/AppLayout.cs:63)

### TabletColorSwatchViewModel (class)

`T:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`. The small color swatch at the bottom of the tablet tool column: the fill in the middle and the stroke as a ring around it. It shows what the Properties panel shows (the color of the selection, or of the next shape when nothing is selected) by reading the fill and stroke fields of the PressToShape.App.ViewModels.Editor.PropertiesViewModel, so there is one source of truth. A tap opens the Properties panel at its Appearance section, where the colors are edited.

- used by: `TabletToolColumnViewModel`
- created at `TabletShellViewModel.TabletShellViewModel` (src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs:30)

### TabletPanelViewModel (class)

`T:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`. The panel of the tablet layout, which floats over the canvas and shows one of the inspector's tabs (Properties, Layers or History) while it is open. A toolbar icon toggles it. The shown tab is kept in PressToShape.App.ViewModels.Editor.InspectorViewModel.SelectedTab, as on the desktop. There is no Assets panel on the tablet until Assets has content.

- used by: `PanelButtonViewModel`, `TabletColorSwatchViewModel`, `TabletToolbarViewModel`
- created at `TabletShellViewModel.TabletShellViewModel` (src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs:29)

### ToolDefinition (class)

`T:PressToShape.App.Workspace.ToolDefinition`. Display data for one tool: name, optional single-key shortcut and icon.

- used by: `ToolItemViewModel`
- created at `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:13), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:14), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:15), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:16), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:17), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:18), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:19), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:20), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:21), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:22), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:23), `ToolDefinitions.All` (src/PressToShape.App/Workspace/ToolDefinitions.cs:24)

### ToolGroupDefinition (class)

`T:PressToShape.App.Workspace.ToolGroupDefinition`. Tools that share one icon in the tablet tool column. The first tool is the one the group starts with.

- used by: `ToolGroupViewModel`
- created at `ToolGroups.All` (src/PressToShape.App/Workspace/ToolGroups.cs:11), `ToolGroups.All` (src/PressToShape.App/Workspace/ToolGroups.cs:12), `ToolGroups.All` (src/PressToShape.App/Workspace/ToolGroups.cs:13), `ToolGroups.All` (src/PressToShape.App/Workspace/ToolGroups.cs:14), `ToolGroups.All` (src/PressToShape.App/Workspace/ToolGroups.cs:15), `ToolGroups.All` (src/PressToShape.App/Workspace/ToolGroups.cs:16)

### ToolboxViewModel (class)

`T:PressToShape.App.ViewModels.Editor.ToolboxViewModel`. State of the tool column: tool list, active tool, shortcut lookup, and the (not yet usable) color swatch commands.

- used by: `TabletToolColumnViewModel`, `ToolGroupViewModel`
- created at `EditorShellViewModel.EditorShellViewModel` (src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:33)

### ZoomViewModel (class)

`T:PressToShape.App.ViewModels.Editor.ZoomViewModel`. The zoom as the user sees it: the text, the preset list and the commands. Shared by the desktop status bar and the tablet toolbar.

- used by: `TabletToolbarViewModel`
- created at `StatusBarViewModel.StatusBarViewModel` (src/PressToShape.App/ViewModels/Desktop/StatusBarViewModel.cs:22), `TabletShellViewModel.TabletShellViewModel` (src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs:31)

## Container registrations

no container registrations found; services are created by hand in `AppLayout.Choose`, `AppLayout.CreateTabletShell`, `CanvasAreaViewModel.CanvasAreaViewModel`, `DesktopShellViewModel.DesktopShellViewModel`, `Document.Create`, `EditorShellViewModel.EditorShellViewModel`, `EmbeddedCatalogSource.EmbeddedCatalogSource`, `GroupOperations.Group`, `InspectorViewModel.InspectorViewModel`, `LocalizationHost.Localizer`, `Mixed.Of`, `PressToShapeApp.Initialize`, `ProjectFile.Build`, `ProjectFile.BuildGroup`, `PropertiesViewModel.PropertiesViewModel`, `ScreenPolygon.FromBox`, `SelectionOverlay.Build`, `ShapeOperationBenchmarks.Setup`, `StatusBarViewModel.StatusBarViewModel`, `TabletShellViewModel.TabletShellViewModel`, `ToolDefinitions.All`, `ToolGroups.All`.
