using System;
using PressToShape.Core.Geometry;

namespace PressToShape.App.Workspace;

/// <summary>A box next to the dragged box: how far away it is and where the measure line between them runs.</summary>
/// <param name="Gap">The distance between the two boxes along the axis. More than 0.</param>
/// <param name="From">Where the gap starts along the axis.</param>
/// <param name="To">Where the gap ends along the axis.</param>
/// <param name="Cross">The middle of the part where the two boxes overlap on the other axis.</param>
internal readonly record struct SmartNeighbour(double Gap, double From, double To, double Cross);

/// <summary>Finds the box next to a dragged box. Pure function, used by the distance labels and the equal spacing snap.</summary>
internal static class SmartNeighbours
{
    /// <summary>
    /// The nearest box before (left, above) or after (right, below) the dragged box along the axis that is clear of it (a gap of more than 0) and
    /// overlaps it on the other axis. A dragged box without extent on the other axis overlaps a box it lies strictly inside of. <c>null</c> if there is none.
    /// </summary>
    public static SmartNeighbour? Nearest(Bounds dragged, SmartTargets targets, SnapAxis axis, bool before)
    {
        ArgumentNullException.ThrowIfNull(targets);

        var (start, end) = Span(dragged, axis);
        var (crossStart, crossEnd) = Span(dragged, Other(axis));
        if (!double.IsFinite(start) || !double.IsFinite(end) || !double.IsFinite(crossStart) || !double.IsFinite(crossEnd))
        {
            return null;
        }

        SmartNeighbour? best = null;
        foreach (var box in targets.Boxes)
        {
            var (boxStart, boxEnd) = Span(box.Bounds, axis);
            var gap = before ? start - boxEnd : boxStart - end;
            if (!(gap > 0) || (best is { } current && gap >= current.Gap))
            {
                continue;
            }

            var (boxCrossStart, boxCrossEnd) = Span(box.Bounds, Other(axis));
            var overlapStart = Math.Max(crossStart, boxCrossStart);
            var overlapEnd = Math.Min(crossEnd, boxCrossEnd);
            var overlaps = crossStart == crossEnd ? boxCrossStart < crossStart && crossStart < boxCrossEnd : overlapEnd > overlapStart;
            if (!overlaps)
            {
                continue;
            }

            var cross = (overlapStart + overlapEnd) / 2;
            best = before ? new SmartNeighbour(gap, boxEnd, start, cross) : new SmartNeighbour(gap, end, boxStart, cross);
        }

        return best;
    }

    /// <summary>The other axis.</summary>
    public static SnapAxis Other(SnapAxis axis) => axis == SnapAxis.X ? SnapAxis.Y : SnapAxis.X;

    /// <summary>The start and the end of the bounds along the axis.</summary>
    public static (double Start, double End) Span(Bounds bounds, SnapAxis axis) =>
        axis == SnapAxis.X ? (bounds.X, bounds.X + bounds.Width) : (bounds.Y, bounds.Y + bounds.Height);
}
