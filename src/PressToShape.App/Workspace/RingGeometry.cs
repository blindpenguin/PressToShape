using System;

namespace PressToShape.App.Workspace;

/// <summary>
/// The wedge math of the ring menu, on plain numbers in screen pixels. The ring is cut into <c>count</c> equal wedges. Wedge 0 is centered at
/// the top and the others follow clockwise. Angles are in degrees, measured clockwise from the top. Points are given relative to the center
/// of the ring, with y pointing down. Non-finite input never produces a hit.
/// </summary>
internal static class RingGeometry
{
    private const double FullTurn = 360;

    /// <summary>The start angle and the size of a wedge. The start may be negative (wedge 0 starts left of the top).</summary>
    public static (double Start, double Sweep) WedgeAngles(int index, int count)
    {
        if (count <= 0)
        {
            return (0, 0);
        }

        var sweep = FullTurn / count;
        return (index * sweep - sweep / 2, sweep);
    }

    /// <summary>
    /// What the point hits. Closer than <paramref name="innerRadius"/> is the center, farther than <paramref name="outerRadius"/> is outside, and
    /// both radii themselves belong to the wedges. A point exactly on the line between two wedges belongs to the next one clockwise.
    /// </summary>
    public static RingHit HitTest(double dx, double dy, int count, double innerRadius, double outerRadius)
    {
        if (!double.IsFinite(dx) || !double.IsFinite(dy))
        {
            return RingHit.Outside;
        }

        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance < innerRadius)
        {
            return RingHit.Center;
        }

        if (distance > outerRadius || count <= 0)
        {
            return RingHit.Outside;
        }

        var sweep = FullTurn / count;
        var angle = Math.Atan2(dx, -dy) * 180 / Math.PI;
        var shifted = (angle + sweep / 2 + FullTurn) % FullTurn;
        return RingHit.Slot(Math.Min((int)(shifted / sweep), count - 1));
    }

    /// <summary>The middle of a wedge, between the two radii, as an offset from the center of the ring. This is where the icon goes.</summary>
    public static (double X, double Y) SlotCenter(int index, int count, double innerRadius, double outerRadius)
    {
        if (count <= 0)
        {
            return (0, 0);
        }

        var angle = index * FullTurn / count * Math.PI / 180;
        var radius = (innerRadius + outerRadius) / 2;
        return (radius * Math.Sin(angle), -radius * Math.Cos(angle));
    }

    /// <summary>
    /// Moves the center so the whole ring stays inside an area of the given size. An area smaller than the ring gets the ring
    /// centered in it, so no side is favored.
    /// </summary>
    public static (double X, double Y) ClampCenter(double x, double y, double outerRadius, double width, double height) =>
        (ClampAxis(x, outerRadius, width), ClampAxis(y, outerRadius, height));

    private static double ClampAxis(double value, double radius, double size)
    {
        if (!double.IsFinite(value) || size < 2 * radius)
        {
            return size / 2;
        }

        return Math.Clamp(value, radius, size - radius);
    }
}
