using System;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class ShapeRotationTests
{
    private static RectangleShape Rect(double rotation = 0) =>
        new() { X = 100, Y = 100, Width = 200, Height = 100, Rotation = rotation };

    private static EllipseShape Ellipse(double rotation = 0) =>
        new() { X = 100, Y = 100, Width = 200, Height = 100, Rotation = rotation };

    [Fact]
    public void Rotation_Default_IsZero()
    {
        Assert.Equal(0, Rect().Rotation);
        Assert.Equal(0, Ellipse().Rotation);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(45, 45)]
    [InlineData(359.5, 359.5)]
    [InlineData(360, 0)]
    [InlineData(720, 0)]
    [InlineData(725.5, 5.5)]
    [InlineData(-90, 270)]
    [InlineData(-360, 0)]
    [InlineData(-725.5, 354.5)]
    [InlineData(-1e-20, 0)]
    public void Rotation_IsNormalizedToZeroUpTo360(double input, double expected)
    {
        Assert.Equal(expected, Rect(input).Rotation, precision: 9);
        Assert.InRange(Rect(input).Rotation, 0, 359.999999999);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Rotation_NotFinite_IsRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Rect(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => Rect() with { Rotation = value });
    }

    [Fact]
    public void With_Rotation_KeepsIdAndEverythingElse()
    {
        var shape = Rect();

        var turned = shape with { Rotation = 30 };

        Assert.Equal(shape.Id, turned.Id);
        Assert.Equal((shape.X, shape.Y, shape.Width, shape.Height), (turned.X, turned.Y, turned.Width, turned.Height));
        Assert.Same(shape.Appearance, turned.Appearance);
        Assert.Equal(30, turned.Rotation);
        Assert.Equal(0, shape.Rotation);
    }

    [Fact]
    public void Center_IsTheMiddleOfTheBox()
    {
        Assert.Equal((200, 150), (Rect().CenterX, Rect().CenterY));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(271)]
    public void Contains_TheCenter_AlwaysHits(double rotation)
    {
        Assert.True(Rect(rotation).Contains(200, 150));
        Assert.True(Ellipse(rotation).Contains(200, 150));
    }

    [Fact]
    public void Contains_Rectangle_Unrotated_UsesTheBox()
    {
        var rect = Rect();

        Assert.True(rect.Contains(101, 101));
        Assert.True(rect.Contains(299, 199));
        Assert.False(rect.Contains(99, 150));
        Assert.False(rect.Contains(200, 201));
    }

    [Fact]
    public void Contains_Rectangle_Rotated90_SwapsWidthAndHeightAroundTheCenter()
    {
        var rect = Rect(90); // turned: 100 wide, 200 tall around (200, 150)

        Assert.True(rect.Contains(200, 60));    // above the unturned box, inside the turned one
        Assert.True(rect.Contains(200, 240));
        Assert.False(rect.Contains(120, 150));  // inside the unturned box, outside the turned one
        Assert.False(rect.Contains(280, 150));
        Assert.True(rect.Contains(160, 150));
    }

    [Fact]
    public void Contains_Rectangle_Rotated45_CornerOfTheUnturnedBoxIsOutside()
    {
        var rect = Rect(45);

        Assert.False(rect.Contains(101, 101));  // top-left corner of the unturned box
        Assert.False(rect.Contains(299, 199));
        Assert.True(rect.Contains(250, 150));   // along the long axis turned by 45 degrees: still near the center
        Assert.True(rect.Contains(200 + 60, 150 + 60)); // on the turned long axis
    }

    [Fact]
    public void Contains_Rectangle_Rotated180_IsTheSameAsUnrotated()
    {
        var rect = Rect(180);

        Assert.True(rect.Contains(101, 101));
        Assert.True(rect.Contains(299, 199));
        Assert.False(rect.Contains(99, 150));
    }

    [Fact]
    public void Contains_Rectangle_PositiveRotationTurnsClockwise()
    {
        // Turned 30 degrees clockwise (y down), the right end of the long axis moves down.
        var rect = Rect(30);
        var endX = 200 + 90 * Math.Cos(Math.PI / 6);
        var endY = 150 + 90 * Math.Sin(Math.PI / 6);

        Assert.True(rect.Contains(endX, endY));
        Assert.False(rect.Contains(endX, 150 - (endY - 150)));
    }

    [Fact]
    public void Contains_Ellipse_Rotated90_SwapsTheRadii()
    {
        var ellipse = Ellipse(90); // turned: radius 50 across, 100 up and down

        Assert.True(ellipse.Contains(200, 70));
        Assert.False(ellipse.Contains(270, 150));
        Assert.False(ellipse.Contains(200, 40));
    }

    [Fact]
    public void Contains_Ellipse_Rotated45_HitsAlongTheTurnedAxis()
    {
        var ellipse = Ellipse(45);
        var along = 90 / Math.Sqrt(2);

        Assert.True(ellipse.Contains(200 + along, 150 + along));
        Assert.False(ellipse.Contains(200 + along, 150 - along));
    }

    [Fact]
    public void Contains_ZeroSizeShapes_StillWorkWhenRotated()
    {
        var point = new RectangleShape { X = 10, Y = 10, Width = 0, Height = 0, Rotation = 37 };

        Assert.True(point.Contains(10, 10));
        Assert.False(point.Contains(11, 10));
    }

    [Fact]
    public void RotatedBounds_Unrotated_EqualsBounds()
    {
        Assert.Equal(Rect().Bounds, Rect().RotatedBounds);
    }

    [Fact]
    public void RotatedBounds_Rotated90_SwapsWidthAndHeightAroundTheCenter()
    {
        var bounds = Rect(90).RotatedBounds;

        Assert.Equal(150, bounds.X, precision: 9);
        Assert.Equal(50, bounds.Y, precision: 9);
        Assert.Equal(100, bounds.Width, precision: 9);
        Assert.Equal(200, bounds.Height, precision: 9);
    }

    [Fact]
    public void RotatedBounds_Rotated45_IsTheDiagonalBox()
    {
        var bounds = Rect(45).RotatedBounds;
        var extent = (200 + 100) / 2 / Math.Sqrt(2) * 2; // (w + h) / sqrt(2)

        Assert.Equal(extent, bounds.Width, precision: 9);
        Assert.Equal(extent, bounds.Height, precision: 9);
        Assert.Equal(200, bounds.X + bounds.Width / 2, precision: 9);
        Assert.Equal(150, bounds.Y + bounds.Height / 2, precision: 9);
    }

    [Fact]
    public void RotatedBounds_ContainTheTurnedCorners()
    {
        var shape = Rect(33.3);
        var bounds = shape.RotatedBounds;
        var radians = 33.3 * Math.PI / 180;
        foreach (var (dx, dy) in new[] { (-100.0, -50.0), (100.0, -50.0), (100.0, 50.0), (-100.0, 50.0) })
        {
            var x = 200 + dx * Math.Cos(radians) - dy * Math.Sin(radians);
            var y = 150 + dx * Math.Sin(radians) + dy * Math.Cos(radians);

            Assert.InRange(x, bounds.X - 1e-9, bounds.X + bounds.Width + 1e-9);
            Assert.InRange(y, bounds.Y - 1e-9, bounds.Y + bounds.Height + 1e-9);
        }
    }

    [Fact]
    public void Document_HitTest_FindsARotatedShapeWhereItIs()
    {
        var shape = Rect(90);
        var document = Document.Create("L");
        document = document.AddShape(document.Layers[0].Id, shape);

        Assert.Single(document.HitTest(200, 60));
        Assert.Empty(document.HitTest(120, 150));
    }
}
