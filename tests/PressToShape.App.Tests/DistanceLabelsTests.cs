using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class DistanceLabelsTests
{
    private static SmartTargets Targets(params (double X, double Y, double W, double H)[] boxes)
    {
        var document = Document.Create("L");
        var shapes = boxes.Select(b => (Shape)new RectangleShape { X = b.X, Y = b.Y, Width = b.W, Height = b.H });
        return SmartTargets.Collect(document.AddShapes(document.Layers[0].Id, shapes), new HashSet<Guid>());
    }

    private static readonly Bounds Dragged = new(100, 100, 50, 50);

    [Fact]
    public void Build_OneNeighbourOnEachSide_GivesFourLabels()
    {
        var targets = Targets((20, 110, 30, 30), (200, 110, 30, 30), (110, 20, 30, 30), (110, 200, 30, 30));

        var labels = DistanceLabels.Build(Dragged, targets);

        Assert.Equal(4, labels.Count);
        var left = labels.Single(l => l.Axis == SnapAxis.X && l.To == 100);
        Assert.Equal((50.0, 100.0, 125.0, 50.0), (left.From, left.To, left.Cross, left.Value));
        var right = labels.Single(l => l.Axis == SnapAxis.X && l.From == 150);
        Assert.Equal((150.0, 200.0, 125.0, 50.0), (right.From, right.To, right.Cross, right.Value));
        var above = labels.Single(l => l.Axis == SnapAxis.Y && l.To == 100);
        Assert.Equal((50.0, 100.0, 125.0, 50.0), (above.From, above.To, above.Cross, above.Value));
        var below = labels.Single(l => l.Axis == SnapAxis.Y && l.From == 150);
        Assert.Equal((150.0, 200.0, 125.0, 50.0), (below.From, below.To, below.Cross, below.Value));
        Assert.All(labels, l => Assert.Equal(SmartMarkerType.Distance, l.Type));
    }

    [Fact]
    public void Build_TheNearestOfSeveralOnASide()
    {
        var targets = Targets((0, 110, 30, 30), (60, 110, 20, 30));

        var label = Assert.Single(DistanceLabels.Build(Dragged, targets));

        Assert.Equal(20, label.Value);
        Assert.Equal(80, label.From);
    }

    [Fact]
    public void Build_NoNeighbour_GivesNoLabel()
    {
        Assert.Empty(DistanceLabels.Build(Dragged, Targets()));
    }

    [Fact]
    public void Build_NeighbourThatDoesNotOverlapOnTheOtherAxis_IsIgnored()
    {
        var targets = Targets((0, 300, 30, 30), (0, 0, 30, 30));

        Assert.Empty(DistanceLabels.Build(Dragged, targets));
    }

    [Fact]
    public void Build_TouchingOrOverlappingBoxes_GiveNoLabelOnThatAxis()
    {
        var targets = Targets((50, 110, 50, 30), (120, 110, 40, 30));

        Assert.Empty(DistanceLabels.Build(Dragged, targets));
    }

    [Fact]
    public void Build_TheLineIsInTheMiddleOfTheOverlap()
    {
        var targets = Targets((0, 130, 40, 100));

        var label = Assert.Single(DistanceLabels.Build(Dragged, targets));

        // The dragged box spans y 100..150, the neighbour 130..230: the overlap is 130..150, its middle 140.
        Assert.Equal(140, label.Cross);
        Assert.Equal(60, label.Value);
    }

    [Fact]
    public void Build_NonFiniteBox_GivesNothing()
    {
        var targets = Targets((0, 110, 30, 30));

        Assert.Empty(DistanceLabels.Build(new Bounds(double.NaN, 0, 10, 10), targets));
        Assert.Empty(DistanceLabels.Build(new Bounds(0, 0, double.PositiveInfinity, 10), targets));
    }

    [Fact]
    public void Build_ZeroSizeDraggedBox_Works()
    {
        var targets = Targets((0, 90, 30, 30));

        var label = Assert.Single(DistanceLabels.Build(new Bounds(100, 100, 0, 0), targets));

        Assert.Equal(70, label.Value);
    }
}
