using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathShapeTests
{
    private static DocumentNode Corner(double x, double y) => new(x, y);

    private static PathShape Triangle(bool closed = true) =>
        PathShape.Create([Corner(10, 20), Corner(110, 20), Corner(60, 120)], closed);

    [Fact]
    public void Create_Line_BoxIsTheBoxOfTheEndPoints()
    {
        var path = PathShape.Create([Corner(10, 20), Corner(40, 5)]);

        Assert.Equal(new Bounds(10, 5, 30, 15), path.Bounds);
        Assert.False(path.SubPaths[0].IsClosed);
        Assert.Equal(FillRule.NonZero, path.FillRule);
    }

    [Fact]
    public void Create_Triangle_StoresNodesAsFractionsOfTheBox()
    {
        var path = Triangle();

        Assert.Equal(new Bounds(10, 20, 100, 100), path.Bounds);
        Assert.Equal(new PathNode(0, 0), path.SubPaths[0].Nodes[0]);
        Assert.Equal(new PathNode(1, 0), path.SubPaths[0].Nodes[1]);
        Assert.Equal(new PathNode(0.5, 1), path.SubPaths[0].Nodes[2]);
        Assert.True(path.SubPaths[0].IsClosed);
    }

    [Fact]
    public void Create_CurveThatOvershootsItsAnchors_BoxReachesTheCurveNotTheHandles()
    {
        var path = PathShape.Create([new DocumentNode(0, 0, outX: -100, outY: 30), new DocumentNode(100, 100, inX: 200, inY: 70)]);

        Assert.True(path.X < 0, "the curve bulges out to the left");
        Assert.True(path.X + path.Width > 100, "and to the right");
        Assert.Equal(0, path.Y, 9);
        Assert.Equal(100, path.Height, 9);
        Assert.True(path.X > -100 && path.X + path.Width < 200, "the handle points are not part of the box");
    }

    [Fact]
    public void Create_ClosedPathWithACurvedClosingSegment_BoxIncludesTheClosingCurve()
    {
        DocumentNode[] nodes =
        [
            new(0, 0, inX: -50, inY: 50),
            new(100, 0),
            new(100, 100, outX: -50, outY: 150),
        ];

        var closed = PathShape.Create(nodes, isClosed: true);
        var open = PathShape.Create(nodes, isClosed: false);

        Assert.True(closed.X < 0);
        Assert.True(closed.Y + closed.Height > 100);
        // Without the closing segment the two handles belong to no segment.
        Assert.Equal(new Bounds(0, 0, 100, 100), open.Bounds);
    }

    [Fact]
    public void Create_FewerThanTwoNodes_Throws()
    {
        Assert.Throws<ArgumentException>(() => PathShape.Create(Array.Empty<DocumentNode>()));
        Assert.Throws<ArgumentException>(() => PathShape.Create([Corner(1, 1)]));
        Assert.Throws<ArgumentNullException>(() => PathShape.Create((IReadOnlyList<DocumentNode>)null!));
    }

    [Fact]
    public void SubPaths_FewerThanTwoNodesOrDefault_Throws()
    {
        Assert.Throws<ArgumentException>(() => new PathShape { SubPaths = [new SubPath([new PathNode(0, 0)])] });
        Assert.Throws<ArgumentException>(() => new PathShape { SubPaths = default });
    }

    [Fact]
    public void Create_HorizontalLine_HasZeroHeightAndKeepsItsPosition()
    {
        var path = PathShape.Create([Corner(10, 50), Corner(110, 50)]);

        Assert.Equal(new Bounds(10, 50, 100, 0), path.Bounds);
        Assert.All(path.SubPaths[0].Nodes, node => Assert.Equal(0, node.Y));
        Assert.Equal(50, path.ToDocumentNodes()[1].Y);
    }

    [Fact]
    public void Create_VerticalLine_HasZeroWidthAndKeepsItsPosition()
    {
        var path = PathShape.Create([Corner(30, 0), Corner(30, 80)]);

        Assert.Equal(new Bounds(30, 0, 0, 80), path.Bounds);
        Assert.Equal(30, path.ToDocumentNodes()[0].X);
    }

    [Fact]
    public void Create_TwoEqualPoints_HasZeroSizeWithoutNaN()
    {
        var path = PathShape.Create([Corner(5, 5), Corner(5, 5)]);

        Assert.Equal(new Bounds(5, 5, 0, 0), path.Bounds);
        Assert.All(path.ToDocumentNodes(), node => Assert.True(double.IsFinite(node.X) && double.IsFinite(node.Y)));
    }

    [Fact]
    public void ToDocumentNodes_AfterCreate_ReturnsTheGivenNodes()
    {
        DocumentNode[] nodes =
        [
            new(10, 20, outX: 40, outY: -10),
            new(110, 20, inX: 80, inY: 90),
            new(60, 120, inX: 70, inY: 130, outX: 50, outY: 130),
        ];

        var back = PathShape.Create(nodes, isClosed: true).ToDocumentNodes();

        Assert.Equal(nodes.Length, back.Count);
        for (var i = 0; i < nodes.Length; i++)
        {
            AssertNodeEqual(nodes[i], back[i]);
        }
    }

    [Fact]
    public void With_DoubledWidth_ScalesThePicture()
    {
        var path = Triangle();

        var wider = path with { Width = path.Width * 2 };

        var nodes = wider.ToDocumentNodes();
        Assert.Equal(10, nodes[0].X, 9);
        Assert.Equal(210, nodes[1].X, 9);
        Assert.Equal(110, nodes[2].X, 9);
        Assert.Equal(120, nodes[2].Y, 9);
    }

    [Fact]
    public void With_NewPosition_MovesThePicture()
    {
        var moved = Triangle() with { X = 0, Y = 0 };

        var nodes = moved.ToDocumentNodes();
        Assert.Equal(0, nodes[0].X, 9);
        Assert.Equal(100, nodes[1].X, 9);
        Assert.Equal(100, nodes[2].Y, 9);
    }

    [Fact]
    public void With_WidthZeroAndBack_RestoresThePicture()
    {
        var path = Triangle();

        var restored = (path with { Width = 0 }) with { Width = path.Width };

        Assert.Equal(path.SubPaths[0].Nodes, restored.SubPaths[0].Nodes);
        Assert.Equal(60, restored.ToDocumentNodes()[2].X, 9);
    }

    [Fact]
    public void MovedCopy_Path_MovesTheBoxAndGetsANewId()
    {
        var path = Triangle();

        var copy = path.MovedCopy(Guid.NewGuid, 5, -5);

        var moved = Assert.IsType<PathShape>(copy);
        Assert.NotEqual(path.Id, moved.Id);
        Assert.Equal(15, moved.X);
        Assert.Equal(15, moved.Y);
        Assert.Equal(path.SubPaths[0].Nodes, moved.SubPaths[0].Nodes);
    }

    [Fact]
    public void RotatedBounds_RotatedPath_IsTheTurnedBox()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(100, 0), Corner(100, 20)]) with { Rotation = 90 };

        var rotated = path.RotatedBounds;

        Assert.Equal(20, rotated.Width, 9);
        Assert.Equal(100, rotated.Height, 9);
    }

    [Fact]
    public void Segments_ClosedTriangle_HasThreeLines()
    {
        var segments = Triangle().Segments();

        Assert.Equal(3, segments.Count);
        Assert.Equal(segments[0].P3, segments[1].P0);
        Assert.Equal(segments[2].P3, segments[0].P0);
        Assert.Equal(segments[0].P0, segments[0].P1);
        Assert.Equal(segments[0].P3, segments[0].P2);
    }

    [Fact]
    public void Segments_OpenPath_HasOneLessThanNodes()
    {
        Assert.Equal(2, Triangle(closed: false).Segments().Count);
    }

    private static void AssertNodeEqual(DocumentNode expected, DocumentNode actual)
    {
        Assert.Equal(expected.X, actual.X, 9);
        Assert.Equal(expected.Y, actual.Y, 9);
        AssertNullableEqual(expected.InX, actual.InX);
        AssertNullableEqual(expected.InY, actual.InY);
        AssertNullableEqual(expected.OutX, actual.OutX);
        AssertNullableEqual(expected.OutY, actual.OutY);
    }

    private static void AssertNullableEqual(double? expected, double? actual)
    {
        Assert.Equal(expected.HasValue, actual.HasValue);
        if (expected.HasValue)
        {
            Assert.Equal(expected.Value, actual!.Value, 9);
        }
    }
}
