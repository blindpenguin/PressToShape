using System;
using System.IO;
using System.Linq;
using SkiaSharp;
using Xunit;

namespace PressToShape.CursorImages.Tests;

public class CursorRendererTests
{
    // The names of CursorImages in the app, copied here because the tool does not reference src/.
    private static readonly string[] AppNames =
    [
        "crosshair", "open-hand", "closed-hand", "magnifier", "magnifier-minus", "resize-north-south", "resize-west-east",
        "resize-north-west-south-east", "resize-north-east-south-west", "rotate", "eyedropper",
    ];

    private static SKPath Icon(string name) => SvgIcon.Load(RepoPaths.Combine($"assets-src/cursors/lucide/{name}.svg"));

    private static SKBitmap Render(string icon, int size)
    {
        using var path = Icon(icon);
        return CursorRenderer.Render(path, size);
    }

    [Theory]
    [InlineData(32)]
    [InlineData(64)]
    public void Render_Size_IsExact(int size)
    {
        using var bitmap = Render("rotate-cw", size);

        Assert.Equal(size, bitmap.Width);
        Assert.Equal(size, bitmap.Height);
    }

    [Fact]
    public void Render_Background_IsTransparent()
    {
        using var bitmap = Render("move-horizontal", 32);

        Assert.Equal(0, bitmap.GetPixel(0, 0).Alpha);
        Assert.Equal(0, bitmap.GetPixel(16, 4).Alpha);
    }

    [Theory]
    [InlineData(32)]
    [InlineData(64)]
    public void Render_Crosshair_DrawsTheRingAndLeavesTheMiddleOpen(int size)
    {
        using var bitmap = Render("crosshair", size);
        var scale = size / 32.0;

        // The ring has radius 10 around the icon center (12, 12). The image starts 4 units before the icon.
        var onRing = bitmap.GetPixel((int)((12 + 10 + 4) * scale), (int)((12 + 4) * scale));
        var middle = bitmap.GetPixel(size / 2, size / 2);

        Assert.True(onRing.Alpha > 0);
        Assert.Equal(0, middle.Alpha);
    }

    [Theory]
    [InlineData(32)]
    [InlineData(64)]
    public void Render_OutermostPixels_AreTransparentForEveryIcon(int size)
    {
        foreach (var entry in CursorSet.Entries)
        {
            using var bitmap = Render(entry.Icon, size);

            for (var i = 0; i < size; i++)
            {
                Assert.True(bitmap.GetPixel(i, 0).Alpha == 0, $"{entry.Icon} top row, x {i}");
                Assert.True(bitmap.GetPixel(i, size - 1).Alpha == 0, $"{entry.Icon} bottom row, x {i}");
                Assert.True(bitmap.GetPixel(0, i).Alpha == 0, $"{entry.Icon} left column, y {i}");
                Assert.True(bitmap.GetPixel(size - 1, i).Alpha == 0, $"{entry.Icon} right column, y {i}");
            }
        }
    }

    [Fact]
    public void Render_Line_IsWhiteInTheMiddleAndDarkAtTheRim()
    {
        // The horizontal line of move-horizontal runs through the middle of the icon, at y = 12 units.
        using var bitmap = Render("move-horizontal", 64);

        var middle = bitmap.GetPixel(32, 32);
        var rim = bitmap.GetPixel(32, 35);

        Assert.Equal(new SKColor(255, 255, 255, 255), middle);
        Assert.Equal(255, rim.Alpha);
        Assert.True(rim.Red < 40 && rim.Green < 40 && rim.Blue < 40, $"The rim pixel is {rim}.");
    }

    [Fact]
    public void Render_SameIconTwice_GivesIdenticalPixels()
    {
        using var first = Render("grab", 64);
        using var second = Render("grab", 64);

        Assert.True(first.Bytes.AsSpan().SequenceEqual(second.Bytes));
    }

    [Fact]
    public void Render_LargeImage_HasTheSameShapeAsTheSmallImageAtDoubleSize()
    {
        foreach (var entry in CursorSet.Entries)
        {
            using var small = Render(entry.Icon, 32);
            using var large = Render(entry.Icon, 64);

            long difference = 0;
            for (var y = 0; y < 32; y++)
            {
                for (var x = 0; x < 32; x++)
                {
                    var block = large.GetPixel(2 * x, 2 * y).Alpha + large.GetPixel(2 * x + 1, 2 * y).Alpha
                        + large.GetPixel(2 * x, 2 * y + 1).Alpha + large.GetPixel(2 * x + 1, 2 * y + 1).Alpha;
                    difference += Math.Abs(block / 4 - small.GetPixel(x, y).Alpha);
                }
            }

            var meanDifference = difference / (32.0 * 32.0);
            Assert.True(meanDifference < 6, $"{entry.Icon}: the mean alpha difference is {meanDifference:F2} of 255.");
        }
    }

    [Fact]
    public void Entries_HaveTheNamesOfTheApp()
    {
        Assert.Equal(AppNames.Order(StringComparer.Ordinal), CursorSet.Entries.Select(e => e.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Entries_UseUniqueNamesAndExistingIcons()
    {
        Assert.Equal(11, CursorSet.Entries.Count);
        Assert.Equal(11, CursorSet.Entries.Select(e => e.Name).Distinct().Count());
        foreach (var entry in CursorSet.Entries)
        {
            Assert.True(File.Exists(RepoPaths.Combine($"assets-src/cursors/lucide/{entry.Icon}.svg")), entry.Icon);
        }
    }

    [Fact]
    public void Sizes_AreTheTwoImageSizes()
    {
        Assert.Equal([32, 64], CursorSet.Sizes);
    }
}
