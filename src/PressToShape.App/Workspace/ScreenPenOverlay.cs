using System.Collections.Generic;
using System.Linq;

namespace PressToShape.App.Workspace;

/// <summary>
/// What the canvas shows over the unfinished path of the Pen, in screen pixels of the canvas area: the nodes with their handle
/// points. The positions are absolute here, not relative to a box as in <see cref="ScreenPath"/>.
/// </summary>
/// <param name="Nodes">The nodes drawn so far, in order.</param>
/// <param name="HighlightFirst">Whether the first node is highlighted because a click would close the path.</param>
internal sealed record ScreenPenOverlay(IReadOnlyList<ScreenPathNode> Nodes, bool HighlightFirst)
{
    /// <summary>Converts the overlay from document coordinates to the screen with the same mapping the canvas uses for shapes.</summary>
    public static ScreenPenOverlay From(PenOverlay overlay, double zoomPercent, double originX, double originY)
    {
        double X(double value) => CanvasMapping.ToScreen(value, originX, zoomPercent);
        double Y(double value) => CanvasMapping.ToScreen(value, originY, zoomPercent);

        var nodes = overlay.Nodes
            .Select(node => new ScreenPathNode(
                X(node.X),
                Y(node.Y),
                node.InX is { } inX ? X(inX) : null,
                node.InY is { } inY ? Y(inY) : null,
                node.OutX is { } outX ? X(outX) : null,
                node.OutY is { } outY ? Y(outY) : null))
            .ToList();
        return new ScreenPenOverlay(nodes, overlay.HighlightFirst);
    }
}
