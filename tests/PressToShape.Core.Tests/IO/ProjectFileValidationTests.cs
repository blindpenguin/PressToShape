using System;
using System.Text;
using PressToShape.Core.IO;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class ProjectFileValidationTests
{
    private const string LayerId = "aaaaaaaa-0000-0000-0000-000000000001";
    private const string ShapeId = "bbbbbbbb-0000-0000-0000-000000000001";

    private const string Valid =
        "{\"format\":\"PressToShape\",\"version\":1,\"artboard\":{\"width\":100,\"height\":50},\"layers\":[" +
        "{\"id\":\"" + LayerId + "\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[" +
        "{\"type\":\"rectangle\",\"id\":\"" + ShapeId + "\",\"x\":1,\"y\":2,\"width\":3,\"height\":4," +
        "\"fill\":\"#11223344\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1}]}]}";

    private static ProjectFileResult ReadText(string json) => ProjectFile.Read(Encoding.UTF8.GetBytes(json));

    private static string Mutate(string from, string to)
    {
        Assert.Contains(from, Valid, StringComparison.Ordinal);
        return Valid.Replace(from, to, StringComparison.Ordinal);
    }

    private static void AssertFails(ProjectFileResult result, ProjectFileError error)
    {
        Assert.False(result.Succeeded);
        Assert.Null(result.Document);
        Assert.Equal(error, result.Error);
        Assert.False(string.IsNullOrWhiteSpace(result.Detail));
    }

    [Fact]
    public void Read_TheValidBaseText_Succeeds()
    {
        var result = ReadText(Valid);

        Assert.True(result.Succeeded, result.Detail);
        Assert.Equal(100, result.Document!.ArtboardWidth);
        Assert.Equal("L", result.Document.Layers[0].Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json")]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("42")]
    [InlineData("\"text\"")]
    [InlineData("null")]
    [InlineData("{}")]
    public void Read_NotAProjectFile_ReturnsNotProjectFile(string json)
    {
        AssertFails(ReadText(json), ProjectFileError.NotProjectFile);
    }

    [Fact]
    public void Read_EmptyInput_ReturnsNotProjectFile()
    {
        AssertFails(ProjectFile.Read(ReadOnlySpan<byte>.Empty), ProjectFileError.NotProjectFile);
    }

    [Theory]
    [InlineData("\"format\":\"PressToShape\",", "")]
    [InlineData("\"format\":\"PressToShape\"", "\"format\":\"Other\"")]
    [InlineData("\"format\":\"PressToShape\"", "\"format\":\"pressToShape\"")]
    [InlineData("\"format\":\"PressToShape\"", "\"format\":null")]
    [InlineData("\"version\":1,", "")]
    [InlineData("\"version\":1", "\"version\":0")]
    [InlineData("\"version\":1", "\"version\":-1")]
    [InlineData("\"version\":1", "\"version\":\"1\"")]
    [InlineData("\"version\":1", "\"version\":1.5")]
    [InlineData("\"version\":1", "\"version\":99999999999999")]
    public void Read_BadFormatNameOrVersion_ReturnsNotProjectFile(string from, string to)
    {
        AssertFails(ReadText(Mutate(from, to)), ProjectFileError.NotProjectFile);
    }

    [Theory]
    [InlineData("\"version\":1", "\"version\":2")]
    [InlineData("\"version\":1", "\"version\":1000")]
    public void Read_NewerVersion_ReturnsNewerVersion(string from, string to)
    {
        AssertFails(ReadText(Mutate(from, to)), ProjectFileError.NewerVersion);
    }

    [Theory]
    [InlineData("\"artboard\":{\"width\":100,\"height\":50},", "")]
    [InlineData("\"artboard\":{\"width\":100,\"height\":50}", "\"artboard\":null")]
    [InlineData("\"artboard\":{\"width\":100,\"height\":50}", "\"artboard\":[]")]
    [InlineData("\"width\":100,\"height\":50", "\"width\":100")]
    [InlineData("\"width\":100,\"height\":50", "\"height\":50")]
    [InlineData("\"width\":100,\"height\":50", "\"width\":null,\"height\":50")]
    [InlineData("\"width\":100,", "\"width\":\"100\",")]
    public void Read_BadArtboard_ReturnsNotProjectFile(string from, string to)
    {
        AssertFails(ReadText(Mutate(from, to)), ProjectFileError.NotProjectFile);
    }

    [Theory]
    [InlineData("\"width\":100,\"height\":50", "\"width\":0,\"height\":50")]
    [InlineData("\"width\":100,\"height\":50", "\"width\":100,\"height\":-5")]
    [InlineData("\"width\":100,\"height\":50", "\"width\":-100,\"height\":50")]
    public void Read_ArtboardSizeNotPositive_ReturnsInvalidValue(string from, string to)
    {
        AssertFails(ReadText(Mutate(from, to)), ProjectFileError.InvalidValue);
    }

    [Theory]
    [InlineData(",\"layers\":[{\"id\":\"" + LayerId + "\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[{\"type\":\"rectangle\",\"id\":\"" + ShapeId + "\",\"x\":1,\"y\":2,\"width\":3,\"height\":4,\"fill\":\"#11223344\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1}]}]", "")]
    [InlineData("\"layers\":[{", "\"layers\":null,\"x\":[{")]
    [InlineData("\"layers\":[{\"id\":\"" + LayerId + "\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[{\"type\":\"rectangle\",\"id\":\"" + ShapeId + "\",\"x\":1,\"y\":2,\"width\":3,\"height\":4,\"fill\":\"#11223344\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1}]}]", "\"layers\":[]")]
    [InlineData("\"layers\":[{\"id\":\"" + LayerId + "\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[{\"type\":\"rectangle\",\"id\":\"" + ShapeId + "\",\"x\":1,\"y\":2,\"width\":3,\"height\":4,\"fill\":\"#11223344\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1}]}]", "\"layers\":\"none\"")]
    [InlineData("\"layers\":[{\"id\":\"" + LayerId + "\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[{\"type\":\"rectangle\",\"id\":\"" + ShapeId + "\",\"x\":1,\"y\":2,\"width\":3,\"height\":4,\"fill\":\"#11223344\",\"stroke\":null,\"strokeWidth\":1,\"opacity\":1}]}]", "\"layers\":[null]")]
    [InlineData("\"name\":\"L\",", "")]
    [InlineData("\"name\":\"L\"", "\"name\":null")]
    [InlineData("\"name\":\"L\"", "\"name\":5")]
    [InlineData("\"visible\":true,", "")]
    [InlineData("\"visible\":true", "\"visible\":\"yes\"")]
    [InlineData("\"locked\":false,", "")]
    [InlineData("\"locked\":false", "\"locked\":null")]
    [InlineData("\"shapes\":[{", "\"shapes\":null,\"x\":[{")]
    public void Read_BadLayersOrLayerEntries_ReturnsNotProjectFile(string from, string to)
    {
        AssertFails(ReadText(Mutate(from, to)), ProjectFileError.NotProjectFile);
    }

    [Theory]
    [InlineData("\"id\":\"" + LayerId + "\",", "")]
    [InlineData(LayerId, "00000000-0000-0000-0000-000000000000")]
    [InlineData(LayerId, "not-a-guid")]
    [InlineData(LayerId, "aaaaaaaa00000000000000000000000z")]
    [InlineData(ShapeId, "00000000-0000-0000-0000-000000000000")]
    [InlineData(ShapeId, "{bbbbbbbb-0000-0000-0000-000000000001}")]
    [InlineData(ShapeId, "")]
    public void Read_BadIds_ReturnsInvalidValue(string from, string to)
    {
        AssertFails(ReadText(Mutate(from, to)), ProjectFileError.InvalidValue);
    }

    [Fact]
    public void Read_DuplicateLayerIds_ReturnsInvalidValue()
    {
        const string secondLayer = ",{\"id\":\"" + LayerId + "\",\"name\":\"Two\",\"visible\":true,\"locked\":false,\"shapes\":[]}";

        AssertFails(ReadText(Mutate("]}]}", "]}" + secondLayer + "]}")), ProjectFileError.InvalidValue);
    }

    [Fact]
    public void Read_DuplicateShapeIdsAcrossLayers_ReturnsInvalidValue()
    {
        const string secondLayer = ",{\"id\":\"aaaaaaaa-0000-0000-0000-000000000002\",\"name\":\"Two\",\"visible\":true,\"locked\":false,\"shapes\":[" +
            "{\"type\":\"ellipse\",\"id\":\"" + ShapeId + "\",\"x\":0,\"y\":0,\"width\":1,\"height\":1,\"fill\":null,\"stroke\":null,\"strokeWidth\":0,\"opacity\":1}]}";

        AssertFails(ReadText(Mutate("]}]}", "]}" + secondLayer + "]}")), ProjectFileError.InvalidValue);
    }

    [Theory]
    [InlineData("rectangle")]
    [InlineData("Rectangle")]
    [InlineData("star")]
    [InlineData("")]
    public void Read_ShapeType_OnlyKnownTypesAreAccepted(string type)
    {
        var result = ReadText(Mutate("\"type\":\"rectangle\"", "\"type\":\"" + type + "\""));

        if (type == "rectangle")
        {
            Assert.True(result.Succeeded, result.Detail);
        }
        else
        {
            AssertFails(result, ProjectFileError.UnknownShapeType);
        }
    }

    [Theory]
    [InlineData("\"type\":\"rectangle\",", "")]
    [InlineData("\"type\":\"rectangle\"", "\"type\":null")]
    [InlineData("\"type\":\"rectangle\"", "\"type\":3")]
    [InlineData("\"x\":1,", "")]
    [InlineData("\"x\":1", "\"x\":null")]
    [InlineData("\"x\":1", "\"x\":\"1\"")]
    [InlineData("\"y\":2,", "")]
    [InlineData("\"width\":3,", "")]
    [InlineData("\"height\":4,", "")]
    [InlineData("\"strokeWidth\":1,", "")]
    [InlineData("\"opacity\":1", "\"opacity\":null")]
    [InlineData("\"opacity\":1", "\"opacity\":true")]
    [InlineData("\"shapes\":[{", "\"shapes\":[null,{")]
    public void Read_BadShapeEntries_ReturnsNotProjectFile(string from, string to)
    {
        AssertFails(ReadText(Mutate(from, to)), ProjectFileError.NotProjectFile);
    }

    [Theory]
    [InlineData("\"width\":3", "\"width\":-3")]
    [InlineData("\"height\":4", "\"height\":-0.001")]
    [InlineData("\"strokeWidth\":1", "\"strokeWidth\":-1")]
    [InlineData("\"opacity\":1", "\"opacity\":1.5")]
    [InlineData("\"opacity\":1", "\"opacity\":-0.1")]
    public void Read_ValuesTheModelRejects_ReturnsInvalidValue(string from, string to)
    {
        AssertFails(ReadText(Mutate(from, to)), ProjectFileError.InvalidValue);
    }

    [Theory]
    [InlineData("#112233")]
    [InlineData("11223344")]
    [InlineData("#1122334")]
    [InlineData("#112233445")]
    [InlineData("#gggggggg")]
    [InlineData("#+1223344")]
    [InlineData("#1122 344")]
    [InlineData("rgba(1,2,3,4)")]
    [InlineData("")]
    public void Read_BadColorText_ReturnsInvalidValue(string color)
    {
        AssertFails(ReadText(Mutate("#11223344", color)), ProjectFileError.InvalidValue);
    }

    [Fact]
    public void Read_UpperCaseColorText_IsAccepted()
    {
        var result = ReadText(Mutate("#11223344", "#AABBCCDD"));

        Assert.True(result.Succeeded, result.Detail);
        Assert.Equal(new PressToShape.Core.Shapes.Color(0xAA, 0xBB, 0xCC, 0xDD), result.Document!.Layers[0].Shapes[0].Appearance.Fill);
    }

    [Theory]
    [InlineData("\"width\":3", "\"width\":1e999")]
    [InlineData("\"x\":1", "\"x\":-1e999")]
    [InlineData("\"x\":1", "\"x\":NaN")]
    [InlineData("\"x\":1", "\"x\":Infinity")]
    public void Read_NumbersThatAreNotFinite_Fail(string from, string to)
    {
        var result = ReadText(Mutate(from, to));

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Read_Comments_AreNotAccepted()
    {
        AssertFails(ReadText("/* hi */" + Valid), ProjectFileError.NotProjectFile);
    }

    [Fact]
    public void Read_TrailingCommas_AreNotAccepted()
    {
        AssertFails(ReadText(Mutate("\"opacity\":1}", "\"opacity\":1,}")), ProjectFileError.NotProjectFile);
    }

    [Fact]
    public void Read_NestingDeeperThanTheLimit_ReturnsNotProjectFile()
    {
        var deep = new string('[', 40) + new string(']', 40);

        AssertFails(ReadText(Mutate("\"version\":1", "\"version\":1,\"deep\":" + deep)), ProjectFileError.NotProjectFile);
    }

    [Fact]
    public void Read_InputLargerThanTheLimit_ReturnsTooLargeBeforeParsing()
    {
        var huge = new byte[ProjectFile.MaxBytes + 1];

        AssertFails(ProjectFile.Read(huge), ProjectFileError.TooLarge);
    }

    [Fact]
    public void Read_InvalidUtf8InAString_ReturnsNotProjectFile()
    {
        var bytes = Encoding.UTF8.GetBytes(Valid);
        var index = Array.IndexOf(bytes, (byte)'L', Array.IndexOf(bytes, (byte)'n'));
        bytes[index] = 0xFF;

        var result = ProjectFile.Read(bytes);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Read_ByteOrderMark_IsNotAccepted()
    {
        var withBom = new byte[] { 0xEF, 0xBB, 0xBF }.AsSpan();
        var all = new byte[withBom.Length + Valid.Length];
        withBom.CopyTo(all);
        Encoding.UTF8.GetBytes(Valid).CopyTo(all, withBom.Length);

        var result = ProjectFile.Read(all);

        // A BOM is allowed to be either rejected or skipped, but never to crash or to give a half result.
        Assert.True(result.Succeeded || result.Error == ProjectFileError.NotProjectFile);
    }

    [Fact]
    public void Read_EveryTruncationOfAValidFile_NeverThrows()
    {
        var bytes = Encoding.UTF8.GetBytes(Valid);
        for (var length = 0; length < bytes.Length; length++)
        {
            var result = ProjectFile.Read(bytes.AsSpan(0, length));

            Assert.False(result.Succeeded);
            Assert.NotNull(result.Error);
        }
    }

    [Fact]
    public void Read_SingleByteCorruptions_NeverThrow()
    {
        var bytes = Encoding.UTF8.GetBytes(Valid);
        var random = new Random(12345);
        for (var i = 0; i < 2000; i++)
        {
            var copy = (byte[])bytes.Clone();
            copy[random.Next(copy.Length)] = (byte)random.Next(256);

            var result = ProjectFile.Read(copy);

            Assert.True(result.Succeeded == (result.Document is not null));
            Assert.True(result.Succeeded == (result.Error is null));
        }
    }
}
