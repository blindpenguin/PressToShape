using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SnapperTests
{
    private static readonly SnapSettings GridOnly = new(SnapToGrid: true, SnapToObjects: false, GridSpacing: 10);
    private static readonly SnapSettings ObjectsOnly = new(SnapToGrid: false, SnapToObjects: true, GridSpacing: 10);
    private static readonly SnapSettings Both = new(SnapToGrid: true, SnapToObjects: true, GridSpacing: 10);
    private static readonly SnapSettings Off = new(SnapToGrid: false, SnapToObjects: false, GridSpacing: 10);

    // One target square from x 100..130, y 200..230 in an otherwise empty document.
    private static SnapTargets Targets()
    {
        var document = Document.Create("Layer");
        var square = new RectangleShape { X = 100, Y = 200, Width = 30, Height = 30 };
        return SnapTargets.Collect(document.AddShape(document.Layers[0].Id, square), new System.Collections.Generic.HashSet<System.Guid>());
    }

    private static SnapResult Snap(System.Collections.Generic.IReadOnlyList<double> x, System.Collections.Generic.IReadOnlyList<double> y, SnapTargets? targets, SnapSettings settings, double zoom = 100, Bounds? dragged = null) =>
        Snapper.Snap(x, y, targets, settings, zoom, dragged);

    [Fact]
    public void Snap_GridOn_PointNearALineMovesOntoIt()
    {
        var result = Snap([23], [48], null, GridOnly);

        Assert.Equal(-3, result.OffsetX, 9);
        Assert.Equal(2, result.OffsetY, 9);
        Assert.Empty(result.Guides);
    }

    [Fact]
    public void Snap_GridOn_PointOutsideToleranceStays()
    {
        var result = Snap([26], [44], null, new SnapSettings(true, false, 100));

        Assert.Equal(0, result.OffsetX);
        Assert.Equal(0, result.OffsetY);
    }

    [Theory]
    [InlineData(100, 8, true)]
    [InlineData(100, 8.5, false)]
    [InlineData(50, 16, true)]
    [InlineData(50, 17, false)]
    [InlineData(400, 2, true)]
    [InlineData(400, 2.5, false)]
    public void Snap_ToleranceIsEightScreenPixelsAtAnyZoom(double zoom, double distanceInDocumentUnits, bool snaps)
    {
        var result = Snap([100 + distanceInDocumentUnits], [0], null, new SnapSettings(true, false, 100), zoom);

        Assert.Equal(snaps, result.OffsetX != 0);
    }

    [Fact]
    public void Snap_BothOff_ChangesNothing()
    {
        var result = Snap([101], [201], Targets(), Off);

        Assert.Same(SnapResult.None, result);
    }

    [Fact]
    public void Snap_ObjectsOn_SnapsToEdgeAndShowsGuide()
    {
        var result = Snap([95], [], Targets(), ObjectsOnly);

        Assert.Equal(5, result.OffsetX, 9);
        var guide = Assert.Single(result.Guides);
        Assert.Equal(SnapAxis.X, guide.Axis);
        Assert.Equal(100, guide.Position, 9);
    }

    [Fact]
    public void Snap_ObjectTargetBeatsANearerGridLine()
    {
        // The grid line at 100 is 1 away from 99, the object edge at 105 is 6 away, but the object wins.
        var document = Document.Create("Layer");
        var shape = new RectangleShape { X = 105, Y = 300, Width = 30, Height = 30 };
        var targets = SnapTargets.Collect(document.AddShape(document.Layers[0].Id, shape), new System.Collections.Generic.HashSet<System.Guid>());

        var result = Snap([99], [], targets, Both);

        Assert.Equal(6, result.OffsetX, 9);
        Assert.Single(result.Guides);
    }

    [Fact]
    public void Snap_ObjectsOnButNoneInRange_FallsBackToTheGrid()
    {
        var result = Snap([43], [], Targets(), Both);

        Assert.Equal(-3, result.OffsetX, 9);
        Assert.Empty(result.Guides);
    }

    [Fact]
    public void Snap_AxesAreIndependent()
    {
        var result = Snap([96], [47], Targets(), Both);

        Assert.Equal(4, result.OffsetX, 9);
        Assert.Equal(3, result.OffsetY, 9);
    }

    [Fact]
    public void Snap_SeveralCandidates_TheSmallestDistanceDecides()
    {
        // Left edge 94 is 6 from the target at 100, the center 110 is 5 from 115, the right edge 126 is 4 from 130.
        var result = Snap(Snapper.CandidatesX(new Bounds(94, 0, 32, 10)), [], Targets(), ObjectsOnly);

        Assert.Equal(4, result.OffsetX, 9);
    }

    [Fact]
    public void Snap_BoxAlignedWithATarget_ShowsAGuideForEveryAlignedCandidate()
    {
        var result = Snap(Snapper.CandidatesX(new Bounds(103, 0, 30, 10)), [], Targets(), ObjectsOnly);

        Assert.Equal(-3, result.OffsetX, 9);
        Assert.Equal(1, result.Guides.Count(g => g.Position == 100));
        Assert.Equal(1, result.Guides.Count(g => g.Position == 115));
        Assert.Equal(1, result.Guides.Count(g => g.Position == 130));
    }

    [Fact]
    public void Snap_GuideReachesFromTheTargetToTheDraggedBox()
    {
        var result = Snap([96], [], Targets(), ObjectsOnly, dragged: new Bounds(96, 20, 10, 10));

        var guide = Assert.Single(result.Guides);
        Assert.Equal(20, guide.From, 9);
        Assert.Equal(230, guide.To, 9);
    }

    [Fact]
    public void Snap_GuideKeepsTheTargetExtentWithoutDraggedBounds()
    {
        var guide = Assert.Single(Snap([96], [], Targets(), ObjectsOnly).Guides);

        Assert.Equal(200, guide.From, 9);
        Assert.Equal(230, guide.To, 9);
    }

    [Fact]
    public void Snap_ArtboardEdges_AreTargets()
    {
        var targets = SnapTargets.Collect(Document.Create("Layer"), new System.Collections.Generic.HashSet<System.Guid>());

        var result = Snap([4], [0], targets, ObjectsOnly);

        Assert.Equal(-4, result.OffsetX, 9);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Snap_NonFiniteCandidates_NeverGiveNaNOrInfinity(double bad)
    {
        var result = Snap([bad, 96], [bad], Targets(), Both);

        Assert.True(double.IsFinite(result.OffsetX));
        Assert.True(double.IsFinite(result.OffsetY));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Snap_UnusableZoom_GivesNoSnap(double zoom)
    {
        Assert.Same(SnapResult.None, Snap([96], [47], Targets(), Both, zoom));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(double.NaN)]
    public void Snap_UnusableSpacing_SkipsTheGrid(double spacing)
    {
        var result = Snap([23], [48], null, new SnapSettings(true, false, spacing));

        Assert.Same(SnapResult.None, result);
    }

    [Fact]
    public void Snap_ZeroSizeBox_Works()
    {
        var result = Snap(Snapper.CandidatesX(new Bounds(96, 0, 0, 0)), Snapper.CandidatesY(new Bounds(0, 0, 0, 0)), Targets(), Both);

        Assert.Equal(4, result.OffsetX, 9);
    }

    [Fact]
    public void Candidates_GiveEdgesAndCenter()
    {
        var bounds = new Bounds(10, 20, 30, 40);

        Assert.Equal([10, 25, 40], Snapper.CandidatesX(bounds));
        Assert.Equal([20, 40, 60], Snapper.CandidatesY(bounds));
    }

    [Fact]
    public void SnapSettingsFrom_ReadsThePreferences()
    {
        var preferences = new SnapPreferences(new MemorySettingsStore(), persist: false) { SnapToGrid = true, GridSpacing = 25 };

        var settings = SnapSettings.From(preferences);

        Assert.Equal(new SnapSettings(true, false, 25), settings);
    }
}
