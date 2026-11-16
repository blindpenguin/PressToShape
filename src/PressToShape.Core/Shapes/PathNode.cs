namespace PressToShape.Core.Shapes;

/// <summary>
/// A point of a <see cref="PathShape"/> as it is stored: the anchor and the optional handle points are fractions of the path's box,
/// so 0 is the left or top edge and 1 the right or bottom edge. Handle points may lie outside 0..1. Resizing the box scales the
/// path without touching the nodes. A segment is a curve if the node it starts at has an out handle or the node it ends at has an
/// in handle, otherwise it is a line.
/// </summary>
public readonly record struct PathNode
{
    /// <summary>Creates a node. Every number is finite, and a handle point is given with both numbers or with none.</summary>
    /// <param name="x">Anchor, fraction of the box width.</param>
    /// <param name="y">Anchor, fraction of the box height.</param>
    /// <param name="inX">Handle point on the side of the previous node, fraction of the box width, or null for no handle.</param>
    /// <param name="inY">Handle point on the side of the previous node, fraction of the box height, or null for no handle.</param>
    /// <param name="outX">Handle point on the side of the next node, fraction of the box width, or null for no handle.</param>
    /// <param name="outY">Handle point on the side of the next node, fraction of the box height, or null for no handle.</param>
    public PathNode(double x, double y, double? inX = null, double? inY = null, double? outX = null, double? outY = null)
    {
        NodeValidation.Check(x, y, inX, inY, outX, outY);
        X = x;
        Y = y;
        InX = inX;
        InY = inY;
        OutX = outX;
        OutY = outY;
    }

    /// <summary>Anchor, fraction of the box width.</summary>
    public double X { get; }

    /// <summary>Anchor, fraction of the box height.</summary>
    public double Y { get; }

    /// <summary>In handle point, fraction of the box width. Null together with <see cref="InY"/> when there is no in handle.</summary>
    public double? InX { get; }

    /// <summary>In handle point, fraction of the box height.</summary>
    public double? InY { get; }

    /// <summary>Out handle point, fraction of the box width. Null together with <see cref="OutY"/> when there is no out handle.</summary>
    public double? OutX { get; }

    /// <summary>Out handle point, fraction of the box height.</summary>
    public double? OutY { get; }

    /// <summary>Whether the node has an in handle.</summary>
    public bool HasIn => InX.HasValue;

    /// <summary>Whether the node has an out handle.</summary>
    public bool HasOut => OutX.HasValue;
}
