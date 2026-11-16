using System;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class ShapeConversionRoundedTests
{
    private static readonly Appearance NoStroke = Appearance.Default with { Stroke = null };

    private static RectangleShape Rectangle(CornerRadii radii, double rotation = 0) =>
        new() { X = 20, Y = 30, Width = 200, Height = 120, Radii = radii, Rotation = rotation, Appearance = NoStroke };

    [Fact]
    public void ToPath_ZeroRadii_GivesTheFourCornerNodesOfBefore()
    {
        var path = ShapeConversion.ToPath(Rectangle(CornerRadii.Zero))!;

        Assert.Equal([new PathNode(0, 0), new PathNode(1, 0), new PathNode(1, 1), new PathNode(0, 1)], path.Nodes.AsSpan().ToArray());
    }

    [Fact]
    public void ToPath_RadiusOnlyAboveTheLimit_StillRoundsWithTheLimit()
    {
        var path = ShapeConversion.ToPath(Rectangle(CornerRadii.Uniform(1000)))!;

        // The limit is 60 (half of the height), so there are eight nodes and the arcs meet in the middle of the short sides.
        Assert.Equal(8, path.Nodes.Length);
    }

    [Theory]
    [InlineData(Corner.TopLeft)]
    [InlineData(Corner.TopRight)]
    [InlineData(Corner.BottomRight)]
    [InlineData(Corner.BottomLeft)]
    public void ToPath_OneRoundedCorner_HasFiveNodes(Corner corner)
    {
        var path = ShapeConversion.ToPath(Rectangle(CornerRadii.Zero.With(corner, 20)))!;

        Assert.Equal(5, path.Nodes.Length);
        Assert.True(path.IsClosed);
    }

    [Fact]
    public void ToPath_AllCornersRounded_HasEightNodes()
    {
        var path = ShapeConversion.ToPath(Rectangle(CornerRadii.Uniform(20)))!;

        Assert.Equal(8, path.Nodes.Length);
        Assert.All(path.Nodes, node => Assert.Equal(NodeType.Corner, node.Type));
    }

    [Fact]
    public void ToPath_AllCornersRounded_NodesSitAtTheEndsOfTheArcs()
    {
        var nodes = ShapeConversion.ToPath(Rectangle(CornerRadii.Uniform(20)))!.ToDocumentNodes();

        // Clockwise from the top left: left edge up to (20, 50), arc to (40, 30), line to (200, 30), arc to (220, 50), and so on.
        (double X, double Y)[] expected =
        [
            (20, 50), (40, 30), (200, 30), (220, 50), (220, 130), (200, 150), (40, 150), (20, 130),
        ];
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].X, nodes[i].X, 9);
            Assert.Equal(expected[i].Y, nodes[i].Y, 9);
        }
    }

    [Fact]
    public void ToPath_RoundedCorner_ArcPointsLieOnTheCircle()
    {
        var nodes = ShapeConversion.ToPath(Rectangle(CornerRadii.Uniform(20)))!.ToDocumentNodes();
        var from = nodes[0];
        var to = nodes[1];
        var centerX = 40.0;
        var centerY = 50.0;

        // Evaluate the cubic of the top left arc. The usual approximation stays within 0.03 percent of the radius.
        for (var t = 0.0; t <= 1.0; t += 0.1)
        {
            var u = 1 - t;
            var x = u * u * u * from.X + 3 * u * u * t * from.OutX!.Value + 3 * u * t * t * to.InX!.Value + t * t * t * to.X;
            var y = u * u * u * from.Y + 3 * u * u * t * from.OutY!.Value + 3 * u * t * t * to.InY!.Value + t * t * t * to.Y;
            var distance = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

            Assert.Equal(20, distance, 1);
        }
    }

    [Fact]
    public void ToPath_Rounded_KeepsIdNameFlagsBoxRotationAndAppearance()
    {
        var shape = Rectangle(CornerRadii.Uniform(30), 25) with
        {
            Name = "Button",
            IsVisible = false,
            IsLocked = true,
            Appearance = Appearance.Default with { StrokeWidth = 7, Opacity = 0.5 },
        };

        var path = ShapeConversion.ToPath(shape)!;

        Assert.Equal(shape.Id, path.Id);
        Assert.Equal("Button", path.Name);
        Assert.False(path.IsVisible);
        Assert.True(path.IsLocked);
        Assert.Equal(shape.Bounds, path.Bounds);
        Assert.Equal(25, path.Rotation);
        Assert.Equal(shape.Appearance, path.Appearance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void ToPath_Rounded_ContainsTheSamePointsAsTheRoundedRectangle(double rotation)
    {
        var shape = Rectangle(new CornerRadii(40, 0, 25, 60), rotation);
        var path = ShapeConversion.ToPath(shape)!;

        for (var x = -20; x <= 280; x += 5)
        {
            for (var y = -40; y <= 220; y += 5)
            {
                if (NearTheOutline(shape, x, y))
                {
                    continue;
                }

                Assert.True(shape.Contains(x, y) == path.Contains(x, y), $"({x}, {y}) at rotation {rotation}");
            }
        }
    }

    // The outline of the path is a curve approximation of the arcs, so points within a pixel of the outline may differ.
    private static bool NearTheOutline(RectangleShape shape, double x, double y)
    {
        foreach (var offset in new[] { -1.5, 0, 1.5 })
        {
            var inner = shape.Contains(x + offset, y);
            var other = shape.Contains(x, y + offset);
            var center = shape.Contains(x, y);
            if (inner != center || other != center)
            {
                return true;
            }
        }

        return false;
    }

    [Fact]
    public void Corners_ZeroRadius_StartAndEndAreTheCornerPoint()
    {
        var corners = RoundedRectangleGeometry.Corners(10, 20, 100, 50, CornerRadii.Zero);

        Assert.Equal(4, corners.Count);
        Assert.All(corners, corner =>
        {
            Assert.Equal(corner.Point, corner.Start);
            Assert.Equal(corner.Point, corner.End);
        });
        Assert.Equal((10.0, 20.0), corners[0].Point);
        Assert.Equal((110.0, 70.0), corners[2].Point);
    }

    [Fact]
    public void Corners_Radii_GiveClockwiseArcEnds()
    {
        var corners = RoundedRectangleGeometry.Corners(0, 0, 100, 50, new CornerRadii(5, 10, 15, 20));

        Assert.Equal(((0.0, 5.0), (5.0, 0.0)), (corners[0].Start, corners[0].End));
        Assert.Equal(((90.0, 0.0), (100.0, 10.0)), (corners[1].Start, corners[1].End));
        Assert.Equal(((100.0, 35.0), (85.0, 50.0)), (corners[2].Start, corners[2].End));
        Assert.Equal(((20.0, 50.0), (0.0, 30.0)), (corners[3].Start, corners[3].End));
    }
}
