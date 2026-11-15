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

Source: `src/PressToShape.App/Controls/DocumentCanvasControl.cs:15-284`

Draws the shapes of a document. It only draws: PressToShape.App.Workspace.CanvasScene decides what is drawn and where. Shape colors are document data, so this control uses no theme brushes. It does not handle pointer input.

**Members**

- `public StyledProperty<Document?> DocumentProperty` - _(no documentation)_
- `public StyledProperty<double> ZoomPercentProperty` - Zoom in percent. 100 means one document unit per screen pixel.
- `public StyledProperty<double> OriginXProperty` - Position of the artboard's left edge in this control, in screen pixels.
- `public StyledProperty<double> OriginYProperty` - Position of the artboard's top edge in this control, in screen pixels.
- `public StyledProperty<ScreenShape?> DraftProperty` - The shape that is being drawn, in screen space. It is not part of the document.
- `public StyledProperty<SelectionOverlay?> OverlayProperty` - What is drawn for the selection, in screen space: outlines, frame, handles and the box being dragged.
- `public StyledProperty<IBrush?> SelectionBrushProperty` - The brush of the selection outlines, handle outlines and the selection box. It comes from the theme.
- `public StyledProperty<IBrush?> SelectionHandleFillBrushProperty` - The fill of the handles. It comes from the theme.
- `public DocumentCanvasControl()` - _(no documentation)_
- `public Document? Document { get; set; }` - _(no documentation)_
- `public double ZoomPercent { get; set; }` - _(no documentation)_
- `public double OriginX { get; set; }` - _(no documentation)_
- `public double OriginY { get; set; }` - _(no documentation)_
- `public ScreenShape? Draft { get; set; }` - _(no documentation)_
- `public SelectionOverlay? Overlay { get; set; }` - _(no documentation)_
- `public IBrush? SelectionBrush { get; set; }` - _(no documentation)_
- `public IBrush? SelectionHandleFillBrush { get; set; }` - _(no documentation)_
- `public void Render(DrawingContext context)` - _(no documentation)_

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
