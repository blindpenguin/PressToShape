using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class CursorImageFilesTests
{
    private static readonly string Folder = RepoPaths.Combine("src/PressToShape.App/Assets/Cursors");

    // A PNG starts with an 8 byte signature, then the IHDR chunk: length (4), "IHDR" (4), width (4), height (4).
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static (int Width, int Height) ReadPngSize(string file)
    {
        var header = new byte[24];
        using (var stream = File.OpenRead(file))
        {
            stream.ReadExactly(header);
        }

        Assert.True(header.AsSpan(0, 8).SequenceEqual(PngSignature), $"{file} is not a PNG.");
        Assert.Equal("IHDR", System.Text.Encoding.ASCII.GetString(header, 12, 4));
        return (BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16)), BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20)));
    }

    [Fact]
    public void EveryCursorTypeButArrow_HasAnImageOfTheRightSize_AtEverySize()
    {
        foreach (var type in Enum.GetValues<CanvasCursorType>().Where(t => t != CanvasCursorType.Arrow))
        {
            foreach (var size in CursorImages.Sizes)
            {
                var file = Path.Combine(Folder, CursorImages.FileName(type, size)!);

                Assert.True(File.Exists(file), $"The image for {type} at {size} px is missing: {file}. Run the CursorImages tool.");
                Assert.Equal((size, size), ReadPngSize(file));
            }
        }
    }

    [Fact]
    public void CursorsFolder_HoldsNoOtherFiles()
    {
        var expected = Enum.GetValues<CanvasCursorType>()
            .Where(t => t != CanvasCursorType.Arrow)
            .SelectMany(t => CursorImages.Sizes.Select(s => CursorImages.FileName(t, s)!))
            .Order(StringComparer.Ordinal)
            .ToList();

        var actual = Directory.GetFiles(Folder).Select(f => Path.GetFileName(f)).Order(StringComparer.Ordinal).ToList();

        Assert.Equal(expected, actual);
    }
}
