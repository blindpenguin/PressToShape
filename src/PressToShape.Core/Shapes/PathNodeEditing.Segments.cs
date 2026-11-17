using System;
using System.Collections.Generic;

namespace PressToShape.Core.Shapes;

public static partial class PathNodeEditing
{
    // The pull of a bend is spread over the two handles of the segment. Near an end the weights would grow without limit, so the
    // parameter that decides the weights stays inside this range: the point still follows the pointer there, only less exactly.
    private const double MinimumBendParameter = 0.15;
    private const double MaximumBendParameter = 0.85;

    /// <summary>
    /// Bends a segment so that its point at <paramref name="t"/> moves by the distance, by changing only the two handle points of the
    /// segment. A straight segment becomes a curve this way. Afterwards a node whose handles no longer match its type becomes a corner.
    /// For <paramref name="t"/> from 0.15 to 0.85 the point moves exactly by the distance, nearer to an end only approximately.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="segmentIndex">The segment, from the node of that index to the next one (see <see cref="PathEditing.InsertNode"/>), counted across the sub-paths.</param>
    /// <param name="t">Where on the segment, from 0 to 1.</param>
    /// <param name="dx">The distance in x, in the frame before the rotation.</param>
    /// <param name="dy">The distance in y, in the frame before the rotation.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a segment.</exception>
    public static PathShape BendSegment(PathShape path, int segmentIndex, double t, double dx, double dy)
    {
        ArgumentNullException.ThrowIfNull(path);
        CheckSegment(path, segmentIndex);
        Guard.Finite(t, nameof(t));
        Guard.Finite(dx, nameof(dx));
        Guard.Finite(dy, nameof(dy));
        if (dx == 0 && dy == 0)
        {
            return path;
        }

        var (nodes, startIndex, endIndex, buffer) = SegmentNodes(path, segmentIndex);
        var start = nodes[startIndex];
        var end = nodes[endIndex];

        var weighted = Math.Clamp(t, MinimumBendParameter, MaximumBendParameter);
        var scale = 1 / (3 * weighted * (1 - weighted) * ((1 - weighted) * (1 - weighted) + weighted * weighted));
        var startWeight = (1 - weighted) * scale;
        var endWeight = weighted * scale;

        var outX = (start.OutX ?? start.X) + dx * startWeight;
        var outY = (start.OutY ?? start.Y) + dy * startWeight;
        var inX = (end.InX ?? end.X) + dx * endWeight;
        var inY = (end.InY ?? end.Y) + dy * endWeight;

        nodes[startIndex] = Fitted(new DocumentNode(start.X, start.Y, start.InX, start.InY, outX, outY, start.Type));
        nodes[endIndex] = Fitted(new DocumentNode(end.X, end.Y, inX, inY, end.OutX, end.OutY, end.Type));
        return buffer.Build()!;
    }

    /// <summary>
    /// Makes a segment straight: the handles that belong to it are removed. A node that lost a handle becomes a corner. A segment that is
    /// already straight leaves the path as it is.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a segment.</exception>
    public static PathShape StraightenSegment(PathShape path, int segmentIndex)
    {
        ArgumentNullException.ThrowIfNull(path);
        CheckSegment(path, segmentIndex);

        var (nodes, startIndex, endIndex, buffer) = SegmentNodes(path, segmentIndex);
        var start = nodes[startIndex];
        var end = nodes[endIndex];
        if (!start.HasOut && !end.HasIn)
        {
            return path;
        }

        nodes[startIndex] = start.HasOut
            ? new DocumentNode(start.X, start.Y, start.InX, start.InY, type: NodeType.Corner)
            : start;
        nodes[endIndex] = end.HasIn
            ? new DocumentNode(end.X, end.Y, outX: end.OutX, outY: end.OutY, type: NodeType.Corner)
            : end;
        return buffer.Build()!;
    }

    /// <summary>
    /// Makes a straight segment a curve with handles one third of its length from each end, on the line itself, so the picture does not
    /// change. A segment that is already curved (it has a handle) leaves the path as it is, and so does a segment of no length. A node
    /// whose handles no longer match its type becomes a corner.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a segment.</exception>
    public static PathShape CurveSegment(PathShape path, int segmentIndex)
    {
        ArgumentNullException.ThrowIfNull(path);
        CheckSegment(path, segmentIndex);

        var (nodes, startIndex, endIndex, buffer) = SegmentNodes(path, segmentIndex);
        var start = nodes[startIndex];
        var end = nodes[endIndex];
        var lengthX = end.X - start.X;
        var lengthY = end.Y - start.Y;
        if (start.HasOut || end.HasIn || Math.Sqrt(lengthX * lengthX + lengthY * lengthY) <= MinimumHandleLength)
        {
            return path;
        }

        nodes[startIndex] = Fitted(new DocumentNode(start.X, start.Y, start.InX, start.InY, start.X + lengthX / 3, start.Y + lengthY / 3, start.Type));
        nodes[endIndex] = Fitted(new DocumentNode(end.X, end.Y, end.X - lengthX / 3, end.Y - lengthY / 3, end.OutX, end.OutY, end.Type));
        return buffer.Build()!;
    }

    // A node whose handles no longer match its type becomes a corner.
    private static DocumentNode Fitted(DocumentNode node) =>
        NodeTypes.Satisfies(node) ? node : WithType(node, NodeType.Corner);

    private static void CheckSegment(PathShape path, int segmentIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(segmentIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(segmentIndex, path.SegmentCount);
    }

    // The nodes of the sub-path that holds the segment, the indices of its start and end node in them (the end wraps to the first node
    // of a closed sub-path), and the buffer to build the edited path from.
    private static (List<DocumentNode> Nodes, int Start, int End, PathBuffer Buffer) SegmentNodes(PathShape path, int segmentIndex)
    {
        var (subPath, start) = path.LocateSegment(segmentIndex);
        var buffer = PathBuffer.From(path);
        var nodes = buffer.Nodes(subPath);
        return (nodes, start, (start + 1) % nodes.Count, buffer);
    }
}
