using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class ShapeTreeTests
{
    private static RectangleShape Rect(double x, double y, double w = 10, double h = 10) => new() { X = x, Y = y, Width = w, Height = h };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static ImmutableArray<Layer> Layers(params Shape[] shapes) =>
        [new Layer { Name = "L", Shapes = [.. shapes] }];

    private static Func<Guid> Ids()
    {
        var next = 100;
        return () => new Guid(next++, 0, 0, new byte[8]);
    }

    [Fact]
    public void Leaves_PlainShape_ReturnsItself()
    {
        var rect = Rect(0, 0);

        Assert.Same(rect, Assert.Single(ShapeTree.Leaves(rect)));
    }

    [Fact]
    public void Leaves_NestedGroups_ReturnsLeavesBottomFirst()
    {
        var a = Rect(0, 0);
        var b = Rect(1, 0);
        var c = Rect(2, 0);

        var leaves = ShapeTree.Leaves(Group(a, Group(b, c)));

        Assert.Equal([a, b, c], leaves);
    }

    [Fact]
    public void Find_ShapeInNestedGroup_ReturnsParentChain()
    {
        var leaf = Rect(0, 0);
        var inner = Group(leaf);
        var outer = Group(inner);
        var layers = Layers(Rect(50, 50), outer);

        var found = ShapeTree.Find(layers, leaf.Id);

        Assert.NotNull(found);
        Assert.Equal(0, found.Value.LayerIndex);
        Assert.Same(leaf, found.Value.Shape);
        Assert.Equal(2, found.Value.Parents.Length);
        Assert.Same(outer, found.Value.Parents[0]);
        Assert.Same(inner, found.Value.Parent);
    }

    [Fact]
    public void Find_TopLevelShape_HasNoParent()
    {
        var rect = Rect(0, 0);

        var found = ShapeTree.Find(Layers(rect), rect.Id);

        Assert.Null(found!.Value.Parent);
    }

    [Fact]
    public void Find_UnknownId_ReturnsNull()
    {
        Assert.Null(ShapeTree.Find(Layers(Rect(0, 0)), Guid.NewGuid()));
    }

    [Fact]
    public void Replace_Leaf_RebuildsAncestorBoxes()
    {
        var leaf = Rect(0, 0);
        var other = Rect(5, 5);
        var group = Group(Group(leaf), other);
        var replaced = 0;

        var result = ShapeTree.Replace([group], new Dictionary<Guid, Shape> { [leaf.Id] = leaf with { X = 100 } }, ref replaced);

        Assert.Equal(1, replaced);
        var outer = Assert.IsType<GroupShape>(Assert.Single(result));
        Assert.Equal(group.Id, outer.Id);
        Assert.Equal(new Bounds(5, 0, 105, 15), outer.Bounds);
        Assert.Equal(new Bounds(0, 0, 15, 15), group.Bounds);
    }

    [Fact]
    public void Replace_NoMatch_ReturnsSameArray()
    {
        ImmutableArray<Shape> shapes = [Group(Rect(0, 0))];
        var replaced = 0;

        var result = ShapeTree.Replace(shapes, new Dictionary<Guid, Shape> { [Guid.NewGuid()] = Rect(1, 1) }, ref replaced);

        Assert.Equal(0, replaced);
        Assert.True(result == shapes);
    }

    [Fact]
    public void Remove_OneOfTwoChildren_UpdatesGroupBox()
    {
        var a = Rect(0, 0);
        var b = Rect(50, 50);
        var matched = 0;

        var result = ShapeTree.Remove([Group(a, b)], new HashSet<Guid> { b.Id }, ref matched);

        Assert.Equal(1, matched);
        Assert.Equal(new Bounds(0, 0, 10, 10), Assert.Single(result).Bounds);
    }

    [Fact]
    public void Remove_LastChild_RemovesGroupAndEmptyAncestors()
    {
        var leaf = Rect(0, 0);
        var sibling = Rect(30, 30);
        var matched = 0;

        var result = ShapeTree.Remove([Group(Group(leaf)), sibling], new HashSet<Guid> { leaf.Id }, ref matched);

        Assert.Equal(1, matched);
        Assert.Same(sibling, Assert.Single(result));
    }

    [Fact]
    public void Remove_GroupAndItsChild_CountsBoth()
    {
        var leaf = Rect(0, 0);
        var group = Group(leaf);
        var matched = 0;

        var result = ShapeTree.Remove([group], new HashSet<Guid> { group.Id, leaf.Id }, ref matched);

        Assert.Empty(result);
        Assert.Equal(2, matched);
    }

    [Fact]
    public void Remove_UnknownId_MatchesNothing()
    {
        ImmutableArray<Shape> shapes = [Rect(0, 0)];
        var matched = 0;

        var result = ShapeTree.Remove(shapes, new HashSet<Guid> { Guid.NewGuid() }, ref matched);

        Assert.Equal(0, matched);
        Assert.True(result == shapes);
    }

    [Fact]
    public void Reidentify_NestedGroup_GivesFreshUniqueIds()
    {
        var group = Group(Rect(0, 0), Group(Rect(5, 5)));

        var copy = ShapeTree.Reidentify(group, Ids());

        var ids = new[] { copy }.Concat(ShapeTree.Leaves(copy)).Select(s => s.Id).ToList();
        var originalIds = ShapeTree.Leaves(group).Select(s => s.Id).Append(group.Id).ToHashSet();
        Assert.DoesNotContain(ids, originalIds.Contains);
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.Equal(group.Bounds, copy.Bounds);
    }

    [Fact]
    public void Translate_Group_MovesAllLeavesAndBox()
    {
        var group = Group(Rect(0, 0), Group(Rect(20, 20)));

        var moved = ShapeTree.Translate(group, 5, 7);

        Assert.Equal(new Bounds(5, 7, 30, 30), moved.Bounds);
        Assert.Equal(new Bounds(0, 0, 30, 30), group.Bounds);
        Assert.Equal(group.Id, moved.Id);
    }

    [Fact]
    public void EffectiveState_HiddenGroup_HidesChild()
    {
        var leaf = Rect(0, 0);
        var group = Group(leaf) with { IsVisible = false };

        var state = ShapeTree.EffectiveState(ShapeTree.Find(Layers(group), leaf.Id)!.Value);

        Assert.False(state.IsVisible);
        Assert.False(state.IsLocked);
    }

    [Fact]
    public void EffectiveState_LockedGroup_LocksChildButChildFlagStays()
    {
        var leaf = Rect(0, 0);
        var group = Group(leaf) with { IsLocked = true };

        var found = ShapeTree.Find(Layers(group), leaf.Id)!.Value;

        Assert.True(ShapeTree.EffectiveState(found).IsLocked);
        Assert.False(found.Shape.IsLocked);
    }

    [Fact]
    public void Depth_NestedGroups_CountsLevels()
    {
        Assert.Equal(0, ShapeTree.Depth(Rect(0, 0)));
        Assert.Equal(1, ShapeTree.Depth(Group(Rect(0, 0))));
        Assert.Equal(2, ShapeTree.Depth(Group(Rect(0, 0), Group(Rect(1, 1)))));
    }

    private static Shape Nested(int levels)
    {
        Shape shape = Rect(0, 0);
        for (var i = 0; i < levels; i++)
        {
            shape = Group(shape);
        }

        return shape;
    }

    [Fact]
    public void Walks_64Levels_Work()
    {
        var shape = Nested(ShapeTree.MaxDepth);

        Assert.Equal(ShapeTree.MaxDepth, ShapeTree.Depth(shape));
        Assert.Single(ShapeTree.Leaves(shape));
    }

    [Fact]
    public void Walks_65Levels_Throw()
    {
        var shape = Nested(ShapeTree.MaxDepth + 1);

        Assert.Throws<InvalidOperationException>(() => ShapeTree.Depth(shape));
        Assert.Throws<InvalidOperationException>(() => ShapeTree.Leaves(shape));
        Assert.Throws<InvalidOperationException>(() => ShapeTree.Translate(shape, 1, 1));
        Assert.Throws<InvalidOperationException>(() => ShapeTree.Find(Layers(shape), Guid.NewGuid()));
    }
}
