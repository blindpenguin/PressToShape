using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class SvgExportTextTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    // Font size 10: a character is 6 wide and a line is 12 high, the first baseline is 10 (see FakeTextMeasurer).
    private static TextShape Text(string content, double x = 100, double y = 50) => new() { X = x, Y = y, Width = 30, Height = 12, Text = content, FontSize = 10 };

    private static XElement Export(params Shape[] shapes)
    {
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, shapes);
        var bytes = SvgExport.ToUtf8(document, new FakeTextMeasurer());
        return XDocument.Parse(new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes)).Root!;
    }

    private static XElement TheText(XElement root) => root.Descendants(Svg + "text").Single();

    [Fact]
    public void ToUtf8_PointText_WritesTextWithOneTspanAtTheBaseline()
    {
        var text = TheText(Export(Text("Hello")));

        var tspan = Assert.Single(text.Elements(Svg + "tspan"));
        Assert.Equal("Hello", tspan.Value);
        Assert.Equal("100", (string?)tspan.Attribute("x"));
        Assert.Equal("60", (string?)tspan.Attribute("y"));
        Assert.Equal("10", (string?)text.Attribute("font-size"));
        Assert.Equal("Inter", (string?)text.Attribute("font-family"));
        Assert.Null(text.Attribute("font-weight"));
        Assert.Null(text.Attribute("font-style"));
        Assert.Null(text.Attribute("text-anchor"));
        Assert.Null(text.Attribute("letter-spacing"));
    }

    [Fact]
    public void ToUtf8_TwoLines_WritesATspanForEachWithItsBaseline()
    {
        var text = TheText(Export(Text("ab\ncd")));

        var tspans = text.Elements(Svg + "tspan").ToArray();
        Assert.Equal(["ab", "cd"], tspans.Select(t => t.Value));
        Assert.Equal(["60", "72"], tspans.Select(t => (string?)t.Attribute("y")));
    }

    [Fact]
    public void ToUtf8_BoldItalicSpacing_WritesTheFontAttributes()
    {
        var text = TheText(Export(Text("a") with { IsBold = true, IsItalic = true, LetterSpacing = 1.5 }));

        Assert.Equal("bold", (string?)text.Attribute("font-weight"));
        Assert.Equal("italic", (string?)text.Attribute("font-style"));
        Assert.Equal("1.5", (string?)text.Attribute("letter-spacing"));
    }

    [Theory]
    [InlineData(TextAlignType.Left, null, "100")]
    [InlineData(TextAlignType.Center, "middle", "115")]
    [InlineData(TextAlignType.Right, "end", "130")]
    public void ToUtf8_PointTextAlignment_WritesTheAnchorAndTheAnchorPosition(TextAlignType alignment, string? anchor, string x)
    {
        // The widest line "abcde" is 30 wide and sets the reference width of a point text.
        var text = TheText(Export(Text("abcde\nabc") with { Alignment = alignment }));

        Assert.Equal(anchor, (string?)text.Attribute("text-anchor"));
        Assert.All(text.Elements(Svg + "tspan"), tspan => Assert.Equal(x, (string?)tspan.Attribute("x")));
    }

    [Fact]
    public void ToUtf8_AreaTextCenter_AnchorsAtTheMiddleOfTheBox()
    {
        var text = TheText(Export(Text("ab") with { IsArea = true, Width = 100, Alignment = TextAlignType.Center }));

        Assert.Equal("150", (string?)text.Elements(Svg + "tspan").Single().Attribute("x"));
    }

    [Fact]
    public void ToUtf8_AreaText_WritesTheWrappedLines()
    {
        var text = TheText(Export(Text("aaa bbb ccc") with { IsArea = true, Width = 40 }));

        Assert.Equal(["aaa", "bbb", "ccc"], text.Elements(Svg + "tspan").Select(t => t.Value));
    }

    [Fact]
    public void ToUtf8_SpecialCharacters_AreEscapedAndParseBack()
    {
        var text = TheText(Export(Text("<a & \"b\">") with { FontFamily = "A&B \"Font\"" }));

        Assert.Equal("<a & \"b\">", text.Elements(Svg + "tspan").Single().Value);
        Assert.Equal("A&B \"Font\"", (string?)text.Attribute("font-family"));
        Assert.Equal("preserve", (string?)text.Attribute(XNamespace.Xml + "space"));
    }

    [Fact]
    public void ToUtf8_ControlCharacters_AreReplaced()
    {
        var text = TheText(Export(Text("a\u0001b")));

        Assert.Equal("a�b", text.Elements(Svg + "tspan").Single().Value);
    }

    [Fact]
    public void ToUtf8_RotatedText_WritesTheTransformAroundTheBoxCenter()
    {
        var text = TheText(Export(Text("a") with { Rotation = 90 }));

        Assert.Equal("rotate(90 115 56)", (string?)text.Attribute("transform"));
    }

    [Fact]
    public void ToUtf8_StrokeAndNoFill_WritesThemOnTheTextElement()
    {
        var shape = Text("a") with { Appearance = new Appearance { Fill = null, Stroke = Color.FromRgb(255, 0, 0), StrokeWidth = 2, Opacity = 0.5 } };

        var text = TheText(Export(shape));

        Assert.Equal("none", (string?)text.Attribute("fill"));
        Assert.Equal("#ff0000", (string?)text.Attribute("stroke"));
        Assert.Equal("2", (string?)text.Attribute("stroke-width"));
        Assert.Equal("0.5", (string?)text.Attribute("opacity"));
    }

    [Fact]
    public void ToUtf8_HiddenText_IsNotExported()
    {
        var root = Export(Text("a") with { IsVisible = false });

        Assert.Empty(root.Descendants(Svg + "text"));
    }

    [Fact]
    public void ToUtf8_EmptyText_WritesNoTspan()
    {
        var text = TheText(Export(Text(string.Empty)));

        Assert.Empty(text.Elements(Svg + "tspan"));
    }

    [Fact]
    public void ToUtf8_TextInAGroup_IsExportedInsideTheGroup()
    {
        var text = Text("in group");
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [text, new RectangleShape { Width = 5, Height = 5 }]);
        document = GroupOperations.Group(document, [text.Id, document.Layers[0].Shapes[1].Id], Guid.NewGuid());

        var root = XDocument.Parse(Encoding.UTF8.GetString(SvgExport.ToUtf8(document, new FakeTextMeasurer()))).Root!;

        Assert.Single(root.Descendants(Svg + "g").Last().Elements(Svg + "text"));
    }

    [Fact]
    public void ToUtf8_NullMeasurer_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SvgExport.ToUtf8(Document.Create("L"), null!));
    }
}
