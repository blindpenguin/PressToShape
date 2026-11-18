using System;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class GridNudgeTests
{
    [Theory]
    [InlineData("Left", false, -1, 0)]
    [InlineData("Right", false, 1, 0)]
    [InlineData("Up", false, 0, -1)]
    [InlineData("Down", false, 0, 1)]
    [InlineData("Left", true, -10, 0)]
    [InlineData("Down", true, 0, 10)]
    public void Delta_WithoutTheGrid_IsOneOrTenUnits(string key, bool shift, double dx, double dy)
    {
        Assert.Equal((dx, dy), GridNudge.Delta(Enum.Parse<CanvasKey>(key), shift, snapToGrid: false, spacing: 25, left: 13, top: 17));
    }

    [Theory]
    [InlineData("Right", 13, 10, 7, 0)]
    [InlineData("Left", 13, 10, -3, 0)]
    [InlineData("Right", 20, 10, 10, 0)]
    [InlineData("Left", 20, 10, -10, 0)]
    public void Delta_WithTheGrid_MovesTheLeftEdgeToTheNextLine(string key, double left, double spacing, double dx, double dy)
    {
        Assert.Equal((dx, dy), GridNudge.Delta(Enum.Parse<CanvasKey>(key), false, true, spacing, left, 0));
    }

    [Fact]
    public void Delta_WithTheGridAndShift_MovesTenSpacingsFurther()
    {
        Assert.Equal((97, 0), GridNudge.Delta(CanvasKey.Right, true, true, 10, 3, 0));
        Assert.Equal((0, -93), GridNudge.Delta(CanvasKey.Up, true, true, 10, 0, 3));
    }

    [Fact]
    public void Delta_UpAndDown_UseTheTopEdge()
    {
        Assert.Equal((0, 3), GridNudge.Delta(CanvasKey.Down, false, true, 10, 999, 7));
        Assert.Equal((0, -7), GridNudge.Delta(CanvasKey.Up, false, true, 10, 999, 7));
    }

    [Fact]
    public void Delta_OtherKey_IsZero()
    {
        Assert.Equal((0, 0), GridNudge.Delta(CanvasKey.Escape, false, true, 10, 5, 5));
    }
}
