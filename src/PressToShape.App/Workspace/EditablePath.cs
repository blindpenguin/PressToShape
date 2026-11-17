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

        return path.WithRotationApplied();
    }
}
