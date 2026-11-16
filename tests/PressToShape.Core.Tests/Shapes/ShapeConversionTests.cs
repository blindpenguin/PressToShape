using System;
using System.Linq;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class ShapeConversionTests
{
    private static readonly Appearance NoStroke = Appearance.Default with { Stroke = null };

    private static RectangleShape Rectangle(double rotation = 0) =>
        new() { X = 20, Y = 30, Width = 200, Height = 120, Rotation = rotation, Appearance = NoStroke, Name = "Box", IsLocked = false };

    private static EllipseShape Ellipse(double rotation = 0) =>
        new() { X = 20, Y = 30, Width = 200, Height = 120, Rotation = rotation, Appearance = NoStroke, Name = "Egg" };

    [Fact]
    public void ToPath_Rectangle_KeepsIdNameFlagsBoxRotationAndAppearance()
    {
        var shape = Rectangle(25) with
        {
            Name = "Window",
            IsVisible = false,
            IsLocked = true,
            Appearance = Appearance.Default with { StrokeWidth = 7, Opacity = 0.5 },
        };

        var path = ShapeConversion.ToPath(shape)!;

        Assert.Equal(shape.Id, path.Id);
        Assert.Equal("Window", path.Name);
        Assert.False(path.IsVisible);
        Assert.True(path.IsLocked);
        Assert.Equal(shape.Bounds, path.Bounds);
        Assert.Equal(25, path.Rotation);
        Assert.Equal(shape.Appearance, path.Appearance);
        Assert.True(path.IsClosed);
    }

    [Fact]
    public void ToPath_Ellipse_KeepsIdNameFlagsBoxRotationAndAppearance()
    {
        var shape = Ellipse(70) with { IsLocked = true };

        var path = ShapeConversion.ToPath(shape)!;

        Assert.Equal(shape.Id, path.Id);
        Assert.Equal("Egg", path.Name);
        Assert.True(path.IsLocked);
        Assert.Equal(shape.Bounds, path.Bounds);
        Assert.Equal(70, path.Rotation);
        Assert.Equal(shape.Appearance, path.Appearance);
    }

    [Fact]
    public void ToPath_Rectangle_HasFourCornerNodesInOrder()
    {
        var path = ShapeConversion.ToPath(Rectangle())!;

        Assert.Equal(4, path.Nodes.Length);
        Assert.All(path.Nodes, n => Assert.Equal(NodeType.Corner, n.Type));
        Assert.All(path.Nodes, n => Assert.False(n.HasIn || n.HasOut));
        Assert.Equal([new PathNode(0, 0), new PathNode(1, 0), new PathNode(1, 1), new PathNode(0, 1)], path.Nodes.AsSpan().ToArray());
    }

    [Fact]
    public void ToPath_Ellipse_HasFourSymmetricNodesWithTheCircleHandles()
    {
        var path = ShapeConversion.ToPath(Ellipse())!;

        Assert.Equal(4, path.Nodes.Length);
        Assert.All(path.Nodes, n => Assert.Equal(NodeType.Symmetric, n.Type));
        Assert.All(path.Nodes, n => Assert.True(NodeTypes.Satisfies(n)));
        // The top node sits in the middle of the top edge and its handles are 0.2761 of the width to each side.
        Assert.Equal(0.5, path.Nodes[0].X, 9);
        Assert.Equal(0, path.Nodes[0].Y, 9);
        Assert.Equal(0.5 + 0.5522847498307936 * 0.5, path.Nodes[0].OutX!.Value, 9);
        Assert.Equal(0.5 - 0.5522847498307936 * 0.5, path.Nodes[0].InX!.Value, 9);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void ToPath_Rectangle_ContainsTheSamePointsAsTheRectangle(double rotation)
    {
        var shape = Rectangle(rotation);
        var path = ShapeConversion.ToPath(shape)!;

        for (var x = -20; x <= 280; x += 7)
        {
            for (var y = -40; y <= 220; y += 7)
            {
                // The edge itself is not compared: both shapes may call it either way.
                if (NearTheEdgeOfTheRectangle(shape, x, y))
                {
                    continue;
                }

                Assert.True(shape.Contains(x, y) == path.Contains(x, y), $"({x}, {y}) at rotation {rotation}");
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void ToPath_Ellipse_ContainsTheSamePointsAsTheEllipseWithinTheApproximation(double rotation)
    {
        var shape = Ellipse(rotation);
        var path = ShapeConversion.ToPath(shape)!;

        for (var x = -20; x <= 280; x += 7)
        {
            for (var y = -40; y <= 220; y += 7)
            {
                if (NearTheEdgeOfTheEllipse(shape, x, y))
                {
                    continue;
                }

                Assert.True(shape.Contains(x, y) == path.Contains(x, y), $"({x}, {y}) at rotation {rotation}");
            }
        }
    }

    [Fact]
    public void ToPath_ZeroSizeShapes_GiveNoNaN()
    {
        var flatRectangle = ShapeConversion.ToPath(new RectangleShape { X = 5, Y = 5, Width = 0, Height = 40 })!;
        var pointEllipse = ShapeConversion.ToPath(new EllipseShape { X = 5, Y = 5, Width = 0, Height = 0 })!;

        Assert.Equal(0, flatRectangle.Width);
        Assert.All(pointEllipse.ToDocumentNodes(), n => Assert.True(double.IsFinite(n.X) && double.IsFinite(n.Y)));
    }

    [Fact]
    public void ToPath_PathGroupAndNull_AreNotConverted()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var group = new GroupShape { Children = [new RectangleShape { Width = 1, Height = 1 }] };

        Assert.Null(ShapeConversion.ToPath(path));
        Assert.Null(ShapeConversion.ToPath(group));
    }

    private static bool NearTheEdgeOfTheRectangle(RectangleShape shape, double x, double y)
    {
        var local = Unturn(shape, x, y);
        var dx = Math.Min(Math.Abs(local.X - shape.X), Math.Abs(local.X - (shape.X + shape.Width)));
        var dy = Math.Min(Math.Abs(local.Y - shape.Y), Math.Abs(local.Y - (shape.Y + shape.Height)));
        var inside = local.X >= shape.X && local.X <= shape.X + shape.Width && local.Y >= shape.Y && local.Y <= shape.Y + shape.Height;
        return inside ? Math.Min(dx, dy) < 0.5 : (local.X >= shape.X - 0.5 && local.X <= shape.X + shape.Width + 0.5 && local.Y >= shape.Y - 0.5 && local.Y <= shape.Y + shape.Height + 0.5);
    }

    private static bool NearTheEdgeOfTheEllipse(EllipseShape shape, double x, double y)
    {
        var local = Unturn(shape, x, y);
        var nx = (local.X - shape.CenterX) / (shape.Width / 2);
        var ny = (local.Y - shape.CenterY) / (shape.Height / 2);
        // The four-segment circle differs from the ellipse by less than 0.03 percent of the radius, so a ring of one percent is skipped.
        return Math.Abs(Math.Sqrt(nx * nx + ny * ny) - 1) < 0.01;
    }

    private static (double X, double Y) Unturn(Shape shape, double x, double y)
    {
        var radians = -shape.Rotation * Math.PI / 180;
        var dx = x - shape.CenterX;
        var dy = y - shape.CenterY;
        return (shape.CenterX + dx * Math.Cos(radians) - dy * Math.Sin(radians), shape.CenterY + dx * Math.Sin(radians) + dy * Math.Cos(radians));
    }
}
