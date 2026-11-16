using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// Questions about which shapes can be selected and which shapes a selection box touches. A shape can be selected when its layer and
/// every group around it are visible and not locked, and it is visible and not locked itself. Results are in document order: bottom
/// layer first, and within a layer or group bottom shape first, a group before its children. Groups are shapes too: a click selects the
/// group, and only after the group was entered (see <see cref="EditorState.EnteredGroupId"/>) its children.
/// </summary>
internal static class SelectionQueries
{
    /// <summary>Whether the document has a shape or group with this id that can be selected.</summary>
    public static bool IsSelectable(Document document, Guid shapeId)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.FindShape(shapeId) is { IsEffectivelyVisible: true, IsEffectivelyLocked: false, Shape: { IsVisible: true, IsLocked: false } };
    }

    /// <summary>
    /// The shapes and groups a click or a box can select: the direct children of the entered group, or, without one, the top-level shapes
    /// of every layer. Hidden and locked ones, and the ones in hidden or locked layers or groups, are left out.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="enteredGroupId">The group that was entered, or null.</param>
    public static IReadOnlyList<Shape> Selectable(Document document, Guid? enteredGroupId = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (enteredGroupId is { } groupId)
        {
            return document.FindShape(groupId) is { Shape: GroupShape group } location && IsSelectableLocation(location)
                ? group.Children.Where(child => child is { IsVisible: true, IsLocked: false }).ToList()
                : [];
        }

        return document.Layers
            .Where(layer => layer.IsVisible && !layer.IsLocked)
            .SelectMany(layer => layer.Shapes)
            .Where(shape => shape is { IsVisible: true, IsLocked: false })
            .ToList();
    }

    /// <summary>
    /// The selectable shapes (see <see cref="Selectable"/>) whose axis-aligned bounds, after rotation, overlap the rectangle. Touching edges count.
    /// </summary>
    public static IReadOnlyList<Shape> ShapesTouching(Document document, double x, double y, double width, double height, Guid? enteredGroupId = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var left = Math.Min(x, x + width);
        var right = Math.Max(x, x + width);
        var top = Math.Min(y, y + height);
        var bottom = Math.Max(y, y + height);

        return Selectable(document, enteredGroupId)
            .Where(shape =>
            {
                var bounds = shape.RotatedBounds;
                return bounds.X <= right && bounds.X + bounds.Width >= left
                    && bounds.Y <= bottom && bounds.Y + bounds.Height >= top;
            })
            .ToList();
    }

    /// <summary>
    /// The shape or group that a hit on <paramref name="hit"/> selects. Without an entered group it is the outermost group around the hit
    /// shape, or the shape itself. With one it is the direct child of the entered group that holds the hit shape, and <c>null</c> when
    /// the hit shape is not inside the entered group.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="hit">A shape from <see cref="Document.HitTest"/>.</param>
    /// <param name="enteredGroupId">The group that was entered, or null.</param>
    public static Shape? TargetFor(Document document, Shape hit, Guid? enteredGroupId = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(hit);

        // The chain from the hit shape up to the outermost group.
        var chain = new List<Shape> { hit };
        var location = document.FindShape(hit.Id);
        while (location?.Parent is { } parent)
        {
            chain.Add(parent);
            location = document.FindShape(parent.Id);
        }

        if (enteredGroupId is not { } entered)
        {
            return chain[^1];
        }

        var index = chain.FindIndex(shape => shape.Id == entered);
        return index > 0 ? chain[index - 1] : null;
    }

    /// <summary>The shapes and groups with these ids, in document order. Unknown ids are ignored.</summary>
    public static IReadOnlyList<Shape> SelectedShapes(Document document, IEnumerable<Guid> shapeIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(shapeIds);

        var wanted = shapeIds.ToHashSet();
        return Walk(document).Where(node => wanted.Contains(node.Shape.Id)).Select(node => node.Shape).ToList();
    }

    /// <summary>The shapes and groups with these ids that can be selected, in document order. One pass, however many ids.</summary>
    public static IReadOnlyList<Shape> SelectedSelectable(Document document, IEnumerable<Guid> shapeIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(shapeIds);

        var wanted = shapeIds.ToHashSet();
        return Walk(document).Where(node => node.IsSelectable && wanted.Contains(node.Shape.Id)).Select(node => node.Shape).ToList();
    }

    /// <summary>
    /// The shapes below the selected shapes and groups that are not groups, in document order: what move, resize, rotate and the
    /// appearance work on. A selected plain shape is its own leaf. Each leaf appears once.
    /// </summary>
    public static IReadOnlyList<Shape> SelectedLeaves(Document document, IEnumerable<Guid> shapeIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(shapeIds);

        var seen = new HashSet<Guid>();
        return SelectedShapes(document, shapeIds).SelectMany(shape => shape.Leaves()).Where(leaf => seen.Add(leaf.Id)).ToList();
    }

    /// <summary>
    /// Of the wanted ids, the ones that can be selected, each with the id of its closest group (null on the top level of a layer).
    /// One pass over the document.
    /// </summary>
    public static IReadOnlyDictionary<Guid, Guid?> SelectableWithParents(Document document, IReadOnlySet<Guid> wanted)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(wanted);

        var found = new Dictionary<Guid, Guid?>();
        if (wanted.Count == 0)
        {
            return found;
        }

        foreach (var node in Walk(document))
        {
            if (node.IsSelectable && wanted.Contains(node.Shape.Id))
            {
                found[node.Shape.Id] = node.ParentId;
            }
        }

        return found;
    }

    private static bool IsSelectableLocation(ShapeLocation location) =>
        location is { IsEffectivelyVisible: true, IsEffectivelyLocked: false, Shape: { IsVisible: true, IsLocked: false } };

    private readonly record struct Node(Shape Shape, Guid? ParentId, bool IsSelectable);

    // Every shape and group of the document, a group before its children. A node is selectable when it and everything around it is
    // visible and not locked.
    private static IEnumerable<Node> Walk(Document document)
    {
        foreach (var layer in document.Layers)
        {
            foreach (var node in WalkShapes(layer.Shapes, null, layer.IsVisible && !layer.IsLocked))
            {
                yield return node;
            }
        }
    }

    private static IEnumerable<Node> WalkShapes(IEnumerable<Shape> shapes, Guid? parentId, bool parentOk)
    {
        foreach (var shape in shapes)
        {
            var ok = parentOk && shape is { IsVisible: true, IsLocked: false };
            yield return new Node(shape, parentId, ok);
            if (shape is GroupShape group)
            {
                foreach (var node in WalkShapes(group.Children, group.Id, ok))
                {
                    yield return node;
                }
            }
        }
    }
}
