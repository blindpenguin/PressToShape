using System.Collections.Generic;

namespace PressToShape.Core.Shapes;

/// <summary>
/// One corner of a rounded rectangle: its radius, the corner point of the box, and where the arc starts and ends when the outline is
/// followed clockwise. For a radius of 0 the arc is empty and <see cref="Start"/> and <see cref="End"/> are the corner point.
/// </summary>
/// <param name="Radius">The radius used, at least 0.</param>
/// <param name="Point">The corner point of the box.</param>
/// <param name="Start">Where the arc starts, coming along the previous edge.</param>
/// <param name="End">Where the arc ends, going on along the next edge.</param>
internal readonly record struct RoundedCorner(double Radius, (double X, double Y) Point, (double X, double Y) Start, (double X, double Y) End);

/// <summary>
/// The outline of a rectangle with rounded corners, in the frame before any rotation. The conversion to a path and the SVG export both
/// start from these points, so the two outlines are the same shape.
/// </summary>
internal static class RoundedRectangleGeometry
{
    /// <summary>The four corners clockwise from the top left. Pass the effective radii (see <see cref="CornerRadii.Limited"/>).</summary>
    public static IReadOnlyList<RoundedCorner> Corners(double x, double y, double width, double height, CornerRadii effective)
    {
        var right = x + width;
        var bottom = y + height;
        return
        [
            new RoundedCorner(effective.TopLeft, (x, y), (x, y + effective.TopLeft), (x + effective.TopLeft, y)),
            new RoundedCorner(effective.TopRight, (right, y), (right - effective.TopRight, y), (right, y + effective.TopRight)),
            new RoundedCorner(effective.BottomRight, (right, bottom), (right, bottom - effective.BottomRight), (right - effective.BottomRight, bottom)),
            new RoundedCorner(effective.BottomLeft, (x, bottom), (x + effective.BottomLeft, bottom), (x, bottom - effective.BottomLeft)),
        ];
    }
}
