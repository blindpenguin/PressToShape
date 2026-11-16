using System;

namespace PressToShape.Core.Shapes;

/// <summary>
/// The radius of each corner of a rectangle, in document units. The struct only holds the values: <see cref="RectangleShape"/> checks
/// that they are finite and not negative. <see cref="Limited"/> gives the radii that are really used for a box of a given size.
/// </summary>
/// <param name="TopLeft">Radius of the top left corner.</param>
/// <param name="TopRight">Radius of the top right corner.</param>
/// <param name="BottomRight">Radius of the bottom right corner.</param>
/// <param name="BottomLeft">Radius of the bottom left corner.</param>
public readonly record struct CornerRadii(double TopLeft, double TopRight, double BottomRight, double BottomLeft)
{
    /// <summary>All four radii are 0: a rectangle with square corners.</summary>
    public static CornerRadii Zero => default;

    /// <summary>All four corners have the same radius.</summary>
    public static CornerRadii Uniform(double radius) => new(radius, radius, radius, radius);

    /// <summary>Whether all four radii are 0.</summary>
    public bool IsZero => TopLeft == 0 && TopRight == 0 && BottomRight == 0 && BottomLeft == 0;

    /// <summary>Whether all four radii are equal.</summary>
    public bool IsUniform => TopLeft == TopRight && TopRight == BottomRight && BottomRight == BottomLeft;

    /// <summary>The radius of one corner.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corner is not a defined value.</exception>
    public double this[Corner corner] => corner switch
    {
        Corner.TopLeft => TopLeft,
        Corner.TopRight => TopRight,
        Corner.BottomRight => BottomRight,
        Corner.BottomLeft => BottomLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
    };

    /// <summary>A copy with the radius of one corner replaced.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corner is not a defined value.</exception>
    public CornerRadii With(Corner corner, double radius) => corner switch
    {
        Corner.TopLeft => this with { TopLeft = radius },
        Corner.TopRight => this with { TopRight = radius },
        Corner.BottomRight => this with { BottomRight = radius },
        Corner.BottomLeft => this with { BottomLeft = radius },
        _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
    };

    /// <summary>
    /// Each radius limited to at least 0 and at most half of the shorter side of a box of this size. A radius that is not a number
    /// becomes 0.
    /// </summary>
    public CornerRadii Limited(double width, double height)
    {
        var max = Math.Max(0, Math.Min(width, height) / 2);
        return new CornerRadii(Clamp(TopLeft, max), Clamp(TopRight, max), Clamp(BottomRight, max), Clamp(BottomLeft, max));
    }

    private static double Clamp(double value, double max) => double.IsNaN(value) ? 0 : Math.Clamp(value, 0, max);
}
