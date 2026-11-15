using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;

namespace PressToShape.App.Workspace;

/// <summary>
/// Everything the canvas draws for the selection, in screen pixels: an outline for every selected shape, the box and handles
/// around the selection, the line to the rotate handle, and the box that is being dragged on empty space.
/// </summary>
/// <param name="Outlines">One turned outline per selected shape. For every tool.</param>
/// <param name="Box">The frame around the selection (Select tool only).</param>
/// <param name="Handles">The eight resize handles and the rotate handle (Select tool only).</param>
/// <param name="RotateLink">The line from the middle of the top edge to the rotate handle (Select tool only).</param>
/// <param name="SelectionBox">The box being dragged, if any (Select tool only).</param>
internal sealed record SelectionOverlay(
    IReadOnlyList<ScreenPolygon> Outlines,
    ScreenPolygon? Box,
    IReadOnlyList<ScreenHandle> Handles,
    ScreenLine? RotateLink,
    ScreenRect? SelectionBox)
{
    /// <summary>The side of a handle in screen pixels.</summary>
    public const double HandleSize = 8;

    /// <summary>An overlay without anything to draw.</summary>
    public static SelectionOverlay Empty { get; } = new([], null, [], null, null);

    /// <summary>
    /// Builds the overlay. Shapes of hidden or locked layers are not part of it, even if their ids are in the selection.
    /// Handles, the box, the link and the dragged box exist only for the Select tool; outlines exist for every tool.
    /// </summary>
    /// <param name="document">The document that holds the shapes.</param>
    /// <param name="selectedIds">The selected shape ids.</param>
    /// <param name="tool">The active tool.</param>
    /// <param name="zoomPercent">Zoom in percent.</param>
    /// <param name="originX">Screen position of the artboard's left edge.</param>
    /// <param name="originY">Screen position of the artboard's top edge.</param>
    /// <param name="selectionBox">The box being dragged, in document units, or <c>null</c>.</param>
    public static SelectionOverlay Build(
        Document document,
        IEnumerable<Guid> selectedIds,
        Tool tool,
        double zoomPercent,
        double originX,
        double originY,
        SelectionBox? selectionBox = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selectedIds);

        var scale = zoomPercent / 100;
        var isSelect = tool == Tool.Select;
        var box = isSelect && selectionBox is { } dragged
            ? new ScreenRect(
                CanvasMapping.ToScreen(dragged.X, originX, zoomPercent),
                CanvasMapping.ToScreen(dragged.Y, originY, zoomPercent),
                dragged.Width * scale,
                dragged.Height * scale)
            : (ScreenRect?)null;

        var shapes = SelectionQueries.SelectedShapes(document, selectedIds.Where(id => SelectionQueries.IsSelectable(document, id)));
        if (shapes.Count == 0)
        {
            return box is null ? Empty : Empty with { SelectionBox = box };
        }

        var outlines = shapes
            .Select(shape => ScreenPolygon.FromBox(
                CanvasMapping.ToScreen(shape.CenterX, originX, zoomPercent),
                CanvasMapping.ToScreen(shape.CenterY, originY, zoomPercent),
                shape.Width * scale,
                shape.Height * scale,
                shape.Rotation))
            .ToList();
        if (!isSelect)
        {
            return Empty with { Outlines = outlines };
        }

        var frame = SelectionGeometry.FrameOf(shapes)!;
        ScreenPoint ToScreen((double X, double Y) point) =>
            new(CanvasMapping.ToScreen(point.X, originX, zoomPercent), CanvasMapping.ToScreen(point.Y, originY, zoomPercent));

        var frameBox = ScreenPolygon.FromBox(
            CanvasMapping.ToScreen(frame.CenterX, originX, zoomPercent),
            CanvasMapping.ToScreen(frame.CenterY, originY, zoomPercent),
            frame.Width * scale,
            frame.Height * scale,
            frame.Rotation);
        var handles = Enum.GetValues<HandleType>()
            .Select(type => new ScreenHandle(type, ToScreen(SelectionGeometry.HandlePosition(frame, type, zoomPercent)), HandleSize))
            .ToList();
        var link = new ScreenLine(
            ToScreen(SelectionGeometry.HandlePosition(frame, HandleType.Top, zoomPercent)),
            ToScreen(SelectionGeometry.HandlePosition(frame, HandleType.Rotate, zoomPercent)));

        return new SelectionOverlay(outlines, frameBox, handles, link, box);
    }
}
