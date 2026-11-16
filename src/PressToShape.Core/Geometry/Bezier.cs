using System;
using System.Collections.Generic;

namespace PressToShape.Core.Geometry;

/// <summary>The curve math for cubic bezier segments. Bounds, hit tests, display, export and point insertion all use it.</summary>
public static class Bezier
{
    private const int MaxFlattenDepth = 16;
    private const int NearestSamples = 32;
    private const int NearestRefinements = 40;

    /// <summary>The point of the curve at <paramref name="t"/>, from 0 (start) to 1 (end).</summary>
    public static (double X, double Y) Point(Cubic curve, double t)
    {
        var u = 1 - t;
        var a = u * u * u;
        var b = 3 * u * u * t;
        var c = 3 * u * t * t;
        var d = t * t * t;
        return (
            a * curve.P0.X + b * curve.P1.X + c * curve.P2.X + d * curve.P3.X,
            a * curve.P0.Y + b * curve.P1.Y + c * curve.P2.Y + d * curve.P3.Y);
    }

    /// <summary>Splits the curve at <paramref name="t"/> into two curves that together draw the same shape (de Casteljau).</summary>
    public static (Cubic Left, Cubic Right) Split(Cubic curve, double t)
    {
        var p01 = Lerp(curve.P0, curve.P1, t);
        var p12 = Lerp(curve.P1, curve.P2, t);
        var p23 = Lerp(curve.P2, curve.P3, t);
        var p012 = Lerp(p01, p12, t);
        var p123 = Lerp(p12, p23, t);
        var mid = Lerp(p012, p123, t);
        return (new Cubic(curve.P0, p01, p012, mid), new Cubic(mid, p123, p23, curve.P3));
    }

    /// <summary>The tightest box around the curve itself. Handle points that lie off the curve do not count, but the extremes between the end points do.</summary>
    public static Bounds ExtremaBounds(Cubic curve)
    {
        var left = Math.Min(curve.P0.X, curve.P3.X);
        var right = Math.Max(curve.P0.X, curve.P3.X);
        var top = Math.Min(curve.P0.Y, curve.P3.Y);
        var bottom = Math.Max(curve.P0.Y, curve.P3.Y);

        foreach (var t in ExtremeParameters(curve.P0.X, curve.P1.X, curve.P2.X, curve.P3.X))
        {
            var x = Point(curve, t).X;
            left = Math.Min(left, x);
            right = Math.Max(right, x);
        }

        foreach (var t in ExtremeParameters(curve.P0.Y, curve.P1.Y, curve.P2.Y, curve.P3.Y))
        {
            var y = Point(curve, t).Y;
            top = Math.Min(top, y);
            bottom = Math.Max(bottom, y);
        }

        return new Bounds(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Approximates the curve with a polyline whose points are at most about <paramref name="tolerance"/> away from the curve.
    /// The result starts with the start point and ends with the end point.
    /// </summary>
    public static IReadOnlyList<(double X, double Y)> Flatten(Cubic curve, double tolerance)
    {
        var points = new List<(double X, double Y)> { curve.P0 };
        FlattenInto(curve, Math.Max(tolerance, 1e-9), points, 0);
        return points;
    }

    /// <summary>The parameter of the point of the curve that is closest to the given point.</summary>
    public static double NearestParameter(Cubic curve, double x, double y)
    {
        var best = 0.0;
        var bestDistance = double.PositiveInfinity;
        for (var i = 0; i <= NearestSamples; i++)
        {
            var t = (double)i / NearestSamples;
            var distance = DistanceSquared(Point(curve, t), x, y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = t;
            }
        }

        // Narrow down inside the two sample steps around the best sample (ternary search, the distance is smooth there).
        var low = Math.Max(0, best - 1.0 / NearestSamples);
        var high = Math.Min(1, best + 1.0 / NearestSamples);
        for (var i = 0; i < NearestRefinements; i++)
        {
            var first = low + (high - low) / 3;
            var second = high - (high - low) / 3;
            if (DistanceSquared(Point(curve, first), x, y) <= DistanceSquared(Point(curve, second), x, y))
            {
                high = second;
            }
            else
            {
                low = first;
            }
        }

        return (low + high) / 2;
    }

    // The parameters strictly between 0 and 1 where the derivative of one coordinate is zero.
    private static IEnumerable<double> ExtremeParameters(double p0, double p1, double p2, double p3)
    {
        // The derivative is a quadratic: a t^2 + b t + c.
        var a = 3 * (-p0 + 3 * p1 - 3 * p2 + p3);
        var b = 6 * (p0 - 2 * p1 + p2);
        var c = 3 * (p1 - p0);

        if (Math.Abs(a) < 1e-12)
        {
            if (Math.Abs(b) >= 1e-12)
            {
                var t = -c / b;
                if (t > 0 && t < 1)
                {
                    yield return t;
                }
            }

            yield break;
        }

        var discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            yield break;
        }

        var root = Math.Sqrt(discriminant);
        var t1 = (-b + root) / (2 * a);
        var t2 = (-b - root) / (2 * a);
        if (t1 > 0 && t1 < 1)
        {
            yield return t1;
        }

        if (t2 > 0 && t2 < 1)
        {
            yield return t2;
        }
    }

    private static void FlattenInto(Cubic curve, double tolerance, List<(double X, double Y)> points, int depth)
    {
        if (depth >= MaxFlattenDepth || IsFlat(curve, tolerance))
        {
            points.Add(curve.P3);
            return;
        }

        var (left, right) = Split(curve, 0.5);
        FlattenInto(left, tolerance, points, depth + 1);
        FlattenInto(right, tolerance, points, depth + 1);
    }

    // Flat enough when both handle points are within the tolerance of the chord (or of the start point if the chord has no length).
    private static bool IsFlat(Cubic curve, double tolerance) =>
        DistanceToChord(curve.P1, curve.P0, curve.P3) <= tolerance && DistanceToChord(curve.P2, curve.P0, curve.P3) <= tolerance;

    private static double DistanceToChord((double X, double Y) point, (double X, double Y) start, (double X, double Y) end)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var lengthSquared = dx * dx + dy * dy;
        if (lengthSquared == 0)
        {
            return Math.Sqrt(DistanceSquared(point, start.X, start.Y));
        }

        // Distance to the infinite line, which is what bounds the curve's deviation from the chord.
        return Math.Abs((point.X - start.X) * dy - (point.Y - start.Y) * dx) / Math.Sqrt(lengthSquared);
    }

    private static double DistanceSquared((double X, double Y) point, double x, double y)
    {
        var dx = point.X - x;
        var dy = point.Y - y;
        return dx * dx + dy * dy;
    }

    private static (double X, double Y) Lerp((double X, double Y) a, (double X, double Y) b, double t) =>
        (a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
}
