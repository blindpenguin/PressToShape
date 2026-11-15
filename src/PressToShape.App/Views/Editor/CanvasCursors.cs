using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PressToShape.App.Workspace;

namespace PressToShape.App.Views.Editor;

/// <summary>
/// The pointer shapes of the canvas. Avalonia has no standard rotate, open hand, closed hand or magnifier, so the cursors come
/// from pre-drawn images (<c>Assets/Cursors</c>, made from Lucide icons by <c>tools/PressToShape.CursorImages</c>) in the size that
/// fits the screen scale. The plain arrow stays the system cursor. If an image cannot be loaded or the platform rejects it, a
/// standard cursor is used instead.
/// </summary>
internal static class CanvasCursors
{
    private const string AssetFolder = "avares://PressToShape.App/Assets/Cursors/";

    private static readonly Dictionary<(CanvasCursorType Type, int Size), Cursor> Cache = [];

    // The bitmaps behind the cursors stay alive as long as the cursors do.
    private static readonly List<IDisposable> Bitmaps = [];

    /// <summary>The cursor for a type on a screen with this scale (1.0 is 100%). Created on first use and then reused.</summary>
    public static Cursor For(CanvasCursorType type, double scale)
    {
        if (type == CanvasCursorType.Arrow)
        {
            return new Cursor(StandardCursorType.Arrow);
        }

        var size = CursorImages.SizeFor(scale);
        if (!Cache.TryGetValue((type, size), out var cursor))
        {
            cursor = FromImage(type, size);
            Cache[(type, size)] = cursor;
        }

        return cursor;
    }

    private static Cursor FromImage(CanvasCursorType type, int size)
    {
        try
        {
            var fileName = CursorImages.FileName(type, size)
                ?? throw new InvalidOperationException($"The cursor type {type} has no image.");
            var (x, y) = CursorImages.Hotspot(type, size);

            using var stream = AssetLoader.Open(new Uri(AssetFolder + fileName));
            var bitmap = new Bitmap(stream);
            Bitmaps.Add(bitmap);
            return new Cursor(bitmap, new PixelPoint(x, y));
        }
        catch (Exception ex) when (ex is System.IO.IOException or ArgumentException or InvalidOperationException or NotSupportedException or FormatException)
        {
            // A cursor is only decoration. If the image is missing or the platform cannot accept it, the standard shape does the job.
            return new Cursor(Fallback(type));
        }
    }

    private static StandardCursorType Fallback(CanvasCursorType type) => type switch
    {
        CanvasCursorType.Crosshair => StandardCursorType.Cross,
        CanvasCursorType.OpenHand => StandardCursorType.Hand,
        CanvasCursorType.ClosedHand => StandardCursorType.SizeAll,
        CanvasCursorType.Magnifier or CanvasCursorType.MagnifierMinus => StandardCursorType.Cross,
        CanvasCursorType.ResizeNorthSouth => StandardCursorType.SizeNorthSouth,
        CanvasCursorType.ResizeWestEast => StandardCursorType.SizeWestEast,
        CanvasCursorType.ResizeNorthWestSouthEast => StandardCursorType.BottomRightCorner,
        CanvasCursorType.ResizeNorthEastSouthWest => StandardCursorType.BottomLeftCorner,
        CanvasCursorType.Rotate => StandardCursorType.Hand,
        _ => StandardCursorType.Arrow,
    };
}
