using System;
using System.Linq;
using PressToShape.Core.Geometry;
using Xunit;

namespace PressToShape.Core.Tests.Geometry;

public class BezierTests
{
    // A curve that rises above both end points and comes back down.
    private static readonly Cubic Arch = new((0, 0), (0, -100), (100, -100), (100, 0));

    private static Cubic Line((double X, double Y) from, (double X, double Y) to) => new(from, from, to, to);

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 100, 0)]
    public void Point_AtTheEnds_ReturnsTheEndPoints(double t, double x, double y)
    {
        var point = Bezier.Point(Arch, t);

        Assert.Equal(x, point.X, 9);
        Assert.Equal(y, point.Y, 9);
    }

    [Fact]
    public void Point_AtHalf_ReturnsTheKnownMiddle()
    {
        var point = Bezier.Point(Arch, 0.5);

        Assert.Equal(50, point.X, 9);
        Assert.Equal(-75, point.Y, 9);
    }

    [Theory]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.8)]
    public void Split_AnyParameter_PiecesDrawTheOriginalCurve(double t)
    {
        var (left, right) = Bezier.Split(Arch, t);

        for (var i = 0; i <= 10; i++)
        {
            var local = i / 10.0;
            var onLeft = Bezier.Point(left, local);
            var expectedLeft = Bezier.Point(Arch, local * t);
            Assert.Equal(expectedLeft.X, onLeft.X, 9);
            Assert.Equal(expectedLeft.Y, onLeft.Y, 9);

            var onRight = Bezier.Point(right, local);
            var expectedRight = Bezier.Point(Arch, t + local * (1 - t));
            Assert.Equal(expectedRight.X, onRight.X, 9);
            Assert.Equal(expectedRight.Y, onRight.Y, 9);
        }
    }

    [Fact]
    public void Split_AnyParameter_PiecesMeetAtTheSplitPoint()
    {
        var (left, right) = Bezier.Split(Arch, 0.3);

        Assert.Equal(left.P3, right.P0);
    }

    [Fact]
    public void ExtremaBounds_CurveThatOvershootsItsEnds_ReachesTheExtremeNotTheHandles()
    {
        var bounds = Bezier.ExtremaBounds(Arch);

        Assert.Equal(0, bounds.X, 9);
        Assert.Equal(100, bounds.Width, 9);
        // The curve peaks at y = -75, the handle points are at -100 and must not count.
        Assert.Equal(-75, bounds.Y, 9);
        Assert.Equal(75, bounds.Height, 9);
    }

    [Fact]
    public void ExtremaBounds_SCurve_FindsBothExtremes()
    {
        var curve = new Cubic((0, 0), (-100, 30), (200, 70), (100, 100));

        var bounds = Bezier.ExtremaBounds(curve);

        Assert.True(bounds.X < 0, "the curve leaves the box of its end points to the left");
        Assert.True(bounds.X + bounds.Width > 100, "and to the right");
        Assert.Equal(0, bounds.Y, 9);
        Assert.Equal(100, bounds.Height, 9);
    }

    [Fact]
    public void ExtremaBounds_Line_IsTheBoxOfTheEndPoints()
    {
        var bounds = Bezier.ExtremaBounds(Line((10, 20), (40, 5)));

        Assert.Equal(new Bounds(10, 5, 30, 15), bounds);
    }

    [Fact]
    public void ExtremaBounds_AllPointsEqual_HasZeroSize()
    {
        var bounds = Bezier.ExtremaBounds(Line((7, 7), (7, 7)));

        Assert.Equal(new Bounds(7, 7, 0, 0), bounds);
    }

    [Fact]
    public void Flatten_Line_ReturnsOnlyTheEndPoints()
    {
        var points = Bezier.Flatten(Line((0, 0), (10, 10)), 0.1);

        Assert.Equal([(0.0, 0.0), (10.0, 10.0)], points);
    }

    [Fact]
    public void Flatten_Curve_StaysWithinTheToleranceOfTheCurve()
    {
        const double tolerance = 0.25;

        var points = Bezier.Flatten(Arch, tolerance);

        Assert.Equal(Arch.P0, points[0]);
        Assert.Equal(Arch.P3, points[^1]);
        Assert.True(points.Count > 3);
        // Every sample of the curve must lie close to the polyline.
        for (var i = 0; i <= 100; i++)
        {
            var sample = Bezier.Point(Arch, i / 100.0);
            Assert.True(DistanceToPolyline(points.ToList(), sample) <= tolerance * 1.01, $"sample {i} is too far from the polyline");
        }
    }

    [Fact]
    public void Flatten_DegenerateCurve_DoesNotProduceNaN()
    {
        var points = Bezier.Flatten(Line((3, 3), (3, 3)), 0.1);

        Assert.All(points, p => Assert.True(double.IsFinite(p.X) && double.IsFinite(p.Y)));
    }

    [Fact]
    public void NearestParameter_PointBesideALine_ReturnsTheParameterOfItsFoot()
    {
        var line = Line((0, 0), (100, 0));

        var t = Bezier.NearestParameter(line, 25, 40);

        // A line with its handles on its end points is not evenly spaced in t, so compare the position, not t.
        Assert.Equal(25, Bezier.Point(line, t).X, 3);
    }

    [Fact]
    public void NearestParameter_PointOnTheCurve_ReturnsItsParameter()
    {
        var onCurve = Bezier.Point(Arch, 0.7);

        var t = Bezier.NearestParameter(Arch, onCurve.X, onCurve.Y);

        Assert.Equal(0.7, t, 3);
    }

    [Fact]
    public void NearestParameter_PointBeyondTheEnd_ClampsToTheEnd()
    {
        var t = Bezier.NearestParameter(Line((0, 0), (100, 0)), 500, 0);

        Assert.Equal(1, t, 6);
    }

    private static double DistanceToPolyline(System.Collections.Generic.IReadOnlyList<(double X, double Y)> line, (double X, double Y) p)
    {
        var best = double.PositiveInfinity;
        for (var i = 0; i + 1 < line.Count; i++)
        {
            var (ax, ay) = line[i];
            var (bx, by) = line[i + 1];
            var dx = bx - ax;
            var dy = by - ay;
            var lengthSquared = dx * dx + dy * dy;
            var t = lengthSquared == 0 ? 0 : Math.Clamp(((p.X - ax) * dx + (p.Y - ay) * dy) / lengthSquared, 0, 1);
            var distance = Math.Sqrt(Math.Pow(p.X - (ax + t * dx), 2) + Math.Pow(p.Y - (ay + t * dy), 2));
            best = Math.Min(best, distance);
        }

        return best;
    }
}
