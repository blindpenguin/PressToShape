using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SmartSnapSpacingTests
{
    private static SmartTargets Targets(params (double X, double Y, double W, double H)[] boxes)
    {
        var document = Document.Create("L");
        var shapes = boxes.Select(b => (Shape)new RectangleShape { X = b.X, Y = b.Y, Width = b.W, Height = b.H });
        return SmartTargets.Collect(document.AddShapes(document.Layers[0].Id, shapes), new HashSet<Guid>());
    }

    // Two boxes 10 wide with a gap of 30 between them (x 0..10 and 40..50), in a row at y 0..10.
    private static readonly (double, double, double, double)[] Row = [(0, 0, 10, 10), (40, 0, 10, 10)];

    private static SnapCandidate? Only(IReadOnlyList<SnapCandidate> candidates, SnapAxis axis) => candidates.SingleOrDefault(c => c.Axis == axis);

    [Fact]
    public void EqualToAnExistingGap_OnTheRightOfANeighbour()
    {
        // A neighbour at x 100..110 and the box dragged to start at 143: its gap is 33, the existing gap is 30.
        var targets = Targets(Row[0], Row[1], (100, 0, 10, 10));

        var candidates = SmartSnap.EqualSpacing(new Bounds(143, 0, 10, 10), targets, 8);

        var candidate = Assert.Single(candidates, c => c.Axis == SnapAxis.X);
        Assert.Equal(-3, candidate.Offset, 9);
        Assert.All(candidate.Markers, m => Assert.Equal(SmartMarkerType.EqualGap, m.Type));
        Assert.Contains(candidate.Markers, m => m.From == 110 && Math.Abs(m.To - 140) < 1e-9 && Math.Abs(m.Value - 30) < 1e-9);
        Assert.Contains(candidate.Markers, m => m.From == 10 && m.To == 40 && m.Value == 30);
    }

    [Fact]
    public void EqualToAnExistingGap_OnTheLeftOfANeighbour()
    {
        // The box sits left of a neighbour at x 100..110 with a gap of 27: it moves left by 3 to make the gap 30.
        var targets = Targets(Row[0], Row[1], (100, 0, 10, 10));

        var candidates = SmartSnap.EqualSpacing(new Bounds(63, 0, 10, 10), targets, 8);

        // The box has a neighbour on each side, so the middle between them is a candidate too (offset 7).
        Assert.Contains(candidates, c => c.Axis == SnapAxis.X && Math.Abs(c.Offset + 3) < 1e-9);
        Assert.Contains(candidates, c => c.Axis == SnapAxis.X && Math.Abs(c.Offset - 7) < 1e-9);
    }

    [Fact]
    public void CenteredBetweenTwoNeighbours()
    {
        var targets = Targets((0, 0, 10, 10), (100, 0, 10, 10));

        // The box is 10 wide at x 42..52: the gaps are 32 and 48, the middle puts it at 50..60 (gaps 40 and 40).
        var candidate = Assert.Single(SmartSnap.EqualSpacing(new Bounds(42, 0, 10, 10), targets, 10), c => c.Axis == SnapAxis.X);

        Assert.Equal(8, candidate.Offset, 9);
        Assert.Equal(2, candidate.Markers.Count);
        Assert.All(candidate.Markers, m => Assert.Equal(40, m.Value, 9));
    }

    [Fact]
    public void CenteredBetweenTwoNeighbours_OutsideTolerance_GivesNothing()
    {
        var targets = Targets((0, 0, 10, 10), (100, 0, 10, 10));

        var candidates = SmartSnap.EqualSpacing(new Bounds(20, 0, 10, 10), targets, 8);

        Assert.DoesNotContain(candidates, c => c.Axis == SnapAxis.X);
    }

    [Fact]
    public void Vertical_WorksLikeHorizontal()
    {
        var targets = Targets((0, 0, 10, 10), (0, 40, 10, 10), (0, 100, 10, 10));

        var candidate = Assert.Single(SmartSnap.EqualSpacing(new Bounds(0, 143, 10, 10), targets, 8), c => c.Axis == SnapAxis.Y);

        Assert.Equal(-3, candidate.Offset, 9);
    }

    [Fact]
    public void OutsideTolerance_GivesNothing()
    {
        var targets = Targets(Row[0], Row[1], (100, 0, 10, 10));

        Assert.Empty(SmartSnap.EqualSpacing(new Bounds(150, 0, 10, 10), targets, 3));
    }

    [Fact]
    public void ToleranceDecidesWhetherItSnaps()
    {
        var targets = Targets(Row[0], Row[1], (100, 0, 10, 10));
        var box = new Bounds(143, 0, 10, 10);

        Assert.Empty(SmartSnap.EqualSpacing(box, targets, 2));
        Assert.NotEmpty(SmartSnap.EqualSpacing(box, targets, 3));
    }

    [Fact]
    public void NoNeighbours_GivesNothing()
    {
        Assert.Empty(SmartSnap.EqualSpacing(new Bounds(0, 0, 10, 10), SmartTargets.Empty, 8));
    }

    [Fact]
    public void ABoxWithoutAnExistingEqualGapAndOneNeighbour_GivesNothing()
    {
        var targets = Targets((0, 0, 10, 10));

        Assert.Empty(SmartSnap.EqualSpacing(new Bounds(40, 0, 10, 10), targets, 8));
    }

    [Fact]
    public void BadInput_GivesNothing()
    {
        var targets = Targets(Row[0], Row[1], (100, 0, 10, 10));

        Assert.Empty(SmartSnap.EqualSpacing(new Bounds(double.NaN, 0, 10, 10), targets, 8));
        Assert.Empty(SmartSnap.EqualSpacing(new Bounds(143, 0, 10, 10), targets, double.NaN));
        Assert.Empty(SmartSnap.EqualSpacing(new Bounds(143, 0, 10, 10), targets, -1));
        Assert.Empty(SmartSnap.EqualSpacing(new Bounds(143, 0, 0, 0), SmartTargets.Empty, 8));
    }

    [Fact]
    public void MarkersMarkAtMostThreeExistingGaps()
    {
        var targets = Targets((0, 0, 10, 10), (40, 0, 10, 10), (0, 100, 10, 10), (40, 100, 10, 10), (0, 200, 10, 10), (40, 200, 10, 10), (0, 300, 10, 10), (40, 300, 10, 10), (200, 0, 10, 10));

        var candidate = Assert.Single(SmartSnap.EqualSpacing(new Bounds(243, 0, 10, 10), targets, 8), c => c.Axis == SnapAxis.X);

        // The dragged gap plus at most three of the four equal gaps.
        Assert.True(candidate.Markers.Count <= 4);
    }
}
