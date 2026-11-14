using System;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class RectangleShapeTests
{
    private static readonly Guid Id1 = new("11111111-1111-1111-1111-111111111111");

    private static RectangleShape Rect() => new() { Id = Id1, X = 10, Y = 20, Width = 100, Height = 50 };

    [Fact]
    public void Bounds_Rectangle_EqualsPositionAndSize()
    {
        Assert.Equal(new Bounds(10, 20, 100, 50), Rect().Bounds);
    }

    [Fact]
    public void Bounds_WithStroke_DoesNotIncludeStrokeWidth()
    {
        var shape = Rect() with { Appearance = Appearance.Default with { StrokeWidth = 20 } };

        Assert.Equal(new Bounds(10, 20, 100, 50), shape.Bounds);
    }

    [Fact]
    public void Constructor_NoId_GetsNonEmptyUniqueIds()
    {
        var a = new RectangleShape();
        var b = new RectangleShape();

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void Constructor_NoAppearance_UsesDefault()
    {
        Assert.Equal(Appearance.Default, new RectangleShape().Appearance);
    }

    [Fact]
    public void With_ChangedGeometry_KeepsIdAndLeavesOriginalUnchanged()
    {
        var original = Rect();

        var moved = original with { X = 0, Width = 5 };

        Assert.Equal(Id1, moved.Id);
        Assert.Equal(10, original.X);
        Assert.Equal(100, original.Width);
    }

    [Fact]
    public void Constructor_ZeroWidthAndHeight_IsAllowed()
    {
        var shape = new RectangleShape { Width = 0, Height = 0 };

        Assert.Equal(0, shape.Width);
        Assert.Equal(0, shape.Height);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Width_Invalid_Throws(double width)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleShape { Width = width });
        Assert.Throws<ArgumentOutOfRangeException>(() => Rect() with { Width = width });
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.NegativeInfinity)]
    public void Height_Invalid_Throws(double height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleShape { Height = height });
        Assert.Throws<ArgumentOutOfRangeException>(() => Rect() with { Height = height });
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Position_NotFinite_Throws(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleShape { X = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => Rect() with { Y = value });
    }

    [Fact]
    public void Position_Negative_IsAllowed()
    {
        var shape = new RectangleShape { X = -50, Y = -5 };

        Assert.Equal((-50, -5), (shape.X, shape.Y));
    }

    [Fact]
    public void Id_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new RectangleShape { Id = Guid.Empty });
    }

    [Fact]
    public void Appearance_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RectangleShape { Appearance = null! });
    }

    [Theory]
    [InlineData(50, 40, true)]
    [InlineData(10, 20, true)]
    [InlineData(110, 70, true)]
    [InlineData(10, 45, true)]
    [InlineData(110, 45, true)]
    [InlineData(60, 20, true)]
    [InlineData(60, 70, true)]
    [InlineData(9.999, 40, false)]
    [InlineData(110.001, 40, false)]
    [InlineData(50, 19.999, false)]
    [InlineData(50, 70.001, false)]
    public void Contains_Point_IncludesEdges(double x, double y, bool expected)
    {
        Assert.Equal(expected, Rect().Contains(x, y));
    }
}
