using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SnapTargetsTests
{
    private static readonly IReadOnlySet<Guid> None = new HashSet<Guid>();

    private static RectangleShape Square(double x, double y, double size = 30, double rotation = 0, bool visible = true, bool locked = false) =>
        new() { X = x, Y = y, Width = size, Height = size, Rotation = rotation, IsVisible = visible, IsLocked = locked };

    private static Document With(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static double[] Values(IReadOnlyList<SnapTarget> targets, params SnapTargetType[] types) =>
        targets.Where(t => types.Contains(t.Type)).Select(t => t.Value).ToArray();

    [Fact]
    public void Collect_EmptyDocument_GivesOnlyTheArtboard()
    {
        var document = Document.Create("Layer");

        var targets = SnapTargets.Collect(document, None);

        Assert.Equal([0, document.ArtboardWidth / 2, document.ArtboardWidth], Values(targets.X, SnapTargetType.Artboard));
        Assert.Equal([0, document.ArtboardHeight / 2, document.ArtboardHeight], Values(targets.Y, SnapTargetType.Artboard));
        Assert.Equal(3, targets.X.Count);
        Assert.Equal(3, targets.Y.Count);
    }

    [Fact]
    public void Collect_Shape_AddsEdgesAndCenterOfItsBox()
    {
        var targets = SnapTargets.Collect(With(Square(100, 200)), None);

        Assert.Equal([100, 115, 130], Values(targets.X, SnapTargetType.BoxEdge, SnapTargetType.BoxCenter));
        Assert.Equal([200, 215, 230], Values(targets.Y, SnapTargetType.BoxEdge, SnapTargetType.BoxCenter));
        Assert.Single(targets.X, t => t.Type == SnapTargetType.BoxCenter);
    }

    [Fact]
    public void Collect_RotatedShape_UsesTheBoundsOfTheTurnedOutline()
    {
        var shape = Square(100, 100, 100, rotation: 45);

        var targets = SnapTargets.Collect(With(shape), None);

        var half = 100 * Math.Sqrt(2) / 2;
        var values = Values(targets.X, SnapTargetType.BoxEdge);
        Assert.Equal(150 - half, values[0], 6);
        Assert.Equal(150 + half, values[1], 6);
    }

    [Fact]
    public void Collect_HiddenAndLockedShapes_HiddenLeftOutLockedKept()
    {
        var targets = SnapTargets.Collect(With(Square(100, 100, visible: false), Square(300, 100, locked: true)), None);

        Assert.Equal([300, 330], Values(targets.X, SnapTargetType.BoxEdge));
    }

    [Fact]
    public void Collect_HiddenLayer_IsLeftOut()
    {
        var document = With(Square(100, 100));
        document = document.UpdateLayer(document.Layers[0].Id, layer => layer with { IsVisible = false });

        var targets = SnapTargets.Collect(document, None);

        Assert.Empty(Values(targets.X, SnapTargetType.BoxEdge));
    }

    [Fact]
    public void Collect_ExcludedShape_GivesNothing()
    {
        var shape = Square(100, 100);

        var targets = SnapTargets.Collect(With(shape, Square(300, 100)), new HashSet<Guid> { shape.Id });

        Assert.Equal([300, 330], Values(targets.X, SnapTargetType.BoxEdge));
    }

    [Fact]
    public void Collect_Group_AddsItsOwnBoxAndTheBoxesOfItsMembers()
    {
        var a = Square(100, 100);
        var b = Square(160, 100);
        var group = new GroupShape { Children = [a, b] };

        var targets = SnapTargets.Collect(With(group), None);

        Assert.Equal([100, 100, 130, 160, 190, 190], Values(targets.X, SnapTargetType.BoxEdge));
    }

    [Fact]
    public void Collect_ExcludedGroup_KeepsItsOtherMembers()
    {
        var a = Square(100, 100);
        var b = Square(160, 100);
        var group = new GroupShape { Children = [a, b] };

        var targets = SnapTargets.Collect(With(group), new HashSet<Guid> { group.Id, a.Id });

        Assert.Equal([160, 190], Values(targets.X, SnapTargetType.BoxEdge));
    }

    [Fact]
    public void Collect_HiddenGroup_LeavesOutItsMembers()
    {
        var group = new GroupShape { Children = [Square(100, 100)], IsVisible = false };

        var targets = SnapTargets.Collect(With(group), None);

        Assert.Empty(Values(targets.X, SnapTargetType.BoxEdge));
    }

    [Fact]
    public void Collect_Path_AddsItsAnchorNodes()
    {
        var path = PathShape.Create([new DocumentNode(10, 20), new DocumentNode(70, 20), new DocumentNode(70, 90)]);

        var targets = SnapTargets.Collect(With(path), None);

        Assert.Equal([10, 70, 70], Values(targets.X, SnapTargetType.PathNode));
        Assert.Equal([20, 20, 90], Values(targets.Y, SnapTargetType.PathNode));
    }

    [Fact]
    public void Collect_RotatedPath_AddsTheNodesWhereTheyAreOnTheScreen()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]) with { Rotation = 90 };

        var targets = SnapTargets.Collect(With(path), None);

        var nodesX = Values(targets.X, SnapTargetType.PathNode);
        Assert.Equal(2, nodesX.Length);
        Assert.Equal(nodesX[0], nodesX[1], 6);
    }

    [Fact]
    public void WithNodes_AddsNodeTargetsOnBothAxes()
    {
        var targets = SnapTargets.Empty.WithNodes([(5, 6), (7, 8), (double.NaN, 1)]);

        Assert.Equal([5, 7], Values(targets.X, SnapTargetType.PathNode));
        Assert.Equal([6, 8], Values(targets.Y, SnapTargetType.PathNode));
    }

    [Fact]
    public void TryFindNearest_WithinTolerance_ReturnsTheNearest()
    {
        var targets = SnapTargets.Empty.WithNodes([(10, 0), (20, 0), (30, 0)]);

        Assert.True(targets.TryFindNearest(SnapAxis.X, 22, 5, out var nearest));
        Assert.Equal(20, nearest.Value);
        Assert.True(targets.TryFindNearest(SnapAxis.X, 26, 5, out nearest));
        Assert.Equal(30, nearest.Value);
    }

    [Fact]
    public void TryFindNearest_ToleranceIsInclusiveAndOutsideFindsNothing()
    {
        var targets = SnapTargets.Empty.WithNodes([(10, 0)]);

        Assert.True(targets.TryFindNearest(SnapAxis.X, 15, 5, out _));
        Assert.False(targets.TryFindNearest(SnapAxis.X, 15.01, 5, out _));
        Assert.False(targets.TryFindNearest(SnapAxis.X, 4.99, 5, out _));
    }

    [Fact]
    public void TryFindNearest_BeyondBothEndsAndNoTargets_Works()
    {
        var targets = SnapTargets.Empty.WithNodes([(10, 0)]);

        Assert.True(targets.TryFindNearest(SnapAxis.X, -100, 200, out var nearest));
        Assert.Equal(10, nearest.Value);
        Assert.False(SnapTargets.Empty.TryFindNearest(SnapAxis.X, 1, 5, out _));
    }

    [Fact]
    public void TryFindNearest_NonFiniteInput_FindsNothing()
    {
        var targets = SnapTargets.Empty.WithNodes([(10, 0)]);

        Assert.False(targets.TryFindNearest(SnapAxis.X, double.NaN, 5, out _));
        Assert.False(targets.TryFindNearest(SnapAxis.X, 10, double.NaN, out _));
        Assert.False(targets.TryFindNearest(SnapAxis.X, 10, -1, out _));
    }

    [Fact]
    public void TryFindNearest_YAxis_UsesTheYTargets()
    {
        var targets = SnapTargets.Empty.WithNodes([(10, 50)]);

        Assert.True(targets.TryFindNearest(SnapAxis.Y, 48, 5, out var nearest));
        Assert.Equal(50, nearest.Value);
        Assert.False(targets.TryFindNearest(SnapAxis.Y, 12, 1, out _));
    }

    [Fact]
    public void Collect_TargetsKeepTheExtentAlongTheOtherAxis()
    {
        var targets = SnapTargets.Collect(With(Square(100, 200)), None);

        var left = targets.X.Single(t => t.Type == SnapTargetType.BoxEdge && t.Value == 100);
        Assert.Equal((200, 230), (left.From, left.To));
    }
}
