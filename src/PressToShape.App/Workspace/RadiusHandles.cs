using System;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// Where the four radius handles of a rectangle are and which radius a pointer position means. Pure functions, document units, like
/// <see cref="SelectionGeometry"/>. A handle sits inside the rectangle on the diagonal of its corner: it is the same distance (the inset)
/// from both sides of the corner. The inset runs from <see cref="MinInset"/> screen pixels at radius 0 to half of the shorter side at the
/// largest radius, in a straight line, so a handle never jumps when it is pressed and never lies on the corner resize handle.
/// </summary>
internal static class RadiusHandles
{
    /// <summary>The inset of a handle at radius 0, in screen pixels.</summary>
    public const double MinInset = 24;

    /// <summary>The handles are only shown when the shorter side of the rectangle is at least this long on the screen, in screen pixels.</summary>
    public const double MinSide = 64;

    /// <summary>The size of the area that grabs a handle with a mouse or a pen, in screen pixels.</summary>
    public const double GrabSize = 14;

    /// <summary>The size of the area that grabs a handle with a finger, in screen pixels. Smaller than the grab area of a resize handle, so the corner handle stays reachable.</summary>
    public const double TouchGrabSize = 36;

    // Hit-test order when two handles are equally close.
    private static readonly Corner[] Priority = [Corner.TopLeft, Corner.TopRight, Corner.BottomRight, Corner.BottomLeft];

    /// <summary>Whether the handles are shown for the rectangle at this zoom: its shorter side is at least <see cref="MinSide"/> on the screen.</summary>
    public static bool IsShown(RectangleShape rectangle, double zoomPercent)
    {
        ArgumentNullException.ThrowIfNull(rectangle);

        return Math.Min(rectangle.Width, rectangle.Height) * Scale(zoomPercent) >= MinSide;
    }

    /// <summary>The center of the handle of a corner in document units, with the rotation of the rectangle applied.</summary>
    public static (double X, double Y) Position(RectangleShape rectangle, Corner corner, double zoomPercent)
    {
        ArgumentNullException.ThrowIfNull(rectangle);

        var (signX, signY) = Direction(corner);
        var inset = InsetOf(rectangle, rectangle.EffectiveRadii[corner], zoomPercent);
        var (x, y) = Turn(signX * (rectangle.Width / 2 - inset), signY * (rectangle.Height / 2 - inset), rectangle.Rotation);
        return (rectangle.CenterX + x, rectangle.CenterY + y);
    }

    /// <summary>
    /// The corner whose handle grab area contains the document point, or <c>null</c>. The closest handle wins, then the order top left,
    /// top right, bottom right, bottom left.
    /// </summary>
    /// <param name="grabSize">The size of the grab area in screen pixels.</param>
    public static Corner? Hit(RectangleShape rectangle, double x, double y, double zoomPercent, double grabSize = GrabSize)
    {
        ArgumentNullException.ThrowIfNull(rectangle);

        var half = (double.IsFinite(grabSize) ? Math.Max(0, grabSize) : GrabSize) / 2 / Scale(zoomPercent);
        Corner? best = null;
        var bestDistance = double.MaxValue;
        foreach (var corner in Priority)
        {
            var (hx, hy) = Position(rectangle, corner, zoomPercent);
            var (dx, dy) = Turn(x - hx, y - hy, -rectangle.Rotation);
            if (Math.Abs(dx) > half || Math.Abs(dy) > half)
            {
                continue;
            }

            var distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < bestDistance - 1e-9)
            {
                best = corner;
                bestDistance = distance;
            }
        }

        return best;
    }

    /// <summary>
    /// How far the center of the handle is from the pointer along the diagonal, in document units of inset. The pointer may be a few
    /// pixels off the handle's center when it presses: pass this to <see cref="RadiusAt"/> so the handle does not jump.
    /// </summary>
    public static double GrabOffset(RectangleShape rectangle, Corner corner, double x, double y, double zoomPercent)
    {
        ArgumentNullException.ThrowIfNull(rectangle);

        return InsetOf(rectangle, rectangle.EffectiveRadii[corner], zoomPercent) - PointerInset(rectangle, corner, x, y);
    }

    /// <summary>
    /// The radius a pointer position means for a corner: from 0 to half of the shorter side. Drag with the same
    /// <paramref name="grabOffset"/> (from <see cref="GrabOffset"/> at the press) for the whole drag.
    /// </summary>
    public static double RadiusAt(RectangleShape rectangle, Corner corner, double x, double y, double zoomPercent, double grabOffset)
    {
        ArgumentNullException.ThrowIfNull(rectangle);

        var limit = Limit(rectangle);
        var minInset = MinInset / Scale(zoomPercent);
        if (limit <= minInset)
        {
            // The handles are not shown for such a small rectangle. A drag that still happens follows the plain inset.
            return Math.Clamp(PointerInset(rectangle, corner, x, y) + grabOffset, 0, limit);
        }

        var inset = PointerInset(rectangle, corner, x, y) + grabOffset;
        return Math.Clamp((inset - minInset) * limit / (limit - minInset), 0, limit);
    }

    // The inset of a handle for a radius, in document units: a straight line from MinInset (radius 0) to the limit (the largest radius).
    private static double InsetOf(RectangleShape rectangle, double radius, double zoomPercent)
    {
        var limit = Limit(rectangle);
        var minInset = MinInset / Scale(zoomPercent);
        if (limit <= minInset)
        {
            return Math.Min(minInset, limit);
        }

        return minInset + (limit - minInset) * Math.Clamp(radius, 0, limit) / limit;
    }

    // The mean distance of the point from the two sides of the corner, measured in the rectangle's own frame.
    private static double PointerInset(RectangleShape rectangle, Corner corner, double x, double y)
    {
        var (signX, signY) = Direction(corner);
        var (localX, localY) = Turn(x - rectangle.CenterX, y - rectangle.CenterY, -rectangle.Rotation);
        var insetX = rectangle.Width / 2 - signX * localX;
        var insetY = rectangle.Height / 2 - signY * localY;
        return (insetX + insetY) / 2;
    }

    private static double Limit(RectangleShape rectangle) => Math.Min(rectangle.Width, rectangle.Height) / 2;

    private static (int X, int Y) Direction(Corner corner) => corner switch
    {
        Corner.TopLeft => (-1, -1),
        Corner.TopRight => (1, -1),
        Corner.BottomRight => (1, 1),
        Corner.BottomLeft => (-1, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
    };

    private static double Scale(double zoomPercent) => zoomPercent / 100;

    private static (double X, double Y) Turn(double x, double y, double degrees)
    {
        if (degrees == 0)
        {
            return (x, y);
        }

        var (sin, cos) = Math.SinCos(degrees * Math.PI / 180);
        return (x * cos - y * sin, x * sin + y * cos);
    }
}
