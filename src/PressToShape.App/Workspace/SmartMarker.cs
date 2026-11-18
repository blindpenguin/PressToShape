namespace PressToShape.App.Workspace;

/// <summary>What a smart marker shows.</summary>
internal enum SmartMarkerType
{
    /// <summary>The distance between the dragged box and a neighbour: a measure line with a number.</summary>
    Distance,

    /// <summary>A gap that is equal to another gap: a measure line with end ticks and a number.</summary>
    EqualGap,

    /// <summary>A side of a box whose size equals the size of another box: a bracket along the side.</summary>
    EqualSize,

    /// <summary>A segment middle or a crossing that a point snapped to: a small cross.</summary>
    PathPoint,
}

/// <summary>
/// One thing the canvas draws while a smart guide is active, in document units. A line along <paramref name="Axis"/> (X: horizontal line,
/// the x values change) from <paramref name="From"/> to <paramref name="To"/> at <paramref name="Cross"/> on the other axis. A
/// <see cref="SmartMarkerType.PathPoint"/> is the point (<paramref name="From"/>, <paramref name="Cross"/>) and has no length.
/// </summary>
/// <param name="Type">What the marker shows.</param>
/// <param name="Axis">The axis the line runs along.</param>
/// <param name="From">Where the line starts along its axis.</param>
/// <param name="To">Where the line ends along its axis.</param>
/// <param name="Cross">The position on the other axis.</param>
/// <param name="Value">The number to show next to the line (a distance or a size), or <see cref="double.NaN"/> for none.</param>
internal readonly record struct SmartMarker(SmartMarkerType Type, SnapAxis Axis, double From, double To, double Cross, double Value);
