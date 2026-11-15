using System;
using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>Where the selection frame and its handles are, and which handle a point hits. Pure functions, document units.</summary>
internal static class SelectionGeometry
{
    /// <summary>How far the rotate handle is from the top edge, in screen pixels.</summary>
    public const double RotateHandleDistance = 24;

    /// <summary>The size of the area that grabs a handle, in screen pixels.</summary>
    public const double GrabSize = 10;

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

        if (shapes.Count == 1)
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

    /// <summary>The handle whose grab area contains the document point, or <c>null</c>. The closest handle wins, then the priority order.</summary>
    public static HandleType? HitHandle(SelectionFrame frame, double x, double y, double zoomPercent)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var half = GrabSize / 2 / Scale(zoomPercent);
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
