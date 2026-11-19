using System.Linq;
using System.Text;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class ProjectFileTextTests
{
    private const string LayerStart =
        "{\"format\":\"PressToShape\",\"version\":2,\"artboard\":{\"width\":100,\"height\":50},\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[";

    private const string LayerEnd = "]}]}";

    private static string TextJson(string extra) =>
        "{\"type\":\"text\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"x\":1,\"y\":2,\"width\":30,\"height\":10,\"fill\":null,\"stroke\":null,\"strokeWidth\":1,\"opacity\":1" + extra + "}";

    private static ProjectFileResult ReadShape(string shapeJson) =>
        ProjectFile.Read(Encoding.UTF8.GetBytes(LayerStart + shapeJson + LayerEnd));

    private static TextShape RoundTrip(TextShape text)
    {
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [text]);
        var result = ProjectFile.Read(ProjectFile.Write(document));
        Assert.True(result.Succeeded, result.Detail);
        return Assert.IsType<TextShape>(Assert.Single(result.Document!.Layers.SelectMany(l => l.Shapes)));
    }

    [Fact]
    public void WriteThenRead_EveryField_IsKept()
    {
        var text = new TextShape
        {
            X = 5,
            Y = 6,
            Width = 120,
            Height = 48,
            Rotation = 30,
            Name = "Title",
            IsVisible = false,
            IsLocked = true,
            Appearance = new Appearance { Fill = Color.FromRgb(1, 2, 3), Stroke = null, StrokeWidth = 2, Opacity = 0.5 },
            Text = "Hello\nWorld <&> \"quoted\"",
            IsArea = true,
            FontFamily = "Some Font Not Installed",
            FontSize = 31.5,
            IsBold = true,
            IsItalic = true,
            Alignment = TextAlignType.Right,
            LineSpacing = 1.5,
            LetterSpacing = -2.25,
        };

        var loaded = RoundTrip(text);

        Assert.Equal(text, loaded);
    }

    [Theory]
    [InlineData(TextAlignType.Left)]
    [InlineData(TextAlignType.Center)]
    [InlineData(TextAlignType.Right)]
    public void WriteThenRead_Alignment_IsKept(TextAlignType alignment)
    {
        Assert.Equal(alignment, RoundTrip(new TextShape { Text = "a", Alignment = alignment }).Alignment);
    }

    [Fact]
    public void Write_DefaultText_LeavesOutTheOptionalEntries()
    {
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [new TextShape { Text = "a" }]);

        var json = Encoding.UTF8.GetString(ProjectFile.Write(document));

        Assert.DoesNotContain("\"area\"", json);
        Assert.DoesNotContain("\"bold\"", json);
        Assert.DoesNotContain("\"italic\"", json);
        Assert.DoesNotContain("\"align\"", json);
        Assert.DoesNotContain("\"lineSpacing\"", json);
        Assert.DoesNotContain("\"letterSpacing\"", json);
        Assert.Contains("\"font\"", json);
    }

    [Fact]
    public void Read_TextWithoutOptionalEntries_UsesTheDefaults()
    {
        var result = ReadShape(TextJson(string.Empty));

        Assert.True(result.Succeeded, result.Detail);
        var text = Assert.IsType<TextShape>(result.Document!.Layers[0].Shapes[0]);
        Assert.Equal(string.Empty, text.Text);
        Assert.False(text.IsArea);
        Assert.Equal(TextShape.DefaultFontFamily, text.FontFamily);
        Assert.Equal(TextShape.DefaultFontSize, text.FontSize);
        Assert.Equal(TextAlignType.Left, text.Alignment);
        Assert.Equal(1, text.LineSpacing);
        Assert.Equal(0, text.LetterSpacing);
    }

    [Fact]
    public void Read_UnknownAlignment_FailsWithInvalidValue()
    {
        var result = ReadShape(TextJson(",\"align\":\"justify\""));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Theory]
    [InlineData(",\"fontSize\":0")]
    [InlineData(",\"fontSize\":5000")]
    [InlineData(",\"lineSpacing\":0")]
    [InlineData(",\"letterSpacing\":9999")]
    [InlineData(",\"font\":\"\"")]
    public void Read_ValueOutOfRange_FailsWithInvalidValue(string extra)
    {
        var result = ReadShape(TextJson(extra));

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.InvalidValue, result.Error);
    }

    [Fact]
    public void Read_TextMissingItsBox_FailsAsNotAProjectFile()
    {
        var result = ReadShape("{\"type\":\"text\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"text\":\"a\"}");

        Assert.False(result.Succeeded);
        Assert.Equal(ProjectFileError.NotProjectFile, result.Error);
    }

    [Fact]
    public void WriteThenRead_DocumentWithoutText_StillLoads()
    {
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [new RectangleShape { Width = 10, Height = 5 }]);

        var result = ProjectFile.Read(ProjectFile.Write(document));

        Assert.True(result.Succeeded, result.Detail);
        Assert.IsType<RectangleShape>(result.Document!.Layers[0].Shapes[0]);
    }

    [Fact]
    public void WriteThenRead_TextInAGroup_IsKept()
    {
        var text = new TextShape { X = 1, Y = 1, Width = 20, Height = 10, Text = "in group" };
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [text, new RectangleShape { Width = 10, Height = 5 }]);
        document = GroupOperations.Group(document, [text.Id, document.Layers[0].Shapes[1].Id], System.Guid.NewGuid());

        var result = ProjectFile.Read(ProjectFile.Write(document));

        Assert.True(result.Succeeded, result.Detail);
        var group = Assert.IsType<GroupShape>(result.Document!.Layers[0].Shapes[0]);
        Assert.Contains(group.Children, child => child is TextShape { Text: "in group" });
    }
}
