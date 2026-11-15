# PressToShape.App.ViewModels.Editor

Project: PressToShape.App

## CanvasAreaViewModel (class)

`internal partial sealed class CanvasAreaViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs:13-547`

Geometry of the canvas area: where the artboard is drawn and where the ruler zero points are. The view only reports the viewport size, everything else is derived here.

**Members**

- `public CanvasAreaViewModel(EditorState state, SelectionEditor? editor = null)` - _(no documentation)_
- `public double ViewportWidth { get; set; }` - _(no documentation)_
- `public double ViewportHeight { get; set; }` - _(no documentation)_
- `public bool IsAltHeld { get; set; }` - _(no documentation)_
- `public Document Document { get; }` - The document the canvas draws.
- `public ScreenShape? Draft { get; }` - The shape that is being drawn, in screen space, or null.
- `public SelectionOverlay Overlay { get; }` - Everything the canvas draws for the selection: outlines, frame, handles and the box being dragged.
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
- `public void TouchPressed(int id, double screenX, double screenY)` - A finger went down. One finger acts with the active tool like the mouse. A second finger undoes that action (see PressToShape.App.Workspace.CanvasInteraction.Abort) and the two fingers then move and zoom the view. Mouse and pen use PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.PointerPressed. The id only has to be unique from this press to the release.
- `public void TouchMoved(int id, double screenX, double screenY)` - A finger moved.
- `public void TouchReleased(int id, double screenX, double screenY)` - A finger went up.
- `public void TouchCancelled()` - The system took the fingers away, or the pointer capture was lost. All fingers are forgotten and an action of one finger that was not finished is undone. Does nothing if no finger is down, for example after a normal release.
- `public void MiddlePressed(double screenX, double screenY)` - The middle button went down: drags the view with any tool, unless another drag is in progress.
- `public void PointerMoved(double screenX, double screenY, bool shift = false, bool alt = false)` - The pointer moved. The position is in screen pixels of the canvas area.
- `public void PointerReleased(double screenX, double screenY, bool shift = false, bool alt = false)` - The button went up. The position is in screen pixels of the canvas area.
- `public void PointerHover(double screenX, double screenY)` - The pointer moved over the canvas without a drag: the handle under it decides the cursor.
- `public void PointerLeft()` - The pointer left the canvas: no handle is hovered any more.
- `public bool KeyPressed(CanvasKey key, bool shift)` - A key went down on the canvas. The arrow keys move the selection by 1 unit, with shift by 10. Escape clears the selection, or ends a pick from the canvas. Nothing else happens during a drag.
- `public void Cancel()` - Ends a drag without a result, for example when the pointer capture is lost.
- `public bool Wheel(double deltaX, double deltaY, bool control, bool shift, double screenX, double screenY)` - The wheel (or a touchpad scroll) turned over the canvas. With Ctrl it zooms around the pointer, otherwise it moves the view, Shift moving it horizontally. A positive delta moves the view toward the top or the left.
- `public void DeleteSelection()` - Removes the selected shapes. Does nothing without a selection.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.SelectionEditor`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

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

Source: `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs:17-215`

What every layout shell needs. Creates the shared PressToShape.App.Workspace.EditorState and the view model of every region that the layouts share (tool column, canvas, inspector), and owns the file, edit, undo and redo commands, so the regions of a layout always agree. The desktop and the tablet layout derive from it and add their own regions.

**Members**

- `public IRelayCommand CopyCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Copy`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Copy.
- `public IRelayCommand CutCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Cut`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Cut.
- `public IRelayCommand DeleteCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Delete`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Delete.
- `public IRelayCommand DeselectCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Deselect`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Deselect.
- `public IRelayCommand DuplicateCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Duplicate`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Duplicate.
- `public IAsyncRelayCommand ExportSvgCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.ExportSvgAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.ExportSvgAsync.
- `public IAsyncRelayCommand NewCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.NewAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.NewAsync.
- `public IAsyncRelayCommand OpenCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.OpenAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.OpenAsync.
- `public IRelayCommand PasteCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Paste`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Paste.
- `public IRelayCommand PasteInPlaceCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.PasteInPlace`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.PasteInPlace.
- `public IRelayCommand RedoCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Redo`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Redo.
- `public IAsyncRelayCommand SaveAsCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsAsync.
- `public IAsyncRelayCommand SaveCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsync.
- `public IRelayCommand SelectAllCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SelectAll`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.SelectAll.
- `public IRelayCommand UndoCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Undo`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.EditorShellViewModel.Undo.
- `protected EditorShellViewModel(ILocalizer localizer, FileServices services)` - _(no documentation)_
- `public EditorState State { get; }` - _(no documentation)_
- `public ToolboxViewModel Toolbox { get; }` - _(no documentation)_
- `public SelectionEditor Editor { get; }` - The selection edits (copy, paste, nudge, property changes). One instance, shared by the canvas, the inspector and the menus.
- `public CanvasAreaViewModel Canvas { get; }` - _(no documentation)_
- `public InspectorViewModel Inspector { get; }` - _(no documentation)_
- `public string DocumentName { get; }` - The file name of the document, or the translated word for "Untitled" while it has none.
- `public bool IsDirty { get; }` - Whether the document has changes that were not saved.
- `public string WindowTitle { get; }` - The window title: an asterisk while there are unsaved changes, the file name (or "Untitled") and the product name.
- `protected RecentFiles Recent { get; }` - The recently used files, for layouts that list them.
- `protected FileMenuCommands FileCommands { get; }` - The commands behind the file items of a menu.
- `protected EditMenuCommands EditCommands { get; }` - The commands behind the edit items of a menu.
- `public Task<bool> ConfirmCloseAsync()` - Asks about unsaved changes before the window closes. Returns whether closing may go on.
- `public void FitToWindow()` - Zooms so the artboard fits the canvas area. Does nothing until the canvas has a size.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.FileServices`
- Inherited by `PressToShape.App.Tests.EditorShellViewModelTests.BareShell`
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

Source: `src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs:10-52`

State of the inspector: the selected tab and the view models of its tabs.

**Members**

- `public InspectorViewModel(EditorState state, ILocalizer localizer, SelectionEditor? editor = null)` - _(no documentation)_
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
- Depends on (constructor) `PressToShape.App.Workspace.SelectionEditor`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`

## LayerItemViewModel (class)

`internal partial sealed class LayerItemViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/LayerItemViewModel.cs:14-51`

One row of the layer list. Name, visibility and lock are display values copied from the document by PressToShape.App.ViewModels.Editor.LayerItemViewModel.Update. The toggle commands edit the document through the editor state, they never change these values directly.

**Members**

- `public IRelayCommand ToggleLockCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.LayerItemViewModel.ToggleLock`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.LayerItemViewModel.ToggleLock.
- `public IRelayCommand ToggleVisibilityCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.LayerItemViewModel.ToggleVisibility`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.LayerItemViewModel.ToggleVisibility.
- `public LayerItemViewModel(EditorState state, Layer layer)` - _(no documentation)_
- `public Guid LayerId { get; }` - _(no documentation)_
- `public string Name { get; private set; }` - _(no documentation)_
- `public bool IsVisible { get; private set; }` - _(no documentation)_
- `public bool IsLocked { get; private set; }` - _(no documentation)_
- `public void Update(Layer layer)` - Copies the display values from the layer. Called when the document changed.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.Core.Documents.Layer`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## LayersViewModel (class)

`internal partial sealed class LayersViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/LayersViewModel.cs:14-127`

State of the Layers tab: the layers of the current document, topmost first. The list follows the document. When only names, visibility or locks changed, the existing items are updated, so the list is not rebuilt under the user's click.

**Members**

- `public IRelayCommand AddLayerCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.LayersViewModel.AddLayer`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.LayersViewModel.AddLayer.
- `public LayersViewModel(EditorState state)` - _(no documentation)_
- `public ObservableCollection<LayerItemViewModel> Items { get; }` - _(no documentation)_
- `public LayerItemViewModel? SelectedItem { get; set; }` - The row of the active layer, the layer new shapes go into. Setting it changes the active layer. null is ignored: there is always an active layer, and the list control sends null while it is rebuilt.

**Relationships**

- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

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

Source: `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs:21-354`

The Properties tab: the position, size, rotation and appearance of the selected shapes, and the section expansion. The fields show the current values (updated in place while shapes are dragged or edited elsewhere) and every committed edit is one undo step.

**Members**

- `public IRelayCommand ResetTransformCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Editor.PropertiesViewModel.ResetTransform`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Editor.PropertiesViewModel.ResetTransform.
- `public PropertiesViewModel(EditorState state, SelectionEditor editor, ILocalizer localizer)` - _(no documentation)_
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
- Depends on (constructor) `PressToShape.App.Workspace.SelectionEditor`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`

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
