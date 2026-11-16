namespace PressToShape.Core.Shapes;

/// <summary>Which of the two handles of a node: the one toward the previous node or the one toward the next node.</summary>
public enum NodeHandleSide
{
    /// <summary>The handle on the side of the previous node.</summary>
    In,

    /// <summary>The handle on the side of the next node.</summary>
    Out,
}
