# PressToShape.App.ViewModels.Editor

Project: PressToShape.App

## CanvasAreaViewModel (class)

`internal partial sealed class CanvasAreaViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.LongPress.cs:8-51`

Geometry of the canvas area: where the artboard is drawn and where the ruler zero points are. The view only reports the viewport size, everything else is derived here.

**Members**

- `public bool IsWatchingLongPress { get; }` - Whether a press may still become a long press, so the view keeps calling PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.Tick.
- `public void Tick()` - Time passed. When a finger or a pen has stayed down long enough without moving, the action the press started is undone and the ring menu opens at the press. The release that follows is no tap.
- `public CanvasAreaViewModel(EditorState state, SelectionEditor? editor = null, TimeProvider? time = null, RingMenuViewModel? ring = null)` - _(no documentation)_
- `public double ViewportWidth { get; set; }` - _(no documentation)_
- `public double ViewportHeight { get; set; }` - _(no documentation)_
- `public bool IsAltHeld { get; set; }` - _(no documentation)_
- `public RingMenuViewModel? Ring { get; }` - The ring context menu, or null when the canvas was made without one.
- `public Document Document { get; }` - The document the canvas draws.
- `public ScreenShape? Draft { get; }` - The shape that is being drawn, in screen space, or null.
- `public SelectionOverlay Overlay { get; }` - Everything the canvas draws for the selection: outlines, frame, handles and the box being dragged.
- `public Guid? EnteredGroupId { get; }` - The group that was entered, or null. The canvas dims everything outside it.
- `public ScreenPenOverlay? PenOverlay { get; }` - The nodes and handles of the path that the Pen is drawing, in screen pixels. null when the Pen is not drawing.
- `public ScreenNodeOverlay? NodeOverlay { get; }` - The nodes and handles of the selected shapes for Direct Select and the Node tool, in screen pixels. null for other tools or when there is nothing to show.
- `public Guid? HiddenShapeId { get; }` - The path that the Pen is continuing. The canvas leaves it out, because the draft shows it. null when none is.
- `public CanvasCursorType CursorType { get; }` - The pointer shape: a closed hand while the view is being dragged, a resize or rotate cursor over or while dragging a handle, otherwise the eyedropper while a pick is active, otherwise the shape of the active tool. The Zoom tool shows the zoom-out magnifier while Alt is held.
- `public double HorizontalScrollMinimum { get; }` - Smallest value of the horizontal scroll bar: minus the furthest the view offset may go.
- `public double HorizontalScrollMaximum { get; }` - Largest value of the horizontal scroll bar: the furthest the view offset may go.
- `public double HorizontalScrollValue { get; set; }` - The horizontal scroll bar's value: the view offset, negated, because the bar's value grows when the view moves right. Setting it moves the view.
- `public double HorizontalScrollViewportSize { get; }` - The visible width, which sets the size of the horizontal thumb.
- `public bool CanScrollHorizontally { get; }` - Whether the view can be moved horizontally at all.
- `public double VerticalScrollMinimum { get; }` - Smallest value of the vertical scroll bar.
- `public double VerticalScrollMaximum { get; }` - Largest value of the vertical scroll bar.
- `public double VerticalScrollValue { get; set; }` - The vertical scroll bar's value, see PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollValue.
- `public double VerticalScrollViewportSize { get; }` - The visible height, which sets the size of the vertical thumb.
- `public bool CanScrollVertically { get; }` - Whether the view can be moved vertically at all.
- `public bool IsPanning { get; }` - Whether the view is being dragged right now.
- `public double ZoomPercent { get; }` - _(no documentation)_
- `public double ArtboardWidth { get; }` - _(no documentation)_
- `public double ArtboardHeight { get; }` - _(no documentation)_
- `public double ArtboardScreenWidth { get; }` - _(no documentation)_
- `public double ArtboardScreenHeight { get; }` - _(no documentation)_
- `public double OriginX { get; }` - Horizontal screen position of the artboard's left edge, where the horizontal ruler's zero is. The artboard is centered (also when it is larger than the canvas area) and then moved by the view offset.
- `public double OriginY { get; }` - Vertical screen position of the artboard's top edge. The vertical ruler's zero is here.
- `public double TouchGrabSize { get; set; }` - The size of the area that grabs a selection handle for a finger, in screen pixels. The tablet layout sets it from the theme.
- `public void PointerPressed(double screenX, double screenY, bool alt = false, bool shift = false, InputPointerType inputType = Mouse)` - The primary button went down. The position is in screen pixels of the canvas area. With the Hand tool this starts dragging the view, with the Zoom tool it zooms one step in (with alt: out) around the position, with the other tools it is handled by the tool. inputType is the device: a finger grabs handles from a larger area.
- `public bool OpenContextMenu(double screenX, double screenY, InputPointerType inputType = Mouse)` - Opens the ring menu at this position, in screen pixels of the canvas area (right-click, long press or the barrel button of a pen). First applies the selection rules of the menu (see PressToShape.App.Workspace.CanvasInteraction.SelectForContextMenu). Does nothing without a ring, with a tool other than Select, or while a drag, a pan, a pick from the canvas or a two-finger gesture is in progress.
- `public void DoubleClick(double screenX, double screenY, bool alt = false, bool shift = false, InputPointerType inputType = Mouse)` - The primary button was pressed a second time at this position, in screen pixels of the canvas area. With the Select tool this enters a group or leaves the entered one (see PressToShape.App.Workspace.CanvasInteraction.DoubleClicked). With other tools it acts as a normal press.
- `public void TouchPressed(int id, double screenX, double screenY)` - A finger went down. One finger acts with the active tool like the mouse. A second finger undoes that action (see PressToShape.App.Workspace.CanvasInteraction.Abort) and the two fingers then move and zoom the view. Mouse and pen use PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.PointerPressed. The id only has to be unique from this press to the release.
- `public void TouchMoved(int id, double screenX, double screenY)` - A finger moved.
- `public void TouchReleased(int id, double screenX, double screenY)` - A finger went up.
- `public void TouchCancelled()` - The system took the fingers away, or the pointer capture was lost. All fingers are forgotten and an action of one finger that was not finished is undone. Does nothing if no finger is down, for example after a normal release.
- `public void MiddlePressed(double screenX, double screenY)` - The middle button went down: drags the view with any tool, unless another drag is in progress.
- `public void PointerMoved(double screenX, double screenY, bool shift = false, bool alt = false)` - The pointer moved. The position is in screen pixels of the canvas area.
- `public void PointerReleased(double screenX, double screenY, bool shift = false, bool alt = false)` - The button went up. The position is in screen pixels of the canvas area.
- `public void PointerHover(double screenX, double screenY)` - The pointer moved over the canvas without a drag: the handle under it decides the cursor, and the Pen shows its next segment.
- `public void PointerLeft()` - The pointer left the canvas: no handle is hovered any more.
- `public bool KeyPressed(CanvasKey key, bool shift)` - A key went down on the canvas. The arrow keys move the selection by 1 unit, with shift by 10. Escape leaves the entered group, or clears the selection, or ends a pick from the canvas. Nothing else happens during a drag.
- `public void Cancel()` - Ends a drag without a result, for example when the pointer capture is lost.
- `public bool Wheel(double deltaX, double deltaY, bool control, bool shift, double screenX, double screenY)` - The wheel (or a touchpad scroll) turned over the canvas. With Ctrl it zooms around the pointer, otherwise it moves the view, Shift moving it horizontally. A positive delta moves the view toward the top or the left.
- `public void DeleteSelection()` - Removes the selected shapes. Does nothing without a selection.

**Relationships**

- Depends on (constructor) `PressToShape.App.ViewModels.Editor.RingMenuViewModel`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.SelectionEditor`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`
- Injected into `PressToShape.App.Tests.CanvasAreaViewModelDoubleTapTests.Fixture`
- Injected into `PressToShape.App.Tests.CanvasAreaViewModelNodeTests.Fixture`

## ColorFieldViewModel (class)

`internal partial sealed class ColorFieldViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/ColorFieldViewModel.cs:12-152`

A color of the Properties tab (fill or stroke). While the color picker is open the user's changes are previewed on the canvas (PressToShape.App.ViewModels.Editor.ColorFieldViewModel.Update) and the whole edit becomes one undo step when the picker closes (PressToShape.App.ViewModels.Editor.ColorFieldViewModel.EndEdit). "None" and a color chosen elsewhere take effect at once as one step.

**Members**

- `public ColorFieldViewModel(Action<Color?> preview, Action commit, Action<Color?> setNow)` - _(no documentation)_
- `public bool HasColor { get; private set; }` - _(no documentation)_
- `public bool IsMixed { get; private set; }` - _(no documentation)_
- `public bool IsNone { get; }` - Whether the shapes all have no color (no fill or no stroke). Never true while the field is disabled.
- `public Color Color { get; private set; }` - _(no documentation)_
- `public bool IsEnabled { get; private set; }` - _(no documentation)_
- `public bool IsEditing { get; private set; }` - _(no documentation)_
- `public void SetValue(Color? color, bool mixed)` - Shows the color of the shapes. Ignored while the user is editing, so the preview is not overwritten.
- `public void SetEnabled(bool enabled)` - Turns the field on or off.
- `public void BeginEdit()` - The picker opened.
- `public void Update(Color color)` - The user changed the color in the picker. Previews it. Ignored when no edit began or the color is the one shown (a picker also reports the color it was given). To get the shown color on a field without a color, use PressToShape.App.ViewModels.Editor.ColorFieldViewModel.SetColor.
- `public void EndEdit()` - The picker closed. Records the edit as one step, if the color changed.
- `public void SetNone()` - Removes the color (no fill or no stroke) as one step.
- `public void SetColor(Color color)` - Sets the color as one step. For a color that is chosen without the picker, for example when "none" is switched off.
- `public void ToggleNone()` - The "none" checkbox: removes the color if there is one, otherwise gives the shown color back. One step.

**Relationships**

- Inherits `PressToShape.App.ViewModels.ViewModelBase`
- Injected into `PressToShape.App.ViewModels.Editor.PaletteViewModel`

## EditMenuCommands (record)

`internal sealed record EditMenuCommands : IEquatable<EditMenuCommands>`

Source: `src/PressToShape.App/ViewModels/Editor/EditMenuCommands.cs:6-14`

What the clipboard, delete and selection items of the Edit menu do. Undo and Redo are passed on their own.

**Members**

- `public EditMenuCommands(ICommand Cut, ICommand Copy, ICommand Paste, ICommand PasteInPlace, ICommand Duplicate, ICommand Delete, ICommand SelectAll, ICommand Deselect)` - What the clipboard, delete and selection items of the Edit menu do. Undo and Redo are passed on their own.
- `public ICommand Cut { get; init; }` - _(no documentation)_
- `public ICommand Copy { get; init; }` - _(no documentation)_
- `public ICommand Paste { get; init; }` - _(no documentation)_
- `public ICommand PasteInPlace { get; init; }` - _(no documentation)_
- `public ICommand Duplicate { get; init; }` - _(no documentation)_
- `public ICommand Delete { get; init; }` - _(no documentation)_
- `public ICommand SelectAll { get; init; }` - _(no documentation)_
- `public ICommand Deselect { get; init; }` - _(no documentation)_

**Relationships**

- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel`

## EditorShellViewModel (class)

`internal abstract partial class EditorShellViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:17-306`

What every layout shell needs. Creates the shared PressToShape.App.Workspace.EditorState and the view model of every region that the layouts share (tool column, canvas, inspector), and owns the file, edit, undo and redo commands, so the regions of a layout always agree. The desktop and the tablet layout derive from it and add their own regions.

**Members**

- `public IRelayCommand BringForwardCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.BringForward`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.BringForward.
- `public IRelayCommand BringToFrontCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.BringToFront`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.BringToFront.
- `public IRelayCommand CopyCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Copy`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Copy.
- `public IRelayCommand CutCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Cut`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Cut.
- `public IRelayCommand DeleteCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Delete`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Delete.
- `public IRelayCommand DeselectCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Deselect`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Deselect.
- `public IRelayCommand DuplicateCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Duplicate`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Duplicate.
- `public IAsyncRelayCommand ExportSvgCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.ExportSvgAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.ExportSvgAsync.
- `public IRelayCommand GroupCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Group`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Group.
- `public IRelayCommand HideCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Hide`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Hide.
- `public IRelayCommand LockCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Lock`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Lock.
- `public IAsyncRelayCommand NewCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.NewAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.NewAsync.
- `public IAsyncRelayCommand OpenCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.OpenAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.OpenAsync.
- `public IRelayCommand PasteCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Paste`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Paste.
- `public IRelayCommand PasteInPlaceCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.PasteInPlace`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.PasteInPlace.
- `public IRelayCommand RedoCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Redo`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Redo.
- `public IAsyncRelayCommand SaveAsCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsAsync.
- `public IAsyncRelayCommand SaveCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsync.
- `public IRelayCommand SelectAllCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SelectAll`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.SelectAll.
- `public IRelayCommand SendBackwardCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SendBackward`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.SendBackward.
- `public IRelayCommand SendToBackCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SendToBack`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.SendToBack.
- `public IRelayCommand ShowAllCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.ShowAll`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.ShowAll.
- `public IRelayCommand UndoCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Undo`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Undo.
- `public IRelayCommand UngroupCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Ungroup`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Ungroup.
- `public IRelayCommand UnlockAllCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.UnlockAll`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.UnlockAll.
- `protected EditorShellViewModel(ILocalizer localizer, FileServices services)` - _(no documentation)_
- `public EditorState State { get; }` - _(no documentation)_
- `public ToolboxViewModel Toolbox { get; }` - _(no documentation)_
- `public SelectionEditor Editor { get; }` - The selection edits (copy, paste, nudge, property changes). One instance, shared by the canvas, the inspector and the menus.
- `public ObjectEditor Objects { get; }` - The edits of the structure: group, arrange, lock, hide, rename. One instance, shared by the menus and the object list.
- `public CanvasAreaViewModel Canvas { get; }` - _(no documentation)_
- `public InspectorViewModel Inspector { get; }` - _(no documentation)_
- `public string DocumentName { get; }` - The file name of the document, or the translated word for "Untitled" while it has none.
- `public bool IsDirty { get; }` - Whether the document has changes that were not saved.
- `public string WindowTitle { get; }` - The window title: an asterisk while there are unsaved changes, the file name (or "Untitled") and the product name.
- `protected RecentFiles Recent { get; }` - The recently used files, for layouts that list them.
- `protected FileMenuCommands FileCommands { get; }` - The commands behind the file items of a menu.
- `protected EditMenuCommands EditCommands { get; }` - The commands behind the edit items of a menu.
- `public ObjectMenuCommands ObjectCommands { get; }` - The commands behind the group, arrange, lock and hide items of a menu.
- `public Task<bool> ConfirmCloseAsync()` - Asks about unsaved changes before the window closes. Returns whether closing may go on.
- `public void FitToWindow()` - Zooms so the artboard fits the canvas area. Does nothing until the canvas has a size.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.FileServices`
- Inherited by `PressToShape.App.Tests.EditorShellViewModelRingTests.BareShell`
- Inherited by `PressToShape.App.Tests.EditorShellViewModelTests.BareShell`
- Inherited by `PressToShape.App.Tests.RingMenuControlTests.BareShell`
- Inherited by `PressToShape.App.Tests.TestShell`
- Inherited by `PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`
- Inherited by `PressToShape.App.ViewModels.Tablet.TabletShellViewModel`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`

## FileMenuCommands (record)

`internal sealed record FileMenuCommands : IEquatable<FileMenuCommands>`

Source: `src/PressToShape.App/ViewModels/Editor/FileMenuCommands.cs:7-13`

What the File menu items do. OpenRecent creates the command for one entry of the recent list.

**Members**

- `public FileMenuCommands(ICommand New, ICommand Open, ICommand Save, ICommand SaveAs, ICommand ExportSvg, Func<string, ICommand> OpenRecent)` - What the File menu items do. OpenRecent creates the command for one entry of the recent list.
- `public ICommand New { get; init; }` - _(no documentation)_
- `public ICommand Open { get; init; }` - _(no documentation)_
- `public ICommand Save { get; init; }` - _(no documentation)_
- `public ICommand SaveAs { get; init; }` - _(no documentation)_
- `public ICommand ExportSvg { get; init; }` - _(no documentation)_
- `public Func<string, ICommand> OpenRecent { get; init; }` - _(no documentation)_

**Relationships**

- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel`

## HistoryItemViewModel (class)

`internal partial sealed class HistoryItemViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/HistoryItemViewModel.cs:11-37`

One row of the History tab: the start state (PressToShape.App.ViewModels.Editor.HistoryItemViewModel.Type is null) or a step. The name is translated when it is read, so it follows a language switch.

**Members**

- `public HistoryItemViewModel(ILocalizer localizer, int index, HistoryStepType? type, string messageId)` - _(no documentation)_
- `public int Index { get; }` - The position in the history: 0 is the start state, k is the state after the k-th step.
- `public HistoryStepType? Type { get; }` - The step type, or null for the start row.
- `public string Name { get; }` - The translated name of the row.
- `public bool IsCurrent { get; set; }` - _(no documentation)_
- `public bool IsRedoable { get; set; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## HistoryViewModel (class)

`internal sealed class HistoryViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/HistoryViewModel.cs:14-112`

State of the History tab: the start state and one row per step, oldest first. The selected row is the current state, and selecting another row jumps there. The list is rebuilt only when the sequence of steps changed, otherwise the rows are updated in place, so undo and redo do not disturb the list under the user's click.

**Members**

- `public HistoryViewModel(EditorState state, ILocalizer localizer)` - _(no documentation)_
- `public ObservableCollection<HistoryItemViewModel> Items { get; }` - The rows, oldest first. The first row is the start state.
- `public HistoryItemViewModel? SelectedItem { get; set; }` - The row of the current state. Setting another row jumps to its state, which may be refused (while a drag is in progress), and the selection then goes back to the current row. null is ignored: one row is always current.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## InspectorViewModel (class)

`internal partial sealed class InspectorViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs:10-53`

State of the inspector: the selected tab and the view models of its tabs.

**Members**

- `public InspectorViewModel(EditorState state, ILocalizer localizer, SelectionEditor? editor = null, ObjectEditor? objects = null)` - _(no documentation)_
- `public PropertiesViewModel Properties { get; }` - _(no documentation)_
- `public LayersViewModel Layers { get; }` - _(no documentation)_
- `public PlaceholderPanelViewModel Assets { get; }` - _(no documentation)_
- `public HistoryViewModel History { get; }` - _(no documentation)_
- `public InspectorTab SelectedTab { get; set; }` - _(no documentation)_
- `public int SelectedTabIndex { get; set; }` - The selected tab as an index, for binding to a tab strip. Out-of-range values are ignored.
- `public void ShowTab(InspectorTab tab)` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.ObjectEditor`
- Depends on (constructor) `PressToShape.App.Workspace.SelectionEditor`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`

## LayersViewModel (class)

`internal partial sealed class LayersViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/LayersViewModel.cs:22-384`

State of the Layers tab: the layers of the document as header rows, and below each layer its shapes and groups as a tree, topmost first. PressToShape.App.ViewModels.Editor.LayersViewModel.Rows is the flat list that the view shows: the rows whose groups and layer are expanded. The list follows the document. When only names, flags or the selection changed, the existing rows are updated, so the list is not rebuilt under the user's click, and which groups are open survives undo and redo as far as the groups still exist.

**Members**

- `public IRelayCommand AddLayerCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.LayersViewModel.AddLayer`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.LayersViewModel.AddLayer.
- `public LayersViewModel(EditorState state, ObjectEditor objects, ILocalizer localizer)` - _(no documentation)_
- `public ObservableCollection<ObjectRowViewModel> Rows { get; }` - The rows to show, topmost first: a layer, then what is in it, and so on. Rows below a closed group or layer are not in it.
- `public void Select(ObjectRowViewModel row, bool extend)` - A row was clicked. A layer row makes its layer the active one. A shape or group row selects the object on the canvas: the group around it is entered, so the object can be selected, and the selection becomes this row, or with extend grows by it when it has the same parent as the rest. A row that cannot be selected (locked or hidden) is only marked as the current row.
- `public void SetExpanded(ObjectRowViewModel row, bool expanded)` - Shows or hides the rows below a layer or group.
- `public void SetHidden(ObjectRowViewModel row, bool hidden)` - Hides or shows the object of the row, as one step.
- `public void SetLocked(ObjectRowViewModel row, bool locked)` - Locks or unlocks the object of the row, as one step.
- `public void Rename(ObjectRowViewModel row, string name)` - Gives the object of the row a name, as one step. An empty name removes the name.
- `public bool CanReorder(ObjectRowViewModel dragged, ObjectRowViewModel target)` - Whether the dragged row can be dropped on the target row: both are objects with the same parent.
- `public void Reorder(ObjectRowViewModel dragged, ObjectRowViewModel target)` - Puts the dragged object directly above the target in the stacking order, as one step. Does nothing if that is not possible.
- `protected void OnLanguageRefreshed()` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.ObjectEditor`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`
- Injected into `PressToShape.App.Tests.LayersViewModelTests.Fixture`
- Injected into `PressToShape.App.ViewModels.Editor.ObjectRowViewModel`

## NumberFieldViewModel (class)

`internal partial sealed class NumberFieldViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/NumberFieldViewModel.cs:13-124`

A number text box of the Properties tab. The text is typed by the user and applied with PressToShape.App.ViewModels.Editor.NumberFieldViewModel.Commit (Enter or leaving the box). Invalid text is never applied: the previous value comes back. Numbers are shown and parsed with the language's culture; a dot is accepted as the decimal separator in every language.

**Members**

- `public NumberFieldViewModel(ILocalizer localizer, double minimum, double maximum, Action<double> apply)` - _(no documentation)_
- `public double Minimum { get; }` - Smallest accepted value.
- `public double Maximum { get; }` - Largest accepted value.
- `public string Text { get; set; }` - _(no documentation)_
- `public bool IsEnabled { get; private set; }` - _(no documentation)_
- `public bool IsMixed { get; private set; }` - _(no documentation)_
- `public bool IsEditing { get; set; }` - _(no documentation)_
- `public void SetValue(double? value, bool mixed = false)` - Shows a value from the document. A null value disables the box.
- `public void Commit()` - Applies the typed text. Text that is empty, not a finite number, or outside the range is refused and the shown value comes back. A value equal to the shown one (at the shown precision) is not applied.
- `public void Revert()` - Puts the shown value back into the text, discarding what the user typed.
- `public string Format(double value)` - The number as the box shows it: at most 3 decimals, no trailing zeros, in the language's culture.
- `protected void OnLanguageRefreshed()` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## ObjectMenuCommands (record)

`internal sealed record ObjectMenuCommands : IEquatable<ObjectMenuCommands>`

Source: `src/PressToShape.App/ViewModels/Editor/ObjectMenuCommands.cs:6-16`

What the Group, Arrange, Lock and Hide items of the Object menu do. A command is disabled while it would change nothing.

**Members**

- `public ObjectMenuCommands(ICommand Group, ICommand Ungroup, ICommand BringToFront, ICommand BringForward, ICommand SendBackward, ICommand SendToBack, ICommand Lock, ICommand UnlockAll, ICommand Hide, ICommand ShowAll)` - What the Group, Arrange, Lock and Hide items of the Object menu do. A command is disabled while it would change nothing.
- `public ICommand Group { get; init; }` - _(no documentation)_
- `public ICommand Ungroup { get; init; }` - _(no documentation)_
- `public ICommand BringToFront { get; init; }` - _(no documentation)_
- `public ICommand BringForward { get; init; }` - _(no documentation)_
- `public ICommand SendBackward { get; init; }` - _(no documentation)_
- `public ICommand SendToBack { get; init; }` - _(no documentation)_
- `public ICommand Lock { get; init; }` - _(no documentation)_
- `public ICommand UnlockAll { get; init; }` - _(no documentation)_
- `public ICommand Hide { get; init; }` - _(no documentation)_
- `public ICommand ShowAll { get; init; }` - _(no documentation)_

**Relationships**

- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel`

## ObjectRowType (enum)

`internal enum ObjectRowType`

Source: `src/PressToShape.App/ViewModels/Editor/ObjectRowType.cs:4-20`

What a row of the object list stands for. It decides the icon and the default name.

**Members**

- `public Layer` - A layer. The row is a header with the layer's name, and has no toggles.
- `public Rectangle` - A rectangle.
- `public Ellipse` - An ellipse.
- `public Group` - A group, which has rows of its own below it.
- `public Path` - A path.

## ObjectRowViewModel (class)

`internal partial sealed class ObjectRowViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/ObjectRowViewModel.cs:11-140`

One row of the object list: a layer header, a shape or a group. The values are display values that PressToShape.App.ViewModels.Editor.LayersViewModel copies from the document. The commands ask the list to change the document, they never change these values themselves.

**Members**

- `public IRelayCommand BeginRenameCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.BeginRename`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ObjectRowViewModel.BeginRename.
- `public IRelayCommand CancelRenameCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CancelRename`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CancelRename.
- `public IRelayCommand CommitRenameCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CommitRename`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ObjectRowViewModel.CommitRename.
- `public IRelayCommand ToggleExpandedCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleExpanded`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleExpanded.
- `public IRelayCommand ToggleLockCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleLock`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleLock.
- `public IRelayCommand ToggleVisibilityCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleVisibility`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ObjectRowViewModel.ToggleVisibility.
- `public ObjectRowViewModel(LayersViewModel owner, Guid id, ObjectRowType type, int depth, Guid parentId)` - _(no documentation)_
- `public Guid Id { get; }` - The id of the layer, shape or group.
- `public ObjectRowType Type { get; }` - What the row stands for.
- `public int Depth { get; }` - How deep the row is: 0 for a layer, 1 for what is on the top level of a layer, 2 below a group, and so on.
- `public Guid ParentId { get; }` - The id of the layer or group that holds this row. The layer's own row has its own id here.
- `public bool IsLayer { get; }` - Whether the row is a layer header.
- `public bool IsGroup { get; }` - Whether the row is a group, so it can be opened and closed.
- `public bool IsRectangle { get; }` - Whether the row is a rectangle.
- `public bool IsEllipse { get; }` - Whether the row is an ellipse.
- `public bool IsPath { get; }` - Whether the row is a path.
- `public bool HasChildren { get; }` - Whether the row has an expander: layers and groups.
- `public bool CanRename { get; }` - Whether the row can be renamed in the list. A layer is renamed elsewhere.
- `public string Name { get; private set; }` - _(no documentation)_
- `public string StoredName { get; private set; }` - _(no documentation)_
- `public bool IsVisible { get; private set; }` - _(no documentation)_
- `public bool IsLocked { get; private set; }` - _(no documentation)_
- `public bool IsEffectivelyHidden { get; private set; }` - _(no documentation)_
- `public bool IsEffectivelyLocked { get; private set; }` - _(no documentation)_
- `public bool IsActiveLayer { get; internal set; }` - _(no documentation)_
- `public bool IsSelected { get; internal set; }` - _(no documentation)_
- `public bool IsCurrent { get; internal set; }` - _(no documentation)_
- `public bool IsExpanded { get; internal set; }` - _(no documentation)_
- `public bool IsDropTarget { get; internal set; }` - _(no documentation)_
- `public bool IsRenaming { get; private set; }` - _(no documentation)_
- `public string EditText { get; set; }` - _(no documentation)_
- `public void Update(string name, string storedName, bool isVisible, bool isLocked, bool effectivelyHidden, bool effectivelyLocked)` - Copies the display values. Called when the document changed.
- `public void BeginRename()` - Starts editing the name.
- `public void CommitRename()` - Gives the object the typed name as one step and ends the editing. An empty name removes the name.
- `public void CancelRename()` - Ends the editing and keeps the old name.

**Relationships**

- Depends on (constructor) `PressToShape.App.ViewModels.Editor.LayersViewModel`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## PaletteViewModel (class)

`internal sealed class PaletteViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/PaletteViewModel.cs:15-76`

The swatches saved with the document, shown in the Appearance section. Adding and removing a swatch is one undo step. Applying a swatch sets the fill or the stroke like a color chosen in the picker.

**Members**

- `public PaletteViewModel(EditorState state, ColorFieldViewModel fill, ColorFieldViewModel stroke)` - _(no documentation)_
- `public IReadOnlyList<Color> Swatches { get; }` - The swatches of the current document, in order.
- `public bool IsEmpty { get; }` - Whether the palette has no swatches, for the hint that replaces the list.
- `public void AddSwatch(Color color)` - Adds the color at the end of the palette. Does nothing if the palette has it.
- `public void RemoveSwatch(int index)` - Removes the swatch at this index. Does nothing for an index outside the palette.
- `public void RemoveSwatch(Color color)` - Removes the swatch with this color. Does nothing if the palette does not have it.
- `public void Apply(Color color, bool toStroke)` - Sets the fill (or the stroke) of the selection to the color. Without a selection it sets the default for new shapes.

**Relationships**

- Depends on (constructor) `PressToShape.App.ViewModels.Editor.ColorFieldViewModel`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## PlaceholderPanelViewModel (class)

`internal sealed class PlaceholderPanelViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/PlaceholderPanelViewModel.cs:6-21`

An inspector tab that only shows an empty-state message (Assets). Title and message are message ids.

**Members**

- `public PlaceholderPanelViewModel(ILocalizer localizer, string title, string message)` - _(no documentation)_
- `public string Title { get; }` - _(no documentation)_
- `public string Message { get; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## PropertiesViewModel (class)

`internal partial sealed class PropertiesViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.Node.cs:10-90`

The Properties tab: the position, size, rotation and appearance of the selected shapes, and the section expansion. The fields show the current values (updated in place while shapes are dragged or edited elsewhere) and every committed edit is one undo step.

**Members**

- `public IRelayCommand ClosePathCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ClosePath`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.ClosePath.
- `public IRelayCommand GroupCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Group`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.Group.
- `public IRelayCommand MakeNodeCornerCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeNodeCorner`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeNodeCorner.
- `public IRelayCommand MakeNodeSmoothCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeNodeSmooth`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeNodeSmooth.
- `public IRelayCommand MakeNodeSymmetricCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeNodeSymmetric`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeNodeSymmetric.
- `public IRelayCommand MakeSegmentCurvedCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeSegmentCurved`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeSegmentCurved.
- `public IRelayCommand MakeSegmentStraightCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeSegmentStraight`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.MakeSegmentStraight.
- `public IRelayCommand OpenPathCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.OpenPath`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.OpenPath.
- `public IRelayCommand ResetTransformCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ResetTransform`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.ResetTransform.
- `public IRelayCommand UngroupCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.Ungroup`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.Ungroup.
- `public IRelayCommand UseEvenOddFillCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.UseEvenOddFill`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.UseEvenOddFill.
- `public IRelayCommand UseNonZeroFillCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.UseNonZeroFill`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.UseNonZeroFill.
- `public bool HasPathSelected { get; }` - Whether the selection has at least one path (also inside a selected group).
- `public bool IsNodeExpanded { get; set; }` - _(no documentation)_
- `public bool IsPathClosed { get; }` - Whether every selected path is closed.
- `public NumberFieldViewModel NodeX { get; }` - The X of the only selected node on the screen, in document units. Disabled without exactly one selected node.
- `public bool IsPathOpen { get; }` - Whether every selected path is open.
- `public NumberFieldViewModel NodeY { get; }` - The Y of the only selected node on the screen, in document units. Disabled without exactly one selected node.
- `public bool IsFillRuleNonZero { get; }` - Whether every selected path uses the non-zero fill rule.
- `public bool ShowNode { get; }` - Whether the Node section is shown: for Direct Select and the Node tool.
- `public bool IsFillRuleEvenOdd { get; }` - Whether every selected path uses the even-odd fill rule.
- `public bool ShowNodeTypeTools { get; }` - Whether the buttons for the node type and the segments are shown: only the Node tool changes them.
- `public bool HasNodesSelected { get; }` - Whether at least one node that can be edited is selected.
- `public bool IsNodeCorner { get; }` - Whether every selected node is a corner.
- `public PropertiesViewModel(EditorState state, SelectionEditor editor, ILocalizer localizer, ObjectEditor? objects = null)` - _(no documentation)_
- `public bool IsNodeSmooth { get; }` - Whether every selected node is smooth.
- `public bool IsNodeSymmetric { get; }` - Whether every selected node is symmetric.
- `public bool CanChangeSegment { get; }` - Whether exactly two neighboring nodes of one path are selected.
- `public NumberFieldViewModel X { get; }` - _(no documentation)_
- `public NumberFieldViewModel Y { get; }` - _(no documentation)_
- `public NumberFieldViewModel Width { get; }` - _(no documentation)_
- `public NumberFieldViewModel Height { get; }` - _(no documentation)_
- `public NumberFieldViewModel Rotation { get; }` - The rotation in degrees. Disabled for several shapes.
- `public NumberFieldViewModel StrokeWidth { get; }` - _(no documentation)_
- `public NumberFieldViewModel OpacityPercent { get; }` - The opacity from 0 to 100.
- `public ColorFieldViewModel Fill { get; }` - _(no documentation)_
- `public ColorFieldViewModel Stroke { get; }` - _(no documentation)_
- `public PaletteViewModel Palette { get; }` - The swatches saved with the document.
- `public IReadOnlyList<Color> RecentColors { get; }` - The colors picked lately, most recent first.
- `public void CommitPendingEdits()` - Applies every number field that the user is still typing in, as if it had been left. For a layout that hides the fields without moving the focus first, so a typed value is not lost.
- `public bool HasSelection { get; }` - _(no documentation)_
- `public bool ShowNothingSelectedHint { get; }` - _(no documentation)_
- `public bool ShowTransform { get; }` - _(no documentation)_
- `public bool ShowAppearance { get; }` - _(no documentation)_
- `public bool ShowPath { get; }` - _(no documentation)_
- `public bool ShowAlign { get; }` - _(no documentation)_
- `public bool ShowQuickActions { get; }` - _(no documentation)_
- `public bool ShowHeader { get; }` - Whether the selection header is shown: any tool that has at least one section.
- `public bool ShowNoPropertiesHint { get; }` - Whether the active tool has no properties, so a hint replaces the sections.
- `public string Header { get; }` - "Nothing selected", "1 selected" or "N selected".
- `public double OpacitySliderValue { get; private set; }` - _(no documentation)_
- `public bool KeepProportions { get; set; }` - _(no documentation)_
- `public bool IsTransformExpanded { get; set; }` - _(no documentation)_
- `public bool IsAppearanceExpanded { get; set; }` - _(no documentation)_
- `public bool IsPathExpanded { get; set; }` - _(no documentation)_
- `public bool IsAlignExpanded { get; set; }` - _(no documentation)_
- `public bool IsQuickActionsExpanded { get; set; }` - _(no documentation)_
- `public void StartPick(ColorPickTarget target)` - Starts a pick from the canvas for the fill or the stroke. The next click on a shape sets that color.
- `public void BeginOpacityDrag()` - The user started to move the opacity slider. The changes until PressToShape.App.ViewModels.Editor.PropertiesViewModel.EndOpacityDrag are one step.
- `public void PreviewOpacity(double percent)` - Shows the slider's opacity on the canvas. Ignored unless a drag began.
- `public void EndOpacityDrag()` - The slider was let go. Records what was shown as one step, if the opacity changed.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.ObjectEditor`
- Depends on (constructor) `PressToShape.App.Workspace.SelectionEditor`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`

## RingMenuContent (class)

`internal static class RingMenuContent`

Source: `src/PressToShape.App/ViewModels/Editor/RingMenuContent.cs:13-48`

The fixed slot tables of the ring menu. Every slot runs a command that the menus already have, so the history entries and the rules for when a command is available are the same. The order and the place of the slots never change, so users can learn them.

**Members**

- `public Func<RingMenuMode, IReadOnlyList<RingSlotDefinition>> Create(ObjectMenuCommands objects, EditMenuCommands edit)` - Gives the slots for a mode: the actions for a selection, or for the empty canvas.

## RingMenuViewModel (class)

`internal partial sealed class RingMenuViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/RingMenuViewModel.cs:14-180`

The state of the ring menu: whether it is open, where, which level of slots it shows, and which slot the pointer or the keyboard is on. The content comes from PressToShape.App.ViewModels.Editor.RingMenuViewModel.Content, so this class knows no commands. It lives as long as the canvas area.

**Members**

- `public RingMenuViewModel(ILocalizer localizer)` - _(no documentation)_
- `public Func<RingMenuMode, IReadOnlyList<RingSlotDefinition>>? Content { get; set; }` - Gives the slots for a mode. Until it is set, the ring does not open.
- `public bool IsOpen { get; private set; }` - _(no documentation)_
- `public double CenterX { get; private set; }` - _(no documentation)_
- `public double CenterY { get; private set; }` - _(no documentation)_
- `public IReadOnlyList<RingSlotViewModel> Slots { get; private set; }` - _(no documentation)_
- `public int? HoveredIndex { get; private set; }` - _(no documentation)_
- `public int? FocusedIndex { get; private set; }` - _(no documentation)_
- `public bool CanGoBack { get; }` - Whether the center goes back one level instead of closing the ring.
- `public string CenterActionName { get; }` - What the center does, translated: "Back" in a submenu, "Close" at the top. For the accessible name of the center.
- `public string CenterText { get; }` - The translated name of the slot under the pointer, else of the focused slot, else empty.
- `public void Open(RingMenuMode mode, double x, double y)` - Opens the ring at a point. Does nothing without content, or when the content has no slots for the mode.
- `public void Close()` - Closes the ring and forgets the levels, so the next opening starts at the top.
- `public void Activate(int index)` - Uses a slot. A submenu slot swaps the level. A command slot closes the ring first and then runs, so a command that changes the selection never meets an open ring. An unavailable slot does nothing.
- `public void ActivateCenter()` - The center was used: goes back one level, or closes at the top.
- `public void Back()` - Goes back one level, or closes at the top.
- `public void Hover(int? index)` - Sets the slot the pointer is over, or none. An index outside the slots counts as none.
- `public void MoveFocus(int step)` - Moves the keyboard focus by steps around the slots, wrapping at both ends. The first move lands on the first or the last slot.
- `protected void OnLanguageRefreshed()` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`

## RingSlotViewModel (class)

`internal partial sealed class RingSlotViewModel : ViewModelBase, IDisposable`

Source: `src/PressToShape.App/ViewModels/Editor/RingSlotViewModel.cs:16-77`

One wedge of the ring menu. Slots are made again for every level that is shown, so this is not a PressToShape.App.ViewModels.LocalizedViewModel (that one never unsubscribes from the localizer): the name is translated when it is read, and the ring calls PressToShape.App.ViewModels.Editor.RingSlotViewModel.Refresh on a language change. The slot listens to its command, or to all commands of its submenu, and must be disposed when it is no longer shown.

**Members**

- `public RingSlotViewModel(ILocalizer localizer, RingSlotDefinition definition)` - _(no documentation)_
- `public string Name { get; }` - The translated name.
- `public MaterialIconKind Icon { get; }` - _(no documentation)_
- `public bool HasChildren { get; }` - Whether the slot opens a submenu instead of running a command.
- `public IReadOnlyList<RingSlotDefinition> Children { get; }` - The entries of the submenu, or an empty list for a slot that runs a command.
- `public bool IsAvailable { get; private set; }` - _(no documentation)_
- `public bool Execute()` - Runs the command of the slot if it can run now. Returns whether it ran. A submenu slot runs nothing.
- `public void Refresh()` - Raises the change of the translated name, after a language change.
- `public void Dispose()` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.RingSlotDefinition`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## ToolItemViewModel (class)

`internal partial sealed class ToolItemViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/ToolItemViewModel.cs:10-37`

One button of the tool column.

**Members**

- `public IRelayCommand SelectCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ToolItemViewModel.Select`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ToolItemViewModel.Select.
- `public ToolItemViewModel(ILocalizer localizer, ToolDefinition definition, IRelayCommand<Tool> selectCommand, bool isActive)` - _(no documentation)_
- `public ToolDefinition Definition { get; }` - _(no documentation)_
- `public string Name { get; }` - The translated tool name.
- `public string ToolTip { get; }` - The translated name with the shortcut key, such as "Select (V)".
- `public MaterialIconKind Icon { get; }` - _(no documentation)_
- `public bool IsActive { get; set; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.ToolDefinition`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## ToolboxViewModel (class)

`internal partial sealed class ToolboxViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/ToolboxViewModel.cs:11-79`

State of the tool column: tool list, active tool, shortcut lookup, and the (not yet usable) color swatch commands.

**Members**

- `public IRelayCommand ResetColorsCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ToolboxViewModel.ResetColors`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ToolboxViewModel.ResetColors.
- `public IRelayCommand<Tool> SelectToolCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ToolboxViewModel.SelectTool(PressToShape.App.Workspace.Tool)`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand`1 instance wrapping PressToShape.App.ViewModels.Editor.ToolboxViewModel.SelectTool.
- `public IRelayCommand SwapColorsCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ToolboxViewModel.SwapColors`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ToolboxViewModel.SwapColors.
- `public ToolboxViewModel(EditorState state, ILocalizer localizer)` - _(no documentation)_
- `public IReadOnlyList<ToolItemViewModel> Tools { get; }` - All tools in display order.
- `public Tool ActiveTool { get; }` - _(no documentation)_
- `public bool TrySelectByKey(char key)` - Selects the tool for a shortcut key. Returns false if the key has no tool.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.ToolGroupViewModel`

## ZoomPresetViewModel (class)

`internal sealed class ZoomPresetViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/ZoomPresetViewModel.cs:8-23`

One entry of the zoom drop-down. It carries its own command so the menu needs no parent lookup.

**Members**

- `public ZoomPresetViewModel(double percent, Action apply)` - _(no documentation)_
- `public double Percent { get; }` - _(no documentation)_
- `public string Text { get; }` - _(no documentation)_
- `public ICommand SelectCommand { get; }` - _(no documentation)_

## ZoomViewModel (class)

`internal partial sealed class ZoomViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Editor/ZoomViewModel.cs:13-59`

The zoom as the user sees it: the text, the preset list and the commands. Shared by the desktop status bar and the tablet toolbar.

**Members**

- `public IRelayCommand FitToWindowCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ZoomViewModel.FitToWindow`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ZoomViewModel.FitToWindow.
- `public IRelayCommand<double> SetZoomCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ZoomViewModel.SetZoom(System.Double)`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand`1 instance wrapping PressToShape.App.ViewModels.Editor.ZoomViewModel.SetZoom.
- `public IRelayCommand ZoomInCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ZoomViewModel.ZoomIn`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ZoomViewModel.ZoomIn.
- `public IRelayCommand ZoomOutCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.ZoomViewModel.ZoomOut`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.ZoomViewModel.ZoomOut.
- `public ZoomViewModel(EditorState state, ILocalizer localizer, Action fitToWindow)` - _(no documentation)_
- `public double ZoomPercent { get; }` - _(no documentation)_
- `public string ZoomText { get; }` - The zoom as shown to the user, for example "100%" or "133.33%".
- `public IReadOnlyList<ZoomPresetViewModel> ZoomPresets { get; }` - The zoom values offered in the drop-down. Each entry carries its own command.
- `public string FormatZoom(double percent, CultureInfo culture)` - Formats a zoom value with the decimal separator of the given culture, for example "80.75%" or "80,75%".

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`
