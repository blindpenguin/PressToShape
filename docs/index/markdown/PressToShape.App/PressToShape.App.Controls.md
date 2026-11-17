# PressToShape.App.Controls

Project: PressToShape.App

## CollapsibleSection (class)

`internal sealed class CollapsibleSection : HeaderedContentControl`

Source: `src/PressToShape.App/Controls/CollapsibleSection.cs:12-22`

A titled section whose whole header row toggles the content. Used by the inspector panels. The look is defined in Styles/Controls.axaml.

**Members**

- `public StyledProperty<bool> IsExpandedProperty` - _(no documentation)_
- `public bool IsExpanded { get; set; }` - _(no documentation)_

## DocumentCanvasControl (class)

`internal sealed class DocumentCanvasControl : Control`

Source: `src/PressToShape.App/Controls/DocumentCanvasControl.cs:16-533`

Draws the shapes of a document. It only draws: PressToShape.App.Workspace.CanvasScene decides what is drawn and where. Shape colors are document data, so this control uses no theme brushes. It does not handle pointer input.

**Members**

- `public StyledProperty<Document?> DocumentProperty` - _(no documentation)_
- `public StyledProperty<double> ZoomPercentProperty` - Zoom in percent. 100 means one document unit per screen pixel.
- `public StyledProperty<double> OriginXProperty` - Position of the artboard's left edge in this control, in screen pixels.
- `public StyledProperty<double> OriginYProperty` - Position of the artboard's top edge in this control, in screen pixels.
- `public StyledProperty<ScreenShape?> DraftProperty` - The shape that is being drawn, in screen space. It is not part of the document.
- `public StyledProperty<SelectionOverlay?> OverlayProperty` - What is drawn for the selection, in screen space: outlines, frame, handles and the box being dragged.
- `public StyledProperty<ScreenPenOverlay?> PenOverlayProperty` - The nodes and handles of the path that the Pen is drawing, in screen space. They are drawn over the shapes.
- `public StyledProperty<ScreenNodeOverlay?> NodeOverlayProperty` - The nodes and handles of the selected shapes for Direct Select and the Node tool, in screen space. They are drawn over the shapes.
- `public StyledProperty<Guid?> HiddenShapeIdProperty` - A shape that is not drawn because the draft takes its place (a path that is being continued). null hides nothing.
- `public StyledProperty<IBrush?> SelectionBrushProperty` - The brush of the selection outlines, handle outlines and the selection box. It comes from the theme.
- `public StyledProperty<IBrush?> SelectionHandleFillBrushProperty` - The fill of the handles. It comes from the theme.
- `public StyledProperty<double> HandleSizeProperty` - The side of a selection handle in screen pixels. It comes from the theme, so a layout for touch can draw larger handles.
- `public StyledProperty<Guid?> EnteredGroupIdProperty` - The entered group. Shapes outside it are drawn faded (see PressToShape.App.Workspace.CanvasScene). null when no group is entered.
- `public StyledProperty<double> DimOpacityProperty` - The opacity of the shapes outside the entered group, 0 to 1. It comes from the theme.
- `public DocumentCanvasControl()` - _(no documentation)_
- `public Document? Document { get; set; }` - _(no documentation)_
- `public double ZoomPercent { get; set; }` - _(no documentation)_
- `public double OriginX { get; set; }` - _(no documentation)_
- `public double OriginY { get; set; }` - _(no documentation)_
- `public ScreenShape? Draft { get; set; }` - _(no documentation)_
- `public SelectionOverlay? Overlay { get; set; }` - _(no documentation)_
- `public ScreenPenOverlay? PenOverlay { get; set; }` - _(no documentation)_
- `public ScreenNodeOverlay? NodeOverlay { get; set; }` - _(no documentation)_
- `public Guid? HiddenShapeId { get; set; }` - _(no documentation)_
- `public IBrush? SelectionBrush { get; set; }` - _(no documentation)_
- `public IBrush? SelectionHandleFillBrush { get; set; }` - _(no documentation)_
- `public Guid? EnteredGroupId { get; set; }` - _(no documentation)_
- `public double DimOpacity { get; set; }` - _(no documentation)_
- `public double HandleSize { get; set; }` - _(no documentation)_
- `public void Render(DrawingContext context)` - _(no documentation)_

## NumberFieldBehavior (class)

`internal static class NumberFieldBehavior`

Source: `src/PressToShape.App/Controls/NumberFieldBehavior.cs:13-60`

What a number text box does with focus and keys, for every view that shows PressToShape.App.ViewModels.Editor.NumberFieldViewModels: the value is applied when the box is left or Enter is pressed, and the shown value comes back on Escape. A text box takes part when its Tag holds its field view model. The handlers sit on the view, so keys in other text boxes keep their meaning. The view model decides what the value means.

**Members**

- `public void Attach(Interactive view)` - Lets the number fields below view commit and revert.

## OpenFlyoutTracker (class)

`internal sealed class OpenFlyoutTracker`

Source: `src/PressToShape.App/Controls/OpenFlyoutTracker.cs:12-46`

Knows which of some flyouts are open, so the Back button can close the last one that was opened before anything else happens. A menu is not part of a view model, so this is a view-only helper. Used by the tablet toolbar (overflow menu) and the tablet status bar (zoom list).

**Members**

- `public EventHandler? Closed` - Raised when a tracked flyout closed, however it was closed.
- `public void Track(params FlyoutBase?[] flyouts)` - Starts to watch the flyouts. Flyouts that are null are skipped.
- `public bool TryCloseLast()` - Closes the flyout that was opened last and is still open, if there is one.

## RingMenuControl (class)

`internal sealed class RingMenuControl : Panel`

Source: `src/PressToShape.App/Controls/RingMenuControl.cs:23-435`

The ring context menu. It fills the canvas area (so a press beside the ring is caught here and closes it), and draws the wedges, their icons and the center around the point where the ring was opened. It draws what PressToShape.App.ViewModels.Editor.RingMenuViewModel holds and forwards pointer and key input to it; PressToShape.App.Workspace.RingGeometry does the math. The colors and sizes come from the theme (see the styles of the classes ringWedge, ringIcon, ringCenter and ringCenterText).

**Members**

- `public StyledProperty<double> InnerRadiusProperty` - _(no documentation)_
- `public StyledProperty<double> OuterRadiusProperty` - _(no documentation)_
- `public RingMenuControl()` - _(no documentation)_
- `public double InnerRadius { get; set; }` - The radius of the center disc. Wedges start here.
- `public double OuterRadius { get; set; }` - The outer radius of the wedges.
- `public void AdoptPointer(IPointer pointer)` - Takes over a pointer that is already down, for the long press of a finger or a pen: the ring opened under it. Its release then uses the slot it is on, and does nothing on the center (where it started) or beside the ring.
- `protected void OnDataContextChanged(EventArgs e)` - _(no documentation)_
- `protected void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)` - _(no documentation)_
- `protected Size MeasureOverride(Size availableSize)` - _(no documentation)_
- `protected Size ArrangeOverride(Size finalSize)` - _(no documentation)_
- `protected void OnPointerMoved(PointerEventArgs e)` - _(no documentation)_
- `protected void OnPointerExited(PointerEventArgs e)` - _(no documentation)_
- `protected void OnPointerPressed(PointerPressedEventArgs e)` - _(no documentation)_
- `protected void OnPointerReleased(PointerReleasedEventArgs e)` - _(no documentation)_
- `protected void OnPointerCaptureLost(PointerCaptureLostEventArgs e)` - _(no documentation)_
- `protected void OnPointerWheelChanged(PointerWheelEventArgs e)` - _(no documentation)_
- `protected void OnKeyDown(KeyEventArgs e)` - _(no documentation)_

## RulerControl (class)

`internal sealed class RulerControl : Control`

Source: `src/PressToShape.App/Controls/RulerControl.cs:14-147`

A horizontal or vertical ruler. It asks PressToShape.App.Workspace.RulerTicks for the ticks of the current zoom and origin and draws them with the theme brushes. It does not know about documents or the canvas.

**Members**

- `public StyledProperty<Orientation> OrientationProperty` - _(no documentation)_
- `public StyledProperty<double> ZoomPercentProperty` - Zoom in percent. 100 means one document pixel per screen pixel.
- `public StyledProperty<double> OriginPxProperty` - Position of document zero along the ruler, in screen pixels.
- `public RulerControl()` - _(no documentation)_
- `public Orientation Orientation { get; set; }` - _(no documentation)_
- `public double ZoomPercent { get; set; }` - _(no documentation)_
- `public double OriginPx { get; set; }` - _(no documentation)_
- `public void Render(DrawingContext context)` - _(no documentation)_

## ToolGroupButton (class)

`public sealed class ToolGroupButton : ToolToggleButton`

Source: `src/PressToShape.App/Controls/ToolGroupButton.cs:16-115`

The button of a tool group in the tablet tool column. A tap selects the current tool of the group (see PressToShape.App.Controls.ToolToggleButton). Holding the pointer down for a while runs PressToShape.App.Controls.ToolGroupButton.LongPressCommand (which opens the flyout of the group), and the click that follows the release is then swallowed, so a long press never also selects the tool. The decision is made by PressToShape.App.Workspace.LongPressTracker; this control only passes it pointer events and the time. It looks like a plain toggle button, so the tool styles apply to it.

**Members**

- `public StyledProperty<ICommand?> LongPressCommandProperty` - _(no documentation)_
- `internal Func<long> Clock { get; set; }` - The time source. Tests replace it so they need no real time.
- `public ICommand? LongPressCommand { get; set; }` - Run when the button was held down long enough.
- `protected void OnPointerPressed(PointerPressedEventArgs e)` - _(no documentation)_
- `protected void OnPointerMoved(PointerEventArgs e)` - _(no documentation)_
- `protected void OnPointerReleased(PointerReleasedEventArgs e)` - _(no documentation)_
- `protected void OnPointerCaptureLost(PointerCaptureLostEventArgs e)` - _(no documentation)_
- `protected void OnClick()` - _(no documentation)_
- `protected void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)` - _(no documentation)_
- `internal void OnTimerTick()` - One tick of the timer. Public to the tests only.

**Relationships**

- Inherits `PressToShape.App.Controls.ToolToggleButton`

## ToolToggleButton (class)

`public class ToolToggleButton : ToggleButton`

Source: `src/PressToShape.App/Controls/ToolToggleButton.cs:12-20`

A toggle button for something whose state lives in a view model: a tool in the tool column, or the icon that shows whether a panel is open. Its checked state comes only from the binding. A tap runs the command and does not flip the button by itself, so tapping the active tool again keeps it active (a plain toggle button would show it as off while the tool is still the active one, and would also lose its binding). It looks like a plain toggle button, so the tool styles apply.

**Members**

- `protected Type StyleKeyOverride { get; }` - _(no documentation)_
- `protected void Toggle()` - _(no documentation)_

**Relationships**

- Inherited by `PressToShape.App.Controls.ToolGroupButton`

## TouchSizes (class)

`internal static class TouchSizes`

Source: `src/PressToShape.App/Controls/TouchSizes.cs:11-28`

The sizes for touch and stylus, defined in Styles/Theme.axaml as the dictionary TouchSizes. The tablet layout applies them to its root, so every control below it that looks a size up with DynamicResource gets the touch value, and the desktop keeps its own. The dictionary cannot simply be merged, because Avalonia allows a dictionary only one owner. Its entries are copied instead.

**Members**

- `public string Key` - _(no documentation)_
- `public void ApplyTo(Control root)` - Puts the touch sizes into the resources of root. Does nothing if the application has no such dictionary.
