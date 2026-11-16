namespace PressToShape.Core.Shapes;

/// <summary>
/// How the two handles of a node relate when one of them is edited. The type is a rule for editing: a model value that does not match
/// its type (for example after a non-uniform resize) is not an error, the next handle edit enforces the type again.
/// </summary>
public enum NodeType
{
    /// <summary>The handles move independently, and the node may have none, one or two handles.</summary>
    Corner,

    /// <summary>The two handles lie on one straight line through the anchor on opposite sides, each with its own length.</summary>
    Smooth,

    /// <summary>The two handles lie on one straight line through the anchor on opposite sides and have the same length.</summary>
    Symmetric,
}
