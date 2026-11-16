using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class RingGeometryTests
{
    private const double Inner = 20;
    private const double Outer = 100;

    [Theory]
    [InlineData(2, 0, -90, 180)]
    [InlineData(2, 1, 90, 180)]
    [InlineData(4, 0, -45, 90)]
    [InlineData(4, 3, 225, 90)]
    public void WedgeAngles_EvenCounts_AreCenteredAtTheTopAndClockwise(int count, int index, double start, double sweep)
    {
        var angles = RingGeometry.WedgeAngles(index, count);

        Assert.Equal(start, angles.Start, 9);
        Assert.Equal(sweep, angles.Sweep, 9);
    }

    [Fact]
    public void WedgeAngles_SevenSlots_AllWedgesTogetherFillTheCircle()
    {
        var angles = Enumerable.Range(0, 7).Select(i => RingGeometry.WedgeAngles(i, 7)).ToList();

        Assert.Equal(360, angles.Sum(a => a.Sweep), 9);
        Assert.Equal(-180.0 / 7, angles[0].Start, 9);
        Assert.All(angles.Zip(angles.Skip(1)), pair => Assert.Equal(pair.First.Start + pair.First.Sweep, pair.Second.Start, 9));
    }

    [Fact]
    public void WedgeAngles_NoSlots_IsEmpty()
    {
        Assert.Equal((0.0, 0.0), RingGeometry.WedgeAngles(0, 0));
    }

    [Theory]
    [InlineData(0, -60, 0)]
    [InlineData(60, 0, 1)]
    [InlineData(0, 60, 2)]
    [InlineData(-60, 0, 3)]
    public void HitTest_FourSlots_PointInsideAWedge_HitsIt(double dx, double dy, int expected)
    {
        Assert.Equal(RingHit.Slot(expected), RingGeometry.HitTest(dx, dy, 4, Inner, Outer));
    }

    [Fact]
    public void HitTest_SevenSlots_EveryWedgeCenterHitsItsOwnWedge()
    {
        for (var i = 0; i < 7; i++)
        {
            var (x, y) = RingGeometry.SlotCenter(i, 7, Inner, Outer);

            Assert.Equal(RingHit.Slot(i), RingGeometry.HitTest(x, y, 7, Inner, Outer));
        }
    }

    [Fact]
    public void HitTest_OnTheLineBetweenTwoWedges_TakesTheNextOneClockwise()
    {
        // Four slots: the line between wedge 0 and wedge 1 points to the upper right at 45 degrees.
        Assert.Equal(RingHit.Slot(1), RingGeometry.HitTest(50, -50, 4, Inner, Outer));
    }

    [Fact]
    public void HitTest_JustLeftOfTheTop_StillHitsTheFirstWedge()
    {
        Assert.Equal(RingHit.Slot(0), RingGeometry.HitTest(-1, -60, 7, Inner, Outer));
    }

    [Fact]
    public void HitTest_CloserThanTheInnerRadius_HitsTheCenter()
    {
        Assert.True(RingGeometry.HitTest(0, 0, 4, Inner, Outer).IsCenter);
        Assert.True(RingGeometry.HitTest(0, -19.9, 4, Inner, Outer).IsCenter);
    }

    [Fact]
    public void HitTest_ExactlyOnTheRadii_BelongsToTheWedge()
    {
        Assert.Equal(RingHit.Slot(0), RingGeometry.HitTest(0, -Inner, 4, Inner, Outer));
        Assert.Equal(RingHit.Slot(0), RingGeometry.HitTest(0, -Outer, 4, Inner, Outer));
    }

    [Fact]
    public void HitTest_FartherThanTheOuterRadius_IsOutside()
    {
        Assert.True(RingGeometry.HitTest(0, -100.1, 4, Inner, Outer).IsOutside);
    }

    [Fact]
    public void HitTest_NoSlots_HitsNothingExceptTheCenter()
    {
        Assert.True(RingGeometry.HitTest(0, -60, 0, Inner, Outer).IsOutside);
        Assert.True(RingGeometry.HitTest(0, 0, 0, Inner, Outer).IsCenter);
    }

    [Fact]
    public void HitTest_NotANumber_IsOutside()
    {
        Assert.True(RingGeometry.HitTest(double.NaN, 5, 4, Inner, Outer).IsOutside);
        Assert.True(RingGeometry.HitTest(5, double.PositiveInfinity, 4, Inner, Outer).IsOutside);
    }

    [Fact]
    public void SlotCenter_FirstSlot_IsAboveTheCenterAtTheMiddleRadius()
    {
        var (x, y) = RingGeometry.SlotCenter(0, 7, Inner, Outer);

        Assert.Equal(0, x, 9);
        Assert.Equal(-60, y, 9);
    }

    [Fact]
    public void SlotCenter_SecondOfFour_IsRightOfTheCenter()
    {
        var (x, y) = RingGeometry.SlotCenter(1, 4, Inner, Outer);

        Assert.Equal(60, x, 9);
        Assert.Equal(0, y, 9);
    }

    [Fact]
    public void ClampCenter_InsideTheArea_IsUnchanged()
    {
        Assert.Equal((300.0, 200.0), RingGeometry.ClampCenter(300, 200, Outer, 800, 600));
    }

    [Theory]
    [InlineData(10, 300, 100, 300)]
    [InlineData(790, 300, 700, 300)]
    [InlineData(400, 5, 400, 100)]
    [InlineData(400, 590, 400, 500)]
    public void ClampCenter_NearAnEdge_MovesTheRingInside(double x, double y, double expectedX, double expectedY)
    {
        Assert.Equal((expectedX, expectedY), RingGeometry.ClampCenter(x, y, Outer, 800, 600));
    }

    [Fact]
    public void ClampCenter_AreaSmallerThanTheRing_CentersTheRing()
    {
        Assert.Equal((90.0, 75.0), RingGeometry.ClampCenter(10, 400, Outer, 180, 150));
    }
}
