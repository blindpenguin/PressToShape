using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class DocumentShapeTests
{
    private static readonly Guid LayerA = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid LayerB = new("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid Shape1 = new("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid Shape2 = new("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid Shape3 = new("bbbbbbbb-0000-0000-0000-000000000003");

    private static Document TwoLayers() =>
        Document.Create("ignored") with { Layers = [new Layer { Id = LayerA, Name = "A" }, new Layer { Id = LayerB, Name = "B" }] };

    [Fact]
    public void AddShape_TwoShapes_AreStackedBottomFirstInTheRightLayer()
    {
        var document = TwoLayers()
            .AddShape(LayerA, new RectangleShape { Id = Shape1 })
            .AddShape(LayerA, new EllipseShape { Id = Shape2 })
            .AddShape(LayerB, new RectangleShape { Id = Shape3 });

        Assert.Equal(new[] { Shape1, Shape2 }, document.Layers[0].Shapes.Select(s => s.Id));
        Assert.Equal(new[] { Shape3 }, document.Layers[1].Shapes.Select(s => s.Id));
    }

    [Fact]
    public void AddShape_Shape_LeavesOriginalUnchangedAndSharesOtherLayers()
    {
        var original = TwoLayers();

        var changed = original.AddShape(LayerA, new RectangleShape { Id = Shape1 });

        Assert.Empty(original.Layers[0].Shapes);
        Assert.Single(changed.Layers[0].Shapes);
        Assert.Same(original.Layers[1], changed.Layers[1]);
    }

    [Fact]
    public void AddShape_DuplicateIdInOtherLayer_ThrowsAndLeavesDocumentUnchanged()
    {
        var document = TwoLayers().AddShape(LayerA, new RectangleShape { Id = Shape1 });

        Assert.Throws<ArgumentException>(() => document.AddShape(LayerB, new EllipseShape { Id = Shape1 }));

        Assert.Empty(document.Layers[1].Shapes);
    }

    [Fact]
    public void AddShape_UnknownLayer_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => TwoLayers().AddShape(Guid.NewGuid(), new RectangleShape()));
    }

    [Fact]
    public void AddShape_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => TwoLayers().AddShape(LayerA, null!));
    }

    [Fact]
    public void UpdateShape_GeometryAndAppearance_ReplacesShapeAndKeepsItsPlace()
    {
        var original = TwoLayers()
            .AddShape(LayerA, new RectangleShape { Id = Shape1, Width = 10 })
            .AddShape(LayerA, new RectangleShape { Id = Shape2 });

        var changed = original.UpdateShape(Shape1, s => s with { X = 5, Width = 20, Appearance = s.Appearance with { Opacity = 0.5 } });

        var updated = changed.Layers[0].Shapes[0];
        Assert.Equal(Shape1, updated.Id);
        Assert.Equal((5, 20, 0.5), (updated.X, updated.Width, updated.Appearance.Opacity));
        Assert.Equal(Shape2, changed.Layers[0].Shapes[1].Id);
        Assert.Equal((0, 10, 1), (original.Layers[0].Shapes[0].X, original.Layers[0].Shapes[0].Width, original.Layers[0].Shapes[0].Appearance.Opacity));
    }

    [Fact]
    public void UpdateShape_ChangeToOtherShapeType_IsAllowedWithSameId()
    {
        var document = TwoLayers().AddShape(LayerA, new RectangleShape { Id = Shape1 });

        var changed = document.UpdateShape(Shape1, s => new EllipseShape { Id = s.Id });

        Assert.IsType<EllipseShape>(changed.Layers[0].Shapes[0]);
    }

    [Fact]
    public void UpdateShape_UnknownId_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => TwoLayers().UpdateShape(Guid.NewGuid(), s => s));
    }

    [Fact]
    public void UpdateShape_ChangeReturnsOtherId_Throws()
    {
        var document = TwoLayers().AddShape(LayerA, new RectangleShape { Id = Shape1 });

        Assert.Throws<InvalidOperationException>(() => document.UpdateShape(Shape1, s => s with { Id = Shape2 }));
    }

    [Fact]
    public void UpdateShape_ChangeReturnsNull_Throws()
    {
        var document = TwoLayers().AddShape(LayerA, new RectangleShape { Id = Shape1 });

        Assert.Throws<InvalidOperationException>(() => document.UpdateShape(Shape1, _ => null!));
    }

    [Fact]
    public void UpdateShape_NullChange_Throws()
    {
        var document = TwoLayers().AddShape(LayerA, new RectangleShape { Id = Shape1 });

        Assert.Throws<ArgumentNullException>(() => document.UpdateShape(Shape1, null!));
    }

    [Fact]
    public void RemoveShape_ExistingShape_RemovesOnlyThatShape()
    {
        var original = TwoLayers()
            .AddShape(LayerA, new RectangleShape { Id = Shape1 })
            .AddShape(LayerA, new RectangleShape { Id = Shape2 });

        var changed = original.RemoveShape(Shape1);

        Assert.Equal(new[] { Shape2 }, changed.Layers[0].Shapes.Select(s => s.Id));
        Assert.Equal(2, original.Layers[0].Shapes.Length);
    }

    [Fact]
    public void RemoveShape_UnknownId_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => TwoLayers().RemoveShape(Guid.NewGuid()));
    }

    [Fact]
    public void FindShape_ExistingShape_ReturnsShapeAndItsLayer()
    {
        var shape = new EllipseShape { Id = Shape1 };
        var document = TwoLayers().AddShape(LayerB, shape);

        var location = document.FindShape(Shape1);

        Assert.NotNull(location);
        Assert.Equal(shape, location.Shape);
        Assert.Equal(LayerB, location.Layer.Id);
    }

    [Fact]
    public void FindShape_UnknownId_ReturnsNullWithoutThrowing()
    {
        Assert.Null(TwoLayers().FindShape(Guid.NewGuid()));
    }

    [Fact]
    public void FindShape_AfterRemove_ReturnsNull()
    {
        var document = TwoLayers().AddShape(LayerA, new RectangleShape { Id = Shape1 }).RemoveShape(Shape1);

        Assert.Null(document.FindShape(Shape1));
    }
}
