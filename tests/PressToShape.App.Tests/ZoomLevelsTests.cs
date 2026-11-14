using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class ZoomLevelsTests
{
    [Theory]
    [InlineData(1, 10)]
    [InlineData(10, 10)]
    [InlineData(100, 100)]
    [InlineData(6400, 6400)]
    [InlineData(10000, 6400)]
    [InlineData(double.NaN, 100)]
    public void Clamp_KeepsValueInsideAllowedRange(double input, double expected)
    {
        Assert.Equal(expected, ZoomLevels.Clamp(input));
    }

    [Theory]
    [InlineData(100, 150)]
    [InlineData(10, 25)]
    [InlineData(3200, 6400)]
    [InlineData(6400, 6400)]
    [InlineData(120, 150)]
    [InlineData(5, 10)]
    public void NextIn_ReturnsNextPresetAbove(double current, double expected)
    {
        Assert.Equal(expected, ZoomLevels.NextIn(current));
    }

    [Theory]
    [InlineData(100, 75)]
    [InlineData(25, 10)]
    [InlineData(10, 10)]
    [InlineData(6400, 3200)]
    [InlineData(120, 100)]
    [InlineData(9000, 6400)]
    public void NextOut_ReturnsNextPresetBelow(double current, double expected)
    {
        Assert.Equal(expected, ZoomLevels.NextOut(current));
    }

    [Fact]
    public void Presets_AreAscending_AndWithinLimits()
    {
        Assert.Equal(ZoomLevels.Min, ZoomLevels.Presets[0]);
        Assert.Equal(ZoomLevels.Max, ZoomLevels.Presets[^1]);
        Assert.Equal(ZoomLevels.Presets.OrderBy(p => p), ZoomLevels.Presets);
    }

    [Fact]
    public void FitToViewport_FitsArtboardWithMargin()
    {
        // available = 1000 x 600, artboard 500 x 500 -> limited by height: 600 / 500 = 120 percent
        var fit = ZoomLevels.FitToViewport(1048, 648, 500, 500, margin: 24);

        Assert.NotNull(fit);
        Assert.Equal(120, fit!.Value, precision: 6);
    }

    [Fact]
    public void FitToViewport_ClampsToMaximum()
    {
        var fit = ZoomLevels.FitToViewport(100000, 100000, 1, 1);

        Assert.Equal(ZoomLevels.Max, fit);
    }

    [Theory]
    [InlineData(0, 100, 10, 10)]
    [InlineData(100, 0, 10, 10)]
    [InlineData(100, 100, 0, 10)]
    [InlineData(100, 100, 10, 0)]
    [InlineData(-5, 100, 10, 10)]
    public void FitToViewport_NonPositiveSize_ReturnsNull(double vw, double vh, double aw, double ah)
    {
        Assert.Null(ZoomLevels.FitToViewport(vw, vh, aw, ah));
    }

    [Fact]
    public void FitToViewport_ViewportSmallerThanMargin_ClampsToMinimum()
    {
        var fit = ZoomLevels.FitToViewport(30, 30, 1200, 800, margin: 24);

        Assert.Equal(ZoomLevels.Min, fit);
    }
}
