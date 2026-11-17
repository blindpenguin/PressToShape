using System;
using System.Collections.Generic;

namespace PressToShape.Core.Geometry;

/// <summary>Turns the cubic segments of a sub-path into a polyline whose points know where on which segment they lie. Deterministic.</summary>
public static class Flattening
{
    // No segment is cut into more pieces than this, however tight the tolerance is.
    private const int MaximumPieces = 2000;

    /// <summary>
    /// The points of one sub-path as a polyline. A straight segment (a cubic whose handle points lie on its end points) gives only its
    /// start point. A curve is cut into equal steps of <c>t</c>, as many as keep the curve within <paramref name="tolerance"/> of the
    /// polyline. Every segment gives its points from <c>t = 0</c> up to, but not including, the end, because the end is the start of the
    /// next segment. Only the last segment of an open sub-path also gives its end point, so a closed sub-path does not repeat its first point.
    /// </summary>
    /// <param name="segments">The segments of the sub-path in drawing order (for a closed sub-path including the closing one).</param>
    /// <param name="isClosed">Whether the sub-path is closed.</param>
    /// <param name="operand">The operand index to put in the tags.</param>
    /// <param name="firstSegment">The index of the first segment of the sub-path among all segments of the operand, to put in the tags.</param>
    /// <param name="tolerance">How far the polyline may be from the curve, in document units, greater than 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tolerance is not greater than 0 and finite.</exception>
    /// <exception cref="ArgumentException">A number of a segment is not finite.</exception>
    public static IReadOnlyList<FlatPoint> Flatten(IReadOnlyList<Cubic> segments, bool isClosed, int operand, int firstSegment, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(segments);
        if (!(tolerance > 0) || !double.IsFinite(tolerance))
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "The tolerance must be greater than 0.");
        }

        var points = new List<FlatPoint>(segments.Count);
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            CheckFinite(segment);
            var pieces = IsLine(segment) ? 1 : Pieces(segment, tolerance);
            for (var k = 0; k < pieces; k++)
            {
                var t = (double)k / pieces;
                var point = k == 0 ? segment.P0 : Bezier.Point(segment, t);
                points.Add(new FlatPoint(point.X, point.Y, operand, firstSegment + i, t));
            }

            if (!isClosed && i == segments.Count - 1)
            {
                points.Add(new FlatPoint(segment.P3.X, segment.P3.Y, operand, firstSegment + i, 1));
            }
        }

        return points;
    }

    // A cubic whose handle points lie on its end points is a straight line.
    private static bool IsLine(Cubic segment) => segment.P1 == segment.P0 && segment.P2 == segment.P3;

    // The distance between a curve and its chord over a step of t is at most M / 8 * step², with M the largest second derivative.
    private static int Pieces(Cubic segment, double tolerance)
    {
        var firstX = segment.P0.X - 2 * segment.P1.X + segment.P2.X;
        var firstY = segment.P0.Y - 2 * segment.P1.Y + segment.P2.Y;
        var secondX = segment.P1.X - 2 * segment.P2.X + segment.P3.X;
        var secondY = segment.P1.Y - 2 * segment.P2.Y + segment.P3.Y;
        var second = 6 * Math.Max(Math.Sqrt(firstX * firstX + firstY * firstY), Math.Sqrt(secondX * secondX + secondY * secondY));
        var pieces = Math.Ceiling(Math.Sqrt(second / (8 * tolerance)));
        return (int)Math.Clamp(pieces, 1, MaximumPieces);
    }

    private static void CheckFinite(Cubic segment)
    {
        if (!double.IsFinite(segment.P0.X + segment.P0.Y + segment.P1.X + segment.P1.Y + segment.P2.X + segment.P2.Y + segment.P3.X + segment.P3.Y))
        {
            throw new ArgumentException("A segment has a number that is not finite.", nameof(segment));
        }
    }
}
