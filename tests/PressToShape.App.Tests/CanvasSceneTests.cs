using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasSceneTests
{
    private static readonly Guid LayerA = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid LayerB = new("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid Shape1 = new("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid Shape2 = new("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid Shape3 = new("bbbbbbbb-0000-0000-0000-000000000003");

    private static Document TwoLayers() =>
        Document.Create("ignored") with { Layers = [new Layer { Id = LayerA, Name = "A" }, new Layer { Id = LayerB, Name = "B" }] };

    private static RectangleShape Rect(Guid id, double x = 0, double y = 0) =>
        new() { Id = id, X = x, Y = y, Width = 10, Height = 10 };

    [Fact]
    public void Build_NoShapes_ReturnsEmptyList()
    {
        var scene = CanvasScene.Build(TwoLayers(), 100, 0, 0);

        Assert.Empty(scene);
    }

    [Fact]
    public void Build_NullDocument_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CanvasScene.Build(null!, 100, 0, 0));
    }

    [Fact]
    public void Build_ShapesInTwoLayers_AreOrderedBottomLayerFirstThenBottomShapeFirst()
    {
        var document = TwoLayers()
            .AddShape(LayerA, Rect(Shape1, x: 1))
            .AddShape(LayerB, Rect(Shape2, x: 2))
            .AddShape(LayerA, Rect(Shape3, x: 3));

        var scene = CanvasScene.Build(document, 100, 0, 0);

        Assert.Equal(new[] { 1.0, 3.0, 2.0 }, scene.Select(s => s.X));
    }

    [Fact]
    public void Build_HiddenLayer_IsSkippedAndComesBackWhenShownAgain()
    {
        var document = TwoLayers()
            .AddShape(LayerA, Rect(Shape1, x: 1))
            .AddShape(LayerB, Rect(Shape2, x: 2));

        var hidden = document.UpdateLayer(LayerA, l => l with { IsVisible = false });
        var shown = hidden.UpdateLayer(LayerA, l => l with { IsVisible = true });

        Assert.Equal(new[] { 2.0 }, CanvasScene.Build(hidden, 100, 0, 0).Select(s => s.X));
        Assert.Equal(new[] { 1.0, 2.0 }, CanvasScene.Build(shown, 100, 0, 0).Select(s => s.X));
    }

    [Fact]
    public void Build_LockedLayer_IsDrawn()
    {
        var document = TwoLayers()
            .AddShape(LayerA, Rect(Shape1))
            .UpdateLayer(LayerA, l => l with { IsLocked = true });

        Assert.Single(CanvasScene.Build(document, 100, 0, 0));
    }

    [Fact]
    public void Build_EmptyLayerBetweenFilledLayers_DrawsOnlyTheShapes()
    {
        var document = TwoLayers().AddLayer(new Layer { Name = "Empty" }).AddShape(LayerB, Rect(Shape1));

        Assert.Single(CanvasScene.Build(document, 100, 0, 0));
    }

    [Theory]
    [InlineData(50, 10, 15, 10, 15, 5, 3)]
    [InlineData(100, 10, 15, 10, 15, 10, 6)]
    [InlineData(200, 10, 15, 10, 15, 20, 12)]
    public void Build_Zoom_ScalesPositionAndSizeFromTheOrigin(
        double zoom, double originX, double originY, double x, double y, double expectedWidth, double expectedHeight)
    {
        // Document shape at (x, y) with size 10 x 6, drawn with the artboard's top-left at (originX, originY).
        var shape = new RectangleShape { Id = Shape1, X = x, Y = y, Width = 10, Height = 6 };
        var document = TwoLayers().AddShape(LayerA, shape);

        var drawn = Assert.Single(CanvasScene.Build(document, zoom, originX, originY));

        var scale = zoom / 100;
        Assert.Equal(originX + x * scale, drawn.X);
        Assert.Equal(originY + y * scale, drawn.Y);
        Assert.Equal(expectedWidth, drawn.Width);
        Assert.Equal(expectedHeight, drawn.Height);
    }

    [Fact]
    public void Build_StrokeWidth_IsScaledWithTheZoom()
    {
        var shape = Rect(Shape1) with { Appearance = Appearance.Default with { StrokeWidth = 4 } };
        var document = TwoLayers().AddShape(LayerA, shape);

        var drawn = Assert.Single(CanvasScene.Build(document, 50, 0, 0));

        Assert.Equal(2, drawn.StrokeWidth);
        Assert.Equal(Appearance.Default.Stroke, drawn.Stroke);
    }

    [Fact]
    public void Build_NoFill_StaysNull()
    {
        var shape = Rect(Shape1) with { Appearance = Appearance.Default with { Fill = null } };
        var document = TwoLayers().AddShape(LayerA, shape);

        var drawn = Assert.Single(CanvasScene.Build(document, 100, 0, 0));

        Assert.Null(drawn.Fill);
    }

    [Fact]
    public void Build_NoStroke_GivesNullStrokeAndZeroWidth()
    {
        var shape = Rect(Shape1) with { Appearance = Appearance.Default with { Stroke = null, StrokeWidth = 3 } };
        var document = TwoLayers().AddShape(LayerA, shape);

        var drawn = Assert.Single(CanvasScene.Build(document, 100, 0, 0));

        Assert.Null(drawn.Stroke);
        Assert.Equal(0, drawn.StrokeWidth);
    }

    [Fact]
    public void Build_StrokeWidthZero_GivesNullStroke()
    {
        var shape = Rect(Shape1) with { Appearance = Appearance.Default with { StrokeWidth = 0 } };
        var document = TwoLayers().AddShape(LayerA, shape);

        var drawn = Assert.Single(CanvasScene.Build(document, 100, 0, 0));

        Assert.Null(drawn.Stroke);
        Assert.Equal(0, drawn.StrokeWidth);
    }

    [Fact]
    public void Build_OpacityAndAlpha_ArePassedThrough()
    {
        var fill = new Color(10, 20, 30, 128);
        var shape = Rect(Shape1) with { Appearance = Appearance.Default with { Fill = fill, Opacity = 0.5 } };
        var document = TwoLayers().AddShape(LayerA, shape);

        var drawn = Assert.Single(CanvasScene.Build(document, 100, 0, 0));

        Assert.Equal(fill, drawn.Fill);
        Assert.Equal(0.5, drawn.Opacity);
    }

    [Fact]
    public void Build_RectangleAndEllipse_GetTheirTypes()
    {
        var document = TwoLayers()
            .AddShape(LayerA, Rect(Shape1))
            .AddShape(LayerA, new EllipseShape { Id = Shape2, Width = 10, Height = 10 });

        var scene = CanvasScene.Build(document, 100, 0, 0);

        Assert.Equal(new[] { ScreenShapeType.Rectangle, ScreenShapeType.Ellipse }, scene.Select(s => s.Type));
    }

    [Fact]
    public void Build_ZeroSizeShape_IsKept()
    {
        var shape = new RectangleShape { Id = Shape1, X = 5, Y = 5, Width = 0, Height = 0 };
        var document = TwoLayers().AddShape(LayerA, shape);

        var drawn = Assert.Single(CanvasScene.Build(document, 200, 0, 0));

        Assert.Equal((10, 10, 0, 0), (drawn.X, drawn.Y, drawn.Width, drawn.Height));
    }

    [Fact]
    public void Build_ShapeOutsideTheArtboard_IsKept()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1, x: -500, y: 5000));

        var drawn = Assert.Single(CanvasScene.Build(document, 100, 20, 20));

        Assert.Equal((-480, 5020), (drawn.X, drawn.Y));
    }

    [Fact]
    public void Build_SameDocumentTwice_DoesNotChangeTheDocument()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1));

        _ = CanvasScene.Build(document, 200, 5, 5);

        Assert.Equal(0, document.Layers[0].Shapes[0].X);
        Assert.Equal(10, document.Layers[0].Shapes[0].Width);
    }

    [Fact]
    public void BuildShape_ShapeInDocument_EqualsTheMatchingBuildEntry()
    {
        var shape = Rect(Shape1, x: 7, y: 9) with { Appearance = Appearance.Default with { StrokeWidth = 3, Opacity = 0.5 } };
        var document = TwoLayers().AddShape(LayerA, shape);

        var viaBuild = Assert.Single(CanvasScene.Build(document, 200, 30, 40));

        Assert.Equal(viaBuild, CanvasScene.BuildShape(shape, 200, 30, 40));
    }

    [Fact]
    public void BuildShape_NullShape_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CanvasScene.BuildShape(null!, 100, 0, 0));
    }

    [Theory]
    [InlineData(50, 10, 20, 15, 30, 5, 5)]
    [InlineData(100, 10, 20, 20, 40, 10, 10)]
    [InlineData(200, 10, 20, 30, 60, 20, 20)]
    public void SelectionOutline_SelectedShape_ReturnsScreenBounds(double zoom, double originX, double originY, double x, double y, double width, double height)
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1, x: 10, y: 20));

        var outline = CanvasScene.SelectionOutline(document, Shape1, zoom, originX, originY);

        Assert.Equal(new ScreenRect(originX + 10 * zoom / 100, originY + 20 * zoom / 100, width, height), outline);
        Assert.Equal(x, outline!.Value.X, precision: 9);
        Assert.Equal(y, outline.Value.Y, precision: 9);
    }

    [Fact]
    public void SelectionOutline_ShapeInLockedLayer_IsReturned()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1)).UpdateLayer(LayerA, l => l with { IsLocked = true });

        Assert.NotNull(CanvasScene.SelectionOutline(document, Shape1, 100, 0, 0));
    }

    [Fact]
    public void SelectionOutline_NoId_ReturnsNull()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1));

        Assert.Null(CanvasScene.SelectionOutline(document, null, 100, 0, 0));
    }

    [Fact]
    public void SelectionOutline_UnknownId_ReturnsNull()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1));

        Assert.Null(CanvasScene.SelectionOutline(document, Shape2, 100, 0, 0));
    }

    [Fact]
    public void SelectionOutline_ShapeInHiddenLayer_ReturnsNull()
    {
        var document = TwoLayers().AddShape(LayerA, Rect(Shape1)).UpdateLayer(LayerA, l => l with { IsVisible = false });

        Assert.Null(CanvasScene.SelectionOutline(document, Shape1, 100, 0, 0));
    }
}
