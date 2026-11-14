namespace PressToShape.Core.Shapes;

/// <summary>An axis-aligned ellipse that fills its bounds.</summary>
public sealed record EllipseShape : Shape
{
    /// <inheritdoc />
    public override bool Contains(double x, double y)
    {
        var radiusX = Width / 2;
        var radiusY = Height / 2;
        var dx = x - (X + radiusX);
        var dy = y - (Y + radiusY);

        return Squared(dx, radiusX) + Squared(dy, radiusY) <= 1;
    }

    // A zero radius collapses the ellipse to a line or a point on that axis: only a zero offset stays inside.
    private static double Squared(double offset, double radius)
    {
        if (radius == 0)
        {
            return offset == 0 ? 0 : double.PositiveInfinity;
        }

        var ratio = offset / radius;
        return ratio * ratio;
    }
}
