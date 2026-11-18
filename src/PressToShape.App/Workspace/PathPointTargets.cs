using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>What a path point target is.</summary>
internal enum PathPointType
{
    /// <summary>The middle of a segment (the point of a curve at half of its parameter).</summary>
    SegmentMiddle,

    /// <summary>The point where two segments cross.</summary>
    Crossing,
}

/// <summary>A point on the screen of the drawing that a Pen point or a node can snap to, in document units.</summary>
internal readonly record struct PathPointTarget(double X, double Y, PathPointType Type);

/// <summary>
/// The segment middles and the crossings of segments of the visible paths that are not excluded, for the Pen and the node tools. Only segments
/// near a center (within <see cref="Radius"/> of it, in document units) are used, so the work stays small on a big drawing: the caller builds the
/// targets again when the pointer has moved far from the center. The rotation of a path is applied. Crossings are found among the near segments,
/// also between segments of the same path; the place where two neighbouring segments meet is a node, not a crossing. Pure code.
/// </summary>
internal sealed class PathPointTargets
{
    // A curve is replaced by a polyline for the crossings, at most this far from the curve (document units).
    private const double FlattenTolerance = 0.1;

    // Crossings are only searched among this many near segments, so a dense area cannot make the build slow.
    private const int MaxSegmentsForCrossings = 400;

    // Two points closer than this are the same point.
    private const double SamePoint = 1e-6;

    private readonly List<PathPointTarget> _points;

    private PathPointTargets(double centerX, double centerY, double radius, List<PathPointTarget> points)
    {
        CenterX = centerX;
        CenterY = centerY;
        Radius = radius;
        _points = points;
    }

    /// <summary>No targets.</summary>
    public static PathPointTargets Empty { get; } = new(0, 0, 0, []);

    /// <summary>The x of the center the targets were built for.</summary>
    public double CenterX { get; }

    /// <summary>The y of the center the targets were built for.</summary>
    public double CenterY { get; }

    /// <summary>How far from the center segments were used.</summary>
    public double Radius { get; }

    /// <summary>The targets.</summary>
    public IReadOnlyList<PathPointTarget> Points => _points;

    /// <summary>Builds the targets of the segments within the radius of the center.</summary>
    /// <param name="document">The document.</param>
    /// <param name="excludedIds">The paths that are edited or continued and the groups around them. Their segments give no target.</param>
    /// <param name="centerX">The x of the center, in document units.</param>
    /// <param name="centerY">The y of the center, in document units.</param>
    /// <param name="radius">How far from the center segments are used, in document units.</param>
    public static PathPointTargets Build(Document document, IReadOnlySet<Guid> excludedIds, double centerX, double centerY, double radius)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(excludedIds);

        if (!double.IsFinite(centerX) || !double.IsFinite(centerY) || !double.IsFinite(radius) || radius <= 0)
        {
            return new PathPointTargets(centerX, centerY, radius, []);
        }

        var near = new List<NearSegment>();
        foreach (var layer in document.Layers)
        {
            if (layer.IsVisible)
            {
                CollectSegments(layer.Shapes, excludedIds, centerX, centerY, radius, near);
            }
        }

        var points = new List<PathPointTarget>();
        foreach (var segment in near)
        {
            var (x, y) = Bezier.Point(segment.Curve, 0.5);
            if (Math.Abs(x - centerX) <= radius && Math.Abs(y - centerY) <= radius)
            {
                AddPoint(points, new PathPointTarget(x, y, PathPointType.SegmentMiddle));
            }
        }

        if (near.Count <= MaxSegmentsForCrossings)
        {
            AddCrossings(near, centerX, centerY, radius, points);
        }

        return new PathPointTargets(centerX, centerY, radius, points);
    }

    /// <summary>
    /// The target that is within the tolerance (inclusive) of the point on both axes and nearest to it, or <c>null</c>. A segment middle wins
    /// over a crossing at the same distance. Nothing for a point or tolerance that is not finite.
    /// </summary>
    public PathPointTarget? Nearest(double x, double y, double tolerance)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(tolerance) || tolerance < 0)
        {
            return null;
        }

        PathPointTarget? best = null;
        var bestDistance = double.MaxValue;
        foreach (var point in _points)
        {
            if (Math.Abs(point.X - x) > tolerance || Math.Abs(point.Y - y) > tolerance)
            {
                continue;
            }

            var distance = Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
            var closer = distance < bestDistance - SamePoint;
            var sameButBetterType = Math.Abs(distance - bestDistance) <= SamePoint && best is { Type: PathPointType.Crossing } && point.Type == PathPointType.SegmentMiddle;
            if (best is null || closer || sameButBetterType)
            {
                best = point;
                bestDistance = distance;
            }
        }

        return best;
    }

    private sealed record NearSegment(Cubic Curve, Bounds Box);

    private static void CollectSegments(
        IEnumerable<Shape> shapes,
        IReadOnlySet<Guid> excludedIds,
        double centerX,
        double centerY,
        double radius,
        List<NearSegment> near)
    {
        foreach (var shape in shapes)
        {
            if (!shape.IsVisible)
            {
                continue;
            }

            if (shape is GroupShape group)
            {
                CollectSegments(group.Children, excludedIds, centerX, centerY, radius, near);
                continue;
            }

            if (shape is not PathShape path || excludedIds.Contains(path.Id))
            {
                continue;
            }

            foreach (var curve in path.WithRotationApplied().Segments())
            {
                var box = Bezier.ExtremaBounds(curve);
                if (box.X <= centerX + radius && box.X + box.Width >= centerX - radius && box.Y <= centerY + radius && box.Y + box.Height >= centerY - radius)
                {
                    near.Add(new NearSegment(curve, box));
                }
            }
        }
    }

    private static void AddCrossings(List<NearSegment> near, double centerX, double centerY, double radius, List<PathPointTarget> points)
    {
        var polylines = new IReadOnlyList<(double X, double Y)>?[near.Count];
        for (var i = 0; i < near.Count; i++)
        {
            for (var j = i + 1; j < near.Count; j++)
            {
                if (!Overlap(near[i].Box, near[j].Box))
                {
                    continue;
                }

                var first = polylines[i] ??= Bezier.Flatten(near[i].Curve, FlattenTolerance);
                var second = polylines[j] ??= Bezier.Flatten(near[j].Curve, FlattenTolerance);
                foreach (var point in Crossings(first, second))
                {
                    if (Math.Abs(point.X - centerX) <= radius && Math.Abs(point.Y - centerY) <= radius
                        && !IsEnd(near[i].Curve, point) && !IsEnd(near[j].Curve, point))
                    {
                        AddPoint(points, new PathPointTarget(point.X, point.Y, PathPointType.Crossing));
                    }
                }
            }
        }
    }

    private static bool Overlap(Bounds a, Bounds b) =>
        a.X <= b.X + b.Width && b.X <= a.X + a.Width && a.Y <= b.Y + b.Height && b.Y <= a.Y + a.Height;

    // Where two polylines cross: every pair of their lines that intersect, the end points included (the caller drops the ends).
    private static IEnumerable<(double X, double Y)> Crossings(IReadOnlyList<(double X, double Y)> first, IReadOnlyList<(double X, double Y)> second)
    {
        for (var i = 0; i + 1 < first.Count; i++)
        {
            for (var j = 0; j + 1 < second.Count; j++)
            {
                if (Intersect(first[i], first[i + 1], second[j], second[j + 1]) is { } point)
                {
                    yield return point;
                }
            }
        }
    }

    private static (double X, double Y)? Intersect((double X, double Y) p, (double X, double Y) p2, (double X, double Y) q, (double X, double Y) q2)
    {
        var rx = p2.X - p.X;
        var ry = p2.Y - p.Y;
        var sx = q2.X - q.X;
        var sy = q2.Y - q.Y;
        var denominator = rx * sy - ry * sx;
        if (Math.Abs(denominator) < 1e-12)
        {
            return null;
        }

        var t = ((q.X - p.X) * sy - (q.Y - p.Y) * sx) / denominator;
        var u = ((q.X - p.X) * ry - (q.Y - p.Y) * rx) / denominator;
        return t is >= 0 and <= 1 && u is >= 0 and <= 1 ? (p.X + t * rx, p.Y + t * ry) : null;
    }

    // A point at the start or the end of a segment is a node of the path, not a crossing.
    private static bool IsEnd(Cubic curve, (double X, double Y) point) =>
        Distance(curve.P0, point) < 1e-3 || Distance(curve.P3, point) < 1e-3;

    private static double Distance((double X, double Y) a, (double X, double Y) b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

    private static void AddPoint(List<PathPointTarget> points, PathPointTarget point)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y))
        {
            return;
        }

        if (!points.Any(p => Math.Abs(p.X - point.X) < SamePoint && Math.Abs(p.Y - point.Y) < SamePoint))
        {
            points.Add(point);
        }
    }
}
