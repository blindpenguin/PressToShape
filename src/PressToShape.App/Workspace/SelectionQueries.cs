using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// Questions about which shapes can be selected and which shapes a selection box touches.
/// A shape can be selected when it is in a visible, unlocked layer. Results are in document order:
/// bottom layer first, and within a layer bottom shape first.
/// </summary>
internal static class SelectionQueries
{
    /// <summary>Whether the document has a shape with this id in a visible, unlocked layer.</summary>
    public static bool IsSelectable(Document document, Guid shapeId)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.FindShape(shapeId) is { Layer: { IsVisible: true, IsLocked: false } };
    }

    /// <summary>All shapes that can be selected.</summary>
    public static IReadOnlyList<Shape> Selectable(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.Layers
            .Where(layer => layer.IsVisible && !layer.IsLocked)
            .SelectMany(layer => layer.Shapes)
            .ToList();
    }

    /// <summary>
    /// The selectable shapes whose axis-aligned bounds, after rotation, overlap the rectangle. Touching edges count.
    /// </summary>
    public static IReadOnlyList<Shape> ShapesTouching(Document document, double x, double y, double width, double height)
    {
        ArgumentNullException.ThrowIfNull(document);

        var left = Math.Min(x, x + width);
        var right = Math.Max(x, x + width);
        var top = Math.Min(y, y + height);
        var bottom = Math.Max(y, y + height);

        return Selectable(document)
            .Where(shape =>
            {
                var bounds = shape.RotatedBounds;
                return bounds.X <= right && bounds.X + bounds.Width >= left
                    && bounds.Y <= bottom && bounds.Y + bounds.Height >= top;
            })
            .ToList();
    }

    /// <summary>The shapes with these ids, in document order. Unknown ids are ignored.</summary>
    public static IReadOnlyList<Shape> SelectedShapes(Document document, IEnumerable<Guid> shapeIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(shapeIds);

        var wanted = shapeIds.ToHashSet();
        return document.Layers
            .SelectMany(layer => layer.Shapes)
            .Where(shape => wanted.Contains(shape.Id))
            .ToList();
    }
}
