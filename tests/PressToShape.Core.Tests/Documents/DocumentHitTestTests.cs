using System;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class DocumentHitTestTests
{
    private static readonly Guid LayerA = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid LayerB = new("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid Shape1 = new("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid Shape2 = new("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid Shape3 = new("bbbbbbbb-0000-0000-0000-000000000003");

    private static Document TwoLayers() =>
        Document.Create("ignored") with { Layers = [new Layer { Id = LayerA, Name = "A" }, new Layer { Id = LayerB, Name = "B" }] };

    private static RectangleShape Rect(Guid id) => new() { Id = id, X = 0, Y = 0, Width = 100, Height = 100 };

    [Fact]
    public void HitTest_OverlappingShapesInOneLayer_ReturnsTopmostFirst()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1)).AddShape(LayerA, Rect(Shape2));

        var hits = document.HitTest(50, 50);

        Assert.Equal(new[] { Shape2, Shape1 }, hits.Select(s => s.Id));
    }

    [Fact]
    public void HitTest_ShapesInTwoLayers_UpperLayerComesBeforeLowerLayer()
    {
        var document = TwoLayers()
            .AddShape(LayerA, Rect(Shape1))
            .AddShape(LayerB, Rect(Shape2))
            .AddShape(LayerA, Rect(Shape3));

        var hits = document.HitTest(50, 50);

        Assert.Equal(new[] { Shape2, Shape3, Shape1 }, hits.Select(s => s.Id));
    }

    [Fact]
    public void HitTest_RectangleAndEllipse_EachUsesItsOwnGeometry()
    {
        var document = TwoLayers()
            .AddShape(LayerA, Rect(Shape1))
            .AddShape(LayerA, new EllipseShape { Id = Shape2, X = 0, Y = 0, Width = 100, Height = 100 });

        Assert.Equal(new[] { Shape2, Shape1 }, document.HitTest(50, 50).Select(s => s.Id));
        Assert.Equal(new[] { Shape1 }, document.HitTest(1, 1).Select(s => s.Id));
    }

    [Fact]
    public void HitTest_HiddenLayer_IsSkipped()
    {
        var document = TwoLayers()
            .AddShape(LayerA, Rect(Shape1))
            .AddShape(LayerB, Rect(Shape2))
            .UpdateLayer(LayerB, l => l with { IsVisible = false });

        var hits = document.HitTest(50, 50);

        Assert.Equal(new[] { Shape1 }, hits.Select(s => s.Id));
    }

    [Fact]
    public void HitTest_LockedLayer_IsIncluded()
    {
        var document = TwoLayers()
            .AddShape(LayerA, Rect(Shape1))
            .UpdateLayer(LayerA, l => l with { IsLocked = true });

        Assert.Equal(new[] { Shape1 }, document.HitTest(50, 50).Select(s => s.Id));
    }

    [Fact]
    public void HitTest_Miss_ReturnsEmptyListNotNull()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1));

        var hits = document.HitTest(500, 500);

        Assert.NotNull(hits);
        Assert.Empty(hits);
    }

    [Fact]
    public void HitTest_PointOnRectangleEdge_Hits()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1));

        Assert.Single(document.HitTest(100, 100));
        Assert.Single(document.HitTest(0, 50));
    }

    [Fact]
    public void HitTest_EllipseBoundingBoxCorner_Misses()
    {
        var document = TwoLayers().AddShape(LayerA, new EllipseShape { Id = Shape1, X = 0, Y = 0, Width = 100, Height = 100 });

        Assert.Empty(document.HitTest(1, 1));
    }

    [Fact]
    public void HitTest_StrokeWidth_IsIgnored()
    {
        var thick = Rect(Shape1) with { Appearance = Appearance.Default with { StrokeWidth = 50 } };
        var document = TwoLayers().AddShape(LayerA, thick);

        Assert.Empty(document.HitTest(120, 50));
    }
}
