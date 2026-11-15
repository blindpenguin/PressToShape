using System;
using System.Collections.Generic;

namespace PressToShape.App.Workspace;

/// <summary>
/// The facts about the cursor images of the canvas: which size fits a screen scale, the file name of each image and its
/// active point. Pure logic. The images are made by <c>tools/PressToShape.CursorImages</c> and committed under
/// <c>Assets/Cursors</c>.
/// </summary>
internal static class CursorImages
{
    /// <summary>The image sizes in pixels, ascending. The images are square.</summary>
    public static IReadOnlyList<int> Sizes { get; } = [32, 64];

    private const int SmallSize = 32;
    private const int LargeSize = 64;

    // The nearest of 1x and 2x, measured on the scale itself: the middle between them is 1.5.
    private const double LargeFromScale = 1.5;

    /// <summary>
    /// The image size for a screen scale (1.0 is 100%). A scale that is not a finite positive number counts as 100%, because a
    /// wrong size is better than no cursor.
    /// </summary>
    public static int SizeFor(double scale)
    {
        if (!double.IsFinite(scale) || scale <= 0)
        {
            return SmallSize;
        }

        return scale >= LargeFromScale ? LargeSize : SmallSize;
    }

    /// <summary>The file name of the image for a cursor type and size, or <c>null</c> for the arrow, which stays the system cursor.</summary>
    public static string? FileName(CanvasCursorType type, int size)
    {
        EnsureKnownSize(size);
        var name = type switch
        {
            CanvasCursorType.Arrow => null,
            CanvasCursorType.Crosshair => "crosshair",
            CanvasCursorType.OpenHand => "open-hand",
            CanvasCursorType.ClosedHand => "closed-hand",
            CanvasCursorType.Magnifier => "magnifier",
            CanvasCursorType.MagnifierMinus => "magnifier-minus",
            CanvasCursorType.ResizeNorthSouth => "resize-north-south",
            CanvasCursorType.ResizeWestEast => "resize-west-east",
            CanvasCursorType.ResizeNorthWestSouthEast => "resize-north-west-south-east",
            CanvasCursorType.ResizeNorthEastSouthWest => "resize-north-east-south-west",
            CanvasCursorType.Rotate => "rotate",
            CanvasCursorType.Eyedropper => "eyedropper",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown cursor type."),
        };

        return name is null ? null : $"{name}-{size}.png";
    }

    /// <summary>
    /// The active point of the image in its own pixels. It is the center of the image, except for the magnifier, where it is the
    /// center of the lens. The point at 64 px is exactly double the point at 32 px.
    /// </summary>
    public static (int X, int Y) Hotspot(CanvasCursorType type, int size)
    {
        EnsureKnownSize(size);
        if (type == CanvasCursorType.Arrow)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "The arrow has no image.");
        }

        // The pipette icon ends in a tip at (2, 22) of 24 units, so with the 4 unit margin the tip is at (6, 26) of 32.
        if (type == CanvasCursorType.Eyedropper)
        {
            var scale = size / SmallSize;
            return (6 * scale, 26 * scale);
        }

        // At 32 px. The lens of the zoom icon is centered at 11 of 24 units, and the image shows 32 units with a 4 unit margin, so the lens center is at 15.
        var point = type is CanvasCursorType.Magnifier or CanvasCursorType.MagnifierMinus ? 15 : 16;
        var factor = size / SmallSize;
        return (point * factor, point * factor);
    }

    private static void EnsureKnownSize(int size)
    {
        if (size != SmallSize && size != LargeSize)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "There are no cursor images of this size.");
        }
    }
}
