using System;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class CornerRadiiTests
{
    [Fact]
    public void Zero_Default_IsZeroAndUniform()
    {
        Assert.True(CornerRadii.Zero.IsZero);
        Assert.True(CornerRadii.Zero.IsUniform);
    }

    [Fact]
    public void Uniform_Value_SetsAllCorners()
    {
        var radii = CornerRadii.Uniform(5);

        Assert.Equal(new CornerRadii(5, 5, 5, 5), radii);
        Assert.True(radii.IsUniform);
        Assert.False(radii.IsZero);
    }

    [Theory]
    [InlineData(Corner.TopLeft, 1)]
    [InlineData(Corner.TopRight, 2)]
    [InlineData(Corner.BottomRight, 3)]
    [InlineData(Corner.BottomLeft, 4)]
    public void Indexer_Corner_ReturnsItsRadius(Corner corner, double expected)
    {
        var radii = new CornerRadii(1, 2, 3, 4);

        Assert.Equal(expected, radii[corner]);
    }

    [Theory]
    [InlineData(Corner.TopLeft)]
    [InlineData(Corner.TopRight)]
    [InlineData(Corner.BottomRight)]
    [InlineData(Corner.BottomLeft)]
    public void With_Corner_ChangesOnlyThatCorner(Corner corner)
    {
        var radii = CornerRadii.Zero.With(corner, 7);

        Assert.Equal(7, radii[corner]);
        foreach (var other in Enum.GetValues<Corner>())
        {
            if (other != corner)
            {
                Assert.Equal(0, radii[other]);
            }
        }
    }

    [Fact]
    public void Indexer_UndefinedCorner_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CornerRadii.Zero[(Corner)99]);
    }

    [Fact]
    public void With_UndefinedCorner_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CornerRadii.Zero.With((Corner)99, 1));
    }

    [Fact]
    public void IsUniform_DifferentRadii_IsFalse()
    {
        Assert.False(new CornerRadii(1, 1, 1, 2).IsUniform);
    }

    [Fact]
    public void Limited_WideBox_UsesHalfOfTheHeight()
    {
        var limited = new CornerRadii(100, 10, 0, 25).Limited(200, 40);

        Assert.Equal(new CornerRadii(20, 10, 0, 20), limited);
    }

    [Fact]
    public void Limited_TallBox_UsesHalfOfTheWidth()
    {
        var limited = CornerRadii.Uniform(100).Limited(30, 200);

        Assert.Equal(CornerRadii.Uniform(15), limited);
    }

    [Fact]
    public void Limited_SquareBox_UsesHalfOfTheSide()
    {
        Assert.Equal(CornerRadii.Uniform(50), CornerRadii.Uniform(80).Limited(100, 100));
    }

    [Fact]
    public void Limited_ZeroSizeBox_GivesZero()
    {
        Assert.True(CornerRadii.Uniform(10).Limited(0, 50).IsZero);
        Assert.True(CornerRadii.Uniform(10).Limited(50, 0).IsZero);
    }

    [Fact]
    public void Limited_NegativeValue_BecomesZero()
    {
        Assert.Equal(new CornerRadii(0, 5, 0, 0), new CornerRadii(-3, 5, double.NaN, -0.0).Limited(100, 100));
    }
}
