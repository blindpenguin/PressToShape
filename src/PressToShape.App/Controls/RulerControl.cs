using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PressToShape.App.Workspace;

namespace PressToShape.App.Controls;

/// <summary>
/// A horizontal or vertical ruler. It asks <see cref="RulerTicks"/> for the ticks of the current zoom and origin
/// and draws them with the theme brushes. It does not know about documents or the canvas.
/// </summary>
internal sealed class RulerControl : Control
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<RulerControl, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>Zoom in percent. 100 means one document pixel per screen pixel.</summary>
    public static readonly StyledProperty<double> ZoomPercentProperty =
        AvaloniaProperty.Register<RulerControl, double>(nameof(ZoomPercent), 100);

    /// <summary>Position of document zero along the ruler, in screen pixels.</summary>
    public static readonly StyledProperty<double> OriginPxProperty =
        AvaloniaProperty.Register<RulerControl, double>(nameof(OriginPx));

    private const double LabelFontSize = 9;
    private const double MajorTickLength = 12;
    private const double MediumTickLength = 8;
    private const double MinorTickLength = 4;

    static RulerControl()
    {
        AffectsRender<RulerControl>(OrientationProperty, ZoomPercentProperty, OriginPxProperty);
    }

    public RulerControl()
    {
        // Brushes come from the theme dictionaries, so redraw when the theme variant changes.
        ActualThemeVariantChanged += (_, _) => InvalidateVisual();
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double ZoomPercent
    {
        get => GetValue(ZoomPercentProperty);
        set => SetValue(ZoomPercentProperty, value);
    }

    public double OriginPx
    {
        get => GetValue(OriginPxProperty);
        set => SetValue(OriginPxProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var background = Brush("RulerBackgroundBrush");
        var tickBrush = Brush("RulerTickBrush");
        var textBrush = Brush("RulerTextBrush");
        var borderBrush = Brush("BorderSubtleBrush");

        var bounds = new Rect(Bounds.Size);
        context.DrawRectangle(background, null, bounds);

        var horizontal = Orientation == Orientation.Horizontal;
        var length = horizontal ? bounds.Width : bounds.Height;
        var thickness = horizontal ? bounds.Height : bounds.Width;
        if (length <= 0 || thickness <= 0)
        {
            return;
        }

        var tickPen = new Pen(tickBrush, 1);
        var borderPen = new Pen(borderBrush, 1);

        // Border on the side facing the canvas.
        if (horizontal)
        {
            context.DrawLine(borderPen, new Point(0, bounds.Height - 0.5), new Point(bounds.Width, bounds.Height - 0.5));
        }
        else
        {
            context.DrawLine(borderPen, new Point(bounds.Width - 0.5, 0), new Point(bounds.Width - 0.5, bounds.Height));
        }

        foreach (var tick in RulerTicks.Compute(ZoomPercent, OriginPx, length))
        {
            var position = System.Math.Floor(tick.ScreenPosition) + 0.5;
            var tickLength = tick.Type switch
            {
                TickType.Major => MajorTickLength,
                TickType.Medium => MediumTickLength,
                _ => MinorTickLength,
            };
            tickLength = System.Math.Min(tickLength, thickness);

            if (horizontal)
            {
                context.DrawLine(tickPen, new Point(position, thickness - tickLength), new Point(position, thickness));
            }
            else
            {
                context.DrawLine(tickPen, new Point(thickness - tickLength, position), new Point(thickness, position));
            }

            if (tick.Label is { } label)
            {
                DrawLabel(context, label, textBrush, position, horizontal);
            }
        }
    }

    private static void DrawLabel(DrawingContext context, string label, IBrush brush, double position, bool horizontal)
    {
        var text = new FormattedText(
            label,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            LabelFontSize,
            brush);

        if (horizontal)
        {
            context.DrawText(text, new Point(position + 3, 1));
        }
        else
        {
            // Rotate the label by -90 degrees so it reads from bottom to top beside its tick.
            using (context.PushTransform(Matrix.CreateRotation(-System.Math.PI / 2) * Matrix.CreateTranslation(1, position - 3)))
            {
                context.DrawText(text, new Point(0, 0));
            }
        }
    }

    private IBrush Brush(string key) =>
        this.TryFindResource(key, ActualThemeVariant, out var value) && value is IBrush brush
            ? brush
            : Brushes.Gray;
}
