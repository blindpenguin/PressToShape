namespace PressToShape.Core.Shapes;

/// <summary>An axis-aligned rectangle, with square or rounded corners.</summary>
public sealed record RectangleShape : Shape
{
    private readonly CornerRadii _radii;

    /// <summary>
    /// The corner radii as they were set: each one finite and not negative, 0 for square corners (the default). They are not trimmed when
    /// the rectangle is resized, so a corner that was limited gets its radius back when the rectangle grows. Drawing, hit tests, export and
    /// the editing fields use <see cref="EffectiveRadii"/>.
    /// </summary>
    public CornerRadii Radii
    {
        get => _radii;
        init
        {
            Guard.FiniteNonNegative(value.TopLeft, nameof(Radii));
            Guard.FiniteNonNegative(value.TopRight, nameof(Radii));
            Guard.FiniteNonNegative(value.BottomRight, nameof(Radii));
            Guard.FiniteNonNegative(value.BottomLeft, nameof(Radii));
            _radii = value;
        }
    }

    /// <summary>The radii that are used: each stored radius limited to half of the shorter side of the box.</summary>
    public CornerRadii EffectiveRadii => _radii.Limited(Width, Height);

    /// <inheritdoc />
    protected override bool ContainsLocal(double x, double y)
    {
        if (x < X || x > X + Width || y < Y || y > Y + Height)
        {
            return false;
        }

        var radii = EffectiveRadii;
        return InCorner(x, y, X, Y, radii.TopLeft, 1, 1)
            && InCorner(x, y, X + Width, Y, radii.TopRight, -1, 1)
            && InCorner(x, y, X + Width, Y + Height, radii.BottomRight, -1, -1)
            && InCorner(x, y, X, Y + Height, radii.BottomLeft, 1, -1);
    }

    // The corner point is (cornerX, cornerY). The signs point from the corner into the rectangle. A point in the square of side
    // radius at the corner is inside only if it is within the radius of the arc's center.
    private static bool InCorner(double x, double y, double cornerX, double cornerY, double radius, int signX, int signY)
    {
        if (radius <= 0)
        {
            return true;
        }

        var dx = (x - cornerX) * signX;
        var dy = (y - cornerY) * signY;
        if (dx >= radius || dy >= radius)
        {
            return true;
        }

        var offsetX = radius - dx;
        var offsetY = radius - dy;
        return offsetX * offsetX + offsetY * offsetY <= radius * radius;
    }
}
