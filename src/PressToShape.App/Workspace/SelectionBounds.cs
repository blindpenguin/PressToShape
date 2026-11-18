using System;
using System.Collections.Generic;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>The box around shapes, for snapping and nudging. Pure function.</summary>
internal static class SelectionBounds
{
    /// <summary>The axis-aligned bounds around the turned outlines of the shapes, or <c>null</c> for no shapes.</summary>
    public static Bounds? Union(IReadOnlyList<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        if (shapes.Count == 0)
        {
            return null;
        }

        var left = double.MaxValue;
        var top = double.MaxValue;
        var right = double.MinValue;
        var bottom = double.MinValue;
        foreach (var shape in shapes)
        {
            var box = shape.RotatedBounds;
            left = Math.Min(left, box.X);
            top = Math.Min(top, box.Y);
            right = Math.Max(right, box.X + box.Width);
            bottom = Math.Max(bottom, box.Y + box.Height);
        }

        return new Bounds(left, top, right - left, bottom - top);
    }
}
