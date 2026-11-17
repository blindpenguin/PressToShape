using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>Changes to the list of layers of a document: insert and reorder. Layers are listed bottom first.</summary>
internal static class LayerOperations
{
    /// <summary>Inserts the layer so that it has this index afterwards (0 is the bottom, the layer count is the top).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is below 0 or above the layer count.</exception>
    /// <exception cref="ArgumentException">A layer or shape in <paramref name="layer"/> has an id that already exists.</exception>
    public static Document Insert(Document document, int index, Layer layer)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, document.Layers.Length);

        return document with { Layers = document.Layers.Insert(index, layer) };
    }

    /// <summary>Moves the layer so that it has this index afterwards. Returns the same document if it has it already.</summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a layer.</exception>
    public static Document MoveToIndex(Document document, Guid layerId, int index)
    {
        ArgumentNullException.ThrowIfNull(document);

        var from = IndexOf(document, layerId);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, document.Layers.Length);
        if (from == index)
        {
            return document;
        }

        var layer = document.Layers[from];
        return document with { Layers = document.Layers.RemoveAt(from).Insert(index, layer) };
    }

    /// <summary>Puts the layer directly above the target layer. Returns the same document if it is there already.</summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a layer.</exception>
    /// <exception cref="InvalidOperationException">The ids are the same.</exception>
    public static Document MoveAbove(Document document, Guid layerId, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(document);

        var from = IndexOf(document, layerId);
        var target = IndexOf(document, targetId);
        if (from == target)
        {
            throw new InvalidOperationException("A layer cannot be moved above itself.");
        }

        // Once the layer is taken out, every layer above it moves down by one.
        var to = from < target ? target : target + 1;
        return MoveToIndex(document, layerId, to);
    }

    /// <summary>
    /// Inserts a copy of the layer directly above it, with the same flags and a copy of every shape below it. The copy, and each shape and
    /// group in it, gets a new id.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="ArgumentException">An id of the copy exists in the document already.</exception>
    public static Document Duplicate(Document document, Guid layerId, Guid newLayerId, string name, Func<Guid> newShapeId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(newShapeId);

        var index = IndexOf(document, layerId);
        var original = document.Layers[index];
        var copy = original with
        {
            Id = newLayerId,
            Name = name,
            Shapes = [.. original.Shapes.Select(shape => ShapeTree.Reidentify(shape, newShapeId))],
        };

        return Insert(document, index + 1, copy);
    }

    /// <summary>
    /// Moves the shapes of the layer onto the top of the layer below and removes the layer. The lower layer keeps its id, name and flags.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="InvalidOperationException">The layer is the bottom layer.</exception>
    public static Document MergeDown(Document document, Guid layerId)
    {
        ArgumentNullException.ThrowIfNull(document);

        var index = IndexOf(document, layerId);
        if (index == 0)
        {
            throw new InvalidOperationException("The bottom layer has no layer below to merge into.");
        }

        var lower = document.Layers[index - 1];
        var merged = lower with { Shapes = lower.Shapes.AddRange(document.Layers[index].Shapes) };
        return document with { Layers = document.Layers.RemoveAt(index).SetItem(index - 1, merged) };
    }

    /// <summary>
    /// Moves the shapes onto the top of the layer. Shapes inside groups leave their group, and a group that loses its last child goes with
    /// it. The shapes keep their order in the document: lower layers first, then stack order. A shape and its own group move together.
    /// </summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape, or no layer has the layer id.</exception>
    public static Document MoveShapesToLayer(Document document, IReadOnlySet<Guid> ids, Guid layerId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);

        IndexOf(document, layerId);
        foreach (var id in ids)
        {
            _ = ShapeTree.Find(document.Layers, id) ?? throw new KeyNotFoundException($"No shape with id {id}.");
        }

        var moved = new List<Shape>();
        foreach (var layer in document.Layers)
        {
            Collect(layer.Shapes, ids, moved);
        }

        if (moved.Count == 0)
        {
            return document;
        }

        return document.RemoveShapes(moved.Select(shape => shape.Id).ToHashSet()).AddShapes(layerId, moved);
    }

    // A shape that is collected is not searched further, so a selected group does not move its selected children a second time.
    private static void Collect(ImmutableArray<Shape> shapes, IReadOnlySet<Guid> ids, List<Shape> moved)
    {
        foreach (var shape in shapes)
        {
            if (ids.Contains(shape.Id))
            {
                moved.Add(shape);
            }
            else if (shape is GroupShape group)
            {
                Collect(group.Children, ids, moved);
            }
        }
    }

    private static int IndexOf(Document document, Guid layerId)
    {
        for (var i = 0; i < document.Layers.Length; i++)
        {
            if (document.Layers[i].Id == layerId)
            {
                return i;
            }
        }

        throw new KeyNotFoundException($"No layer with id {layerId}.");
    }
}
