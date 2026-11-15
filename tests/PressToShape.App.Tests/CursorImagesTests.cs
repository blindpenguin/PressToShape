using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class CursorImagesTests
{
    private static IEnumerable<CanvasCursorType> ImageTypes() =>
        Enum.GetValues<CanvasCursorType>().Where(t => t != CanvasCursorType.Arrow);

    [Theory]
    [InlineData(1.0, 32)]
    [InlineData(1.25, 32)]
    [InlineData(1.49, 32)]
    [InlineData(1.5, 64)]
    [InlineData(2.0, 64)]
    [InlineData(3.0, 64)]
    public void SizeFor_Scale_ReturnsNearestSize(double scale, int expected)
    {
        Assert.Equal(expected, CursorImages.SizeFor(scale));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-2.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void SizeFor_ScaleNotUsable_ReturnsSmallSize(double scale)
    {
        Assert.Equal(32, CursorImages.SizeFor(scale));
    }

    [Fact]
    public void Sizes_AreTheTwoImageSizes()
    {
        Assert.Equal([32, 64], CursorImages.Sizes);
    }

    [Fact]
    public void FileName_EveryTypeButArrow_HasAName()
    {
        foreach (var type in ImageTypes())
        {
            foreach (var size in CursorImages.Sizes)
            {
                Assert.False(string.IsNullOrEmpty(CursorImages.FileName(type, size)), $"{type} at {size}");
            }
        }
    }

    [Fact]
    public void FileName_Arrow_HasNone()
    {
        Assert.Null(CursorImages.FileName(CanvasCursorType.Arrow, 32));
    }

    [Fact]
    public void FileName_AllImages_AreUnique()
    {
        var names = ImageTypes().SelectMany(t => CursorImages.Sizes.Select(s => CursorImages.FileName(t, s))).ToList();

        Assert.Equal(22, names.Count);
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Theory]
    [InlineData("Crosshair", 32, "crosshair-32.png")]
    [InlineData("OpenHand", 64, "open-hand-64.png")]
    [InlineData("ClosedHand", 32, "closed-hand-32.png")]
    [InlineData("Magnifier", 64, "magnifier-64.png")]
    [InlineData("MagnifierMinus", 32, "magnifier-minus-32.png")]
    [InlineData("MagnifierMinus", 64, "magnifier-minus-64.png")]
    [InlineData("ResizeNorthSouth", 32, "resize-north-south-32.png")]
    [InlineData("ResizeWestEast", 32, "resize-west-east-32.png")]
    [InlineData("ResizeNorthWestSouthEast", 64, "resize-north-west-south-east-64.png")]
    [InlineData("ResizeNorthEastSouthWest", 64, "resize-north-east-south-west-64.png")]
    [InlineData("Rotate", 32, "rotate-32.png")]
    [InlineData("Eyedropper", 32, "eyedropper-32.png")]
    [InlineData("Eyedropper", 64, "eyedropper-64.png")]
    public void FileName_Type_MatchesTheToolOutput(string typeName, int size, string expected)
    {
        Assert.Equal(expected, CursorImages.FileName(Enum.Parse<CanvasCursorType>(typeName), size));
    }

    [Fact]
    public void Hotspot_At64_IsDoubleTheHotspotAt32()
    {
        foreach (var type in ImageTypes())
        {
            var small = CursorImages.Hotspot(type, 32);
            var large = CursorImages.Hotspot(type, 64);

            Assert.Equal((small.X * 2, small.Y * 2), large);
        }
    }

    [Fact]
    public void Hotspot_Magnifier_IsTheLensCenter()
    {
        Assert.Equal((15, 15), CursorImages.Hotspot(CanvasCursorType.Magnifier, 32));
        Assert.Equal((30, 30), CursorImages.Hotspot(CanvasCursorType.Magnifier, 64));
        Assert.Equal((15, 15), CursorImages.Hotspot(CanvasCursorType.MagnifierMinus, 32));
        Assert.Equal((30, 30), CursorImages.Hotspot(CanvasCursorType.MagnifierMinus, 64));
    }

    [Fact]
    public void Hotspot_Eyedropper_IsTheTipOfThePipette()
    {
        Assert.Equal((6, 26), CursorImages.Hotspot(CanvasCursorType.Eyedropper, 32));
        Assert.Equal((12, 52), CursorImages.Hotspot(CanvasCursorType.Eyedropper, 64));
    }

    [Fact]
    public void Hotspot_OtherTypes_AreTheImageCenter()
    {
        foreach (var type in ImageTypes().Where(t => t is not (CanvasCursorType.Magnifier or CanvasCursorType.MagnifierMinus or CanvasCursorType.Eyedropper)))
        {
            Assert.Equal((16, 16), CursorImages.Hotspot(type, 32));
            Assert.Equal((32, 32), CursorImages.Hotspot(type, 64));
        }
    }

    [Theory]
    [InlineData(16)]
    [InlineData(48)]
    [InlineData(0)]
    public void SizeDependentMembers_UnknownSize_Throw(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CursorImages.FileName(CanvasCursorType.Rotate, size));
        Assert.Throws<ArgumentOutOfRangeException>(() => CursorImages.Hotspot(CanvasCursorType.Rotate, size));
    }

    [Fact]
    public void Hotspot_Arrow_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CursorImages.Hotspot(CanvasCursorType.Arrow, 32));
    }
}
