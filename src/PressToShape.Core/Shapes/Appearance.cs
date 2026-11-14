namespace PressToShape.Core.Shapes;

/// <summary>How a shape looks: solid fill, solid stroke and opacity. Values are validated on creation and on <c>with</c>.</summary>
public sealed record Appearance
{
    private readonly double _strokeWidth = 1;
    private readonly double _opacity = 1;

    /// <summary>The appearance of a new shape: a blue fill, a dark gray stroke of width 1 and full opacity.</summary>
    public static Appearance Default { get; } = new()
    {
        Fill = Color.FromRgb(0x4A, 0x90, 0xD9),
        Stroke = Color.FromRgb(0x1F, 0x29, 0x33),
    };

    /// <summary>Fill color, or <c>null</c> for no fill.</summary>
    public Color? Fill { get; init; }

    /// <summary>Stroke color, or <c>null</c> for no stroke.</summary>
    public Color? Stroke { get; init; }

    /// <summary>Stroke width in document units. Finite and not negative. Zero is allowed.</summary>
    public double StrokeWidth
    {
        get => _strokeWidth;
        init => _strokeWidth = Guard.FiniteNonNegative(value, nameof(StrokeWidth));
    }

    /// <summary>Opacity from 0 (invisible) to 1 (opaque).</summary>
    public double Opacity
    {
        get => _opacity;
        init => _opacity = Guard.FiniteInRange(value, 0, 1, nameof(Opacity));
    }
}
