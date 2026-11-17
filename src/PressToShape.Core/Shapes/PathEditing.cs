using System;
using System.Collections.Generic;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.Shapes;

/// <summary>Operations that change the nodes of a <see cref="PathShape"/>: add a point, delete a point, continue an open sub-path. Nodes and segments are addressed by their flat index across the sub-paths. Every result is a new path, the input is never changed.</summary>
public static class PathEditing
{
    /// <summary>
    /// Adds a node on a segment without changing the curve. A segment is the stretch from node <paramref name="segmentIndex"/> to the
    /// next node of its sub-path (the first node again for the last segment of a closed sub-path).
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="segmentIndex">The segment, from 0 to <see cref="PathShape.SegmentCount"/> minus 1, counted across the sub-paths.</param>
    /// <param name="t">Where on the segment, strictly between 0 (its start) and 1 (its end).</param>
    public static PathShape InsertNode(PathShape path, int segmentIndex, double t)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfNegative(segmentIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(segmentIndex, path.SegmentCount);
        if (!(t > 0 && t < 1))
        {
            throw new ArgumentOutOfRangeException(nameof(t), t, "t must be greater than 0 and less than 1.");
        }

        var (subPath, local) = path.LocateSegment(segmentIndex);
        var buffer = PathBuffer.From(path);
        var nodes = buffer.Nodes(subPath);
        var endIndex = (local + 1) % nodes.Count;
        var start = nodes[local];
        var end = nodes[endIndex];
        var (left, right) = Bezier.Split(path.Segments()[segmentIndex], t);

        DocumentNode inserted;
        if (!start.HasOut && !end.HasIn)
        {
            // A straight segment stays straight, so the new node gets no handles.
            inserted = new DocumentNode(left.P3.X, left.P3.Y);
        }
        else
        {
            nodes[local] = WithOut(start, left.P1);
            nodes[endIndex] = WithIn(end, right.P2);
            // On a curve the new node is smooth: both handles are on the tangent of the curve.
            inserted = new DocumentNode(left.P3.X, left.P3.Y, left.P2.X, left.P2.Y, right.P1.X, right.P1.Y, NodeType.Smooth);
        }

        nodes.Insert(local + 1, inserted);
        return buffer.Build()!;
    }

    /// <summary>
    /// Removes a node. The neighbors connect, each keeping its own handle, so the curve changes a little. A sub-path that is left with
    /// fewer than two nodes is removed. Returns null if no sub-path is left, in which case the caller should delete the shape.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="index">The flat index of the node, counted across the sub-paths.</param>
    public static PathShape? RemoveNode(PathShape path, int index)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, path.NodeCount);

        var (subPath, local) = path.Locate(index);
        var buffer = PathBuffer.From(path);
        buffer.Nodes(subPath).RemoveAt(local);
        return buffer.Build();
    }

    /// <summary>
    /// The nodes of the open sub-path that has the node as one of its ends, in document coordinates, ordered so that the end to continue
    /// from is the last one: as they are for the last node of the sub-path, reversed for its first node. Reversing swaps the in and out
    /// handle of every node. Write the continued nodes back with <see cref="ReplaceSubPath"/>.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="endIndex">The flat index of the first or the last node of an open sub-path.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a node.</exception>
    /// <exception cref="ArgumentException">The node is not an end of an open sub-path.</exception>
    public static IReadOnlyList<DocumentNode> Continue(PathShape path, int endIndex)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfNegative(endIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(endIndex, path.NodeCount);

        var (subPath, local) = path.Locate(endIndex);
        var source = path.ToDocumentSubPaths()[subPath];
        if (source.IsClosed || (local != 0 && local != source.Nodes.Count - 1))
        {
            throw new ArgumentException("The node is not an end of an open sub-path.", nameof(endIndex));
        }

        var nodes = new List<DocumentNode>(source.Nodes);
        if (local != 0)
        {
            return nodes;
        }

        nodes.Reverse();
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            nodes[i] = new DocumentNode(node.X, node.Y, node.OutX, node.OutY, node.InX, node.InY, node.Type);
        }

        return nodes;
    }

    /// <summary>
    /// The path with one sub-path replaced by the nodes, for example the result of continuing it. The other sub-paths keep their place on
    /// the screen (see <see cref="PathShape.Rebuilt(IReadOnlyList{DocumentSubPath}, FillRule)"/>).
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="subPathIndex">The sub-path to replace.</param>
    /// <param name="nodes">The new nodes in document coordinates, at least two.</param>
    /// <param name="isClosed">Whether the new sub-path is closed.</param>
    public static PathShape ReplaceSubPath(PathShape path, int subPathIndex, IReadOnlyList<DocumentNode> nodes, bool isClosed)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentOutOfRangeException.ThrowIfNegative(subPathIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(subPathIndex, path.SubPaths.Length);

        var subPaths = new List<DocumentSubPath>(path.ToDocumentSubPaths())
        {
            [subPathIndex] = new DocumentSubPath(nodes, isClosed),
        };
        return path.Rebuilt(subPaths, path.FillRule);
    }

    // A handle that lies exactly on its anchor is the same as no handle. Splitting a curve shortens the handle of the node next to the new
    // node along its own direction, so a symmetric node stays on its line but is no longer equally long: it becomes smooth.
    private static DocumentNode WithOut(DocumentNode node, (double X, double Y) handle) =>
        Fitted(handle.X == node.X && handle.Y == node.Y
            ? new DocumentNode(node.X, node.Y, node.InX, node.InY, type: node.Type)
            : new DocumentNode(node.X, node.Y, node.InX, node.InY, handle.X, handle.Y, node.Type));

    private static DocumentNode WithIn(DocumentNode node, (double X, double Y) handle) =>
        Fitted(handle.X == node.X && handle.Y == node.Y
            ? new DocumentNode(node.X, node.Y, outX: node.OutX, outY: node.OutY, type: node.Type)
            : new DocumentNode(node.X, node.Y, handle.X, handle.Y, node.OutX, node.OutY, node.Type));

    private static DocumentNode Fitted(DocumentNode node) =>
        node.Type == NodeType.Symmetric && !NodeTypes.Satisfies(node)
            ? new DocumentNode(node.X, node.Y, node.InX, node.InY, node.OutX, node.OutY, NodeType.Smooth)
            : node;
}
