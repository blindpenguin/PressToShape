using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class ProjectFileTests
{
    private static readonly Guid LayerA = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid LayerB = new("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid Shape1 = new("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid Shape2 = new("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid Shape3 = new("bbbbbbbb-0000-0000-0000-000000000003");

    private static Document Sample() =>
        (Document.Create("ignored") with
        {
            ArtboardWidth = 640.5,
            ArtboardHeight = 480,
            Layers = [new Layer { Id = LayerA, Name = "Background" }, new Layer { Id = LayerB, Name = "Foreä \U0001F600", IsVisible = false, IsLocked = true }],
        })
        .AddShape(LayerA, new RectangleShape
        {
            Id = Shape1,
            X = -12.25,
            Y = 3.5,
            Width = 100,
            Height = 50,
            Appearance = new Appearance { Fill = new Color(0x4A, 0x90, 0xD9, 0x80), Stroke = null, StrokeWidth = 0, Opacity = 0.5 },
        })
        .AddShape(LayerA, new EllipseShape
        {
            Id = Shape2,
            X = 1e-7,
            Y = 1e15,
            Width = 0,
            Height = 0,
            Appearance = new Appearance { Fill = null, Stroke = new Color(1, 2, 3, 255), StrokeWidth = 4.125, Opacity = 1 },
        })
        .AddShape(LayerB, new RectangleShape { Id = Shape3, X = 7, Y = 8, Width = 9, Height = 10 });

    private static void AssertSameContent(Document expected, Document actual)
    {
        Assert.Equal(expected.ArtboardWidth, actual.ArtboardWidth);
        Assert.Equal(expected.ArtboardHeight, actual.ArtboardHeight);
        Assert.Equal(expected.Layers.Length, actual.Layers.Length);
        for (var i = 0; i < expected.Layers.Length; i++)
        {
            var (e, a) = (expected.Layers[i], actual.Layers[i]);
            Assert.Equal((e.Id, e.Name, e.IsVisible, e.IsLocked), (a.Id, a.Name, a.IsVisible, a.IsLocked));
            Assert.Equal(e.Shapes.Length, a.Shapes.Length);
            for (var j = 0; j < e.Shapes.Length; j++)
            {
                var (es, ash) = (e.Shapes[j], a.Shapes[j]);
                Assert.Equal(es.GetType(), ash.GetType());
                Assert.Equal((es.Id, es.X, es.Y, es.Width, es.Height), (ash.Id, ash.X, ash.Y, ash.Width, ash.Height));
                Assert.Equal(es.Appearance, ash.Appearance);
            }
        }
    }

    private static Document ReadOk(byte[] bytes)
    {
        var result = ProjectFile.Read(bytes);
        Assert.True(result.Succeeded, result.Detail);
        return result.Document!;
    }

    [Fact]
    public void WriteThenRead_Sample_ReturnsTheSameContent()
    {
        var original = Sample();

        var back = ReadOk(ProjectFile.Write(original));

        AssertSameContent(original, back);
    }

    [Fact]
    public void WriteThenRead_FreshDocument_ReturnsTheSameContent()
    {
        var original = Document.Create("Layer 1");

        AssertSameContent(original, ReadOk(ProjectFile.Write(original)));
    }

    [Fact]
    public void WriteThenRead_ShapeOrderAndLayerOrder_AreKept()
    {
        var back = ReadOk(ProjectFile.Write(Sample()));

        Assert.Equal(new[] { LayerA, LayerB }, back.Layers.Select(l => l.Id));
        Assert.Equal(new[] { Shape1, Shape2 }, back.Layers[0].Shapes.Select(s => s.Id));
    }

    [Fact]
    public void WriteThenRead_NullFillAndStroke_StayNull()
    {
        var back = ReadOk(ProjectFile.Write(Sample()));

        Assert.Null(back.Layers[0].Shapes[0].Appearance.Stroke);
        Assert.Null(back.Layers[0].Shapes[1].Appearance.Fill);
    }

    [Fact]
    public void WriteThenRead_Alpha_IsKept()
    {
        var back = ReadOk(ProjectFile.Write(Sample()));

        Assert.Equal(new Color(0x4A, 0x90, 0xD9, 0x80), back.Layers[0].Shapes[0].Appearance.Fill);
    }

    [Theory]
    [InlineData(double.Epsilon)]
    [InlineData(1e-300)]
    [InlineData(1e300)]
    [InlineData(-0.1)]
    [InlineData(0.1 + 0.2)]
    [InlineData(123456789.123456789)]
    public void WriteThenRead_ExtremeNumbers_AreExact(double value)
    {
        var empty = Document.Create("L");
        var original = empty.AddShape(empty.Layers[0].Id, new RectangleShape { X = value, Y = -value, Width = Math.Abs(value), Height = 1 });

        var back = ReadOk(ProjectFile.Write(original));

        Assert.Equal(value, back.Layers[0].Shapes[0].X);
        Assert.Equal(-value, back.Layers[0].Shapes[0].Y);
    }

    [Fact]
    public void Write_ContainsFormatNameAndVersion()
    {
        using var json = JsonDocument.Parse(ProjectFile.Write(Sample()));

        Assert.Equal("PressToShape", json.RootElement.GetProperty("format").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("version").GetInt32());
        Assert.Equal(ProjectFile.FormatVersion, json.RootElement.GetProperty("version").GetInt32());
    }

    [Fact]
    public void Write_Output_IsUtf8WithoutByteOrderMark()
    {
        var bytes = ProjectFile.Write(Sample());

        Assert.NotEqual(0xEF, bytes[0]);
        Assert.Equal((byte)'{', bytes[0]);
        _ = new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes);
    }

    [Fact]
    public void Write_ColorsAreRrggbbaaText_AndNoColorIsNull()
    {
        using var json = JsonDocument.Parse(ProjectFile.Write(Sample()));
        var shapes = json.RootElement.GetProperty("layers")[0].GetProperty("shapes");

        Assert.Equal("#4a90d980", shapes[0].GetProperty("fill").GetString());
        Assert.Equal(JsonValueKind.Null, shapes[0].GetProperty("stroke").ValueKind);
        Assert.Equal("#010203ff", shapes[1].GetProperty("stroke").GetString());
    }

    [Fact]
    public void WriteAndRead_UnderAGermanCulture_UseInvariantNumbers()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            var original = Sample();
            var bytes = ProjectFile.Write(original);

            Assert.Contains("640.5", Encoding.UTF8.GetString(bytes));
            AssertSameContent(original, ReadOk(bytes));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Write_LoneSurrogateInName_IsReplacedAndStillReadable()
    {
        var document = Document.Create("ok") with { Layers = [new Layer { Name = "a\uD800b" }] };

        var back = ReadOk(ProjectFile.Write(document));

        Assert.Equal("a�b", back.Layers[0].Name);
    }

    [Fact]
    public void Write_NullDocument_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ProjectFile.Write(null!));
    }

    [Fact]
    public void Read_UnknownExtraEntries_AreIgnored()
    {
        var json = Encoding.UTF8.GetString(ProjectFile.Write(Sample()))
            .Replace("\"format\"", "\"future\": {\"x\": [1,2,3]}, \"format\"", StringComparison.Ordinal)
            .Replace("\"shapes\"", "\"color\": \"red\", \"shapes\"", StringComparison.Ordinal);

        AssertSameContent(Sample(), ReadOk(Encoding.UTF8.GetBytes(json)));
    }

    [Fact]
    public void Read_Success_HasNoErrorAndNoDetail()
    {
        var result = ProjectFile.Read(ProjectFile.Write(Sample()));

        Assert.True(result.Succeeded);
        Assert.Null(result.Error);
        Assert.Null(result.Detail);
    }
}
