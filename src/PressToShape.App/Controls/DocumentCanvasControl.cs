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
            HiddenShapeIdProperty,
            SelectionBrushProperty,
            SelectionHandleFillBrushProperty,
            HandleSizeProperty,
            EnteredGroupIdProperty,
            DimOpacityProperty);
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

    public override void Render(DrawingContext context)
    {
        if (Document is not { } document)
        {
            return;
        }

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
            context.DrawGeometry(new ImmutableSolidColorBrush(ToAvalonia(fillColor)), null, ToGeometry(outline, shape, closeFigure: true));
        }

        if (shape.Stroke is { } strokeColor)
        {
            var pen = new ImmutablePen(new ImmutableSolidColorBrush(ToAvalonia(strokeColor)), shape.StrokeWidth);
            context.DrawGeometry(null, pen, ToGeometry(outline, shape, closeFigure: outline.IsClosed));
        }
    }

    private static StreamGeometry ToGeometry(ScreenPath outline, ScreenShape shape, bool closeFigure)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.SetFillRule(outline.FillRule == CoreFillRule.EvenOdd ? FillRule.EvenOdd : FillRule.NonZero);
            var open = false;
            foreach (var command in PathOutline.Commands(outline, shape.X, shape.Y))
            {
                switch (command.Type)
                {
                    case PathCommandType.Move:
                        context.BeginFigure(new Point(command.X, command.Y), closeFigure);
                        open = true;
                        break;
                    case PathCommandType.Line:
                        context.LineTo(new Point(command.X, command.Y));
                        break;
                    case PathCommandType.Cubic:
                        context.CubicBezierTo(new Point(command.Control1X, command.Control1Y), new Point(command.Control2X, command.Control2Y), new Point(command.X, command.Y));
                        break;
                    case PathCommandType.Close:
                        break;
                }
            }

            if (open)
            {
                context.EndFigure(closeFigure);
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
