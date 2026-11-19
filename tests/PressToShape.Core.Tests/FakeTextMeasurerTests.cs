using System.Linq;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests;

public class FakeTextMeasurerTests
{
    // Font size 10: a character is 6 wide, a line is 12 high, the first baseline is 10.
    private static TextShape Text(string content, bool isArea = false, double width = 0, TextAlignType alignment = TextAlignType.Left) =>
        new() { Text = content, FontSize = 10, IsArea = isArea, Width = width, Alignment = alignment };

    private static string[] Lines(TextLayoutResult result) => result.Lines.Select(line => line.Text).ToArray();

    [Fact]
    public void Layout_EmptyText_HasNoLines()
    {
        var result = new FakeTextMeasurer().Layout(Text(string.Empty));

        Assert.Empty(result.Lines);
        Assert.Equal(0, result.Width);
        Assert.Equal(0, result.Height);
    }

    [Fact]
    public void Layout_PointText_OneLineHasSizeOfItsCharacters()
    {
        var result = new FakeTextMeasurer().Layout(Text("Hello"));

        Assert.Equal(["Hello"], Lines(result));
        Assert.Equal(30, result.Width);
        Assert.Equal(12, result.Height);
        Assert.Equal(10, result.Lines[0].Baseline);
    }

    [Fact]
    public void Layout_Newline_BreaksTheLine()
    {
        var result = new FakeTextMeasurer().Layout(Text("ab\ncde"));

        Assert.Equal(["ab", "cde"], Lines(result));
        Assert.Equal(22, result.Lines[1].Baseline);
        Assert.Equal(18, result.Width);
        Assert.Equal(24, result.Height);
    }

    [Fact]
    public void Layout_PointText_DoesNotWrap()
    {
        var result = new FakeTextMeasurer().Layout(Text("aaa bbb ccc", isArea: false, width: 20));

        Assert.Single(result.Lines);
    }

    [Fact]
    public void Layout_AreaText_WrapsOnSpacesAtTheBoxWidth()
    {
        // 6 per character: "aaa bbb" is 42 wide, so it does not fit into 40.
        var result = new FakeTextMeasurer().Layout(Text("aaa bbb ccc", isArea: true, width: 40));

        Assert.Equal(["aaa", "bbb", "ccc"], Lines(result));
    }

    [Fact]
    public void Layout_AreaTextNarrowBox_KeepsOneWordPerLine()
    {
        var result = new FakeTextMeasurer().Layout(Text("abcdef gh", isArea: true, width: 1));

        Assert.Equal(["abcdef", "gh"], Lines(result));
    }

    [Theory]
    [InlineData(TextAlignType.Left, 0)]
    [InlineData(TextAlignType.Center, 6)]
    [InlineData(TextAlignType.Right, 12)]
    public void Layout_PointText_AlignsShorterLinesToTheWidest(TextAlignType alignment, double expectedX)
    {
        var result = new FakeTextMeasurer().Layout(Text("abcd\nab", alignment: alignment));

        Assert.Equal(expectedX, result.Lines[1].X);
        Assert.Equal(0, result.Lines[0].X);
    }

    [Theory]
    [InlineData(TextAlignType.Left, 0)]
    [InlineData(TextAlignType.Center, 44)]
    [InlineData(TextAlignType.Right, 88)]
    public void Layout_AreaText_AlignsToTheBox(TextAlignType alignment, double expectedX)
    {
        // "ab" is 12 wide in a 100 wide box.
        var result = new FakeTextMeasurer().Layout(Text("ab", isArea: true, width: 100, alignment: alignment));

        Assert.Equal(expectedX, result.Lines[0].X);
    }

    [Fact]
    public void Outline_TextWithBlanks_GivesOneContourPerVisibleCharacter()
    {
        var contours = new FakeTextOutliner().Outline(Text("a b"));

        Assert.NotNull(contours);
        Assert.Equal(2, contours.Count);
        Assert.All(contours, contour => Assert.True(contour.IsClosed));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Outline_NothingVisible_ReturnsNull(string content)
    {
        Assert.Null(new FakeTextOutliner().Outline(Text(content)));
    }
}
