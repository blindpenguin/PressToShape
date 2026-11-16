using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class ProjectFileGroupTests
{
    private const string LayerStart =
        "{\"format\":\"PressToShape\",\"version\":2,\"artboard\":{\"width\":100,\"height\":50},\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[";

    private const string LayerEnd = "]}]}";

    private static string Rect(string id, string extra = "") =>
        "{\"type\":\"rectangle\",\"id\":\"" + id + "\",\"x\":1,\"y\":2,\"width\":10,\"height\":20,\"fill\":\"#ff0000ff\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1" + extra + "}";

    private static string Id(int n) => $"bbbbbbbb-0000-0000-0000-{n:D12}";

    private static ProjectFileResult ReadShapes(string shapesJson) =>
        ProjectFile.Read(Encoding.UTF8.GetBytes(LayerStart + shapesJson + LayerEnd));

    private static RectangleShape Box(double x, double y) => new() { X = x, Y = y, Width = 10, Height = 10 };

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

    private static Shape Nested(int levels)
    {
        Shape shape = Box(0, 0);
        for (var i = 0; i < levels; i++)
        {
            shape = new GroupShape { Children = [shape] };
        }

        return shape;
    }

    [Fact]
    public void WriteThenRead_NestedGroupsNamesAndFlags_AreKept()
    {
        var inner = new GroupShape { Name = "Wheels", IsLocked = true, Children = [Box(0, 0) with { Name = "Front", IsVisible = false }, Box(30, 0)] };
        var outer = new GroupShape { Name = "Car", Children = [inner, Box(100, 100) with { IsLocked = true }] };
        var loose = Box(200, 0);
        var document = Build(loose, outer);

        var back = RoundTrip(document);

        var shapes = back.Layers[0].Shapes;
        Assert.Equal([loose.Id, outer.Id], shapes.Select(s => s.Id));
        var backOuter = Assert.IsType<GroupShape>(shapes[1]);
        Assert.Equal("Car", backOuter.Name);
        var backInner = Assert.IsType<GroupShape>(backOuter.Children[0]);
        Assert.Equal(inner.Id, backInner.Id);
        Assert.Equal("Wheels", backInner.Name);
        Assert.True(backInner.IsLocked);
        Assert.Equal("Front", backInner.Children[0].Name);
        Assert.False(backInner.Children[0].IsVisible);
        Assert.True(backInner.Children[1].IsVisible);
        Assert.True(backOuter.Children[1].IsLocked);
        Assert.Equal(outer.Bounds, backOuter.Bounds);
        Assert.Equal(new Bounds(0, 0, 110, 110), backOuter.Bounds);
    }

    [Fact]
    public void Write_Group_HasNoBoxAndOptionalEntriesOnlyWhenSet()
    {
        var group = new GroupShape { Children = [Box(0, 0)] };
        var plain = Box(50, 50);

        using var json = JsonDocument.Parse(ProjectFile.Write(Build(plain, group)));

        var shapes = json.RootElement.GetProperty("layers")[0].GetProperty("shapes");
        Assert.Equal("group", shapes[1].GetProperty("type").GetString());
        Assert.False(shapes[1].TryGetProperty("x", out _));
        Assert.False(shapes[1].TryGetProperty("width", out _));
        Assert.False(shapes[1].TryGetProperty("opacity", out _));
        Assert.Equal(1, shapes[1].GetProperty("children").GetArrayLength());
        Assert.False(shapes[0].TryGetProperty("name", out _));
        Assert.False(shapes[0].TryGetProperty("visible", out _));
        Assert.False(shapes[0].TryGetProperty("locked", out _));
        Assert.False(shapes[0].TryGetProperty("children", out _));
    }

    [Fact]
    public void Write_HiddenLockedNamedShape_WritesTheEntries()
    {
        var shape = Box(0, 0) with { Name = "Sun", IsVisible = false, IsLocked = true };

        using var json = JsonDocument.Parse(ProjectFile.Write(Build(shape)));

        var written = json.RootElement.GetProperty("layers")[0].GetProperty("shapes")[0];
        Assert.Equal("Sun", written.GetProperty("name").GetString());
        Assert.False(written.GetProperty("visible").GetBoolean());
        Assert.True(written.GetProperty("locked").GetBoolean());
    }

    [Fact]
    public void Read_FileFromBeforeGroups_LoadsWithDefaults()
    {
        var json = LayerStart.Replace("\"version\":2", "\"version\":1") + Rect(Id(1)) + LayerEnd;

        var result = ProjectFile.Read(Encoding.UTF8.GetBytes(json));

        Assert.True(result.Succeeded, result.Detail);
        var shape = Assert.Single(result.Document!.Layers[0].Shapes);
        Assert.True(shape.IsVisible);
        Assert.False(shape.IsLocked);
        Assert.Equal(string.Empty, shape.Name);
    }

    [Fact]
    public void Read_GroupInFile_BoxIsDerivedFromChildren()
    {
        var json = "{\"type\":\"group\",\"id\":\"" + Id(9) + "\",\"children\":[" + Rect(Id(1)) + "," + Rect(Id(2)) + "]}";

        var result = ReadShapes(json);

        Assert.True(result.Succeeded, result.Detail);
        var group = Assert.IsType<GroupShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(new Bounds(1, 2, 10, 20), group.Bounds);
    }

    [Fact]
    public void Read_64NestedGroups_Loads()
    {
        var result = ProjectFile.Read(ProjectFile.Write(Build(Nested(ShapeTree.MaxDepth))));

        Assert.True(result.Succeeded, result.Detail);
    }

    [Fact]
    public void Read_65NestedGroups_FailsWithInvalidValue()
    {
        var json = new StringBuilder();
        for (var i = 0; i < ShapeTree.MaxDepth + 1; i++)
        {
            json.Append("{\"type\":\"group\",\"id\":\"").Append(Id(1000 + i)).Append("\",\"children\":[");
        }

        json.Append(Rect(Id(1)));
        json.Append(string.Concat(Enumerable.Repeat("]}", ShapeTree.MaxDepth + 1)));

        var result = ReadShapes(json.ToString());

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void Read_EmptyGroup_FailsWithInvalidValue()
    {
        var result = ReadShapes("{\"type\":\"group\",\"id\":\"" + Id(9) + "\",\"children\":[]}");

        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void Read_GroupWithoutChildren_FailsWithNotProjectFile()
    {
        var result = ReadShapes("{\"type\":\"group\",\"id\":\"" + Id(9) + "\"}");

        Assert.Equal(ProjectFileError.NotProjectFile, result.Error);
    }

    [Fact]
    public void Read_GroupWithNullChild_FailsWithNotProjectFile()
    {
        var result = ReadShapes("{\"type\":\"group\",\"id\":\"" + Id(9) + "\",\"children\":[null]}");

        Assert.Equal(ProjectFileError.NotProjectFile, result.Error);
    }

    [Fact]
    public void Read_DuplicateIdInsideGroup_FailsWithInvalidValue()
    {
        var json = Rect(Id(1)) + ",{\"type\":\"group\",\"id\":\"" + Id(9) + "\",\"children\":[" + Rect(Id(1)) + "]}";

        var result = ReadShapes(json);

        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void Read_GroupIdSameAsChild_FailsWithInvalidValue()
    {
        var json = "{\"type\":\"group\",\"id\":\"" + Id(1) + "\",\"children\":[" + Rect(Id(1)) + "]}";

        Assert.Equal(ProjectFileError.InvalidValue, ReadShapes(json).Error);
    }

    [Fact]
    public void Read_UnknownTypeInsideGroup_FailsWithUnknownShapeType()
    {
        var json = "{\"type\":\"group\",\"id\":\"" + Id(9) + "\",\"children\":[{\"type\":\"star\",\"id\":\"" + Id(1) + "\",\"x\":0,\"y\":0,\"width\":1,\"height\":1,\"fill\":null,\"stroke\":null,\"strokeWidth\":1,\"opacity\":1}]}";

        Assert.Equal(ProjectFileError.UnknownShapeType, ReadShapes(json).Error);
    }

    [Fact]
    public void Read_FlagsAndNameInFile_AreRead()
    {
        var result = ReadShapes(Rect(Id(1), ",\"name\":\"Sun\",\"visible\":false,\"locked\":true"));

        var shape = Assert.Single(result.Document!.Layers[0].Shapes);
        Assert.Equal("Sun", shape.Name);
        Assert.False(shape.IsVisible);
        Assert.True(shape.IsLocked);
    }

    [Fact]
    public void Read_NewerVersionThanKnown_Fails()
    {
        var json = LayerStart.Replace("\"version\":2", "\"version\":3") + LayerEnd;

        Assert.Equal(ProjectFileError.NewerVersion, ProjectFile.Read(Encoding.UTF8.GetBytes(json)).Error);
    }

    [Fact]
    public void Read_NullBytesOrArbitraryText_NeverThrows()
    {
        Assert.False(ProjectFile.Read("{\"format\":\"PressToShape\",\"version\":2,\"layers\":[{\"shapes\":[{\"type\":\"group\"}]}]}"u8).Succeeded);
    }
}
