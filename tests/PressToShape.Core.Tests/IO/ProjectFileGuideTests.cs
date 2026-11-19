using System.Linq;
using System.Text;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public sealed class ProjectFileGuideTests
{
    private const string Prefix = "{\"format\":\"PressToShape\",\"version\":2,\"artboard\":{\"width\":100,\"height\":50},";

    private const string Layers =
        "\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[]}]";

    private static ProjectFileResult ReadText(string json) => ProjectFile.Read(Encoding.UTF8.GetBytes(json));

    private static Document RoundTrip(Document document)
    {
        var result = ProjectFile.Read(ProjectFile.Write(document));
        Assert.True(result.Succeeded, result.Detail);
        return result.Document!;
    }

    [Fact]
    public void Write_DocumentWithoutGuides_HasNoGuidesEntry()
    {
        var text = Encoding.UTF8.GetString(ProjectFile.Write(Document.Create("L")));

        Assert.DoesNotContain("guides", text);
    }

    [Fact]
    public void WriteThenRead_Guides_KeepOrientationPositionAndOrder()
    {
        var document = Document.Create("L")
            .AddGuide(new Guide { Orientation = GuideOrientation.Horizontal, Position = 120.5 })
            .AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = -30 })
            .AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 400 });

        var read = RoundTrip(document);

        Assert.Equal(
            [(GuideOrientation.Horizontal, 120.5), (GuideOrientation.Vertical, -30), (GuideOrientation.Vertical, 400)],
            read.Guides.Select(g => (g.Orientation, g.Position)).ToArray());
    }

    [Fact]
    public void Read_ALoadedGuide_GetsANewId()
    {
        var document = Document.Create("L").AddGuide(new Guide { Orientation = GuideOrientation.Horizontal, Position = 5 });

        var read = RoundTrip(document);

        Assert.NotEqual(document.Guides[0].Id, read.Guides[0].Id);
    }

    [Fact]
    public void Write_Guides_UseTheNamesHorizontalAndVertical()
    {
        var document = Document.Create("L")
            .AddGuide(new Guide { Orientation = GuideOrientation.Horizontal, Position = 1 })
            .AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 2 });

        var text = Encoding.UTF8.GetString(ProjectFile.Write(document));

        Assert.Contains("\"horizontal\"", text);
        Assert.Contains("\"vertical\"", text);
    }

    [Fact]
    public void Read_FileWithoutGuides_HasNone()
    {
        var result = ReadText(Prefix + Layers + "}");

        Assert.True(result.Succeeded, result.Detail);
        Assert.Empty(result.Document!.Guides);
    }

    [Fact]
    public void Read_VersionOneFile_HasNoGuides()
    {
        var result = ReadText(Prefix.Replace("\"version\":2", "\"version\":1") + Layers + "}");

        Assert.True(result.Succeeded, result.Detail);
        Assert.Empty(result.Document!.Guides);
    }

    [Fact]
    public void Read_DamagedGuides_AreSkippedAndTheRestLoads()
    {
        var guides = ",\"guides\":[" +
            "{\"orientation\":\"horizontal\",\"position\":10}," +
            "{\"orientation\":\"diagonal\",\"position\":20}," +
            "{\"position\":30}," +
            "{\"orientation\":\"vertical\"}," +
            "null," +
            "{\"orientation\":\"vertical\",\"position\":40}]";

        var result = ReadText(Prefix + Layers + guides + "}");

        Assert.True(result.Succeeded, result.Detail);
        Assert.Equal([(GuideOrientation.Horizontal, 10.0), (GuideOrientation.Vertical, 40.0)], result.Document!.Guides.Select(g => (g.Orientation, g.Position)).ToArray());
    }

    [Fact]
    public void Read_GuidesEntryNull_MeansNoGuides()
    {
        var result = ReadText(Prefix + Layers + ",\"guides\":null}");

        Assert.True(result.Succeeded, result.Detail);
        Assert.Empty(result.Document!.Guides);
    }

    [Fact]
    public void FormatVersion_StaysTwo_AndANewerFileStillGivesTheError()
    {
        Assert.Equal(2, ProjectFile.FormatVersion);

        var result = ReadText(Prefix.Replace("\"version\":2", "\"version\":3") + Layers + "}");

        Assert.Equal(ProjectFileError.NewerVersion, result.Error);
    }

    [Fact]
    public void SvgExport_IgnoresGuides()
    {
        var plain = Document.Create("L");
        var withGuides = plain.AddGuide(new Guide { Orientation = GuideOrientation.Horizontal, Position = 5 });

        Assert.Equal(SvgExport.ToUtf8(plain, new FakeTextMeasurer()), SvgExport.ToUtf8(withGuides, new FakeTextMeasurer()));
    }

    [Fact]
    public void WriteThenRead_ShapesAndPaletteAreUnaffectedByGuides()
    {
        var document = Document.Create("L")
            .AddSwatch(PressToShape.Core.Shapes.Color.FromRgb(1, 2, 3))
            .AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 7 });

        var read = RoundTrip(document);

        Assert.Single(read.Palette);
        Assert.Single(read.Guides);
        Assert.Equal(document.ArtboardWidth, read.ArtboardWidth);
    }
}
