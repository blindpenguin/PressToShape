# PressToShape.App.Workspace

Project: PressToShape.App

## BackRequestGuard (class)

`internal sealed class BackRequestGuard`

Source: `src/PressToShape.App/Workspace/BackRequestGuard.cs:9-30`

On Android one press of the Back key arrives twice: as the Escape key, which closes an open menu or flyout by itself, and as the back request. If the Escape half already closed something, the back request must not do more (it would go on to ask about unsaved changes). The layout tells this guard when a menu or flyout closed; a back request right after that (within PressToShape.App.Workspace.BackRequestGuard.WindowMs) is the second half of the same press. The caller passes the time, so tests need no real time.

**Members**

- `public long WindowMs` - How long after a close a back request still counts as the second half of the same key press, in milliseconds.
- `public void NoteClosed(long nowMs)` - A menu or flyout was closed now.
- `public bool ConsumeIfSamePress(long nowMs)` - Whether a back request now is the second half of a press that already closed something. Answers true once for each close.

## CanvasCursorType (enum)

`internal enum CanvasCursorType`

Source: `src/PressToShape.App/Workspace/CanvasCursorType.cs:4-41`

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
- `public Eyedropper` - A pipette, while a color is picked from the canvas and for the Eyedropper tool. Its active point is the tip.

## CanvasInteraction (class)

`internal partial sealed class CanvasInteraction`

Source: `src/PressToShape.App/Workspace/CanvasInteraction.Groups.cs:9-77`

What the mouse does on the canvas: draw rectangles and ellipses, select, move and delete shapes. Works on document positions and edits the document through PressToShape.App.Workspace.EditorState. It has no UI types. A drag that is in progress ends with PressToShape.App.Workspace.CanvasInteraction.PointerReleased or PressToShape.App.Workspace.CanvasInteraction.Cancel.

**Members**

- `public void DoubleClicked(double x, double y)` - A double-click at this document position. With the Select tool on a group it enters the group and selects what is under the pointer; on a shape of an entered group that is a group itself it enters that group. On empty space it leaves the entered group by one level. Ends a drag that is in progress without a result.
- `public CanvasInteraction(EditorState state)` - _(no documentation)_
- `public bool IsDragging { get; }` - Whether a drag is in progress.
- `public HandleType? DraggedHandle { get; }` - The handle that is being dragged, or null if no resize or rotate drag is in progress.
- `public double TouchGrabSize { get; set; }` - The size of the area that grabs a selection handle for a finger, in screen pixels. A press by mouse or pen uses PressToShape.App.Workspace.SelectionGeometry.GrabSize.
- `public void PointerPressed(double x, double y, bool shift = false, InputPointerType inputType = Mouse)` - The primary button went down at this document position. shift adds to or removes from the selection. inputType is the device: a finger grabs a handle from a larger area than a mouse or a pen.
- `public void PointerMoved(double x, double y, bool shift = false, bool alt = false)` - The pointer moved to this document position. Does nothing without a drag.
- `public void PointerReleased(double x, double y, bool shift = false, bool alt = false)` - The button went up at this document position. Ends the drag in every case.
- `public void Cancel()` - Ends a drag without a result: the draft is dropped and a moved shape keeps the position it has, which then counts as one step if the shape moved.
- `public void Abort()` - Ends a drag and undoes it: the draft and the selection box are dropped, and a move, resize or rotate puts the document back to the one from the press. Nothing is recorded in the history. Used when the drag was a mistake, for example when a second finger arrives. Does nothing without a drag. PressToShape.App.Workspace.CanvasInteraction.Cancel differs: it keeps what the drag reached.
- `public void DeleteSelection()` - Removes all selected shapes from the document as one step. Does nothing without a selection.
- `public HandleType? HoverHandle(double x, double y)` - The handle under this document position, for the cursor. null during a drag, for other tools than Select, without a selection, or away from the handles.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Injected into `PressToShape.App.Tests.CanvasInteractionGroupTests.Scene`

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

Source: `src/PressToShape.App/Workspace/CanvasScene.cs:11-95`

Turns a document into the shapes to draw on the canvas, so the drawing control needs no geometry rules.

**Members**

- `public IReadOnlyList<ScreenShape> Build(Document document, double zoomPercent, double originX, double originY, Guid? enteredGroupId = null)` - Builds the shapes in drawing order: layers bottom first, shapes bottom first, so later shapes cover earlier ones. Groups are flattened: only the shapes in them are drawn, in order. Hidden layers, hidden shapes and the shapes of hidden groups are left out. Locked shapes are drawn like any other.
- `public ScreenShape BuildShape(Shape shape, double zoomPercent, double originX, double originY)` - Converts one shape, for example a shape that is still being drawn and is not in a document.

## ColorPickTarget (enum)

`internal enum ColorPickTarget`

Source: `src/PressToShape.App/Workspace/ColorPickTarget.cs:4-14`

Which color a pick from the canvas (the eyedropper) will set.

**Members**

- `public None` - No pick is going on.
- `public Fill` - The fill.
- `public Stroke` - The stroke.

## CursorImages (class)

`internal static class CursorImages`

Source: `src/PressToShape.App/Workspace/CursorImages.cs:11-92`

The facts about the cursor images of the canvas: which size fits a screen scale, the file name of each image and its active point. Pure logic. The images are made by tools/PressToShape.CursorImages and committed under Assets/Cursors.

**Members**

- `public IReadOnlyList<int> Sizes { get; }` - The image sizes in pixels, ascending. The images are square.
- `public int SizeFor(double scale)` - The image size for a screen scale (1.0 is 100%). A scale that is not a finite positive number counts as 100%, because a wrong size is better than no cursor.
- `public string? FileName(CanvasCursorType type, int size)` - The file name of the image for a cursor type and size, or null for the arrow, which stays the system cursor.
- `public (int X, int Y) Hotspot(CanvasCursorType type, int size)` - The active point of the image in its own pixels. It is the center of the image, except for the magnifier, where it is the center of the lens. The point at 64 px is exactly double the point at 32 px.

## DocumentFileService (class)

`internal sealed class DocumentFileService`

Source: `src/PressToShape.App/Workspace/DocumentFileService.cs:18-249`

The file actions of the File menu: New, Open, Open Recent, Save, Save As, Export as SVG, and the question what to do with unsaved changes. It works through interfaces for files, dialogs and messages, so it is tested without a UI. Parsing and serializing run off the calling (UI) context, and the file store does its reading and writing without blocking it. State changes happen on the calling context.

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

## DoubleTapDetector (class)

`internal sealed class DoubleTapDetector`

Source: `src/PressToShape.App/Workspace/DoubleTapDetector.cs:9-48`

Recognises a double-tap: two taps close in time and place. It holds no clock: the caller says when a tap happened, so a test can say it with plain numbers. A tap that moved too far, or any other input in between, is not a tap and starts the count again.

**Members**

- `public long MaxIntervalMilliseconds` - The most time between two taps of a double-tap, in milliseconds.
- `public double MaxDistance` - The most distance, in screen pixels, between two taps of a double-tap, and the most a finger may move during one tap.
- `public bool Tap(long timeMilliseconds, double x, double y)` - A tap ended at this place and time. Returns whether it completes a double-tap. A completed double-tap uses up both taps, so a third tap right after does not count as another one.
- `public void Reset()` - The last tap does not count any more: something else happened (a drag, two fingers, another tool).

## EditorState (class)

`internal partial sealed class EditorState : ObservableObject`

Source: `src/PressToShape.App/Workspace/EditorState.Groups.cs:11-63`

State shared by all regions of the editor shell: the document, its undo history, active tool, zoom and selection summary. View models observe it through PropertyChanged, so every route that changes a value stays in sync. This class is the only place that changes the document: edits go through PressToShape.App.Workspace.EditorState.Edit, a drag through PressToShape.App.Workspace.EditorState.Preview and PressToShape.App.Workspace.EditorState.Commit, and a new or opened document through PressToShape.App.Workspace.EditorState.ReplaceDocument.

**Members**

- `public Guid? EnteredGroupId { get; private set; }` - _(no documentation)_
- `public EditorState(Document? document = null)` - Creates the state. Without a document it starts with an empty one: Core has no default layer name, the shell passes the real start document.
- `public bool EnterGroup(Guid groupId)` - Enters the group. Of the selection only the direct children of the group stay selected. Does nothing if the id is not a group that can be selected.
- `public Action<ColorPickTarget, Color>? ColorPicked` - Raised when a pick from the canvas ended on a shape: the target that asked for it and the color that was taken.
- `public EventHandler? HistoryChanged` - Raised after the undo history changed: a step was recorded, undone or redone, or the history was reset.
- `public DocumentHistory History { get; }` - The undo history of the current document.
- `public bool IsDragInProgress { get; set; }` - _(no documentation)_
- `public void LeaveGroup()` - Leaves the entered group by one level and selects it. If that group is inside another group, the other group is entered. Does nothing when no group is entered.
- `public bool CanUndo { get; }` - Whether a step can be undone right now.
- `public bool CanRedo { get; }` - Whether a step can be redone right now.
- `public Document Document { get; set; }` - _(no documentation)_
- `public Guid ActiveLayerId { get; set; }` - _(no documentation)_
- `public void LeaveAllGroups()` - Leaves every entered group at once, so the top level of the drawing is selectable again. The selection stays.
- `public ImmutableArray<Guid> SelectedShapeIds { get; private set; }` - _(no documentation)_
- `public Guid? SelectedShapeId { get; }` - The id of the selected shape if exactly one shape is selected, otherwise null.
- `public int SelectedCount { get; }` - How many shapes are selected.
- `public bool HasSelection { get; }` - Whether at least one shape is selected.
- `public Shape? Draft { get; set; }` - _(no documentation)_
- `public Tool ActiveTool { get; set; }` - _(no documentation)_
- `public ColorPickTarget ColorPick { get; private set; }` - _(no documentation)_
- `public void BeginColorPick(ColorPickTarget target)` - Starts a pick from the canvas for the fill or the stroke. A new pick replaces the one before.
- `public void CancelColorPick()` - Ends the pick without a color (Escape, or a click away from every shape).
- `public void CompleteColorPick(Color? color)` - Ends the pick. A color is reported to PressToShape.App.Workspace.EditorState.ColorPicked, null ends the pick without a change.
- `public Appearance DefaultAppearance { get; set; }` - _(no documentation)_
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
- `public void SetSelection(IEnumerable<Guid> shapeIds)` - Replaces the selection by these shapes or groups, keeping their order. Ids that are unknown, repeated, hidden or locked (also through their layer or a group around them) are left out. A selection holds only shapes with the same parent, the same group or the top level of the layers: of ids with different parents, the ones with the parent of the first valid id stay.
- `public void ToggleSelected(Guid shapeId)` - Adds the shape to the selection, or removes it if it is selected. A shape that cannot be selected is ignored.
- `public void ClearSelection()` - Clears the selection.
- `public void SetZoom(double percent)` - _(no documentation)_
- `public void ZoomIn()` - _(no documentation)_
- `public void ZoomOut()` - _(no documentation)_
- `public void ZoomToActualSize()` - _(no documentation)_
- `public void ZoomToFit(double viewportWidth, double viewportHeight)` - Sets the zoom so the artboard fits the viewport and centers the artboard (the view offset becomes zero). The zoom stays unchanged if the viewport has no size yet.

**Relationships**

- Depends on (constructor) `PressToShape.Core.Documents.Document`
- Injected into `PressToShape.App.Tests.CanvasAreaViewModelDoubleTapTests.Fixture`
- Injected into `PressToShape.App.Tests.CanvasInteractionGroupTests.Scene`
- Injected into `PressToShape.App.Tests.ObjectsViewModelTests.Fixture`
- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.StatusBarViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.ToolbarViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.HistoryViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.InspectorViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.LayerItemViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.LayersViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.ObjectsViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.PaletteViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.PropertiesViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.ToolboxViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.ZoomViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.ToolGroupViewModel`
- Injected into `PressToShape.App.Workspace.CanvasInteraction`
- Injected into `PressToShape.App.Workspace.DocumentFileService`
- Injected into `PressToShape.App.Workspace.DocumentSession`
- Injected into `PressToShape.App.Workspace.ObjectEditor`
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

Source: `src/PressToShape.App/Workspace/HistoryStepNames.cs:8-56`

The message ids (English texts) for the rows of the history list. Translate them with ILocalizer.T when shown.

**Members**

- `public string Start { get; }` - The first row of the list: the state before the first kept step.
- `public string MessageId(HistoryStepType type)` - The message id for the name of a step type.

## InputPointerType (enum)

`internal enum InputPointerType`

Source: `src/PressToShape.App/Workspace/InputPointerType.cs:4-9`

The device that made a pointer event.

**Members**

- `public Mouse` - _(no documentation)_
- `public Pen` - _(no documentation)_
- `public Touch` - _(no documentation)_

## InspectorTab (enum)

`internal enum InspectorTab`

Source: `src/PressToShape.App/Workspace/InspectorTab.cs:4-11`

The tabs of the inspector panel.

**Members**

- `public Properties` - _(no documentation)_
- `public Layers` - _(no documentation)_
- `public Objects` - _(no documentation)_
- `public Assets` - _(no documentation)_
- `public History` - _(no documentation)_

## KeyboardInset (class)

`internal static class KeyboardInset`

Source: `src/PressToShape.App/Workspace/KeyboardInset.cs:9-27`

How far the layout has to stop above the bottom of the screen so that the on-screen keyboard does not cover it. On Android the keyboard does not resize the view, it only reports the area it covers, so the layout leaves that gap itself.

**Members**

- `public double Gap(bool isKeyboardOpen, double viewBottom, double coveredTop)` - The gap at the bottom of the view, in the same units as the arguments.

## LongPressTracker (class)

`internal sealed class LongPressTracker`

Source: `src/PressToShape.App/Workspace/LongPressTracker.cs:8-80`

Decides whether a press became a long press. The caller feeds it the pointer events and the time (so there is no clock in here and tests need no real time). It fires once, when the pointer has been down for PressToShape.App.Workspace.LongPressTracker.DelayMs without moving farther than PressToShape.App.Workspace.LongPressTracker.Slop. PressToShape.App.Workspace.LongPressTracker.ConsumeFired tells the caller to ignore the click that follows the release.

**Members**

- `public long DelayMs` - How long the pointer must stay down, in milliseconds.
- `public double Slop` - How far the pointer may move and still count as holding, in the units of the caller (dp).
- `public bool IsWatching { get; }` - Whether a press is being watched, so the caller knows to keep ticking.
- `public void Press(double x, double y, long timestampMs)` - _(no documentation)_
- `public void Move(double x, double y)` - The pointer moved. Moving too far ends the watching.
- `public void Release()` - _(no documentation)_
- `public void Cancel()` - _(no documentation)_
- `public bool Tick(long timestampMs)` - Time passed. Returns true exactly once, at the moment the long press happens.
- `public bool ConsumeFired()` - Whether the last press was a long press. True once, then false until the next long press.

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

## ObjectEditor (class)

`internal sealed class ObjectEditor`

Source: `src/PressToShape.App/Workspace/ObjectEditor.cs:14-169`

Edits of the structure of the drawing: group and ungroup, arrange, lock and hide single objects, and names. Each change of the document is one history step. A command that would change nothing records no step, and its Can... property is false.

**Members**

- `public ObjectEditor(EditorState state, Func<Guid>? idSource = null)` - _(no documentation)_
- `public bool CanGroup { get; }` - Whether at least two shapes with the same layer and the same group are selected.
- `public bool CanUngroup { get; }` - Whether a group is selected.
- `public bool CanHideSelection { get; }` - Whether the selection has something to hide.
- `public bool CanLockSelection { get; }` - Whether the selection has something to lock.
- `public bool CanShowAll { get; }` - Whether any shape or group of the document, nested ones included, is hidden.
- `public bool CanUnlockAll { get; }` - Whether any shape or group of the document, nested ones included, is locked.
- `public bool CanArrange(ArrangeType type)` - Whether arranging the selection changes the order of anything.
- `public bool CanMoveAbove(Guid id, Guid targetId)` - Whether the shape can be moved directly above the target: both exist, are different and have the same layer and group.
- `public void Group()` - Replaces the selected shapes by a new group, which becomes the selection. Does nothing if PressToShape.App.Workspace.ObjectEditor.CanGroup is false.
- `public void Ungroup()` - Replaces the selected groups by their shapes, which become the selection together with the other selected shapes.
- `public void Arrange(ArrangeType type)` - Moves the selected shapes in the stacking order of their layer or group. The selection stays.
- `public void MoveAbove(Guid id, Guid targetId)` - Puts a shape directly above another one that has the same layer and group, as one step. Does nothing if that is not possible.
- `public void LockSelection()` - Locks the selected shapes. They leave the selection, because locked shapes cannot be selected.
- `public void HideSelection()` - Hides the selected shapes. They leave the selection, because hidden shapes cannot be selected.
- `public void SetLocked(IEnumerable<Guid> ids, bool locked)` - Locks or unlocks the shapes with these ids, as one step. Unknown ids are ignored.
- `public void SetHidden(IEnumerable<Guid> ids, bool hidden)` - Hides or shows the shapes with these ids, as one step. Unknown ids are ignored.
- `public void ShowAll()` - Shows every shape and group of the document, nested ones included, as one step.
- `public void UnlockAll()` - Unlocks every shape and group of the document, nested ones included, as one step.
- `public void Rename(Guid id, string name)` - Gives a shape or group a name, as one step. An empty or blank name removes the name. Does nothing for an unknown id.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Injected into `PressToShape.App.Tests.ObjectsViewModelTests.Fixture`
- Injected into `PressToShape.App.ViewModels.Editor.InspectorViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.ObjectsViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.PropertiesViewModel`

## PropertiesSection (enum)

`internal enum PropertiesSection`

Source: `src/PressToShape.App/Workspace/PropertiesSection.cs:6-15`

The sections of the Properties tab. Combined as flags to say which ones a tool shows.

**Members**

- `public None` - _(no documentation)_
- `public Transform` - _(no documentation)_
- `public Appearance` - _(no documentation)_
- `public Path` - _(no documentation)_
- `public Align` - _(no documentation)_
- `public QuickActions` - _(no documentation)_

## PropertiesSections (class)

`internal static class PropertiesSections`

Source: `src/PressToShape.App/Workspace/PropertiesSections.cs:6-21`

Which sections of the Properties tab are relevant for a tool.

**Members**

- `public PropertiesSection For(Tool tool)` - The sections to show for tool. A rule and not a table. The throwing arm only covers values outside the enum (the compiler asks for it). A test covers every PressToShape.App.Workspace.Tool value, so a new tool fails there until it is placed here.

## RecentColors (class)

`internal sealed class RecentColors`

Source: `src/PressToShape.App/Workspace/RecentColors.cs:11-31`

The colors the user picked lately, most recent first. Each color appears once and the list keeps at most PressToShape.App.Workspace.RecentColors.MaxCount colors. It lives as long as the app runs and is not saved.

**Members**

- `public int MaxCount` - The most colors the list keeps.
- `public IReadOnlyList<Color> Items { get; }` - The colors, most recent first.
- `public void Add(Color color)` - Puts the color first. If the list already has it, it moves to the front. The oldest color drops out when the list is full.

## RecentFiles (class)

`internal sealed class RecentFiles`

Source: `src/PressToShape.App/Workspace/RecentFiles.cs:14-92`

The list of recently used files, most recent first, kept in the settings file. It holds each path once and at most PressToShape.App.Workspace.RecentFiles.MaxCount paths. The stored value is untrusted: anything that is not a list of paths is ignored. A list that does not persist stays empty: the paths of a platform whose file references do not survive a restart are of no use later.

**Members**

- `public int MaxCount` - The most paths the list keeps.
- `public RecentFiles(ISettingsStore store, bool persist = true)` - _(no documentation)_
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

Source: `src/PressToShape.App/Workspace/ScreenShape.cs:20-31`

A shape ready to draw: position, size and stroke width are already in screen pixels. PressToShape.App.Workspace.ScreenShape.Stroke is null when nothing should be stroked, which includes a stroke width of zero.

**Members**

- `public ScreenShape(ScreenShapeType Type, double X, double Y, double Width, double Height, Color? Fill, Color? Stroke, double StrokeWidth, double Opacity, double RotationDegrees = 0, bool IsDimmed = false)` - A shape ready to draw: position, size and stroke width are already in screen pixels. PressToShape.App.Workspace.ScreenShape.Stroke is null when nothing should be stroked, which includes a stroke width of zero.
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
- `public bool IsDimmed { get; init; }` - Whether the shape is drawn faded because a group is entered and the shape is not inside it.

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

Source: `src/PressToShape.App/Workspace/SelectionEditor.cs:14-217`

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
- `public void Duplicate()` - Adds copies of the selected shapes and groups, each at the top of its own layer or group, 10 units right and down, as one step, and selects them. The clipboard is not touched.
- `public void Nudge(double dx, double dy)` - Moves the selected shapes by the distance in document units, not limited to the artboard. Repeated nudges of the same selection become one history step, as long as the document was not changed by anything else in between.
- `public void Apply(HistoryStepType type, Func<Shape, Shape> change)` - Applies the change to every selected shape as one step. A selected group is not changed itself: the change is applied to the shapes below it. No step if no shape changed.
- `public void ApplyTransform(HistoryStepType type, Func<IReadOnlyList<Shape>, IReadOnlyList<Shape>> change)` - Replaces the shapes of the selection by the returned list, which must have the same shapes (same ids) in the same order, as one step. For changes that need all shapes together. The shapes are those below the selected groups. No step if nothing changed.
- `public void ApplyTransformWithFrame(HistoryStepType type, Func<IReadOnlyList<Shape>, SelectionFrame, IReadOnlyList<Shape>> change)` - Like PressToShape.App.Workspace.SelectionEditor.ApplyTransform, and the change also gets the frame of the selection. A selected group makes the frame upright and around all its shapes, as for several shapes.

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

Source: `src/PressToShape.App/Workspace/SelectionGeometry.cs:8-158`

Where the selection frame and its handles are, and which handle a point hits. Pure functions, document units.

**Members**

- `public double RotateHandleDistance` - How far the rotate handle is from the top edge, in screen pixels.
- `public double GrabSize` - The size of the area that grabs a handle with a mouse or a pen, in screen pixels.
- `public double TouchGrabSize` - The size of the area that grabs a handle with a finger, in screen pixels. The tablet layout can set its own through the theme.
- `public SelectionFrame? FrameOf(IReadOnlyList<Shape> shapes)` - The frame of the shapes, or null for none.
- `public (int X, int Y) Direction(HandleType handle)` - The direction of a handle from the frame center in frame units: (-1, -1) is the top left corner.
- `public (double X, double Y) HandlePosition(SelectionFrame frame, HandleType handle, double zoomPercent)` - The center of the handle in document units. The handles turn with the frame.
- `public HandleType? HitHandle(SelectionFrame frame, double x, double y, double zoomPercent, double grabSize = 10)` - The handle whose grab area contains the document point, or null. The closest handle wins, then the priority order. A grab area larger than PressToShape.App.Workspace.SelectionGeometry.GrabSize (for a finger) is limited to a quarter of the shorter side of the frame, so that the handles of a small shape do not cover its middle and the shape can still be moved by it.
- `public double HandleDirectionDegrees(HandleType handle, double rotation)` - The direction in which a handle points on the screen, in degrees [0, 360): 0 is right, 90 is down. For choosing a cursor.

## SelectionOverlay (record)

`internal sealed record SelectionOverlay : IEquatable<SelectionOverlay>`

Source: `src/PressToShape.App/Workspace/SelectionOverlay.cs:19-115`

Everything the canvas draws for the selection, in screen pixels: an outline for every selected shape, the box and handles around the selection, the line to the rotate handle, and the box that is being dragged on empty space.

**Members**

- `public SelectionOverlay(IReadOnlyList<ScreenPolygon> Outlines, ScreenPolygon? Box, IReadOnlyList<ScreenHandle> Handles, ScreenLine? RotateLink, ScreenRect? SelectionBox, ScreenPolygon? EnteredGroup = null)` - Everything the canvas draws for the selection, in screen pixels: an outline for every selected shape, the box and handles around the selection, the line to the rotate handle, and the box that is being dragged on empty space.
- `public IReadOnlyList<ScreenPolygon> Outlines { get; init; }` - One turned outline per selected shape. For every tool.
- `public ScreenPolygon? Box { get; init; }` - The frame around the selection (Select tool only).
- `public IReadOnlyList<ScreenHandle> Handles { get; init; }` - The eight resize handles and the rotate handle (Select tool only).
- `public ScreenLine? RotateLink { get; init; }` - The line from the middle of the top edge to the rotate handle (Select tool only).
- `public ScreenRect? SelectionBox { get; init; }` - The box being dragged, if any (Select tool only).
- `public ScreenPolygon? EnteredGroup { get; init; }` - The outline of the entered group, if one is entered. Drawn for every tool.
- `public double HandleSize` - The side of a handle in screen pixels.
- `public SelectionOverlay Empty { get; }` - An overlay without anything to draw.
- `public SelectionOverlay Build(Document document, IEnumerable<Guid> selectedIds, Tool tool, double zoomPercent, double originX, double originY, SelectionBox? selectionBox = null, Guid? enteredGroupId = null)` - Builds the overlay. Shapes of hidden or locked layers are not part of it, even if their ids are in the selection. Handles, the box, the link and the dragged box exist only for the Select tool; outlines exist for every tool.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.ScreenPolygon`

## SelectionQueries (class)

`internal static class SelectionQueries`

Source: `src/PressToShape.App/Workspace/SelectionQueries.cs:15-194`

Questions about which shapes can be selected and which shapes a selection box touches. A shape can be selected when its layer and every group around it are visible and not locked, and it is visible and not locked itself. Results are in document order: bottom layer first, and within a layer or group bottom shape first, a group before its children. Groups are shapes too: a click selects the group, and only after the group was entered (see PressToShape.App.Workspace.EditorState.EnteredGroupId) its children.

**Members**

- `public bool IsSelectable(Document document, Guid shapeId)` - Whether the document has a shape or group with this id that can be selected.
- `public IReadOnlyList<Shape> Selectable(Document document, Guid? enteredGroupId = null)` - The shapes and groups a click or a box can select: the direct children of the entered group, or, without one, the top-level shapes of every layer. Hidden and locked ones, and the ones in hidden or locked layers or groups, are left out.
- `public IReadOnlyList<Shape> ShapesTouching(Document document, double x, double y, double width, double height, Guid? enteredGroupId = null)` - The selectable shapes (see PressToShape.App.Workspace.SelectionQueries.Selectable) whose axis-aligned bounds, after rotation, overlap the rectangle. Touching edges count.
- `public Shape? TargetFor(Document document, Shape hit, Guid? enteredGroupId = null)` - The shape or group that a hit on hit selects. Without an entered group it is the outermost group around the hit shape, or the shape itself. With one it is the direct child of the entered group that holds the hit shape, and null when the hit shape is not inside the entered group.
- `public IReadOnlyList<Shape> SelectedShapes(Document document, IEnumerable<Guid> shapeIds)` - The shapes and groups with these ids, in document order. Unknown ids are ignored.
- `public IReadOnlyList<Shape> SelectedSelectable(Document document, IEnumerable<Guid> shapeIds)` - The shapes and groups with these ids that can be selected, in document order. One pass, however many ids.
- `public IReadOnlyList<Shape> SelectedLeaves(Document document, IEnumerable<Guid> shapeIds)` - The shapes below the selected shapes and groups that are not groups, in document order: what move, resize, rotate and the appearance work on. A selected plain shape is its own leaf. Each leaf appears once.
- `public IReadOnlyDictionary<Guid, Guid?> SelectableWithParents(Document document, IReadOnlySet<Guid> wanted)` - Of the wanted ids, the ones that can be selected, each with the id of its closest group (null on the top level of a layer). One pass over the document.

## SelectionSummaryText (class)

`internal static class SelectionSummaryText`

Source: `src/PressToShape.App/Workspace/SelectionSummaryText.cs:7-15`

The translated selection text: "Nothing selected", "1 selected" or "N selected".

**Members**

- `public string Format(ILocalizer localizer, int selectedCount)` - _(no documentation)_

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

Source: `src/PressToShape.App/Workspace/SelectionValues.cs:54-90`

The values the Properties tab shows for a selection. Position and size are those of the selection frame (one shape: its own box, several or a group: the box around all of them, with rotation 0). Appearance values are those of the shapes below the groups and are mixed when they differ.

**Members**

- `public SelectionValues(double X, double Y, double Width, double Height, double Rotation, bool IsSingle, Mixed<Color?> Fill, Mixed<Color?> Stroke, Mixed<double> StrokeWidth, Mixed<double> Opacity)` - The values the Properties tab shows for a selection. Position and size are those of the selection frame (one shape: its own box, several or a group: the box around all of them, with rotation 0). Appearance values are those of the shapes below the groups and are mixed when they differ.
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

## ToolGroupDefinition (record)

`internal sealed record ToolGroupDefinition : IEquatable<ToolGroupDefinition>`

Source: `src/PressToShape.App/Workspace/ToolGroupDefinition.cs:6-13`

Tools that share one icon in the tablet tool column. The first tool is the one the group starts with.

**Members**

- `public ToolGroupDefinition(IReadOnlyList<Tool> Tools)` - Tools that share one icon in the tablet tool column. The first tool is the one the group starts with.
- `public IReadOnlyList<Tool> Tools { get; init; }` - _(no documentation)_
- `public Tool DefaultTool { get; }` - The tool the group has before any other one was chosen.
- `public bool IsGroup { get; }` - Whether the group has more than one tool, so it needs a mark and a flyout.

**Relationships**

- Injected into `PressToShape.App.ViewModels.Tablet.ToolGroupViewModel`

## ToolGroups (class)

`internal static class ToolGroups`

Source: `src/PressToShape.App/Workspace/ToolGroups.cs:7-21`

The icons of the tablet tool column, in order. Text and Eyedropper are groups of one tool.

**Members**

- `public IReadOnlyList<ToolGroupDefinition> All { get; }` - _(no documentation)_
- `public ToolGroupDefinition Of(Tool tool)` - The group a tool belongs to.

## TouchGestureStep (record)

`internal readonly record TouchGestureStep : IEquatable<TouchGestureStep>`

Source: `src/PressToShape.App/Workspace/TouchGestureStep.cs:10-19`

One decision of the PressToShape.App.Workspace.TouchGestureTracker. All positions are screen positions of the canvas.

**Members**

- `public TouchGestureStep(TouchGestureStepType Type, double X = 0, double Y = 0, double TargetZoom = 0, double PreviousMidX = 0, double PreviousMidY = 0)` - One decision of the PressToShape.App.Workspace.TouchGestureTracker. All positions are screen positions of the canvas.
- `public TouchGestureStepType Type { get; init; }` - What to do.
- `public double X { get; init; }` - The position of the press, move or release, or the midpoint of the two fingers for a gesture.
- `public double Y { get; init; }` - See X.
- `public double TargetZoom { get; init; }` - For a gesture: the zoom in percent, within the zoom range.
- `public double PreviousMidX { get; init; }` - For a gesture: the midpoint of the two fingers before this event.
- `public double PreviousMidY { get; init; }` - For a gesture: see PreviousMidX.
- `public TouchGestureStep Ignored { get; }` - _(no documentation)_

## TouchGestureStepType (enum)

`internal enum TouchGestureStepType`

Source: `src/PressToShape.App/Workspace/TouchGestureStepType.cs:4-26`

What the canvas has to do after a touch event, as decided by PressToShape.App.Workspace.TouchGestureTracker.

**Members**

- `public Ignore` - Nothing. The finger is not used.
- `public NotTracked` - The event is not a finger (mouse or pen). The caller handles it as usual.
- `public Press` - The first finger went down: the active tool acts, like a mouse press at X, Y.
- `public Move` - The first finger moved: like a mouse move at X, Y.
- `public Release` - The first finger went up: like a mouse release at X, Y.
- `public CancelPress` - The one-finger action that was running must be cancelled without a result, because a second finger arrived or the gesture was cancelled.
- `public Gesture` - Two fingers moved: zoom to TargetZoom around the previous midpoint, then move the view by the midpoint's change.

## TouchGestureTracker (class)

`internal sealed class TouchGestureTracker`

Source: `src/PressToShape.App/Workspace/TouchGestureTracker.cs:12-146`

Decides what the fingers on the canvas mean. One finger acts with the active tool, like the mouse. A second finger cancels that action (so nothing stray stays in the drawing) and the two fingers then move and zoom the view around their midpoint. When a finger of the two lifts, the rest is ignored until every finger is up, so a lifting hand does not draw. Mouse and pen are not tracked. The tracker works on plain numbers and holds no document or view state. A pointer id only has to be unique from a finger's press to its release.

**Members**

- `public bool IsGestureActive { get; }` - Whether two fingers are moving the view right now.
- `public TouchGestureStep PointerDown(int id, InputPointerType type, double x, double y, double currentZoomPercent)` - _(no documentation)_
- `public TouchGestureStep PointerMove(int id, double x, double y)` - _(no documentation)_
- `public TouchGestureStep PointerUp(int id, double x, double y)` - _(no documentation)_
- `public TouchGestureStep Cancel()` - The system took the fingers away (for example the pointer capture was lost). Forgets every finger. A running one-finger action must be cancelled.

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
