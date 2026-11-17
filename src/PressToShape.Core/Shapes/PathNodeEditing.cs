using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.Core.Shapes;

/// <summary>
/// The rules for editing the nodes and handles of a <see cref="PathShape"/>. A node is addressed by its flat index across the sub-paths
/// (see <see cref="PathShape.Locate"/>) and an edit changes only the sub-path that holds it. Every method takes the path and positions in the frame of
/// the path before its rotation (see <see cref="PathShape.ToLocal"/>), changes nothing in place and returns a new path built with
/// <see cref="PathShape.Rebuilt(IReadOnlyList{DocumentSubPath}, FillRule)"/>, so every node that was not moved keeps its place on the screen, also for a rotated path.
/// </summary>
public static partial class PathNodeEditing
{
    // A handle shorter than this counts as removed, and has no direction.
    private const double MinimumHandleLength = 1e-6;

    /// <summary>Moves the nodes by the distance. Their handles move with them, so their types stay valid.</summary>
    /// <exception cref="ArgumentOutOfRangeException">An index is not the index of a node.</exception>
    public static PathShape MoveNodes(PathShape path, IReadOnlyCollection<int> indices, double dx, double dy)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(indices);
        CheckIndices(path, indices);
        if (indices.Count == 0)
        {
            return path;
        }

        var buffer = PathBuffer.From(path);
        foreach (var index in indices.Distinct())
        {
            var node = buffer[index];
            buffer[index] = new DocumentNode(
                node.X + dx,
                node.Y + dy,
                node.InX + dx,
                node.InY + dy,
                node.OutX + dx,
                node.OutY + dy,
                node.Type);
        }

        return buffer.Build()!;
    }

    /// <summary>
    /// Moves one handle to the position. The other handle follows the type of the node: a corner leaves it alone, a smooth node keeps it
    /// on the line through the anchor on the opposite side with its own length, a symmetric node mirrors it. With
    /// <paramref name="breakLink"/> only this handle moves and the node becomes a corner. A handle moved onto its anchor is removed and
    /// the node becomes a corner.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a node.</exception>
    public static PathShape MoveHandle(PathShape path, int index, NodeHandleSide side, double x, double y, bool breakLink)
    {
        ArgumentNullException.ThrowIfNull(path);
        CheckIndices(path, [index]);
        Guard.Finite(x, nameof(x));
        Guard.Finite(y, nameof(y));

        var buffer = PathBuffer.From(path);
        var node = buffer[index];
        var vectorX = x - node.X;
        var vectorY = y - node.Y;
        var length = Math.Sqrt(vectorX * vectorX + vectorY * vectorY);

        double? inX = node.InX;
        double? inY = node.InY;
        double? outX = node.OutX;
        double? outY = node.OutY;
        var type = breakLink ? NodeType.Corner : node.Type;

        if (length <= MinimumHandleLength)
        {
            // Dragged onto the anchor: the handle is gone, and a node that lost a handle is a corner.
            if (side == NodeHandleSide.In)
            {
                inX = null;
                inY = null;
            }
            else
            {
                outX = null;
                outY = null;
            }

            buffer[index] = new DocumentNode(node.X, node.Y, inX, inY, outX, outY, NodeType.Corner);
            return buffer.Build()!;
        }

        // The other handle, if there is one, is placed opposite to the moved one.
        var otherX = side == NodeHandleSide.In ? outX : inX;
        var otherY = side == NodeHandleSide.In ? outY : inY;
        if (type != NodeType.Corner && otherX is { } ox && otherY is { } oy)
        {
            var otherLength = type == NodeType.Symmetric ? length : Math.Sqrt((ox - node.X) * (ox - node.X) + (oy - node.Y) * (oy - node.Y));
            if (otherLength > MinimumHandleLength || type == NodeType.Symmetric)
            {
                otherX = node.X - vectorX / length * otherLength;
                otherY = node.Y - vectorY / length * otherLength;
            }
        }

        if (side == NodeHandleSide.In)
        {
            inX = x;
            inY = y;
            outX = otherX;
            outY = otherY;
        }
        else
        {
            outX = x;
            outY = y;
            inX = otherX;
            inY = otherY;
        }

        buffer[index] = new DocumentNode(node.X, node.Y, inX, inY, outX, outY, type);
        return buffer.Build()!;
    }

    /// <summary>
    /// Removes the nodes. The neighbors connect, each keeping its own handle, so the curve changes a little. A sub-path that would be left
    /// with fewer than two nodes is removed with them. Returns <c>null</c> if no sub-path would remain, in which case the caller deletes the shape.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">An index is not the index of a node.</exception>
    public static PathShape? RemoveNodes(PathShape path, IReadOnlyCollection<int> indices)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(indices);
        CheckIndices(path, indices);

        var removed = indices.ToHashSet();
        if (removed.Count == 0)
        {
            return path;
        }

        var buffer = PathBuffer.From(path);
        var flat = 0;
        for (var subPath = 0; subPath < buffer.SubPathCount; subPath++)
        {
            var nodes = buffer.Nodes(subPath);
            var kept = nodes.Where((_, index) => !removed.Contains(flat + index)).ToList();
            flat += nodes.Count;
            nodes.Clear();
            nodes.AddRange(kept);
        }

        return buffer.Build();
    }

    /// <summary>The position of the anchor of a node on the screen, with the rotation of the path applied.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a node.</exception>
    public static (double X, double Y) NodePosition(PathShape path, int index)
    {
        ArgumentNullException.ThrowIfNull(path);
        CheckIndices(path, [index]);

        var node = PathBuffer.From(path)[index];
        return path.FromLocal(node.X, node.Y);
    }

    private static void CheckIndices(PathShape path, IReadOnlyCollection<int> indices)
    {
        foreach (var index in indices)
        {
            if (index < 0 || index >= path.NodeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(indices), index, "The index is not the index of a node.");
            }
        }
    }
}
