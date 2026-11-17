using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasScenePathTests
{
    private static readonly Guid LayerId = new("aaaaaaaa-0000-0000-0000-000000000001");

    private static PathShape Curve(bool closed = false) =>
        PathShape.Create(
            [new DocumentNode(100, 50, outX: 100, outY: 90), new DocumentNode(200, 150, inX: 140, inY: 150)],
            closed,
            FillRule.EvenOdd);

    private static Document DocumentWith(params Shape[] shapes)
    {
        var document = Document.Create("ignored") with { Layers = [new Layer { Id = LayerId, Name = "A" }] };
        return document.AddShapes(LayerId, shapes);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    public void BuildShape_Path_PutsTheBoxAndTheNodesInScreenPixels(double zoom)
    {
        var path = Curve();
        var scale = zoom / 100;

        var screen = CanvasScene.BuildShape(path, zoom, 10, 20);

        Assert.Equal(ScreenShapeType.Path, screen.Type);
        Assert.Equal(10 + 100 * scale, screen.X, 9);
        Assert.Equal(20 + 50 * scale, screen.Y, 9);
        Assert.Equal(100 * scale, screen.Width, 9);
        Assert.Equal(100 * scale, screen.Height, 9);
        var outline = screen.Path!;
        Assert.Equal(2, outline.SubPaths[0].Nodes.Length);
        // The first node sits at the box's top left, the second at its bottom right, both relative to the box.
        Assert.Equal(0, outline.SubPaths[0].Nodes[0].X, 9);
        Assert.Equal(0, outline.SubPaths[0].Nodes[0].Y, 9);
        Assert.Equal(100 * scale, outline.SubPaths[0].Nodes[1].X, 9);
        Assert.Equal(100 * scale, outline.SubPaths[0].Nodes[1].Y, 9);
        Assert.Equal(0, outline.SubPaths[0].Nodes[0].OutX!.Value, 9);
        Assert.Equal(40 * scale, outline.SubPaths[0].Nodes[0].OutY!.Value, 9);
        Assert.Equal(40 * scale, outline.SubPaths[0].Nodes[1].InX!.Value, 9);
        Assert.Equal(100 * scale, outline.SubPaths[0].Nodes[1].InY!.Value, 9);
        Assert.False(outline.SubPaths[0].Nodes[0].HasIn);
        Assert.True(outline.SubPaths[0].Nodes[0].HasOut);
        Assert.True(outline.SubPaths[0].Nodes[1].HasIn);
        Assert.False(outline.SubPaths[0].Nodes[1].HasOut);
    }

    [Fact]
    public void BuildShape_ClosedPath_CarriesTheClosedFlagAndTheFillRule()
    {
        var open = CanvasScene.BuildShape(Curve(), 100, 0, 0);
        var closed = CanvasScene.BuildShape(Curve(closed: true), 100, 0, 0);

        Assert.False(open.Path!.SubPaths[0].IsClosed);
        Assert.True(closed.Path!.SubPaths[0].IsClosed);
        Assert.Equal(FillRule.EvenOdd, closed.Path.FillRule);
    }

    [Fact]
    public void BuildShape_RotatedPathWithAStroke_CarriesRotationAndAppearance()
    {
        var path = Curve() with
        {
            Rotation = 25,
            Appearance = new Appearance { Fill = null, Stroke = Color.FromRgb(1, 2, 3), StrokeWidth = 4, Opacity = 0.5 },
        };

        var screen = CanvasScene.BuildShape(path, 200, 0, 0);

        Assert.Equal(25, screen.RotationDegrees);
        Assert.Null(screen.Fill);
        Assert.Equal(Color.FromRgb(1, 2, 3), screen.Stroke);
        Assert.Equal(8, screen.StrokeWidth, 9);
        Assert.Equal(0.5, screen.Opacity);
    }

    [Fact]
    public void BuildShape_StraightHorizontalPath_HasZeroHeightAndFiniteNodes()
    {
        var line = PathShape.Create([new DocumentNode(0, 30), new DocumentNode(80, 30)]);

        var screen = CanvasScene.BuildShape(line, 100, 0, 0);

        Assert.Equal(0, screen.Height);
        Assert.All(screen.Path!.SubPaths[0].Nodes, node => Assert.True(double.IsFinite(node.X) && double.IsFinite(node.Y)));
    }

    [Fact]
    public void BuildShape_RectangleAndEllipse_HaveNoPathData()
    {
        Assert.Null(CanvasScene.BuildShape(new RectangleShape { Width = 1, Height = 1 }, 100, 0, 0).Path);
        Assert.Null(CanvasScene.BuildShape(new EllipseShape { Width = 1, Height = 1 }, 100, 0, 0).Path);
    }

    [Fact]
    public void Build_PathInAGroup_IsFlattenedLikeOtherShapes()
    {
        var group = new GroupShape { Children = [new RectangleShape { Width = 5, Height = 5 }, Curve()] };

        var scene = CanvasScene.Build(DocumentWith(group), 100, 0, 0);

        Assert.Equal([ScreenShapeType.Rectangle, ScreenShapeType.Path], scene.Select(s => s.Type));
    }

    [Fact]
    public void Build_HiddenPath_IsLeftOut()
    {
        var scene = CanvasScene.Build(DocumentWith(Curve() with { IsVisible = false }), 100, 0, 0);

        Assert.Empty(scene);
    }
}
