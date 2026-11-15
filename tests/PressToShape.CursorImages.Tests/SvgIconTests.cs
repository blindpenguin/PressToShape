using System;
using System.IO;
using System.Linq;
using SkiaSharp;
using Xunit;

namespace PressToShape.CursorImages.Tests;

public sealed class SvgIconTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "PressToShape.CursorImages.Tests", Guid.NewGuid().ToString("N"));

    public SvgIconTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
    }

    private string Write(string body, string viewBox = "0 0 24 24")
    {
        var file = Path.Combine(_folder, "icon.svg");
        File.WriteAllText(file, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"{viewBox}\">{body}</svg>");
        return file;
    }

    [Fact]
    public void Load_Path_ReadsItsBounds()
    {
        var file = Write("<path d=\"M2 3H10V8H2Z\" />");

        using var path = SvgIcon.Load(file);

        Assert.Equal(new SKRect(2, 3, 10, 8), path.Bounds);
    }

    [Fact]
    public void Load_Circle_ReadsItsBounds()
    {
        var file = Write("<circle cx=\"5\" cy=\"6\" r=\"2\" />");

        using var path = SvgIcon.Load(file);

        Assert.Equal(new SKRect(3, 4, 7, 8), path.Bounds);
    }

    [Fact]
    public void Load_Line_ReadsItsBounds()
    {
        var file = Write("<line x1=\"4\" y1=\"5\" x2=\"12\" y2=\"20\" />");

        using var path = SvgIcon.Load(file);

        Assert.Equal(new SKRect(4, 5, 12, 20), path.Bounds);
    }

    [Fact]
    public void Load_SeveralElements_AreJoinedIntoOnePath()
    {
        var file = Write("<path d=\"M0 0H2\" /><circle cx=\"20\" cy=\"20\" r=\"2\" />");

        using var path = SvgIcon.Load(file);

        Assert.Equal(new SKRect(0, 0, 22, 22), path.Bounds);
    }

    [Theory]
    [InlineData("<rect x=\"1\" y=\"1\" width=\"4\" height=\"4\" />", "rect")]
    [InlineData("<polyline points=\"1,1 5,5\" />", "polyline")]
    [InlineData("<ellipse cx=\"5\" cy=\"5\" rx=\"2\" ry=\"3\" />", "ellipse")]
    [InlineData("<g><path d=\"M0 0H2\" /></g>", "g")]
    public void Load_UnsupportedElement_FailsWithFileAndElementName(string body, string element)
    {
        var file = Write(body);

        var ex = Assert.Throws<InvalidDataException>(() => SvgIcon.Load(file));

        Assert.Contains(file, ex.Message);
        Assert.Contains($"'{element}'", ex.Message);
    }

    [Fact]
    public void Load_MissingFile_FailsWithFileName()
    {
        var file = Path.Combine(_folder, "missing.svg");

        var ex = Assert.Throws<FileNotFoundException>(() => SvgIcon.Load(file));

        Assert.Contains(file, ex.Message);
    }

    [Fact]
    public void Load_InvalidPathData_FailsWithFileName()
    {
        var file = Write("<path d=\"this is not path data\" />");

        var ex = Assert.Throws<InvalidDataException>(() => SvgIcon.Load(file));

        Assert.Contains(file, ex.Message);
    }

    [Fact]
    public void Load_MissingOrBadNumber_Fails()
    {
        Assert.Throws<InvalidDataException>(() => SvgIcon.Load(Write("<circle cx=\"5\" cy=\"6\" />")));
        Assert.Throws<InvalidDataException>(() => SvgIcon.Load(Write("<circle cx=\"x\" cy=\"6\" r=\"2\" />")));
        Assert.Throws<InvalidDataException>(() => SvgIcon.Load(Write("<circle cx=\"NaN\" cy=\"6\" r=\"2\" />")));
    }

    [Fact]
    public void Load_NotXml_FailsWithFileName()
    {
        var file = Path.Combine(_folder, "broken.svg");
        File.WriteAllText(file, "<svg");

        var ex = Assert.Throws<InvalidDataException>(() => SvgIcon.Load(file));

        Assert.Contains(file, ex.Message);
    }

    [Fact]
    public void Load_OtherViewBox_Fails()
    {
        var file = Write("<path d=\"M0 0H2\" />", viewBox: "0 0 16 16");

        var ex = Assert.Throws<InvalidDataException>(() => SvgIcon.Load(file));

        Assert.Contains("view box", ex.Message);
    }

    [Fact]
    public void Load_TheBundledLucideIcons_AllLoad()
    {
        var files = Directory.GetFiles(RepoPaths.Combine("assets-src/cursors/lucide"), "*.svg");

        Assert.Equal(11, files.Length);
        foreach (var file in files.OrderBy(f => f, StringComparer.Ordinal))
        {
            using var path = SvgIcon.Load(file);

            Assert.False(path.IsEmpty, file);
            Assert.True(path.Bounds.Left >= 0 && path.Bounds.Top >= 0 && path.Bounds.Right <= 24 && path.Bounds.Bottom <= 24, file);
        }
    }
}
