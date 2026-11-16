namespace PressToShape.Core.Shapes;

/// <summary>
/// A point of a path in document units, in the frame of the path before its rotation. Used to build and edit a
/// <see cref="PathShape"/>, which stores its nodes as fractions of its box (<see cref="PathNode"/>).
/// </summary>
public readonly record struct DocumentNode
{
    /// <summary>Creates a node. Every number is finite, and a handle point is given with both numbers or with none.</summary>
    /// <param name="x">Anchor x in document units.</param>
    /// <param name="y">Anchor y in document units.</param>
    /// <param name="inX">Handle point on the side of the previous node, or null for no handle.</param>
    /// <param name="inY">Handle point on the side of the previous node, or null for no handle.</param>
    /// <param name="outX">Handle point on the side of the next node, or null for no handle.</param>
    /// <param name="outY">Handle point on the side of the next node, or null for no handle.</param>
    /// <param name="type">The node type.</param>
    public DocumentNode(double x, double y, double? inX = null, double? inY = null, double? outX = null, double? outY = null, NodeType type = NodeType.Corner)
    {
        NodeValidation.Check(x, y, inX, inY, outX, outY, type);
        X = x;
        Y = y;
        InX = inX;
        InY = inY;
        OutX = outX;
        OutY = outY;
        Type = type;
    }

    /// <summary>Anchor x in document units.</summary>
    public double X { get; }

    /// <summary>Anchor y in document units.</summary>
    public double Y { get; }

    /// <summary>In handle point x, or null together with <see cref="InY"/> when there is no in handle.</summary>
    public double? InX { get; }

    /// <summary>In handle point y.</summary>
    public double? InY { get; }

    /// <summary>Out handle point x, or null together with <see cref="OutY"/> when there is no out handle.</summary>
    public double? OutX { get; }

    /// <summary>Out handle point y.</summary>
    public double? OutY { get; }

    /// <summary>Whether the node has an in handle.</summary>
    /// <summary>How the two handles relate when one of them is edited. The default is <see cref="NodeType.Corner"/>.</summary>
    public NodeType Type { get; }

    public bool HasIn => InX.HasValue;

    /// <summary>Whether the node has an out handle.</summary>
    public bool HasOut => OutX.HasValue;
}
