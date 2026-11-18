using System.Globalization;
using PressToShape.App.ViewModels.Editor;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class NumberParsingTests
{
    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

    [Theory]
    [InlineData("12,5", "de", 12.5)]
    [InlineData("12.5", "de", 12.5)]
    [InlineData("12.5", "en", 12.5)]
    [InlineData("-3", "en", -3)]
    [InlineData("+3", "en", 3)]
    [InlineData("  7  ", "en", 7)]
    public void TryParse_ValidText_GivesTheNumber(string text, string culture, double expected)
    {
        var ok = NumberParsing.TryParse(text, CultureInfo.GetCultureInfo(culture), out var value);

        Assert.True(ok);
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("1x")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    [InlineData("1,000.5")]
    public void TryParse_InvalidText_GivesFalseAndZero(string? text)
    {
        var ok = NumberParsing.TryParse(text, English, out var value);

        Assert.False(ok);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryParse_ThousandsSeparatorInGerman_IsRefused()
    {
        var ok = NumberParsing.TryParse("1.000,5", German, out _);

        Assert.False(ok);
    }
}
