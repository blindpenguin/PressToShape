using System;
using System.IO;
using System.Linq;
using SkiaSharp;
using Xunit;

namespace PressToShape.CursorImages.Tests;

public sealed class CommandTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "PressToShape.CursorImages.Tests", Guid.NewGuid().ToString("N"));
    private readonly string _source = RepoPaths.Combine("assets-src/cursors/lucide");
    private readonly StringWriter _output = new();
    private readonly StringWriter _error = new();

    public CommandTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
    }

    private string Images => Path.Combine(_folder, "images");

    private int Run(string command, string? source = null) =>
        Program.Run([command, "--source", source ?? _source, "--output", Images], _output, _error);

    [Fact]
    public void Generate_WritesTwentyTwoImagesOfTheRightSize()
    {
        var code = Run("generate");

        Assert.Equal(0, code);
        var files = Directory.GetFiles(Images);
        Assert.Equal(22, files.Length);
        foreach (var entry in CursorSet.Entries)
        {
            foreach (var size in CursorSet.Sizes)
            {
                var file = Path.Combine(Images, $"{entry.Name}-{size}.png");
                Assert.True(File.Exists(file), file);
                using var bitmap = SKBitmap.Decode(file);
                Assert.Equal((size, size), (bitmap.Width, bitmap.Height));
            }
        }
    }

    [Fact]
    public void Generate_Twice_GivesIdenticalFiles()
    {
        Run("generate");
        var first = Directory.GetFiles(Images).Order(StringComparer.Ordinal).ToDictionary(f => Path.GetFileName(f), File.ReadAllBytes);

        Run("generate");

        foreach (var (name, bytes) in first)
        {
            Assert.True(bytes.AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(Images, name))), name);
        }
    }

    [Fact]
    public void Check_AfterGenerate_Passes()
    {
        Run("generate");

        var code = Run("check");

        Assert.Equal(0, code);
        Assert.Equal(string.Empty, _error.ToString());
    }

    [Fact]
    public void Check_ImageMissing_FailsAndNamesTheFile()
    {
        Run("generate");
        File.Delete(Path.Combine(Images, "rotate-64.png"));

        var code = Run("check");

        Assert.Equal(1, code);
        Assert.Contains("missing: rotate-64.png", _error.ToString());
    }

    [Fact]
    public void Check_OnePixelChanged_FailsAndNamesTheFile()
    {
        Run("generate");
        var file = Path.Combine(Images, "magnifier-32.png");
        using (var bitmap = SKBitmap.Decode(file))
        {
            var pixel = bitmap.GetPixel(16, 16);
            bitmap.SetPixel(16, 16, pixel.Alpha == 255 ? new SKColor(1, 2, 3, 255) : SKColors.Red);
            File.WriteAllBytes(file, ImageFiles.Encode(bitmap));
        }

        var code = Run("check");

        Assert.Equal(1, code);
        Assert.Contains("different: magnifier-32.png", _error.ToString());
        Assert.DoesNotContain("different: magnifier-64.png", _error.ToString());
    }

    [Fact]
    public void Check_ImageOfTheWrongSize_Fails()
    {
        Run("generate");
        File.Copy(Path.Combine(Images, "rotate-64.png"), Path.Combine(Images, "rotate-32.png"), overwrite: true);

        var code = Run("check");

        Assert.Equal(1, code);
        Assert.Contains("rotate-32.png", _error.ToString());
    }

    [Fact]
    public void Generate_TargetCannotBeWritten_FailsAndLeavesNoTemporaryFile()
    {
        // A folder where an image should go makes the final move fail.
        Directory.CreateDirectory(Path.Combine(Images, "crosshair-32.png"));

        var code = Run("generate");

        Assert.Equal(1, code);
        Assert.Empty(Directory.GetFiles(Images, "*.tmp"));
        Assert.True(Directory.Exists(Path.Combine(Images, "crosshair-32.png")));
    }

    [Fact]
    public void WriteAtomic_ReplacesAnExistingFile()
    {
        var file = Path.Combine(_folder, "image.png");
        File.WriteAllBytes(file, [1, 2, 3]);

        ImageFiles.WriteAtomic(file, [4, 5]);

        Assert.Equal([4, 5], File.ReadAllBytes(file));
        Assert.Single(Directory.GetFiles(_folder, "image*"));
    }

    [Fact]
    public void Generate_UnsupportedSvg_FailsWithFileNameAndWritesNothing()
    {
        var source = Path.Combine(_folder, "source");
        Directory.CreateDirectory(source);
        foreach (var file in Directory.GetFiles(_source, "*.svg"))
        {
            File.Copy(file, Path.Combine(source, Path.GetFileName(file)));
        }

        var broken = Path.Combine(source, "rotate-cw.svg");
        File.WriteAllText(broken, "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><rect x=\"1\" y=\"1\" width=\"2\" height=\"2\" /></svg>");

        var code = Run("generate", source);

        Assert.Equal(1, code);
        Assert.Contains(broken, _error.ToString());
        Assert.Contains("'rect'", _error.ToString());
    }

    [Theory]
    [InlineData]
    [InlineData("other")]
    [InlineData("generate", "--unknown", "x")]
    [InlineData("generate", "--source")]
    public void Run_BadArguments_PrintsUsageAndFails(params string[] args)
    {
        var code = Program.Run(args, _output, _error);

        Assert.Equal(1, code);
        Assert.Contains("Usage:", _error.ToString());
    }
}
