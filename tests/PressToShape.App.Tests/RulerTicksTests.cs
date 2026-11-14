using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class RulerTicksTests
{
    [Fact]
    public void At100Percent_MajorTicksEvery100_MinorEvery10()
    {
        var ticks = RulerTicks.Compute(100, 0, 500);

        var majors = ticks.Where(t => t.Type == TickType.Major).ToList();
        Assert.Equal(new[] { 0.0, 100, 200, 300, 400, 500 }, majors.Select(t => t.ScreenPosition));
        Assert.Equal(new[] { "0", "100", "200", "300", "400", "500" }, majors.Select(t => t.Label));
        Assert.Equal(51, ticks.Count); // 0, 10, ..., 500
        Assert.Contains(ticks, t => t.Type == TickType.Medium && t.ScreenPosition == 50);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(150)]
    [InlineData(400)]
    [InlineData(1600)]
    [InlineData(6400)]
    public void MajorTicks_AreAtLeast60PixelsApart(double zoom)
    {
        var majors = RulerTicks.Compute(zoom, 0, 3000).Where(t => t.Type == TickType.Major).ToList();

        Assert.True(majors.Count >= 2);
        for (var i = 1; i < majors.Count; i++)
        {
            Assert.True(majors[i].ScreenPosition - majors[i - 1].ScreenPosition >= RulerTicks.MinMajorSpacing - 1e-6);
        }
    }

    [Theory]
    [InlineData(10, 1000)]
    [InlineData(50, 200)]
    [InlineData(400, 20)]
    [InlineData(6400, 1)]
    public void MajorStep_FollowsZoom(double zoom, double expectedStep)
    {
        var majors = RulerTicks.Compute(zoom, 0, 5000).Where(t => t.Type == TickType.Major).ToList();

        var stepInDocumentPixels = (majors[1].ScreenPosition - majors[0].ScreenPosition) / (zoom / 100);
        Assert.Equal(expectedStep, stepInDocumentPixels, precision: 6);
    }

    [Fact]
    public void Origin_ShiftsTicks_AndZeroIsLabeledAtOrigin()
    {
        var ticks = RulerTicks.Compute(100, 150, 500);

        var zero = ticks.Single(t => t.Label == "0");
        Assert.Equal(150, zero.ScreenPosition);
    }

    [Fact]
    public void Origin_InsideRuler_ShowsNegativeLabelsBeforeZero()
    {
        var ticks = RulerTicks.Compute(100, 250, 300);

        var labels = ticks.Where(t => t.Type == TickType.Major).Select(t => t.Label).ToList();
        Assert.Contains("-200", labels);
        Assert.Contains("0", labels);
        Assert.Equal(new[] { 50.0, 150, 250 }, ticks.Where(t => t.Type == TickType.Major).Select(t => t.ScreenPosition).Take(3));
    }

    [Fact]
    public void TickPositions_StayInsideTheRuler()
    {
        var ticks = RulerTicks.Compute(37, 41.5, 800);

        Assert.All(ticks, t => Assert.InRange(t.ScreenPosition, 0, 800));
    }

    [Theory]
    [InlineData(100, 0, 0)]
    [InlineData(100, 0, -10)]
    [InlineData(0, 0, 500)]
    [InlineData(-50, 0, 500)]
    [InlineData(double.NaN, 0, 500)]
    [InlineData(100, double.NaN, 500)]
    [InlineData(100, 0, double.PositiveInfinity)]
    public void InvalidInput_ReturnsNoTicks(double zoom, double origin, double length)
    {
        Assert.Empty(RulerTicks.Compute(zoom, origin, length));
    }

    [Fact]
    public void HugeLength_IsCapped()
    {
        var ticks = RulerTicks.Compute(100, 0, 1e9);

        Assert.True(ticks.Count <= RulerTicks.MaxTickCount);
    }

    [Fact]
    public void Output_IsDeterministic()
    {
        var first = RulerTicks.Compute(75, 12, 640);
        var second = RulerTicks.Compute(75, 12, 640);

        Assert.Equal(first, second);
    }
}
