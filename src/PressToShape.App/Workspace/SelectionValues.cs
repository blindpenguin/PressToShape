using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>A value that is the same for every selected shape, or marked as mixed when the shapes differ.</summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="Value">The shared value. For a mixed value it is the value of the first shape.</param>
/// <param name="IsMixed">Whether the shapes have different values.</param>
internal readonly record struct Mixed<T>(T Value, bool IsMixed)
{
    /// <summary>Combines the values of all shapes.</summary>
    public static Mixed<T> Of(IEnumerable<T> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        using var enumerator = values.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return default;
        }

        var first = enumerator.Current;
        var comparer = EqualityComparer<T>.Default;
        while (enumerator.MoveNext())
        {
            if (!comparer.Equals(first, enumerator.Current))
            {
                return new Mixed<T>(first, true);
            }
        }

        return new Mixed<T>(first, false);
    }
}

/// <summary>
/// The values the Properties tab shows for a selection. Position and size are those of the selection frame (one shape: its own box,
/// several: the box around all of them, with rotation 0). Appearance values are mixed when the shapes differ.
/// </summary>
/// <param name="X">Left edge of the frame.</param>
/// <param name="Y">Top edge of the frame.</param>
/// <param name="Width">Width of the frame.</param>
/// <param name="Height">Height of the frame.</param>
/// <param name="Rotation">Rotation in degrees. 0 for several shapes.</param>
/// <param name="IsSingle">Whether exactly one shape is selected.</param>
/// <param name="Fill">Fill color, <c>null</c> inside means no fill.</param>
/// <param name="Stroke">Stroke color, <c>null</c> inside means no stroke.</param>
/// <param name="StrokeWidth">Stroke width.</param>
/// <param name="Opacity">Opacity from 0 to 1.</param>
internal sealed record SelectionValues(
    double X,
    double Y,
    double Width,
    double Height,
    double Rotation,
    bool IsSingle,
    Mixed<Color?> Fill,
    Mixed<Color?> Stroke,
    Mixed<double> StrokeWidth,
    Mixed<double> Opacity)
{
    /// <summary>The values of the shapes, or <c>null</c> for an empty list.</summary>
    public static SelectionValues? From(IReadOnlyList<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        if (SelectionGeometry.FrameOf(shapes) is not { } frame)
        {
            return null;
        }

        return new SelectionValues(
            frame.CenterX - frame.Width / 2,
            frame.CenterY - frame.Height / 2,
            frame.Width,
            frame.Height,
            frame.Rotation,
            frame.IsSingle,
            Mixed<Color?>.Of(shapes.Select(s => s.Appearance.Fill)),
            Mixed<Color?>.Of(shapes.Select(s => s.Appearance.Stroke)),
            Mixed<double>.Of(shapes.Select(s => s.Appearance.StrokeWidth)),
            Mixed<double>.Of(shapes.Select(s => s.Appearance.Opacity)));
    }
}
