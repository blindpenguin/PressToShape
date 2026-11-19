using System.Linq;
using PressToShape.App.Services;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

// The headless platform has its own simple font metrics, so the tests compare layouts with each other and do not expect pixel values.
[Collection(HeadlessUi.CollectionName)]
public class AvaloniaTextMeasurerTests
{
    // The default font is the bundled Inter, which the headless test application does not register, so the tests use a family name that gets
    // the platform's fallback font.
    private static TextShape Text(string content, double width = 0) =>
        new() { Text = content, FontSize = 20, FontFamily = "Test Font", IsArea = width > 0, Width = width };

    private static TextLayoutResult Layout(TextShape text) => HeadlessUi.Run(() => new AvaloniaTextMeasurer().Layout(text));

    [Fact]
    public void Layout_EmptyText_HasNoLines()
    {
        var result = Layout(Text(string.Empty));

        Assert.Empty(result.Lines);
        Assert.Equal(0, result.Width);
        Assert.Equal(0, result.Height);
    }

    [Fact]
    public void Layout_PointText_OneLineWithPositiveSize()
    {
        var result = Layout(Text("Hello"));

        var line = Assert.Single(result.Lines);
        Assert.Equal("Hello", line.Text);
        Assert.True(result.Width > 0);
        Assert.True(result.Height > 0);
        Assert.InRange(line.Baseline, 1, result.Height);
    }

    [Fact]
    public void Layout_Newline_GivesTwoLinesWithGrowingBaselines()
    {
        var result = Layout(Text("a\nb"));

        Assert.Equal(["a", "b"], result.Lines.Select(line => line.Text));
        Assert.True(result.Lines[1].Baseline > result.Lines[0].Baseline);
        Assert.Equal(Layout(Text("a")).Height * 2, result.Height, 6);
    }

    [Fact]
    public void Layout_PointText_DoesNotWrap()
    {
        Assert.Single(Layout(Text("one two three four five six seven eight nine ten")).Lines);
    }

    [Fact]
    public void Layout_AreaTextNarrowBox_WrapsAtWordBoundaries()
    {
        // Each word fits alone into 1.5 word widths, two words do not.
        var wordWidth = Layout(Text("aaaa")).Width;

        var result = Layout(Text("aaaa bbbb cccc dddd", width: wordWidth * 1.5));

        Assert.Equal(["aaaa", "bbbb", "cccc", "dddd"], result.Lines.Select(line => line.Text.Trim()));
    }

    [Fact]
    public void Layout_Bold_IsNotNarrowerThanRegular()
    {
        var regular = Layout(Text("Hello World"));
        var bold = Layout(Text("Hello World") with { IsBold = true });

        Assert.True(bold.Width >= regular.Width);
    }

    [Fact]
    public void Layout_LetterSpacing_MakesTheTextWider()
    {
        var plain = Layout(Text("Hello"));
        var spaced = Layout(Text("Hello") with { LetterSpacing = 5 });

        Assert.True(spaced.Width > plain.Width);
    }

    [Fact]
    public void Layout_LineSpacing_ScalesTheLineHeight()
    {
        var plain = Layout(Text("a\nb"));
        var doubled = Layout(Text("a\nb") with { LineSpacing = 2 });

        Assert.Equal(plain.Height * 2, doubled.Height, 3);
    }

    [Fact]
    public void Layout_BiggerFont_IsBiggerInBothDirections()
    {
        var small = Layout(Text("Hello"));
        var big = Layout(Text("Hello") with { FontSize = 40 });

        Assert.True(big.Width > small.Width);
        Assert.True(big.Height > small.Height);
    }

    [Theory]
    [InlineData(TextAlignType.Center)]
    [InlineData(TextAlignType.Right)]
    public void Layout_AreaTextAlignment_ShiftsAShortLineIntoTheBox(TextAlignType alignment)
    {
        var left = Layout(Text("ab", width: 200));
        var aligned = Layout(Text("ab", width: 200) with { Alignment = alignment });

        Assert.Equal(0, left.Lines[0].X, 6);
        Assert.True(aligned.Lines[0].X > 0);
    }

    [Fact]
    public void Layout_PointTextCenter_ShiftsTheShorterLineByHalfTheDifference()
    {
        var result = Layout(Text("abcdefgh\nab") with { Alignment = TextAlignType.Center });

        Assert.Equal(0, result.Lines[0].X, 6);
        Assert.True(result.Lines[1].X > 0);
    }

    [Fact]
    public void Layout_FontNotInstalled_DoesNotThrowAndStillMeasures()
    {
        var result = Layout(Text("Hello") with { FontFamily = "No Such Font Family 12345" });

        Assert.Single(result.Lines);
        Assert.True(result.Width > 0);
    }

    [Theory]
    [InlineData("Inter")]
    [InlineData("inter")]
    public void FamilyFor_DefaultFontNameWithTheBundledFont_IsTheBundledFont(string name)
    {
        Assert.NotNull(TextFonts.FamilyFor(name, bundledAvailable: true).Key);
    }

    [Fact]
    public void FamilyFor_DefaultFontNameWithoutTheBundledFont_KeepsTheName()
    {
        var family = TextFonts.FamilyFor("Inter", bundledAvailable: false);

        Assert.Null(family.Key);
        Assert.Equal("Inter", family.Name);
    }

    [Fact]
    public void FamilyFor_OtherName_KeepsTheName()
    {
        var family = TextFonts.FamilyFor("Some Other Font", bundledAvailable: true);

        Assert.Null(family.Key);
        Assert.Equal("Some Other Font", family.Name);
    }

    [Fact]
    public void FamilyFor_InTheTestApplication_DoesNotThrow()
    {
        // The headless application has no bundled font, so the plain name comes back. The point is that a text with the default font can be laid out.
        var family = HeadlessUi.Run(() => TextFonts.FamilyFor(TextShape.DefaultFontFamily));

        Assert.NotNull(family);
    }
}
