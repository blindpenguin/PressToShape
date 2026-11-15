using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Media;
using Xunit;

namespace PressToShape.App.Tests;

// The Properties tab sets ColorView.HexInputAlphaPosition to Trailing, so the hex field reads and writes #RRGGBBAA like the project file.
// ColorView uses ColorToHexConverter for its hex text. These tests pin down what that converter accepts with the same settings.
public class ColorHexInputTests
{
    private static ColorToHexConverter Converter() => new() { IsAlphaVisible = true, AlphaPosition = AlphaComponentPosition.Trailing };

    private static object? Parse(string text) =>
        Converter().ConvertBack(text, typeof(Color), null, CultureInfo.InvariantCulture);

    [Fact]
    public void Convert_ColorWithAlpha_WritesRrggbbaaWithoutTheHash()
    {
        var text = Converter().Convert(Color.FromArgb(0x80, 0x11, 0x22, 0x33), typeof(string), null, CultureInfo.InvariantCulture);

        // ColorView puts the "#" in front itself.
        Assert.Equal("11223380", text);
    }

    [Fact]
    public void ConvertBack_EightDigits_ReadsTheAlphaLast() =>
        Assert.Equal(Color.FromArgb(0x80, 0x11, 0x22, 0x33), Assert.IsType<Color>(Parse("#11223380")));

    [Fact]
    public void ConvertBack_SixDigits_IsOpaque() =>
        Assert.Equal(Color.FromArgb(0xFF, 0x11, 0x22, 0x33), Assert.IsType<Color>(Parse("#112233")));

    [Theory]
    [InlineData("red!")]
    [InlineData("#11223")]
    [InlineData("#GG0000")]
    public void ConvertBack_InvalidText_IsNotAColor(string text) =>
        Assert.IsNotType<Color>(Parse(text));
}
