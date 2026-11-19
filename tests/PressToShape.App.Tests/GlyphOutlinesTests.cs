using System;
using System.IO;
using System.Linq;
using Avalonia.Platform;
using PressToShape.App.Services;
using PressToShape.Core.Shapes;
using SkiaSharp;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class GlyphOutlinesTests
{
    private static SKPath Square()
    {
        var path = new SKPath();
        path.MoveTo(0, 0);
        path.LineTo(10, 0);
        path.LineTo(10, 10);
        path.LineTo(0, 10);
        path.Close();
        return path;
    }

    [Fact]
    public void FromPath_Square_IsOneClosedContourOfFourCornerNodes()
    {
        using var path = Square();

        var contour = Assert.Single(GlyphOutlines.FromPath(path, 0, 0));

        Assert.True(contour.IsClosed);
        Assert.Equal([(0d, 0d), (10d, 0d), (10d, 10d), (0d, 10d)], contour.Nodes.Select(node => (node.X, node.Y)));
        Assert.All(contour.Nodes, node => Assert.Null(node.OutX));
    }

    [Fact]
    public void FromPath_Offset_MovesEveryPoint()
    {
        using var path = Square();

        var contour = Assert.Single(GlyphOutlines.FromPath(path, 100, 200));

        Assert.Equal((100d, 200d), (contour.Nodes[0].X, contour.Nodes[0].Y));
        Assert.Equal((110d, 210d), (contour.Nodes[2].X, contour.Nodes[2].Y));
    }

    [Fact]
    public void FromPath_LastPointEqualsFirst_IsOneNodeAndKeepsTheArrivingHandle()
    {
        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.CubicTo(0, 5, 5, 10, 10, 10);
        path.CubicTo(15, 10, 0, 5, 0, 0);
        path.Close();

        var contour = Assert.Single(GlyphOutlines.FromPath(path, 0, 0));

        Assert.Equal(2, contour.Nodes.Count);
        Assert.Equal((0d, 5d), (contour.Nodes[0].OutX, contour.Nodes[0].OutY));
        Assert.Equal((0d, 5d), (contour.Nodes[0].InX, contour.Nodes[0].InY));
        Assert.Equal((5d, 10d), (contour.Nodes[1].InX, contour.Nodes[1].InY));
        Assert.Equal((15d, 10d), (contour.Nodes[1].OutX, contour.Nodes[1].OutY));
    }

    [Fact]
    public void FromPath_Quad_BecomesACubicWithHandlesAtTwoThirds()
    {
        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.QuadTo(30, 0, 30, 30);
        path.LineTo(0, 30);
        path.Close();

        var contour = Assert.Single(GlyphOutlines.FromPath(path, 0, 0));

        Assert.Equal(20d, contour.Nodes[0].OutX!.Value, 9);
        Assert.Equal(0d, contour.Nodes[0].OutY!.Value, 9);
        Assert.Equal(30d, contour.Nodes[1].InX!.Value, 9);
        Assert.Equal(10d, contour.Nodes[1].InY!.Value, 9);
    }

    [Fact]
    public void FromPath_Conic_IsCutIntoTwoCurves()
    {
        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.ConicTo(10, 0, 10, 10, 0.7071f);
        path.LineTo(0, 10);
        path.Close();

        var contour = Assert.Single(GlyphOutlines.FromPath(path, 0, 0));

        // Start, the middle of the arc, its end and the corner.
        Assert.Equal(4, contour.Nodes.Count);
        Assert.NotNull(contour.Nodes[0].OutX);
        Assert.NotNull(contour.Nodes[1].InX);
    }

    [Fact]
    public void FromPath_TwoContours_GiveTwoSubPathsInOrder()
    {
        using var path = Square();
        path.MoveTo(20, 20);
        path.LineTo(30, 20);
        path.LineTo(30, 30);
        path.Close();

        var contours = GlyphOutlines.FromPath(path, 0, 0);

        Assert.Equal(2, contours.Count);
        Assert.Equal(20, contours[1].Nodes[0].X);
        Assert.Equal(3, contours[1].Nodes.Count);
    }

    [Fact]
    public void FromPath_ContourWithOnePoint_IsLeftOut()
    {
        using var path = new SKPath();
        path.MoveTo(5, 5);
        path.Close();

        Assert.Empty(GlyphOutlines.FromPath(path, 0, 0));
    }

    [Fact]
    public void FromPath_EmptyPath_GivesNothing()
    {
        using var path = new SKPath();

        Assert.Empty(GlyphOutlines.FromPath(path, 0, 0));
    }

    [Theory]
    [InlineData("O", 2)]
    [InlineData("A", 2)]
    [InlineData("i", 2)]
    [InlineData("l", 1)]
    [InlineData("B", 3)]
    public void FromPath_InterGlyph_HasTheContoursOfItsShape(string letter, int expected)
    {
        // The real font file that the app ships: "O" is the outer ring and the counter, "i" the dot and the stem, "B" the outline and two counters.
        var contours = HeadlessUi.Run(() =>
        {
            using var stream = AssetLoader.Open(new Uri("avares://Avalonia.Fonts.Inter/Assets/Inter-Regular.ttf"));
            using var data = SKData.Create(stream);
            using var typeface = SKTypeface.FromData(data);
            using var font = new SKFont(typeface, 100) { Hinting = SKFontHinting.None };
            using var path = font.GetGlyphPath(typeface.GetGlyphs(letter)[0]);
            return GlyphOutlines.FromPath(path, 0, 0);
        });

        Assert.Equal(expected, contours.Count);
        Assert.All(contours, contour => Assert.True(contour.IsClosed));
    }

    [Fact]
    public void Outline_BlankText_IsNull()
    {
        Assert.Null(new AvaloniaTextOutliner().Outline(new TextShape { Text = "  \n " }));
        Assert.Null(new AvaloniaTextOutliner().Outline(new TextShape { Text = string.Empty }));
    }

    [Fact]
    public void Outline_Text_GivesClosedContoursInsideTheMeasuredBoxAtTheTextPosition()
    {
        var text = new TextShape { X = 100, Y = 200, Text = "Hello\nWorld", FontSize = 30, FontFamily = "Test Font" };

        var (contours, layout) = HeadlessUi.Run(() => (new AvaloniaTextOutliner().Outline(text), new AvaloniaTextMeasurer().Layout(text)));

        Assert.NotNull(contours);
        Assert.True(contours.Count >= 10, "Every letter has at least one contour.");
        Assert.All(contours, contour => Assert.True(contour.IsClosed));
        var nodes = contours.SelectMany(contour => contour.Nodes).ToArray();
        Assert.InRange(nodes.Min(node => node.X), 100, 130);
        Assert.InRange(nodes.Max(node => node.X), 100, 100 + layout.Width + 1);
        Assert.InRange(nodes.Min(node => node.Y), 200, 200 + layout.Height);
        Assert.InRange(nodes.Max(node => node.Y), 200, 200 + layout.Height + 1);
    }

    [Fact]
    public void Outline_SecondLine_SitsBelowTheFirst()
    {
        var one = new TextShape { Text = "H", FontSize = 30, FontFamily = "Test Font" };
        var two = one with { Text = "H\nH" };

        var (first, both) = HeadlessUi.Run(() => (new AvaloniaTextOutliner().Outline(one), new AvaloniaTextOutliner().Outline(two)));

        Assert.NotNull(first);
        Assert.NotNull(both);
        Assert.Equal(first.Count * 2, both.Count);
        var lineHeight = both[first.Count].Nodes[0].Y - both[0].Nodes[0].Y;
        Assert.True(lineHeight > 30 * 0.8, "The second line is about a line height lower.");
    }

    [Fact]
    public void Outline_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AvaloniaTextOutliner().Outline(null!));
    }
}
