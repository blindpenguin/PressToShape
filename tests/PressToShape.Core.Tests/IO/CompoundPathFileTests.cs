using System.Linq;
using System.Text;
using System.Xml.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class CompoundPathFileTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    private const string LayerStart =
        "{\"format\":\"PressToShape\",\"version\":2,\"artboard\":{\"width\":100,\"height\":50},\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[";

    private const string LayerEnd = "]}]}";

    private const string TwoNodes = "[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]";

    private static string PathJson(string body) =>
        "{\"type\":\"path\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"x\":1,\"y\":2,\"width\":10,\"height\":20,\"fill\":\"#ff0000ff\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1," + body + "}";

    private static ProjectFileResult ReadShape(string shapeJson) =>
        ProjectFile.Read(Encoding.UTF8.GetBytes(LayerStart + shapeJson + LayerEnd));

    private static string WrittenShapeJson(Document document) =>
        Encoding.UTF8.GetString(ProjectFile.Write(document));

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("L");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static PathShape Compound() =>
        PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0, outX: 20, outY: -10), new DocumentNode(100, 0), new DocumentNode(100, 100, inX: 120, inY: 90)], true),
                new DocumentSubPath([new DocumentNode(200, 0), new DocumentNode(250, 50, 240, 40, 260, 60, NodeType.Smooth)], false),
            ],
            FillRule.EvenOdd);

    [Fact]
    public void WriteThenRead_CompoundPath_KeepsEverySubPath()
    {
        var path = Compound();

        var result = ProjectFile.Read(ProjectFile.Write(Build(path)));

        Assert.True(result.Succeeded, result.Detail);
        var loaded = Assert.IsType<PathShape>(result.Document!.Layers[0].Shapes[0]);
        Assert.Equal(path.SubPaths.Length, loaded.SubPaths.Length);
        for (var i = 0; i < path.SubPaths.Length; i++)
        {
            Assert.Equal(path.SubPaths[i].IsClosed, loaded.SubPaths[i].IsClosed);
            Assert.Equal(path.SubPaths[i].Nodes.AsSpan().ToArray(), loaded.SubPaths[i].Nodes.AsSpan().ToArray());
        }

        Assert.Equal(path.FillRule, loaded.FillRule);
        Assert.Equal(path.X, loaded.X);
        Assert.Equal(path.Width, loaded.Width);
    }

    [Fact]
    public void Write_CompoundPath_UsesSubPathsAndNoNodesOfItsOwn()
    {
        var text = WrittenShapeJson(Build(Compound()));

        Assert.Contains("\"subPaths\"", text);
        // "nodes" appears once per sub-path, never beside the sub-paths.
        Assert.Equal(2, text.Split("\"nodes\"").Length - 1);
    }

    [Fact]
    public void Write_PathWithOneSubPath_WritesNodesAsBefore()
    {
        var single = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)], isClosed: true);

        var text = WrittenShapeJson(Build(single));

        Assert.Contains("\"nodes\"", text);
        Assert.Contains("\"closed\"", text);
        Assert.DoesNotContain("subPaths", text);
    }

    [Fact]
    public void Read_FileWithoutSubPaths_StillLoadsAsOneSubPath()
    {
        var result = ReadShape(PathJson("\"nodes\":" + TwoNodes + ",\"closed\":true"));

        Assert.True(result.Succeeded, result.Detail);
        var path = Assert.IsType<PathShape>(result.Document!.Layers[0].Shapes[0]);
        Assert.Single(path.SubPaths);
        Assert.True(path.SubPaths[0].IsClosed);
    }

    [Fact]
    public void Read_SubPaths_Succeeds()
    {
        var result = ReadShape(PathJson("\"subPaths\":[{\"nodes\":" + TwoNodes + ",\"closed\":true},{\"nodes\":" + TwoNodes + "}]"));

        Assert.True(result.Succeeded, result.Detail);
        var path = Assert.IsType<PathShape>(result.Document!.Layers[0].Shapes[0]);
        Assert.Equal(2, path.SubPaths.Length);
        Assert.True(path.SubPaths[0].IsClosed);
        Assert.False(path.SubPaths[1].IsClosed);
    }

    [Theory]
    [InlineData("\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}],\"subPaths\":[{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]},{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]}]")]
    [InlineData("\"closed\":true,\"subPaths\":[{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]},{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]}]")]
    [InlineData("\"subPaths\":[{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]}]")]
    [InlineData("\"subPaths\":[]")]
    [InlineData("\"subPaths\":[{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]},{\"nodes\":[{\"x\":0,\"y\":0}]}]")]
    public void Read_InvalidSubPaths_FailsWithInvalidValue(string body)
    {
        var result = ReadShape(PathJson(body));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Theory]
    [InlineData("\"subPaths\":[null,{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]}]")]
    [InlineData("\"subPaths\":[{\"closed\":true},{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]}]")]
    [InlineData("\"subPaths\":[{\"nodes\":[{\"x\":0},{\"x\":1,\"y\":1}]},{\"nodes\":[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]}]")]
    public void Read_SubPathWithMissingParts_FailsWithNotProjectFile(string body)
    {
        var result = ReadShape(PathJson(body));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.NotProjectFile, result.Error);
    }

    [Fact]
    public void Read_MoreNodesInAllSubPathsThanTheLimit_FailsWithInvalidValue()
    {
        var half = string.Join(',', Enumerable.Repeat("{\"x\":0,\"y\":0}", ProjectFile.MaxPathNodes / 2 + 1));
        var body = "\"subPaths\":[{\"nodes\":[" + half + "]},{\"nodes\":[" + half + "]}]";

        var result = ReadShape(PathJson(body));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void ToUtf8_CompoundPath_WritesOneMRunPerSubPathInOnePathElement()
    {
        var bytes = SvgExport.ToUtf8(Build(Compound()));
        var svg = XDocument.Parse(new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes));

        var element = svg.Root!.Descendants(Svg + "path").Single();
        var d = (string)element.Attribute("d")!;

        Assert.Equal(2, d.Split('M').Length - 1);
        Assert.Equal(1, d.Split('Z').Length - 1);
        Assert.Equal("evenodd", (string?)element.Attribute("fill-rule"));
    }
}
