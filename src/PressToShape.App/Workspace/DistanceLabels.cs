using System;
using System.Collections.Generic;
using PressToShape.Core.Geometry;

namespace PressToShape.App.Workspace;

/// <summary>
/// The distance labels of the smart guides: from the dragged box to the nearest box on each of the four sides (see <see cref="SmartNeighbours"/>).
/// The line runs across the gap in the middle of the overlap and carries the gap as its value. Pure function.
/// </summary>
internal static class DistanceLabels
{
    /// <summary>Up to four <see cref="SmartMarkerType.Distance"/> markers, one for each side that has a neighbour. Nothing for a box that is not finite.</summary>
    public static IReadOnlyList<SmartMarker> Build(Bounds dragged, SmartTargets targets)
    {
        ArgumentNullException.ThrowIfNull(targets);

        var markers = new List<SmartMarker>(4);
        foreach (var axis in new[] { SnapAxis.X, SnapAxis.Y })
        {
            foreach (var before in new[] { true, false })
            {
                if (SmartNeighbours.Nearest(dragged, targets, axis, before) is { } neighbour)
                {
                    markers.Add(new SmartMarker(SmartMarkerType.Distance, axis, neighbour.From, neighbour.To, neighbour.Cross, neighbour.Gap));
                }
            }
        }

        return markers;
    }
}
