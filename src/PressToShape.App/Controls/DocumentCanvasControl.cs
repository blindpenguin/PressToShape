using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using CoreColor = PressToShape.Core.Shapes.Color;
using CoreFillRule = PressToShape.Core.Shapes.FillRule;

namespace PressToShape.App.Controls;

/// <summary>
/// Draws the shapes of a document. It only draws: <see cref="CanvasScene"/> decides what is drawn and where.
/// Shape colors are document data, so this control uses no theme brushes. It does not handle pointer input.
/// </summary>
internal sealed class DocumentCanvasControl : Control
{
    public static readonly StyledProperty<Document?> DocumentProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, Document?>(nameof(Document));

    /// <summary>Zoom in percent. 100 means one document unit per screen pixel.</summary>
    public static readonly StyledProperty<double> ZoomPercentProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, double>(nameof(ZoomPercent), 100);

    /// <summary>Position of the artboard's left edge in this control, in screen pixels.</summary>
    public static readonly StyledProperty<double> OriginXProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, double>(nameof(OriginX));

    /// <summary>Position of the artboard's top edge in this control, in screen pixels.</summary>
    public static readonly StyledProperty<double> OriginYProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, double>(nameof(OriginY));

    /// <summary>The shape that is being drawn, in screen space. It is not part of the document.</summary>
    public static readonly StyledProperty<ScreenShape?> DraftProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, ScreenShape?>(nameof(Draft));

    /// <summary>What is drawn for the selection, in screen space: outlines, frame, handles and the box being dragged.</summary>
    public static readonly StyledProperty<SelectionOverlay?> OverlayProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, SelectionOverlay?>(nameof(Overlay));

    /// <summary>The nodes and handles of the path that the Pen is drawing, in screen space. They are drawn over the shapes.</summary>
    public static readonly StyledProperty<ScreenPenOverlay?> PenOverlayProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, ScreenPenOverlay?>(nameof(PenOverlay));

    /// <summary>The nodes and handles of the selected shapes for Direct Select and the Node tool, in screen space. They are drawn over the shapes.</summary>
    public static readonly StyledProperty<ScreenNodeOverlay?> NodeOverlayProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, ScreenNodeOverlay?>(nameof(NodeOverlay));

    /// <summary>A shape that is not drawn because the draft takes its place (a path that is being continued). <c>null</c> hides nothing.</summary>
    public static readonly StyledProperty<System.Guid?> HiddenShapeIdProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, System.Guid?>(nameof(HiddenShapeId));

    /// <summary>The brush of the selection outlines, handle outlines and the selection box. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(SelectionBrush));

    /// <summary>The fill of the handles. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> SelectionHandleFillBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(SelectionHandleFillBrush));

    /// <summary>The side of a selection handle in screen pixels. It comes from the theme, so a layout for touch can draw larger handles.</summary>
    public static readonly StyledProperty<double> HandleSizeProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, double>(nameof(HandleSize), defaultValue: SelectionOverlay.HandleSize);

    /// <summary>The entered group. Shapes outside it are drawn faded (see <see cref="CanvasScene"/>). <c>null</c> when no group is entered.</summary>
    public static readonly StyledProperty<System.Guid?> EnteredGroupIdProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, System.Guid?>(nameof(EnteredGroupId));

    /// <summary>The opacity of the shapes outside the entered group, 0 to 1. It comes from the theme.</summary>
    public static readonly StyledProperty<double> DimOpacityProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, double>(nameof(DimOpacity), defaultValue: 0.35);

    /// <summary>Whether the grid is drawn, behind the shapes.</summary>
    public static readonly StyledProperty<bool> ShowGridProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, bool>(nameof(ShowGrid));

    /// <summary>The distance between grid lines in document units.</summary>
    public static readonly StyledProperty<double> GridSpacingProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, double>(nameof(GridSpacing), defaultValue: 10);

    /// <summary>The brush of the grid lines. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> GridBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(GridBrush));

    /// <summary>The guide lines of the object a drag is snapped to, in screen space. They are drawn over everything else.</summary>
    public static readonly StyledProperty<IReadOnlyList<ScreenSnapGuide>?> SnapGuidesProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IReadOnlyList<ScreenSnapGuide>?>(nameof(SnapGuides));

    /// <summary>The brush of the snap guide lines. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> GuideBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(GuideBrush));

    /// <summary>The ruler guides in screen space. They are drawn as lines across the whole control, over the shapes.</summary>
    public static readonly StyledProperty<IReadOnlyList<ScreenRulerGuide>?> RulerGuidesProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IReadOnlyList<ScreenRulerGuide>?>(nameof(RulerGuides));

    /// <summary>The markers and labels of the smart guides in screen space. They are drawn last.</summary>
    public static readonly StyledProperty<IReadOnlyList<ScreenSmartMarker>?> SmartMarkersProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IReadOnlyList<ScreenSmartMarker>?>(nameof(SmartMarkers));

    /// <summary>The brush of a ruler guide. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> RulerGuideBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(RulerGuideBrush));

    /// <summary>The brush of the selected ruler guide. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> RulerGuideSelectedBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(RulerGuideSelectedBrush));

    /// <summary>The brush of a ruler guide that the drag is snapped to. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> RulerGuideActiveBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(RulerGuideActiveBrush));

    /// <summary>The brush of the lines of the smart guide markers. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> SmartGuideBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(SmartGuideBrush));

    /// <summary>The brush of the numbers of the smart guide markers. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> SmartGuideTextBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(SmartGuideTextBrush));

    /// <summary>The background of the numbers of the smart guide markers. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> SmartGuideLabelBackgroundBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(SmartGuideLabelBackgroundBrush));

    // The size of the numbers of the smart guides in screen pixels. It stays the same at every zoom, so the numbers stay readable.
    private const double LabelFontSize = 11;

    // How far the bracket of an equal size marker is from the side it measures, and how long its end ticks are, in screen pixels.
    private const double BracketDistance = 6;
    private const double TickLength = 4;

    // The arm length of the cross that marks a path point, in screen pixels.
    private const double CrossArm = 4;

    // The outlines keep this width at every zoom.
    private const double OutlineThickness = 1;

    // The area of the dragged selection box is filled with the selection brush at this opacity.
    private const double SelectionBoxFillOpacity = 0.12;

    static DocumentCanvasControl()
    {
        AffectsRender<DocumentCanvasControl>(
            DocumentProperty,
            ZoomPercentProperty,
            OriginXProperty,
            OriginYProperty,
            DraftProperty,
            OverlayProperty,
            PenOverlayProperty,
            NodeOverlayProperty,
            HiddenShapeIdProperty,
            SelectionBrushProperty,
            SelectionHandleFillBrushProperty,
            HandleSizeProperty,
            EnteredGroupIdProperty,
            DimOpacityProperty,
            ShowGridProperty,
            GridSpacingProperty,
            GridBrushProperty,
            SnapGuidesProperty,
            GuideBrushProperty,
            RulerGuidesProperty,
            SmartMarkersProperty,
            RulerGuideBrushProperty,
            RulerGuideSelectedBrushProperty,
            RulerGuideActiveBrushProperty,
            SmartGuideBrushProperty,
            SmartGuideTextBrushProperty,
            SmartGuideLabelBackgroundBrushProperty);
    }

    public DocumentCanvasControl()
    {
        IsHitTestVisible = false;
    }

    public Document? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public double ZoomPercent
    {
        get => GetValue(ZoomPercentProperty);
        set => SetValue(ZoomPercentProperty, value);
    }

    public double OriginX
    {
        get => GetValue(OriginXProperty);
        set => SetValue(OriginXProperty, value);
    }

    public double OriginY
    {
        get => GetValue(OriginYProperty);
        set => SetValue(OriginYProperty, value);
    }

    public ScreenShape? Draft
    {
        get => GetValue(DraftProperty);
        set => SetValue(DraftProperty, value);
    }

    public SelectionOverlay? Overlay
    {
        get => GetValue(OverlayProperty);
        set => SetValue(OverlayProperty, value);
    }

    public ScreenPenOverlay? PenOverlay
    {
        get => GetValue(PenOverlayProperty);
        set => SetValue(PenOverlayProperty, value);
    }

    public ScreenNodeOverlay? NodeOverlay
    {
        get => GetValue(NodeOverlayProperty);
        set => SetValue(NodeOverlayProperty, value);
    }

    public System.Guid? HiddenShapeId
    {
        get => GetValue(HiddenShapeIdProperty);
        set => SetValue(HiddenShapeIdProperty, value);
    }

    public IBrush? SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public IBrush? SelectionHandleFillBrush
    {
        get => GetValue(SelectionHandleFillBrushProperty);
        set => SetValue(SelectionHandleFillBrushProperty, value);
    }

    public System.Guid? EnteredGroupId
    {
        get => GetValue(EnteredGroupIdProperty);
        set => SetValue(EnteredGroupIdProperty, value);
    }

    public double DimOpacity
    {
        get => GetValue(DimOpacityProperty);
        set => SetValue(DimOpacityProperty, value);
    }

    public double HandleSize
    {
        get => GetValue(HandleSizeProperty);
        set => SetValue(HandleSizeProperty, value);
    }

    public bool ShowGrid
    {
        get => GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public double GridSpacing
    {
        get => GetValue(GridSpacingProperty);
        set => SetValue(GridSpacingProperty, value);
    }

    public IBrush? GridBrush
    {
        get => GetValue(GridBrushProperty);
        set => SetValue(GridBrushProperty, value);
    }

    public IReadOnlyList<ScreenSnapGuide>? SnapGuides
    {
        get => GetValue(SnapGuidesProperty);
        set => SetValue(SnapGuidesProperty, value);
    }

    public IReadOnlyList<ScreenRulerGuide>? RulerGuides
    {
        get => GetValue(RulerGuidesProperty);
        set => SetValue(RulerGuidesProperty, value);
    }

    public IReadOnlyList<ScreenSmartMarker>? SmartMarkers
    {
        get => GetValue(SmartMarkersProperty);
        set => SetValue(SmartMarkersProperty, value);
    }

    public IBrush? RulerGuideBrush
    {
        get => GetValue(RulerGuideBrushProperty);
        set => SetValue(RulerGuideBrushProperty, value);
    }

    public IBrush? RulerGuideSelectedBrush
    {
        get => GetValue(RulerGuideSelectedBrushProperty);
        set => SetValue(RulerGuideSelectedBrushProperty, value);
    }

    public IBrush? RulerGuideActiveBrush
    {
        get => GetValue(RulerGuideActiveBrushProperty);
        set => SetValue(RulerGuideActiveBrushProperty, value);
    }

    public IBrush? SmartGuideBrush
    {
        get => GetValue(SmartGuideBrushProperty);
        set => SetValue(SmartGuideBrushProperty, value);
    }

    public IBrush? SmartGuideTextBrush
    {
        get => GetValue(SmartGuideTextBrushProperty);
        set => SetValue(SmartGuideTextBrushProperty, value);
    }

    public IBrush? SmartGuideLabelBackgroundBrush
    {
        get => GetValue(SmartGuideLabelBackgroundBrushProperty);
        set => SetValue(SmartGuideLabelBackgroundBrushProperty, value);
    }

    public IBrush? GuideBrush
    {
        get => GetValue(GuideBrushProperty);
        set => SetValue(GuideBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        if (Document is not { } document)
        {
            return;
        }

        DrawGrid(context);

        var dim = double.IsFinite(DimOpacity) ? System.Math.Clamp(DimOpacity, 0, 1) : 1;
        foreach (var shape in CanvasScene.Build(document, ZoomPercent, OriginX, OriginY, EnteredGroupId, HiddenShapeId))
        {
            if (shape.IsDimmed)
            {
                using (context.PushOpacity(dim))
                {
                    DrawWithOpacity(context, shape);
                }
            }
            else
            {
                DrawWithOpacity(context, shape);
            }
        }

        if (Draft is { } draft)
        {
            DrawWithOpacity(context, draft);
        }

        if (Overlay is { } overlay && SelectionBrush is { } brush)
        {
            DrawOverlay(context, overlay, brush);
        }

        if (PenOverlay is { } penOverlay && SelectionBrush is { } penBrush)
        {
            DrawPenOverlay(context, penOverlay, penBrush);
        }

        if (NodeOverlay is { } nodeOverlay && SelectionBrush is { } nodeBrush)
        {
            DrawNodeOverlay(context, nodeOverlay, nodeBrush);
        }

        DrawRulerGuides(context);
        DrawGuides(context);
        DrawSmartMarkers(context);
    }

    // Thin lines on the artboard, behind the shapes. Every line sits on the middle of a pixel, so it stays sharp.
    private void DrawGrid(DrawingContext context)
    {
        if (!ShowGrid || GridBrush is not { } brush || Document is not { } document)
        {
            return;
        }

        // Only the artboard has a grid. The part of it that is on the control gives the rectangle for the lines.
        var scale = ZoomPercent / 100;
        var left = System.Math.Max(0, OriginX);
        var top = System.Math.Max(0, OriginY);
        var right = System.Math.Min(Bounds.Width, OriginX + document.ArtboardWidth * scale);
        var bottom = System.Math.Min(Bounds.Height, OriginY + document.ArtboardHeight * scale);
        if (!(right > left) || !(bottom > top))
        {
            return;
        }

        var lines = GridLines.BuildInScreenRect(GridSpacing, ZoomPercent, OriginX, OriginY, left, top, right, bottom);
        var pen = new Pen(brush, OutlineThickness);
        foreach (var x in lines.Vertical)
        {
            var at = System.Math.Floor(x) + 0.5;
            context.DrawLine(pen, new Point(at, top), new Point(at, bottom));
        }

        foreach (var y in lines.Horizontal)
        {
            var at = System.Math.Floor(y) + 0.5;
            context.DrawLine(pen, new Point(left, at), new Point(right, at));
        }
    }

    // The ruler guides: thin lines across the whole control. The guide that the drag is snapped to and the selected guide have their own color.
    private void DrawRulerGuides(DrawingContext context)
    {
        if (RulerGuides is not { Count: > 0 } guides)
        {
            return;
        }

        foreach (var guide in guides)
        {
            var brush = guide.IsActive ? RulerGuideActiveBrush : guide.IsSelected ? RulerGuideSelectedBrush : RulerGuideBrush;
            if (brush is null || !double.IsFinite(guide.Position))
            {
                continue;
            }

            var pen = new Pen(brush, OutlineThickness);
            var at = System.Math.Floor(guide.Position) + 0.5;
            context.DrawLine(pen, guide.IsVertical ? new Point(at, 0) : new Point(0, at), guide.IsVertical ? new Point(at, Bounds.Height) : new Point(Bounds.Width, at));
        }
    }

    // The markers of the smart guides. A distance or an equal gap is a measure line with end ticks, an equal size a bracket beside the side
    // that it measures, a path point a small cross. A marker with a text gets the number on a small background.
    private void DrawSmartMarkers(DrawingContext context)
    {
        if (SmartMarkers is not { Count: > 0 } markers || SmartGuideBrush is not { } brush)
        {
            return;
        }

        var pen = new Pen(brush, OutlineThickness);
        foreach (var marker in markers)
        {
            if (!double.IsFinite(marker.From) || !double.IsFinite(marker.To) || !double.IsFinite(marker.Cross))
            {
                continue;
            }

            switch (marker.Type)
            {
                case SmartMarkerType.PathPoint:
                    var center = marker.IsHorizontal ? new Point(marker.From, marker.Cross) : new Point(marker.Cross, marker.From);
                    context.DrawLine(pen, new Point(center.X - CrossArm, center.Y - CrossArm), new Point(center.X + CrossArm, center.Y + CrossArm));
                    context.DrawLine(pen, new Point(center.X - CrossArm, center.Y + CrossArm), new Point(center.X + CrossArm, center.Y - CrossArm));
                    break;
                case SmartMarkerType.EqualSize:
                    DrawMeasure(context, pen, marker, marker.Cross + BracketDistance, -TickLength, 0, labelOutside: true);
                    break;
                default:
                    DrawMeasure(context, pen, marker, marker.Cross, -TickLength, TickLength);
                    break;
            }
        }
    }

    // A line from the start to the end of the marker at the position across, with a tick at each end that reaches from the offset before to the
    // offset after (a bracket has ticks only on the side of the box) and the number.
    private void DrawMeasure(DrawingContext context, Pen pen, ScreenSmartMarker marker, double across, double tickBefore, double tickAfter, bool labelOutside = false)
    {
        Point Along(double along, double offset) => marker.IsHorizontal ? new Point(along, across + offset) : new Point(across + offset, along);

        context.DrawLine(pen, Along(marker.From, 0), Along(marker.To, 0));
        context.DrawLine(pen, Along(marker.From, tickBefore), Along(marker.From, tickAfter));
        context.DrawLine(pen, Along(marker.To, tickBefore), Along(marker.To, tickAfter));
        if (marker.Text is { Length: > 0 } text && SmartGuideTextBrush is { } textBrush)
        {
            DrawLabel(context, text, textBrush, Along((marker.From + marker.To) / 2, 0), marker.IsHorizontal, labelOutside);
        }
    }

    // The number next to the middle of a line: above a horizontal line (below it for a bracket, so it does not cover the shape), to the right of a
    // vertical one, on a small rounded background.
    private void DrawLabel(DrawingContext context, string text, IBrush textBrush, Point middle, bool horizontalLine, bool outside)
    {
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            LabelFontSize,
            textBrush);
        var origin = horizontalLine
            ? new Point(middle.X - formatted.Width / 2, outside ? middle.Y + 4 : middle.Y - formatted.Height - 3)
            : new Point(middle.X + 6, middle.Y - formatted.Height / 2);
        if (SmartGuideLabelBackgroundBrush is { } background)
        {
            context.DrawRectangle(background, null, new Rect(origin.X - 3, origin.Y - 1, formatted.Width + 6, formatted.Height + 2), 3, 3);
        }

        context.DrawText(formatted, origin);
    }

    // The lines that show which object the drag is snapped to, over everything else.
    private void DrawGuides(DrawingContext context)
    {
        if (SnapGuides is not { Count: > 0 } guides || GuideBrush is not { } brush)
        {
            return;
        }

        var pen = new Pen(brush, OutlineThickness);
        foreach (var guide in guides)
        {
            if (!double.IsFinite(guide.Position) || !double.IsFinite(guide.From) || !double.IsFinite(guide.To))
            {
                continue;
            }

            var at = System.Math.Floor(guide.Position) + 0.5;
            context.DrawLine(
                pen,
                guide.IsVertical ? new Point(at, guide.From) : new Point(guide.From, at),
                guide.IsVertical ? new Point(at, guide.To) : new Point(guide.To, at));
        }
    }

    // The nodes of the selected shapes as squares (the selected ones filled with the selection color) and the shown handles as lines ending
    // in dots. Handles are drawn first so a node is never hidden by one. Sizes follow the handle size, so touch layouts get larger ones.
    private void DrawNodeOverlay(DrawingContext context, ScreenNodeOverlay overlay, IBrush brush)
    {
        var pen = new Pen(brush, OutlineThickness);
        var size = double.IsFinite(HandleSize) && HandleSize > 0 ? HandleSize : SelectionOverlay.HandleSize;
        var dot = size * 0.375;

        foreach (var node in overlay.Nodes)
        {
            var anchor = new Point(node.X, node.Y);
            if (node.HasIn)
            {
                var end = new Point(node.InX!.Value, node.InY!.Value);
                context.DrawLine(pen, anchor, end);
                context.DrawEllipse(SelectionHandleFillBrush, pen, end, dot, dot);
            }

            if (node.HasOut)
            {
                var end = new Point(node.OutX!.Value, node.OutY!.Value);
                context.DrawLine(pen, anchor, end);
                context.DrawEllipse(SelectionHandleFillBrush, pen, end, dot, dot);
            }
        }

        foreach (var node in overlay.Nodes)
        {
            var half = size / 2;
            var area = new Rect(node.X - half, node.Y - half, size, size);
            context.DrawRectangle(node.IsSelected ? brush : SelectionHandleFillBrush, pen, area);
        }
    }

    // The nodes of an unfinished path as squares and their handles as lines ending in dots, in the colors of the selection. The
    // first node is filled solid while a click would close the path. Sizes follow the handle size, so touch layouts get larger ones.
    private void DrawPenOverlay(DrawingContext context, ScreenPenOverlay overlay, IBrush brush)
    {
        var pen = new Pen(brush, OutlineThickness);
        var size = double.IsFinite(HandleSize) && HandleSize > 0 ? HandleSize : SelectionOverlay.HandleSize;
        var dot = size * 0.375;

        foreach (var node in overlay.Nodes)
        {
            var anchor = new Point(node.X, node.Y);
            if (node.HasIn)
            {
                var end = new Point(node.InX!.Value, node.InY!.Value);
                context.DrawLine(pen, anchor, end);
                context.DrawEllipse(SelectionHandleFillBrush, pen, end, dot, dot);
            }

            if (node.HasOut)
            {
                var end = new Point(node.OutX!.Value, node.OutY!.Value);
                context.DrawLine(pen, anchor, end);
                context.DrawEllipse(SelectionHandleFillBrush, pen, end, dot, dot);
            }
        }

        for (var i = 0; i < overlay.Nodes.Count; i++)
        {
            var node = overlay.Nodes[i];
            var highlighted = i == 0 && overlay.HighlightFirst;
            var half = highlighted ? size * 0.75 : size / 2;
            var area = new Rect(node.X - half, node.Y - half, half * 2, half * 2);
            context.DrawRectangle(highlighted ? brush : SelectionHandleFillBrush, pen, area);
        }
    }

    // Lines keep the same width at every zoom and handles the same size: the overlay is already in screen pixels.
    private void DrawOverlay(DrawingContext context, SelectionOverlay overlay, IBrush brush)
    {
        var pen = new Pen(brush, OutlineThickness);

        if (overlay.EnteredGroup is { } entered)
        {
            // Dashed, so it cannot be mistaken for the outline of a selected shape.
            var dashed = new Pen(brush, OutlineThickness, new DashStyle([4, 3], 0));
            context.DrawGeometry(null, dashed, ToGeometry(entered));
        }

        foreach (var outline in overlay.Outlines)
        {
            context.DrawGeometry(null, pen, ToGeometry(outline));
        }

        if (overlay.Box is { } box)
        {
            context.DrawGeometry(null, pen, ToGeometry(box));
        }

        if (overlay.RotateLink is { } link)
        {
            context.DrawLine(pen, new Point(link.From.X, link.From.Y), new Point(link.To.X, link.To.Y));
        }

        var size = double.IsFinite(HandleSize) && HandleSize > 0 ? HandleSize : SelectionOverlay.HandleSize;
        foreach (var handle in overlay.Handles)
        {
            var area = new Rect(handle.Center.X - size / 2, handle.Center.Y - size / 2, size, size);
            if (handle.Type == HandleType.Rotate)
            {
                context.DrawEllipse(SelectionHandleFillBrush, pen, area.Center, size / 2, size / 2);
            }
            else
            {
                context.DrawRectangle(SelectionHandleFillBrush, pen, area);
            }
        }

        // A circle with a dot, so a radius handle is not taken for the plain circle of the rotate handle or the squares of the resize handles.
        foreach (var handle in overlay.RadiusHandles)
        {
            var center = new Point(handle.Center.X, handle.Center.Y);
            context.DrawEllipse(SelectionHandleFillBrush, pen, center, size / 2, size / 2);
            context.DrawEllipse(brush, null, center, size * 0.2, size * 0.2);
        }

        if (overlay.SelectionBox is { } selectionBox)
        {
            var area = new Rect(selectionBox.X, selectionBox.Y, selectionBox.Width, selectionBox.Height);
            using (context.PushOpacity(SelectionBoxFillOpacity))
            {
                context.DrawRectangle(brush, null, area);
            }

            context.DrawRectangle(null, pen, area);
        }
    }

    private static StreamGeometry ToGeometry(ScreenPolygon polygon)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(new Point(polygon.Points[0].X, polygon.Points[0].Y), false);
            for (var i = 1; i < polygon.Points.Count; i++)
            {
                context.LineTo(new Point(polygon.Points[i].X, polygon.Points[i].Y));
            }

            context.EndFigure(true);
        }

        return geometry;
    }

    private static void DrawWithOpacity(DrawingContext context, ScreenShape shape)
    {
        if (shape.RotationDegrees != 0)
        {
            // Turn around the center of the box. The shape itself is drawn upright, so degenerate shapes turn the same way.
            var centerX = shape.X + shape.Width / 2;
            var centerY = shape.Y + shape.Height / 2;
            var turn = Matrix.CreateTranslation(-centerX, -centerY)
                * Matrix.CreateRotation(shape.RotationDegrees * System.Math.PI / 180)
                * Matrix.CreateTranslation(centerX, centerY);
            using (context.PushTransform(turn))
            {
                DrawWithOpacity(context, shape with { RotationDegrees = 0 });
            }

            return;
        }

        if (shape.Opacity < 1)
        {
            // One opacity for fill and stroke together, like SVG opacity.
            using (context.PushOpacity(shape.Opacity))
            {
                DrawShape(context, shape);
            }
        }
        else
        {
            DrawShape(context, shape);
        }
    }

    private static void DrawShape(DrawingContext context, ScreenShape shape)
    {
        if (shape.Type == ScreenShapeType.Path)
        {
            DrawPath(context, shape);
            return;
        }

        if (shape.Width == 0 || shape.Height == 0)
        {
            DrawDegenerateShape(context, shape);
            return;
        }

        IBrush? fill = shape.Fill is { } fillColor ? new ImmutableSolidColorBrush(ToAvalonia(fillColor)) : null;
        IPen? pen = shape.Stroke is { } strokeColor
            ? new ImmutablePen(new ImmutableSolidColorBrush(ToAvalonia(strokeColor)), shape.StrokeWidth)
            : null;
        var bounds = new Rect(shape.X, shape.Y, shape.Width, shape.Height);

        if (shape.Type == ScreenShapeType.Ellipse)
        {
            context.DrawEllipse(fill, pen, bounds.Center, bounds.Width / 2, bounds.Height / 2);
        }
        else if (shape.Corners is { } corners)
        {
            context.DrawRectangle(fill, pen, new RoundedRect(bounds, corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft));
        }
        else
        {
            context.DrawRectangle(fill, pen, bounds);
        }
    }

    // The fill and the stroke are two geometries. An open path is filled as if it were closed, as SVG does, but its stroke does not
    // include the closing line. A path without area (a straight line) still gets its stroke.
    private static void DrawPath(DrawingContext context, ScreenShape shape)
    {
        if (shape.Path is not { } outline)
        {
            return;
        }

        if (shape.Fill is { } fillColor)
        {
            context.DrawGeometry(new ImmutableSolidColorBrush(ToAvalonia(fillColor)), null, ToGeometry(outline, shape, forFill: true));
        }

        if (shape.Stroke is { } strokeColor)
        {
            var pen = new ImmutablePen(new ImmutableSolidColorBrush(ToAvalonia(strokeColor)), shape.StrokeWidth);
            context.DrawGeometry(null, pen, ToGeometry(outline, shape, forFill: false));
        }
    }

    // One figure per sub-path. For the fill every figure is closed, for the stroke only the closed sub-paths are.
    private static StreamGeometry ToGeometry(ScreenPath outline, ScreenShape shape, bool forFill)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.SetFillRule(outline.FillRule == CoreFillRule.EvenOdd ? FillRule.EvenOdd : FillRule.NonZero);
            var open = false;
            var closedByPath = false;
            foreach (var command in PathOutline.Commands(outline, shape.X, shape.Y))
            {
                switch (command.Type)
                {
                    case PathCommandType.Move:
                        if (open)
                        {
                            context.EndFigure(forFill || closedByPath);
                        }

                        context.BeginFigure(new Point(command.X, command.Y), forFill);
                        open = true;
                        closedByPath = false;
                        break;
                    case PathCommandType.Line:
                        context.LineTo(new Point(command.X, command.Y));
                        break;
                    case PathCommandType.Cubic:
                        context.CubicBezierTo(new Point(command.Control1X, command.Control1Y), new Point(command.Control2X, command.Control2Y), new Point(command.X, command.Y));
                        break;
                    case PathCommandType.Close:
                        closedByPath = true;
                        break;
                }
            }

            if (open)
            {
                context.EndFigure(forFill || closedByPath);
            }
        }

        return geometry;
    }

    // Skia draws nothing for a rectangle or ellipse without area, even with a stroke. A shape without area has no fill,
    // so only its stroke is shown: a dot for a point, a line for a segment.
    private static void DrawDegenerateShape(DrawingContext context, ScreenShape shape)
    {
        if (shape.Stroke is not { } strokeColor)
        {
            return;
        }

        var brush = new ImmutableSolidColorBrush(ToAvalonia(strokeColor));
        if (shape.Width == 0 && shape.Height == 0)
        {
            context.DrawEllipse(brush, null, new Point(shape.X, shape.Y), shape.StrokeWidth / 2, shape.StrokeWidth / 2);
            return;
        }

        context.DrawLine(
            new ImmutablePen(brush, shape.StrokeWidth),
            new Point(shape.X, shape.Y),
            new Point(shape.X + shape.Width, shape.Y + shape.Height));
    }

    private static Color ToAvalonia(CoreColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);
}
