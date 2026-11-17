using System.Linq;
using System.Text;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class ProjectFilePathTests
{
    private const string LayerStart =
        "{\"format\":\"PressToShape\",\"version\":2,\"artboard\":{\"width\":100,\"height\":50},\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[";

    private const string LayerEnd = "]}]}";

    private static string PathJson(string nodes, string extra = "") =>
        "{\"type\":\"path\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"x\":1,\"y\":2,\"width\":10,\"height\":20,\"fill\":\"#ff0000ff\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1,\"nodes\":" + nodes + extra + "}";

    private const string TwoNodes = "[{\"x\":0,\"y\":0},{\"x\":1,\"y\":1}]";

    private static ProjectFileResult ReadShape(string shapeJson) =>
        ProjectFile.Read(Encoding.UTF8.GetBytes(LayerStart + shapeJson + LayerEnd));

    private static Document RoundTrip(Document document)
    {
        var result = ProjectFile.Read(ProjectFile.Write(document));
        Assert.True(result.Succeeded, result.Detail);
        return result.Document!;
    }

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("L");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static PathShape Sample() =>
        PathShape.Create(
            [
                new DocumentNode(10, 20, outX: 40, outY: -10),
                new DocumentNode(110, 20, inX: 80, inY: 90, outX: 120, outY: 60),
                new DocumentNode(60, 120),
            ],
            isClosed: true,
            fillRule: FillRule.EvenOdd);

    private static void AssertSamePath(PathShape expected, PathShape actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Bounds, actual.Bounds);
        Assert.Equal(expected.SubPaths[0].Nodes.AsSpan().ToArray(), actual.SubPaths[0].Nodes.AsSpan().ToArray());
        Assert.Equal(expected.SubPaths[0].IsClosed, actual.SubPaths[0].IsClosed);
        Assert.Equal(expected.FillRule, actual.FillRule);
        Assert.Equal(expected.Rotation, actual.Rotation);
        Assert.Equal(expected.Appearance, actual.Appearance);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.IsVisible, actual.IsVisible);
        Assert.Equal(expected.IsLocked, actual.IsLocked);
    }

    [Fact]
    public void WriteThenRead_PathWithCurvesClosedEvenOddAndRotation_IsKept()
    {
        var path = Sample() with { Rotation = 33.5, Name = "Blob", IsLocked = true, IsVisible = false };

        var loaded = RoundTrip(Build(path)).Layers[0].Shapes[0];

        AssertSamePath(path, Assert.IsType<PathShape>(loaded));
    }

    [Fact]
    public void WriteThenRead_OpenPathWithDefaults_WritesNoClosedOrFillRuleEntry()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);

        var json = Encoding.UTF8.GetString(ProjectFile.Write(Build(path)));
        var loaded = Assert.IsType<PathShape>(RoundTrip(Build(path)).Layers[0].Shapes[0]);

        Assert.DoesNotContain("\"closed\"", json);
        Assert.DoesNotContain("\"fillRule\"", json);
        Assert.DoesNotContain("\"inX\"", json);
        Assert.False(loaded.SubPaths[0].IsClosed);
        Assert.Equal(FillRule.NonZero, loaded.FillRule);
    }

    [Fact]
    public void WriteThenRead_PathInsideAGroup_IsKept()
    {
        var path = Sample();
        var group = new GroupShape { Children = [path, new RectangleShape { Width = 5, Height = 5 }] };

        var loaded = Assert.IsType<GroupShape>(RoundTrip(Build(group)).Layers[0].Shapes[0]);

        AssertSamePath(path, Assert.IsType<PathShape>(loaded.Children[0]));
    }

    [Fact]
    public void Read_MinimalPath_Succeeds()
    {
        var result = ReadShape(PathJson(TwoNodes));

        Assert.True(result.Succeeded, result.Detail);
        var path = Assert.IsType<PathShape>(result.Document!.Layers[0].Shapes[0]);
        Assert.Equal(2, path.SubPaths[0].Nodes.Length);
        Assert.False(path.SubPaths[0].IsClosed);
    }

    [Fact]
    public void Read_PathWithOneNode_FailsWithInvalidValue()
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0}]"));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void Read_PathWithEmptyNodes_FailsWithInvalidValue()
    {
        Assert.Equal(ProjectFileError.InvalidValue, ReadShape(PathJson("[]")).Error);
    }

    [Fact]
    public void Read_PathWithoutNodes_FailsWithNotProjectFile()
    {
        var json = "{\"type\":\"path\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"x\":1,\"y\":2,\"width\":10,\"height\":20,\"fill\":null,\"stroke\":null,\"strokeWidth\":1,\"opacity\":1}";

        var result = ReadShape(json);

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.NotProjectFile, result.Error);
    }

    [Fact]
    public void Read_NodeWithoutAnY_FailsWithNotProjectFile()
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0},{\"x\":1}]"));

        Assert.Equal(ProjectFileError.NotProjectFile, result.Error);
    }

    [Fact]
    public void Read_NullNode_FailsWithNotProjectFile()
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0},null]"));

        Assert.Equal(ProjectFileError.NotProjectFile, result.Error);
    }

    [Fact]
    public void Read_HalfAHandle_FailsWithInvalidValue()
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0,\"outX\":0.5},{\"x\":1,\"y\":1}]"));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void Read_NumberThatIsNotFinite_Fails()
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0},{\"x\":1e999,\"y\":1}]"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Read_NumberWrittenAsText_Fails()
    {
        var result = ReadShape(PathJson("[{\"x\":0,\"y\":0},{\"x\":\"NaN\",\"y\":1}]"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Read_UnknownFillRule_FailsWithInvalidValue()
    {
        var result = ReadShape(PathJson(TwoNodes, ",\"fillRule\":\"sideways\""));

        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void Read_TooManyNodes_FailsWithInvalidValue()
    {
        var nodes = "[" + string.Join(",", Enumerable.Repeat("{\"x\":0,\"y\":0}", ProjectFile.MaxPathNodes + 1)) + "]";

        var result = ReadShape(PathJson(nodes));

        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void Read_PathWithNegativeWidth_FailsWithInvalidValue()
    {
        var json = PathJson(TwoNodes).Replace("\"width\":10", "\"width\":-10");

        Assert.Equal(ProjectFileError.InvalidValue, ReadShape(json).Error);
    }

    [Fact]
    public void Read_FileWithoutPaths_StillLoads()
    {
        var rectangle = "{\"type\":\"rectangle\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000002\",\"x\":1,\"y\":2,\"width\":10,\"height\":20,\"fill\":\"#ff0000ff\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1}";

        var result = ReadShape(rectangle);

        Assert.True(result.Succeeded, result.Detail);
        Assert.IsType<RectangleShape>(result.Document!.Layers[0].Shapes[0]);
    }
}
