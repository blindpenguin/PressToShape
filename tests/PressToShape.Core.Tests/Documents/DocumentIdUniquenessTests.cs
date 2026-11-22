using System;
using System.Collections.Immutable;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

// Layers and shapes share one id space: the Guid of a layer is never the Guid of a shape, at any depth.
public class DocumentIdUniquenessTests
{
    private static readonly Guid Shared = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static RectangleShape Rect(Guid? id = null) => new() { Id = id ?? Guid.NewGuid(), Width = 10, Height = 10 };

    private static Layer LayerOf(Guid? id = null, params Shape[] shapes) =>
        new() { Id = id ?? Guid.NewGuid(), Name = "L", Shapes = [.. shapes] };

    private static Document DocumentOf(params Layer[] layers) => Document.Create("first") with { Layers = [.. layers] };

    [Fact]
    public void Layers_LayerAndItsOwnShapeShareAnId_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => DocumentOf(LayerOf(Shared, Rect(Shared))));

        Assert.Contains(Shared.ToString(), ex.Message);
    }

    [Fact]
    public void Layers_ShapeOfAnEarlierLayerHasTheIdOfALaterLayer_Throws()
    {
        Assert.Throws<ArgumentException>(() => DocumentOf(LayerOf(null, Rect(Shared)), LayerOf(Shared)));
    }

    [Fact]
    public void Layers_ShapeOfALaterLayerHasTheIdOfAnEarlierLayer_Throws()
    {
        Assert.Throws<ArgumentException>(() => DocumentOf(LayerOf(Shared), LayerOf(null, Rect(Shared))));
    }

    [Fact]
    public void Layers_ShapeInANestedGroupHasTheIdOfALayer_Throws()
    {
        var inner = new GroupShape { Children = [Rect(Shared)] };
        var outer = new GroupShape { Children = [inner] };

        Assert.Throws<ArgumentException>(() => DocumentOf(LayerOf(Shared, outer)));
    }

    [Fact]
    public void Layers_GroupHasTheIdOfALayer_Throws()
    {
        var group = new GroupShape { Id = Shared, Children = [Rect()] };

        Assert.Throws<ArgumentException>(() => DocumentOf(LayerOf(null, group), LayerOf(Shared)));
    }

    [Fact]
    public void Layers_TwoLayersShareAnId_ThrowsWithTheLayerMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() => DocumentOf(LayerOf(Shared), LayerOf(Shared)));

        Assert.Contains("Layer id", ex.Message);
    }

    [Fact]
    public void Layers_TwoShapesShareAnId_ThrowsWithTheShapeMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() => DocumentOf(LayerOf(null, Rect(Shared), Rect(Shared))));

        Assert.Contains("Shape id", ex.Message);
    }

    [Fact]
    public void Layers_AllIdsDifferent_AreAccepted()
    {
        var document = DocumentOf(LayerOf(null, Rect(), new GroupShape { Children = [Rect()] }), LayerOf(null, Rect()));

        Assert.Equal(2, document.Layers.Length);
    }

    [Fact]
    public void Guides_GuideHasTheIdOfAShape_IsStillAccepted()
    {
        var shape = Rect(Shared);
        var document = DocumentOf(LayerOf(null, shape));

        var withGuide = document.AddGuide(new Guide { Id = Shared, Orientation = GuideOrientation.Vertical, Position = 5 });

        Assert.Single(withGuide.Guides);
    }

    [Fact]
    public void AddShape_IdOfALayer_Throws()
    {
        var document = Document.Create("L");

        Assert.Throws<ArgumentException>(() => document.AddShape(document.Layers[0].Id, Rect(document.Layers[0].Id)));
    }

    [Fact]
    public void AddLayer_IdOfAShape_Throws()
    {
        var shape = Rect();
        var document = Document.Create("L");
        document = document.AddShape(document.Layers[0].Id, shape);

        Assert.Throws<ArgumentException>(() => document.AddLayer(LayerOf(shape.Id)));
    }

    [Fact]
    public void AddLayer_ShapeWithTheIdOfAnExistingLayer_Throws()
    {
        var document = Document.Create("L");

        Assert.Throws<ArgumentException>(() => document.AddLayer(LayerOf(null, Rect(document.Layers[0].Id))));
    }

    [Fact]
    public void InsertLayer_IdOfAShape_Throws()
    {
        var shape = Rect();
        var document = Document.Create("L");
        document = document.AddShape(document.Layers[0].Id, shape);

        Assert.Throws<ArgumentException>(() => document.InsertLayer(0, LayerOf(shape.Id)));
    }

    [Fact]
    public void DuplicateLayer_NewLayerIdIsTheIdOfAShape_Throws()
    {
        var shape = Rect();
        var document = Document.Create("L");
        document = document.AddShape(document.Layers[0].Id, shape);

        Assert.Throws<ArgumentException>(() => document.DuplicateLayer(document.Layers[0].Id, shape.Id, "Copy", Guid.NewGuid));
    }

    [Fact]
    public void DuplicateLayer_NewShapeIdIsTheIdOfALayer_Throws()
    {
        var document = Document.Create("L");
        document = document.AddShape(document.Layers[0].Id, Rect());
        var layerId = document.Layers[0].Id;

        Assert.Throws<ArgumentException>(() => document.DuplicateLayer(layerId, Guid.NewGuid(), "Copy", () => layerId));
    }

    [Fact]
    public void MoveShapesToLayer_AllIdsDifferent_StillWorks()
    {
        var shape = Rect();
        var document = Document.Create("L").AddLayer(LayerOf());
        document = document.AddShape(document.Layers[0].Id, shape);

        var moved = document.MoveShapesToLayer(ImmutableHashSet.Create(shape.Id), document.Layers[1].Id);

        Assert.Single(moved.Layers[1].Shapes);
    }
}
