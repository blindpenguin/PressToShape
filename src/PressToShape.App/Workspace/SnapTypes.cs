using System;
using System.Collections.Generic;

namespace PressToShape.App.Workspace;

/// <summary>Which snaps are on and the grid spacing, as <see cref="Snapper"/> needs them.</summary>
/// <param name="SnapToGrid">Whether points snap to grid lines.</param>
/// <param name="SnapToObjects">Whether points snap to the targets of other objects.</param>
/// <param name="GridSpacing">The distance between grid lines in document units.</param>
internal readonly record struct SnapSettings(bool SnapToGrid, bool SnapToObjects, double GridSpacing)
{
    /// <summary>The current state of the preferences.</summary>
    public static SnapSettings From(SnapPreferences preferences) =>
        new(preferences.SnapToGrid, preferences.SnapToObjects, preferences.GridSpacing);
}

/// <summary>A guide line to draw while a point is snapped to an object. All numbers are document units.</summary>
/// <param name="Axis">X for a vertical line at an x position, Y for a horizontal line at a y position.</param>
/// <param name="Position">The x or the y of the line.</param>
/// <param name="From">Where the line starts along the other axis.</param>
/// <param name="To">Where the line ends along the other axis.</param>
internal readonly record struct SnapGuide(SnapAxis Axis, double Position, double From, double To);

/// <summary>The correction a snap gives: add the offsets to the dragged position. The guides are the object targets that were hit.</summary>
internal sealed record SnapResult(double OffsetX, double OffsetY, IReadOnlyList<SnapGuide> Guides)
{
    /// <summary>The markers of the smart guide snap that won on an axis, to draw while the drag lasts.</summary>
    public IReadOnlyList<SmartMarker> Markers { get; init; } = [];

    /// <summary>The ids of the ruler guides the drag snapped to. The canvas draws them as active.</summary>
    public IReadOnlyList<Guid> ActiveGuideIds { get; init; } = [];

    /// <summary>No snap: no offset and no guide.</summary>
    public static SnapResult None { get; } = new(0, 0, []);
}

/// <summary>
/// A snap that a smart guide offers on one axis: move the dragged box by <paramref name="Offset"/> and show the <paramref name="Markers"/>.
/// <see cref="Snapper"/> compares it with the other snaps of that axis.
/// </summary>
/// <param name="Axis">The axis the offset is on.</param>
/// <param name="Offset">How far to move, in document units.</param>
/// <param name="Markers">What to draw while the snap is active.</param>
internal sealed record SnapCandidate(SnapAxis Axis, double Offset, IReadOnlyList<SmartMarker> Markers);
