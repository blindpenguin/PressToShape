using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class GroupOperationsTests
{
    private static readonly Guid NewGroup = new("99999999-9999-9999-9999-999999999999");

    private static RectangleShape Rect(double x, double y = 0) => new() { X = x, Y = y, Width = 10, Height = 10 };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static IEnumerable<Guid> Ids(IEnumerable<Shape> shapes) => shapes.Select(s => s.Id);

    [Fact]
    public void Group_TwoOfThree_ReplacesAtTopmostPositionKeepingOrder()
    {
        var a = Rect(0);
        var b = Rect(20);
        var c = Rect(40);
        var document = Build(a, b, c);

        var result = document.Group([c.Id, a.Id], NewGroup);

        var shapes = result.Layers[0].Shapes;
        Assert.Equal([b.Id, NewGroup], Ids(shapes));
        var group = Assert.IsType<GroupShape>(shapes[1]);
        Assert.Equal([a.Id, c.Id], Ids(group.Children));
        Assert.Equal(new Bounds(0, 0, 50, 10), group.Bounds);
    }

    [Fact]
    public void Group_TopMembersFirst_GroupStaysOnTop()
    {
        var a = Rect(0);
        var b = Rect(20);
        var c = Rect(40);

        var result = Build(a, b, c).Group([b.Id, c.Id], NewGroup);

        Assert.Equal([a.Id, NewGroup], Ids(result.Layers[0].Shapes));
    }

    [Fact]
    public void Group_GroupAndShape_NestsGroup()
    {
        var inner = Group(Rect(0), Rect(20));
        var plain = Rect(40);

        var result = Build(inner, plain).Group([inner.Id, plain.Id], NewGroup);

        var outer = Assert.IsType<GroupShape>(Assert.Single(result.Layers[0].Shapes));
        Assert.Equal([inner.Id, plain.Id], Ids(outer.Children));
        Assert.Equal(2, outer.Children.OfType<GroupShape>().Single().Children.Length);
    }

    [Fact]
    public void Group_MembersOfOneGroup_GroupsInsideThatGroup()
    {
        var a = Rect(0);
        var b = Rect(20);
        var c = Rect(40);
        var parent = Group(a, b, c);

        var result = Build(parent).Group([a.Id, b.Id], NewGroup);

        var outer = Assert.IsType<GroupShape>(Assert.Single(result.Layers[0].Shapes));
        Assert.Equal(parent.Id, outer.Id);
        Assert.Equal([NewGroup, c.Id], Ids(outer.Children));
        Assert.Equal(parent.Bounds, outer.Bounds);
    }

    [Fact]
    public void Group_DifferentLayers_Throws()
    {
        var a = Rect(0);
        var b = Rect(20);
        var document = Build(a);
        document = document.AddLayer(new Layer { Name = "Two", Shapes = [b] });

        Assert.Throws<InvalidOperationException>(() => document.Group([a.Id, b.Id], NewGroup));
    }

    [Fact]
    public void Group_DifferentParents_Throws()
    {
        var a = Rect(0);
        var b = Rect(20);
        var document = Build(Group(a), b);

        Assert.Throws<InvalidOperationException>(() => document.Group([a.Id, b.Id], NewGroup));
    }

    [Fact]
    public void Group_OneIdOrRepeatedId_Throws()
    {
        var a = Rect(0);
        var document = Build(a, Rect(20));

        Assert.Throws<ArgumentException>(() => document.Group([a.Id], NewGroup));
        Assert.Throws<ArgumentException>(() => document.Group([a.Id, a.Id], NewGroup));
        Assert.Throws<ArgumentException>(() => document.Group([], NewGroup));
    }

    [Fact]
    public void Group_UnknownId_Throws()
    {
        var a = Rect(0);

        Assert.Throws<KeyNotFoundException>(() => Build(a).Group([a.Id, Guid.NewGuid()], NewGroup));
    }

    [Fact]
    public void Group_GroupIdAlreadyUsed_Throws()
    {
        var a = Rect(0);
        var b = Rect(20);

        Assert.Throws<ArgumentException>(() => Build(a, b).Group([a.Id, b.Id], a.Id));
    }

    [Fact]
    public void Group_Result_LeavesOriginalUnchanged()
    {
        var a = Rect(0);
        var b = Rect(20);
        var document = Build(a, b);

        _ = document.Group([a.Id, b.Id], NewGroup);

        Assert.Equal([a.Id, b.Id], Ids(document.Layers[0].Shapes));
    }

    [Fact]
    public void Ungroup_Group_ReplacesByChildrenInPlace()
    {
        var below = Rect(-20);
        var a = Rect(0);
        var b = Rect(20);
        var above = Rect(60);
        var group = Group(a, b);

        var result = Build(below, group, above).Ungroup(new HashSet<Guid> { group.Id });

        Assert.Equal([below.Id, a.Id, b.Id, above.Id], Ids(result.Layers[0].Shapes));
        Assert.Equal(a, result.FindShape(a.Id)!.Shape);
    }

    [Fact]
    public void Ungroup_ChildrenKeepTheirOwnFlagsAndGroupFlagsAreDropped()
    {
        var a = Rect(0) with { IsLocked = true, Name = "A" };
        var group = Group(a) with { IsVisible = false, Name = "G" };

        var result = Build(group).Ungroup(new HashSet<Guid> { group.Id });

        var shape = Assert.Single(result.Layers[0].Shapes);
        Assert.True(shape.IsLocked);
        Assert.True(shape.IsVisible);
        Assert.Equal("A", shape.Name);
    }

    [Fact]
    public void Ungroup_SeveralGroups_UngroupsEach()
    {
        var g1 = Group(Rect(0));
        var g2 = Group(Rect(20));

        var result = Build(g1, g2).Ungroup(new HashSet<Guid> { g1.Id, g2.Id });

        Assert.All(result.Layers[0].Shapes, shape => Assert.IsNotType<GroupShape>(shape));
        Assert.Equal(2, result.Layers[0].Shapes.Length);
    }

    [Fact]
    public void Ungroup_OuterGroup_KeepsInnerGroup()
    {
        var inner = Group(Rect(0));
        var outer = Group(inner, Rect(20));

        var result = Build(outer).Ungroup(new HashSet<Guid> { outer.Id });

        Assert.Same(inner, result.Layers[0].Shapes[0]);
        Assert.Equal(2, result.Layers[0].Shapes.Length);
    }

    [Fact]
    public void Ungroup_InnerGroup_StaysInsideOuter()
    {
        var a = Rect(0);
        var inner = Group(a, Rect(20));
        var outer = Group(inner, Rect(40));

        var result = Build(outer).Ungroup(new HashSet<Guid> { inner.Id });

        var changed = Assert.IsType<GroupShape>(Assert.Single(result.Layers[0].Shapes));
        Assert.Equal(outer.Id, changed.Id);
        Assert.Equal(3, changed.Children.Length);
        Assert.Equal(outer.Bounds, changed.Bounds);
    }

    [Fact]
    public void Ungroup_PlainShape_Throws()
    {
        var a = Rect(0);

        Assert.Throws<ArgumentException>(() => Build(a).Ungroup(new HashSet<Guid> { a.Id }));
    }

    [Fact]
    public void Ungroup_UnknownId_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => Build(Rect(0)).Ungroup(new HashSet<Guid> { Guid.NewGuid() }));
    }

    [Fact]
    public void Ungroup_NoIds_ReturnsSameDocument()
    {
        var document = Build(Group(Rect(0)));

        Assert.Same(document, document.Ungroup(new HashSet<Guid>()));
    }

    [Fact]
    public void Ungroup_AfterGroup_MembersMoveToTheTopmostPosition()
    {
        var a = Rect(0);
        var b = Rect(20);
        var c = Rect(40);

        var result = Build(a, b, c).Group([a.Id, c.Id], NewGroup).Ungroup(new HashSet<Guid> { NewGroup });

        Assert.Equal([b.Id, a.Id, c.Id], Ids(result.Layers[0].Shapes));
    }
}
