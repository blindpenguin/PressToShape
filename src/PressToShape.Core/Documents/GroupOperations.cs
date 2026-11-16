using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>Group and ungroup on a document. Documents are validated when they are made, so their groups are at most <see cref="ShapeTree.MaxDepth"/> deep.</summary>
internal static class GroupOperations
{
    /// <summary>
    /// Replaces the shapes with one new group. The group takes the stack position of the topmost shape, and the shapes keep their order.
    /// </summary>
    /// <exception cref="ArgumentException">Fewer than two ids, or an id twice.</exception>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    /// <exception cref="InvalidOperationException">The shapes do not have the same parent (the same layer or the same group).</exception>
    public static Document Group(Document document, IReadOnlyList<Guid> ids, Guid groupId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);

        if (ids.Count < 2 || ids.Distinct().Count() != ids.Count)
        {
            throw new ArgumentException("Grouping needs at least two different shapes.", nameof(ids));
        }

        var found = ids
            .Select(id => ShapeTree.Find(document.Layers, id) ?? throw new KeyNotFoundException($"No shape with id {id}."))
            .ToList();
        var layerIndex = found[0].LayerIndex;
        var parent = found[0].Parent;
        if (found.Any(f => f.LayerIndex != layerIndex || f.Parent?.Id != parent?.Id))
        {
            throw new InvalidOperationException("Shapes can only be grouped with shapes of the same layer and group.");
        }

        var layer = document.Layers[layerIndex];
        var siblings = parent is null ? layer.Shapes : parent.Children;
        var members = ids.ToHashSet();
        var topmost = siblings.Select((shape, index) => (shape, index)).Last(x => members.Contains(x.shape.Id)).index;

        var group = new GroupShape { Id = groupId, Children = [.. siblings.Where(shape => members.Contains(shape.Id))] };
        var result = ImmutableArray.CreateBuilder<Shape>(siblings.Length - members.Count + 1);
        for (var i = 0; i < siblings.Length; i++)
        {
            if (i == topmost)
            {
                result.Add(group);
            }
            else if (!members.Contains(siblings[i].Id))
            {
                result.Add(siblings[i]);
            }
        }

        return WithSiblings(document, layerIndex, parent, result.ToImmutable());
    }

    /// <summary>
    /// Replaces each group by its children, in place and in order. One level for each group: a group inside a group that is not in
    /// <paramref name="groupIds"/> stays a group. The groups' own names and flags are dropped.
    /// </summary>
    /// <exception cref="ArgumentException">An id is the id of a shape that is not a group.</exception>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public static Document Ungroup(Document document, IReadOnlySet<Guid> groupIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(groupIds);

        if (groupIds.Count == 0)
        {
            return document;
        }

        foreach (var id in groupIds)
        {
            var found = ShapeTree.Find(document.Layers, id) ?? throw new KeyNotFoundException($"No shape with id {id}.");
            if (found.Shape is not GroupShape)
            {
                throw new ArgumentException($"Shape {id} is not a group.", nameof(groupIds));
            }
        }

        var layers = document.Layers.Select(layer => layer with { Shapes = UngroupIn(layer.Shapes, groupIds) }).ToImmutableArray();
        return document with { Layers = layers };
    }

    // The shapes of a layer or a group are replaced. For a group the change goes through the tree, so its ancestors follow.
    internal static Document WithSiblings(Document document, int layerIndex, GroupShape? parent, ImmutableArray<Shape> siblings)
    {
        var layer = document.Layers[layerIndex];
        if (parent is null)
        {
            return document with { Layers = document.Layers.SetItem(layerIndex, layer with { Shapes = siblings }) };
        }

        var replaced = 0;
        var shapes = ShapeTree.Replace(layer.Shapes, new Dictionary<Guid, Shape> { [parent.Id] = parent with { Children = siblings } }, ref replaced);
        return document with { Layers = document.Layers.SetItem(layerIndex, layer with { Shapes = shapes }) };
    }

    private static ImmutableArray<Shape> UngroupIn(ImmutableArray<Shape> shapes, IReadOnlySet<Guid> groupIds)
    {
        ImmutableArray<Shape>.Builder? builder = null;
        for (var i = 0; i < shapes.Length; i++)
        {
            if (shapes[i] is not GroupShape group)
            {
                builder?.Add(shapes[i]);
                continue;
            }

            var children = UngroupIn(group.Children, groupIds);
            var ungrouped = groupIds.Contains(group.Id);
            if (!ungrouped && children == group.Children)
            {
                builder?.Add(shapes[i]);
                continue;
            }

            if (builder is null)
            {
                builder = ImmutableArray.CreateBuilder<Shape>();
                builder.AddRange(shapes.Take(i));
            }

            if (ungrouped)
            {
                builder.AddRange(children);
            }
            else
            {
                builder.Add(group with { Children = children });
            }
        }

        return builder is null ? shapes : builder.ToImmutable();
    }
}
