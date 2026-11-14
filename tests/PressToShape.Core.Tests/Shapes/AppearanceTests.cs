using System;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class AppearanceTests
{
    [Fact]
    public void Default_Always_HasBlueFillDarkStrokeWidthOneAndFullOpacity()
    {
        var appearance = Appearance.Default;

        Assert.Equal(Color.FromRgb(0x4A, 0x90, 0xD9), appearance.Fill);
        Assert.Equal(Color.FromRgb(0x1F, 0x29, 0x33), appearance.Stroke);
        Assert.Equal(1, appearance.StrokeWidth);
        Assert.Equal(1, appearance.Opacity);
    }

    [Fact]
    public void With_NullFillAndStroke_MeansNone()
    {
        var appearance = Appearance.Default with { Fill = null, Stroke = null };

        Assert.Null(appearance.Fill);
        Assert.Null(appearance.Stroke);
    }

    [Fact]
    public void With_ChangedFill_LeavesOriginalUnchanged()
    {
        var original = Appearance.Default;

        var changed = original with { Fill = Color.FromRgb(1, 2, 3) };

        Assert.Equal(Color.FromRgb(0x4A, 0x90, 0xD9), original.Fill);
        Assert.Equal(Color.FromRgb(1, 2, 3), changed.Fill);
    }

    [Fact]
    public void Constructor_NewInstance_DefaultsToStrokeWidthOneAndFullOpacity()
    {
        var appearance = new Appearance();

        Assert.Equal(1, appearance.StrokeWidth);
        Assert.Equal(1, appearance.Opacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(100)]
    public void StrokeWidth_NotNegativeAndFinite_IsAccepted(double width)
    {
        var appearance = Appearance.Default with { StrokeWidth = width };

        Assert.Equal(width, appearance.StrokeWidth);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void StrokeWidth_Invalid_Throws(double width)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Appearance.Default with { StrokeWidth = width });

        Assert.Equal("StrokeWidth", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.25)]
    [InlineData(1)]
    public void Opacity_InRange_IsAccepted(double opacity)
    {
        var appearance = Appearance.Default with { Opacity = opacity };

        Assert.Equal(opacity, appearance.Opacity);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Opacity_Invalid_Throws(double opacity)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Appearance.Default with { Opacity = opacity });

        Assert.Equal("Opacity", ex.ParamName);
    }

    [Fact]
    public void Constructor_InvalidInitializer_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Appearance { StrokeWidth = -1 });
    }
}
