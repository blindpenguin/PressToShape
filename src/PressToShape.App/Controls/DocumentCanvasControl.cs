using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using CoreColor = PressToShape.Core.Shapes.Color;

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

    /// <summary>The brush of the selection outlines, handle outlines and the selection box. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(SelectionBrush));

    /// <summary>The fill of the handles. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> SelectionHandleFillBrushProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, IBrush?>(nameof(SelectionHandleFillBrush));

    /// <summary>The side of a selection handle in screen pixels. It comes from the theme, so a layout for touch can draw larger handles.</summary>
    public static readonly StyledProperty<double> HandleSizeProperty =
        AvaloniaProperty.Register<DocumentCanvasControl, double>(nameof(HandleSize), defaultValue: SelectionOverlay.HandleSize);

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
            SelectionBrushProperty,
            SelectionHandleFillBrushProperty,
            HandleSizeProperty);
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

        foreach (var shape in CanvasScene.Build(document, ZoomPercent, OriginX, OriginY))
        {
            DrawWithOpacity(context, shape);
        }

        if (Draft is { } draft)
        {
            DrawWithOpacity(context, draft);
        }

        if (Overlay is { } overlay && SelectionBrush is { } brush)
        {
            DrawOverlay(context, overlay, brush);
        }
    }

    // Lines keep the same width at every zoom and handles the same size: the overlay is already in screen pixels.
    private void DrawOverlay(DrawingContext context, SelectionOverlay overlay, IBrush brush)
    {
        var pen = new Pen(brush, OutlineThickness);

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
