using System;
using SkiaSharp;

namespace PressToShape.CursorImages;

/// <summary>
/// Draws an icon as a cursor image: a white line with a dark outline on a transparent background, so it shows on light and
/// dark canvases. The square image shows a 32 x 32 unit area around the 24 x 24 icon, which leaves a margin for the outline. One unit is one pixel in the 32 px image, so the shape is 24 px wide at 100% screen scale.
/// </summary>
internal static class CursorRenderer
{
    private const float IconUnits = 24;
    private const float MarginUnits = 4;
    private const float ImageUnits = IconUnits + 2 * MarginUnits;
    private const float OutlineWidth = 4.6f;
    private const float LineWidth = 2f;

    // These are the cursor's own colors, not theme colors: the image has to read on any canvas.
    private static readonly SKColor Outline = SKColors.Black;
    private static readonly SKColor Line = SKColors.White;

    /// <summary>Renders the icon into a transparent <paramref name="size"/> x <paramref name="size"/> bitmap.</summary>
    public static SKBitmap Render(SKPath icon, int size) => Render(icon, size, null);

    // The hook is called right after the bitmap exists, so a test can make the drawing fail and check that the bitmap is freed.
    internal static SKBitmap Render(SKPath icon, int size, Action<SKBitmap>? onCreated)
    {
        var bitmap = new SKBitmap(new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul));
        try
        {
            onCreated?.Invoke(bitmap);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            var scale = size / ImageUnits;
            canvas.Scale(scale);
            canvas.Translate(MarginUnits, MarginUnits);

            using var outlinePaint = CreateStroke(Outline, OutlineWidth);
            using var linePaint = CreateStroke(Line, LineWidth);
            canvas.DrawPath(icon, outlinePaint);
            canvas.DrawPath(icon, linePaint);
            return bitmap;
        }
        catch
        {
            // The caller never gets the bitmap, so nobody else can free its native memory.
            bitmap.Dispose();
            throw;
        }
    }

    private static SKPaint CreateStroke(SKColor color, float width) => new()
    {
        Style = SKPaintStyle.Stroke,
        Color = color,
        StrokeWidth = width,
        StrokeCap = SKStrokeCap.Round,
        StrokeJoin = SKStrokeJoin.Round,
        IsAntialias = true,
    };
}
