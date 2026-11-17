using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathNodeEditingMoveTests
{
    // Three nodes in a row with a node of the given type in the middle.
    private static PathShape Path(NodeType middle, double rotation = 0)
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 100, outX: 30, outY: 100),
                new DocumentNode(100, 100, inX: 80, inY: 100, outX: 140, outY: 100, type: middle),
                new DocumentNode(200, 100, inX: 170, inY: 100),
            ]);
        return path with { Rotation = rotation };
    }

    private static (double X, double Y) OnScreen(PathShape path, int index)
    {
        var node = path.ToDocumentNodes()[index];
        return path.FromLocal(node.X, node.Y);
    }

    private static void AssertSame((double X, double Y) expected, (double X, double Y) actual)
    {
        Assert.Equal(expected.X, actual.X, 6);
        Assert.Equal(expected.Y, actual.Y, 6);
    }

    [Fact]
    public void MoveNodes_OneNode_MovesTheAnchorAndBothHandles()
    {
        var path = Path(NodeType.Smooth);

        var moved = PathNodeEditing.MoveNodes(path, [1], 10, -30).ToDocumentNodes()[1];

        AssertSame((110, 70), (moved.X, moved.Y));
        AssertSame((90, 70), (moved.InX!.Value, moved.InY!.Value));
        AssertSame((150, 70), (moved.OutX!.Value, moved.OutY!.Value));
        Assert.Equal(NodeType.Smooth, moved.Type);
    }

    [Fact]
    public void MoveNodes_SeveralNodes_MoveByTheSameDistanceAndOthersStay()
    {
        var path = Path(NodeType.Corner);

        var moved = PathNodeEditing.MoveNodes(path, [0, 2], 0, 25).ToDocumentNodes();

        Assert.Equal(125, moved[0].Y, 6);
        Assert.Equal(125, moved[2].Y, 6);
        Assert.Equal(100, moved[1].Y, 6);
        AssertSame((30, 125), (moved[0].OutX!.Value, moved[0].OutY!.Value));
    }

    [Fact]
    public void MoveNodes_NoIndicesOrZeroDistance_KeepsThePicture()
    {
        var path = Path(NodeType.Corner);

        Assert.Same(path, PathNodeEditing.MoveNodes(path, [], 5, 5));
        var same = PathNodeEditing.MoveNodes(path, [1], 0, 0);
        Assert.Equal(path.ToDocumentNodes().Select(n => n.X), same.ToDocumentNodes().Select(n => n.X));
    }

    [Fact]
    public void MoveNodes_RepeatedIndex_MovesTheNodeOnce()
    {
        var moved = PathNodeEditing.MoveNodes(Path(NodeType.Corner), [1, 1], 10, 0).ToDocumentNodes()[1];

        Assert.Equal(110, moved.X, 6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void MoveNodes_RotatedPath_NodesThatWereNotMovedStayWhereTheyAreOnTheScreen(double rotation)
    {
        var path = Path(NodeType.Corner, rotation);
        var before0 = OnScreen(path, 0);
        var before2 = OnScreen(path, 2);

        var moved = PathNodeEditing.MoveNodes(path, [1], 40, 60);

        AssertSame(before0, OnScreen(moved, 0));
        AssertSame(before2, OnScreen(moved, 2));
    }

    [Fact]
    public void MoveNodes_BadIndex_Throws()
    {
        var path = Path(NodeType.Corner);

        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.MoveNodes(path, [3], 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.MoveNodes(path, [-1], 1, 1));
    }

    [Fact]
    public void MoveHandle_CornerNode_MovesOnlyThatHandle()
    {
        var moved = PathNodeEditing.MoveHandle(Path(NodeType.Corner), 1, NodeHandleSide.Out, 150, 40, breakLink: false).ToDocumentNodes()[1];

        Assert.Equal((150, 40), (moved.OutX!.Value, moved.OutY!.Value));
        Assert.Equal((80, 100), (moved.InX!.Value, moved.InY!.Value));
        Assert.Equal(NodeType.Corner, moved.Type);
    }

    [Fact]
    public void MoveHandle_SmoothNode_TheOtherHandleStaysOppositeWithItsOwnLength()
    {
        // The in handle is 20 long and has to stay 20 long, now pointing against the new direction of the out handle.
        var moved = PathNodeEditing.MoveHandle(Path(NodeType.Smooth), 1, NodeHandleSide.Out, 100, 160, breakLink: false).ToDocumentNodes()[1];

        Assert.Equal((100, 160), (moved.OutX!.Value, moved.OutY!.Value));
        Assert.Equal(100, moved.InX!.Value, 6);
        Assert.Equal(80, moved.InY!.Value, 6);
        Assert.Equal(NodeType.Smooth, moved.Type);
        Assert.True(NodeTypes.Satisfies(moved));
    }

    [Fact]
    public void MoveHandle_SmoothNode_MovingTheInHandleTurnsTheOutHandle()
    {
        var moved = PathNodeEditing.MoveHandle(Path(NodeType.Smooth), 1, NodeHandleSide.In, 100, 70, breakLink: false).ToDocumentNodes()[1];

        // The out handle is 40 long and now points the other way, down.
        Assert.Equal(100, moved.OutX!.Value, 6);
        Assert.Equal(140, moved.OutY!.Value, 6);
    }

    [Fact]
    public void MoveHandle_SymmetricNode_TheOtherHandleMirrorsIt()
    {
        var moved = PathNodeEditing.MoveHandle(Path(NodeType.Symmetric), 1, NodeHandleSide.Out, 130, 140, breakLink: false).ToDocumentNodes()[1];

        Assert.Equal((70, 60), (moved.InX!.Value, moved.InY!.Value));
        Assert.Equal(NodeType.Symmetric, moved.Type);
        Assert.True(NodeTypes.Satisfies(moved));
    }

    [Fact]
    public void MoveHandle_WithBreakLink_MovesOnlyThatHandleAndMakesACorner()
    {
        var moved = PathNodeEditing.MoveHandle(Path(NodeType.Symmetric), 1, NodeHandleSide.Out, 130, 140, breakLink: true).ToDocumentNodes()[1];

        Assert.Equal((80, 100), (moved.InX!.Value, moved.InY!.Value));
        Assert.Equal((130, 140), (moved.OutX!.Value, moved.OutY!.Value));
        Assert.Equal(NodeType.Corner, moved.Type);
    }

    [Theory]
    [InlineData(NodeHandleSide.In)]
    [InlineData(NodeHandleSide.Out)]
    public void MoveHandle_OntoItsAnchor_RemovesTheHandleAndMakesACorner(NodeHandleSide side)
    {
        var moved = PathNodeEditing.MoveHandle(Path(NodeType.Symmetric), 1, side, 100, 100, breakLink: false).ToDocumentNodes()[1];

        Assert.Equal(side == NodeHandleSide.In, !moved.HasIn);
        Assert.Equal(side == NodeHandleSide.Out, !moved.HasOut);
        Assert.Equal(NodeType.Corner, moved.Type);
    }

    [Fact]
    public void MoveHandle_NodeWithOnlyOneHandle_DoesNotCreateTheOther()
    {
        var path = Path(NodeType.Smooth);

        var moved = PathNodeEditing.MoveHandle(path, 0, NodeHandleSide.Out, 30, 60, breakLink: false).ToDocumentNodes()[0];

        Assert.False(moved.HasIn);
        Assert.Equal((30, 60), (moved.OutX!.Value, moved.OutY!.Value));
    }

    [Fact]
    public void MoveHandle_NodeWithoutHandles_GetsTheHandleThatWasMoved()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)]);

        var moved = PathNodeEditing.MoveHandle(path, 1, NodeHandleSide.Out, 130, 20, breakLink: false).ToDocumentNodes()[1];

        Assert.True(moved.HasOut);
        Assert.False(moved.HasIn);
    }

    [Fact]
    public void MoveHandle_SmoothNodeWhoseOtherHandleHasNoLength_StaysUsable()
    {
        var path = PathShape.Create(
            [new DocumentNode(0, 0), new DocumentNode(100, 100, inX: 100, inY: 100, outX: 140, outY: 100, type: NodeType.Smooth), new DocumentNode(200, 0)]);

        var moved = PathNodeEditing.MoveHandle(path, 1, NodeHandleSide.Out, 100, 160, breakLink: false).ToDocumentNodes()[1];

        Assert.All(new[] { moved.X, moved.Y, moved.InX!.Value, moved.InY!.Value, moved.OutX!.Value, moved.OutY!.Value }, v => Assert.True(double.IsFinite(v)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void MoveHandle_RotatedPath_OtherNodesStayWhereTheyAreOnTheScreen(double rotation)
    {
        var path = Path(NodeType.Symmetric, rotation);
        var before0 = OnScreen(path, 0);
        var before2 = OnScreen(path, 2);
        var target = path.ToLocal(220, 40);

        var moved = PathNodeEditing.MoveHandle(path, 1, NodeHandleSide.Out, target.X, target.Y, breakLink: false);

        AssertSame(before0, OnScreen(moved, 0));
        AssertSame(before2, OnScreen(moved, 2));
    }

    [Fact]
    public void MoveHandle_BadInput_Throws()
    {
        var path = Path(NodeType.Corner);

        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.MoveHandle(path, 5, NodeHandleSide.In, 0, 0, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.MoveHandle(path, 1, NodeHandleSide.In, double.NaN, 0, false));
    }

    [Fact]
    public void RemoveNodes_Several_NeighborsConnect()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(50, 50), new DocumentNode(100, 0), new DocumentNode(150, 50), new DocumentNode(200, 0)]);

        var result = PathNodeEditing.RemoveNodes(path, [1, 3])!;

        Assert.Equal([0.0, 100.0, 200.0], result.ToDocumentNodes().Select(n => n.X));
    }

    [Fact]
    public void RemoveNodes_DownToOneNode_ReturnsNull()
    {
        var path = Path(NodeType.Corner);

        Assert.Null(PathNodeEditing.RemoveNodes(path, [0, 1]));
        Assert.Null(PathNodeEditing.RemoveNodes(path, [0, 1, 2]));
    }

    [Fact]
    public void RemoveNodes_DownToTwoNodes_KeepsThePath()
    {
        Assert.NotNull(PathNodeEditing.RemoveNodes(Path(NodeType.Corner), [1]));
    }

    [Fact]
    public void RemoveNodes_NothingToRemove_ReturnsTheSamePath()
    {
        var path = Path(NodeType.Corner);

        Assert.Same(path, PathNodeEditing.RemoveNodes(path, []));
    }

    [Fact]
    public void RemoveNodes_ClosedPath_KeepsItClosed()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100), new DocumentNode(0, 100)], isClosed: true);

        var result = PathNodeEditing.RemoveNodes(path, [3])!;

        Assert.True(result.SubPaths[0].IsClosed);
        Assert.Equal(3, result.SubPaths[0].Nodes.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void RemoveNodes_RotatedPath_KeepsTheOtherNodesOnTheScreen(double rotation)
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 20), new DocumentNode(200, 0), new DocumentNode(300, 80)]) with { Rotation = rotation };
        var before = new[] { OnScreen(path, 0), OnScreen(path, 2), OnScreen(path, 3) };

        var result = PathNodeEditing.RemoveNodes(path, [1])!;

        AssertSame(before[0], OnScreen(result, 0));
        AssertSame(before[1], OnScreen(result, 1));
        AssertSame(before[2], OnScreen(result, 2));
    }

    [Fact]
    public void RemoveNodes_BadIndex_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.RemoveNodes(Path(NodeType.Corner), [7]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void NodePosition_RotatedPath_IsTheTurnedPosition(double rotation)
    {
        var path = Path(NodeType.Corner, rotation);
        var radians = rotation * Math.PI / 180;
        var dx = 0 - path.CenterX;
        var dy = 100 - path.CenterY;

        var position = PathNodeEditing.NodePosition(path, 0);

        Assert.Equal(path.CenterX + dx * Math.Cos(radians) - dy * Math.Sin(radians), position.X, 6);
        Assert.Equal(path.CenterY + dx * Math.Sin(radians) + dy * Math.Cos(radians), position.Y, 6);
    }

    [Fact]
    public void FromLocal_IsTheInverseOfToLocal()
    {
        var path = Path(NodeType.Corner, 37);

        var screen = path.FromLocal(12, 34);
        var back = path.ToLocal(screen.X, screen.Y);

        Assert.Equal(12, back.X, 6);
        Assert.Equal(34, back.Y, 6);
    }
}
