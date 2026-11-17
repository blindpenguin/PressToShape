using System;
using System.Linq;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class CompoundPathEditingTests
{
    // Sub-path 0: closed square 0..100 (nodes 0 to 3). Sub-path 1: open line 200,0 to 300,0 with a third node (nodes 4 to 6).
    private static PathShape Sample() =>
        PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100), new DocumentNode(0, 100)], true),
                new DocumentSubPath([new DocumentNode(200, 0), new DocumentNode(250, 50), new DocumentNode(300, 0)], false),
            ]);

    private static (double X, double Y) OnScreen(PathShape path, int flatIndex) => PathNodeEditing.NodePosition(path, flatIndex);

    [Fact]
    public void MoveNodes_NodeOfSecondSubPath_MovesOnlyThatNode()
    {
        var path = Sample();

        var moved = PathNodeEditing.MoveNodes(path, [5], 10, 20);

        Assert.Equal(path.SubPaths.Length, moved.SubPaths.Length);
        Assert.Equal(260, OnScreen(moved, 5).X, 6);
        Assert.Equal(70, OnScreen(moved, 5).Y, 6);
        foreach (var index in new[] { 0, 1, 2, 3, 4, 6 })
        {
            Assert.Equal(OnScreen(path, index).X, OnScreen(moved, index).X, 6);
            Assert.Equal(OnScreen(path, index).Y, OnScreen(moved, index).Y, 6);
        }
    }

    [Fact]
    public void MoveNodes_NodesOfTwoSubPaths_MovesBoth()
    {
        var moved = PathNodeEditing.MoveNodes(Sample(), [0, 4], 5, 5);

        Assert.Equal(5, OnScreen(moved, 0).X, 6);
        Assert.Equal(205, OnScreen(moved, 4).X, 6);
    }

    [Fact]
    public void MoveNodes_RotatedCompoundPath_KeepsTheOthersOnTheScreen()
    {
        var path = Sample() with { Rotation = 40 };

        var moved = PathNodeEditing.MoveNodes(path, [5], 0, -80);

        foreach (var index in new[] { 0, 1, 2, 3, 4, 6 })
        {
            Assert.Equal(OnScreen(path, index).X, OnScreen(moved, index).X, 6);
            Assert.Equal(OnScreen(path, index).Y, OnScreen(moved, index).Y, 6);
        }
    }

    [Fact]
    public void MoveNodes_IndexOutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.MoveNodes(Sample(), [7], 1, 1));
    }

    [Fact]
    public void InsertNode_OnSegmentOfSecondSubPath_AddsThereAndLeavesTheFirstAlone()
    {
        var path = Sample();

        // Flat segment 4 is the first segment of the open sub-path.
        var result = PathEditing.InsertNode(path, 4, 0.5);

        Assert.Equal(4, result.SubPaths[0].Nodes.Length);
        Assert.Equal(4, result.SubPaths[1].Nodes.Length);
        Assert.Equal(225, OnScreen(result, 5).X, 6);
        Assert.Equal(25, OnScreen(result, 5).Y, 6);
    }

    [Fact]
    public void InsertNode_OnClosingSegmentOfFirstSubPath_AppendsToIt()
    {
        var result = PathEditing.InsertNode(Sample(), 3, 0.5);

        Assert.Equal(5, result.SubPaths[0].Nodes.Length);
        Assert.Equal(0, OnScreen(result, 4).X, 6);
        Assert.Equal(50, OnScreen(result, 4).Y, 6);
    }

    [Fact]
    public void InsertNode_SegmentOutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PathEditing.InsertNode(Sample(), 6, 0.5));
    }

    [Fact]
    public void BendSegment_SegmentOfSecondSubPath_BendsThatSegment()
    {
        var path = Sample();

        var bent = PathNodeEditing.BendSegment(path, 4, 0.5, 0, -30);

        Assert.True(bent.SubPaths[1].Nodes[0].HasOut || bent.SubPaths[1].Nodes[1].HasIn);
        Assert.False(bent.SubPaths[0].Nodes.Any(n => n.HasIn || n.HasOut));
    }

    [Fact]
    public void StraightenAndCurveSegment_SecondSubPath_Work()
    {
        var curved = PathNodeEditing.CurveSegment(Sample(), 5);

        Assert.True(curved.SubPaths[1].Nodes[1].HasOut);

        var straight = PathNodeEditing.StraightenSegment(curved, 5);

        Assert.False(straight.SubPaths[1].Nodes[1].HasOut);
    }

    [Fact]
    public void SetNodeType_NodeOfSecondSubPath_AlignsUsingItsOwnNeighbors()
    {
        var result = PathNodeEditing.SetNodeType(Sample(), [5], NodeType.Smooth);

        var node = result.SubPaths[1].Nodes[1];
        Assert.Equal(NodeType.Smooth, node.Type);
        Assert.True(node.HasIn && node.HasOut);
        Assert.Equal(NodeType.Corner, result.SubPaths[0].Nodes[0].Type);
    }

    [Fact]
    public void RemoveNode_NodeOfFirstSubPath_KeepsTheSecond()
    {
        var result = PathEditing.RemoveNode(Sample(), 1);

        Assert.NotNull(result);
        Assert.Equal(3, result.SubPaths[0].Nodes.Length);
        Assert.Equal(3, result.SubPaths[1].Nodes.Length);
    }

    [Fact]
    public void RemoveNode_LeavesASubPathWithOneNode_DropsThatSubPath()
    {
        var path = PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(10, 0), new DocumentNode(10, 10)], true),
                new DocumentSubPath([new DocumentNode(50, 0), new DocumentNode(60, 0)], false),
            ]);

        var result = PathEditing.RemoveNode(path, 3);

        Assert.NotNull(result);
        Assert.Single(result.SubPaths);
        Assert.Equal(3, result.SubPaths[0].Nodes.Length);
    }

    [Fact]
    public void RemoveNodes_OneNodeOfTwoNodeSubPaths_DropsThatSubPathAndNullWhenNoneLeft()
    {
        var path = PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(10, 0)], false),
                new DocumentSubPath([new DocumentNode(50, 0), new DocumentNode(60, 0)], false),
            ]);

        var onlyFirstGone = PathNodeEditing.RemoveNodes(path, [0]);

        Assert.NotNull(onlyFirstGone);
        Assert.Single(onlyFirstGone.SubPaths);
        Assert.Null(PathNodeEditing.RemoveNodes(path, [0, 2]));
    }

    [Fact]
    public void RemoveNodes_SomeNodesOfTwoSubPaths_RemovesEach()
    {
        var result = PathNodeEditing.RemoveNodes(Sample(), [1, 5]);

        Assert.NotNull(result);
        Assert.Equal(3, result.SubPaths[0].Nodes.Length);
        Assert.Single(result.SubPaths.Skip(1));
        Assert.Equal(2, result.SubPaths[1].Nodes.Length);
    }

    [Fact]
    public void Continue_LastNodeOfSecondSubPath_ReturnsItsNodesAsTheyAre()
    {
        var nodes = PathEditing.Continue(Sample(), 6);

        Assert.Equal(3, nodes.Count);
        Assert.Equal(300, nodes[2].X, 6);
    }

    [Fact]
    public void Continue_FirstNodeOfSecondSubPath_ReturnsThemReversed()
    {
        var nodes = PathEditing.Continue(Sample(), 4);

        Assert.Equal(3, nodes.Count);
        Assert.Equal(200, nodes[2].X, 6);
        Assert.Equal(300, nodes[0].X, 6);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Continue_NotAnEndOfAnOpenSubPath_Throws(int flatIndex)
    {
        Assert.Throws<ArgumentException>(() => PathEditing.Continue(Sample(), flatIndex));
    }

    [Fact]
    public void ReplaceSubPath_ContinuedSecondSubPath_LeavesTheFirstOnScreen()
    {
        var path = Sample() with { Rotation = 25 };
        var nodes = PathEditing.Continue(path, 6).ToList();
        nodes.Add(new DocumentNode(400, 400));

        var result = PathEditing.ReplaceSubPath(path, 1, nodes, false);

        Assert.Equal(4, result.SubPaths[1].Nodes.Length);
        Assert.Equal(4, result.SubPaths[0].Nodes.Length);
        foreach (var index in new[] { 0, 1, 2, 3 })
        {
            Assert.Equal(OnScreen(path, index).X, OnScreen(result, index).X, 6);
            Assert.Equal(OnScreen(path, index).Y, OnScreen(result, index).Y, 6);
        }
    }
}
