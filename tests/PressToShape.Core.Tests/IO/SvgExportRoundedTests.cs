using System.Linq;
using System.Text;
using System.Xml.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class SvgExportRoundedTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    private static XElement ExportOnly(RectangleShape shape)
    {
        var document = Document.Create("Layer 1");
        document = document.AddShape(document.Layers[0].Id, shape);
        var svg = XDocument.Parse(new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(SvgExport.ToUtf8(document)));
        return svg.Root!.Elements(Svg + "g").Single().Elements().Single();
    }

    private static RectangleShape Rect(CornerRadii radii) => new() { X = 10, Y = 20, Width = 100, Height = 40, Radii = radii };

    [Fact]
    public void ToUtf8_ZeroRadii_IsAPlainRectWithoutRadiusAttributes()
    {
        var element = ExportOnly(Rect(CornerRadii.Zero));

        Assert.Equal(Svg + "rect", element.Name);
        Assert.Null(element.Attribute("rx"));
        Assert.Null(element.Attribute("ry"));
    }

    [Fact]
    public void ToUtf8_EqualRadii_WritesRxAndRy()
    {
        var element = ExportOnly(Rect(CornerRadii.Uniform(8.5)));

        Assert.Equal(Svg + "rect", element.Name);
        Assert.Equal("8.5", (string?)element.Attribute("rx"));
        Assert.Equal("8.5", (string?)element.Attribute("ry"));
        Assert.Equal("100", (string?)element.Attribute("width"));
    }

    [Fact]
    public void ToUtf8_EqualRadiiAboveTheLimit_WritesTheLimitedRadius()
    {
        var element = ExportOnly(Rect(CornerRadii.Uniform(500)));

        Assert.Equal("20", (string?)element.Attribute("rx"));
        Assert.Equal("20", (string?)element.Attribute("ry"));
    }

    [Fact]
    public void ToUtf8_DifferentStoredRadiiThatAreLimitedToTheSame_StayARect()
    {
        var element = ExportOnly(Rect(new CornerRadii(500, 600, 700, 800)));

        Assert.Equal(Svg + "rect", element.Name);
        Assert.Equal("20", (string?)element.Attribute("rx"));
    }

    [Fact]
    public void ToUtf8_DifferentRadii_IsAPathWithArcs()
    {
        var element = ExportOnly(Rect(new CornerRadii(5, 0, 10, 0)));

        Assert.Equal(Svg + "path", element.Name);
        Assert.Equal("M 10 25 A 5 5 0 0 1 15 20 L 110 20 L 110 50 A 10 10 0 0 1 100 60 L 10 60 Z", (string?)element.Attribute("d"));
        Assert.Null(element.Attribute("fill-rule"));
        Assert.Null(element.Attribute("x"));
    }

    [Fact]
    public void ToUtf8_DifferentRadiiOnEveryCorner_HasFourArcsAndCloses()
    {
        var element = ExportOnly(Rect(new CornerRadii(1, 2, 3, 4)));
        var data = (string)element.Attribute("d")!;

        Assert.Equal(4, data.Split('A').Length - 1);
        Assert.EndsWith(" Z", data);
        Assert.StartsWith("M 10 21 A 1 1 0 0 1 11 20", data);
    }

    [Fact]
    public void ToUtf8_DifferentRadii_KeepsFillStrokeOpacityAndRotation()
    {
        var shape = Rect(new CornerRadii(5, 0, 10, 0)) with
        {
            Rotation = 30,
            Appearance = new Appearance { Fill = new Color(1, 2, 3, 255), Stroke = new Color(4, 5, 6, 255), StrokeWidth = 2, Opacity = 0.5 },
        };

        var element = ExportOnly(shape);

        Assert.Equal("#010203", (string?)element.Attribute("fill"));
        Assert.Equal("#040506", (string?)element.Attribute("stroke"));
        Assert.Equal("2", (string?)element.Attribute("stroke-width"));
        Assert.Equal("0.5", (string?)element.Attribute("opacity"));
        Assert.Equal("rotate(30 60 40)", (string?)element.Attribute("transform"));
    }

    [Fact]
    public void ToUtf8_EqualRadii_KeepsFillStrokeOpacityAndRotation()
    {
        var shape = Rect(CornerRadii.Uniform(6)) with
        {
            Rotation = 90,
            Appearance = new Appearance { Fill = null, Stroke = new Color(4, 5, 6, 255), StrokeWidth = 2, Opacity = 1 },
        };

        var element = ExportOnly(shape);

        Assert.Equal("none", (string?)element.Attribute("fill"));
        Assert.Equal("#040506", (string?)element.Attribute("stroke"));
        Assert.Equal("rotate(90 60 40)", (string?)element.Attribute("transform"));
    }
}
