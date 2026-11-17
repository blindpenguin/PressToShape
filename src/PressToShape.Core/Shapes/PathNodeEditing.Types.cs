using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.Core.Shapes;

public static partial class PathNodeEditing
{
    /// <summary>
    /// Gives the nodes the type. A corner only changes the type, the handles stay. A smooth or symmetric node first needs its handles on
    /// one line on opposite sides: if it has an out handle, the direction of that handle wins and the in handle turns to the opposite side
    /// (with its own length for smooth, with the length of the out handle for symmetric), and if it only has an in handle the out handle
    /// follows it in the same way. A node without handles gets handles on the line from its previous to its next neighbor, a third of
    /// the distance to the neighbor on each side (for symmetric both as long as the shorter one). An end node of an open path only gets
    /// the handle toward its one neighbor. No handle is created toward a neighbor on the same spot.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">An index is not the index of a node, or the type is not defined.</exception>
    public static PathShape SetNodeType(PathShape path, IReadOnlyCollection<int> indices, NodeType type)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(indices);
        CheckIndices(path, indices);
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "The node type is not defined.");
        }

        if (indices.Count == 0)
        {
            return path;
        }

        var original = path.ToDocumentSubPaths();
        var buffer = PathBuffer.From(path);
        foreach (var index in indices.Distinct())
        {
            var (subPath, local) = path.Locate(index);
            var before = original[subPath];
            buffer.Nodes(subPath)[local] = type == NodeType.Corner
                ? WithType(before.Nodes[local], NodeType.Corner)
                : Aligned(before.Nodes, local, before.IsClosed, type);
        }

        return buffer.Build()!;
    }

    private static DocumentNode WithType(DocumentNode node, NodeType type) =>
        new(node.X, node.Y, node.InX, node.InY, node.OutX, node.OutY, type);

    // The node with handles that fit the type, built from the nodes as they were before this edit.
    private static DocumentNode Aligned(IReadOnlyList<DocumentNode> nodes, int index, bool isClosed, NodeType type)
    {
        var node = nodes[index];
        var count = nodes.Count;
        DocumentNode? previous = index > 0 ? nodes[index - 1] : isClosed ? nodes[count - 1] : null;
        DocumentNode? next = index < count - 1 ? nodes[index + 1] : isClosed ? nodes[0] : null;
        var distanceToPrevious = previous is { } p ? Distance(p.X, p.Y, node.X, node.Y) : 0;
        var distanceToNext = next is { } n ? Distance(n.X, n.Y, node.X, node.Y) : 0;

        (double X, double Y)? inHandle = node.HasIn ? (node.InX!.Value, node.InY!.Value) : null;
        (double X, double Y)? outHandle = node.HasOut ? (node.OutX!.Value, node.OutY!.Value) : null;

        if (outHandle is { } o && Distance(o.X, o.Y, node.X, node.Y) > MinimumHandleLength)
        {
            var outLength = Distance(o.X, o.Y, node.X, node.Y);
            var inLength = type == NodeType.Symmetric ? outLength : inHandle is { } i ? Distance(i.X, i.Y, node.X, node.Y) : distanceToPrevious / 3;
            inHandle = OppositeOf(node, o, inLength, inHandle, createIfMissing: previous is not null);
        }
        else if (inHandle is { } existingIn && Distance(existingIn.X, existingIn.Y, node.X, node.Y) > MinimumHandleLength)
        {
            var inLength = Distance(existingIn.X, existingIn.Y, node.X, node.Y);
            var outLength = type == NodeType.Symmetric ? inLength : distanceToNext / 3;
            outHandle = OppositeOf(node, existingIn, outLength, outHandle, createIfMissing: next is not null);
        }
        else
        {
            (inHandle, outHandle) = HandlesFromNeighbors(node, previous, next, distanceToPrevious, distanceToNext, type);
        }

        return new DocumentNode(node.X, node.Y, inHandle?.X, inHandle?.Y, outHandle?.X, outHandle?.Y, type);
    }

    // The handle opposite to the given one, with the length. A node without that handle only gets one if the caller says so.
    private static (double X, double Y)? OppositeOf(DocumentNode node, (double X, double Y) handle, double length, (double X, double Y)? current, bool createIfMissing)
    {
        if (current is null && !createIfMissing)
        {
            return null;
        }

        if (length <= MinimumHandleLength)
        {
            return current;
        }

        var dx = handle.X - node.X;
        var dy = handle.Y - node.Y;
        var size = Math.Sqrt(dx * dx + dy * dy);
        return (node.X - dx / size * length, node.Y - dy / size * length);
    }

    // The handles of a node that has none: along the line from the previous to the next neighbor.
    private static ((double X, double Y)? In, (double X, double Y)? Out) HandlesFromNeighbors(
        DocumentNode node,
        DocumentNode? previous,
        DocumentNode? next,
        double distanceToPrevious,
        double distanceToNext,
        NodeType type)
    {
        double directionX;
        double directionY;
        if (previous is { } p && next is { } n)
        {
            directionX = n.X - p.X;
            directionY = n.Y - p.Y;
        }
        else if (next is { } onlyNext)
        {
            directionX = onlyNext.X - node.X;
            directionY = onlyNext.Y - node.Y;
        }
        else if (previous is { } onlyPrevious)
        {
            directionX = node.X - onlyPrevious.X;
            directionY = node.Y - onlyPrevious.Y;
        }
        else
        {
            return (null, null);
        }

        var size = Math.Sqrt(directionX * directionX + directionY * directionY);
        if (size <= MinimumHandleLength)
        {
            // The neighbors lie on one spot, so there is no line to follow.
            return (null, null);
        }

        directionX /= size;
        directionY /= size;
        var inLength = distanceToPrevious / 3;
        var outLength = distanceToNext / 3;
        if (type == NodeType.Symmetric && inLength > MinimumHandleLength && outLength > MinimumHandleLength)
        {
            inLength = outLength = Math.Min(inLength, outLength);
        }

        (double X, double Y)? inHandle = inLength > MinimumHandleLength && previous is not null
            ? (node.X - directionX * inLength, node.Y - directionY * inLength)
            : null;
        (double X, double Y)? outHandle = outLength > MinimumHandleLength && next is not null
            ? (node.X + directionX * outLength, node.Y + directionY * outLength)
            : null;
        return (inHandle, outHandle);
    }

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
