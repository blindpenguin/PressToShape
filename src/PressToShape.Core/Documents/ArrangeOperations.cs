using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>Changes the stacking order of shapes among their siblings (the shapes of one layer or of one group). Nothing changes its parent.</summary>
internal static class ArrangeOperations
{
    /// <summary>
    /// Moves the shapes. Shapes with different parents are arranged each among their own siblings. The selected siblings keep their order
    /// among themselves. Returns the same document if no shape changes its place.
    /// </summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public static Document Arrange(Document document, IReadOnlySet<Guid> ids, ArrangeType type)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);

        var parents = new Dictionary<(int LayerIndex, Guid? ParentId), List<Guid>>();
        foreach (var id in ids)
        {
            var found = ShapeTree.Find(document.Layers, id) ?? throw new KeyNotFoundException($"No shape with id {id}.");
            var key = (found.LayerIndex, found.Parent?.Id);
            if (!parents.TryGetValue(key, out var list))
            {
                parents[key] = list = [];
            }

            list.Add(id);
        }

        var result = document;
        foreach (var ((layerIndex, parentId), siblingIds) in parents)
        {
            // Earlier changes may have rebuilt the parent, so it is looked up again in the current document.
            var siblings = SiblingsOf(result, layerIndex, parentId, out var parent);
            var selected = siblingIds.ToHashSet();
            var arranged = Reorder(siblings, selected, type);
            if (arranged != siblings)
            {
                result = GroupOperations.WithSiblings(result, layerIndex, parent, arranged);
            }
        }

        return result;
    }

    /// <summary>
    /// Puts the shape directly above another shape that has the same parent. Returns the same document if it is there already.
    /// </summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    /// <exception cref="InvalidOperationException">The shapes have different parents, or are the same shape.</exception>
    public static Document MoveAbove(Document document, Guid id, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(document);

        var found = ShapeTree.Find(document.Layers, id) ?? throw new KeyNotFoundException($"No shape with id {id}.");
        var target = ShapeTree.Find(document.Layers, targetId) ?? throw new KeyNotFoundException($"No shape with id {targetId}.");
        if (id == targetId || found.LayerIndex != target.LayerIndex || found.Parent?.Id != target.Parent?.Id)
        {
            throw new InvalidOperationException("A shape can only be moved among its own siblings.");
        }

        var siblings = SiblingsOf(document, found.LayerIndex, found.Parent?.Id, out var parent);
        var from = siblings.IndexOf(found.Shape);
        var without = siblings.RemoveAt(from);
        var arranged = without.Insert(without.IndexOf(target.Shape) + 1, found.Shape);
        return arranged.SequenceEqual(siblings) ? document : GroupOperations.WithSiblings(document, found.LayerIndex, parent, arranged);
    }

    private static ImmutableArray<Shape> SiblingsOf(Document document, int layerIndex, Guid? parentId, out GroupShape? parent)
    {
        parent = null;
        var layer = document.Layers[layerIndex];
        if (parentId is null)
        {
            return layer.Shapes;
        }

        parent = (GroupShape)ShapeTree.Find(document.Layers, parentId.Value)!.Value.Shape;
        return parent.Children;
    }

    // Returns the same array when the order does not change.
    private static ImmutableArray<Shape> Reorder(ImmutableArray<Shape> siblings, HashSet<Guid> selected, ArrangeType type)
    {
        var order = siblings.ToList();
        var isSelected = (Shape shape) => selected.Contains(shape.Id);
        switch (type)
        {
            case ArrangeType.ToFront:
                order = [.. order.Where(shape => !isSelected(shape)), .. order.Where(isSelected)];
                break;
            case ArrangeType.ToBack:
                order = [.. order.Where(isSelected), .. order.Where(shape => !isSelected(shape))];
                break;
            case ArrangeType.Forward:
                for (var i = order.Count - 2; i >= 0; i--)
                {
                    if (isSelected(order[i]) && !isSelected(order[i + 1]))
                    {
                        (order[i], order[i + 1]) = (order[i + 1], order[i]);
                    }
                }

                break;
            case ArrangeType.Backward:
                for (var i = 1; i < order.Count; i++)
                {
                    if (isSelected(order[i]) && !isSelected(order[i - 1]))
                    {
                        (order[i], order[i - 1]) = (order[i - 1], order[i]);
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return order.SequenceEqual(siblings) ? siblings : [.. order];
    }
}
