using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class CompoundPathShapeTests
{
    private static DocumentSubPath Square(double x, double y, double size, bool closed = true) =>
        new(
            [
                new DocumentNode(x, y),
                new DocumentNode(x + size, y),
                new DocumentNode(x + size, y + size),
                new DocumentNode(x, y + size),
            ],
            closed);

    // A big square with a smaller one inside it.
    private static PathShape Ring(FillRule fillRule = FillRule.EvenOdd) =>
        PathShape.Create([Square(0, 0, 100), Square(25, 25, 50)], fillRule) with { Appearance = new Appearance { Fill = Color.FromRgb(255, 0, 0) } };

    [Fact]
    public void Create_TwoSubPaths_BoxIsTheBoundsOfBoth()
    {
        var path = PathShape.Create([Square(10, 20, 10), Square(50, 60, 20)]);

        Assert.Equal(2, path.SubPaths.Length);
        Assert.Equal(10, path.X, 9);
        Assert.Equal(20, path.Y, 9);
        Assert.Equal(60, path.Width, 9);
        Assert.Equal(60, path.Height, 9);
    }

    [Fact]
    public void Create_TwoSubPaths_StoresFractionsOfTheCommonBox()
    {
        var path = PathShape.Create([Square(0, 0, 50), Square(50, 50, 50)]);

        Assert.Equal(new PathNode(0, 0), path.SubPaths[0].Nodes[0]);
        Assert.Equal(new PathNode(0.5, 0.5), path.SubPaths[0].Nodes[2]);
        Assert.Equal(new PathNode(0.5, 0.5), path.SubPaths[1].Nodes[0]);
        Assert.Equal(new PathNode(1, 1), path.SubPaths[1].Nodes[2]);
    }

    [Fact]
    public void Create_EachSubPathKeepsItsOwnClosedFlag()
    {
        var path = PathShape.Create([Square(0, 0, 10), Square(20, 0, 10, closed: false)]);

        Assert.True(path.SubPaths[0].IsClosed);
        Assert.False(path.SubPaths[1].IsClosed);
    }

    [Fact]
    public void Create_NoSubPaths_Throws()
    {
        Assert.Throws<ArgumentException>(() => PathShape.Create(Array.Empty<DocumentSubPath>()));
    }

    [Fact]
    public void SubPaths_TooMany_Throws()
    {
        var one = new SubPath([new PathNode(0, 0), new PathNode(1, 1)]);

        Assert.Throws<ArgumentException>(() => new PathShape { SubPaths = [.. Enumerable.Repeat(one, PathShape.MaxSubPaths + 1)] });
    }

    [Fact]
    public void SubPath_OneNode_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SubPath([new PathNode(0, 0)]));
        Assert.Throws<ArgumentException>(() => new DocumentSubPath([new DocumentNode(0, 0)]));
    }

    [Fact]
    public void ToDocumentSubPaths_AfterCreate_GivesTheSameNodes()
    {
        var input = new[] { Square(10, 20, 30), Square(100, 200, 40, closed: false) };

        var restored = PathShape.Create(input).ToDocumentSubPaths();

        Assert.Equal(2, restored.Count);
        for (var i = 0; i < input.Length; i++)
        {
            Assert.Equal(input[i].IsClosed, restored[i].IsClosed);
            Assert.Equal(input[i].Nodes.Select(n => (n.X, n.Y)), restored[i].Nodes.Select(n => (Math.Round(n.X, 9), Math.Round(n.Y, 9))));
        }
    }

    [Fact]
    public void NodeCountAndSegmentCount_CountAllSubPaths()
    {
        var path = PathShape.Create([Square(0, 0, 10), Square(20, 0, 10, closed: false)]);

        Assert.Equal(8, path.NodeCount);
        Assert.Equal(4 + 3, path.SegmentCount);
        Assert.Equal(7, path.Segments().Count);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(3, 0, 3)]
    [InlineData(4, 1, 0)]
    [InlineData(7, 1, 3)]
    public void Locate_FlatIndex_GivesSubPathAndNode(int flat, int subPath, int node)
    {
        var path = PathShape.Create([Square(0, 0, 10), Square(20, 0, 10, closed: false)]);

        Assert.Equal((subPath, node), path.Locate(flat));
    }

    [Fact]
    public void Locate_OutOfRange_Throws()
    {
        var path = PathShape.Create([Square(0, 0, 10), Square(20, 0, 10)]);

        Assert.Throws<ArgumentOutOfRangeException>(() => path.Locate(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => path.Locate(8));
    }

    [Theory]
    [InlineData(3, 0, 3)]
    [InlineData(4, 1, 0)]
    [InlineData(6, 1, 2)]
    public void LocateSegment_FlatIndex_GivesSubPathAndSegment(int flat, int subPath, int segment)
    {
        // The first sub-path is closed (4 segments), the second is open (3 segments).
        var path = PathShape.Create([Square(0, 0, 10), Square(20, 0, 10, closed: false)]);

        Assert.Equal((subPath, segment), path.LocateSegment(flat));
        Assert.Throws<ArgumentOutOfRangeException>(() => path.LocateSegment(7));
    }

    [Fact]
    public void FirstNodeIndexAndFirstSegmentIndex_OfSecondSubPath_SkipTheFirst()
    {
        var path = PathShape.Create([Square(0, 0, 10), Square(20, 0, 10, closed: false)]);

        Assert.Equal(4, path.FirstNodeIndex(1));
        Assert.Equal(4, path.FirstSegmentIndex(1));
    }

    [Fact]
    public void Contains_RingWithEvenOdd_HoleIsNotHit()
    {
        var ring = Ring();

        Assert.True(ring.Contains(10, 50));
        Assert.False(ring.Contains(50, 50));
    }

    [Fact]
    public void Contains_TwoSquaresWithNonZeroSameWinding_InnerIsHit()
    {
        var path = Ring(FillRule.NonZero);

        Assert.True(path.Contains(50, 50));
    }

    [Fact]
    public void Contains_OpenSubPathOfFilledPath_IsFilledLikeClosed()
    {
        var path = PathShape.Create([Square(0, 0, 100, closed: false)]) with { Appearance = new Appearance { Fill = Color.FromRgb(0, 0, 255) } };

        Assert.True(path.Contains(50, 50));
    }

    [Fact]
    public void Contains_ClosedWithoutFillAndStroke_InsideIsHitButOutlineSubPathNeedsAStroke()
    {
        var path = PathShape.Create([Square(0, 0, 100), Square(200, 0, 10, closed: false)]);

        Assert.True(path.Contains(50, 50));
        Assert.True(path.Contains(205, 0));
    }

    [Fact]
    public void Rebuilt_RotatedCompoundPath_KeepsUnmovedNodesOnTheScreen()
    {
        var path = PathShape.Create([Square(0, 0, 100), Square(200, 0, 50)]) with { Rotation = 30 };
        var before = path.SubPaths[1].Nodes.Select((_, i) => OnScreen(path, 1, i)).ToList();

        // Move one node of the first sub-path, which changes the box.
        var subPaths = path.ToDocumentSubPaths().ToList();
        var moved = subPaths[0].Nodes.ToList();
        moved[2] = new DocumentNode(moved[2].X - 40, moved[2].Y + 70);
        subPaths[0] = new DocumentSubPath(moved, true);
        var rebuilt = path.Rebuilt(subPaths, path.FillRule);

        for (var i = 0; i < before.Count; i++)
        {
            var after = OnScreen(rebuilt, 1, i);
            Assert.Equal(before[i].X, after.X, 6);
            Assert.Equal(before[i].Y, after.Y, 6);
        }
    }

    [Fact]
    public void Rebuilt_OneSubPathOverload_KeepsTheOtherData()
    {
        var path = Ring();

        var rebuilt = path.Rebuilt([new DocumentNode(0, 0), new DocumentNode(5, 5)], false, FillRule.NonZero);

        Assert.Single(rebuilt.SubPaths);
        Assert.Equal(path.Id, rebuilt.Id);
        Assert.Equal(path.Appearance, rebuilt.Appearance);
        Assert.Equal(FillRule.NonZero, rebuilt.FillRule);
    }

    [Fact]
    public void WithRotationApplied_NoRotation_ReturnsTheSameInstance()
    {
        var path = Ring();

        Assert.Same(path, path.WithRotationApplied());
    }

    [Fact]
    public void WithRotationApplied_Rotated_KeepsIdAppearanceAndPlacesNodesOnScreen()
    {
        var path = Ring() with { Rotation = 90 };

        var applied = path.WithRotationApplied();

        Assert.Equal(0, applied.Rotation);
        Assert.Equal(path.Id, applied.Id);
        Assert.Equal(path.Appearance, applied.Appearance);
        Assert.Equal(path.SubPaths.Length, applied.SubPaths.Length);
        for (var sub = 0; sub < path.SubPaths.Length; sub++)
        {
            for (var node = 0; node < path.SubPaths[sub].Nodes.Length; node++)
            {
                var expected = OnScreen(path, sub, node);
                var actual = OnScreen(applied, sub, node);
                Assert.Equal(expected.X, actual.X, 6);
                Assert.Equal(expected.Y, actual.Y, 6);
            }
        }
    }

    private static (double X, double Y) OnScreen(PathShape path, int subPath, int node)
    {
        var local = path.ToDocumentSubPaths()[subPath].Nodes[node];
        return path.FromLocal(local.X, local.Y);
    }
}
