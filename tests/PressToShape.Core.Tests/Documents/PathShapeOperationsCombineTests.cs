using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class PathShapeOperationsCombineTests
{
    private static readonly Guid ResultId = new("99999999-9999-9999-9999-999999999999");

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static PathShape Compound(double rotation = 0) =>
        PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100), new DocumentNode(0, 100)], true),
                new DocumentSubPath([new DocumentNode(200, 0), new DocumentNode(300, 0)], false),
                new DocumentSubPath([new DocumentNode(25, 25), new DocumentNode(75, 25), new DocumentNode(75, 75)], true),
            ],
            FillRule.NonZero) with
        {
            Rotation = rotation,
            Name = "Compound",
            Appearance = new Appearance { Fill = Color.FromRgb(1, 2, 3), StrokeWidth = 7 },
        };

    private static (double X, double Y) OnScreen(PathShape path, int subPath, int node)
    {
        var local = path.ToDocumentSubPaths()[subPath].Nodes[node];
        return path.FromLocal(local.X, local.Y);
    }

    [Fact]
    public void Combine_BigAndSmallCircle_ShowsAHoleInTheMiddle()
    {
        var big = new EllipseShape { X = 0, Y = 0, Width = 200, Height = 200 };
        var small = new EllipseShape { X = 50, Y = 50, Width = 100, Height = 100 };

        var result = PathShapeOperations.Combine(Build(big, small), [big.Id, small.Id], ResultId);

        Assert.True(result.Succeeded);
        var path = Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(2, path.SubPaths.Length);
        Assert.Equal(FillRule.EvenOdd, path.FillRule);
        Assert.True(path.Contains(20, 100));
        Assert.False(path.Contains(100, 100));
    }

    [Fact]
    public void Combine_KeepsTheNodesInDocumentCoordinatesAndTakesTheBottomAppearance()
    {
        var bottom = new RectangleShape { X = 10, Y = 20, Width = 30, Height = 40, Name = "Bottom", Appearance = new Appearance { Fill = Color.FromRgb(9, 9, 9) } };
        var open = PathShape.Create([new DocumentNode(100, 100), new DocumentNode(150, 120)]);
        var document = Build(bottom, open);

        var result = PathShapeOperations.Combine(document, [open.Id, bottom.Id], ResultId);

        var path = Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(ResultId, path.Id);
        Assert.Equal("Bottom", path.Name);
        Assert.Equal(bottom.Appearance, path.Appearance);
        Assert.Equal(2, path.SubPaths.Length);
        Assert.True(path.SubPaths[0].IsClosed);
        Assert.False(path.SubPaths[1].IsClosed);
        Assert.Equal((10.0, 20.0), OnScreen(path, 0, 0));
        Assert.Equal(150, OnScreen(path, 1, 1).X, 6);
    }

    [Fact]
    public void Combine_RotatedOperand_IsCombinedAsItLooks()
    {
        var upright = new RectangleShape { X = 0, Y = 0, Width = 50, Height = 50 };
        var turned = new RectangleShape { X = 100, Y = 0, Width = 50, Height = 50, Rotation = 90 };
        var expected = ShapeConversion.ToPath(turned)!.WithRotationApplied().ToDocumentSubPaths()[0].Nodes;

        var result = PathShapeOperations.Combine(Build(upright, turned), [upright.Id, turned.Id], ResultId);

        var path = Assert.IsType<PathShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(0, path.Rotation);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].X, OnScreen(path, 1, i).X, 6);
            Assert.Equal(expected[i].Y, OnScreen(path, 1, i).Y, 6);
        }
    }

    [Fact]
    public void Combine_GroupHiddenOrFewerThanTwo_IsInvalid()
    {
        var a = new RectangleShape { Width = 10, Height = 10 };
        var group = new GroupShape { Children = [new RectangleShape { Width = 5, Height = 5 }] };
        var hidden = new RectangleShape { Width = 10, Height = 10, IsVisible = false };
        var document = Build(a, group, hidden);

        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.Combine(document, [a.Id], ResultId).Failure);
        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.Combine(document, [a.Id, group.Id], ResultId).Failure);
        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.Combine(document, [a.Id, hidden.Id], ResultId).Failure);
    }

    [Fact]
    public void BreakApart_CompoundPath_GivesOnePathPerSubPathInOrder()
    {
        var path = Compound();
        var before = Build(new RectangleShape { Width = 1, Height = 1 }, path, new RectangleShape { Width = 2, Height = 2 });
        var ids = new Queue<Guid>(Enumerable.Range(1, 3).Select(i => new Guid(i, 0, 0, new byte[8])));

        var result = PathShapeOperations.BreakApart(before, path.Id, ids.Dequeue);

        Assert.True(result.Succeeded);
        var shapes = result.Document!.Layers[0].Shapes;
        Assert.Equal(5, shapes.Length);
        Assert.Equal(result.NewShapeIds, shapes.Skip(1).Take(3).Select(s => s.Id));
        Assert.DoesNotContain(shapes, s => s.Id == path.Id);
        Assert.Equal([4, 2, 3], shapes.Skip(1).Take(3).Cast<PathShape>().Select(p => p.NodeCount));
        Assert.Equal([true, false, true], shapes.Skip(1).Take(3).Cast<PathShape>().Select(p => p.SubPaths[0].IsClosed));
    }

    [Fact]
    public void BreakApart_KeepsAppearanceNameFillRuleAndRotation()
    {
        var path = Compound(rotation: 30);
        var document = Build(path);

        var result = PathShapeOperations.BreakApart(document, path.Id, Guid.NewGuid);

        var pieces = result.Document!.Layers[0].Shapes.Cast<PathShape>().ToList();
        Assert.All(pieces, piece =>
        {
            Assert.Equal(path.Appearance, piece.Appearance);
            Assert.Equal("Compound", piece.Name);
            Assert.Equal(path.FillRule, piece.FillRule);
            Assert.Equal(30, piece.Rotation);
        });
    }

    [Fact]
    public void BreakApart_RotatedPath_EveryNodeStaysWhereItWasOnTheScreen()
    {
        var path = Compound(rotation: 30);

        var result = PathShapeOperations.BreakApart(Build(path), path.Id, Guid.NewGuid);

        var pieces = result.Document!.Layers[0].Shapes.Cast<PathShape>().ToList();
        for (var sub = 0; sub < path.SubPaths.Length; sub++)
        {
            for (var node = 0; node < path.SubPaths[sub].Nodes.Length; node++)
            {
                var expected = OnScreen(path, sub, node);
                var actual = OnScreen(pieces[sub], 0, node);
                Assert.Equal(expected.X, actual.X, 6);
                Assert.Equal(expected.Y, actual.Y, 6);
            }
        }
    }

    [Fact]
    public void BreakApart_PathInAGroup_PiecesStayInTheGroup()
    {
        var path = Compound();
        var group = new GroupShape { Children = [path] };

        var result = PathShapeOperations.BreakApart(Build(group), path.Id, Guid.NewGuid);

        var kept = Assert.IsType<GroupShape>(Assert.Single(result.Document!.Layers[0].Shapes));
        Assert.Equal(3, kept.Children.Length);
    }

    [Fact]
    public void BreakApart_OneSubPathOtherShapeOrLockedOrUnknown_IsInvalid()
    {
        var single = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var locked = Compound() with { IsLocked = true };
        var document = Build(single, rectangle, locked);

        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.BreakApart(document, single.Id, Guid.NewGuid).Failure);
        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.BreakApart(document, rectangle.Id, Guid.NewGuid).Failure);
        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.BreakApart(document, locked.Id, Guid.NewGuid).Failure);
        Assert.Equal(PathOperationFailure.Invalid, PathShapeOperations.BreakApart(document, Guid.NewGuid(), Guid.NewGuid).Failure);
    }

    [Fact]
    public void CombineThenBreakApart_GivesBackTheShapesAsPaths()
    {
        var a = new RectangleShape { X = 0, Y = 0, Width = 10, Height = 10 };
        var b = new RectangleShape { X = 50, Y = 0, Width = 10, Height = 10 };
        var combined = PathShapeOperations.Combine(Build(a, b), [a.Id, b.Id], ResultId).Document!;

        var apart = PathShapeOperations.BreakApart(combined, ResultId, Guid.NewGuid);

        var pieces = apart.Document!.Layers[0].Shapes.Cast<PathShape>().ToList();
        Assert.Equal(2, pieces.Count);
        Assert.True(pieces[0].Contains(5, 5));
        Assert.True(pieces[1].Contains(55, 5));
    }

    [Fact]
    public void Operations_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => PathShapeOperations.Combine(null!, [], ResultId));
        Assert.Throws<ArgumentNullException>(() => PathShapeOperations.BreakApart(Document.Create("x"), Guid.NewGuid(), null!));
    }
}
