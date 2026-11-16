using System.Linq;
using System.Text;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class ProjectFileRadiiTests
{
    private const string LayerStart =
        "{\"format\":\"PressToShape\",\"version\":2,\"artboard\":{\"width\":100,\"height\":50},\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[";

    private const string LayerEnd = "]}]}";

    private static string RectangleJson(string extra) =>
        "{\"type\":\"rectangle\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"x\":1,\"y\":2,\"width\":100,\"height\":40,\"fill\":null,\"stroke\":null,\"strokeWidth\":1,\"opacity\":1" + extra + "}";

    private static ProjectFileResult ReadShape(string shapeJson) =>
        ProjectFile.Read(Encoding.UTF8.GetBytes(LayerStart + shapeJson + LayerEnd));

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("L");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static RectangleShape RoundTrip(RectangleShape rectangle)
    {
        var result = ProjectFile.Read(ProjectFile.Write(Build(rectangle)));
        Assert.True(result.Succeeded, result.Detail);
        return Assert.IsType<RectangleShape>(Assert.Single(result.Document!.Layers.SelectMany(l => l.Shapes)));
    }

    [Fact]
    public void WriteThenRead_EqualRadii_AreKept()
    {
        var loaded = RoundTrip(new RectangleShape { Width = 100, Height = 40, Radii = CornerRadii.Uniform(12.5) });

        Assert.Equal(CornerRadii.Uniform(12.5), loaded.Radii);
    }

    [Fact]
    public void WriteThenRead_DifferentRadii_AreKeptPerCorner()
    {
        var loaded = RoundTrip(new RectangleShape { Width = 100, Height = 40, Radii = new CornerRadii(1, 2, 3, 4) });

        Assert.Equal(new CornerRadii(1, 2, 3, 4), loaded.Radii);
    }

    [Fact]
    public void Write_ZeroRadii_WritesNoRadiiField()
    {
        var json = Encoding.UTF8.GetString(ProjectFile.Write(Build(new RectangleShape { Width = 100, Height = 40 })));

        Assert.DoesNotContain("radii", json);
    }

    [Fact]
    public void Write_Radii_AreWrittenInTheOrderTopLeftTopRightBottomRightBottomLeft()
    {
        var json = Encoding.UTF8.GetString(ProjectFile.Write(Build(new RectangleShape { Width = 100, Height = 40, Radii = new CornerRadii(1, 2, 3, 4) })));

        using var document = System.Text.Json.JsonDocument.Parse(json);
        var shape = document.RootElement.GetProperty("layers")[0].GetProperty("shapes")[0];
        Assert.Equal([1.0, 2.0, 3.0, 4.0], shape.GetProperty("radii").EnumerateArray().Select(value => value.GetDouble()).ToArray());
    }

    [Fact]
    public void Write_RadiusAboveTheLimit_IsWrittenAsStored()
    {
        var loaded = RoundTrip(new RectangleShape { Width = 100, Height = 40, Radii = CornerRadii.Uniform(500) });

        Assert.Equal(CornerRadii.Uniform(500), loaded.Radii);
        Assert.Equal(CornerRadii.Uniform(20), loaded.EffectiveRadii);
    }

    [Fact]
    public void Read_RectangleWithoutRadii_HasSquareCorners()
    {
        var result = ReadShape(RectangleJson(string.Empty));

        Assert.True(result.Succeeded, result.Detail);
        var rectangle = Assert.IsType<RectangleShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.True(rectangle.Radii.IsZero);
    }

    [Fact]
    public void Read_RadiusAboveTheLimit_LoadsAndIsLimitedWhenUsed()
    {
        var result = ReadShape(RectangleJson(",\"radii\":[500,0,0,30]"));

        Assert.True(result.Succeeded, result.Detail);
        var rectangle = Assert.IsType<RectangleShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(new CornerRadii(500, 0, 0, 30), rectangle.Radii);
        Assert.Equal(new CornerRadii(20, 0, 0, 20), rectangle.EffectiveRadii);
    }

    [Theory]
    [InlineData(",\"radii\":[1,2,3]")]
    [InlineData(",\"radii\":[1,2,3,4,5]")]
    [InlineData(",\"radii\":[]")]
    [InlineData(",\"radii\":[-1,0,0,0]")]
    [InlineData(",\"radii\":[0,0,0,-0.5]")]
    [InlineData(",\"radii\":[1,null,3,4]")]
    public void Read_InvalidRadii_FailsWithInvalidValue(string extra)
    {
        var result = ReadShape(RectangleJson(extra));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Theory]
    [InlineData(",\"radii\":\"5\"")]
    [InlineData(",\"radii\":[\"a\",0,0,0]")]
    [InlineData(",\"radii\":5")]
    public void Read_RadiiOfTheWrongJsonType_Fails(string extra)
    {
        var result = ReadShape(RectangleJson(extra));

        Assert.False(result.Succeeded);
    }
}
