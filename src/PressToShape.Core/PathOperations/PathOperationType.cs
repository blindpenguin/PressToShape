namespace PressToShape.Core.PathOperations;

/// <summary>The boolean operation that combines the outlines of shapes.</summary>
public enum PathOperationType
{
    /// <summary>The area covered by any operand.</summary>
    Union,

    /// <summary>The area of the bottom operand that no other operand covers.</summary>
    Difference,

    /// <summary>The area covered by every operand.</summary>
    Intersection,

    /// <summary>The area covered by an odd number of operands.</summary>
    Exclusion,
}
