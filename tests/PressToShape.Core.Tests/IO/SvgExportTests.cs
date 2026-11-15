using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class SvgExportTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
    private static readonly XNamespace Ink = "http://www.inkscape.org/namespaces/inkscape";

    private static XDocument Export(Document document)
    {
        var bytes = SvgExport.ToUtf8(document);
        return XDocument.Parse(new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes));
    }

    private static Document OneLayer(params Shape[] shapes)
    {
        var document = Document.Create("Layer 1");
        foreach (var shape in shapes)
        {
            document = document.AddShape(document.Layers[0].Id, shape);
        }

        return document;
    }

    private static XElement OnlyShape(XDocument svg) => svg.Root!.Elements(Svg + "g").Single().Elements().Single();

    [Fact]
    public void ToUtf8_Root_HasArtboardSizeAndViewBoxAndNoBackground()
    {
        var document = Document.Create("L") with { ArtboardWidth = 640.5, ArtboardHeight = 480 };

        var svg = Export(document);

        Assert.Equal(Svg + "svg", svg.Root!.Name);
        Assert.Equal("1.1", (string?)svg.Root.Attribute("version"));
        Assert.Equal("640.5", (string?)svg.Root.Attribute("width"));
        Assert.Equal("480", (string?)svg.Root.Attribute("height"));
        Assert.Equal("0 0 640.5 480", (string?)svg.Root.Attribute("viewBox"));
        Assert.Empty(svg.Root.Descendants(Svg + "rect"));
    }

    [Fact]
    public void ToUtf8_Output_IsUtf8WithoutByteOrderMarkAndHasADeclaration()
    {
        var bytes = SvgExport.ToUtf8(Document.Create("L"));

        Assert.Equal((byte)'<', bytes[0]);
        Assert.StartsWith("<?xml", Encoding.UTF8.GetString(bytes), StringComparison.Ordinal);
    }

    [Fact]
    public void ToUtf8_VisibleLayers_BecomeGroupsInDocumentOrder()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var document = Document.Create("x") with { Layers = [new Layer { Id = a, Name = "Bottom" }, new Layer { Id = b, Name = "Top" }] };

        var groups = Export(document).Root!.Elements(Svg + "g").ToList();

        Assert.Equal(new[] { "Bottom", "Top" }, groups.Select(g => (string?)g.Attribute(Ink + "label")));
        Assert.All(groups, g => Assert.Equal("layer", (string?)g.Attribute(Ink + "groupmode")));
        Assert.Equal(new[] { "layer-1", "layer-2" }, groups.Select(g => (string?)g.Attribute("id")));
    }

    [Fact]
    public void ToUtf8_HiddenLayer_IsLeftOut_AndLockedLayerIsExported()
    {
        var document = Document.Create("x") with
        {
            Layers =
            [
                new Layer { Name = "Hidden", IsVisible = false, Shapes = [new RectangleShape { Width = 1, Height = 1 }] },
                new Layer { Name = "Locked", IsLocked = true, Shapes = [new RectangleShape { Width = 1, Height = 1 }] },
            ],
        };

        var groups = Export(document).Root!.Elements(Svg + "g").ToList();

        var group = Assert.Single(groups);
        Assert.Equal("Locked", (string?)group.Attribute(Ink + "label"));
        Assert.Single(group.Elements(Svg + "rect"));
    }

    [Fact]
    public void ToUtf8_Rectangle_HasPositionAndSize()
    {
        var rect = OnlyShape(Export(OneLayer(new RectangleShape { X = 10, Y = -20.5, Width = 30, Height = 40 })));

        Assert.Equal(Svg + "rect", rect.Name);
        Assert.Equal(new[] { "10", "-20.5", "30", "40" }, new[] { "x", "y", "width", "height" }.Select(n => (string?)rect.Attribute(n)));
    }

    [Fact]
    public void ToUtf8_Ellipse_HasCenterAndRadii()
    {
        var ellipse = OnlyShape(Export(OneLayer(new EllipseShape { X = 10, Y = 20, Width = 30, Height = 50 })));

        Assert.Equal(Svg + "ellipse", ellipse.Name);
        Assert.Equal(new[] { "25", "45", "15", "25" }, new[] { "cx", "cy", "rx", "ry" }.Select(n => (string?)ellipse.Attribute(n)));
    }

    [Fact]
    public void ToUtf8_ShapesInStackingOrder_KeepTheOrder()
    {
        var svg = Export(OneLayer(
            new RectangleShape { X = 1, Width = 1, Height = 1 },
            new EllipseShape { X = 2, Width = 2, Height = 2 },
            new RectangleShape { X = 3, Width = 1, Height = 1 }));

        var names = svg.Root!.Elements(Svg + "g").Single().Elements().Select(e => e.Name.LocalName);

        Assert.Equal(new[] { "rect", "ellipse", "rect" }, names);
    }

    [Fact]
    public void ToUtf8_OpaqueFillAndStroke_HaveNoOpacityAttributes()
    {
        var shape = new RectangleShape
        {
            Width = 1,
            Height = 1,
            Appearance = new Appearance { Fill = new Color(0xAA, 0xBB, 0xCC, 255), Stroke = new Color(1, 2, 3, 255), StrokeWidth = 2.5, Opacity = 1 },
        };

        var rect = OnlyShape(Export(OneLayer(shape)));

        Assert.Equal("#aabbcc", (string?)rect.Attribute("fill"));
        Assert.Equal("#010203", (string?)rect.Attribute("stroke"));
        Assert.Equal("2.5", (string?)rect.Attribute("stroke-width"));
        Assert.Null(rect.Attribute("fill-opacity"));
        Assert.Null(rect.Attribute("stroke-opacity"));
        Assert.Null(rect.Attribute("opacity"));
    }

    [Fact]
    public void ToUtf8_TransparentFillAndStroke_HaveOpacityAttributes()
    {
        var shape = new RectangleShape
        {
            Width = 1,
            Height = 1,
            Appearance = new Appearance { Fill = new Color(0, 0, 0, 128), Stroke = new Color(0, 0, 0, 51), StrokeWidth = 1 },
        };

        var rect = OnlyShape(Export(OneLayer(shape)));

        Assert.Equal(128 / 255.0, double.Parse((string)rect.Attribute("fill-opacity")!, CultureInfo.InvariantCulture), precision: 3);
        Assert.Equal("0.2", (string?)rect.Attribute("stroke-opacity"));
    }

    [Fact]
    public void ToUtf8_NoFill_WritesFillNone()
    {
        var shape = new RectangleShape { Width = 1, Height = 1, Appearance = new Appearance { Fill = null, Stroke = null } };

        var rect = OnlyShape(Export(OneLayer(shape)));

        Assert.Equal("none", (string?)rect.Attribute("fill"));
        Assert.Null(rect.Attribute("fill-opacity"));
    }

    [Fact]
    public void ToUtf8_NoStrokeOrZeroStrokeWidth_WriteNoStrokeAttributes()
    {
        var noStroke = new RectangleShape { Width = 1, Height = 1, Appearance = new Appearance { Stroke = null, StrokeWidth = 3 } };
        var zeroWidth = new RectangleShape { Width = 1, Height = 1, Appearance = new Appearance { Stroke = new Color(1, 1, 1, 255), StrokeWidth = 0 } };

        foreach (var shape in new[] { noStroke, zeroWidth })
        {
            var rect = OnlyShape(Export(OneLayer(shape)));

            Assert.Null(rect.Attribute("stroke"));
            Assert.Null(rect.Attribute("stroke-width"));
            Assert.Null(rect.Attribute("stroke-opacity"));
        }
    }

    [Theory]
    [InlineData(0.5, "0.5")]
    [InlineData(0.25, "0.25")]
    [InlineData(0, "0")]
    public void ToUtf8_ShapeOpacityBelowOne_WritesOpacity(double opacity, string expected)
    {
        var shape = new RectangleShape { Width = 1, Height = 1, Appearance = new Appearance { Opacity = opacity } };

        var rect = OnlyShape(Export(OneLayer(shape)));

        Assert.Equal(expected, (string?)rect.Attribute("opacity"));
    }

    [Fact]
    public void ToUtf8_ZeroSizeShape_IsExported()
    {
        var rect = OnlyShape(Export(OneLayer(new RectangleShape { X = 5, Y = 5, Width = 0, Height = 0 })));

        Assert.Equal("0", (string?)rect.Attribute("width"));
    }

    [Fact]
    public void ToUtf8_UnderAGermanCulture_UsesADotAndNoExponent()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            var shape = new RectangleShape { X = 1.5, Y = 1e-7, Width = 1e21, Height = -0.0 };

            var rect = OnlyShape(Export(OneLayer(shape)));

            Assert.Equal("1.5", (string?)rect.Attribute("x"));
            Assert.Equal("0.0000001", (string?)rect.Attribute("y"));
            Assert.Equal("1000000000000000000000", (string?)rect.Attribute("width"));
            Assert.Equal("0", (string?)rect.Attribute("height"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData("a<b&c\"d'e>f")]
    [InlineData("tab\there")]
    [InlineData("unicode äö \U0001F600")]
    public void ToUtf8_LayerNameWithSpecialCharacters_RoundTripsThroughTheXmlParser(string name)
    {
        var document = Document.Create("x") with { Layers = [new Layer { Name = name }] };

        var label = (string?)Export(document).Root!.Element(Svg + "g")!.Attribute(Ink + "label");

        Assert.Equal(name, label);
    }

    [Fact]
    public void ToUtf8_ControlCharactersAndLoneSurrogatesInAName_AreReplaced()
    {
        var document = Document.Create("x") with { Layers = [new Layer { Name = "a\u0001b\u0000c\uD800d\uDC00e\u001Ff" }] };

        var label = (string?)Export(document).Root!.Element(Svg + "g")!.Attribute(Ink + "label");

        Assert.Equal("a�b�c�d�e�f", label);
    }

    [Fact]
    public void ToUtf8_EmptyLayer_GivesAnEmptyGroup()
    {
        var group = Export(Document.Create("Empty")).Root!.Element(Svg + "g")!;

        Assert.Empty(group.Elements());
    }

    [Fact]
    public void ToUtf8_DocumentWithOnlyHiddenLayers_IsAnEmptyValidSvg()
    {
        var document = Document.Create("x") with { Layers = [new Layer { Name = "H", IsVisible = false }] };

        var svg = Export(document);

        Assert.Empty(svg.Root!.Elements());
    }

    [Fact]
    public void ToUtf8_Output_HasNoScriptsLinksOrExternalReferences()
    {
        var svg = Export(OneLayer(new RectangleShape { Width = 1, Height = 1 }, new EllipseShape { Width = 1, Height = 1 }));

        Assert.DoesNotContain(svg.Descendants(), e => e.Name.LocalName is "script" or "a" or "use" or "image" or "foreignObject");
        Assert.DoesNotContain(svg.Descendants().Attributes(), a => a.Name.LocalName is "href" or "src");
        Assert.Null(svg.DocumentType);
    }

    [Fact]
    public void ToUtf8_NullDocument_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SvgExport.ToUtf8(null!));
    }

    [Fact]
    public void ToUtf8_ManyShapes_ExportsAllOfThem()
    {
        var shapes = Enumerable.Range(0, 2000).Select(i => (Shape)new RectangleShape { X = i, Width = 1, Height = 1 }).ToArray();

        var svg = Export(OneLayer(shapes));

        Assert.Equal(2000, svg.Root!.Element(Svg + "g")!.Elements().Count());
    }

    [Fact]
    public void ToUtf8_RotatedRectangle_HasATransformAroundItsCenter()
    {
        var rect = OnlyShape(Export(OneLayer(new RectangleShape { X = 10, Y = 20, Width = 100, Height = 50, Rotation = 30 })));

        Assert.Equal("rotate(30 60 45)", (string?)rect.Attribute("transform"));
        Assert.Equal("10", (string?)rect.Attribute("x"));
        Assert.Equal("100", (string?)rect.Attribute("width"));
    }

    [Fact]
    public void ToUtf8_RotatedEllipse_HasATransformAroundItsCenter()
    {
        var ellipse = OnlyShape(Export(OneLayer(new EllipseShape { X = 0, Y = 0, Width = 80, Height = 40, Rotation = 359.5 })));

        Assert.Equal("rotate(359.5 40 20)", (string?)ellipse.Attribute("transform"));
        Assert.Equal("40", (string?)ellipse.Attribute("cx"));
    }

    [Fact]
    public void ToUtf8_UnrotatedShapes_HaveNoTransform()
    {
        var svg = Export(OneLayer(new RectangleShape { Width = 1, Height = 1 }, new EllipseShape { Width = 1, Height = 1 }));

        Assert.DoesNotContain(svg.Descendants().Attributes(), a => a.Name.LocalName == "transform");
    }

    [Fact]
    public void ToUtf8_RotationNumbers_UseADotUnderAGermanCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            var rect = OnlyShape(Export(OneLayer(new RectangleShape { X = 1.5, Y = 0, Width = 3, Height = 3, Rotation = 12.5 })));

            Assert.Equal("rotate(12.5 3 1.5)", (string?)rect.Attribute("transform"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void ToUtf8_RotatedShapeWithOpacityAndStroke_KeepsAllAttributes()
    {
        var shape = new RectangleShape
        {
            Width = 10,
            Height = 10,
            Rotation = 45,
            Appearance = new Appearance { Fill = new Color(1, 2, 3, 255), Stroke = new Color(4, 5, 6, 255), StrokeWidth = 2, Opacity = 0.5 },
        };

        var rect = OnlyShape(Export(OneLayer(shape)));

        Assert.Equal("0.5", (string?)rect.Attribute("opacity"));
        Assert.Equal("#040506", (string?)rect.Attribute("stroke"));
        Assert.Equal("rotate(45 5 5)", (string?)rect.Attribute("transform"));
    }
}
