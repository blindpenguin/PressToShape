using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class SvgExportPathTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    private static XDocument Export(Document document)
    {
        var bytes = SvgExport.ToUtf8(document, new FakeTextMeasurer());
        return XDocument.Parse(new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes));
    }

    private static Document OneLayer(params Shape[] shapes)
    {
        var document = Document.Create("Layer 1");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static XElement ExportedPath(PathShape path) =>
        Export(OneLayer(path)).Root!.Descendants(Svg + "path").Single();

    private static string D(PathShape path) => (string)ExportedPath(path).Attribute("d")!;

    [Fact]
    public void ToUtf8_ClosedLinePath_WritesMLLAndZ()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(50, 100)], isClosed: true);

        Assert.Equal("M 0 0 L 100 0 L 50 100 Z", D(path));
    }

    [Fact]
    public void ToUtf8_OpenLinePath_HasNoZ()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(50, 100)]);

        Assert.Equal("M 0 0 L 100 0 L 50 100", D(path));
    }

    [Fact]
    public void ToUtf8_CurvedSegment_WritesC()
    {
        var path = PathShape.Create([new DocumentNode(0, 0, outX: 0, outY: 50), new DocumentNode(100, 100, inX: 50, inY: 100)]);

        Assert.Equal("M 0 0 C 0 50 50 100 100 100", D(path));
    }

    [Fact]
    public void ToUtf8_SegmentWithOnlyOneHandle_UsesTheAnchorForTheMissingOne()
    {
        var path = PathShape.Create([new DocumentNode(0, 0, outX: 0, outY: 100), new DocumentNode(100, 100)]);

        Assert.Equal("M 0 0 C 0 100 100 100 100 100", D(path));
    }

    [Fact]
    public void ToUtf8_ClosedPathWithACurvedClosingSegment_WritesTheCurveBeforeZ()
    {
        var path = PathShape.Create(
            [new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100, outX: 0, outY: 100)],
            isClosed: true);

        Assert.Equal("M 0 0 L 100 0 L 100 100 C 0 100 0 0 0 0 Z", D(path));
    }

    [Fact]
    public void ToUtf8_EvenOddPath_WritesFillRule()
    {
        var evenOdd = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)], fillRule: FillRule.EvenOdd);
        var nonZero = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);

        Assert.Equal("evenodd", (string?)ExportedPath(evenOdd).Attribute("fill-rule"));
        Assert.Null(ExportedPath(nonZero).Attribute("fill-rule"));
    }

    [Fact]
    public void ToUtf8_PathAppearance_IsWrittenLikeOtherShapes()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]) with
        {
            Appearance = new Appearance { Fill = Color.FromRgb(255, 0, 0), Stroke = Color.FromRgb(0, 0, 255), StrokeWidth = 3, Opacity = 0.5 },
        };

        var element = ExportedPath(path);

        Assert.Equal("#ff0000", (string?)element.Attribute("fill"));
        Assert.Equal("#0000ff", (string?)element.Attribute("stroke"));
        Assert.Equal("3", (string?)element.Attribute("stroke-width"));
        Assert.Equal("0.5", (string?)element.Attribute("opacity"));
    }

    [Fact]
    public void ToUtf8_RotatedPath_TurnsAroundTheCenterOfItsBox()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 40)]) with { Rotation = 30 };

        Assert.Equal("rotate(30 50 20)", (string?)ExportedPath(path).Attribute("transform"));
    }

    [Fact]
    public void ToUtf8_PathAfterAResizeWithWith_WritesTheScaledCoordinates()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 50)]);

        var wider = path with { Width = 200 };

        Assert.Equal("M 0 0 L 200 50", D(wider));
    }

    [Fact]
    public void ToUtf8_PathInAGroup_IsWrittenInsideTheGroup()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var group = new GroupShape { Children = [path] };

        var svg = Export(OneLayer(group));

        var element = svg.Root!.Descendants(Svg + "g").Last().Elements().Single();
        Assert.Equal(Svg + "path", element.Name);
    }

    [Fact]
    public void ToUtf8_HiddenPathAndPathInAHiddenLayer_AreLeftOut()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var document = OneLayer(path with { IsVisible = false });
        var hiddenLayer = OneLayer(path);
        hiddenLayer = hiddenLayer.UpdateLayer(hiddenLayer.Layers[0].Id, layer => layer with { IsVisible = false });

        Assert.Empty(Export(document).Root!.Descendants(Svg + "path"));
        Assert.Empty(Export(hiddenLayer).Root!.Descendants(Svg + "path"));
    }

    [Fact]
    public void ToUtf8_GermanCulture_StillWritesDots()
    {
        var path = PathShape.Create([new DocumentNode(0.5, 0.25), new DocumentNode(10.75, 9.5)]);
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            Assert.Equal("M 0.5 0.25 L 10.75 9.5", D(path));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
