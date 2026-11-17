using System;
using System.Linq;
using System.Threading;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class PathShapeOperationsBooleanTests
{
    private static readonly Guid ResultId = new("99999999-9999-9999-9999-999999999999");

    private static RectangleShape Rect(double x, double y, double size = 10) => new() { X = x, Y = y, Width = size, Height = size };

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static PathOperationResult Union(Document document, params Shape[] operands) =>
        PathShapeOperations.Boolean(document, [.. operands.Select(shape => shape.Id)], PathOperationType.Union, ResultId);

    [Fact]
    public void Boolean_TwoRectangles_ReplacesThemWithOnePath()
    {
        var a = Rect(0, 0, 100);
        var b = Rect(50, 50, 100);
        var document = Build(a, b);

        var result = Union(document, a, b);

        Assert.True(result.Succeeded);
        Assert.Equal([ResultId], result.NewShapeIds);
        var shape = Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(ResultId, shape.Id);
        Assert.Equal(FillRule.EvenOdd, shape.FillRule);
        Assert.Equal(0, shape.Rotation);
        Assert.Single(shape.SubPaths);
        Assert.True(shape.Contains(20, 20));
        Assert.True(shape.Contains(120, 120));
        Assert.False(shape.Contains(120, 10));
    }

    [Fact]
    public void Boolean_TakesTheAppearanceNameAndFlagsOfTheBottomOperand()
    {
        var look = new Appearance { Fill = Color.FromRgb(10, 20, 30), StrokeWidth = 3 };
        var bottom = Rect(0, 0) with { Appearance = look, Name = "Bottom", IsVisible = true };
        var top = Rect(5, 5) with { Appearance = new Appearance { Fill = Color.FromRgb(200, 0, 0) }, Name = "Top" };

        var result = Union(Build(bottom, top), bottom, top);

        var shape = Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(look, shape.Appearance);
        Assert.Equal("Bottom", shape.Name);
    }

    [Fact]
    public void Boolean_OperandsGivenInAnyOrder_BottomOneIsTheLowestInTheDocument()
    {
        var bottom = Rect(0, 0) with { Name = "Bottom" };
        var top = Rect(5, 5) with { Name = "Top" };
        var document = Build(bottom, top);

        var result = PathShapeOperations.Boolean(document, [top.Id, bottom.Id], PathOperationType.Union, ResultId);

        Assert.Equal("Bottom", Assert.Single(result.Document!.Layers[0].Shapes).Name);
    }

    [Fact]
    public void Boolean_ResultTakesThePlaceOfTheBottomOperandInTheStack()
    {
        var below = Rect(100, 0);
        var bottom = Rect(0, 0);
        var between = Rect(200, 0);
        var top = Rect(5, 5);
        var above = Rect(300, 0);
        var document = Build(below, bottom, between, top, above);

        var result = Union(document, bottom, top);

        Assert.Equal([below.Id, ResultId, between.Id, above.Id], result.Document!.Layers[0].Shapes.Select(s => s.Id));
    }

    [Fact]
    public void Boolean_OperandsInDifferentLayers_ResultIsInTheLayerOfTheBottomOperand()
    {
        var bottom = Rect(0, 0);
        var top = Rect(5, 5);
        var document = Document.Create("Lower");
        var upperLayer = new Layer { Name = "Upper" };
        document = document.AddLayer(upperLayer);
        document = document.AddShape(document.Layers[0].Id, bottom).AddShape(upperLayer.Id, top);

        var result = Union(document, bottom, top);

        Assert.Equal(ResultId, Assert.Single(result.Document!.Layers[0].Shapes).Id);
        Assert.Empty(result.Document.Layers[1].Shapes);
    }

    [Fact]
    public void Boolean_BottomOperandInAGroup_ResultStaysInThatGroup()
    {
        var bottom = Rect(0, 0);
        var other = Rect(100, 100);
        var top = Rect(5, 5);
        var group = new GroupShape { Children = [bottom, other] };
        var document = Build(group, top);

        var result = Union(document, bottom, top);

        var shapes = result.Document!.Layers[0].Shapes;
        var kept = Assert.IsType<GroupShape>(Assert.Single(shapes));
        Assert.Equal([ResultId, other.Id], kept.Children.Select(s => s.Id));
    }

    [Fact]
    public void Boolean_OtherOperandWasTheOnlyChildOfAGroup_TheEmptyGroupGoes()
    {
        var bottom = Rect(0, 0);
        var lonely = Rect(5, 5);
        var document = Build(bottom, new GroupShape { Children = [lonely] });

        var result = Union(document, bottom, lonely);

        Assert.Equal([ResultId], result.Document!.Layers[0].Shapes.Select(s => s.Id));
    }

    [Fact]
    public void Boolean_RotatedAndRoundedOperands_UseWhatYouSee()
    {
        var rotated = new RectangleShape { X = 0, Y = 0, Width = 50, Height = 50, Rotation = 45 };
        var upright = Rect(0, 0, 50);
        var document = Build(upright, rotated);

        var result = PathShapeOperations.Boolean(document, [upright.Id, rotated.Id], PathOperationType.Intersection, ResultId);

        var shape = Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(8, shape.NodeCount);
        Assert.True(shape.Contains(25, 25));
        Assert.False(shape.Contains(1, 1));
    }

    [Fact]
    public void Boolean_EllipseAndRectangle_KeepsTheCurve()
    {
        var circle = new EllipseShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var half = Rect(50, 0, 100);
        var document = Build(circle, half);

        var result = PathShapeOperations.Boolean(document, [circle.Id, half.Id], PathOperationType.Intersection, ResultId);

        var shape = Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.True(shape.NodeCount <= 6);
        Assert.True(shape.SubPaths[0].Nodes.Any(node => node.HasIn || node.HasOut));
    }

    [Fact]
    public void Boolean_EmptyResult_FailsWithEmptyAndLeavesTheDocument()
    {
        var a = Rect(0, 0);
        var b = Rect(50, 50);
        var document = Build(a, b);

        var result = PathShapeOperations.Boolean(document, [a.Id, b.Id], PathOperationType.Intersection, ResultId);

        Assert.False(result.Succeeded);
        Assert.Equal(PathOperationFailure.Empty, result.Failure);
        Assert.Null(result.Document);
        Assert.Equal(2, document.Layers[0].Shapes.Length);
    }

    [Fact]
    public void Boolean_Difference_CutsUpperOperandsFromTheBottomOne()
    {
        var bottom = Rect(0, 0, 100);
        var hole = Rect(40, 40, 20);
        var document = Build(bottom, hole);

        var result = PathShapeOperations.Boolean(document, [bottom.Id, hole.Id], PathOperationType.Difference, ResultId);

        var shape = Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(2, shape.SubPaths.Length);
        Assert.False(shape.Contains(50, 50));
        Assert.True(shape.Contains(10, 10));
    }

    [Fact]
    public void Boolean_Undo_RestoresTheOperandsAndTheirOrder()
    {
        var a = Rect(0, 0);
        var b = Rect(5, 5);
        var c = Rect(100, 100);
        var document = Build(a, c, b);
        var history = new DocumentHistory(document);

        var result = Union(document, a, b);
        history.Record(HistoryStepType.PasteShapes, result.Document!);
        var undone = history.Undo();

        Assert.Equal([a.Id, c.Id, b.Id], undone!.Layers[0].Shapes.Select(s => s.Id));
    }

    [Fact]
    public void Boolean_DoesNotChangeTheInputDocument()
    {
        var a = Rect(0, 0);
        var b = Rect(5, 5);
        var document = Build(a, b);

        Union(document, a, b);

        Assert.Equal([a.Id, b.Id], document.Layers[0].Shapes.Select(s => s.Id));
    }

    [Fact]
    public void Boolean_FewerThanTwoOrRepeatedIds_IsInvalid()
    {
        var a = Rect(0, 0);
        var document = Build(a);

        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.Boolean(document, [a.Id], PathOperationType.Union, ResultId).Failure);
        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.Boolean(document, [a.Id, a.Id], PathOperationType.Union, ResultId).Failure);
        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.Boolean(document, [a.Id, Guid.NewGuid()], PathOperationType.Union, ResultId).Failure);
    }

    [Fact]
    public void Boolean_Group_IsInvalid()
    {
        var group = new GroupShape { Children = [Rect(0, 0)] };
        var other = Rect(5, 5);

        var result = Union(Build(group, other), group, other);

        Assert.Equal(PathOperationFailure.Invalid, result.Failure);
    }

    [Fact]
    public void Boolean_OpenPath_IsInvalid()
    {
        var open = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var other = Rect(5, 5);

        Assert.Equal(PathOperationFailure.Invalid, Union(Build(open, other), open, other).Failure);
    }

    [Fact]
    public void Boolean_ClosedPathOperand_IsAccepted()
    {
        var triangle = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(20, 0), new DocumentNode(0, 20)], isClosed: true);
        var other = Rect(5, 5);

        Assert.True(Union(Build(triangle, other), triangle, other).Succeeded);
    }

    [Fact]
    public void Boolean_LockedOrHiddenOperand_IsInvalid()
    {
        var locked = Rect(0, 0) with { IsLocked = true };
        var hidden = Rect(0, 0) with { IsVisible = false };
        var plain = Rect(5, 5);

        Assert.Equal(PathOperationFailure.Invalid, Union(Build(locked, plain), locked, plain).Failure);
        Assert.Equal(PathOperationFailure.Invalid, Union(Build(hidden, plain), hidden, plain).Failure);
    }

    [Fact]
    public void Boolean_OperandOfALockedLayerOrGroup_IsInvalid()
    {
        var inGroup = Rect(0, 0);
        var plain = Rect(5, 5);
        var lockedGroup = new GroupShape { Children = [inGroup], IsLocked = true };
        var document = Build(lockedGroup, plain);

        Assert.Equal(PathOperationFailure.Invalid, Union(document, inGroup, plain).Failure);

        var lockedLayer = Build(Rect(0, 0), plain);
        lockedLayer = lockedLayer.UpdateLayer(lockedLayer.Layers[0].Id, layer => layer with { IsLocked = true });
        var shapes = lockedLayer.Layers[0].Shapes;
        Assert.Equal(PathOperationFailure.Invalid, Union(lockedLayer, shapes[0], shapes[1]).Failure);
    }

    [Fact]
    public void Boolean_CancelledToken_Throws()
    {
        var a = Rect(0, 0);
        var b = Rect(5, 5);
        using var source = new CancellationTokenSource();
        source.Cancel();

        Assert.Throws<OperationCanceledException>(() => PathShapeOperations.Boolean(Build(a, b), [a.Id, b.Id], PathOperationType.Union, ResultId, source.Token));
    }
}
