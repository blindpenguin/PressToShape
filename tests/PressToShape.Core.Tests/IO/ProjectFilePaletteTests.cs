using System;
using System.Linq;
using System.Text;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class ProjectFilePaletteTests
{
    private const string Prefix =
        "{\"format\":\"PressToShape\",\"version\":1,\"artboard\":{\"width\":100,\"height\":50},";

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
    public void WriteThenRead_Palette_KeepsOrderAndAlpha()
    {
        var first = new Color(1, 2, 3, 255);
        var second = new Color(10, 20, 30, 0x80);
        var document = Document.Create("x").AddSwatch(first).AddSwatch(second);

        var back = RoundTrip(document);

        Assert.Equal([first, second], back.Palette.ToArray());
    }

    [Fact]
    public void Write_EmptyPalette_WritesNoPaletteField()
    {
        var text = Encoding.UTF8.GetString(ProjectFile.Write(Document.Create("x")));

        Assert.DoesNotContain("palette", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_FileWithoutPalette_LoadsWithEmptyPalette()
    {
        var result = ReadText(Prefix + Layers + "}");

        Assert.True(result.Succeeded, result.Detail);
        Assert.Empty(result.Document!.Palette);
    }

    [Theory]
    [InlineData("\"palette\":[\"red\"]")]
    [InlineData("\"palette\":[\"#11223\"]")]
    [InlineData("\"palette\":[null]")]
    public void Read_InvalidPaletteEntry_FailsWithInvalidValue(string palette)
    {
        var result = ReadText(Prefix + palette + "," + Layers + "}");

        Assert.False(result.Succeeded);
        Assert.Null(result.Document);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void WriteThenRead_StrokeWithAlpha_IsKept()
    {
        var document = Document.Create("x");
        document = document.AddShape(document.Layers[0].Id, new RectangleShape
        {
            Appearance = new Appearance { Fill = null, Stroke = new Color(5, 6, 7, 0x40) },
        });

        var back = RoundTrip(document);

        Assert.Equal(new Color(5, 6, 7, 0x40), back.Layers[0].Shapes[0].Appearance.Stroke);
    }
}
