using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class LayerOperationsTests
{
    private static RectangleShape Rect(double x) => new() { X = x, Y = 0, Width = 10, Height = 10 };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static Func<Guid> Ids() => Guid.NewGuid;

    private static IEnumerable<Guid> ShapeIds(Layer layer) => layer.Shapes.Select(s => s.Id);

    private static IEnumerable<Guid> AllIds(Document document) =>
        document.Layers.SelectMany(l => new[] { l.Id }.Concat(l.Shapes.SelectMany(AllShapeIds)));

    private static IEnumerable<Guid> AllShapeIds(Shape shape) =>
        shape is GroupShape group ? new[] { group.Id }.Concat(group.Children.SelectMany(AllShapeIds)) : [shape.Id];

    // Layers A (bottom, with a, b), B (locked, with c) and C (top, hidden, with a group of d and e).
    private sealed record Fixture(Document Document, Layer A, Layer B, Layer C, Shape ShapeA, Shape ShapeB, Shape ShapeC, GroupShape Group, Shape ShapeD, Shape ShapeE);

    private static Fixture Build()
    {
        var a = Rect(0);
        var b = Rect(20);
        var c = Rect(40);
        var d = Rect(60);
        var e = Rect(80);
        var group = Group(d, e);
        var document = Document.Create("A");
        document = document.AddShapes(document.Layers[0].Id, [a, b])
            .AddLayer(new Layer { Name = "B", IsLocked = true, Shapes = [c] })
            .AddLayer(new Layer { Name = "C", IsVisible = false, Shapes = [group] });
        return new Fixture(document, document.Layers[0], document.Layers[1], document.Layers[2], a, b, c, group, d, e);
    }

    [Fact]
    public void DuplicateLayer_Layer_InsertsCopyAboveWithSameFlagsAndLook()
    {
        var f = Build();
        var newId = Guid.NewGuid();

        var result = f.Document.DuplicateLayer(f.B.Id, newId, "B copy", Ids());

        Assert.Equal(["A", "B", "B copy", "C"], result.Layers.Select(l => l.Name));
        var copy = result.Layers[2];
        Assert.Equal(newId, copy.Id);
        Assert.True(copy.IsLocked);
        Assert.True(copy.IsVisible);
        var rect = Assert.IsType<RectangleShape>(Assert.Single(copy.Shapes));
        Assert.Equal(40, rect.X);
        Assert.NotEqual(f.ShapeC.Id, rect.Id);
    }

    [Fact]
    public void DuplicateLayer_LayerWithGroup_GivesNewIdsToEveryShapeAndGroup()
    {
        var f = Build();

        var result = f.Document.DuplicateLayer(f.C.Id, Guid.NewGuid(), "C copy", Ids());

        var copyGroup = Assert.IsType<GroupShape>(Assert.Single(result.Layers[3].Shapes));
        Assert.NotEqual(f.Group.Id, copyGroup.Id);
        Assert.Equal(2, copyGroup.Children.Length);
        Assert.DoesNotContain(copyGroup.Children, child => child.Id == f.ShapeD.Id || child.Id == f.ShapeE.Id);
        Assert.Equal(f.Group.Children.Select(c => c.X), copyGroup.Children.Select(c => c.X));
        Assert.False(result.Layers[3].IsVisible);
        var all = AllIds(result).ToList();
        Assert.Equal(all.Count, all.Distinct().Count());
    }

    [Fact]
    public void DuplicateLayer_EmptyLayer_InsertsEmptyCopy()
    {
        var document = Document.Create("A");

        var result = document.DuplicateLayer(document.Layers[0].Id, Guid.NewGuid(), "A copy", Ids());

        Assert.Equal(2, result.Layers.Length);
        Assert.Empty(result.Layers[1].Shapes);
    }

    [Fact]
    public void DuplicateLayer_UnknownLayer_Throws()
    {
        var f = Build();

        Assert.Throws<KeyNotFoundException>(() => f.Document.DuplicateLayer(Guid.NewGuid(), Guid.NewGuid(), "x", Ids()));
    }

    [Fact]
    public void MergeLayerDown_TwoLayers_PutsUpperShapesOnTopOfLowerAndKeepsLowerLayer()
    {
        var f = Build();

        var result = f.Document.MergeLayerDown(f.B.Id);

        Assert.Equal(["A", "C"], result.Layers.Select(l => l.Name));
        Assert.Equal(f.A.Id, result.Layers[0].Id);
        Assert.Equal([f.ShapeA.Id, f.ShapeB.Id, f.ShapeC.Id], ShapeIds(result.Layers[0]));
        Assert.False(result.Layers[0].IsLocked);
    }

    [Fact]
    public void MergeLayerDown_UpperLayerWithGroup_KeepsGroup()
    {
        var f = Build();

        var result = f.Document.MergeLayerDown(f.C.Id);

        Assert.Equal(f.B.Id, result.Layers[1].Id);
        Assert.True(result.Layers[1].IsLocked);
        Assert.Equal([f.ShapeC.Id, f.Group.Id], ShapeIds(result.Layers[1]));
        Assert.Same(f.Group, result.Layers[1].Shapes[1]);
    }

    [Fact]
    public void MergeLayerDown_BottomLayer_Throws()
    {
        var f = Build();

        Assert.Throws<InvalidOperationException>(() => f.Document.MergeLayerDown(f.A.Id));
    }

    [Fact]
    public void MergeLayerDown_UnknownLayer_Throws()
    {
        var f = Build();

        Assert.Throws<KeyNotFoundException>(() => f.Document.MergeLayerDown(Guid.NewGuid()));
    }

    [Fact]
    public void MoveShapesToLayer_OneShape_GoesToTopOfTargetLayer()
    {
        var f = Build();

        var result = f.Document.MoveShapesToLayer(new HashSet<Guid> { f.ShapeA.Id }, f.B.Id);

        Assert.Equal([f.ShapeB.Id], ShapeIds(result.Layers[0]));
        Assert.Equal([f.ShapeC.Id, f.ShapeA.Id], ShapeIds(result.Layers[1]));
        Assert.Same(f.ShapeA, result.Layers[1].Shapes[1]);
    }

    [Fact]
    public void MoveShapesToLayer_ShapesFromTwoLayers_KeepDocumentOrder()
    {
        var f = Build();

        var result = f.Document.MoveShapesToLayer(new HashSet<Guid> { f.ShapeC.Id, f.ShapeB.Id }, f.A.Id);

        Assert.Equal([f.ShapeA.Id, f.ShapeB.Id, f.ShapeC.Id], ShapeIds(result.Layers[0]));
        Assert.Empty(result.Layers[1].Shapes);
    }

    [Fact]
    public void MoveShapesToLayer_MemberOfGroup_LeavesGroupAndGroupStays()
    {
        var f = Build();

        var result = f.Document.MoveShapesToLayer(new HashSet<Guid> { f.ShapeD.Id }, f.A.Id);

        Assert.Equal([f.ShapeA.Id, f.ShapeB.Id, f.ShapeD.Id], ShapeIds(result.Layers[0]));
        var group = Assert.IsType<GroupShape>(Assert.Single(result.Layers[2].Shapes));
        Assert.Equal([f.ShapeE.Id], group.Children.Select(c => c.Id));
    }

    [Fact]
    public void MoveShapesToLayer_AllMembersOfGroup_RemovesEmptyGroup()
    {
        var f = Build();

        var result = f.Document.MoveShapesToLayer(new HashSet<Guid> { f.ShapeD.Id, f.ShapeE.Id }, f.A.Id);

        Assert.Empty(result.Layers[2].Shapes);
        Assert.Equal([f.ShapeA.Id, f.ShapeB.Id, f.ShapeD.Id, f.ShapeE.Id], ShapeIds(result.Layers[0]));
    }

    [Fact]
    public void MoveShapesToLayer_GroupAndItsMember_MovesGroupOnce()
    {
        var f = Build();

        var result = f.Document.MoveShapesToLayer(new HashSet<Guid> { f.Group.Id, f.ShapeD.Id }, f.A.Id);

        Assert.Equal([f.ShapeA.Id, f.ShapeB.Id, f.Group.Id], ShapeIds(result.Layers[0]));
        var all = AllIds(result).ToList();
        Assert.Equal(all.Count, all.Distinct().Count());
    }

    [Fact]
    public void MoveShapesToLayer_IntoOwnLayer_GoesToTop()
    {
        var f = Build();

        var result = f.Document.MoveShapesToLayer(new HashSet<Guid> { f.ShapeA.Id }, f.A.Id);

        Assert.Equal([f.ShapeB.Id, f.ShapeA.Id], ShapeIds(result.Layers[0]));
    }

    [Fact]
    public void MoveShapesToLayer_NoIds_ReturnsSameDocument()
    {
        var f = Build();

        Assert.Same(f.Document, f.Document.MoveShapesToLayer(new HashSet<Guid>(), f.A.Id));
    }

    [Fact]
    public void MoveShapesToLayer_UnknownIds_Throw()
    {
        var f = Build();

        Assert.Throws<KeyNotFoundException>(() => f.Document.MoveShapesToLayer(new HashSet<Guid> { Guid.NewGuid() }, f.A.Id));
        Assert.Throws<KeyNotFoundException>(() => f.Document.MoveShapesToLayer(new HashSet<Guid> { f.ShapeA.Id }, Guid.NewGuid()));
    }

    [Fact]
    public void LayerOperations_Applied_LeaveOriginalDocumentUnchanged()
    {
        var f = Build();

        _ = f.Document.DuplicateLayer(f.B.Id, Guid.NewGuid(), "x", Ids());
        _ = f.Document.MergeLayerDown(f.B.Id);
        _ = f.Document.MoveShapesToLayer(new HashSet<Guid> { f.ShapeA.Id }, f.B.Id);

        Assert.Equal(3, f.Document.Layers.Length);
        Assert.Equal([f.ShapeA.Id, f.ShapeB.Id], ShapeIds(f.Document.Layers[0]));
    }
}
