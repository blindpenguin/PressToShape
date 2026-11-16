namespace PressToShape.App.Workspace;

/// <summary>One step of the outline of a path in screen pixels. <see cref="PathCommandType.Move"/> and <see cref="PathCommandType.Line"/> use only the end point, <see cref="PathCommandType.Close"/> none.</summary>
/// <param name="Type">What the step does.</param>
/// <param name="X">End point x.</param>
/// <param name="Y">End point y.</param>
/// <param name="Control1X">First handle point x, for a curve.</param>
/// <param name="Control1Y">First handle point y, for a curve.</param>
/// <param name="Control2X">Second handle point x, for a curve.</param>
/// <param name="Control2Y">Second handle point y, for a curve.</param>
internal readonly record struct PathCommand(
    PathCommandType Type,
    double X = 0,
    double Y = 0,
    double Control1X = 0,
    double Control1Y = 0,
    double Control2X = 0,
    double Control2Y = 0);
