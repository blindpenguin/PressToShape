using System;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class ViewGeometryTests
{
    [Theory]
    [InlineData(1000, 400, 0, 300)]
    [InlineData(1000, 400, 50, 350)]
    [InlineData(1000, 1600, 0, -300)]
    [InlineData(1000, 1600, -40, -340)]
    [InlineData(1000, 1000, 0, 0)]
    public void Origin_CentersTheArtboardThenAppliesTheOffset(double viewport, double artboard, double pan, double expected)
    {
        Assert.Equal(expected, ViewGeometry.Origin(viewport, artboard, pan), precision: 9);
    }

    [Theory]
    [InlineData(1000, 400, 600)]   // (1000 + 400) / 2 - 100
    [InlineData(1000, 1600, 1200)] // (1000 + 1600) / 2 - 100
    [InlineData(500, 50, 225)]     // artboard smaller than 100: all of it stays, (500 + 50) / 2 - 50
    public void PanLimit_KeepsTheMinimumVisible(double viewport, double artboard, double expected)
    {
        Assert.Equal(expected, ViewGeometry.PanLimit(viewport, artboard), precision: 9);
    }

    [Fact]
    public void PanLimit_AtTheLimit_LeavesExactlyTheMinimumVisible()
    {
        const double viewport = 1000;
        const double artboard = 1600;
        var limit = ViewGeometry.PanLimit(viewport, artboard);

        var originRight = ViewGeometry.Origin(viewport, artboard, limit);
        var originLeft = ViewGeometry.Origin(viewport, artboard, -limit);

        Assert.Equal(viewport - ViewGeometry.MinimumVisible, originRight, precision: 9); // 100 px visible at the right edge
        Assert.Equal(ViewGeometry.MinimumVisible - artboard, originLeft, precision: 9);  // 100 px visible at the left edge
    }

    [Fact]
    public void PanLimit_SmallArtboard_KeepsItsWholeSizeVisible()
    {
        const double viewport = 800;
        const double artboard = 60;
        var limit = ViewGeometry.PanLimit(viewport, artboard);

        var origin = ViewGeometry.Origin(viewport, artboard, limit);

        Assert.Equal(viewport - artboard, origin, precision: 9);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(double.NaN)]
    public void PanLimit_UnknownViewport_HasNoLimit(double viewport)
    {
        Assert.Equal(double.PositiveInfinity, ViewGeometry.PanLimit(viewport, 500));
        Assert.Equal(12345, ViewGeometry.ClampPan(12345, viewport, 500));
    }

    [Fact]
    public void PanLimit_ViewportSmallerThanTheMargin_IsNeverNegative()
    {
        Assert.True(ViewGeometry.PanLimit(40, 80) >= 0);
        Assert.True(ViewGeometry.PanLimit(1, 1) >= 0);
        Assert.True(ViewGeometry.PanLimit(30, 0) >= 0);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(500, 500)]
    [InlineData(-500, -500)]
    [InlineData(5000, 600)]
    [InlineData(-5000, -600)]
    public void ClampPan_LimitsTheOffset(double pan, double expected)
    {
        Assert.Equal(expected, ViewGeometry.ClampPan(pan, 1000, 400), precision: 9);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ClampPan_NonFiniteOffset_BecomesZero(double pan)
    {
        Assert.Equal(0, ViewGeometry.ClampPan(pan, 1000, 400));
    }

    public static TheoryData<double, double> ZoomPairs() => new()
    {
        { 100, 200 }, { 100, 50 }, { 50, 51 }, { 10, 6400 }, { 6400, 10 }, { 81.92, 133.33 }, { 300, 300 }, { 25, 800 },
    };

    [Theory]
    [MemberData(nameof(ZoomPairs))]
    public void PanAfterZoom_KeepsTheDocumentPointUnderTheAnchor(double oldZoom, double newZoom)
    {
        foreach (var viewport in new[] { 400.0, 1000.0, 1800.0 })
        {
            foreach (var pan in new[] { 0.0, 120.0, -340.5, 800.0 })
            {
                foreach (var anchor in new[] { 0.0, 1.0, viewport / 2, viewport - 1, viewport, 123.4 })
                {
                    const double artboardDocument = 1200;
                    var newPan = ViewGeometry.PanAfterZoom(pan, viewport, oldZoom, newZoom, artboardDocument, anchor);

                    var before = (anchor - ViewGeometry.Origin(viewport, artboardDocument * oldZoom / 100, pan)) / (oldZoom / 100);
                    var after = (anchor - ViewGeometry.Origin(viewport, artboardDocument * newZoom / 100, newPan)) / (newZoom / 100);

                    Assert.Equal(before, after, precision: 4);
                }
            }
        }
    }

    [Fact]
    public void PanAfterZoom_ZoomingAtTheCenterOfACenteredArtboard_StaysCentered()
    {
        var newPan = ViewGeometry.PanAfterZoom(0, 1000, 100, 250, 800, 500);

        Assert.Equal(0, newPan, precision: 9);
    }

    [Theory]
    [InlineData(0, 100, 5)]
    [InlineData(double.NaN, 100, 5)]
    [InlineData(100, 0, 5)]
    [InlineData(100, double.NaN, 5)]
    [InlineData(100, 200, double.NaN)]
    public void PanAfterZoom_InvalidInput_IsFinite(double oldZoom, double newZoom, double anchor)
    {
        var result = ViewGeometry.PanAfterZoom(7, 1000, oldZoom, newZoom, 800, anchor);

        Assert.True(double.IsFinite(result));
    }

    [Theory]
    [InlineData(100, 0, 100)]
    [InlineData(100, 1, 110)]
    [InlineData(100, -1, 100 / 1.1)]
    [InlineData(100, 3, 133.1)]
    [InlineData(100, -3, 100 / 1.331)]
    public void WheelZoom_UsesTheFactorPerNotch(double zoom, double delta, double expected)
    {
        Assert.Equal(expected, ViewGeometry.WheelZoom(zoom, delta), precision: 6);
    }

    [Fact]
    public void WheelZoom_FractionalDelta_ScalesTheFactorInProportion()
    {
        Assert.Equal(100 * Math.Pow(1.1, 0.1), ViewGeometry.WheelZoom(100, 0.1), precision: 9);
        Assert.Equal(ViewGeometry.WheelZoom(ViewGeometry.WheelZoom(100, 0.5), 0.5), ViewGeometry.WheelZoom(100, 1), precision: 9);
    }

    [Theory]
    [InlineData(ZoomLevels.Max, 5, ZoomLevels.Max)]
    [InlineData(ZoomLevels.Min, -5, ZoomLevels.Min)]
    [InlineData(100, 1e9, ZoomLevels.Max)]
    [InlineData(100, -1e9, ZoomLevels.Min)]
    public void WheelZoom_StopsAtTheZoomLimits(double zoom, double delta, double expected)
    {
        Assert.Equal(expected, ViewGeometry.WheelZoom(zoom, delta));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void WheelZoom_NonFiniteDelta_LeavesTheZoomUnchanged(double delta)
    {
        Assert.Equal(150, ViewGeometry.WheelZoom(150, delta));
    }

    [Fact]
    public void RandomZoomPanAndResizeSteps_KeepTheArtboardVisibleAndEverythingFinite()
    {
        var random = new Random(20260115);
        double zoom = 100;
        double pan = 0;
        double viewport = 1000;
        const double artboardDocument = 1200;

        for (var i = 0; i < 5000; i++)
        {
            switch (random.Next(3))
            {
                case 0:
                    var newZoom = ViewGeometry.WheelZoom(zoom, (random.NextDouble() - 0.5) * 8);
                    var anchor = random.NextDouble() * viewport;
                    pan = ViewGeometry.PanAfterZoom(pan, viewport, zoom, newZoom, artboardDocument, anchor);
                    zoom = newZoom;
                    break;
                case 1:
                    pan += (random.NextDouble() - 0.5) * 3000;
                    break;
                default:
                    viewport = 50 + random.NextDouble() * 3000;
                    break;
            }

            var artboard = artboardDocument * zoom / 100;
            pan = ViewGeometry.ClampPan(pan, viewport, artboard);

            Assert.True(double.IsFinite(pan));
            var origin = ViewGeometry.Origin(viewport, artboard, pan);
            var overlap = Math.Min(origin + artboard, viewport) - Math.Max(origin, 0);
            var required = Math.Min(ViewGeometry.MinimumVisible, Math.Min(artboard, viewport));
            Assert.True(overlap >= required - 1e-6, $"overlap {overlap} < {required} at step {i}");
        }
    }
}
