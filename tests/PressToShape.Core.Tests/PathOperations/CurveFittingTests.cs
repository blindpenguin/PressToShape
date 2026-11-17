using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Geometry;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.PathOperations;

public class CurveFittingTests
{
    // A circle of radius 50 around the origin as four cubic segments (the first runs from the top to the right).
    private static (SourceCurves Sources, IReadOnlyList<FlatPoint> Points) Circle()
    {
        var path = ShapeConversion.ToPath(new EllipseShape { X = -50, Y = -50, Width = 100, Height = 100 })!;
        var subPath = path.ToDocumentSubPaths()[0];
        var segments = PathShape.BuildSegments(subPath.Nodes, isClosed: true);
        var points = Flattening.Flatten(segments, true, 0, 0, BooleanEngine.Tolerance);
        var sources = new SourceCurves();
        sources.Add(0, 0, segments, points);
        return (sources, points);
    }

    [Fact]
    public void Fit_PolygonAlongOneFlattenedSegment_BecomesOneCubic()
    {
        var (sources, points) = Circle();
        List<(double X, double Y)> polygon = [.. points.Where(p => p.Segment == 0).Select(p => (p.X, p.Y)), (0, 0)];

        var result = CurveFitting.Fit(polygon, sources);

        Assert.True(result.IsClosed);
        Assert.Equal(3, result.Nodes.Count);
        Assert.True(result.Nodes[0].HasOut);
        Assert.False(result.Nodes[0].HasIn);
        Assert.True(result.Nodes[1].HasIn);
        Assert.False(result.Nodes[1].HasOut);
        Assert.False(result.Nodes[2].HasIn || result.Nodes[2].HasOut);
        Assert.Equal((0.0, -50.0), (result.Nodes[0].X, result.Nodes[0].Y));
    }

    [Fact]
    public void Fit_PolygonRunningAgainstTheCurve_StillKeepsTheCurve()
    {
        var (sources, points) = Circle();
        List<(double X, double Y)> polygon = [(0, 0), .. points.Where(p => p.Segment == 0).Select(p => (p.X, p.Y)).Reverse()];

        var result = CurveFitting.Fit(polygon, sources);

        Assert.Equal(3, result.Nodes.Count);
        Assert.Contains(result.Nodes, node => node.HasIn || node.HasOut);
    }

    [Fact]
    public void Fit_ChordAcrossABigArc_IsALine()
    {
        var (sources, points) = Circle();
        var start = points.First(p => p.Segment == 0);
        var end = points.First(p => p.Segment == 1);
        List<(double X, double Y)> polygon = [(start.X, start.Y), (end.X, end.Y), (0, 0)];

        var result = CurveFitting.Fit(polygon, sources);

        Assert.Equal(3, result.Nodes.Count);
        Assert.All(result.Nodes, node => Assert.False(node.HasIn || node.HasOut));
    }

    [Fact]
    public void Fit_PolygonWithoutAnySource_IsLines()
    {
        List<(double X, double Y)> polygon = [(0, 0), (10, 0), (10, 10), (0, 10)];

        var result = CurveFitting.Fit(polygon, new SourceCurves());

        Assert.Equal(4, result.Nodes.Count);
        Assert.All(result.Nodes, node => Assert.False(node.HasIn || node.HasOut));
    }

    [Fact]
    public void Fit_KeptCurveNodes_HaveTheTypeTheirHandlesGive()
    {
        var (sources, points) = Circle();
        List<(double X, double Y)> polygon = [.. points.Where(p => p.Segment <= 1).Select(p => (p.X, p.Y)), (0, 0)];

        var result = CurveFitting.Fit(polygon, sources);

        // The node where the two quarters meet keeps both handles on one line.
        var joint = result.Nodes.Single(node => node.HasIn && node.HasOut);
        Assert.NotEqual(NodeType.Corner, joint.Type);
        Assert.Equal(NodeTypes.Derive(joint), joint.Type);
    }
}
