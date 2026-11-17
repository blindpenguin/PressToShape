using System;
using System.Linq;
using PressToShape.Core.Geometry;
using Xunit;

namespace PressToShape.Core.Tests.Geometry;

public class FlatteningTests
{
    private const double Circle = 0.5522847498307936;

    private static Cubic Line((double X, double Y) from, (double X, double Y) to) => new(from, from, to, to);

    // The quarter of a circle of radius 100 from (100, 0) to (0, 100) around the origin.
    private static Cubic Quarter() => new((100, 0), (100, 100 * Circle), (100 * Circle, 100), (0, 100));

    [Fact]
    public void Flatten_OpenLine_GivesStartAndEndPoint()
    {
        var points = Flattening.Flatten([Line((0, 0), (10, 0))], isClosed: false, operand: 3, firstSegment: 5, tolerance: 0.1);

        Assert.Equal(2, points.Count);
        Assert.Equal(new FlatPoint(0, 0, 3, 5, 0), points[0]);
        Assert.Equal(new FlatPoint(10, 0, 3, 5, 1), points[1]);
    }

    [Fact]
    public void Flatten_ClosedSquare_GivesFourPointsWithoutRepeatingTheFirst()
    {
        var segments = new[]
        {
            Line((0, 0), (10, 0)),
            Line((10, 0), (10, 10)),
            Line((10, 10), (0, 10)),
            Line((0, 10), (0, 0)),
        };

        var points = Flattening.Flatten(segments, isClosed: true, operand: 0, firstSegment: 0, tolerance: 0.1);

        Assert.Equal([(0.0, 0.0), (10, 0), (10, 10), (0, 10)], points.Select(p => (p.X, p.Y)));
        Assert.Equal([0, 1, 2, 3], points.Select(p => p.Segment));
        Assert.All(points, p => Assert.Equal(0, p.T));
    }

    [Fact]
    public void Flatten_Curve_ChordsStayWithinTheTolerance()
    {
        const double tolerance = 0.1;
        var curve = Quarter();

        var points = Flattening.Flatten([curve], isClosed: false, operand: 0, firstSegment: 0, tolerance: tolerance);

        Assert.True(points.Count > 3);
        for (var i = 0; i + 1 < points.Count; i++)
        {
            var middle = Bezier.Point(curve, (points[i].T + points[i + 1].T) / 2);
            var chordX = (points[i].X + points[i + 1].X) / 2;
            var chordY = (points[i].Y + points[i + 1].Y) / 2;
            Assert.True(Math.Sqrt(Math.Pow(middle.X - chordX, 2) + Math.Pow(middle.Y - chordY, 2)) <= tolerance);
        }
    }

    [Fact]
    public void Flatten_Curve_TagsGrowFromZeroToOneAndPointsLieOnTheCurve()
    {
        var curve = Quarter();

        var points = Flattening.Flatten([curve], isClosed: false, operand: 0, firstSegment: 0, tolerance: 0.1);

        Assert.Equal(0, points[0].T);
        Assert.Equal(1, points[^1].T);
        Assert.True(points.Zip(points.Skip(1), (a, b) => a.T < b.T).All(ok => ok));
        foreach (var point in points)
        {
            var expected = Bezier.Point(curve, point.T);
            Assert.Equal(expected.X, point.X, 9);
            Assert.Equal(expected.Y, point.Y, 9);
        }
    }

    [Fact]
    public void Flatten_TighterTolerance_GivesMorePoints()
    {
        var coarse = Flattening.Flatten([Quarter()], isClosed: false, operand: 0, firstSegment: 0, tolerance: 1);
        var fine = Flattening.Flatten([Quarter()], isClosed: false, operand: 0, firstSegment: 0, tolerance: 0.01);

        Assert.True(fine.Count > coarse.Count);
    }

    [Fact]
    public void Flatten_SameInputTwice_GivesEqualOutput()
    {
        var first = Flattening.Flatten([Quarter()], isClosed: false, operand: 1, firstSegment: 2, tolerance: 0.1);
        var second = Flattening.Flatten([Quarter()], isClosed: false, operand: 1, firstSegment: 2, tolerance: 0.1);

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Flatten_BadTolerance_Throws(double tolerance)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Flattening.Flatten([Quarter()], false, 0, 0, tolerance));
    }

    [Fact]
    public void Flatten_SegmentWithANumberThatIsNotFinite_Throws()
    {
        var bad = Line((0, 0), (double.NaN, 1));

        Assert.Throws<ArgumentException>(() => Flattening.Flatten([bad], false, 0, 0, 0.1));
    }
}
