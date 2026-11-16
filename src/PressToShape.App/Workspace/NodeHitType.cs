namespace PressToShape.App.Workspace;

/// <summary>What a <see cref="NodeHit"/> found.</summary>
internal enum NodeHitType
{
    /// <summary>Nothing.</summary>
    None,

    /// <summary>The anchor of a node.</summary>
    Node,

    /// <summary>The handle point of a selected node, or of a neighbor of one.</summary>
    Handle,

    /// <summary>A segment, between its nodes.</summary>
    Segment,
}
