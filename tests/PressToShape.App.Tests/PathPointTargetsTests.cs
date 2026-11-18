using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class PathPointTargetsTests
{
    private static readonly IReadOnlySet<Guid> None = new HashSet<Guid>();

    private static PathShape Line(double x1, double y1, double x2, double y2) => PathShape.Create([new DocumentNode(x1, y1), new DocumentNode(x2, y2)]);

    private static Document With(params Shape[] shapes)
    {
        var document = Document.Create("L");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static PathPointTargets Build(Document document, IReadOnlySet<Guid>? excluded = null, double radius = 1000) =>
        PathPointTargets.Build(document, excluded ?? None, 100, 100, radius);

    private static List<PathPointTarget> OfType(PathPointTargets targets, PathPointType type) => targets.Points.Where(p => p.Type == type).ToList();

    [Fact]
    public void Middle_OfAStraightSegment()
    {
        var targets = Build(With(Line(0, 0, 100, 50)));

        var middle = Assert.Single(OfType(targets, PathPointType.SegmentMiddle));
        Assert.Equal((50.0, 25.0), (middle.X, middle.Y));
    }

    [Fact]
    public void Middle_OfACurve_IsThePointAtHalfTheParameter()
    {
        // A symmetric arch: its middle is at x 100 and above the chord.
        var path = PathShape.Create([new DocumentNode(0, 100, outX: 0, outY: 0), new DocumentNode(200, 100, inX: 200, inY: 0)]);

        var middle = Assert.Single(OfType(Build(With(path)), PathPointType.SegmentMiddle));

        Assert.Equal(100, middle.X, 6);
        Assert.Equal(25, middle.Y, 6);
    }

    [Fact]
    public void Middles_OfAllSegmentsOfAPolyline()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)]);

        var middles = OfType(Build(With(path)), PathPointType.SegmentMiddle);

        Assert.Equal(2, middles.Count);
        Assert.Contains(middles, m => Math.Abs(m.X - 50) < 1e-6 && Math.Abs(m.Y) < 1e-6);
        Assert.Contains(middles, m => Math.Abs(m.X - 100) < 1e-6 && Math.Abs(m.Y - 50) < 1e-6);
    }

    [Fact]
    public void Crossing_OfTwoLines()
    {
        // y = x and y = 300 - 2x cross at (100, 100), which is neither segment's middle.
        var targets = Build(With(Line(0, 0, 300, 300), Line(0, 300, 150, 0)));

        var crossing = Assert.Single(OfType(targets, PathPointType.Crossing));
        Assert.Equal(100, crossing.X, 6);
        Assert.Equal(100, crossing.Y, 6);
    }

    [Fact]
    public void Crossing_OfALineAndACurve()
    {
        var curve = PathShape.Create([new DocumentNode(0, 100, outX: 0, outY: 0), new DocumentNode(200, 100, inX: 200, inY: 0)]);
        var targets = Build(With(curve, Line(60, 0, 60, 200)));

        var crossing = Assert.Single(OfType(targets, PathPointType.Crossing));
        Assert.Equal(60, crossing.X, 3);
        Assert.InRange(crossing.Y, 25, 100);
    }

    [Fact]
    public void Crossing_OfTwoSegmentsOfTheSamePath()
    {
        // The first and the third segment cross at (120, 120).
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(300, 300), new DocumentNode(300, 0), new DocumentNode(0, 200)]);

        var targets = Build(With(path));

        Assert.Contains(OfType(targets, PathPointType.Crossing), c => Math.Abs(c.X - 120) < 1e-3 && Math.Abs(c.Y - 120) < 1e-3);
    }

    [Fact]
    public void NoCrossing_ForParallelTouchingAndNeighbouringSegments()
    {
        var parallel = Build(With(Line(0, 0, 100, 0), Line(0, 10, 100, 10)));
        var neighbours = Build(With(PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)])));
        var touching = Build(With(Line(0, 0, 100, 100), Line(100, 100, 200, 0)));

        Assert.Empty(OfType(parallel, PathPointType.Crossing));
        Assert.Empty(OfType(neighbours, PathPointType.Crossing));
        Assert.Empty(OfType(touching, PathPointType.Crossing));
    }

    [Fact]
    public void RadiusLimit_OnlyNearSegmentsGiveTargets()
    {
        var document = With(Line(0, 0, 40, 0), Line(5000, 5000, 5040, 5000));

        var targets = PathPointTargets.Build(document, None, 20, 0, 100);

        var middle = Assert.Single(targets.Points);
        Assert.Equal(20, middle.X, 6);
    }

    [Fact]
    public void Build_RecordsTheCenterAndRadius()
    {
        var targets = PathPointTargets.Build(With(), None, 12, 34, 56);

        Assert.Equal((12.0, 34.0, 56.0), (targets.CenterX, targets.CenterY, targets.Radius));
    }

    [Fact]
    public void ExcludedPaths_HiddenPathsAndHiddenLayers_GiveNothing()
    {
        var excluded = Line(0, 0, 100, 0);
        var hidden = Line(0, 50, 100, 50) with { IsVisible = false };
        var targets = Build(With(excluded, hidden), new HashSet<Guid> { excluded.Id });

        Assert.Empty(targets.Points);

        var document = With(Line(0, 0, 100, 0));
        document = document.UpdateLayer(document.Layers[0].Id, layer => layer with { IsVisible = false });
        Assert.Empty(Build(document).Points);
    }

    [Fact]
    public void RectanglesAndEllipses_GiveNoTargets()
    {
        var targets = Build(With(new RectangleShape { X = 0, Y = 0, Width = 100, Height = 100 }));

        Assert.Empty(targets.Points);
    }

    [Fact]
    public void PathsInGroups_Count_UnlessTheGroupIsHidden()
    {
        var visible = new GroupShape { Children = [Line(0, 0, 100, 0)] };
        var hidden = new GroupShape { Children = [Line(0, 50, 100, 50)], IsVisible = false };

        var targets = Build(With(visible, hidden));

        Assert.Single(targets.Points);
    }

    [Fact]
    public void RotatedPaths_UseTheirPositionsOnTheScreen()
    {
        var rotated = Line(0, 0, 100, 0) with { Rotation = 90 };

        var middle = Assert.Single(Build(With(rotated)).Points);

        // The path is turned around the middle of its box, so the middle of the segment stays there.
        Assert.Equal(50, middle.X, 6);
        Assert.Equal(0, middle.Y, 6);
    }

    [Fact]
    public void Nearest_WithinTheToleranceOnBothAxes()
    {
        var targets = Build(With(Line(0, 0, 100, 0)));

        Assert.NotNull(targets.Nearest(54, 4, 5));
        Assert.Null(targets.Nearest(54, 6, 5));
        Assert.Null(targets.Nearest(57, 0, 5));
    }

    [Fact]
    public void Nearest_ASegmentMiddleBeatsACrossingAtTheSameDistance()
    {
        // The middle of the first line is at (100, 100), the same place as the crossing with the second line.
        var targets = Build(With(Line(0, 0, 200, 200), Line(0, 200, 200, 0)));

        var nearest = targets.Nearest(102, 100, 5);

        Assert.NotNull(nearest);
        Assert.Equal(PathPointType.SegmentMiddle, nearest!.Value.Type);
    }

    [Fact]
    public void Nearest_TheNearerOfTwo()
    {
        var targets = Build(With(Line(0, 0, 20, 0), Line(0, 0, 14, 0)));

        var nearest = targets.Nearest(8, 0, 5);

        Assert.Equal(7, nearest!.Value.X, 6);
    }

    [Fact]
    public void BadInput_GivesNothing()
    {
        var document = With(Line(0, 0, 100, 0));

        Assert.Empty(PathPointTargets.Build(document, None, double.NaN, 0, 100).Points);
        Assert.Empty(PathPointTargets.Build(document, None, 0, 0, 0).Points);
        Assert.Empty(PathPointTargets.Build(document, None, 0, 0, double.PositiveInfinity).Points);
        var targets = Build(document);
        Assert.Null(targets.Nearest(double.NaN, 0, 5));
        Assert.Null(targets.Nearest(50, 0, -1));
        Assert.Empty(PathPointTargets.Empty.Points);
    }

    [Fact]
    public void ThreeHundredSegments_BuildQuickly()
    {
        var nodes = Enumerable.Range(0, 301).Select(i => new DocumentNode(i * 3 % 200, i * 7 % 200)).ToList();
        var document = With(PathShape.Create(nodes));
        var timer = Stopwatch.StartNew();

        var targets = Build(document, radius: 400);

        Assert.NotEmpty(targets.Points);
        Assert.True(timer.ElapsedMilliseconds < 5000, $"took {timer.ElapsedMilliseconds} ms");
    }

    [Fact]
    public void MoreThanTheLimitOfSegments_StillGivesTheMiddlesButNoCrossings()
    {
        var shapes = Enumerable.Range(0, 450).Select(i => (Shape)Line(i, 0, i + 1, 200)).Concat([Line(0, 100, 500, 100)]).ToArray();

        var targets = Build(With(shapes), radius: 1000);

        Assert.NotEmpty(OfType(targets, PathPointType.SegmentMiddle));
        Assert.Empty(OfType(targets, PathPointType.Crossing));
    }
}
