using System;
using System.Collections.Generic;
using PressToShape.Core.Geometry;

namespace PressToShape.App.Workspace;

/// <summary>
/// Finds the correction that makes a dragged point or box line up with a target. The x axis and the y axis snap on their own. On an axis
/// the nearest object target within the tolerance wins; only when there is none the nearest grid line does, so an object target beats a
/// grid line that is nearer. Of several candidates (the left edge, the center and the right edge of a box) the one with the smallest
/// distance to a target decides. The tolerance is <see cref="TolerancePixels"/> on the screen, so it feels the same at every zoom.
/// Pure functions: no input that is not finite gives NaN or infinity, it gives no snap.
/// </summary>
internal static class Snapper
{
    /// <summary>How close, in screen pixels, a point must be to a target or a grid line to snap.</summary>
    public const double TolerancePixels = 8;

    // Candidates that land on the same target after the offset are drawn as guides too. They are equal up to rounding.
    private const double SamePosition = 1e-6;

    /// <summary>The left edge, the center and the right edge of the bounds: the candidates on the x axis.</summary>
    public static IReadOnlyList<double> CandidatesX(Bounds bounds) => [bounds.X, bounds.X + bounds.Width / 2, bounds.X + bounds.Width];

    /// <summary>The top edge, the center and the bottom edge of the bounds: the candidates on the y axis.</summary>
    public static IReadOnlyList<double> CandidatesY(Bounds bounds) => [bounds.Y, bounds.Y + bounds.Height / 2, bounds.Y + bounds.Height];

    /// <summary>
    /// The offsets that put the best candidate on a target or a grid line, and the guide lines of the object targets that were hit.
    /// </summary>
    /// <param name="xCandidates">The x positions of the dragged thing that may snap (document units).</param>
    /// <param name="yCandidates">The y positions of the dragged thing that may snap.</param>
    /// <param name="targets">The targets of the other objects. Null or empty means there are none.</param>
    /// <param name="settings">Which snaps are on and the grid spacing.</param>
    /// <param name="zoomPercent">The zoom in percent. 100 means one document unit per screen pixel.</param>
    /// <param name="dragged">The bounds of the dragged thing before the offsets, so a guide line reaches from the target to it. Null draws only the target's extent.</param>
    /// <param name="extras">The snaps of the smart guides (equal spacing, equal size). On an axis the smallest distance wins among the object, guide and extra snaps; a ruler guide wins a tie, an extra wins a tie with an object edge, and an object snap or an extra always beats a grid line.</param>
    public static SnapResult Snap(
        IReadOnlyList<double> xCandidates,
        IReadOnlyList<double> yCandidates,
        SnapTargets? targets,
        SnapSettings settings,
        double zoomPercent,
        Bounds? dragged = null,
        IReadOnlyList<SnapCandidate>? extras = null)
    {
        ArgumentNullException.ThrowIfNull(xCandidates);
        ArgumentNullException.ThrowIfNull(yCandidates);

        if (!double.IsFinite(zoomPercent) || zoomPercent <= 0)
        {
            return SnapResult.None;
        }

        var tolerance = TolerancePixels / (zoomPercent / 100);
        var x = SnapAxisOffset(SnapAxis.X, xCandidates, targets, settings, tolerance, extras);
        var y = SnapAxisOffset(SnapAxis.Y, yCandidates, targets, settings, tolerance, extras);
        if (x is null && y is null)
        {
            return SnapResult.None;
        }

        var offsetX = x?.Offset ?? 0;
        var offsetY = y?.Offset ?? 0;
        var guides = new List<SnapGuide>();
        var markers = new List<SmartMarker>();
        var activeGuides = new List<Guid>();
        foreach (var (axis, snap, candidates, offset, otherOffset) in new[]
        {
            (SnapAxis.X, x, xCandidates, offsetX, offsetY),
            (SnapAxis.Y, y, yCandidates, offsetY, offsetX),
        })
        {
            if (snap is { Extra: { } extra })
            {
                markers.AddRange(extra.Markers);
            }
            else if (snap is { IsObject: true })
            {
                (double From, double To)? extent = dragged is { } box
                    ? axis == SnapAxis.X ? (box.Y + otherOffset, box.Y + box.Height + otherOffset) : (box.X + otherOffset, box.X + box.Width + otherOffset)
                    : null;
                AddGuides(axis, candidates, targets!, offset, extent, guides, activeGuides);
            }
        }

        return new SnapResult(offsetX, offsetY, guides) { Markers = markers, ActiveGuideIds = activeGuides };
    }

    private readonly record struct AxisSnap(double Offset, bool IsObject, SnapCandidate? Extra = null);

    private static AxisSnap? SnapAxisOffset(
        SnapAxis axis,
        IReadOnlyList<double> candidates,
        SnapTargets? targets,
        SnapSettings settings,
        double tolerance,
        IReadOnlyList<SnapCandidate>? extras)
    {
        double? best = null;
        var bestDistance = double.MaxValue;
        var bestIsGuide = false;
        if (settings.SnapToObjects && targets is not null)
        {
            foreach (var candidate in candidates)
            {
                if (targets.TryFindNearest(axis, candidate, tolerance, out var target))
                {
                    var distance = Math.Abs(target.Value - candidate);
                    var better = distance < bestDistance || (distance == bestDistance && !bestIsGuide && target.Type == SnapTargetType.Guide);
                    if (better)
                    {
                        bestDistance = distance;
                        best = target.Value - candidate;
                        bestIsGuide = target.Type == SnapTargetType.Guide;
                    }
                }
            }
        }

        // The smart guide snaps of this axis: the smallest offset within the tolerance.
        SnapCandidate? extra = null;
        foreach (var candidate in extras ?? [])
        {
            if (candidate.Axis == axis && double.IsFinite(candidate.Offset) && Math.Abs(candidate.Offset) <= tolerance
                && (extra is null || Math.Abs(candidate.Offset) < Math.Abs(extra.Offset)))
            {
                extra = candidate;
            }
        }

        if (extra is not null && (best is null || Math.Abs(extra.Offset) < bestDistance || (Math.Abs(extra.Offset) == bestDistance && !bestIsGuide)))
        {
            return new AxisSnap(extra.Offset, IsObject: true, extra);
        }

        if (best is { } offset)
        {
            return new AxisSnap(offset, IsObject: true);
        }

        if (settings.SnapToGrid && double.IsFinite(settings.GridSpacing) && settings.GridSpacing > 0)
        {
            double? bestGrid = null;
            var bestGridDistance = double.MaxValue;
            foreach (var candidate in candidates)
            {
                if (!double.IsFinite(candidate))
                {
                    continue;
                }

                var gridOffset = GridMath.Nearest(candidate, settings.GridSpacing) - candidate;
                if (Math.Abs(gridOffset) <= tolerance && Math.Abs(gridOffset) < bestGridDistance)
                {
                    bestGridDistance = Math.Abs(gridOffset);
                    bestGrid = gridOffset;
                }
            }

            if (bestGrid is { } found)
            {
                return new AxisSnap(found, IsObject: false);
            }
        }

        return null;
    }

    // Every candidate that lands on a target after the offset gets a guide, so a box whose left and right edge both hit targets shows both.
    private static void AddGuides(
        SnapAxis axis,
        IReadOnlyList<double> candidates,
        SnapTargets targets,
        double offset,
        (double From, double To)? dragged,
        List<SnapGuide> guides,
        List<Guid> activeGuides)
    {
        foreach (var candidate in candidates)
        {
            var position = candidate + offset;
            if (!targets.TryFindNearest(axis, position, SamePosition, out var target))
            {
                continue;
            }

            // A ruler guide is drawn by the canvas itself, as active. It gets no guide line.
            if (target.Type == SnapTargetType.Guide)
            {
                if (target.Id is { } id && !activeGuides.Contains(id))
                {
                    activeGuides.Add(id);
                }

                continue;
            }

            if (guides.Exists(g => g.Axis == axis && Math.Abs(g.Position - target.Value) <= SamePosition))
            {
                continue;
            }

            var from = target.From;
            var to = target.To;
            if (dragged is { } extent)
            {
                from = Math.Min(from, extent.From);
                to = Math.Max(to, extent.To);
            }

            guides.Add(new SnapGuide(axis, target.Value, from, to));
        }
    }
}
