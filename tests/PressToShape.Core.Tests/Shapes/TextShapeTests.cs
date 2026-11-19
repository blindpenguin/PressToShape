using System;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class TextShapeTests
{
    private static TextShape Text() => new() { X = 10, Y = 20, Width = 100, Height = 40, Text = "Hello" };

    [Fact]
    public void New_Default_HasDocumentedDefaults()
    {
        var text = new TextShape();

        Assert.Equal(string.Empty, text.Text);
        Assert.False(text.IsArea);
        Assert.Equal(TextShape.DefaultFontFamily, text.FontFamily);
        Assert.Equal(TextShape.DefaultFontSize, text.FontSize);
        Assert.False(text.IsBold);
        Assert.False(text.IsItalic);
        Assert.Equal(TextAlignType.Left, text.Alignment);
        Assert.Equal(1, text.LineSpacing);
        Assert.Equal(0, text.LetterSpacing);
    }

    [Theory]
    [InlineData(TextShape.MinFontSize)]
    [InlineData(TextShape.MaxFontSize)]
    [InlineData(12.5)]
    public void FontSize_InsideRange_IsAccepted(double size)
    {
        Assert.Equal(size, new TextShape { FontSize = size }.FontSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.99)]
    [InlineData(2000.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void FontSize_OutsideRange_Throws(double size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextShape { FontSize = size });
    }

    [Theory]
    [InlineData(TextShape.MinLineSpacing)]
    [InlineData(TextShape.MaxLineSpacing)]
    public void LineSpacing_AtLimit_IsAccepted(double spacing)
    {
        Assert.Equal(spacing, new TextShape { LineSpacing = spacing }.LineSpacing);
    }

    [Theory]
    [InlineData(0.49)]
    [InlineData(5.01)]
    [InlineData(double.NaN)]
    public void LineSpacing_OutsideRange_Throws(double spacing)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextShape { LineSpacing = spacing });
    }

    [Theory]
    [InlineData(TextShape.MinLetterSpacing)]
    [InlineData(TextShape.MaxLetterSpacing)]
    public void LetterSpacing_AtLimit_IsAccepted(double spacing)
    {
        Assert.Equal(spacing, new TextShape { LetterSpacing = spacing }.LetterSpacing);
    }

    [Theory]
    [InlineData(-50.01)]
    [InlineData(500.01)]
    [InlineData(double.NegativeInfinity)]
    public void LetterSpacing_OutsideRange_Throws(double spacing)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextShape { LetterSpacing = spacing });
    }

    [Fact]
    public void Text_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new TextShape { Text = null! });
    }

    [Fact]
    public void Text_AtMaxLength_IsAcceptedAndOneMoreThrows()
    {
        Assert.Equal(TextShape.MaxTextLength, new TextShape { Text = new string('a', TextShape.MaxTextLength) }.Text.Length);
        Assert.Throws<ArgumentException>(() => new TextShape { Text = new string('a', TextShape.MaxTextLength + 1) });
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FontFamily_EmptyOrBlank_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new TextShape { FontFamily = name });
    }

    [Fact]
    public void FontFamily_TooLong_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TextShape { FontFamily = new string('a', TextShape.MaxFontFamilyLength + 1) });
    }

    [Fact]
    public void With_ChangedText_KeepsIdAndOtherValues()
    {
        var text = Text() with { FontSize = 30 };

        var changed = text with { Text = "Bye" };

        Assert.Equal(text.Id, changed.Id);
        Assert.Equal(30, changed.FontSize);
        Assert.Equal("Bye", changed.Text);
    }

    [Fact]
    public void With_InvalidValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Text() with { FontSize = 0 });
    }

    [Theory]
    [InlineData(10, 20, true)]
    [InlineData(110, 60, true)]
    [InlineData(60, 40, true)]
    [InlineData(9.99, 40, false)]
    [InlineData(60, 60.01, false)]
    public void Contains_Point_IsInsideTheBox(double x, double y, bool expected)
    {
        Assert.Equal(expected, Text().Contains(x, y));
    }

    [Fact]
    public void Contains_RotatedQuarterTurn_FollowsTheTurnedBox()
    {
        // Box center (60, 40). A quarter turn makes the box 40 wide and 100 tall around that center.
        var text = Text() with { Rotation = 90 };

        Assert.True(text.Contains(60, 85));
        Assert.False(text.Contains(105, 40));
    }
}
