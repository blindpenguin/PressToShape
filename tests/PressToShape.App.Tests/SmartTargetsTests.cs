using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SmartTargetsTests
{
    private static readonly IReadOnlySet<Guid> None = new HashSet<Guid>();

    private static RectangleShape Box(double x, double y, double width = 10, double height = 10, bool visible = true, bool locked = false, double rotation = 0) =>
        new() { X = x, Y = y, Width = width, Height = height, IsVisible = visible, IsLocked = locked, Rotation = rotation };

    private static Document With(params Shape[] shapes)
    {
        var document = Document.Create("L");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    [Fact]
    public void Collect_EmptyDocument_HasNothing()
    {
        var targets = SmartTargets.Collect(Document.Create("L"), None);

        Assert.Empty(targets.Boxes);
        Assert.Empty(targets.Gaps(SnapAxis.X));
        Assert.Empty(targets.Sizes(SnapAxis.Y));
    }

    [Fact]
    public void Collect_HiddenLockedAndExcluded()
    {
        var excluded = Box(500, 0);
        var targets = SmartTargets.Collect(
            With(Box(0, 0, visible: false), Box(100, 0, locked: true), excluded, Box(300, 0)),
            new HashSet<Guid> { excluded.Id });

        Assert.Equal([100.0, 300.0], targets.Boxes.Select(b => b.Bounds.X).ToArray());
    }

    [Fact]
    public void Collect_HiddenLayer_AndHiddenGroupMembers_AreLeftOut()
    {
        var document = With(new GroupShape { Children = [Box(0, 0)], IsVisible = false });
        document = document.UpdateLayer(document.Layers[0].Id, layer => layer with { IsVisible = false });

        Assert.Empty(SmartTargets.Collect(document, None).Boxes);
        Assert.Empty(SmartTargets.Collect(With(new GroupShape { Children = [Box(0, 0)], IsVisible = false }), None).Boxes);
    }

    [Fact]
    public void Collect_GroupCountsByItsOwnBox_AndItsMembersToo()
    {
        var group = new GroupShape { Children = [Box(0, 0), Box(100, 0)] };

        var targets = SmartTargets.Collect(With(group), None);

        Assert.Equal(3, targets.Boxes.Count);
        Assert.Contains(targets.Boxes, b => b.Id == group.Id && b.Bounds.Width == 110);
    }

    [Fact]
    public void Collect_ExcludedGroup_KeepsItsMembers()
    {
        var a = Box(0, 0);
        var group = new GroupShape { Children = [a, Box(100, 0)] };

        var targets = SmartTargets.Collect(With(group), new HashSet<Guid> { group.Id, a.Id });

        Assert.Single(targets.Boxes);
        Assert.Equal(100, targets.Boxes[0].Bounds.X);
    }

    [Fact]
    public void Gaps_ARow_AreBetweenNeighboursOnly()
    {
        // Three boxes in a row with gaps 20 and 30. The gap between the first and the third is not a gap.
        var targets = SmartTargets.Collect(With(Box(0, 0), Box(30, 0), Box(70, 0)), None);

        Assert.Equal([20.0, 30.0], targets.Gaps(SnapAxis.X).Select(g => g.Size).ToArray());
        Assert.Empty(targets.Gaps(SnapAxis.Y));
    }

    [Fact]
    public void Gaps_AColumn_AreVertical()
    {
        var targets = SmartTargets.Collect(With(Box(0, 0), Box(0, 25), Box(0, 60)), None);

        Assert.Equal([15.0, 25.0], targets.Gaps(SnapAxis.Y).Select(g => g.Size).ToArray());
        Assert.Empty(targets.Gaps(SnapAxis.X));
    }

    [Fact]
    public void Gaps_BoxesThatDoNotOverlapOnTheOtherAxis_HaveNoGap()
    {
        var targets = SmartTargets.Collect(With(Box(0, 0), Box(50, 100)), None);

        Assert.Empty(targets.Gaps(SnapAxis.X));
    }

    [Fact]
    public void Gaps_TouchingOrOverlappingBoxes_HaveNoGap()
    {
        var targets = SmartTargets.Collect(With(Box(0, 0), Box(10, 0), Box(15, 0)), None);

        Assert.Empty(targets.Gaps(SnapAxis.X));
    }

    [Fact]
    public void Gaps_PositionIsInTheMiddleOfTheOverlap()
    {
        var targets = SmartTargets.Collect(With(Box(0, 0, 10, 20), Box(30, 10, 10, 20)), None);

        var gap = Assert.Single(targets.Gaps(SnapAxis.X));
        Assert.Equal((20.0, 10.0, 30.0, 15.0), (gap.Size, gap.From, gap.To, gap.Cross));
    }

    [Fact]
    public void Gaps_UseTheNearestNeighbourOfEachBox()
    {
        // Box 0 sees box 1 at gap 10, not box 2 at gap 40.
        var targets = SmartTargets.Collect(With(Box(0, 0), Box(20, 0), Box(50, 0)), None);

        Assert.Equal([10.0, 20.0], targets.Gaps(SnapAxis.X).Select(g => g.Size).ToArray());
    }

    [Fact]
    public void Boxes_AreTheBoundsOfTheTurnedOutline()
    {
        var targets = SmartTargets.Collect(With(Box(100, 100, 100, 20, rotation: 90)), None);

        var box = targets.Boxes[0].Bounds;
        Assert.Equal(20, box.Width, 6);
        Assert.Equal(100, box.Height, 6);
    }

    [Fact]
    public void Sizes_AreSortedAndSkipZero()
    {
        var targets = SmartTargets.Collect(With(Box(0, 0, 30, 5), Box(100, 0, 10, 50), Box(200, 0, 0, 7)), None);

        Assert.Equal([10.0, 30.0], targets.Sizes(SnapAxis.X).Select(s => s.Size).ToArray());
        Assert.Equal([5.0, 7.0, 50.0], targets.Sizes(SnapAxis.Y).Select(s => s.Size).ToArray());
    }

    [Fact]
    public void FindGaps_ReturnsTheOnesWithinTheTolerance_NearestFirst()
    {
        var targets = SmartTargets.Collect(With(Box(0, 0), Box(20, 0), Box(50, 0), Box(85, 0)), None);

        var found = targets.FindGaps(SnapAxis.X, 21, 3, 4);

        Assert.Equal([20.0], found.Select(g => g.Size).ToArray());
        Assert.Equal([20.0, 25.0], targets.FindGaps(SnapAxis.X, 25, 6, 4).Select(g => g.Size).OrderBy(s => s).ToArray());
        Assert.Single(targets.FindGaps(SnapAxis.X, 25, 6, 1));
    }

    [Fact]
    public void FindGaps_BadInput_FindsNothing()
    {
        var targets = SmartTargets.Collect(With(Box(0, 0), Box(20, 0)), None);

        Assert.Empty(targets.FindGaps(SnapAxis.X, double.NaN, 3, 4));
        Assert.Empty(targets.FindGaps(SnapAxis.X, 10, -1, 4));
        Assert.Empty(targets.FindGaps(SnapAxis.X, 10, 3, 0));
    }

    [Fact]
    public void FindSize_NearestWithinTolerance_SmallerOnATie()
    {
        var targets = SmartTargets.Collect(With(Box(0, 0, 40, 10), Box(100, 0, 50, 10)), None);

        Assert.Equal(40, targets.FindSize(SnapAxis.X, 43, 5)!.Value.Size);
        Assert.Equal(40, targets.FindSize(SnapAxis.X, 45, 5)!.Value.Size);
        Assert.Null(targets.FindSize(SnapAxis.X, 60, 5));
        Assert.Null(targets.FindSize(SnapAxis.X, double.NaN, 5));
    }

    [Fact]
    public void Collect_ThousandBoxes_IsFast()
    {
        var shapes = Enumerable.Range(0, 1000).Select(i => (Shape)Box(i % 50 * 30, i / 50 * 30)).ToArray();
        var document = With(shapes);
        var timer = Stopwatch.StartNew();

        var targets = SmartTargets.Collect(document, None);

        Assert.Equal(1000, targets.Boxes.Count);
        Assert.True(timer.ElapsedMilliseconds < 5000, $"took {timer.ElapsedMilliseconds} ms");
    }
}
