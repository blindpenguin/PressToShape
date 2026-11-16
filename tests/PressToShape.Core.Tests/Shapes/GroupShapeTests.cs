using System;
using System.Collections.Immutable;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class GroupShapeTests
{
    private static RectangleShape Rect(double x, double y, double w, double h, double rotation = 0) =>
        new() { X = x, Y = y, Width = w, Height = h, Rotation = rotation };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    [Fact]
    public void Bounds_OneChild_EqualsChildBounds()
    {
        var group = Group(Rect(10, 20, 100, 50));

        Assert.Equal(new Bounds(10, 20, 100, 50), group.Bounds);
        Assert.Equal(0, group.Rotation);
    }

    [Fact]
    public void Bounds_TwoChildren_IsUnion()
    {
        var group = Group(Rect(0, 0, 10, 10), Rect(20, 30, 10, 10));

        Assert.Equal(new Bounds(0, 0, 30, 40), group.Bounds);
    }

    [Fact]
    public void Bounds_RotatedChild_UsesRotatedBounds()
    {
        var child = Rect(0, 0, 100, 20, rotation: 90);

        var group = Group(child);

        Assert.Equal(child.RotatedBounds.X, group.X, 6);
        Assert.Equal(child.RotatedBounds.Y, group.Y, 6);
        Assert.Equal(20, group.Width, 6);
        Assert.Equal(100, group.Height, 6);
    }

    [Fact]
    public void Bounds_NestedGroups_IncludesInnerGroup()
    {
        var inner = Group(Rect(50, 50, 10, 10));

        var outer = Group(Rect(0, 0, 10, 10), inner);

        Assert.Equal(new Bounds(0, 0, 60, 60), outer.Bounds);
    }

    [Fact]
    public void Children_AnyOrder_KeepsOrder()
    {
        var a = Rect(0, 0, 1, 1);
        var b = Rect(5, 5, 1, 1);

        var group = Group(b, a);

        Assert.Same(b, group.Children[0]);
        Assert.Same(a, group.Children[1]);
    }

    [Fact]
    public void Children_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new GroupShape { Children = ImmutableArray<Shape>.Empty });
    }

    [Fact]
    public void Children_DefaultArray_Throws()
    {
        Assert.Throws<ArgumentException>(() => new GroupShape { Children = default });
    }

    [Fact]
    public void Children_ContainsNull_Throws()
    {
        Assert.Throws<ArgumentException>(() => new GroupShape { Children = [Rect(0, 0, 1, 1), null!] });
    }

    [Fact]
    public void With_NewChildren_RecomputesBox()
    {
        var group = Group(Rect(0, 0, 10, 10));

        var changed = group with { Children = [Rect(100, 100, 5, 5)] };

        Assert.Equal(new Bounds(100, 100, 5, 5), changed.Bounds);
        Assert.Equal(new Bounds(0, 0, 10, 10), group.Bounds);
    }

    [Fact]
    public void Name_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Rect(0, 0, 1, 1) with { Name = null! });
    }

    [Fact]
    public void Flags_NewShape_VisibleUnlockedWithoutName()
    {
        var shape = Rect(0, 0, 1, 1);

        Assert.True(shape.IsVisible);
        Assert.False(shape.IsLocked);
        Assert.Equal(string.Empty, shape.Name);
    }

    [Fact]
    public void Contains_PointOnChild_True()
    {
        var group = Group(Rect(0, 0, 10, 10), Rect(50, 50, 10, 10));

        Assert.True(group.Contains(55, 55));
        Assert.False(group.Contains(30, 30));
    }

    [Fact]
    public void Contains_PointOnHiddenChild_False()
    {
        var group = Group(Rect(0, 0, 10, 10) with { IsVisible = false }, Rect(50, 50, 10, 10));

        Assert.False(group.Contains(5, 5));
        Assert.True(group.Contains(55, 55));
    }

    [Fact]
    public void Constructor_NoId_GetsUniqueIds()
    {
        Assert.NotEqual(Group(Rect(0, 0, 1, 1)).Id, Group(Rect(0, 0, 1, 1)).Id);
    }

    [Fact]
    public void MovedCopy_Group_GivesNewIdsAndMovesAllLeaves()
    {
        var group = Group(Rect(0, 0, 10, 10), Group(Rect(20, 20, 10, 10)));
        var next = 1;

        var copy = group.MovedCopy(() => new Guid(next++, 0, 0, new byte[8]), 5, 7);

        Assert.Equal(new Bounds(5, 7, 30, 30), copy.Bounds);
        Assert.Equal(2, copy.Leaves().Count);
        Assert.NotEqual(group.Id, copy.Id);
        Assert.Equal(new Bounds(0, 0, 30, 30), group.Bounds);
    }

    [Fact]
    public void Leaves_PlainShapeAndNestedGroups_ReturnTheNonGroupShapes()
    {
        var a = Rect(0, 0, 1, 1);
        var b = Rect(5, 5, 1, 1);

        Assert.Same(a, Assert.Single(a.Leaves()));
        Assert.Equal([a, b], Group(a, Group(b)).Leaves());
    }
}
