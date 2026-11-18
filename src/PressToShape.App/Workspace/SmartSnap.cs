using System;
using System.Collections.Generic;
using PressToShape.Core.Geometry;

namespace PressToShape.App.Workspace;

/// <summary>Which equal spacing snaps apply on an axis: the gap before the box (left, above), the gap after it, and the middle between the two.</summary>
internal readonly record struct SpacingSides(bool Before, bool After, bool Centered)
{
    /// <summary>All three.</summary>
    public static SpacingSides All { get; } = new(true, true, true);

    /// <summary>None: the axis is skipped.</summary>
    public static SpacingSides None { get; } = new(false, false, false);

    /// <summary>Only the gap after the box: for a resize that drags the right or the bottom edge.</summary>
    public static SpacingSides AfterOnly { get; } = new(false, true, false);

    /// <summary>Only the gap before the box: for a resize that drags the left or the top edge.</summary>
    public static SpacingSides BeforeOnly { get; } = new(true, false, false);
}

/// <summary>
/// The snaps of the smart guides that need the other objects: equal spacing and equal size. Each function returns candidates that
/// <see cref="Snapper"/> compares with the edge, guide and grid snaps. Pure functions; input that is not finite gives no candidate.
/// </summary>
internal static class SmartSnap
{
    // The gaps that are as long as the gap a snap makes equal are found with this tolerance (they are equal up to rounding).
    private const double SameSize = 1e-6;

    // At most this many existing gaps are marked next to the gap of the dragged box.
    private const int MaxMarkedGaps = 3;

    /// <summary>
    /// The equal spacing snaps for a dragged box, on both axes. A gap to the nearest neighbour on a side that is within the tolerance of a gap
    /// that exists between two other boxes snaps to that gap. A box with a neighbour on each side snaps to the middle between them when the two gaps
    /// are within the tolerance of being equal (each gap changes by half the difference, so the offset is that half). The markers show the gaps that
    /// are equal after the snap.
    /// </summary>
    /// <param name="dragged">The box as the pointer puts it, before any snap.</param>
    /// <param name="targets">The other boxes and their gaps.</param>
    /// <param name="tolerance">The tolerance in document units.</param>
    /// <param name="x">Which snaps apply on the x axis. Null means all of them. Use <see cref="SpacingSides.None"/> to skip the axis.</param>
    /// <param name="y">Which snaps apply on the y axis. Null means all of them.</param>
    public static IReadOnlyList<SnapCandidate> EqualSpacing(Bounds dragged, SmartTargets targets, double tolerance, SpacingSides? x = null, SpacingSides? y = null)
    {
        ArgumentNullException.ThrowIfNull(targets);

        if (!IsFinite(dragged) || !double.IsFinite(tolerance) || tolerance < 0)
        {
            return [];
        }

        var candidates = new List<SnapCandidate>();
        foreach (var axis in new[] { SnapAxis.X, SnapAxis.Y })
        {
            var sides = (axis == SnapAxis.X ? x : y) ?? SpacingSides.All;
            var before = sides.Before || sides.Centered ? SmartNeighbours.Nearest(dragged, targets, axis, before: true) : null;
            var after = sides.After || sides.Centered ? SmartNeighbours.Nearest(dragged, targets, axis, before: false) : null;

            if (sides.Before && before is { } b && NearestGap(targets, axis, b.Gap, tolerance) is { } equalBefore)
            {
                // Moving the box towards larger coordinates makes the gap before it larger.
                Add(candidates, axis, equalBefore.Size - b.Gap, tolerance, shift => [Marker(axis, b, shift, 1), .. Existing(targets, axis, equalBefore.Size)]);
            }

            if (sides.After && after is { } a && NearestGap(targets, axis, a.Gap, tolerance) is { } equalAfter)
            {
                Add(candidates, axis, a.Gap - equalAfter.Size, tolerance, shift => [Marker(axis, a, shift, -1), .. Existing(targets, axis, equalAfter.Size)]);
            }

            if (sides.Centered && before is { } left && after is { } right)
            {
                Add(candidates, axis, (right.Gap - left.Gap) / 2, tolerance, shift => [Marker(axis, left, shift, 1), Marker(axis, right, shift, -1)]);
            }
        }

        return candidates;
    }

    /// <summary>
    /// The equal size snaps for a box that is being resized or drawn. On an axis where an edge moves (the direction is not 0) the width or
    /// height of the box snaps to the width or height of another box within the tolerance. The offset is for the pointer on that axis: the edge
    /// that the pointer drags moves by the size difference, and with <paramref name="fromCenter"/> (both edges move) by half of it. The markers
    /// are a bracket along the side of the dragged box as it will be and one along the same side of the matching box.
    /// </summary>
    /// <param name="box">The box as the pointer puts it, before any snap.</param>
    /// <param name="directionX">1 if the right edge is dragged, -1 if the left edge, 0 if no edge moves horizontally.</param>
    /// <param name="directionY">1 if the bottom edge is dragged, -1 if the top edge, 0 if no edge moves vertically.</param>
    /// <param name="fromCenter">Whether the box grows on both sides (the Alt key).</param>
    /// <param name="targets">The other boxes and their sizes.</param>
    /// <param name="tolerance">The tolerance in document units.</param>
    public static IReadOnlyList<SnapCandidate> EqualSize(Bounds box, int directionX, int directionY, bool fromCenter, SmartTargets targets, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(targets);

        if (!IsFinite(box) || !double.IsFinite(tolerance) || tolerance < 0)
        {
            return [];
        }

        var candidates = new List<SnapCandidate>(2);
        foreach (var (axis, direction) in new[] { (SnapAxis.X, directionX), (SnapAxis.Y, directionY) })
        {
            if (direction == 0)
            {
                continue;
            }

            var size = axis == SnapAxis.X ? box.Width : box.Height;
            if (!(size > 0) || targets.FindSize(axis, size, tolerance) is not { } match)
            {
                continue;
            }

            var change = match.Size - size;
            var offset = Math.Sign(direction) * change / (fromCenter ? 2 : 1);
            var (start, end) = SmartNeighbours.Span(box, axis);
            var (newStart, newEnd) = fromCenter
                ? ((start + end) / 2 - match.Size / 2, (start + end) / 2 + match.Size / 2)
                : direction > 0 ? (start, start + match.Size) : (end - match.Size, end);
            var cross = axis == SnapAxis.X ? box.Y + box.Height : box.X + box.Width;
            var (matchStart, matchEnd) = SmartNeighbours.Span(match.Box, axis);
            var matchCross = axis == SnapAxis.X ? match.Box.Y + match.Box.Height : match.Box.X + match.Box.Width;
            candidates.Add(new SnapCandidate(
                axis,
                offset,
                [
                    new SmartMarker(SmartMarkerType.EqualSize, axis, newStart, newEnd, cross, match.Size),
                    new SmartMarker(SmartMarkerType.EqualSize, axis, matchStart, matchEnd, matchCross, match.Size),
                ]));
        }

        return candidates;
    }

    private static void Add(List<SnapCandidate> candidates, SnapAxis axis, double offset, double tolerance, Func<double, SmartMarker[]> markers)
    {
        if (double.IsFinite(offset) && Math.Abs(offset) <= tolerance)
        {
            candidates.Add(new SnapCandidate(axis, offset, markers(offset)));
        }
    }

    // The existing gap whose size is nearest to the size, within the tolerance.
    private static SmartGap? NearestGap(SmartTargets targets, SnapAxis axis, double size, double tolerance)
    {
        var found = targets.FindGaps(axis, size, tolerance, 1);
        return found.Count == 0 ? null : found[0];
    }

    // The gaps between other boxes that are as long as the gap that was made equal to them.
    private static IEnumerable<SmartMarker> Existing(SmartTargets targets, SnapAxis axis, double size)
    {
        foreach (var gap in targets.FindGaps(axis, size, SameSize, MaxMarkedGaps))
        {
            yield return new SmartMarker(SmartMarkerType.EqualGap, axis, gap.From, gap.To, gap.Cross, gap.Size);
        }
    }

    // The gap between the dragged box and a neighbour after the box moved by the shift. A gap before the box grows with the shift, a gap after it shrinks.
    private static SmartMarker Marker(SnapAxis axis, SmartNeighbour neighbour, double shift, int direction) => direction > 0
        ? new SmartMarker(SmartMarkerType.EqualGap, axis, neighbour.From, neighbour.To + shift, neighbour.Cross, neighbour.Gap + shift)
        : new SmartMarker(SmartMarkerType.EqualGap, axis, neighbour.From + shift, neighbour.To, neighbour.Cross, neighbour.Gap - shift);

    private static bool IsFinite(Bounds box) =>
        double.IsFinite(box.X) && double.IsFinite(box.Y) && double.IsFinite(box.Width) && double.IsFinite(box.Height);
}
