# PressToShape.App.ViewModels.Editor

Project: PressToShape.App

## CanvasAreaViewModel (class)

`internal partial sealed class CanvasAreaViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs:13-449`

Geometry of the canvas area: where the artboard is drawn and where the ruler zero points are. The view only reports the viewport size, everything else is derived here.

**Members**

- `public CanvasAreaViewModel(EditorState state, SelectionEditor? editor = null)` - _(no documentation)_
- `public double ViewportWidth { get; set; }` - _(no documentation)_
- `public double ViewportHeight { get; set; }` - _(no documentation)_
- `public bool IsAltHeld { get; set; }` - _(no documentation)_
- `public Document Document { get; }` - The document the canvas draws.
- `public ScreenShape? Draft { get; }` - The shape that is being drawn, in screen space, or null.
- `public SelectionOverlay Overlay { get; }` - Everything the canvas draws for the selection: outlines, frame, handles and the box being dragged.
- `public CanvasCursorType CursorType { get; }` - The pointer shape: a closed hand while the view is being dragged, a resize or rotate cursor over or while dragging a handle, otherwise the shape of the active tool. The Zoom tool shows the zoom-out magnifier while Alt is held.
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
- `public void PointerPressed(double screenX, double screenY, bool alt = false, bool shift = false)` - The primary button went down. The position is in screen pixels of the canvas area. With the Hand tool this starts dragging the view, with the Zoom tool it zooms one step in (with alt: out) around the position, with the other tools it is handled by the tool.
- `public void MiddlePressed(double screenX, double screenY)` - The middle button went down: drags the view with any tool, unless another drag is in progress.
- `public void PointerMoved(double screenX, double screenY, bool shift = false, bool alt = false)` - The pointer moved. The position is in screen pixels of the canvas area.
- `public void PointerReleased(double screenX, double screenY, bool shift = false, bool alt = false)` - The button went up. The position is in screen pixels of the canvas area.
- `public void PointerHover(double screenX, double screenY)` - The pointer moved over the canvas without a drag: the handle under it decides the cursor.
- `public void PointerLeft()` - The pointer left the canvas: no handle is hovered any more.
- `public bool KeyPressed(CanvasKey key, bool shift)` - A key went down on the canvas. The arrow keys move the selection by 1 unit, with shift by 10. Escape clears the selection. Nothing happens during a drag.
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

Source: `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs:21-230`

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
- `public bool HasSelection { get; }` - _(no documentation)_
- `public bool ShowNothingSelectedHint { get; }` - _(no documentation)_
- `public string Header { get; }` - "Nothing selected", "1 selected" or "N selected".
- `public double OpacitySliderValue { get; private set; }` - _(no documentation)_
- `public bool KeepProportions { get; set; }` - _(no documentation)_
- `public bool IsTransformExpanded { get; set; }` - _(no documentation)_
- `public bool IsAppearanceExpanded { get; set; }` - _(no documentation)_
- `public bool IsPathExpanded { get; set; }` - _(no documentation)_
- `public bool IsAlignExpanded { get; set; }` - _(no documentation)_
- `public bool IsQuickActionsExpanded { get; set; }` - _(no documentation)_
- `public void BeginOpacityDrag()` - The user started to move the opacity slider. The changes until PressToShape.App.ViewModels.Editor.PropertiesViewModel.EndOpacityDrag are one step.
- `public void PreviewOpacity(double percent)` - Shows the slider's opacity on the canvas. Ignored unless a drag began.
- `public void EndOpacityDrag()` - The slider was let go. Records what was shown as one step, if the opacity changed.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.SelectionEditor`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

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
