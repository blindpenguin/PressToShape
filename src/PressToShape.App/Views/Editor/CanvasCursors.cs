using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Material.Icons;
using PressToShape.App.Workspace;

namespace PressToShape.App.Views.Editor;

/// <summary>
/// The pointer shapes of the canvas. Avalonia has no standard open hand, closed hand or magnifier, so these are drawn
/// from the Material icons the tool column already uses, with a light fill and a dark outline that show on both themes.
/// If a cursor cannot be created, a standard cursor is used instead.
/// </summary>
internal static class CanvasCursors
{
    private const int Size = 32;
    private const double IconSize = 24;

    private static readonly Dictionary<CanvasCursorType, Cursor> Cache = [];

    // The bitmaps behind the cursors stay alive as long as the cursors do.
    private static readonly List<IDisposable> Bitmaps = [];

    /// <summary>The cursor for a type. Created on first use and then reused.</summary>
    public static Cursor For(CanvasCursorType type)
    {
        if (!Cache.TryGetValue(type, out var cursor))
        {
            cursor = Create(type);
            Cache[type] = cursor;
        }

        return cursor;
    }

    private static Cursor Create(CanvasCursorType type) => type switch
    {
        CanvasCursorType.Crosshair => new Cursor(StandardCursorType.Cross),
        CanvasCursorType.OpenHand => FromIcon(MaterialIconKind.HandBackLeftOutline, new PixelPoint(16, 16), StandardCursorType.Hand),
        CanvasCursorType.ClosedHand => FromIcon(MaterialIconKind.HandBackLeft, new PixelPoint(16, 16), StandardCursorType.SizeAll),
        CanvasCursorType.Magnifier => FromIcon(MaterialIconKind.MagnifyPlusOutline, new PixelPoint(13, 13), StandardCursorType.Cross),
        _ => new Cursor(StandardCursorType.Arrow),
    };

    private static Cursor FromIcon(MaterialIconKind icon, PixelPoint hotSpot, StandardCursorType fallback)
    {
        try
        {
            var geometry = Geometry.Parse(MaterialIconDataProvider.GetData(icon));
            var bitmap = new RenderTargetBitmap(new PixelSize(Size, Size));
            using (var context = bitmap.CreateDrawingContext())
            using (context.PushTransform(Matrix.CreateScale(Size / IconSize, Size / IconSize)))
            {
                // A thick dark line first, then the light fill on top: a light shape with a dark rim.
                context.DrawGeometry(null, new Pen(Brushes.Black, 2.6, lineJoin: PenLineJoin.Round), geometry);
                context.DrawGeometry(Brushes.White, null, geometry);
            }

            Bitmaps.Add(bitmap);
            return new Cursor(bitmap, hotSpot);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or NotSupportedException or FormatException)
        {
            // A cursor is only decoration. If the platform cannot draw or accept it, the standard shape does the job.
            return new Cursor(fallback);
        }
    }
}
