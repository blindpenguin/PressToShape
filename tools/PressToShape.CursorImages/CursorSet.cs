using System.Collections.Generic;

namespace PressToShape.CursorImages;

/// <summary>One cursor image: its name (the part of the file name before the size) and the Lucide icon it is drawn from.</summary>
internal sealed record CursorEntry(string Name, string Icon);

/// <summary>
/// The cursors of the canvas and their icons. The names must match <c>CursorImages.FileName</c> in the app, which the app tests
/// check against the generated files.
/// </summary>
internal static class CursorSet
{
    /// <summary>The image sizes in pixels. Must match <c>CursorImages.Sizes</c> in the app.</summary>
    public static IReadOnlyList<int> Sizes { get; } = [32, 64];

    public static IReadOnlyList<CursorEntry> Entries { get; } =
    [
        new("crosshair", "crosshair"),
        new("open-hand", "hand"),
        new("closed-hand", "grab"),
        new("magnifier", "zoom-in"),
        new("magnifier-minus", "zoom-out"),
        new("resize-north-south", "move-vertical"),
        new("resize-west-east", "move-horizontal"),
        new("resize-north-west-south-east", "move-diagonal-2"),
        new("resize-north-east-south-west", "move-diagonal"),
        new("rotate", "rotate-cw"),
    ];
}
