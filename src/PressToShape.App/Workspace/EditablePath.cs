using System;
using System.Collections.Generic;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>What the node tools and the Pen edit: a path, or a rectangle or an ellipse seen as the path it would become.</summary>
internal static class EditablePath
{
    /// <summary>
    /// The path of a shape: the path itself, the conversion of a rectangle or an ellipse (not stored, the shape is only replaced when it is
    /// edited), and <c>null</c> for every other shape.
    /// </summary>
    public static PathShape? For(Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        return shape as PathShape ?? ShapeConversion.ToPath(shape);
    }

    /// <summary>
    /// The editable paths of the selected shapes (the shapes below a selected group too) that are visible and not locked, each with its
    /// rotation applied (see <see cref="AsOnScreen"/>), in drawing order.
    /// </summary>
    public static IReadOnlyList<PathShape> OnScreenOfSelection(Document document, IEnumerable<Guid> selectedShapeIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selectedShapeIds);

        var paths = new List<PathShape>();
        foreach (var leaf in SelectionQueries.SelectedLeaves(document, selectedShapeIds))
        {
            if (SelectionQueries.IsSelectable(document, leaf.Id) && For(leaf) is { } path)
            {
                paths.Add(AsOnScreen(path));
            }
        }

        return paths;
    }

    /// <summary>
    /// The same path (same id) with its rotation applied to every node and handle point, so it has no rotation itself and its positions are
    /// the ones on the screen. For hit tests and drawing, not for storing.
    /// </summary>
    public static PathShape AsOnScreen(PathShape path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Rotation == 0)
        {
            return path;
        }

        var radians = path.Rotation * Math.PI / 180;
        var sin = Math.Sin(radians);
        var cos = Math.Cos(radians);
        (double X, double Y) Turn(double x, double y)
        {
            var dx = x - path.CenterX;
            var dy = y - path.CenterY;
            return (path.CenterX + dx * cos - dy * sin, path.CenterY + dx * sin + dy * cos);
        }

        var turned = new List<DocumentNode>();
        foreach (var node in path.ToDocumentNodes())
        {
            var anchor = Turn(node.X, node.Y);
            var handleIn = node.HasIn ? Turn(node.InX!.Value, node.InY!.Value) : ((double X, double Y)?)null;
            var handleOut = node.HasOut ? Turn(node.OutX!.Value, node.OutY!.Value) : ((double X, double Y)?)null;
            turned.Add(new DocumentNode(anchor.X, anchor.Y, handleIn?.X, handleIn?.Y, handleOut?.X, handleOut?.Y, node.Type));
        }

        return PathShape.Create(turned, path.IsClosed, path.FillRule) with { Id = path.Id };
    }
}
