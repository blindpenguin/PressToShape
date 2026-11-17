using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathShapeGeometryTests
{
    private static readonly Appearance NoStroke = Appearance.Default with { Stroke = null };

    private static DocumentNode Corner(double x, double y) => new(x, y);

    // A five-pointed star drawn with one stroke: the middle pentagon is wound twice.
    private static PathShape Pentagram(FillRule rule)
    {
        var nodes = new List<DocumentNode>();
        for (var i = 0; i < 5; i++)
        {
            var angle = (-90 + i * 144) * Math.PI / 180;
            nodes.Add(Corner(100 + 100 * Math.Cos(angle), 100 + 100 * Math.Sin(angle)));
        }

        return PathShape.Create(nodes, isClosed: true, fillRule: rule) with { Appearance = NoStroke };
    }

    // Where a node of this shape appears on the screen: turned around the center of the box.
    private static (double X, double Y) Visible(PathShape shape, DocumentNode node)
    {
        var radians = shape.Rotation * Math.PI / 180;
        var dx = node.X - shape.CenterX;
        var dy = node.Y - shape.CenterY;
        return (
            shape.CenterX + dx * Math.Cos(radians) - dy * Math.Sin(radians),
            shape.CenterY + dx * Math.Sin(radians) + dy * Math.Cos(radians));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(200)]
    public void Rebuilt_MovedNode_EveryNodeStaysWhereItWasOnTheScreen(double rotation)
    {
        var path = PathShape.Create([Corner(0, 0), Corner(100, 0), Corner(100, 60)]) with { Rotation = rotation };
        var edited = new List<DocumentNode>(path.ToDocumentNodes());
        edited[2] = new DocumentNode(160, 200);

        var rebuilt = path.Rebuilt(edited, isClosed: false, FillRule.NonZero);

        // Expected: the edited nodes turned around the OLD center. Actual: the rebuilt nodes turned around the NEW center.
        var rebuiltNodes = rebuilt.ToDocumentNodes();
        for (var i = 0; i < edited.Count; i++)
        {
            var expected = Visible(path, edited[i]);
            var actual = Visible(rebuilt, rebuiltNodes[i]);
            Assert.Equal(expected.X, actual.X, 6);
            Assert.Equal(expected.Y, actual.Y, 6);
        }
    }

    [Fact]
    public void Rebuilt_Edit_KeepsIdAppearanceNameFlagsAndRotation()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(50, 50)]) with
        {
            Rotation = 45,
            Name = "line",
            IsLocked = true,
            IsVisible = false,
            Appearance = Appearance.Default with { StrokeWidth = 7 },
        };

        var rebuilt = path.Rebuilt([Corner(0, 0), Corner(50, 50), Corner(0, 100)], isClosed: true, FillRule.EvenOdd);

        Assert.Equal(path.Id, rebuilt.Id);
        Assert.Equal(path.Appearance, rebuilt.Appearance);
        Assert.Equal("line", rebuilt.Name);
        Assert.True(rebuilt.IsLocked);
        Assert.False(rebuilt.IsVisible);
        Assert.Equal(45, rebuilt.Rotation);
        Assert.True(rebuilt.SubPaths[0].IsClosed);
        Assert.Equal(FillRule.EvenOdd, rebuilt.FillRule);
        Assert.Equal(3, rebuilt.SubPaths[0].Nodes.Length);
    }

    [Fact]
    public void Rebuilt_FewerThanTwoNodes_Throws()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(50, 50)]);

        Assert.Throws<ArgumentException>(() => path.Rebuilt([Corner(1, 1)], false, FillRule.NonZero));
    }

    [Fact]
    public void Contains_PentagramCenterWithNonZero_IsInside()
    {
        Assert.True(Pentagram(FillRule.NonZero).Contains(100, 100));
    }

    [Fact]
    public void Contains_PentagramCenterWithEvenOdd_IsOutside()
    {
        Assert.False(Pentagram(FillRule.EvenOdd).Contains(100, 100));
    }

    [Theory]
    [InlineData(FillRule.NonZero)]
    [InlineData(FillRule.EvenOdd)]
    public void Contains_PentagramPointOfTheStar_IsInsideWithBothRules(FillRule rule)
    {
        // A point inside the upper arm of the star, between the center and the tip at (100, 0).
        Assert.True(Pentagram(rule).Contains(100, 20));
    }

    [Fact]
    public void Contains_OutsideAClosedFilledPath_IsAMiss()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(100, 0), Corner(50, 100)], isClosed: true) with { Appearance = NoStroke };

        Assert.True(path.Contains(50, 30));
        Assert.False(path.Contains(5, 90));
        Assert.False(path.Contains(200, 50));
    }

    [Fact]
    public void Contains_ThinStrokeOfAnUnfilledOpenPath_IsHitNearTheLineOnly()
    {
        var path = PathShape.Create([Corner(0, 50), Corner(200, 50)]) with
        {
            Appearance = Appearance.Default with { Fill = null, StrokeWidth = 1 },
        };

        Assert.True(path.Contains(100, 50));
        Assert.True(path.Contains(100, 53), "the minimum margin makes a thin line easy to hit");
        Assert.False(path.Contains(100, 60));
        Assert.False(path.Contains(300, 50));
    }

    [Fact]
    public void Contains_WideStroke_HitsWithinHalfTheStrokeWidth()
    {
        var path = PathShape.Create([Corner(0, 50), Corner(200, 50)]) with
        {
            Appearance = Appearance.Default with { Fill = null, StrokeWidth = 30 },
        };

        Assert.True(path.Contains(100, 64));
        Assert.False(path.Contains(100, 70));
    }

    [Fact]
    public void Contains_UnfilledOpenCurve_DoesNotHitTheEmptyInside()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(100, 0), Corner(100, 100)]) with
        {
            Appearance = Appearance.Default with { Fill = null },
        };

        Assert.False(path.Contains(80, 30), "the area between the path and its closing line is not hit without a fill");
    }

    [Fact]
    public void Contains_FilledOpenPath_IsFilledAcrossTheGapButTheClosingLineIsNotAnOutline()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(100, 0), Corner(100, 100)]);

        Assert.True(path.Contains(80, 30));
        Assert.False(path.Contains(20, 80));
        // On the closing line itself, far from every stroked segment, only the fill decides.
        Assert.False(path.Contains(30, 40));
    }

    [Fact]
    public void Contains_RotatedPath_UsesTheTurnedPath()
    {
        var bar = PathShape.Create([Corner(0, 0), Corner(100, 0), Corner(100, 20), Corner(0, 20)], isClosed: true);
        var path = bar with { Rotation = 90, Appearance = NoStroke };

        // Turned by 90 degrees around (50, 10) the 100 by 20 bar stands upright.
        Assert.True(path.Contains(50, -30));
        Assert.False(path.Contains(95, 10));
    }

    [Fact]
    public void Contains_CurvedClosedPath_FollowsTheCurve()
    {
        // A half disc: a straight base and a bulging arch on top.
        DocumentNode[] nodes =
        [
            new(0, 100, outX: 0, outY: -33),
            new(100, 100, inX: 100, inY: -33),
        ];
        var path = PathShape.Create(nodes, isClosed: true) with { Appearance = NoStroke };

        Assert.True(path.Contains(50, 60));
        Assert.False(path.Contains(5, 10));
    }
}
