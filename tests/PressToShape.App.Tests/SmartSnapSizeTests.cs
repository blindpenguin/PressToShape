using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SmartSnapSizeTests
{
    private static SmartTargets Targets(params (double X, double Y, double W, double H)[] boxes)
    {
        var document = Document.Create("L");
        var shapes = boxes.Select(b => (Shape)new RectangleShape { X = b.X, Y = b.Y, Width = b.W, Height = b.H });
        return SmartTargets.Collect(document.AddShapes(document.Layers[0].Id, shapes), new HashSet<Guid>());
    }

    // One box 100 wide and 60 high at (300, 300).
    private static readonly SmartTargets Other = Targets((300, 300, 100, 60));

    [Fact]
    public void Width_SnapsToTheWidthOfAnotherBox_RightEdgeDragged()
    {
        var candidate = Assert.Single(SmartSnap.EqualSize(new Bounds(0, 0, 96, 10), 1, 0, false, Other, 8));

        Assert.Equal(SnapAxis.X, candidate.Axis);
        Assert.Equal(4, candidate.Offset, 9);
    }

    [Fact]
    public void Width_LeftEdgeDragged_TheOffsetPointsTheOtherWay()
    {
        // The box is 96 wide, the left edge moves left by 4 to make it 100: the pointer goes left, so the offset is -4.
        var candidate = Assert.Single(SmartSnap.EqualSize(new Bounds(0, 0, 96, 10), -1, 0, false, Other, 8));

        Assert.Equal(-4, candidate.Offset, 9);
    }

    [Fact]
    public void Height_Snaps()
    {
        var candidate = Assert.Single(SmartSnap.EqualSize(new Bounds(0, 0, 10, 63), 0, 1, false, Other, 8));

        Assert.Equal(SnapAxis.Y, candidate.Axis);
        Assert.Equal(-3, candidate.Offset, 9);
    }

    [Fact]
    public void WidthAndHeightAtOnce_GiveTwoCandidates()
    {
        var candidates = SmartSnap.EqualSize(new Bounds(0, 0, 97, 63), 1, 1, false, Other, 8);

        Assert.Equal(2, candidates.Count);
        Assert.Contains(candidates, c => c.Axis == SnapAxis.X && Math.Abs(c.Offset - 3) < 1e-9);
        Assert.Contains(candidates, c => c.Axis == SnapAxis.Y && Math.Abs(c.Offset + 3) < 1e-9);
    }

    [Fact]
    public void AxisWithoutAMovingEdge_IsSkipped()
    {
        var candidates = SmartSnap.EqualSize(new Bounds(0, 0, 97, 63), 1, 0, false, Other, 8);

        Assert.DoesNotContain(candidates, c => c.Axis == SnapAxis.Y);
    }

    [Fact]
    public void OutsideTolerance_GivesNothing()
    {
        Assert.Empty(SmartSnap.EqualSize(new Bounds(0, 0, 80, 10), 1, 0, false, Other, 8));
    }

    [Fact]
    public void FromCenter_TheOffsetIsHalf()
    {
        var candidate = Assert.Single(SmartSnap.EqualSize(new Bounds(0, 0, 96, 10), 1, 0, true, Other, 8));

        Assert.Equal(2, candidate.Offset, 9);
    }

    [Fact]
    public void Markers_BracketTheDraggedBoxAsItWillBe_AndTheMatchingBox()
    {
        var candidate = Assert.Single(SmartSnap.EqualSize(new Bounds(10, 20, 96, 40), 1, 0, false, Other, 8));

        Assert.Equal(2, candidate.Markers.Count);
        Assert.All(candidate.Markers, m => Assert.Equal(SmartMarkerType.EqualSize, m.Type));
        var dragged = candidate.Markers[0];
        Assert.Equal((10.0, 110.0, 60.0, 100.0), (dragged.From, dragged.To, dragged.Cross, dragged.Value));
        var matching = candidate.Markers[1];
        Assert.Equal((300.0, 400.0, 360.0, 100.0), (matching.From, matching.To, matching.Cross, matching.Value));
    }

    [Fact]
    public void Markers_LeftEdgeDragged_KeepTheRightEdgeFixed()
    {
        var candidate = Assert.Single(SmartSnap.EqualSize(new Bounds(10, 20, 96, 40), -1, 0, false, Other, 8));

        Assert.Equal((6.0, 106.0), (candidate.Markers[0].From, candidate.Markers[0].To));
    }

    [Fact]
    public void Markers_FromCenter_StayCentered()
    {
        var candidate = Assert.Single(SmartSnap.EqualSize(new Bounds(10, 20, 96, 40), 1, 0, true, Other, 8));

        Assert.Equal((8.0, 108.0), (candidate.Markers[0].From, candidate.Markers[0].To));
    }

    [Fact]
    public void TheNearestOfSeveralSizes_Wins()
    {
        var targets = Targets((0, 0, 100, 10), (200, 0, 106, 10));

        var candidate = Assert.Single(SmartSnap.EqualSize(new Bounds(0, 0, 104, 10), 1, 0, false, targets, 8));

        Assert.Equal(2, candidate.Offset, 9);
    }

    [Fact]
    public void ZeroNaNAndNegativeInput_GiveNothing()
    {
        Assert.Empty(SmartSnap.EqualSize(new Bounds(0, 0, 0, 0), 1, 1, false, Other, 8));
        Assert.Empty(SmartSnap.EqualSize(new Bounds(0, 0, double.NaN, 10), 1, 1, false, Other, 8));
        Assert.Empty(SmartSnap.EqualSize(new Bounds(0, 0, 96, 10), 1, 1, false, Other, double.NaN));
        Assert.Empty(SmartSnap.EqualSize(new Bounds(0, 0, 96, 10), 1, 1, false, Other, -1));
        Assert.Empty(SmartSnap.EqualSize(new Bounds(0, 0, 96, 10), 1, 1, false, SmartTargets.Empty, 8));
    }
}
