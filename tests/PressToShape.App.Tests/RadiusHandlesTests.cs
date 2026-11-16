using System;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class RadiusHandlesTests
{
    // 200 x 100 box with its top left corner at (100, 50): center (200, 100), limit 50.
    private static RectangleShape Rect(CornerRadii radii = default, double rotation = 0) =>
        new() { X = 100, Y = 50, Width = 200, Height = 100, Radii = radii, Rotation = rotation };

    [Theory]
    [InlineData(100, 63.9, false)]
    [InlineData(100, 64, true)]
    [InlineData(100, 200, true)]
    [InlineData(50, 127, false)]
    [InlineData(50, 128, true)]
    [InlineData(200, 31.9, false)]
    [InlineData(200, 32, true)]
    public void IsShown_ShorterSideOnTheScreen_NeedsAtLeastSixtyFourPixels(double zoom, double side, bool expected)
    {
        var shape = new RectangleShape { Width = 1000, Height = side };

        Assert.Equal(expected, RadiusHandles.IsShown(shape, zoom));
    }

    [Fact]
    public void IsShown_UsesTheShorterSideEvenIfItIsTheWidth()
    {
        Assert.False(RadiusHandles.IsShown(new RectangleShape { Width = 40, Height = 1000 }, 100));
    }

    [Fact]
    public void Position_RadiusZero_IsTwentyFourPixelsInOnBothSides()
    {
        var shape = Rect();

        Assert.Equal((124.0, 74.0), Round(RadiusHandles.Position(shape, Corner.TopLeft, 100)));
        Assert.Equal((276.0, 74.0), Round(RadiusHandles.Position(shape, Corner.TopRight, 100)));
        Assert.Equal((276.0, 126.0), Round(RadiusHandles.Position(shape, Corner.BottomRight, 100)));
        Assert.Equal((124.0, 126.0), Round(RadiusHandles.Position(shape, Corner.BottomLeft, 100)));
    }

    [Fact]
    public void Position_RadiusZeroAtDoubleZoom_IsTwelveDocumentUnitsIn()
    {
        var (x, y) = RadiusHandles.Position(Rect(), Corner.TopLeft, 200);

        Assert.Equal(112, x, 9);
        Assert.Equal(62, y, 9);
    }

    [Fact]
    public void Position_LargestRadius_IsHalfTheShorterSideIn()
    {
        var shape = Rect(CornerRadii.Uniform(50));

        var (x, y) = RadiusHandles.Position(shape, Corner.TopLeft, 100);

        Assert.Equal(150, x, 9);
        Assert.Equal(100, y, 9);
    }

    [Fact]
    public void Position_RadiusInBetween_MovesInAStraightLine()
    {
        // Half of the largest radius: inset 24 + (50 - 24) / 2 = 37.
        var (x, y) = RadiusHandles.Position(Rect(CornerRadii.Uniform(25)), Corner.TopLeft, 100);

        Assert.Equal(137, x, 9);
        Assert.Equal(87, y, 9);
    }

    [Fact]
    public void Position_UsesTheRadiusOfThatCornerOnly()
    {
        var shape = Rect(CornerRadii.Zero.With(Corner.TopRight, 50));

        Assert.Equal((124.0, 74.0), Round(RadiusHandles.Position(shape, Corner.TopLeft, 100)));
        Assert.Equal((250.0, 100.0), Round(RadiusHandles.Position(shape, Corner.TopRight, 100)));
    }

    [Fact]
    public void Position_RadiusAboveTheLimit_IsPlacedAtTheLimit()
    {
        var shape = Rect(CornerRadii.Uniform(900));

        Assert.Equal((150.0, 100.0), Round(RadiusHandles.Position(shape, Corner.TopLeft, 100)));
    }

    [Fact]
    public void Position_RotatedByNinetyDegrees_TurnsAboutTheCenter()
    {
        // The top left handle is at (-76, -26) from the center (200, 100). Turned clockwise by 90 degrees, (x, y) becomes (-y, x).
        var (x, y) = RadiusHandles.Position(Rect(rotation: 90), Corner.TopLeft, 100);

        Assert.Equal(226, x, 9);
        Assert.Equal(24, y, 9);
    }

    [Fact]
    public void Hit_AtTheHandleCenter_FindsTheCorner()
    {
        var shape = Rect();

        Assert.Equal(Corner.TopLeft, RadiusHandles.Hit(shape, 124, 74, 100));
        Assert.Equal(Corner.BottomRight, RadiusHandles.Hit(shape, 276, 126, 100));
    }

    [Fact]
    public void Hit_InsideAndOutsideTheGrabArea_DependsOnTheGrabSize()
    {
        var shape = Rect();

        Assert.Equal(Corner.TopLeft, RadiusHandles.Hit(shape, 124 + 6.9, 74, 100));
        Assert.Null(RadiusHandles.Hit(shape, 124 + 7.1, 74, 100));
        Assert.Equal(Corner.TopLeft, RadiusHandles.Hit(shape, 124 + 17, 74, 100, RadiusHandles.TouchGrabSize));
        Assert.Null(RadiusHandles.Hit(shape, 124 + 19, 74, 100, RadiusHandles.TouchGrabSize));
    }

    [Fact]
    public void Hit_GrabAreaIsInScreenPixels_SoItShrinksInDocumentUnitsWhenZoomedIn()
    {
        var shape = Rect();
        var (hx, hy) = RadiusHandles.Position(shape, Corner.TopLeft, 200);

        Assert.Equal(Corner.TopLeft, RadiusHandles.Hit(shape, hx + 3.4, hy, 200));
        Assert.Null(RadiusHandles.Hit(shape, hx + 3.6, hy, 200));
    }

    [Fact]
    public void Hit_NothingNear_IsNull()
    {
        Assert.Null(RadiusHandles.Hit(Rect(), 200, 100, 100));
    }

    [Fact]
    public void Hit_TwoHandlesOverlap_TheCloserOneWins()
    {
        // At the largest radius of a square all four handles lie at the center. The first in the order wins an exact tie.
        var square = new RectangleShape { X = 0, Y = 0, Width = 100, Height = 100, Radii = CornerRadii.Uniform(50) };

        Assert.Equal(Corner.TopLeft, RadiusHandles.Hit(square, 50, 50, 100));

        // Without radii on one side the two handles are 52 px apart, so a point near one only hits that one.
        var shape = Rect();
        Assert.Equal(Corner.TopRight, RadiusHandles.Hit(shape, 270, 74, 100, RadiusHandles.TouchGrabSize));
    }

    [Fact]
    public void Hit_Rotated_UsesTheRotatedPositions()
    {
        var shape = Rect(rotation: 90);

        Assert.Equal(Corner.TopLeft, RadiusHandles.Hit(shape, 226, 24, 100));
        Assert.Null(RadiusHandles.Hit(shape, 124, 74, 100));
    }

    [Fact]
    public void RadiusAt_PointerAtTheHandle_GivesTheCurrentRadius()
    {
        foreach (var radius in new[] { 0.0, 10, 25, 50 })
        {
            var shape = Rect(CornerRadii.Uniform(radius));
            var (x, y) = RadiusHandles.Position(shape, Corner.TopLeft, 100);

            Assert.Equal(radius, RadiusHandles.RadiusAt(shape, Corner.TopLeft, x, y, 100, 0), 9);
        }
    }

    [Fact]
    public void RadiusAt_PressedAFewPixelsOffTheHandle_DoesNotJumpWithTheGrabOffset()
    {
        var shape = Rect(CornerRadii.Uniform(10));
        var (hx, hy) = RadiusHandles.Position(shape, Corner.TopLeft, 100);
        var pressX = hx + 4;
        var pressY = hy - 2;

        var offset = RadiusHandles.GrabOffset(shape, Corner.TopLeft, pressX, pressY, 100);

        Assert.Equal(10, RadiusHandles.RadiusAt(shape, Corner.TopLeft, pressX, pressY, 100, offset), 9);
    }

    [Fact]
    public void RadiusAt_DragInwardAndOutward_Follows()
    {
        var shape = Rect();
        var inward = RadiusHandles.RadiusAt(shape, Corner.TopLeft, 137, 87, 100, 0);
        var farther = RadiusHandles.RadiusAt(shape, Corner.TopLeft, 150, 100, 100, 0);

        Assert.Equal(25, inward, 9);
        Assert.Equal(50, farther, 9);
    }

    [Fact]
    public void RadiusAt_Limits_StayBetweenZeroAndHalfTheShorterSide()
    {
        var shape = Rect();

        Assert.Equal(0, RadiusHandles.RadiusAt(shape, Corner.TopLeft, 90, 40, 100, 0));
        Assert.Equal(0, RadiusHandles.RadiusAt(shape, Corner.TopLeft, 110, 60, 100, 0));
        Assert.Equal(50, RadiusHandles.RadiusAt(shape, Corner.TopLeft, 250, 120, 100, 0));
    }

    [Fact]
    public void RadiusAt_UsesTheMeanOfTheTwoInsets()
    {
        // 40 in from the left side and 34 in from the top side: mean 37, which is a radius of 25.
        Assert.Equal(25, RadiusHandles.RadiusAt(Rect(), Corner.TopLeft, 140, 84, 100, 0), 9);
    }

    [Theory]
    [InlineData(Corner.TopLeft)]
    [InlineData(Corner.TopRight)]
    [InlineData(Corner.BottomRight)]
    [InlineData(Corner.BottomLeft)]
    public void RadiusAt_EveryCornerWhenRotated_RoundTripsWithPosition(Corner corner)
    {
        var shape = Rect(CornerRadii.Uniform(30), rotation: 37);
        var (x, y) = RadiusHandles.Position(shape, corner, 150);

        Assert.Equal(30, RadiusHandles.RadiusAt(shape, corner, x, y, 150, 0), 6);
    }

    [Fact]
    public void RadiusAt_ZoomedIn_UsesTheMinimumInsetInDocumentUnits()
    {
        var shape = Rect();
        var (x, y) = RadiusHandles.Position(shape, Corner.TopLeft, 400);

        Assert.Equal(0, RadiusHandles.RadiusAt(shape, Corner.TopLeft, x, y, 400, 0), 9);
    }

    private static (double X, double Y) Round((double X, double Y) point) => (Math.Round(point.X, 6), Math.Round(point.Y, 6));
}
