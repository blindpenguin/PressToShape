using System;
using System.Collections.Generic;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.Shapes;

/// <summary>Operations that change the nodes of a <see cref="PathShape"/>: add a point, delete a point, continue an open path. Every result is a new path, the input is never changed.</summary>
public static class PathEditing
{
    /// <summary>
    /// Adds a node on a segment without changing the curve. A segment is the stretch from node <paramref name="segmentIndex"/> to the
    /// next node (the first node again for the last segment of a closed path).
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="segmentIndex">The segment, from 0 to the segment count minus 1 (nodes minus 1 when open, nodes when closed).</param>
    /// <param name="t">Where on the segment, strictly between 0 (its start) and 1 (its end).</param>
    public static PathShape InsertNode(PathShape path, int segmentIndex, double t)
    {
        ArgumentNullException.ThrowIfNull(path);

        var nodes = new List<DocumentNode>(path.ToDocumentNodes());
        var segmentCount = path.IsClosed ? nodes.Count : nodes.Count - 1;
        ArgumentOutOfRangeException.ThrowIfNegative(segmentIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(segmentIndex, segmentCount);
        if (!(t > 0 && t < 1))
        {
            throw new ArgumentOutOfRangeException(nameof(t), t, "t must be greater than 0 and less than 1.");
        }

        var endIndex = (segmentIndex + 1) % nodes.Count;
        var start = nodes[segmentIndex];
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
            nodes[segmentIndex] = WithOut(start, left.P1);
            nodes[endIndex] = WithIn(end, right.P2);
            inserted = new DocumentNode(left.P3.X, left.P3.Y, left.P2.X, left.P2.Y, right.P1.X, right.P1.Y);
        }

        nodes.Insert(segmentIndex + 1, inserted);
        return path.Rebuilt(nodes, path.IsClosed, path.FillRule);
    }

    /// <summary>
    /// Removes a node. The neighbors connect, each keeping its own handle, so the curve changes a little. Returns null if fewer than
    /// two nodes would be left, in which case the caller should delete the shape.
    /// </summary>
    public static PathShape? RemoveNode(PathShape path, int index)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, path.Nodes.Length);

        if (path.Nodes.Length <= 2)
        {
            return null;
        }

        var nodes = new List<DocumentNode>(path.ToDocumentNodes());
        nodes.RemoveAt(index);
        return path.Rebuilt(nodes, path.IsClosed, path.FillRule);
    }

    /// <summary>
    /// The nodes of an open path in document coordinates, ordered so that the end to continue from is the last one: as they are for
    /// the end of the path, reversed for its start. Reversing swaps the in and out handle of every node.
    /// </summary>
    /// <exception cref="ArgumentException">The path is closed and has no ends.</exception>
    public static IReadOnlyList<DocumentNode> Continue(PathShape path, bool atStart)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.IsClosed)
        {
            throw new ArgumentException("A closed path has no end to continue from.", nameof(path));
        }

        var nodes = new List<DocumentNode>(path.ToDocumentNodes());
        if (!atStart)
        {
            return nodes;
        }

        nodes.Reverse();
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            nodes[i] = new DocumentNode(node.X, node.Y, node.OutX, node.OutY, node.InX, node.InY);
        }

        return nodes;
    }

    // A handle that lies exactly on its anchor is the same as no handle.
    private static DocumentNode WithOut(DocumentNode node, (double X, double Y) handle) =>
        handle.X == node.X && handle.Y == node.Y
            ? new DocumentNode(node.X, node.Y, node.InX, node.InY)
            : new DocumentNode(node.X, node.Y, node.InX, node.InY, handle.X, handle.Y);

    private static DocumentNode WithIn(DocumentNode node, (double X, double Y) handle) =>
        handle.X == node.X && handle.Y == node.Y
            ? new DocumentNode(node.X, node.Y, outX: node.OutX, outY: node.OutY)
            : new DocumentNode(node.X, node.Y, handle.X, handle.Y, node.OutX, node.OutY);
}
