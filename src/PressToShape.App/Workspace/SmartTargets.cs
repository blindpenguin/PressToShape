using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>A box of an object that smart guides compare the dragged box with, in document units (the bounds of the turned outline).</summary>
internal readonly record struct SmartBox(Guid Id, Bounds Bounds);

/// <summary>
/// A gap between two boxes on one axis: <paramref name="Size"/> is its length, it runs from <paramref name="From"/> to <paramref name="To"/> along
/// the axis, at <paramref name="Cross"/> on the other axis (the middle of the part where the two boxes overlap).
/// </summary>
internal readonly record struct SmartGap(double Size, double From, double To, double Cross);

/// <summary>The width or the height of a box.</summary>
internal readonly record struct SmartSize(double Size, Bounds Box);

/// <summary>
/// What the smart guides compare a drag with, collected once at the press: the boxes of all visible objects that are not part of the drag, the
/// gaps that exist between them, and their widths and heights. Hidden layers, hidden shapes and the members of hidden groups are left out, locked
/// objects count. A group counts by its own box and its members count too. A group that is excluded gives no box of its own, its members are still visited.
/// A gap is measured between a box and its nearest neighbour on a side that overlaps it on the other axis, so there are at most as many gaps as boxes.
/// </summary>
internal sealed class SmartTargets
{
    private readonly SmartGap[] _horizontalGaps;
    private readonly SmartGap[] _verticalGaps;
    private readonly SmartSize[] _widths;
    private readonly SmartSize[] _heights;

    private SmartTargets(List<SmartBox> boxes)
    {
        Boxes = boxes;
        _horizontalGaps = FindGaps(boxes, SnapAxis.X);
        _verticalGaps = FindGaps(boxes, SnapAxis.Y);
        _widths = [.. boxes.Where(b => b.Bounds.Width > 0).Select(b => new SmartSize(b.Bounds.Width, b.Bounds)).OrderBy(s => s.Size)];
        _heights = [.. boxes.Where(b => b.Bounds.Height > 0).Select(b => new SmartSize(b.Bounds.Height, b.Bounds)).OrderBy(s => s.Size)];
    }

    /// <summary>No boxes, gaps or sizes.</summary>
    public static SmartTargets Empty { get; } = new([]);

    /// <summary>The boxes, in document order.</summary>
    public IReadOnlyList<SmartBox> Boxes { get; }

    /// <summary>The gaps between boxes along an axis (X: between boxes side by side), ordered by size.</summary>
    public IReadOnlyList<SmartGap> Gaps(SnapAxis axis) => axis == SnapAxis.X ? _horizontalGaps : _verticalGaps;

    /// <summary>The widths of the boxes (axis X) or the heights (axis Y), ordered by size.</summary>
    public IReadOnlyList<SmartSize> Sizes(SnapAxis axis) => axis == SnapAxis.X ? _widths : _heights;

    /// <summary>Collects the targets of a document.</summary>
    /// <param name="document">The document.</param>
    /// <param name="excludedIds">The shapes and groups that give no box of their own, for example the ones being dragged and the groups around them.</param>
    public static SmartTargets Collect(Document document, IReadOnlySet<Guid> excludedIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(excludedIds);

        var boxes = new List<SmartBox>();
        foreach (var layer in document.Layers)
        {
            if (layer.IsVisible)
            {
                AddShapes(layer.Shapes, excludedIds, boxes);
            }
        }

        return new SmartTargets(boxes);
    }

    /// <summary>
    /// The gaps on the axis whose size is within the tolerance (inclusive) of the size, the nearest size first, at most <paramref name="max"/>.
    /// Nothing for a size or tolerance that is not finite.
    /// </summary>
    public IReadOnlyList<SmartGap> FindGaps(SnapAxis axis, double size, double tolerance, int max)
    {
        if (!double.IsFinite(size) || !double.IsFinite(tolerance) || tolerance < 0 || max <= 0)
        {
            return [];
        }

        var gaps = Gaps(axis);
        var start = LowerBound(gaps, size - tolerance);
        var found = new List<SmartGap>();
        for (var i = start; i < gaps.Count && gaps[i].Size <= size + tolerance; i++)
        {
            found.Add(gaps[i]);
        }

        return [.. found.OrderBy(g => Math.Abs(g.Size - size)).Take(max)];
    }

    /// <summary>
    /// The nearest width (axis X) or height (axis Y) of a box within the tolerance (inclusive) of the size, or <c>null</c>. Of two at the same
    /// distance the smaller one wins.
    /// </summary>
    public SmartSize? FindSize(SnapAxis axis, double size, double tolerance)
    {
        if (!double.IsFinite(size) || !double.IsFinite(tolerance) || tolerance < 0)
        {
            return null;
        }

        var sizes = Sizes(axis);
        var index = LowerBound(sizes, size - tolerance);
        SmartSize? best = null;
        for (var i = index; i < sizes.Count && sizes[i].Size <= size + tolerance; i++)
        {
            if (best is null || Math.Abs(sizes[i].Size - size) < Math.Abs(best.Value.Size - size))
            {
                best = sizes[i];
            }
        }

        return best;
    }

    private static int LowerBound(IReadOnlyList<SmartGap> gaps, double value)
    {
        int low = 0, high = gaps.Count;
        while (low < high)
        {
            var middle = (low + high) / 2;
            if (gaps[middle].Size < value)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    private static int LowerBound(IReadOnlyList<SmartSize> sizes, double value)
    {
        int low = 0, high = sizes.Count;
        while (low < high)
        {
            var middle = (low + high) / 2;
            if (sizes[middle].Size < value)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    private static void AddShapes(IEnumerable<Shape> shapes, IReadOnlySet<Guid> excludedIds, List<SmartBox> boxes)
    {
        foreach (var shape in shapes)
        {
            if (!shape.IsVisible)
            {
                continue;
            }

            if (!excludedIds.Contains(shape.Id))
            {
                var bounds = shape.RotatedBounds;
                if (double.IsFinite(bounds.X) && double.IsFinite(bounds.Y) && double.IsFinite(bounds.Width) && double.IsFinite(bounds.Height))
                {
                    boxes.Add(new SmartBox(shape.Id, bounds));
                }
            }

            if (shape is GroupShape group)
            {
                AddShapes(group.Children, excludedIds, boxes);
            }
        }
    }

    // For every box the nearest box after it on the axis that overlaps it on the other axis. A gap counts only when it is more than 0.
    private static SmartGap[] FindGaps(List<SmartBox> boxes, SnapAxis axis)
    {
        var gaps = new List<SmartGap>();
        for (var i = 0; i < boxes.Count; i++)
        {
            var a = boxes[i].Bounds;
            var aEnd = axis == SnapAxis.X ? a.X + a.Width : a.Y + a.Height;
            SmartGap? nearest = null;
            for (var j = 0; j < boxes.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                var b = boxes[j].Bounds;
                var bStart = axis == SnapAxis.X ? b.X : b.Y;
                var size = bStart - aEnd;
                if (size < 0 || (nearest is { } best && size >= best.Size))
                {
                    continue;
                }

                if (OverlapOnOtherAxis(a, b, axis) is { } cross)
                {
                    nearest = new SmartGap(size, aEnd, bStart, cross);
                }
            }

            // A neighbour that touches the box (gap 0) is the nearest too, and no gap lies behind it.
            if (nearest is { Size: > 0 } gap)
            {
                gaps.Add(gap);
            }
        }

        return [.. gaps.OrderBy(g => g.Size)];
    }

    // The middle of the part where the two boxes overlap on the axis across, or null when they do not overlap there.
    private static double? OverlapOnOtherAxis(Bounds a, Bounds b, SnapAxis axis)
    {
        var (aStart, aEnd, bStart, bEnd) = axis == SnapAxis.X
            ? (a.Y, a.Y + a.Height, b.Y, b.Y + b.Height)
            : (a.X, a.X + a.Width, b.X, b.X + b.Width);
        var start = Math.Max(aStart, bStart);
        var end = Math.Min(aEnd, bEnd);
        return end > start ? (start + end) / 2 : null;
    }
}
