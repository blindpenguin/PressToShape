namespace PressToShape.App.Workspace;

/// <summary>What a press with the Pen did to the unfinished path.</summary>
internal enum PenPressResult
{
    /// <summary>A node was added. A drag that follows turns it into a smooth point.</summary>
    AddedNode,

    /// <summary>The press hit the first node of a path with at least three nodes: the path is to be finished as closed.</summary>
    ClosedPath,

    /// <summary>The press landed on the last node (a double click or double tap): the path is to be finished as open.</summary>
    FinishedPath,
}
