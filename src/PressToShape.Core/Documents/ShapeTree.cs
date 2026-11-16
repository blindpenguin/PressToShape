using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>
/// The one place that walks nested shapes. All results are new values, the input is never changed. A group's box follows its
/// children by itself (see <see cref="GroupShape"/>), so every method that changes children also updates the boxes of all ancestors.
/// Every walk stops with an <see cref="InvalidOperationException"/> when groups are nested deeper than <see cref="MaxDepth"/>.
/// </summary>
internal static class ShapeTree
{
    /// <summary>The deepest nesting of groups that the model accepts.</summary>
    public const int MaxDepth = 64;

    /// <summary>Where a shape was found: the layer, the enclosing groups from the outermost to the innermost, and the shape.</summary>
    /// <param name="LayerIndex">Index of the layer in the document.</param>
    /// <param name="Parents">The enclosing groups, outermost first. Empty for a shape on the top level of the layer.</param>
    /// <param name="Shape">The shape.</param>
    public readonly record struct Found(int LayerIndex, ImmutableArray<GroupShape> Parents, Shape Shape)
    {
        /// <summary>The closest enclosing group, or null on the top level.</summary>
        public GroupShape? Parent => Parents.IsEmpty ? null : Parents[^1];
    }

    /// <summary>The shape itself if it is not a group, otherwise all non-group shapes below it, bottom first.</summary>
    public static IReadOnlyList<Shape> Leaves(Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        var leaves = new List<Shape>();
        CollectLeaves(shape, leaves, 0);
        return leaves;
    }

    /// <summary>Finds a shape (or group) by id in the layers. Returns null if no shape has this id.</summary>
    public static Found? Find(ImmutableArray<Layer> layers, Guid id)
    {
        for (var layerIndex = 0; layerIndex < layers.Length; layerIndex++)
        {
            var parents = new List<GroupShape>();
            if (FindIn(layers[layerIndex].Shapes, id, parents, 0) is { } shape)
            {
                return new Found(layerIndex, [.. parents], shape);
            }
        }

        return null;
    }

    /// <summary>
    /// Replaces every shape whose id is a key of <paramref name="replacements"/>, anywhere in the list. Ancestors are rebuilt, so
    /// their boxes are right. Shapes inside a replaced shape are not visited. Counts the replaced shapes in <paramref name="replaced"/>.
    /// </summary>
    public static ImmutableArray<Shape> Replace(ImmutableArray<Shape> shapes, IReadOnlyDictionary<Guid, Shape> replacements, ref int replaced) =>
        ReplaceIn(shapes, replacements, ref replaced, 0);

    /// <summary>
    /// Applies <paramref name="change"/> to every shape and group in the list, children before the group around them. A change that returns
    /// its input leaves it alone. Returns the same array if nothing changes.
    /// </summary>
    public static ImmutableArray<Shape> Map(ImmutableArray<Shape> shapes, Func<Shape, Shape> change)
    {
        ArgumentNullException.ThrowIfNull(change);

        return MapIn(shapes, change, 0);
    }

    /// <summary>
    /// Removes the shapes with these ids, anywhere in the list. A group that loses its last child is removed too, and the boxes of the
    /// other ancestors are updated. Counts in <paramref name="matched"/> how many of the ids were found, descendants of a removed
    /// group included.
    /// </summary>
    public static ImmutableArray<Shape> Remove(ImmutableArray<Shape> shapes, IReadOnlySet<Guid> ids, ref int matched) =>
        RemoveIn(shapes, ids, ref matched, 0);

    /// <summary>
    /// Adds the shapes at the point to <paramref name="hits"/>, topmost first. Groups are entered, never returned. Hidden shapes and
    /// hidden groups are skipped.
    /// </summary>
    public static void HitTest(ImmutableArray<Shape> shapes, double x, double y, List<Shape> hits) =>
        HitTestIn(shapes, x, y, hits, 0);

    /// <summary>A copy of the shape and everything below it, each with a new id from <paramref name="newId"/>.</summary>
    public static Shape Reidentify(Shape shape, Func<Guid> newId)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ArgumentNullException.ThrowIfNull(newId);

        return ReidentifyIn(shape, newId, 0);
    }

    /// <summary>A copy of the shape moved by the distance. A group moves all its leaves.</summary>
    public static Shape Translate(Shape shape, double dx, double dy)
    {
        ArgumentNullException.ThrowIfNull(shape);

        return TranslateIn(shape, dx, dy, 0);
    }

    /// <summary>The number of group levels below and including this shape: 0 for a plain shape, 1 for a group of plain shapes.</summary>
    public static int Depth(Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        return DepthIn(shape, 0);
    }

    /// <summary>
    /// Whether the shape is shown and whether it is locked, counting the enclosing groups: hidden or locked if the shape or any
    /// enclosing group is hidden or locked. Layers are not included.
    /// </summary>
    public static (bool IsVisible, bool IsLocked) EffectiveState(Found found)
    {
        var visible = found.Shape.IsVisible;
        var locked = found.Shape.IsLocked;
        foreach (var parent in found.Parents)
        {
            visible &= parent.IsVisible;
            locked |= parent.IsLocked;
        }

        return (visible, locked);
    }

    private static void CollectLeaves(Shape shape, List<Shape> leaves, int depth)
    {
        if (shape is not GroupShape group)
        {
            leaves.Add(shape);
            return;
        }

        CheckDepth(depth + 1);
        foreach (var child in group.Children)
        {
            CollectLeaves(child, leaves, depth + 1);
        }
    }

    private static Shape? FindIn(ImmutableArray<Shape> shapes, Guid id, List<GroupShape> parents, int depth)
    {
        foreach (var shape in shapes)
        {
            if (shape.Id == id)
            {
                return shape;
            }

            if (shape is GroupShape group)
            {
                CheckDepth(depth + 1);
                parents.Add(group);
                if (FindIn(group.Children, id, parents, depth + 1) is { } inner)
                {
                    return inner;
                }

                parents.RemoveAt(parents.Count - 1);
            }
        }

        return null;
    }

    private static ImmutableArray<Shape> ReplaceIn(ImmutableArray<Shape> shapes, IReadOnlyDictionary<Guid, Shape> replacements, ref int replaced, int depth)
    {
        ImmutableArray<Shape>.Builder? builder = null;
        for (var i = 0; i < shapes.Length; i++)
        {
            var shape = shapes[i];
            Shape result = shape;
            if (replacements.TryGetValue(shape.Id, out var replacement))
            {
                result = replacement;
                replaced++;
            }
            else if (shape is GroupShape group)
            {
                CheckDepth(depth + 1);
                var children = ReplaceIn(group.Children, replacements, ref replaced, depth + 1);
                if (children != group.Children)
                {
                    result = group with { Children = children };
                }
            }

            if (!ReferenceEquals(result, shape))
            {
                builder ??= shapes.ToBuilder();
                builder[i] = result;
            }
        }

        return builder is null ? shapes : builder.MoveToImmutable();
    }

    private static ImmutableArray<Shape> MapIn(ImmutableArray<Shape> shapes, Func<Shape, Shape> change, int depth)
    {
        ImmutableArray<Shape>.Builder? builder = null;
        for (var i = 0; i < shapes.Length; i++)
        {
            var shape = shapes[i];
            if (shape is GroupShape group)
            {
                CheckDepth(depth + 1);
                var children = MapIn(group.Children, change, depth + 1);
                if (children != group.Children)
                {
                    shape = group with { Children = children };
                }
            }

            shape = change(shape);
            if (!ReferenceEquals(shape, shapes[i]))
            {
                builder ??= shapes.ToBuilder();
                builder[i] = shape;
            }
        }

        return builder is null ? shapes : builder.MoveToImmutable();
    }

    private static ImmutableArray<Shape> RemoveIn(ImmutableArray<Shape> shapes, IReadOnlySet<Guid> ids, ref int matched, int depth)
    {
        ImmutableArray<Shape>.Builder? builder = null;
        for (var i = 0; i < shapes.Length; i++)
        {
            var shape = shapes[i];
            Shape? result = shape;
            if (ids.Contains(shape.Id))
            {
                matched++;
                result = null;
                if (shape is GroupShape removedGroup)
                {
                    CheckDepth(depth + 1);
                    matched += CountMatches(removedGroup.Children, ids, depth + 1);
                }
            }
            else if (shape is GroupShape group)
            {
                CheckDepth(depth + 1);
                var children = RemoveIn(group.Children, ids, ref matched, depth + 1);
                if (children != group.Children)
                {
                    result = children.IsEmpty ? null : group with { Children = children };
                }
            }

            if (!ReferenceEquals(result, shape))
            {
                builder ??= shapes.ToBuilder();
                builder[i] = result!;
            }
        }

        return builder is null ? shapes : builder.Where(shape => shape is not null).ToImmutableArray();
    }

    private static int CountMatches(ImmutableArray<Shape> shapes, IReadOnlySet<Guid> ids, int depth)
    {
        var count = 0;
        foreach (var shape in shapes)
        {
            if (ids.Contains(shape.Id))
            {
                count++;
            }

            if (shape is GroupShape group)
            {
                CheckDepth(depth + 1);
                count += CountMatches(group.Children, ids, depth + 1);
            }
        }

        return count;
    }

    private static void HitTestIn(ImmutableArray<Shape> shapes, double x, double y, List<Shape> hits, int depth)
    {
        for (var i = shapes.Length - 1; i >= 0; i--)
        {
            var shape = shapes[i];
            if (!shape.IsVisible)
            {
                continue;
            }

            if (shape is GroupShape group)
            {
                CheckDepth(depth + 1);
                HitTestIn(group.Children, x, y, hits, depth + 1);
            }
            else if (shape.Contains(x, y))
            {
                hits.Add(shape);
            }
        }
    }

    private static Shape ReidentifyIn(Shape shape, Func<Guid> newId, int depth)
    {
        if (shape is not GroupShape group)
        {
            return shape with { Id = newId() };
        }

        CheckDepth(depth + 1);
        var children = group.Children.Select(child => ReidentifyIn(child, newId, depth + 1)).ToImmutableArray();
        return group with { Id = newId(), Children = children };
    }

    private static Shape TranslateIn(Shape shape, double dx, double dy, int depth)
    {
        if (shape is not GroupShape group)
        {
            return shape with { X = shape.X + dx, Y = shape.Y + dy };
        }

        CheckDepth(depth + 1);
        return group with { Children = group.Children.Select(child => TranslateIn(child, dx, dy, depth + 1)).ToImmutableArray() };
    }

    private static int DepthIn(Shape shape, int depth)
    {
        if (shape is not GroupShape group)
        {
            return 0;
        }

        CheckDepth(depth + 1);
        return 1 + group.Children.Max(child => DepthIn(child, depth + 1));
    }

    private static void CheckDepth(int depth)
    {
        if (depth > MaxDepth)
        {
            throw new InvalidOperationException($"Groups are nested deeper than {MaxDepth} levels.");
        }
    }
}
