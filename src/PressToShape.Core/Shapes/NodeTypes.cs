using System;

namespace PressToShape.Core.Shapes;

/// <summary>
/// The rules that connect a <see cref="NodeType"/> with the handles of a node. A node counts as having its handles on one line when the
/// sine of the angle between them is below 1e-6, and as equally long when the lengths differ by less than 1e-6 of the longer one.
/// </summary>
public static class NodeTypes
{
    private const double Tolerance = 1e-6;

    // A handle this short has no direction.
    private const double MinimumLength = 1e-12;

    /// <summary>
    /// The type that matches the handles of the node, whatever type it stores: fewer than two handles give a corner, two handles on one
    /// line on opposite sides give symmetric if they are equally long and smooth otherwise, every other case gives a corner.
    /// </summary>
    public static NodeType Derive(PathNode node) => Derive(node.X, node.Y, node.InX, node.InY, node.OutX, node.OutY);

    /// <inheritdoc cref="Derive(PathNode)"/>
    public static NodeType Derive(DocumentNode node) => Derive(node.X, node.Y, node.InX, node.InY, node.OutX, node.OutY);

    /// <summary>
    /// Whether the handles of the node match the type it stores. A corner always does, so does a node with at most one handle.
    /// A smooth node needs its handles on one line on opposite sides, a symmetric node also needs them equally long.
    /// </summary>
    public static bool Satisfies(PathNode node) => Satisfies(node.Type, node.X, node.Y, node.InX, node.InY, node.OutX, node.OutY);

    /// <inheritdoc cref="Satisfies(PathNode)"/>
    public static bool Satisfies(DocumentNode node) => Satisfies(node.Type, node.X, node.Y, node.InX, node.InY, node.OutX, node.OutY);

    private static NodeType Derive(double x, double y, double? inX, double? inY, double? outX, double? outY)
    {
        if (!Measure(x, y, inX, inY, outX, outY, out var inLength, out var outLength, out var opposite))
        {
            return NodeType.Corner;
        }

        if (inLength <= MinimumLength || outLength <= MinimumLength || !opposite)
        {
            return NodeType.Corner;
        }

        return IsEqual(inLength, outLength) ? NodeType.Symmetric : NodeType.Smooth;
    }

    private static bool Satisfies(NodeType type, double x, double y, double? inX, double? inY, double? outX, double? outY)
    {
        if (type == NodeType.Corner || !Measure(x, y, inX, inY, outX, outY, out var inLength, out var outLength, out var opposite))
        {
            return true;
        }

        // A handle without length has no direction, so only the lengths can disagree.
        var hasDirection = inLength > MinimumLength && outLength > MinimumLength;
        if (type == NodeType.Smooth)
        {
            return !hasDirection || opposite;
        }

        return IsEqual(inLength, outLength) && (!hasDirection || opposite);
    }

    // False when the node has fewer than two handles. Otherwise the lengths of the handles and whether they lie on one line on opposite sides.
    private static bool Measure(double x, double y, double? inX, double? inY, double? outX, double? outY, out double inLength, out double outLength, out bool opposite)
    {
        inLength = 0;
        outLength = 0;
        opposite = false;
        if (inX is not { } ix || inY is not { } iy || outX is not { } ox || outY is not { } oy)
        {
            return false;
        }

        var ax = ix - x;
        var ay = iy - y;
        var bx = ox - x;
        var by = oy - y;
        inLength = Math.Sqrt(ax * ax + ay * ay);
        outLength = Math.Sqrt(bx * bx + by * by);
        if (inLength <= MinimumLength || outLength <= MinimumLength)
        {
            return true;
        }

        var cross = ax * by - ay * bx;
        var dot = ax * bx + ay * by;
        opposite = Math.Abs(cross) <= Tolerance * inLength * outLength && dot < 0;
        return true;
    }

    private static bool IsEqual(double a, double b) => Math.Abs(a - b) <= Tolerance * Math.Max(a, b);
}
