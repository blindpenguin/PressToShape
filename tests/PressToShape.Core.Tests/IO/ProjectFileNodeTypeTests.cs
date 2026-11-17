using System.Linq;
using System.Text;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class ProjectFileNodeTypeTests
{
    private const string LayerStart =
        "{\"format\":\"PressToShape\",\"version\":2,\"artboard\":{\"width\":100,\"height\":50},\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[";

    private const string LayerEnd = "]}]}";

    private static string PathJson(string nodes) =>
        "{\"type\":\"path\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"x\":1,\"y\":2,\"width\":10,\"height\":20,\"fill\":null,\"stroke\":null,\"strokeWidth\":1,\"opacity\":1,\"nodes\":" + nodes + "}";

    private static ProjectFileResult ReadShape(string shapeJson) =>
        ProjectFile.Read(Encoding.UTF8.GetBytes(LayerStart + shapeJson + LayerEnd));

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("L");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static PathShape RoundTrip(PathShape path)
    {
        var result = ProjectFile.Read(ProjectFile.Write(Build(path)));
        Assert.True(result.Succeeded, result.Detail);
        return Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers.SelectMany(l => l.Shapes)));
    }

    private static string Json(PathShape path) => Encoding.UTF8.GetString(ProjectFile.Write(Build(path)));

    [Fact]
    public void WriteThenRead_EachType_IsKept()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 0, outX: 20, outY: 0, type: NodeType.Corner),
                new DocumentNode(100, 0, inX: 80, inY: 0, outX: 130, outY: 0, type: NodeType.Smooth),
                new DocumentNode(100, 100, inX: 120, inY: 100, outX: 80, outY: 100, type: NodeType.Symmetric),
                new DocumentNode(0, 100),
            ]);

        var loaded = RoundTrip(path);

        Assert.Equal(path.SubPaths[0].Nodes.Select(n => n.Type), loaded.SubPaths[0].Nodes.Select(n => n.Type));
        Assert.Equal(path.SubPaths[0].Nodes.AsSpan().ToArray(), loaded.SubPaths[0].Nodes.AsSpan().ToArray());
    }

    [Fact]
    public void WriteThenRead_CornerWithHandlesOnOneLine_StaysACorner()
    {
        // The handles would derive to symmetric, so the type has to be written or the corner would be lost.
        var path = PathShape.Create(
            [new DocumentNode(0, 50), new DocumentNode(50, 50, inX: 30, inY: 50, outX: 70, outY: 50, type: NodeType.Corner), new DocumentNode(100, 50)]);

        var loaded = RoundTrip(path);

        Assert.Equal(NodeType.Corner, loaded.SubPaths[0].Nodes[1].Type);
        Assert.Contains("\"type\": \"corner\"", Json(path));
    }

    [Fact]
    public void WriteThenRead_SmoothNodeThatWouldDeriveAsSymmetric_StaysSmooth()
    {
        var path = PathShape.Create(
            [new DocumentNode(0, 50), new DocumentNode(50, 50, inX: 30, inY: 50, outX: 70, outY: 50, type: NodeType.Smooth), new DocumentNode(100, 50)]);

        Assert.Equal(NodeType.Smooth, RoundTrip(path).SubPaths[0].Nodes[1].Type);
    }

    [Fact]
    public void Write_TypeThatEqualsTheDerivedType_IsNotWritten()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 0),
                new DocumentNode(50, 50, inX: 30, inY: 50, outX: 70, outY: 50, type: NodeType.Symmetric),
                new DocumentNode(100, 100, inX: 80, inY: 100, outX: 130, outY: 100, type: NodeType.Smooth),
            ]);

        var json = Json(path);

        Assert.DoesNotContain("\"type\": \"symmetric\"", json);
        Assert.DoesNotContain("\"type\": \"smooth\"", json);
        Assert.DoesNotContain("\"type\": \"corner\"", json);
    }

    [Fact]
    public void Read_FileWithoutTypes_GetsTheTypesThatTheHandlesGive()
    {
        var nodes = "[{\"x\":0,\"y\":0},{\"x\":0.5,\"y\":0.5,\"inX\":0.3,\"inY\":0.5,\"outX\":0.7,\"outY\":0.5},{\"x\":1,\"y\":1,\"inX\":0.9,\"inY\":1,\"outX\":1.4,\"outY\":1},{\"x\":1,\"y\":0,\"outX\":1,\"outY\":0.5}]";

        var result = ReadShape(PathJson(nodes));

        Assert.True(result.Succeeded, result.Detail);
        var path = Assert.IsType<PathShape>(result.Document!.Layers[0].Shapes[0]);
        Assert.Equal([NodeType.Corner, NodeType.Symmetric, NodeType.Smooth, NodeType.Corner], path.SubPaths[0].Nodes.Select(n => n.Type));
    }

    [Theory]
    [InlineData("corner", NodeType.Corner)]
    [InlineData("smooth", NodeType.Smooth)]
    [InlineData("symmetric", NodeType.Symmetric)]
    public void Read_NodeWithAType_UsesIt(string text, NodeType expected)
    {
        var nodes = "[{\"x\":0,\"y\":0,\"type\":\"" + text + "\"},{\"x\":1,\"y\":1}]";

        var result = ReadShape(PathJson(nodes));

        Assert.True(result.Succeeded, result.Detail);
        Assert.Equal(expected, ((PathShape)result.Document!.Layers[0].Shapes[0]).SubPaths[0].Nodes[0].Type);
    }

    [Fact]
    public void Read_UnknownType_FailsWithInvalidValue()
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0,\"type\":\"wobbly\"},{\"x\":1,\"y\":1}]"));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Theory]
    [InlineData("\"type\":5")]
    [InlineData("\"type\":true")]
    [InlineData("\"type\":{}")]
    public void Read_TypeThatIsNotText_Fails(string entry)
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0," + entry + "},{\"x\":1,\"y\":1}]"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Read_UpperCaseType_IsUnknown()
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0,\"type\":\"Smooth\"},{\"x\":1,\"y\":1}]"));

        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void WriteThenRead_PathInsideAGroup_KeepsItsTypes()
    {
        var path = PathShape.Create(
            [new DocumentNode(0, 50), new DocumentNode(50, 50, inX: 30, inY: 50, outX: 70, outY: 50, type: NodeType.Corner), new DocumentNode(100, 50)]);
        var group = new GroupShape { Children = [path, new RectangleShape { Width = 5, Height = 5 }] };

        var result = ProjectFile.Read(ProjectFile.Write(Build(group)));

        Assert.True(result.Succeeded, result.Detail);
        var loaded = Assert.IsType<GroupShape>(result.Document!.Layers[0].Shapes[0]);
        Assert.Equal(NodeType.Corner, Assert.IsType<PathShape>(loaded.Children[0]).SubPaths[0].Nodes[1].Type);
    }
}
