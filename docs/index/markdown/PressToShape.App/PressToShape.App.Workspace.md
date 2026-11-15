# PressToShape.App.Workspace

Project: PressToShape.App

## CanvasCursorType (enum)

`internal enum CanvasCursorType`

Source: `src/PressToShape.App/Workspace/CanvasCursorType.cs:4-38`

The pointer shape the canvas should show. The view turns it into a real cursor.

**Members**

- `public Arrow` - The normal arrow, for the Select tool and tools that do not use the canvas yet.
- `public Crosshair` - A crosshair, for the shape tools that draw by dragging.
- `public OpenHand` - An open hand, for the Hand tool.
- `public ClosedHand` - A closed hand, while the view is being dragged.
- `public Magnifier` - A magnifier, for the Zoom tool.
- `public MagnifierMinus` - A magnifier with a minus, for the Zoom tool while Alt is held, because a click then zooms out.
- `public ResizeNorthSouth` - A double arrow pointing up and down, for the top and bottom handles.
- `public ResizeWestEast` - A double arrow pointing left and right, for the left and right handles.
- `public ResizeNorthWestSouthEast` - A diagonal double arrow from top left to bottom right.
- `public ResizeNorthEastSouthWest` - A diagonal double arrow from top right to bottom left.
- `public Rotate` - A turning arrow, for the rotate handle.

## CanvasInteraction (class)

`internal sealed class CanvasInteraction`

Source: `src/PressToShape.App/Workspace/CanvasInteraction.cs:15-378`

What the mouse does on the canvas: draw rectangles and ellipses, select, move and delete shapes. Works on document positions and edits the document through PressToShape.App.Workspace.EditorState. It has no UI types. A drag that is in progress ends with PressToShape.App.Workspace.CanvasInteraction.PointerReleased or PressToShape.App.Workspace.CanvasInteraction.Cancel.

**Members**

- `public CanvasInteraction(EditorState state)` - _(no documentation)_
- `public bool IsDragging { get; }` - Whether a drag is in progress.
- `public HandleType? DraggedHandle { get; }` - The handle that is being dragged, or null if no resize or rotate drag is in progress.
- `public void PointerPressed(double x, double y, bool shift = false)` - The primary button went down at this document position. shift adds to or removes from the selection.
- `public void PointerMoved(double x, double y, bool shift = false, bool alt = false)` - The pointer moved to this document position. Does nothing without a drag.
- `public void PointerReleased(double x, double y, bool shift = false, bool alt = false)` - The button went up at this document position. Ends the drag in every case.
- `public void Cancel()` - Ends a drag without a result: the draft is dropped and a moved shape keeps the position it has, which then counts as one step if the shape moved.
- `public void DeleteSelection()` - Removes all selected shapes from the document as one step. Does nothing without a selection.
- `public HandleType? HoverHandle(double x, double y)` - The handle under this document position, for the cursor. null during a drag, for other tools than Select, without a selection, or away from the handles.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.EditorState`

## CanvasKey (enum)

`internal enum CanvasKey`

Source: `src/PressToShape.App/Workspace/CanvasKey.cs:4-11`

The keys the canvas reacts to. The view turns real key events into these, so the view model needs no UI types.

**Members**

- `public Left` - _(no documentation)_
- `public Right` - _(no documentation)_
- `public Up` - _(no documentation)_
- `public Down` - _(no documentation)_
- `public Escape` - _(no documentation)_

## CanvasMapping (class)

`internal static class CanvasMapping`

Source: `src/PressToShape.App/Workspace/CanvasMapping.cs:7-19`

The one mapping between screen pixels and document units. Drawing and picking both use it, so a shape is always hit where it is drawn.

**Members**

- `public double ToDocument(double screen, double origin, double zoomPercent)` - Converts a screen coordinate to a document coordinate.
- `public double ToScreen(double value, double origin, double zoomPercent)` - Converts a document coordinate to a screen coordinate.

## CanvasScene (class)

`internal static class CanvasScene`

Source: `src/PressToShape.App/Workspace/CanvasScene.cs:11-70`

Turns a document into the shapes to draw on the canvas, so the drawing control needs no geometry rules.

**Members**

- `public IReadOnlyList<ScreenShape> Build(Document document, double zoomPercent, double originX, double originY)` - Builds the shapes in drawing order: layers bottom first, shapes bottom first, so later shapes cover earlier ones. Hidden layers are left out. Locked layers are drawn like any other.
- `public ScreenShape BuildShape(Shape shape, double zoomPercent, double originX, double originY)` - Converts one shape, for example a shape that is still being drawn and is not in a document.

## CursorImages (class)

`internal static class CursorImages`

Source: `src/PressToShape.App/Workspace/CursorImages.cs:11-84`

The facts about the cursor images of the canvas: which size fits a screen scale, the file name of each image and its active point. Pure logic. The images are made by tools/PressToShape.CursorImages and committed under Assets/Cursors.

**Members**

- `public IReadOnlyList<int> Sizes { get; }` - The image sizes in pixels, ascending. The images are square.
- `public int SizeFor(double scale)` - The image size for a screen scale (1.0 is 100%). A scale that is not a finite positive number counts as 100%, because a wrong size is better than no cursor.
- `public string? FileName(CanvasCursorType type, int size)` - The file name of the image for a cursor type and size, or null for the arrow, which stays the system cursor.
- `public (int X, int Y) Hotspot(CanvasCursorType type, int size)` - The active point of the image in its own pixels. It is the center of the image, except for the magnifier, where it is the center of the lens. The point at 64 px is exactly double the point at 32 px.

## DocumentFileService (class)

`internal sealed class DocumentFileService`

Source: `src/PressToShape.App/Workspace/DocumentFileService.cs:17-244`

The file actions of the File menu: New, Open, Open Recent, Save, Save As, Export as SVG, and the question what to do with unsaved changes. It works through interfaces for files, dialogs and messages, so it is tested without a UI. Reading, parsing, serializing and writing run off the calling (UI) context. State changes happen on the calling context.

**Members**

- `public DocumentFileService(EditorState state, DocumentSession session, RecentFiles recent, IFileStore files, IFileDialogs dialogs, IMessageDialogs messages, ILocalizer localizer, Action fitToWindow)` - _(no documentation)_
- `public Task<bool> ConfirmDiscardAsync()` - Makes sure unsaved changes may be dropped: returns true without a question if there are none. Otherwise it asks. Save returns true only if the save worked, Don't save returns true, Cancel returns false.
- `public Task NewAsync()` - Replaces the document by an empty one with a single layer, after the unsaved-changes question.
- `public Task OpenAsync()` - Asks for a project file and opens it, after the unsaved-changes question.
- `public Task OpenRecentAsync(string path)` - Opens a file from the recent list, after the unsaved-changes question.
- `public Task<bool> SaveAsync()` - Saves to the document's file, or asks for one if it has none. Returns whether the document was saved.
- `public Task<bool> SaveAsAsync()` - Asks for a file and saves to it. Returns whether the document was saved (false if cancelled or failed).
- `public Task ExportSvgAsync()` - Asks for a file and writes the SVG export to it. The document's file and unsaved state do not change.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.IFileDialogs`
- Depends on (constructor) `PressToShape.App.Services.IFileStore`
- Depends on (constructor) `PressToShape.App.Services.IMessageDialogs`
- Depends on (constructor) `PressToShape.App.Workspace.DocumentSession`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.RecentFiles`

## DocumentSession (class)

`internal sealed class DocumentSession : ObservableObject`

Source: `src/PressToShape.App/Workspace/DocumentSession.cs:13-66`

Which file the open document belongs to and whether it has unsaved changes. Documents are immutable, so "unsaved" means: the editor's document is no longer the instance that was last saved or opened. A change that is changed back still counts as unsaved.

**Members**

- `public DocumentSession(EditorState state)` - _(no documentation)_
- `public string? Path { get; }` - The file of the document, or null if it has none yet.
- `public string DisplayName { get; }` - The file name without its folder, or an empty text without a file.
- `public bool IsDirty { get; }` - Whether the document changed since it was last saved or opened.
- `public void Reset(string? path, Document savedDocument)` - Starts a new session state: the given document, which the editor now shows, counts as saved.
- `public void MarkSaved(string path, Document savedDocument)` - Records a successful save. Pass the instance that was written: if the document changed while it was written, the session stays dirty.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Injected into `PressToShape.App.Workspace.DocumentFileService`

## EditorState (class)

`internal partial sealed class EditorState : ObservableObject`

Source: `src/PressToShape.App/Workspace/EditorState.cs:18-400`

State shared by all regions of the editor shell: the document, its undo history, active tool, zoom and selection summary. View models observe it through PropertyChanged, so every route that changes a value stays in sync. This class is the only place that changes the document: edits go through PressToShape.App.Workspace.EditorState.Edit, a drag through PressToShape.App.Workspace.EditorState.Preview and PressToShape.App.Workspace.EditorState.Commit, and a new or opened document through PressToShape.App.Workspace.EditorState.ReplaceDocument.

**Members**

- `public EditorState(Document? document = null)` - Creates the state. Without a document it starts with an empty one: Core has no default layer name, the shell passes the real start document.
- `public EventHandler? HistoryChanged` - Raised after the undo history changed: a step was recorded, undone or redone, or the history was reset.
- `public DocumentHistory History { get; }` - The undo history of the current document.
- `public bool IsDragInProgress { get; set; }` - _(no documentation)_
- `public bool CanUndo { get; }` - Whether a step can be undone right now.
- `public bool CanRedo { get; }` - Whether a step can be redone right now.
- `public Document Document { get; set; }` - _(no documentation)_
- `public Guid ActiveLayerId { get; set; }` - _(no documentation)_
- `public ImmutableArray<Guid> SelectedShapeIds { get; private set; }` - _(no documentation)_
- `public Guid? SelectedShapeId { get; }` - The id of the selected shape if exactly one shape is selected, otherwise null.
- `public int SelectedCount { get; }` - How many shapes are selected.
- `public bool HasSelection { get; }` - Whether at least one shape is selected.
- `public Shape? Draft { get; set; }` - _(no documentation)_
- `public Tool ActiveTool { get; set; }` - _(no documentation)_
- `public ShapeClipboard Clipboard { get; }` - The shapes that were copied or cut. Kept by the app, nothing goes to the system clipboard.
- `public SelectionBox? SelectionBox { get; set; }` - _(no documentation)_
- `public double ArtboardWidth { get; }` - Artboard width of the current document.
- `public double ArtboardHeight { get; }` - Artboard height of the current document.
- `public void Edit(HistoryStepType type, Func<Document, Document> change, bool extend = false)` - Changes the document and records the change as one history step. If change returns the same document instance, nothing changed and no step is recorded.
- `public void UpdateLayer(Guid layerId, Func<Layer, Layer> change, HistoryStepType type)` - Replaces one layer of the document by the result of change, as one history step.
- `public void Preview(Document document)` - Sets the document without recording a step, for a change that is still going on such as a drag. End it with PressToShape.App.Workspace.EditorState.Commit.
- `public void Commit(HistoryStepType type)` - Records the current document as one step, if it differs from the last recorded one.
- `public void ReplaceDocument(Document document)` - Shows a different document (New, Open) and starts a fresh history.
- `public void Undo()` - Goes one step back. Does nothing if there is no step or a drag is in progress.
- `public void Redo()` - Goes one step forward. Does nothing if there is no step or a drag is in progress.
- `public void JumpToStep(int index)` - Goes to the state at index (0 is the start state). Does nothing for the current or an invalid index, or while a drag is in progress.
- `public double PanX { get; private set; }` - _(no documentation)_
- `public double PanY { get; private set; }` - _(no documentation)_
- `public double ViewportWidth { get; private set; }` - _(no documentation)_
- `public double ViewportHeight { get; private set; }` - _(no documentation)_
- `public double ZoomPercent { get; set; }` - Zoom in percent. Values outside the allowed range are clamped. Setting it keeps the middle of the canvas area where it is.
- `public void SetViewport(double width, double height)` - Tells the state how large the canvas area is. The view offset is limited again for the new size.
- `public void SetPan(double x, double y)` - Moves the view to this offset, limited so that the artboard stays visible. A non-finite value leaves that axis unchanged.
- `public void PanBy(double dx, double dy)` - Moves the view by this distance, see PressToShape.App.Workspace.EditorState.SetPan.
- `public void ZoomAround(double percent, double anchorX, double anchorY)` - Changes the zoom so that the point of the drawing under the screen position (anchorX, anchorY) of the canvas area stays under it. The zoom stays in its range, and the view offset stays limited.
- `public void Select(Guid? shapeId)` - Selects a shape, or clears the selection with null.
- `public void SelectOnly(Guid shapeId)` - Makes this shape the only selected shape. Does nothing useful for an id that cannot be selected: the selection is then cleared.
- `public void SetSelection(IEnumerable<Guid> shapeIds)` - Replaces the selection by these shapes, keeping their order. Ids that are unknown, repeated, or in a hidden or locked layer are left out.
- `public void ToggleSelected(Guid shapeId)` - Adds the shape to the selection, or removes it if it is selected. A shape that cannot be selected is ignored.
- `public void ClearSelection()` - Clears the selection.
- `public void SetZoom(double percent)` - _(no documentation)_
- `public void ZoomIn()` - _(no documentation)_
- `public void ZoomOut()` - _(no documentation)_
- `public void ZoomToActualSize()` - _(no documentation)_
- `public void ZoomToFit(double viewportWidth, double viewportHeight)` - Sets the zoom so the artboard fits the viewport and centers the artboard (the view offset becomes zero). The zoom stays unchanged if the viewport has no size yet.

**Relationships**

- Depends on (constructor) `PressToShape.Core.Documents.Document`
- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.StatusBarViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.ToolbarViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.HistoryViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.InspectorViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.LayerItemViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.LayersViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.PropertiesViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.ToolboxViewModel`
- Injected into `PressToShape.App.Workspace.CanvasInteraction`
- Injected into `PressToShape.App.Workspace.DocumentFileService`
- Injected into `PressToShape.App.Workspace.DocumentSession`
- Injected into `PressToShape.App.Workspace.SelectionEditor`

## HandleType (enum)

`internal enum HandleType`

Source: `src/PressToShape.App/Workspace/HandleType.cs:4-15`

The handles around a selection. The eight resize handles sit on the frame, PressToShape.App.Workspace.HandleType.Rotate outside the top edge.

**Members**

- `public TopLeft` - _(no documentation)_
- `public Top` - _(no documentation)_
- `public TopRight` - _(no documentation)_
- `public Right` - _(no documentation)_
- `public BottomRight` - _(no documentation)_
- `public Bottom` - _(no documentation)_
- `public BottomLeft` - _(no documentation)_
- `public Left` - _(no documentation)_
- `public Rotate` - _(no documentation)_

## HistoryStepNames (class)

`internal static class HistoryStepNames`

Source: `src/PressToShape.App/Workspace/HistoryStepNames.cs:8-40`

The message ids (English texts) for the rows of the history list. Translate them with ILocalizer.T when shown.

**Members**

- `public string Start { get; }` - The first row of the list: the state before the first kept step.
- `public string MessageId(HistoryStepType type)` - The message id for the name of a step type.

## InspectorTab (enum)

`internal enum InspectorTab`

Source: `src/PressToShape.App/Workspace/InspectorTab.cs:4-10`

The tabs of the inspector panel.

**Members**

- `public Properties` - _(no documentation)_
- `public Layers` - _(no documentation)_
- `public Assets` - _(no documentation)_
- `public History` - _(no documentation)_

## Mixed<T> (record)

`internal readonly record Mixed<T> : IEquatable<Mixed<T>>`

Source: `src/PressToShape.App/Workspace/SelectionValues.cs:12-37`

A value that is the same for every selected shape, or marked as mixed when the shapes differ.

**Members**

- `public Mixed(T Value, bool IsMixed)` - A value that is the same for every selected shape, or marked as mixed when the shapes differ.
- `public bool IsMixed { get; init; }` - Whether the shapes have different values.
- `public T Value { get; init; }` - The shared value. For a mixed value it is the value of the first shape.
- `public Mixed<T> Of(IEnumerable<T> values)` - Combines the values of all shapes.

**Relationships**

- Injected into `PressToShape.App.Workspace.SelectionValues`

## RecentFiles (class)

`internal sealed class RecentFiles`

Source: `src/PressToShape.App/Workspace/RecentFiles.cs:13-87`

The list of recently used files, most recent first, kept in the settings file. It holds each path once and at most PressToShape.App.Workspace.RecentFiles.MaxCount paths. The stored value is untrusted: anything that is not a list of paths is ignored.

**Members**

- `public int MaxCount` - The most paths the list keeps.
- `public RecentFiles(ISettingsStore store)` - _(no documentation)_
- `public EventHandler? Changed` - Raised after the list changed.
- `public IReadOnlyList<string> Paths { get; }` - The paths, most recent first.
- `public void Add(string path)` - Puts a path at the top. An existing entry for the same path moves up. Blank paths are ignored.
- `public void Remove(string path)` - Removes a path. Does nothing if it is not in the list.

**Relationships**

- Depends on (constructor) `PressToShape.App.Settings.ISettingsStore`
- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Injected into `PressToShape.App.Workspace.DocumentFileService`

## RulerTick (record)

`internal readonly record RulerTick : IEquatable<RulerTick>`

Source: `src/PressToShape.App/Workspace/RulerTick.cs:11-11`

One ruler tick: its position along the ruler in screen pixels, an optional label (major ticks only) and its type.

**Members**

- `public RulerTick(double ScreenPosition, string? Label, TickType Type)` - One ruler tick: its position along the ruler in screen pixels, an optional label (major ticks only) and its type.
- `public string? Label { get; init; }` - _(no documentation)_
- `public double ScreenPosition { get; init; }` - _(no documentation)_
- `public TickType Type { get; init; }` - _(no documentation)_

## RulerTicks (class)

`internal static class RulerTicks`

Source: `src/PressToShape.App/Workspace/RulerTicks.cs:8-96`

Computes ruler ticks for a given zoom. Pure logic, independent of drawing.

**Members**

- `public double MinMajorSpacing` - Minimum distance between two major ticks on screen, in pixels.
- `public int MaxTickCount` - Upper limit for the number of ticks returned, to protect against huge lengths.
- `public IReadOnlyList<RulerTick> Compute(double zoomPercent, double originPx, double lengthPx)` - Computes the ticks that are visible on a ruler.

## ScreenHandle (record)

`internal readonly record ScreenHandle : IEquatable<ScreenHandle>`

Source: `src/PressToShape.App/Workspace/ScreenHandle.cs:4-4`

A handle to draw: its type, its center and its side length, all in screen pixels.

**Members**

- `public ScreenHandle(HandleType Type, ScreenPoint Center, double Size)` - A handle to draw: its type, its center and its side length, all in screen pixels.
- `public ScreenPoint Center { get; init; }` - _(no documentation)_
- `public double Size { get; init; }` - _(no documentation)_
- `public HandleType Type { get; init; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.ScreenPoint`

## ScreenLine (record)

`internal readonly record ScreenLine : IEquatable<ScreenLine>`

Source: `src/PressToShape.App/Workspace/ScreenPolygon.cs:10-10`

A line in screen pixels.

**Members**

- `public ScreenLine(ScreenPoint From, ScreenPoint To)` - A line in screen pixels.
- `public ScreenPoint From { get; init; }` - _(no documentation)_
- `public ScreenPoint To { get; init; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.ScreenPoint`

## ScreenPoint (record)

`internal readonly record ScreenPoint : IEquatable<ScreenPoint>`

Source: `src/PressToShape.App/Workspace/ScreenPolygon.cs:7-7`

A point in screen pixels.

**Members**

- `public ScreenPoint(double X, double Y)` - A point in screen pixels.
- `public double X { get; init; }` - _(no documentation)_
- `public double Y { get; init; }` - _(no documentation)_

**Relationships**

- Injected into `PressToShape.App.Workspace.ScreenHandle`
- Injected into `PressToShape.App.Workspace.ScreenLine`

## ScreenPolygon (record)

`internal sealed record ScreenPolygon : IEquatable<ScreenPolygon>`

Source: `src/PressToShape.App/Workspace/ScreenPolygon.cs:13-24`

A closed shape of straight lines in screen pixels, for example the turned outline of a shape. Always four points here.

**Members**

- `public ScreenPolygon(IReadOnlyList<ScreenPoint> Points)` - A closed shape of straight lines in screen pixels, for example the turned outline of a shape. Always four points here.
- `public IReadOnlyList<ScreenPoint> Points { get; init; }` - _(no documentation)_
- `public ScreenPolygon FromBox(double centerX, double centerY, double width, double height, double rotationDegrees)` - The corners of a box turned around its center, in the order top left, top right, bottom right, bottom left.

**Relationships**

- Injected into `PressToShape.App.Workspace.SelectionOverlay`

## ScreenRect (record)

`internal readonly record ScreenRect : IEquatable<ScreenRect>`

Source: `src/PressToShape.App/Workspace/ScreenRect.cs:4-4`

A rectangle in screen pixels, for example the outline of the selected shape.

**Members**

- `public ScreenRect(double X, double Y, double Width, double Height)` - A rectangle in screen pixels, for example the outline of the selected shape.
- `public double Height { get; init; }` - _(no documentation)_
- `public double Width { get; init; }` - _(no documentation)_
- `public double X { get; init; }` - _(no documentation)_
- `public double Y { get; init; }` - _(no documentation)_

## ScreenShape (record)

`internal readonly record ScreenShape : IEquatable<ScreenShape>`

Source: `src/PressToShape.App/Workspace/ScreenShape.cs:19-29`

A shape ready to draw: position, size and stroke width are already in screen pixels. PressToShape.App.Workspace.ScreenShape.Stroke is null when nothing should be stroked, which includes a stroke width of zero.

**Members**

- `public ScreenShape(ScreenShapeType Type, double X, double Y, double Width, double Height, Color? Fill, Color? Stroke, double StrokeWidth, double Opacity, double RotationDegrees = 0)` - A shape ready to draw: position, size and stroke width are already in screen pixels. PressToShape.App.Workspace.ScreenShape.Stroke is null when nothing should be stroked, which includes a stroke width of zero.
- `public ScreenShapeType Type { get; init; }` - Rectangle or ellipse.
- `public double X { get; init; }` - Left edge in screen pixels.
- `public double Y { get; init; }` - Top edge in screen pixels.
- `public double Width { get; init; }` - Width in screen pixels.
- `public double Height { get; init; }` - Height in screen pixels.
- `public Color? Fill { get; init; }` - Fill color, or null for no fill.
- `public Color? Stroke { get; init; }` - Stroke color, or null for no stroke.
- `public double StrokeWidth { get; init; }` - Stroke width in screen pixels. Zero when there is no stroke.
- `public double Opacity { get; init; }` - Opacity of the whole shape, 0 to 1.
- `public double RotationDegrees { get; init; }` - Rotation in degrees, clockwise, around the center of the box. 0 for an upright shape.

## ScreenShapeType (enum)

`internal enum ScreenShapeType`

Source: `src/PressToShape.App/Workspace/ScreenShapeType.cs:4-8`

The geometry a PressToShape.App.Workspace.ScreenShape is drawn with.

**Members**

- `public Rectangle` - _(no documentation)_
- `public Ellipse` - _(no documentation)_

## SelectionBox (record)

`internal readonly record SelectionBox : IEquatable<SelectionBox>`

Source: `src/PressToShape.App/Workspace/SelectionBox.cs:8-8`

A rectangle in document units. Used for the selection box that is dragged on empty space.

**Members**

- `public SelectionBox(double X, double Y, double Width, double Height)` - A rectangle in document units. Used for the selection box that is dragged on empty space.
- `public double Height { get; init; }` - Height, not negative.
- `public double Width { get; init; }` - Width, not negative.
- `public double X { get; init; }` - Left edge.
- `public double Y { get; init; }` - Top edge.

## SelectionEditor (class)

`internal sealed class SelectionEditor`

Source: `src/PressToShape.App/Workspace/SelectionEditor.cs:14-188`

Edits that work on the selected shapes and do not need the mouse: select all, deselect, copy, cut, paste, duplicate (and, in the same class, nudging and property edits). Every change of the document is one history step.

**Members**

- `public SelectionEditor(EditorState state, Func<Guid>? idSource = null)` - _(no documentation)_
- `public bool CanCopy { get; }` - Whether there is a selection to copy.
- `public bool CanCut { get; }` - Whether there is a selection to cut.
- `public bool CanDuplicate { get; }` - Whether there is a selection to duplicate.
- `public bool CanPaste { get; }` - Whether something can be pasted: the clipboard has shapes and the active layer is visible and not locked.
- `public void SelectAll()` - Selects every shape of every visible, unlocked layer.
- `public void Deselect()` - Clears the selection.
- `public void Copy()` - Puts the selected shapes on the clipboard. The document stays unchanged.
- `public void Cut()` - Copies the selected shapes and removes them from the document, as one step.
- `public void Paste(bool inPlace)` - Adds copies of the clipboard shapes to the top of the active layer, as one step, and selects them. Each paste moves the copies 10 units further right and down, unless inPlace is true.
- `public void Duplicate()` - Adds copies of the selected shapes, each at the top of its own layer, 10 units right and down, as one step, and selects them. The clipboard is not touched.
- `public void Nudge(double dx, double dy)` - Moves the selected shapes by the distance in document units, not limited to the artboard. Repeated nudges of the same selection become one history step, as long as the document was not changed by anything else in between.
- `public void Apply(HistoryStepType type, Func<Shape, Shape> change)` - Applies the change to every selected shape as one step. No step if no shape changed.
- `public void ApplyTransform(HistoryStepType type, Func<IReadOnlyList<Shape>, IReadOnlyList<Shape>> change)` - Replaces the selected shapes by the returned list, which must have the same shapes (same ids) in the same order, as one step. For changes that need all shapes together. No step if nothing changed.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Injected into `PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.InspectorViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.PropertiesViewModel`

## SelectionFrame (record)

`internal sealed record SelectionFrame : IEquatable<SelectionFrame>`

Source: `src/PressToShape.App/Workspace/SelectionFrame.cs:13-13`

The box around the selection, in document units: one shape gives its own (possibly turned) box, several shapes give the upright box around all of them.

**Members**

- `public SelectionFrame(double CenterX, double CenterY, double Width, double Height, double Rotation, bool IsSingle)` - The box around the selection, in document units: one shape gives its own (possibly turned) box, several shapes give the upright box around all of them.
- `public double CenterX { get; init; }` - Center, horizontal.
- `public double CenterY { get; init; }` - Center, vertical.
- `public double Height { get; init; }` - Height before turning.
- `public bool IsSingle { get; init; }` - Whether the frame belongs to exactly one shape.
- `public double Rotation { get; init; }` - Degrees, clockwise on the screen. Always 0 for several shapes.
- `public double Width { get; init; }` - Width before turning.

## SelectionGeometry (class)

`internal static class SelectionGeometry`

Source: `src/PressToShape.App/Workspace/SelectionGeometry.cs:8-142`

Where the selection frame and its handles are, and which handle a point hits. Pure functions, document units.

**Members**

- `public double RotateHandleDistance` - How far the rotate handle is from the top edge, in screen pixels.
- `public double GrabSize` - The size of the area that grabs a handle, in screen pixels.
- `public SelectionFrame? FrameOf(IReadOnlyList<Shape> shapes)` - The frame of the shapes, or null for none.
- `public (int X, int Y) Direction(HandleType handle)` - The direction of a handle from the frame center in frame units: (-1, -1) is the top left corner.
- `public (double X, double Y) HandlePosition(SelectionFrame frame, HandleType handle, double zoomPercent)` - The center of the handle in document units. The handles turn with the frame.
- `public HandleType? HitHandle(SelectionFrame frame, double x, double y, double zoomPercent)` - The handle whose grab area contains the document point, or null. The closest handle wins, then the priority order.
- `public double HandleDirectionDegrees(HandleType handle, double rotation)` - The direction in which a handle points on the screen, in degrees [0, 360): 0 is right, 90 is down. For choosing a cursor.

## SelectionOverlay (record)

`internal sealed record SelectionOverlay : IEquatable<SelectionOverlay>`

Source: `src/PressToShape.App/Workspace/SelectionOverlay.cs:17-101`

Everything the canvas draws for the selection, in screen pixels: an outline for every selected shape, the box and handles around the selection, the line to the rotate handle, and the box that is being dragged on empty space.

**Members**

- `public SelectionOverlay(IReadOnlyList<ScreenPolygon> Outlines, ScreenPolygon? Box, IReadOnlyList<ScreenHandle> Handles, ScreenLine? RotateLink, ScreenRect? SelectionBox)` - Everything the canvas draws for the selection, in screen pixels: an outline for every selected shape, the box and handles around the selection, the line to the rotate handle, and the box that is being dragged on empty space.
- `public IReadOnlyList<ScreenPolygon> Outlines { get; init; }` - One turned outline per selected shape. For every tool.
- `public ScreenPolygon? Box { get; init; }` - The frame around the selection (Select tool only).
- `public IReadOnlyList<ScreenHandle> Handles { get; init; }` - The eight resize handles and the rotate handle (Select tool only).
- `public ScreenLine? RotateLink { get; init; }` - The line from the middle of the top edge to the rotate handle (Select tool only).
- `public ScreenRect? SelectionBox { get; init; }` - The box being dragged, if any (Select tool only).
- `public double HandleSize` - The side of a handle in screen pixels.
- `public SelectionOverlay Empty { get; }` - An overlay without anything to draw.
- `public SelectionOverlay Build(Document document, IEnumerable<Guid> selectedIds, Tool tool, double zoomPercent, double originX, double originY, SelectionBox? selectionBox = null)` - Builds the overlay. Shapes of hidden or locked layers are not part of it, even if their ids are in the selection. Handles, the box, the link and the dragged box exist only for the Select tool; outlines exist for every tool.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.ScreenPolygon`

## SelectionQueries (class)

`internal static class SelectionQueries`

Source: `src/PressToShape.App/Workspace/SelectionQueries.cs:14-83`

Questions about which shapes can be selected and which shapes a selection box touches. A shape can be selected when it is in a visible, unlocked layer. Results are in document order: bottom layer first, and within a layer bottom shape first.

**Members**

- `public bool IsSelectable(Document document, Guid shapeId)` - Whether the document has a shape with this id in a visible, unlocked layer.
- `public IReadOnlyList<Shape> Selectable(Document document)` - All shapes that can be selected.
- `public IReadOnlyList<Shape> ShapesTouching(Document document, double x, double y, double width, double height)` - The selectable shapes whose axis-aligned bounds, after rotation, overlap the rectangle. Touching edges count.
- `public IReadOnlyList<Shape> SelectedShapes(Document document, IEnumerable<Guid> shapeIds)` - The shapes with these ids, in document order. Unknown ids are ignored.
- `public IReadOnlyList<Shape> SelectedSelectable(Document document, IEnumerable<Guid> shapeIds)` - The shapes with these ids that can be selected, in document order. One pass, however many ids.

## SelectionTransforms (class)

`internal static class SelectionTransforms`

Source: `src/PressToShape.App/Workspace/SelectionTransforms.cs:14-371`

The math of resizing, rotating and moving the selection and of the Properties fields. Pure functions: they return new shapes in the same order, never change their input, and return the input list itself when the input makes no sense (a number that is not finite, a handle that does not apply) or the result would not be finite. Resizing does not change strokes. A flip past the opposite edge keeps the rotation of a shape as it is.

**Members**

- `public double SnapDegrees` - The step of snapped rotation, in degrees.
- `public IReadOnlyList<Shape> Resize(IReadOnlyList<Shape> startShapes, SelectionFrame frame, HandleType handle, double pointerX, double pointerY, bool keepProportions, bool fromCenter)` - Resizes by dragging a handle to the pointer. A single shape scales in its turned frame, several shapes scale in the upright frame, each shape through the same scale. If a rotated shape is among several, the scale is uniform.
- `public IReadOnlyList<Shape> Rotate(IReadOnlyList<Shape> startShapes, SelectionFrame frame, double startX, double startY, double pointerX, double pointerY, bool snap)` - Rotates by dragging the rotate handle. A single shape gets the angle of the pointer around its center; with snap the resulting angle is a multiple of 15 degrees. Several shapes turn around the frame center by the angle the pointer moved (snapped in steps of 15 degrees), and each shape's own rotation changes by the same angle.
- `public IReadOnlyList<Shape> Move(IReadOnlyList<Shape> startShapes, double dx, double dy)` - Moves every shape by the distance.
- `public IReadOnlyList<Shape> SetPosition(IReadOnlyList<Shape> shapes, SelectionFrame frame, double? x, double? y)` - Moves the selection so the top left corner of its frame (the unturned box of a single shape, or the box around several) is at the given place. A null coordinate stays as it is.
- `public IReadOnlyList<Shape> SetSize(IReadOnlyList<Shape> shapes, SelectionFrame frame, double? width, double? height, bool keepProportions)` - Sets the width and height of the frame, keeping its top left corner in place (in the turned frame for a rotated single shape). A null value stays as it is. With keepProportions the other side follows the ratio of the frame. Several shapes scale together, uniformly if one of them is rotated. Negative sizes are treated as 0.
- `public IReadOnlyList<Shape> SetRotation(IReadOnlyList<Shape> shapes, SelectionFrame frame, double angle)` - Sets the rotation to the angle. A single shape gets the angle itself. Several shapes turn together around the frame center by the angle (their frame counts as 0 degrees).
- `public IReadOnlyList<Shape> ResetRotation(IReadOnlyList<Shape> shapes)` - Sets the rotation of every shape to 0, each around its own center.

## SelectionValues (record)

`internal sealed record SelectionValues : IEquatable<SelectionValues>`

Source: `src/PressToShape.App/Workspace/SelectionValues.cs:53-87`

The values the Properties tab shows for a selection. Position and size are those of the selection frame (one shape: its own box, several: the box around all of them, with rotation 0). Appearance values are mixed when the shapes differ.

**Members**

- `public SelectionValues(double X, double Y, double Width, double Height, double Rotation, bool IsSingle, Mixed<Color?> Fill, Mixed<Color?> Stroke, Mixed<double> StrokeWidth, Mixed<double> Opacity)` - The values the Properties tab shows for a selection. Position and size are those of the selection frame (one shape: its own box, several: the box around all of them, with rotation 0). Appearance values are mixed when the shapes differ.
- `public double X { get; init; }` - Left edge of the frame.
- `public double Y { get; init; }` - Top edge of the frame.
- `public double Width { get; init; }` - Width of the frame.
- `public double Height { get; init; }` - Height of the frame.
- `public double Rotation { get; init; }` - Rotation in degrees. 0 for several shapes.
- `public bool IsSingle { get; init; }` - Whether exactly one shape is selected.
- `public Mixed<Color?> Fill { get; init; }` - Fill color, null inside means no fill.
- `public Mixed<Color?> Stroke { get; init; }` - Stroke color, null inside means no stroke.
- `public Mixed<double> StrokeWidth { get; init; }` - Stroke width.
- `public Mixed<double> Opacity { get; init; }` - Opacity from 0 to 1.
- `public SelectionValues? From(IReadOnlyList<Shape> shapes)` - The values of the shapes, or null for an empty list.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.Mixed<T>`

## ShapeClipboard (class)

`internal sealed class ShapeClipboard`

Source: `src/PressToShape.App/Workspace/ShapeClipboard.cs:13-49`

The shapes that were copied or cut. A copy kept by the app: nothing goes to the system clipboard. The shapes are immutable, so the clipboard holds them as they were when they were copied.

**Members**

- `public double PasteStep` - The distance in document units that each paste moves the shapes further.
- `public EventHandler? Changed` - Raised when the content changes.
- `public IReadOnlyList<Shape> Shapes { get; }` - The copied shapes in document order.
- `public bool IsEmpty { get; }` - Whether there is nothing to paste.
- `public void Set(IEnumerable<Shape> shapes)` - Replaces the content. The next paste starts at the first offset again.
- `public void Clear()` - Empties the clipboard.
- `public double NextPasteOffset()` - The offset for the next paste: 10 units for the first paste, 20 for the second, and so on.

## TickType (enum)

`internal enum TickType`

Source: `src/PressToShape.App/Workspace/RulerTick.cs:3-8`

_(no documentation)_

**Members**

- `public Major` - _(no documentation)_
- `public Medium` - _(no documentation)_
- `public Minor` - _(no documentation)_

## Tool (enum)

`internal enum Tool`

Source: `src/PressToShape.App/Workspace/Tool.cs:4-18`

The drawing and editing tools shown in the tool column.

**Members**

- `public Select` - _(no documentation)_
- `public DirectSelect` - _(no documentation)_
- `public Pen` - _(no documentation)_
- `public Node` - _(no documentation)_
- `public Text` - _(no documentation)_
- `public Rectangle` - _(no documentation)_
- `public Ellipse` - _(no documentation)_
- `public Star` - _(no documentation)_
- `public Spiral` - _(no documentation)_
- `public Eyedropper` - _(no documentation)_
- `public Hand` - _(no documentation)_
- `public Zoom` - _(no documentation)_

## ToolDefinition (record)

`internal sealed record ToolDefinition : IEquatable<ToolDefinition>`

Source: `src/PressToShape.App/Workspace/ToolDefinition.cs:6-10`

Display data for one tool: name, optional single-key shortcut and icon.

**Members**

- `public ToolDefinition(Tool Tool, string Name, char? ShortcutKey, MaterialIconKind Icon)` - Display data for one tool: name, optional single-key shortcut and icon.
- `public MaterialIconKind Icon { get; init; }` - _(no documentation)_
- `public string Name { get; init; }` - _(no documentation)_
- `public char? ShortcutKey { get; init; }` - _(no documentation)_
- `public Tool Tool { get; init; }` - _(no documentation)_
- `public string ToolTip { get; }` - Tooltip text such as "Select (V)", or just the name when there is no shortcut.

**Relationships**

- Injected into `PressToShape.App.ViewModels.Editor.ToolItemViewModel`

## ToolDefinitions (class)

`internal static class ToolDefinitions`

Source: `src/PressToShape.App/Workspace/ToolDefinitions.cs:9-35`

The ordered list of tools, in the order shown in the mockup, with Illustrator-style shortcuts.

**Members**

- `public IReadOnlyList<ToolDefinition> All { get; }` - _(no documentation)_
- `public ToolDefinition Get(Tool tool)` - _(no documentation)_
- `public ToolDefinition? FindByShortcut(char key)` - Finds the tool for a shortcut key, ignoring case. Returns null for unknown keys.

## ViewGeometry (class)

`internal static class ViewGeometry`

Source: `src/PressToShape.App/Workspace/ViewGeometry.cs:10-85`

The math of the view over the drawing, on plain numbers in screen pixels: where the artboard is, how far the view may be moved, and what a zoom around a point does. The view offset (pan) is the distance of the artboard from its centered position. Non-finite input (devices can send it) never produces NaN or infinity.

**Members**

- `public double MinimumVisible` - The part of the artboard, in screen pixels, that must stay visible on each axis.
- `public double WheelStep` - The distance the view moves for a wheel delta of 1, in screen pixels.
- `public double WheelZoomFactor` - The zoom factor for a wheel delta of 1.
- `public double Origin(double viewport, double artboardScreen, double pan)` - The screen position of the artboard's edge on one axis. The artboard is centered, then moved by the offset.
- `public double PanLimit(double viewport, double artboardScreen)` - How far the offset may go in each direction, so that at least PressToShape.App.Workspace.ViewGeometry.MinimumVisible pixels of the artboard (or all of it, if it is smaller) stay in view. Without a known viewport (zero or less) there is no limit.
- `public double ClampPan(double pan, double viewport, double artboardScreen)` - Limits the offset to what PressToShape.App.Workspace.ViewGeometry.PanLimit allows. A non-finite offset becomes zero.
- `public double PanAfterZoom(double pan, double viewport, double oldZoom, double newZoom, double artboardDocumentSize, double anchor)` - The offset that keeps the document point under anchor (a screen position on this axis) at the same place when the zoom changes. The result is not limited yet, call PressToShape.App.Workspace.ViewGeometry.ClampPan with the new artboard size.
- `public double WheelZoom(double zoomPercent, double deltaY)` - The zoom after a wheel turn: zoom * 1.1^deltaY, kept in the zoom range. Non-finite input leaves the zoom unchanged.

## ZoomLevels (class)

`internal static class ZoomLevels`

Source: `src/PressToShape.App/Workspace/ZoomLevels.cs:6-72`

Zoom limits, preset steps and fit calculation. Pure logic, values are percent.

**Members**

- `public double Min` - _(no documentation)_
- `public double Max` - _(no documentation)_
- `public double Default` - _(no documentation)_
- `public double[] Presets` - Preset zoom steps in percent, ascending. Used by zoom in, zoom out and the zoom drop-down.
- `public double Clamp(double percent)` - _(no documentation)_
- `public double NextIn(double current)` - The smallest preset above the current zoom, or PressToShape.App.Workspace.ZoomLevels.Max if there is none.
- `public double NextOut(double current)` - The largest preset below the current zoom, or PressToShape.App.Workspace.ZoomLevels.Min if there is none.
- `public double? FitToViewport(double viewportWidth, double viewportHeight, double artboardWidth, double artboardHeight, double margin = 24)` - The zoom at which the artboard fits into the viewport with a margin on every side. Returns null when any size is not positive, so callers can leave the zoom unchanged.
