using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class EllipseShapeTests
{
    // Center (50, 30), radii 50 and 30.
    private static EllipseShape Ellipse() => new() { X = 0, Y = 0, Width = 100, Height = 60 };

    [Fact]
    public void Bounds_Ellipse_EqualsPositionAndSize()
    {
        Assert.Equal(new Bounds(0, 0, 100, 60), Ellipse().Bounds);
    }

    [Theory]
    [InlineData(50, 30, true)]
    [InlineData(1, 30, true)]
    [InlineData(0, 30, true)]
    [InlineData(100, 30, true)]
    [InlineData(50, 0, true)]
    [InlineData(50, 60, true)]
    [InlineData(-0.001, 30, false)]
    [InlineData(100.001, 30, false)]
    [InlineData(50, -0.001, false)]
    [InlineData(50, 60.001, false)]
    [InlineData(0, 0, false)]
    [InlineData(100, 60, false)]
    public void Contains_Point_IsInsideOrOnTheEllipse(double x, double y, bool expected)
    {
        Assert.Equal(expected, Ellipse().Contains(x, y));
    }

    [Theory]
    [InlineData(10, 20, true)]
    [InlineData(10, 0, true)]
    [InlineData(10, 40, true)]
    [InlineData(10, 40.001, false)]
    [InlineData(10, -0.001, false)]
    [InlineData(10.001, 20, false)]
    [InlineData(9.999, 20, false)]
    public void Contains_ZeroWidth_ActsAsVerticalSegment(double x, double y, bool expected)
    {
        var shape = new EllipseShape { X = 10, Y = 0, Width = 0, Height = 40 };

        Assert.Equal(expected, shape.Contains(x, y));
    }

    [Theory]
    [InlineData(20, 10, true)]
    [InlineData(0, 10, true)]
    [InlineData(40, 10, true)]
    [InlineData(40.001, 10, false)]
    [InlineData(20, 10.001, false)]
    [InlineData(20, 9.999, false)]
    public void Contains_ZeroHeight_ActsAsHorizontalSegment(double x, double y, bool expected)
    {
        var shape = new EllipseShape { X = 0, Y = 10, Width = 40, Height = 0 };

        Assert.Equal(expected, shape.Contains(x, y));
    }

    [Theory]
    [InlineData(5, 7, true)]
    [InlineData(5.001, 7, false)]
    [InlineData(5, 7.001, false)]
    [InlineData(0, 0, false)]
    public void Contains_ZeroWidthAndHeight_ContainsOnlyItsOwnPoint(double x, double y, bool expected)
    {
        var shape = new EllipseShape { X = 5, Y = 7, Width = 0, Height = 0 };

        Assert.Equal(expected, shape.Contains(x, y));
    }

    [Fact]
    public void Contains_FarAwayPoint_IsFalse()
    {
        Assert.False(Ellipse().Contains(1e300, -1e300));
    }
}
