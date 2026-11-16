using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.Core.Shapes;

/// <summary>
/// The rules for editing the nodes and handles of a <see cref="PathShape"/>. Every method takes the path and positions in the frame of
/// the path before its rotation (see <see cref="PathShape.ToLocal"/>), changes nothing in place and returns a new path built with
/// <see cref="PathShape.Rebuilt"/>, so every node that was not moved keeps its place on the screen, also for a rotated path.
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

        var nodes = new List<DocumentNode>(path.ToDocumentNodes());
        foreach (var index in indices.Distinct())
        {
            var node = nodes[index];
            nodes[index] = new DocumentNode(
                node.X + dx,
                node.Y + dy,
                node.InX + dx,
                node.InY + dy,
                node.OutX + dx,
                node.OutY + dy,
                node.Type);
        }

        return path.Rebuilt(nodes, path.IsClosed, path.FillRule);
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

        var nodes = new List<DocumentNode>(path.ToDocumentNodes());
        var node = nodes[index];
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

            nodes[index] = new DocumentNode(node.X, node.Y, inX, inY, outX, outY, NodeType.Corner);
            return path.Rebuilt(nodes, path.IsClosed, path.FillRule);
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

        nodes[index] = new DocumentNode(node.X, node.Y, inX, inY, outX, outY, type);
        return path.Rebuilt(nodes, path.IsClosed, path.FillRule);
    }

    /// <summary>
    /// Removes the nodes. The neighbors connect, each keeping its own handle, so the curve changes a little. Returns <c>null</c> if fewer
    /// than two nodes would remain, in which case the caller deletes the shape.
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

        var nodes = path.ToDocumentNodes().Where((_, index) => !removed.Contains(index)).ToList();
        return nodes.Count < 2 ? null : path.Rebuilt(nodes, path.IsClosed, path.FillRule);
    }

    /// <summary>The position of the anchor of a node on the screen, with the rotation of the path applied.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a node.</exception>
    public static (double X, double Y) NodePosition(PathShape path, int index)
    {
        ArgumentNullException.ThrowIfNull(path);
        CheckIndices(path, [index]);

        var node = path.ToDocumentNodes()[index];
        return path.FromLocal(node.X, node.Y);
    }

    private static void CheckIndices(PathShape path, IReadOnlyCollection<int> indices)
    {
        foreach (var index in indices)
        {
            if (index < 0 || index >= path.Nodes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indices), index, "The index is not the index of a node.");
            }
        }
    }
}
