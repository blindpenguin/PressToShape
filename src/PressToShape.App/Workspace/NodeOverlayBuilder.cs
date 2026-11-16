using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>Builds what the canvas shows for Direct Select and the Node tool. Pure: no Avalonia types.</summary>
internal static class NodeOverlayBuilder
{
    /// <summary>
    /// The overlay for the node tools: every node of the selected shapes that can be edited (paths, and rectangles and ellipses with their
    /// four nodes), the selected ones marked. A handle is shown for a selected node, and for its neighbors the handle that belongs to the
    /// segment toward the selected node (see <see cref="NodeHits.VisibleHandles"/>). Returns <c>null</c> for the other tools and when there
    /// is nothing to show. Hidden and locked shapes show nothing. A rotated shape is shown where it appears on the screen.
    /// </summary>
    public static ScreenNodeOverlay? Build(
        Document document,
        ImmutableArray<Guid> selectedShapeIds,
        IReadOnlySet<NodeRef> selectedNodes,
        Tool tool,
        double zoomPercent,
        double originX,
        double originY)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selectedNodes);

        if (tool is not (Tool.DirectSelect or Tool.Node) || selectedShapeIds.IsDefaultOrEmpty)
        {
            return null;
        }

        double X(double value) => CanvasMapping.ToScreen(value, originX, zoomPercent);
        double Y(double value) => CanvasMapping.ToScreen(value, originY, zoomPercent);

        var nodes = new List<ScreenOverlayNode>();
        foreach (var path in EditablePath.OnScreenOfSelection(document, selectedShapeIds))
        {
            var selected = selectedNodes.Where(n => n.ShapeId == path.Id).Select(n => n.Index).ToHashSet();
            var visible = NodeHits.VisibleHandles(path, selected);
            var points = path.ToDocumentNodes();
            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var showIn = point.HasIn && visible.Contains((i, NodeHandleSide.In));
                var showOut = point.HasOut && visible.Contains((i, NodeHandleSide.Out));
                nodes.Add(new ScreenOverlayNode(
                    X(point.X),
                    Y(point.Y),
                    selected.Contains(i),
                    showIn ? X(point.InX!.Value) : null,
                    showIn ? Y(point.InY!.Value) : null,
                    showOut ? X(point.OutX!.Value) : null,
                    showOut ? Y(point.OutY!.Value) : null));
            }
        }

        return nodes.Count == 0 ? null : new ScreenNodeOverlay(nodes);
    }
}
