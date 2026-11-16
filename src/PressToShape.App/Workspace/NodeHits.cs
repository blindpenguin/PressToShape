using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>Finds the node, handle or segment under a position, for Direct Select and the Node tool. Pure: it works on the positions on the screen.</summary>
internal static class NodeHits
{
    /// <summary>
    /// The handle points that are shown for the selected nodes of a path: both handles of each selected node, and the handle of each neighbor
    /// that belongs to the segment between the neighbor and the selected node (the out handle of the previous node, the in handle of the next one).
    /// </summary>
    /// <param name="path">The path, in screen positions or not, only the handles that exist matter.</param>
    /// <param name="selected">The indices of the selected nodes.</param>
    public static IReadOnlyCollection<(int Index, NodeHandleSide Side)> VisibleHandles(PathShape path, IReadOnlySet<int> selected)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(selected);

        var count = path.Nodes.Length;
        var handles = new HashSet<(int Index, NodeHandleSide Side)>();
        foreach (var index in selected.Where(i => i >= 0 && i < count))
        {
            AddIfPresent(handles, path, index, NodeHandleSide.In);
            AddIfPresent(handles, path, index, NodeHandleSide.Out);
            if (index > 0 || path.IsClosed)
            {
                AddIfPresent(handles, path, (index - 1 + count) % count, NodeHandleSide.Out);
            }

            if (index < count - 1 || path.IsClosed)
            {
                AddIfPresent(handles, path, (index + 1) % count, NodeHandleSide.In);
            }
        }

        return handles;
    }

    /// <summary>
    /// What the position hits. Handles that are shown come first, then nodes, then (if <paramref name="includeSegments"/>) segments, so a
    /// handle on top of a node can still be grabbed. Within one kind the nearest one wins, on a tie the shape that is drawn later (above).
    /// </summary>
    /// <param name="paths">The paths in drawing order, bottom first, with the rotation applied (see <see cref="EditablePath.AsOnScreen"/>).</param>
    /// <param name="selectedNodes">The selected nodes of all shapes.</param>
    /// <param name="x">The position, in document units.</param>
    /// <param name="y">The position, in document units.</param>
    /// <param name="tolerance">How far a node, handle or segment may be, in document units.</param>
    /// <param name="includeSegments">Whether a segment counts as a hit.</param>
    public static NodeHit Find(IReadOnlyList<PathShape> paths, IReadOnlySet<NodeRef> selectedNodes, double x, double y, double tolerance, bool includeSegments)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(selectedNodes);

        var best = default(NodeHit);
        var bestDistance = tolerance;
        for (var p = paths.Count - 1; p >= 0; p--)
        {
            var path = paths[p];
            var selected = selectedNodes.Where(n => n.ShapeId == path.Id).Select(n => n.Index).ToHashSet();
            var nodes = path.ToDocumentNodes();
            foreach (var (index, side) in VisibleHandles(path, selected))
            {
                var handle = side == NodeHandleSide.In ? (X: nodes[index].InX!.Value, Y: nodes[index].InY!.Value) : (X: nodes[index].OutX!.Value, Y: nodes[index].OutY!.Value);
                var distance = Distance(handle.X, handle.Y, x, y);
                if (distance <= bestDistance && (best.Type == NodeHitType.None || distance < bestDistance))
                {
                    best = new NodeHit(NodeHitType.Handle, path.Id, index, side);
                    bestDistance = distance;
                }
            }
        }

        if (best.Type != NodeHitType.None)
        {
            return best;
        }

        bestDistance = tolerance;
        for (var p = paths.Count - 1; p >= 0; p--)
        {
            var nodes = paths[p].ToDocumentNodes();
            for (var i = 0; i < nodes.Count; i++)
            {
                var distance = Distance(nodes[i].X, nodes[i].Y, x, y);
                if (distance <= bestDistance && (best.Type == NodeHitType.None || distance < bestDistance))
                {
                    best = new NodeHit(NodeHitType.Node, paths[p].Id, i);
                    bestDistance = distance;
                }
            }
        }

        if (best.Type != NodeHitType.None || !includeSegments)
        {
            return best;
        }

        bestDistance = tolerance;
        for (var p = paths.Count - 1; p >= 0; p--)
        {
            var segments = paths[p].Segments();
            for (var i = 0; i < segments.Count; i++)
            {
                var t = Bezier.NearestParameter(segments[i], x, y);
                if (t <= 0 || t >= 1)
                {
                    continue;
                }

                var point = Bezier.Point(segments[i], t);
                var distance = Distance(point.X, point.Y, x, y);
                if (distance <= bestDistance && (best.Type == NodeHitType.None || distance < bestDistance))
                {
                    best = new NodeHit(NodeHitType.Segment, paths[p].Id, i, T: t);
                    bestDistance = distance;
                }
            }
        }

        return best;
    }

    private static void AddIfPresent(HashSet<(int Index, NodeHandleSide Side)> handles, PathShape path, int index, NodeHandleSide side)
    {
        var node = path.Nodes[index];
        if (side == NodeHandleSide.In ? node.HasIn : node.HasOut)
        {
            handles.Add((index, side));
        }
    }

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
