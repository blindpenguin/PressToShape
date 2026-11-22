using System;
using System.IO;
using SkiaSharp;

namespace PressToShape.CursorImages;

/// <summary>Encoding, atomic writing and pixel comparison of the cursor PNGs.</summary>
internal static class ImageFiles
{
    /// <summary>The file name of an image, for example <c>rotate-32.png</c>. Must match <c>CursorImages.FileName</c> in the app.</summary>
    public static string FileName(string name, int size) => $"{name}-{size}.png";

    /// <summary>Encodes a bitmap as PNG.</summary>
    public static byte[] Encode(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>
    /// Writes the data to a temporary file next to the target and moves it over the target, so a failure never leaves a half-written
    /// image behind. The temporary file is removed on failure.
    /// </summary>
    public static void WriteAtomic(string target, byte[] data) => WriteAtomic(target, data, File.Delete);

    // The delete action is a parameter so a test can make the cleanup fail.
    internal static void WriteAtomic(string target, byte[] data, Action<string> delete)
    {
        var temporary = $"{target}.{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllBytes(temporary, data);
            File.Move(temporary, target, overwrite: true);
        }
        catch
        {
            TryDelete(temporary, delete);
            throw;
        }
    }

    // A failed cleanup must not replace the exception that matters: the one of the write.
    private static void TryDelete(string path, Action<string> delete)
    {
        try
        {
            delete(path);
        }
        catch (IOException)
        {
            // The original exception is the one that matters.
        }
        catch (UnauthorizedAccessException)
        {
            // Same as above.
        }
    }

    /// <summary>
    /// Brings a freshly rendered bitmap into the form it has after it was written as PNG and read back. PNG holds unpremultiplied
    /// colors, so the round trip can change semi-transparent pixels by a rounding step. Comparing in this form makes a check right
    /// after a generate exact.
    /// </summary>
    public static SKBitmap AsStored(SKBitmap rendered)
    {
        var bytes = Encode(rendered);
        return Decode(bytes, rendered.Width) ?? throw new InvalidOperationException("A PNG that was just encoded cannot be read back.");
    }

    /// <summary>Reads an image file as a square premultiplied RGBA bitmap of the given size, or <c>null</c> if it is not a PNG of that size.</summary>
    public static SKBitmap? Read(string file, int size)
    {
        return Decode(File.ReadAllBytes(file), size);
    }

    /// <summary>True if both bitmaps have the same size and exactly the same pixels.</summary>
    public static bool SamePixels(SKBitmap first, SKBitmap second) =>
        first.Width == second.Width
        && first.Height == second.Height
        && first.Bytes.AsSpan().SequenceEqual(second.Bytes);

    private static SKBitmap? Decode(byte[] bytes, int size)
    {
        using var codec = SKCodec.Create(new MemoryStream(bytes));
        if (codec is null || codec.Info.Width != size || codec.Info.Height != size)
        {
            return null;
        }

        var bitmap = new SKBitmap(new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul));
        try
        {
            if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
            {
                bitmap.Dispose();
                return null;
            }

            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }
}
