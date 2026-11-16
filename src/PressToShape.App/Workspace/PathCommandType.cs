namespace PressToShape.App.Workspace;

/// <summary>What a <see cref="PathCommand"/> does.</summary>
internal enum PathCommandType
{
    /// <summary>Starts the outline at a point.</summary>
    Move,

    /// <summary>A straight line to a point.</summary>
    Line,

    /// <summary>A cubic bezier curve to a point, with two handle points.</summary>
    Cubic,

    /// <summary>Ends a closed outline.</summary>
    Close,
}
