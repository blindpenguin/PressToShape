using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PressToShape.Core.Geometry;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.PathOperations;

public class BooleanEngineTests
{
    private static Outline FromShape(Shape shape)
    {
        var path = shape as PathShape ?? ShapeConversion.ToPath(shape)!;
        path = path.WithRotationApplied();
        return new Outline(path.ToDocumentSubPaths(), path.FillRule);
    }

    private static Outline Square(double x, double y, double size) => FromShape(new RectangleShape { X = x, Y = y, Width = size, Height = size });

    private static Outline Circle(double centerX, double centerY, double radius) =>
        FromShape(new EllipseShape { X = centerX - radius, Y = centerY - radius, Width = radius * 2, Height = radius * 2 });

    // The area of the result: the signed areas of the flattened sub-paths add up (outlines one way, holes the other).
    private static double Area(IReadOnlyList<DocumentSubPath> subPaths)
    {
        var total = 0.0;
        foreach (var subPath in subPaths)
        {
            var segments = PathShape.BuildSegments(subPath.Nodes, subPath.IsClosed);
            var points = Flattening.Flatten(segments, subPath.IsClosed, 0, 0, 0.01);
            for (var i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                total += a.X * b.Y - b.X * a.Y;
            }
        }

        return Math.Abs(total / 2);
    }

    private static IReadOnlyList<DocumentSubPath> Run(PathOperationType type, params Outline[] operands) => BooleanEngine.Compute(type, operands);

    [Fact]
    public void Union_OverlappingSquares_GivesOneStraightOutlineWithTheUnionArea()
    {
        var result = Run(PathOperationType.Union, Square(0, 0, 10), Square(5, 5, 10));

        var subPath = Assert.Single(result);
        Assert.True(subPath.IsClosed);
        Assert.Equal(175, Area(result), 3);
        Assert.All(subPath.Nodes, node => Assert.False(node.HasIn || node.HasOut));
    }

    [Fact]
    public void Union_DisjointSquares_GivesTwoSubPaths()
    {
        var result = Run(PathOperationType.Union, Square(0, 0, 10), Square(30, 0, 10));

        Assert.Equal(2, result.Count);
        Assert.Equal(200, Area(result), 3);
    }

    [Fact]
    public void Union_SquaresSharingAnEdge_AreMerged()
    {
        var result = Run(PathOperationType.Union, Square(0, 0, 10), Square(10, 0, 10));

        Assert.Single(result);
        Assert.Equal(200, Area(result), 3);
    }

    [Fact]
    public void Difference_SquareInsideSquare_LeavesAHole()
    {
        var result = Run(PathOperationType.Difference, Square(0, 0, 10), Square(3, 3, 4));

        Assert.Equal(2, result.Count);
        Assert.Equal(84, Area(result), 3);
    }

    [Fact]
    public void Difference_CoveredBottomOperand_IsEmpty()
    {
        Assert.Empty(Run(PathOperationType.Difference, Square(2, 2, 4), Square(0, 0, 10)));
    }

    [Fact]
    public void Intersection_OverlappingSquares_GivesTheOverlap()
    {
        var result = Run(PathOperationType.Intersection, Square(0, 0, 10), Square(5, 5, 10));

        Assert.Single(result);
        Assert.Equal(25, Area(result), 3);
    }

    [Fact]
    public void Intersection_SeparateSquares_IsEmpty()
    {
        Assert.Empty(Run(PathOperationType.Intersection, Square(0, 0, 10), Square(30, 0, 10)));
    }

    [Fact]
    public void Exclusion_OverlappingSquares_RemovesTheOverlap()
    {
        var result = Run(PathOperationType.Exclusion, Square(0, 0, 10), Square(5, 5, 10));

        Assert.Equal(150, Area(result), 3);
    }

    [Fact]
    public void Difference_ThreeOperands_CutsEveryUpperOneOutOfTheBottomOne()
    {
        var result = Run(PathOperationType.Difference, Square(0, 0, 100), Square(10, 10, 10), Square(50, 50, 10));

        Assert.Equal(3, result.Count);
        Assert.Equal(10000 - 200, Area(result), 3);
    }

    [Fact]
    public void Exclusion_ThreeIdenticalSquares_KeepsTheAreaCoveredAnOddNumberOfTimes()
    {
        var result = Run(PathOperationType.Exclusion, Square(0, 0, 10), Square(0, 0, 10), Square(0, 0, 10));

        Assert.Equal(100, Area(result), 3);
    }

    [Fact]
    public void Exclusion_TwoIdenticalSquares_IsEmpty()
    {
        Assert.Empty(Run(PathOperationType.Exclusion, Square(0, 0, 10), Square(0, 0, 10)));
    }

    [Fact]
    public void Union_OperandWithEvenOddHole_KeepsTheHoleWhereNothingFillsIt()
    {
        var ring = PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100), new DocumentNode(0, 100)], true),
                new DocumentSubPath([new DocumentNode(25, 25), new DocumentNode(75, 25), new DocumentNode(75, 75), new DocumentNode(25, 75)], true),
            ],
            FillRule.EvenOdd);

        var result = Run(PathOperationType.Union, FromShape(ring), Square(40, 40, 20));

        Assert.Equal(100 * 100 - 50 * 50 + 20 * 20, Area(result), 3);
    }

    [Fact]
    public void Intersection_RotatedSquareWithItsUnrotatedSelf_GivesARegularOctagon()
    {
        var rotated = new RectangleShape { X = 0, Y = 0, Width = 50, Height = 50, Rotation = 45 };

        var result = Run(PathOperationType.Intersection, Square(0, 0, 50), FromShape(rotated));

        Assert.Single(result);
        Assert.Equal(8, result[0].Nodes.Count);
        Assert.Equal(2 * (Math.Sqrt(2) - 1) * 50 * 50, Area(result), 2);
    }

    [Fact]
    public void Intersection_CircleWithHalfPlaneSquare_KeepsTheCurvesAsFewNodes()
    {
        // The right half of a circle of radius 50 around (50, 50).
        var result = Run(PathOperationType.Intersection, Circle(50, 50, 50), Square(50, 0, 100));

        var subPath = Assert.Single(result);
        Assert.InRange(subPath.Nodes.Count, 3, 6);
        Assert.Contains(subPath.Nodes, node => node.HasIn || node.HasOut);
        Assert.Equal(Math.PI * 50 * 50 / 2, Area(result), 10.0);
    }

    [Fact]
    public void Union_TwoSeparateCircles_KeepsEachAsItsFourOriginalNodes()
    {
        var result = Run(PathOperationType.Union, Circle(0, 0, 20), Circle(100, 0, 20));

        Assert.Equal(2, result.Count);
        Assert.All(result, subPath =>
        {
            Assert.Equal(4, subPath.Nodes.Count);
            Assert.All(subPath.Nodes, node => Assert.True(node.HasIn && node.HasOut));
        });
    }

    [Fact]
    public void Difference_CircleInsideCircle_GivesTwoCurvedSubPathsWithTheRingArea()
    {
        var result = Run(PathOperationType.Difference, Circle(0, 0, 50), Circle(0, 0, 20));

        Assert.Equal(2, result.Count);
        Assert.All(result, subPath => Assert.Equal(4, subPath.Nodes.Count));
        Assert.Equal(Math.PI * (50 * 50 - 20 * 20), Area(result), 10.0);
    }

    [Fact]
    public void Difference_CrescentOutlineStaysWithinTheToleranceOfTheTwoCircles()
    {
        var result = Run(PathOperationType.Difference, Circle(0, 0, 50), Circle(30, 0, 40));

        var subPath = Assert.Single(result);
        var segments = PathShape.BuildSegments(subPath.Nodes, subPath.IsClosed);
        foreach (var point in Flattening.Flatten(segments, true, 0, 0, 0.01))
        {
            var first = Math.Abs(Math.Sqrt(point.X * point.X + point.Y * point.Y) - 50);
            var second = Math.Abs(Math.Sqrt(Math.Pow(point.X - 30, 2) + point.Y * point.Y) - 40);
            Assert.True(Math.Min(first, second) <= BooleanEngine.Tolerance + 0.1, $"({point.X}, {point.Y}) is {Math.Min(first, second)} away");
        }

        Assert.True(subPath.Nodes.Count < 12);
    }

    [Fact]
    public void Compute_SameInputTwice_GivesEqualResults()
    {
        var first = Run(PathOperationType.Union, Circle(0, 0, 40), Square(10, 10, 60));
        var second = Run(PathOperationType.Union, Circle(0, 0, 40), Square(10, 10, 60));

        Assert.Equal(first.Count, second.Count);
        for (var i = 0; i < first.Count; i++)
        {
            Assert.Equal(first[i].Nodes, second[i].Nodes);
        }
    }

    [Fact]
    public void Compute_CancelledToken_Throws()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        Assert.Throws<OperationCanceledException>(() => BooleanEngine.Compute(PathOperationType.Union, [Square(0, 0, 1), Square(1, 1, 1)], source.Token));
    }

    [Fact]
    public void Compute_FewerThanTwoOperands_Throws()
    {
        Assert.Throws<ArgumentException>(() => BooleanEngine.Compute(PathOperationType.Union, [Square(0, 0, 1)]));
        Assert.Throws<ArgumentNullException>(() => BooleanEngine.Compute(PathOperationType.Union, null!));
    }

    [Fact]
    public void Compute_UndefinedOperation_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BooleanEngine.Compute((PathOperationType)99, [Square(0, 0, 10), Square(5, 5, 10)]));
    }
}
