using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class GridLinesTests
{
    [Fact]
    public void Build_SpacingTenAtFullZoom_DrawsEveryLine()
    {
        var lines = GridLines.Build(10, 100, 0, 0, 50, 30);

        Assert.Equal([0, 10, 20, 30, 40, 50], lines.Vertical);
        Assert.Equal([0, 10, 20, 30], lines.Horizontal);
    }

    [Theory]
    [InlineData(100, 10)]
    [InlineData(80, 10)]
    [InlineData(50, 20)]
    [InlineData(10, 100)]
    [InlineData(1, 1000)]
    public void DrawnStep_Zoom_ChoosesTheSmallestStepFarEnoughApart(double zoomPercent, double expected)
    {
        Assert.Equal(expected, GridLines.DrawnStep(10, zoomPercent / 100), 9);
    }

    [Fact]
    public void Build_LowZoom_ThinsTheLinesToAtLeastEightPixels()
    {
        var lines = GridLines.Build(10, 10, 0, 0, 1000, 1000);

        Assert.All(lines.Vertical.Zip(lines.Vertical.Skip(1)), pair => Assert.True((pair.Second - pair.First) * 0.1 >= 8 - 1e-9));
        Assert.Equal(100, lines.Vertical[1] - lines.Vertical[0], 9);
    }

    [Fact]
    public void Build_OnlyLinesInsideTheVisibleRange()
    {
        var lines = GridLines.Build(10, 100, 15, -25, 45, 5);

        Assert.Equal([20, 30, 40], lines.Vertical);
        Assert.Equal([-20, -10, 0], lines.Horizontal);
    }

    [Fact]
    public void Build_LineExactlyOnTheRangeEdge_IsIncluded()
    {
        var lines = GridLines.Build(10, 100, 10, 10, 20, 20);

        Assert.Equal([10, 20], lines.Vertical);
    }

    [Theory]
    [InlineData(100, 400)]
    [InlineData(5, 4000)]
    [InlineData(1, 100000)]
    public void Build_AnyZoom_KeepsTheLineCountBelowTheVisibleSizeOverEight(double zoomPercent, double visibleScreenPixels)
    {
        var scale = zoomPercent / 100;
        var lines = GridLines.Build(3, zoomPercent, -500, -500, -500 + visibleScreenPixels / scale, -500 + visibleScreenPixels / scale);

        Assert.True(lines.Vertical.Count <= visibleScreenPixels / 8 + 2);
        Assert.True(lines.Horizontal.Count <= visibleScreenPixels / 8 + 2);
    }

    [Fact]
    public void Build_HugeRange_IsCapped()
    {
        var lines = GridLines.Build(1, 100, 0, 0, 1e9, 1e9);

        Assert.True(lines.Vertical.Count <= 2000);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-10, 100)]
    [InlineData(10, 0)]
    [InlineData(10, -100)]
    [InlineData(double.NaN, 100)]
    [InlineData(10, double.PositiveInfinity)]
    public void Build_UnusableSpacingOrZoom_GivesNoLines(double spacing, double zoomPercent)
    {
        var lines = GridLines.Build(spacing, zoomPercent, 0, 0, 100, 100);

        Assert.Empty(lines.Vertical);
        Assert.Empty(lines.Horizontal);
    }

    [Fact]
    public void Build_EmptyOrNonFiniteRange_GivesNoLines()
    {
        Assert.Empty(GridLines.Build(10, 100, 50, 0, 10, 100).Vertical);
        Assert.Empty(GridLines.Build(10, 100, double.NaN, 0, 10, 100).Vertical);
    }

    [Fact]
    public void BuildOnScreen_LinesStandAtTheScreenPositionsOfTheDocumentLines()
    {
        // The artboard origin is at (50, 30) on screen, 100% zoom, a canvas of 100 x 60 pixels. The grid covers the whole canvas, also left
        // of and above the artboard, so the lines at 0, 10, ... 100 pixels are the document lines -50 ... 50.
        var lines = GridLines.BuildOnScreen(10, 100, 50, 30, 100, 60);

        Assert.Equal([0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100], lines.Vertical);
        Assert.Equal([0, 10, 20, 30, 40, 50, 60], lines.Horizontal);
    }

    [Fact]
    public void BuildOnScreen_AtHalfZoom_LinesAreThinnedAndScaled()
    {
        var lines = GridLines.BuildOnScreen(10, 50, 0, 0, 100, 100);

        // 10 units are 5 pixels, so every 2nd line is drawn: 20 units = 10 pixels.
        Assert.Equal(10, lines.Vertical[1] - lines.Vertical[0], 9);
        Assert.Equal(0, lines.Vertical[0], 9);
    }

    [Fact]
    public void BuildOnScreen_OriginLeftOfTheCanvas_StillStartsAtTheFirstVisibleLine()
    {
        var lines = GridLines.BuildOnScreen(10, 100, -25, -25, 40, 40);

        Assert.Equal([-25 + 30, -25 + 40], lines.Vertical.Where(x => x >= 0).Take(2));
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(0, 0)]
    [InlineData(100, double.NaN)]
    public void BuildOnScreen_UnusableInput_GivesNoLines(double zoom, double origin)
    {
        var lines = GridLines.BuildOnScreen(10, zoom, origin, 0, 100, 100);

        Assert.Empty(lines.Vertical);
    }

    [Fact]
    public void BuildInScreenRect_GivesOnlyTheLinesInsideTheRectangle()
    {
        // Origin at (50, 30), 100% zoom: the rectangle 60..100 x 40..70 holds the document lines 10..50 and 10..40.
        var lines = GridLines.BuildInScreenRect(10, 100, 50, 30, 60, 40, 100, 70);

        Assert.Equal([60, 70, 80, 90, 100], lines.Vertical);
        Assert.Equal([40, 50, 60, 70], lines.Horizontal);
    }

    [Fact]
    public void BuildInScreenRect_UnusableInput_GivesNoLines()
    {
        Assert.Empty(GridLines.BuildInScreenRect(10, 100, double.NaN, 0, 0, 0, 10, 10).Vertical);
        Assert.Empty(GridLines.BuildInScreenRect(10, 0, 0, 0, 0, 0, 10, 10).Vertical);
    }
}
