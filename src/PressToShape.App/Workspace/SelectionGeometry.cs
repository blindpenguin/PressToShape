using System;
using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>Where the selection frame and its handles are, and which handle a point hits. Pure functions, document units.</summary>
internal static class SelectionGeometry
{
    /// <summary>How far the rotate handle is from the top edge, in screen pixels.</summary>
    public const double RotateHandleDistance = 24;

    /// <summary>The size of the area that grabs a handle with a mouse or a pen, in screen pixels.</summary>
    public const double GrabSize = 10;

    /// <summary>The size of the area that grabs a handle with a finger, in screen pixels. The tablet layout can set its own through the theme.</summary>
    public const double TouchGrabSize = 44;

    // Hit-test order when two handles are equally close: rotate, corners, then edges.
    private static readonly HandleType[] Priority =
    [
        HandleType.Rotate,
        HandleType.TopLeft, HandleType.TopRight, HandleType.BottomRight, HandleType.BottomLeft,
        HandleType.Top, HandleType.Right, HandleType.Bottom, HandleType.Left,
    ];

    /// <summary>The frame of the shapes, or <c>null</c> for none.</summary>
    public static SelectionFrame? FrameOf(IReadOnlyList<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        if (shapes.Count == 0)
        {
            return null;
        }

        // A group is framed like several shapes: upright, around its leaves, and scaled uniformly if one of them is turned.
        if (shapes.Count == 1 && shapes[0] is not GroupShape)
        {
            var shape = shapes[0];
            return new SelectionFrame(shape.CenterX, shape.CenterY, shape.Width, shape.Height, shape.Rotation, true);
        }

        var left = double.MaxValue;
        var top = double.MaxValue;
        var right = double.MinValue;
        var bottom = double.MinValue;
        foreach (var shape in shapes)
        {
            var bounds = shape.RotatedBounds;
            left = Math.Min(left, bounds.X);
            top = Math.Min(top, bounds.Y);
            right = Math.Max(right, bounds.X + bounds.Width);
            bottom = Math.Max(bottom, bounds.Y + bounds.Height);
        }

        return new SelectionFrame((left + right) / 2, (top + bottom) / 2, right - left, bottom - top, 0, false);
    }

    /// <summary>The direction of a handle from the frame center in frame units: (-1, -1) is the top left corner.</summary>
    public static (int X, int Y) Direction(HandleType handle) => handle switch
    {
        HandleType.TopLeft => (-1, -1),
        HandleType.Top or HandleType.Rotate => (0, -1),
        HandleType.TopRight => (1, -1),
        HandleType.Right => (1, 0),
        HandleType.BottomRight => (1, 1),
        HandleType.Bottom => (0, 1),
        HandleType.BottomLeft => (-1, 1),
        HandleType.Left => (-1, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(handle), handle, null),
    };

    /// <summary>The center of the handle in document units. The handles turn with the frame.</summary>
    public static (double X, double Y) HandlePosition(SelectionFrame frame, HandleType handle, double zoomPercent)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var (dx, dy) = Direction(handle);
        var localX = dx * frame.Width / 2;
        var localY = dy * frame.Height / 2;
        if (handle == HandleType.Rotate)
        {
            localY -= RotateHandleDistance / Scale(zoomPercent);
        }

        return ToDocument(frame, localX, localY);
    }

    /// <summary>
    /// The handle whose grab area contains the document point, or <c>null</c>. The closest handle wins, then the priority order.
    /// A grab area larger than <see cref="GrabSize"/> (for a finger) is limited to a quarter of the shorter side of the frame, so that the
    /// handles of a small shape do not cover its middle and the shape can still be moved by it.
    /// </summary>
    /// <param name="grabSize">The size of the grab area in screen pixels.</param>
    public static HandleType? HitHandle(SelectionFrame frame, double x, double y, double zoomPercent, double grabSize = GrabSize)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var scale = Scale(zoomPercent);
        var halfOnScreen = (double.IsFinite(grabSize) ? Math.Max(0, grabSize) : GrabSize) / 2;
        if (halfOnScreen > GrabSize / 2)
        {
            halfOnScreen = Math.Max(GrabSize / 2, Math.Min(halfOnScreen, Math.Min(frame.Width, frame.Height) * scale / 4));
        }

        var half = halfOnScreen / scale;
        HandleType? best = null;
        var bestDistance = double.MaxValue;
        foreach (var handle in Priority)
        {
            var (hx, hy) = HandlePosition(frame, handle, zoomPercent);
            var (dx, dy) = Turn(x - hx, y - hy, -frame.Rotation);
            if (Math.Abs(dx) > half || Math.Abs(dy) > half)
            {
                continue;
            }

            var distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < bestDistance - 1e-9)
            {
                best = handle;
                bestDistance = distance;
            }
        }

        return best;
    }

    /// <summary>The direction in which a handle points on the screen, in degrees [0, 360): 0 is right, 90 is down. For choosing a cursor.</summary>
    public static double HandleDirectionDegrees(HandleType handle, double rotation)
    {
        var (dx, dy) = Direction(handle);
        var degrees = Math.Atan2(dy, dx) * 180 / Math.PI + rotation;
        degrees %= 360;
        return degrees < 0 ? degrees + 360 : degrees;
    }

    private static double Scale(double zoomPercent) => zoomPercent / 100;

    private static (double X, double Y) ToDocument(SelectionFrame frame, double localX, double localY)
    {
        var (x, y) = Turn(localX, localY, frame.Rotation);
        return (frame.CenterX + x, frame.CenterY + y);
    }

    private static (double X, double Y) Turn(double x, double y, double degrees)
    {
        if (degrees == 0)
        {
            return (x, y);
        }

        var radians = degrees * Math.PI / 180;
        var (sin, cos) = Math.SinCos(radians);
        return (x * cos - y * sin, x * sin + y * cos);
    }
}
