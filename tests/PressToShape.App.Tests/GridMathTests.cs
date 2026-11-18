using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class GridMathTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(4.9, 10, 0)]
    [InlineData(5, 10, 10)]
    [InlineData(14, 10, 10)]
    [InlineData(-4.9, 10, 0)]
    [InlineData(-5, 10, -10)]
    [InlineData(-16, 10, -20)]
    [InlineData(10, 10, 10)]
    [InlineData(7.4, 2.5, 7.5)]
    public void Nearest_Value_GivesTheClosestLine(double value, double spacing, double expected)
    {
        Assert.Equal(expected, GridMath.Nearest(value, spacing), 9);
    }

    [Theory]
    [InlineData(3, 10, 1, 10)]
    [InlineData(3, 10, -1, 0)]
    [InlineData(10, 10, 1, 20)]
    [InlineData(10, 10, -1, 0)]
    [InlineData(0, 10, 1, 10)]
    [InlineData(0, 10, -1, -10)]
    [InlineData(-3, 10, 1, 0)]
    [InlineData(-3, 10, -1, -10)]
    [InlineData(-10, 10, 1, 0)]
    public void NextLine_Value_GivesTheLineStrictlyBeyond(double value, double spacing, int direction, double expected)
    {
        Assert.Equal(expected, GridMath.NextLine(value, spacing, direction), 9);
    }

    [Fact]
    public void NextLine_ValueOnALineWithFloatingPointNoise_CountsAsOnTheLine()
    {
        // 0.1 * 3 is 0.30000000000000004 and 0.3 / 0.1 is 2.9999999999999996.
        Assert.Equal(0.4, GridMath.NextLine(0.1 * 3, 0.1, 1), 9);
        Assert.Equal(0.2, GridMath.NextLine(0.3, 0.1, -1), 9);
    }

    [Theory]
    [InlineData(3, 10, 1, 1, 7)]
    [InlineData(3, 10, -1, 1, -3)]
    [InlineData(10, 10, 1, 1, 10)]
    [InlineData(10, 10, -1, 1, -10)]
    [InlineData(3, 10, 1, 10, 97)]
    [InlineData(3, 10, -1, 10, -93)]
    [InlineData(10, 10, 1, 10, 100)]
    public void NudgeDelta_Edge_MovesToALineAndFurtherForMoreSteps(double edge, double spacing, int direction, int steps, double expected)
    {
        Assert.Equal(expected, GridMath.NudgeDelta(edge, spacing, direction, steps), 9);
    }

    [Fact]
    public void NudgeDelta_ManyPresses_DoNotDrift()
    {
        var edge = 0.0;

        for (var i = 0; i < 1000; i++)
        {
            edge += GridMath.NudgeDelta(edge, 0.1, 1, 1);
        }

        Assert.Equal(1000 * 0.1, edge, 9);
        Assert.Equal(edge, GridMath.Nearest(edge, 0.1), 9);
    }

    [Theory]
    [InlineData(double.NaN, 10)]
    [InlineData(double.PositiveInfinity, 10)]
    [InlineData(5, double.NaN)]
    [InlineData(5, 0)]
    [InlineData(5, -10)]
    public void Methods_UnusableInput_ChangeNothing(double value, double spacing)
    {
        Assert.Equal(value, GridMath.Nearest(value, spacing));
        Assert.Equal(value, GridMath.NextLine(value, spacing, 1));
        Assert.Equal(0, GridMath.NudgeDelta(value, spacing, 1, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void NextLine_BadDirection_ChangesNothing(int direction)
    {
        Assert.Equal(3, GridMath.NextLine(3, 10, direction));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -3)]
    public void NudgeDelta_BadDirectionOrSteps_MovesNothing(int direction, int steps)
    {
        Assert.Equal(0, GridMath.NudgeDelta(3, 10, direction, steps));
    }
}
