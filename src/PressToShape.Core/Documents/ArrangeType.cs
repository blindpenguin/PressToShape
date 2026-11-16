namespace PressToShape.Core.Documents;

/// <summary>How <see cref="Document.Arrange"/> changes the stacking order of shapes among their siblings.</summary>
public enum ArrangeType
{
    /// <summary>Above all siblings.</summary>
    ToFront,

    /// <summary>One place up, past the next sibling that is not moved.</summary>
    Forward,

    /// <summary>One place down, past the next sibling that is not moved.</summary>
    Backward,

    /// <summary>Below all siblings.</summary>
    ToBack,
}
