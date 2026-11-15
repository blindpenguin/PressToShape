using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// The math of resizing, rotating and moving the selection and of the Properties fields. Pure functions: they return new
/// shapes in the same order, never change their input, and return the input list itself when the input makes no sense
/// (a number that is not finite, a handle that does not apply) or the result would not be finite.
/// Resizing does not change strokes. A flip past the opposite edge keeps the rotation of a shape as it is.
/// </summary>
internal static class SelectionTransforms
{
    /// <summary>The step of snapped rotation, in degrees.</summary>
    public const double SnapDegrees = 15;

    /// <summary>
    /// Resizes by dragging a handle to the pointer. A single shape scales in its turned frame, several shapes scale in the upright
    /// frame, each shape through the same scale. If a rotated shape is among several, the scale is uniform.
    /// </summary>
    /// <param name="startShapes">The shapes as they were when the drag began.</param>
    /// <param name="frame">The frame of <paramref name="startShapes"/>.</param>
    /// <param name="handle">The dragged handle. <see cref="HandleType.Rotate"/> does not resize.</param>
    /// <param name="pointerX">Pointer in document units.</param>
    /// <param name="pointerY">Pointer in document units.</param>
    /// <param name="keepProportions">Keeps the ratio of width to height (Shift).</param>
    /// <param name="fromCenter">Keeps the center fixed instead of the opposite edge or corner (Alt).</param>
    public static IReadOnlyList<Shape> Resize(
        IReadOnlyList<Shape> startShapes,
        SelectionFrame frame,
        HandleType handle,
        double pointerX,
        double pointerY,
        bool keepProportions,
        bool fromCenter)
    {
        ArgumentNullException.ThrowIfNull(startShapes);
        return Guarded(startShapes, () => ResizeCore(startShapes, frame, handle, pointerX, pointerY, keepProportions, fromCenter));
    }

    private static IReadOnlyList<Shape> ResizeCore(
        IReadOnlyList<Shape> startShapes,
        SelectionFrame frame,
        HandleType handle,
        double pointerX,
        double pointerY,
        bool keepProportions,
        bool fromCenter)
    {
        ArgumentNullException.ThrowIfNull(startShapes);
        ArgumentNullException.ThrowIfNull(frame);

        if (handle == HandleType.Rotate || !double.IsFinite(pointerX) || !double.IsFinite(pointerY))
        {
            return startShapes;
        }

        var (dx, dy) = SelectionGeometry.Direction(handle);
        var (qx, qy) = Turn(pointerX - frame.CenterX, pointerY - frame.CenterY, -frame.Rotation);
        double sx = 1, sy = 1, ax = 0, ay = 0;
        if (dx != 0)
        {
            ax = fromCenter ? 0 : -dx * frame.Width / 2;
            var span = dx * frame.Width / 2 - ax;
            if (span == 0)
            {
                return startShapes;
            }

            sx = (qx - ax) / span;
        }

        if (dy != 0)
        {
            ay = fromCenter ? 0 : -dy * frame.Height / 2;
            var span = dy * frame.Height / 2 - ay;
            if (span == 0)
            {
                return startShapes;
            }

            sy = (qy - ay) / span;
        }

        if (keepProportions || (!frame.IsSingle && startShapes.Any(s => s.Rotation != 0)))
        {
            if (dx != 0 && dy != 0)
            {
                var magnitude = Math.Max(Math.Abs(sx), Math.Abs(sy));
                sx = Sign(sx) * magnitude;
                sy = Sign(sy) * magnitude;
            }
            else if (dx != 0)
            {
                sy = Math.Abs(sx);
                ay = 0;
            }
            else
            {
                sx = Math.Abs(sy);
                ax = 0;
            }
        }

        return ScaleAbout(startShapes, frame, ax, ay, sx, sy);
    }

    /// <summary>
    /// Rotates by dragging the rotate handle. A single shape gets the angle of the pointer around its center; with
    /// <paramref name="snap"/> the resulting angle is a multiple of 15 degrees. Several shapes turn around the frame center by the
    /// angle the pointer moved (snapped in steps of 15 degrees), and each shape's own rotation changes by the same angle.
    /// </summary>
    public static IReadOnlyList<Shape> Rotate(
        IReadOnlyList<Shape> startShapes,
        SelectionFrame frame,
        double startX,
        double startY,
        double pointerX,
        double pointerY,
        bool snap)
    {
        ArgumentNullException.ThrowIfNull(startShapes);
        return Guarded(startShapes, () => RotateCore(startShapes, frame, startX, startY, pointerX, pointerY, snap));
    }

    private static IReadOnlyList<Shape> RotateCore(
        IReadOnlyList<Shape> startShapes,
        SelectionFrame frame,
        double startX,
        double startY,
        double pointerX,
        double pointerY,
        bool snap)
    {
        ArgumentNullException.ThrowIfNull(startShapes);
        ArgumentNullException.ThrowIfNull(frame);

        if (!AllFinite(startX, startY, pointerX, pointerY)
            || (pointerX == frame.CenterX && pointerY == frame.CenterY)
            || (startX == frame.CenterX && startY == frame.CenterY))
        {
            return startShapes;
        }

        var delta = AngleOf(pointerX - frame.CenterX, pointerY - frame.CenterY) - AngleOf(startX - frame.CenterX, startY - frame.CenterY);
        if (frame.IsSingle)
        {
            var angle = frame.Rotation + delta;
            if (snap)
            {
                angle = Math.Round(angle / SnapDegrees) * SnapDegrees;
            }

            return Result(startShapes, startShapes.Select(shape => shape with { Rotation = angle }).ToList());
        }

        if (snap)
        {
            delta = Math.Round(delta / SnapDegrees) * SnapDegrees;
        }

        return TurnAround(startShapes, frame, delta);
    }

    /// <summary>Moves every shape by the distance.</summary>
    public static IReadOnlyList<Shape> Move(IReadOnlyList<Shape> startShapes, double dx, double dy)
    {
        ArgumentNullException.ThrowIfNull(startShapes);
        return Guarded(startShapes, () => MoveCore(startShapes, dx, dy));
    }

    private static IReadOnlyList<Shape> MoveCore(IReadOnlyList<Shape> startShapes, double dx, double dy)
    {
        ArgumentNullException.ThrowIfNull(startShapes);

        if (!AllFinite(dx, dy))
        {
            return startShapes;
        }

        return Result(startShapes, startShapes.Select(shape => shape with { X = shape.X + dx, Y = shape.Y + dy }).ToList());
    }

    /// <summary>
    /// Moves the selection so the top left corner of its frame (the unturned box of a single shape, or the box around several) is at
    /// the given place. A <c>null</c> coordinate stays as it is.
    /// </summary>
    public static IReadOnlyList<Shape> SetPosition(IReadOnlyList<Shape> shapes, SelectionFrame frame, double? x, double? y)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        return Guarded(shapes, () => SetPositionCore(shapes, frame, x, y));
    }

    private static IReadOnlyList<Shape> SetPositionCore(IReadOnlyList<Shape> shapes, SelectionFrame frame, double? x, double? y)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        ArgumentNullException.ThrowIfNull(frame);

        var dx = x is { } newX ? newX - (frame.CenterX - frame.Width / 2) : 0;
        var dy = y is { } newY ? newY - (frame.CenterY - frame.Height / 2) : 0;
        return Move(shapes, dx, dy);
    }

    /// <summary>
    /// Sets the width and height of the frame, keeping its top left corner in place (in the turned frame for a rotated single shape).
    /// A <c>null</c> value stays as it is. With <paramref name="keepProportions"/> the other side follows the ratio of the frame.
    /// Several shapes scale together, uniformly if one of them is rotated. Negative sizes are treated as 0.
    /// </summary>
    public static IReadOnlyList<Shape> SetSize(IReadOnlyList<Shape> shapes, SelectionFrame frame, double? width, double? height, bool keepProportions)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        return Guarded(shapes, () => SetSizeCore(shapes, frame, width, height, keepProportions));
    }

    private static IReadOnlyList<Shape> SetSizeCore(IReadOnlyList<Shape> shapes, SelectionFrame frame, double? width, double? height, bool keepProportions)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        ArgumentNullException.ThrowIfNull(frame);

        if ((width is { } w && !double.IsFinite(w)) || (height is { } h && !double.IsFinite(h)))
        {
            return shapes;
        }

        var newWidth = Math.Max(0, width ?? frame.Width);
        var newHeight = Math.Max(0, height ?? frame.Height);
        var uniform = keepProportions || (!frame.IsSingle && shapes.Any(s => s.Rotation != 0));
        if (uniform && frame.Width > 0 && frame.Height > 0)
        {
            if (width is not null)
            {
                newHeight = frame.Height * newWidth / frame.Width;
            }
            else if (height is not null)
            {
                newWidth = frame.Width * newHeight / frame.Height;
            }
        }

        var anchorX = -frame.Width / 2;
        var anchorY = -frame.Height / 2;
        if (frame.IsSingle)
        {
            var (cx, cy) = Turn(anchorX + newWidth / 2, anchorY + newHeight / 2, frame.Rotation);
            var centerX = frame.CenterX + cx;
            var centerY = frame.CenterY + cy;
            return Result(shapes, shapes.Select(shape => shape with
            {
                X = centerX - newWidth / 2,
                Y = centerY - newHeight / 2,
                Width = newWidth,
                Height = newHeight,
            }).ToList());
        }

        var sx = frame.Width > 0 ? newWidth / frame.Width : 1;
        var sy = frame.Height > 0 ? newHeight / frame.Height : 1;
        return ScaleAbout(shapes, frame, anchorX, anchorY, sx, sy);
    }

    /// <summary>
    /// Sets the rotation to the angle. A single shape gets the angle itself. Several shapes turn together around the frame center
    /// by the angle (their frame counts as 0 degrees).
    /// </summary>
    public static IReadOnlyList<Shape> SetRotation(IReadOnlyList<Shape> shapes, SelectionFrame frame, double angle)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        return Guarded(shapes, () => SetRotationCore(shapes, frame, angle));
    }

    private static IReadOnlyList<Shape> SetRotationCore(IReadOnlyList<Shape> shapes, SelectionFrame frame, double angle)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        ArgumentNullException.ThrowIfNull(frame);

        if (!double.IsFinite(angle))
        {
            return shapes;
        }

        return frame.IsSingle
            ? Result(shapes, shapes.Select(shape => shape with { Rotation = angle }).ToList())
            : TurnAround(shapes, frame, angle);
    }

    /// <summary>Sets the rotation of every shape to 0, each around its own center.</summary>
    public static IReadOnlyList<Shape> ResetRotation(IReadOnlyList<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        return Guarded(shapes, () => ResetRotationCore(shapes));
    }

    private static IReadOnlyList<Shape> ResetRotationCore(IReadOnlyList<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        return Result(shapes, shapes.Select(shape => shape with { Rotation = 0 }).ToList());
    }

    // Numbers that are far too large overflow while shapes are built, and shapes reject non-finite numbers.
    private static IReadOnlyList<Shape> Guarded(IReadOnlyList<Shape> old, Func<IReadOnlyList<Shape>> transform)
    {
        try
        {
            return transform();
        }
        catch (ArgumentOutOfRangeException)
        {
            return old;
        }
    }

    private static IReadOnlyList<Shape> TurnAround(IReadOnlyList<Shape> shapes, SelectionFrame frame, double delta)
    {
        var turned = shapes.Select(shape =>
        {
            var (x, y) = Turn(shape.CenterX - frame.CenterX, shape.CenterY - frame.CenterY, delta);
            return shape with
            {
                X = frame.CenterX + x - shape.Width / 2,
                Y = frame.CenterY + y - shape.Height / 2,
                Rotation = shape.Rotation + delta,
            };
        }).ToList();
        return Result(shapes, turned);
    }

    // Scales the centers and sizes of all shapes about the anchor, which is given in the frame's own (turned) coordinates
    // relative to its center. A negative scale flips the shape's position to the other side of the anchor.
    private static IReadOnlyList<Shape> ScaleAbout(IReadOnlyList<Shape> shapes, SelectionFrame frame, double ax, double ay, double sx, double sy)
    {
        if (!AllFinite(ax, ay, sx, sy))
        {
            return shapes;
        }

        var scaled = shapes.Select(shape =>
        {
            var (lx, ly) = Turn(shape.CenterX - frame.CenterX, shape.CenterY - frame.CenterY, -frame.Rotation);
            var (cx, cy) = Turn(ax + (lx - ax) * sx, ay + (ly - ay) * sy, frame.Rotation);
            var width = shape.Width * Math.Abs(sx);
            var height = shape.Height * Math.Abs(sy);
            return shape with { X = frame.CenterX + cx - width / 2, Y = frame.CenterY + cy - height / 2, Width = width, Height = height };
        }).ToList();
        return Result(shapes, scaled);
    }

    // The new shapes if all their numbers are finite, otherwise the old ones. Shape setters reject non-finite numbers,
    // so a result that overflowed shows up as an exception while the shapes are built; the checks below catch the rest.
    private static IReadOnlyList<Shape> Result(IReadOnlyList<Shape> old, List<Shape> shapes) =>
        shapes.All(s => AllFinite(s.X, s.Y, s.Width, s.Height, s.Rotation)) ? shapes : old;

    private static bool AllFinite(params double[] values) => values.All(double.IsFinite);

    private static double Sign(double value) => value < 0 ? -1 : 1;

    private static double AngleOf(double x, double y) => Math.Atan2(y, x) * 180 / Math.PI;

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
