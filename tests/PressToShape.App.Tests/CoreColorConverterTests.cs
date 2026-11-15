using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using PressToShape.App.Views.Editor;
using Xunit;
using AvaloniaColor = Avalonia.Media.Color;
using CoreColor = PressToShape.Core.Shapes.Color;

namespace PressToShape.App.Tests;

public class CoreColorConverterTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    [Fact]
    public void Convert_KeepsAllChannelsIncludingAlpha()
    {
        var result = CoreColorConverter.Instance.Convert(new CoreColor(10, 20, 30, 40), typeof(AvaloniaColor), null, Culture);

        Assert.Equal(AvaloniaColor.FromArgb(40, 10, 20, 30), result);
    }

    [Fact]
    public void ConvertBack_KeepsAllChannelsIncludingAlpha()
    {
        var result = CoreColorConverter.Instance.ConvertBack(AvaloniaColor.FromArgb(200, 1, 2, 3), typeof(CoreColor), null, Culture);

        Assert.Equal(new CoreColor(1, 2, 3, 200), result);
    }

    [Fact]
    public void RoundTrip_EveryAlphaAndSampleColors_IsLossless()
    {
        foreach (var alpha in new byte[] { 0, 1, 127, 128, 254, 255 })
        {
            foreach (var color in new[] { new CoreColor(0, 0, 0, alpha), new CoreColor(255, 255, 255, alpha), new CoreColor(74, 144, 217, alpha) })
            {
                var back = CoreColorConverter.Instance.ConvertBack(CoreColorConverter.Instance.Convert(color, typeof(AvaloniaColor), null, Culture), typeof(CoreColor), null, Culture);

                Assert.Equal(color, back);
            }
        }
    }

    [Fact]
    public void WrongInput_GivesNull()
    {
        Assert.Null(CoreColorConverter.Instance.Convert("red", typeof(AvaloniaColor), null, Culture));
        Assert.Null(CoreColorConverter.Instance.Convert(null, typeof(AvaloniaColor), null, Culture));
        Assert.Null(CoreColorConverter.Instance.ConvertBack(5, typeof(CoreColor), null, Culture));
    }

    [Fact]
    public void BrushConverter_MakesASolidBrushWithTheColor()
    {
        var brush = Assert.IsType<ImmutableSolidColorBrush>(CoreColorBrushConverter.Instance.Convert(new CoreColor(9, 8, 7, 100), typeof(IBrush), null, Culture));

        Assert.Equal(AvaloniaColor.FromArgb(100, 9, 8, 7), brush.Color);
        Assert.Null(CoreColorBrushConverter.Instance.Convert(null, typeof(IBrush), null, Culture));
    }

    [Fact]
    public void BrushConverter_DoesNotConvertBack()
    {
        Assert.Throws<System.NotSupportedException>(() => CoreColorBrushConverter.Instance.ConvertBack(null, typeof(CoreColor), null, Culture));
    }
}
