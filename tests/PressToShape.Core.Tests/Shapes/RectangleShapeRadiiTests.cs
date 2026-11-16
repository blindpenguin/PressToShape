using System;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class RectangleShapeRadiiTests
{
    // 100 x 60 box at (10, 20), so the limit is 30.
    private static RectangleShape Rect(CornerRadii radii) => new() { X = 10, Y = 20, Width = 100, Height = 60, Radii = radii };

    [Fact]
    public void Radii_NewRectangle_IsZero()
    {
        var shape = new RectangleShape();

        Assert.True(shape.Radii.IsZero);
        Assert.True(shape.EffectiveRadii.IsZero);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Radii_InvalidValue_Throws(double value)
    {
        foreach (var corner in Enum.GetValues<Corner>())
        {
            var radii = CornerRadii.Zero.With(corner, value);

            Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleShape { Radii = radii });
            Assert.Throws<ArgumentOutOfRangeException>(() => Rect(CornerRadii.Zero) with { Radii = radii });
        }
    }

    [Fact]
    public void EffectiveRadii_RadiusAboveLimit_IsLimitedButStoredValueIsKept()
    {
        var shape = Rect(new CornerRadii(100, 10, 0, 40));

        Assert.Equal(new CornerRadii(100, 10, 0, 40), shape.Radii);
        Assert.Equal(new CornerRadii(30, 10, 0, 30), shape.EffectiveRadii);
    }

    [Fact]
    public void EffectiveRadii_ResizedSmallerAndLargerAgain_ReturnsToTheStoredValue()
    {
        var shape = Rect(CornerRadii.Uniform(20));

        var small = shape with { Height = 10 };
        var restored = small with { Height = 60 };

        Assert.Equal(CornerRadii.Uniform(5), small.EffectiveRadii);
        Assert.Equal(CornerRadii.Uniform(20), small.Radii);
        Assert.Equal(CornerRadii.Uniform(20), restored.EffectiveRadii);
    }

    [Fact]
    public void Radii_MovedAndRotated_AreKept()
    {
        var shape = Rect(new CornerRadii(1, 2, 3, 4)) with { X = 500, Y = -5, Rotation = 45 };

        Assert.Equal(new CornerRadii(1, 2, 3, 4), shape.Radii);
    }

    [Fact]
    public void Equals_DifferentRadii_AreNotEqual()
    {
        var a = Rect(CornerRadii.Uniform(5));

        Assert.NotEqual(a, a with { Radii = CornerRadii.Uniform(6) });
        Assert.Equal(a, a with { Radii = CornerRadii.Uniform(5) });
    }

    [Fact]
    public void Contains_SquareCorners_HitsTheCornerPoints()
    {
        var shape = Rect(CornerRadii.Zero);

        Assert.True(shape.Contains(10, 20));
        Assert.True(shape.Contains(110, 80));
    }

    [Fact]
    public void Contains_RoundedCorner_MissesTheCutOffArea()
    {
        var shape = Rect(CornerRadii.Uniform(20));

        Assert.False(shape.Contains(10.5, 20.5));
        Assert.False(shape.Contains(109.5, 79.5));
        Assert.False(shape.Contains(10.5, 79.5));
        Assert.False(shape.Contains(109.5, 20.5));
    }

    [Fact]
    public void Contains_RoundedCorner_HitsInsideTheArcAndOnTheStraightParts()
    {
        var shape = Rect(CornerRadii.Uniform(20));

        // The arc center is (30, 40). The arc at 45 degrees is 20 * (1 - 0.7071) = 5.86 in from both sides.
        Assert.False(shape.Contains(10 + 5, 20 + 5));
        Assert.True(shape.Contains(10 + 7, 20 + 7));
        Assert.True(shape.Contains(60, 20));
        Assert.True(shape.Contains(10, 50));
        Assert.True(shape.Contains(10, 40));
    }

    [Fact]
    public void Contains_OnlyOneCornerRounded_OtherCornersStaySquare()
    {
        var shape = Rect(CornerRadii.Zero.With(Corner.TopRight, 20));

        Assert.True(shape.Contains(10.5, 20.5));
        Assert.False(shape.Contains(109.5, 20.5));
        Assert.True(shape.Contains(109.5, 79.5));
    }

    [Fact]
    public void Contains_RadiusAboveLimit_UsesTheEffectiveRadius()
    {
        var shape = Rect(CornerRadii.Uniform(500));

        // The effective radius is 30: a pill. The middle of the left edge is inside, the corner point is not.
        Assert.True(shape.Contains(10, 50));
        Assert.False(shape.Contains(10.5, 20.5));
    }

    [Fact]
    public void Contains_Rotated_RoundsTheRotatedCorners()
    {
        // Turned by 90 degrees about the center (60, 50): the rounded top left corner moves to the top right.
        var shape = Rect(CornerRadii.Zero.With(Corner.TopLeft, 20)) with { Rotation = 90 };

        // The turned shape covers x 30..90 and y 0..100, its top right corner (90, 0) is the rounded one.
        Assert.False(shape.Contains(89.5, 0.5));
        Assert.True(shape.Contains(30.5, 0.5));
        Assert.True(shape.Contains(60, 50));
    }

    [Fact]
    public void Contains_OutsideTheBox_IsFalse()
    {
        var shape = Rect(CornerRadii.Uniform(20));

        Assert.False(shape.Contains(5, 50));
        Assert.False(shape.Contains(60, 85));
    }
}
