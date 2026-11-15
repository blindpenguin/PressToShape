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

    static DocumentCanvasControl()
    {
        AffectsRender<DocumentCanvasControl>(DocumentProperty, ZoomPercentProperty, OriginXProperty, OriginYProperty);
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

    public override void Render(DrawingContext context)
    {
        if (Document is not { } document)
        {
            return;
        }

        foreach (var shape in CanvasScene.Build(document, ZoomPercent, OriginX, OriginY))
        {
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
