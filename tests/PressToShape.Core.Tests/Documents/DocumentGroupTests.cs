using System;
using System.Collections.Generic;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class DocumentGroupTests
{
    private static RectangleShape Rect(double x, double y, double w = 10, double h = 10) => new() { X = x, Y = y, Width = w, Height = h };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static Document Build(Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    [Fact]
    public void FindShape_InNestedGroup_ReturnsParentAndFlags()
    {
        var leaf = Rect(0, 0);
        var group = Group(leaf) with { IsLocked = true };
        var document = Build([group]);

        var location = document.FindShape(leaf.Id);

        Assert.NotNull(location);
        Assert.Same(group, location.Parent);
        Assert.True(location.IsEffectivelyVisible);
        Assert.True(location.IsEffectivelyLocked);
        Assert.False(location.Shape.IsLocked);
    }

    [Fact]
    public void FindShape_Group_ReturnsGroupWithoutParent()
    {
        var group = Group(Rect(0, 0));

        var location = Build([group]).FindShape(group.Id);

        Assert.Same(group, location!.Shape);
        Assert.Null(location.Parent);
    }

    [Fact]
    public void FindShape_HiddenLayerOrGroup_IsNotEffectivelyVisible()
    {
        var leaf = Rect(0, 0);
        var hiddenGroup = Group(leaf) with { IsVisible = false };
        var plain = Rect(50, 50);
        var document = Build([hiddenGroup, plain]);
        var hiddenLayer = document.UpdateLayer(document.Layers[0].Id, layer => layer with { IsVisible = false, IsLocked = true });

        Assert.False(document.FindShape(leaf.Id)!.IsEffectivelyVisible);
        Assert.True(document.FindShape(plain.Id)!.IsEffectivelyVisible);
        Assert.False(hiddenLayer.FindShape(plain.Id)!.IsEffectivelyVisible);
        Assert.True(hiddenLayer.FindShape(plain.Id)!.IsEffectivelyLocked);
    }

    [Fact]
    public void UpdateShapes_Leaf_RebuildsGroupBoxes()
    {
        var leaf = Rect(0, 0);
        var group = Group(Group(leaf), Rect(5, 5));
        var document = Build([group]);

        var result = document.UpdateShapes(new Dictionary<Guid, Shape> { [leaf.Id] = leaf with { X = 100 } });

        var outer = (GroupShape)result.Layers[0].Shapes[0];
        Assert.Equal(group.Id, outer.Id);
        Assert.Equal(new Bounds(5, 0, 105, 15), outer.Bounds);
        Assert.Equal(new Bounds(0, 0, 15, 15), ((GroupShape)document.Layers[0].Shapes[0]).Bounds);
    }

    [Fact]
    public void UpdateShape_LeafInGroup_Changes()
    {
        var leaf = Rect(0, 0);
        var document = Build([Group(leaf)]);

        var result = document.UpdateShape(leaf.Id, shape => shape with { Width = 50 });

        Assert.Equal(50, result.FindShape(leaf.Id)!.Shape.Width);
        Assert.Equal(new Bounds(0, 0, 50, 10), result.Layers[0].Shapes[0].Bounds);
    }

    [Fact]
    public void UpdateShapes_GroupWithItsOwnBoxChanged_IsRejected()
    {
        var group = Group(Rect(0, 0));
        var document = Build([group]);

        Assert.Throws<ArgumentException>(() => document.UpdateShapes(new Dictionary<Guid, Shape> { [group.Id] = group with { X = 40 } }));
        Assert.Throws<ArgumentException>(() => document.UpdateShapes(new Dictionary<Guid, Shape> { [group.Id] = group with { Rotation = 30 } }));
    }

    [Fact]
    public void RemoveShapes_LastChild_RemovesGroup()
    {
        var leaf = Rect(0, 0);
        var other = Rect(80, 80);
        var document = Build([Group(Group(leaf)), other]);

        var result = document.RemoveShapes(new HashSet<Guid> { leaf.Id });

        Assert.Same(other, Assert.Single(result.Layers[0].Shapes));
    }

    [Fact]
    public void RemoveShape_OneOfTwoChildren_ShrinksGroup()
    {
        var a = Rect(0, 0);
        var b = Rect(50, 50);
        var document = Build([Group(a, b)]);

        var result = document.RemoveShape(b.Id);

        Assert.Equal(new Bounds(0, 0, 10, 10), Assert.Single(result.Layers[0].Shapes).Bounds);
        Assert.Null(result.FindShape(b.Id));
    }

    [Fact]
    public void RemoveShapes_UnknownId_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => Build([Group(Rect(0, 0))]).RemoveShapes(new HashSet<Guid> { Guid.NewGuid() }));
    }

    [Fact]
    public void HitTest_GroupMembers_ReturnsLeavesTopmostFirst()
    {
        var a = Rect(0, 0, 100, 100);
        var b = Rect(0, 0, 100, 100);
        var below = Rect(0, 0, 100, 100);
        var document = Build([below, Group(a, b)]);

        var hits = document.HitTest(5, 5);

        Assert.Equal([b, a, below], hits);
    }

    [Fact]
    public void HitTest_HiddenShapeAndHiddenGroup_AreSkipped()
    {
        var hiddenLeaf = Rect(0, 0, 100, 100) with { IsVisible = false };
        var visible = Rect(0, 0, 100, 100);
        var inHiddenGroup = Rect(0, 0, 100, 100);
        var document = Build([Group(inHiddenGroup) with { IsVisible = false }, hiddenLeaf, visible]);

        var hits = document.HitTest(5, 5);

        Assert.Same(visible, Assert.Single(hits));
    }

    [Fact]
    public void HitTest_LockedShape_IsStillReported()
    {
        var locked = Rect(0, 0, 100, 100) with { IsLocked = true };

        Assert.Same(locked, Assert.Single(Build([locked]).HitTest(5, 5)));
    }

    [Fact]
    public void AddShapes_ToGroup_AppendsAndGrowsBox()
    {
        var group = Group(Rect(0, 0));
        var added = Rect(100, 100);
        var document = Build([group]);

        var result = document.AddShapes(group.Id, [added]);

        var changed = (GroupShape)result.Layers[0].Shapes[0];
        Assert.Equal(2, changed.Children.Length);
        Assert.Same(added, changed.Children[1]);
        Assert.Equal(new Bounds(0, 0, 110, 110), changed.Bounds);
    }

    [Fact]
    public void AddShapes_UnknownContainer_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => Build([Group(Rect(0, 0))]).AddShapes(Guid.NewGuid(), [Rect(1, 1)]));
    }

    [Fact]
    public void AddShapes_ContainerIsPlainShape_Throws()
    {
        var rect = Rect(0, 0);

        Assert.Throws<KeyNotFoundException>(() => Build([rect]).AddShapes(rect.Id, [Rect(1, 1)]));
    }

    [Fact]
    public void Layers_DuplicateIdInsideGroup_IsRejected()
    {
        var leaf = Rect(0, 0);
        var document = Build([leaf]);

        Assert.Throws<ArgumentException>(() => document.AddShapes(document.Layers[0].Id, [Group(leaf)]));
    }

    [Fact]
    public void Layers_GroupWithWrongBox_IsRejected()
    {
        var layer = Document.Create("L").Layers[0] with { Shapes = [Group(Rect(0, 0)) with { X = 99 }] };

        Assert.Throws<ArgumentException>(() => Document.Create("L").AddLayer(layer));
    }

    [Fact]
    public void Layers_GroupsNested65Levels_AreRejected()
    {
        Shape deepest = Rect(0, 0);
        for (var i = 0; i < ShapeTree.MaxDepth + 1; i++)
        {
            deepest = Group(deepest);
        }

        var document = Document.Create("L");

        Assert.Throws<ArgumentException>(() => document.AddShapes(document.Layers[0].Id, [deepest]));
    }

    [Fact]
    public void Layers_GroupsNested64Levels_AreAccepted()
    {
        Shape deepest = Rect(0, 0);
        for (var i = 0; i < ShapeTree.MaxDepth; i++)
        {
            deepest = Group(deepest);
        }

        var document = Build([deepest]);

        Assert.Single(document.Layers[0].Shapes);
    }
}
