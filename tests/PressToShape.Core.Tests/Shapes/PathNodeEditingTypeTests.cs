using System;
using System.Linq;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathNodeEditingTypeTests
{
    private static double Length(double ax, double ay, double bx, double by) => Math.Sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));

    private static DocumentNode WithHandles(NodeType type = NodeType.Corner) =>
        new(100, 100, inX: 80, inY: 90, outX: 140, outY: 100, type: type);

    private static PathShape Around(DocumentNode middle) =>
        PathShape.Create([new DocumentNode(0, 100), middle, new DocumentNode(200, 100)]);

    private static DocumentNode Middle(PathShape path) => path.ToDocumentNodes()[1];

    private static (double X, double Y) OnScreen(PathShape path, int index)
    {
        var node = path.ToDocumentNodes()[index];
        return path.FromLocal(node.X, node.Y);
    }

    [Fact]
    public void SetNodeType_ToCorner_OnlyChangesTheType()
    {
        var path = Around(WithHandles(NodeType.Symmetric));

        var result = Middle(PathNodeEditing.SetNodeType(path, [1], NodeType.Corner));

        Assert.Equal(NodeType.Corner, result.Type);
        Assert.Equal((80, 90), (result.InX!.Value, result.InY!.Value));
        Assert.Equal((140, 100), (result.OutX!.Value, result.OutY!.Value));
    }

    [Fact]
    public void SetNodeType_ToSmoothWithBothHandles_TheOutDirectionWinsAndTheInHandleKeepsItsLength()
    {
        var path = Around(WithHandles());
        var inLength = Length(80, 90, 100, 100);

        var result = Middle(PathNodeEditing.SetNodeType(path, [1], NodeType.Smooth));

        Assert.Equal(NodeType.Smooth, result.Type);
        Assert.Equal((140, 100), (result.OutX!.Value, result.OutY!.Value));
        Assert.Equal(inLength, Length(result.InX!.Value, result.InY!.Value, result.X, result.Y), 6);
        Assert.Equal(100 - inLength, result.InX!.Value, 6);
        Assert.Equal(100, result.InY!.Value, 6);
        Assert.True(NodeTypes.Satisfies(result));
    }

    [Fact]
    public void SetNodeType_ToSymmetricWithBothHandles_TheInHandleGetsTheOutLength()
    {
        var path = Around(WithHandles());

        var result = Middle(PathNodeEditing.SetNodeType(path, [1], NodeType.Symmetric));

        Assert.Equal(60, result.InX!.Value, 6);
        Assert.Equal(100, result.InY!.Value, 6);
        Assert.True(NodeTypes.Satisfies(result));
    }

    [Fact]
    public void SetNodeType_NodeWithOnlyAnInHandle_TheOutHandleFollowsIt()
    {
        var path = Around(new DocumentNode(100, 100, inX: 70, inY: 100));

        var symmetric = Middle(PathNodeEditing.SetNodeType(path, [1], NodeType.Symmetric));
        var smooth = Middle(PathNodeEditing.SetNodeType(path, [1], NodeType.Smooth));

        Assert.Equal(130, symmetric.OutX!.Value, 6);
        Assert.Equal(100, symmetric.OutY!.Value, 6);
        // A third of the distance to the next node (100 away).
        Assert.Equal(100 + 100 / 3.0, smooth.OutX!.Value, 6);
        Assert.True(NodeTypes.Satisfies(smooth));
    }

    [Fact]
    public void SetNodeType_NodeWithOnlyAnOutHandle_TheInHandleIsACreatedThirdOfTheWayToThePreviousNodeForSmooth()
    {
        var path = Around(new DocumentNode(100, 100, outX: 150, outY: 100));

        var smooth = Middle(PathNodeEditing.SetNodeType(path, [1], NodeType.Smooth));

        Assert.Equal(100 - 100 / 3.0, smooth.InX!.Value, 6);
        Assert.Equal(100, smooth.InY!.Value, 6);
    }

    [Theory]
    [InlineData(NodeType.Smooth)]
    [InlineData(NodeType.Symmetric)]
    public void SetNodeType_NodeWithoutHandles_GetsHandlesOnTheLineBetweenItsNeighbors(NodeType type)
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)]);

        var result = PathNodeEditing.SetNodeType(path, [1], type).ToDocumentNodes()[1];

        // The neighbors are (0, 0) and (100, 100): the line between them goes along (1, 1), the neighbors are 100 away from the node.
        var third = 100 / 3.0 / Math.Sqrt(2);
        Assert.Equal(100 + third, result.OutX!.Value, 6);
        Assert.Equal(third, result.OutY!.Value, 6);
        Assert.Equal(100 - third, result.InX!.Value, 6);
        Assert.Equal(-third, result.InY!.Value, 6);
        Assert.Equal(type, result.Type);
        Assert.True(NodeTypes.Satisfies(result));
    }

    [Fact]
    public void SetNodeType_SymmetricWithNeighborsAtDifferentDistances_BothHandlesGetTheShorterThird()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(30, 0), new DocumentNode(130, 0)]);

        var result = PathNodeEditing.SetNodeType(path, [1], NodeType.Symmetric).ToDocumentNodes()[1];

        Assert.Equal(10, result.OutX!.Value - 30, 6);
        Assert.Equal(10, 30 - result.InX!.Value, 6);
    }

    [Fact]
    public void SetNodeType_SmoothWithNeighborsAtDifferentDistances_HandlesKeepTheirOwnThird()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(30, 0), new DocumentNode(130, 0)]);

        var result = PathNodeEditing.SetNodeType(path, [1], NodeType.Smooth).ToDocumentNodes()[1];

        Assert.Equal(100 / 3.0, result.OutX!.Value - 30, 6);
        Assert.Equal(10, 30 - result.InX!.Value, 6);
    }

    [Fact]
    public void SetNodeType_FirstNodeOfAnOpenPath_OnlyGetsTheOutHandle()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(90, 0), new DocumentNode(90, 90)]);

        var result = PathNodeEditing.SetNodeType(path, [0], NodeType.Smooth).ToDocumentNodes()[0];

        Assert.False(result.HasIn);
        Assert.Equal(30, result.OutX!.Value, 6);
        Assert.Equal(0, result.OutY!.Value, 6);
        Assert.Equal(NodeType.Smooth, result.Type);
    }

    [Fact]
    public void SetNodeType_LastNodeOfAnOpenPath_OnlyGetsTheInHandle()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(90, 0), new DocumentNode(90, 90)]);

        var result = PathNodeEditing.SetNodeType(path, [2], NodeType.Symmetric).ToDocumentNodes()[2];

        Assert.False(result.HasOut);
        Assert.Equal(90, result.InX!.Value, 6);
        Assert.Equal(60, result.InY!.Value, 6);
    }

    [Fact]
    public void SetNodeType_FirstNodeOfAClosedPath_UsesTheLastNodeAsItsPreviousNeighbor()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(90, 0), new DocumentNode(90, 90), new DocumentNode(0, 90)], isClosed: true);

        var result = PathNodeEditing.SetNodeType(path, [0], NodeType.Smooth).ToDocumentNodes()[0];

        Assert.True(result.HasIn);
        Assert.True(result.HasOut);
        Assert.True(NodeTypes.Satisfies(result));
        // The neighbors are (0, 90) before the node and (90, 0) after it, so the line between them goes along (1, -1), and each handle is
        // a third of the 90 to its neighbor long.
        var along = 30 / Math.Sqrt(2);
        Assert.Equal(along, result.OutX!.Value, 6);
        Assert.Equal(-along, result.OutY!.Value, 6);
        Assert.Equal(-along, result.InX!.Value, 6);
        Assert.Equal(along, result.InY!.Value, 6);
    }

    [Fact]
    public void SetNodeType_NeighborsOnTheSameSpot_CreatesNoHandleAndNoNaN()
    {
        var path = PathShape.Create([new DocumentNode(50, 50), new DocumentNode(50, 50), new DocumentNode(50, 50)]);

        var result = PathNodeEditing.SetNodeType(path, [1], NodeType.Smooth).ToDocumentNodes()[1];

        Assert.False(result.HasIn);
        Assert.False(result.HasOut);
        Assert.Equal(NodeType.Smooth, result.Type);
    }

    [Fact]
    public void SetNodeType_NeighborOnTheSpotOfTheNode_OnlyTheOtherSideGetsAHandle()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(0, 0), new DocumentNode(100, 0)]);

        var result = PathNodeEditing.SetNodeType(path, [1], NodeType.Smooth).ToDocumentNodes()[1];

        Assert.False(result.HasIn);
        Assert.True(result.HasOut);
        Assert.All(new[] { result.OutX!.Value, result.OutY!.Value }, v => Assert.True(double.IsFinite(v)));
    }

    [Fact]
    public void SetNodeType_SeveralNodes_ChangesAllOfThem()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(200, 50), new DocumentNode(300, 0)]);

        var result = PathNodeEditing.SetNodeType(path, [1, 2], NodeType.Symmetric).ToDocumentNodes();

        Assert.Equal([NodeType.Corner, NodeType.Symmetric, NodeType.Symmetric, NodeType.Corner], result.Select(n => n.Type));
        Assert.True(NodeTypes.Satisfies(result[1]));
        Assert.True(NodeTypes.Satisfies(result[2]));
    }

    [Fact]
    public void SetNodeType_NoIndices_ReturnsTheSamePath()
    {
        var path = Around(WithHandles());

        Assert.Same(path, PathNodeEditing.SetNodeType(path, [], NodeType.Smooth));
    }

    [Fact]
    public void SetNodeType_BadInput_Throws()
    {
        var path = Around(WithHandles());

        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.SetNodeType(path, [9], NodeType.Smooth));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.SetNodeType(path, [1], (NodeType)7));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void SetNodeType_RotatedPath_EveryAnchorStaysWhereItIsOnTheScreen(double rotation)
    {
        var path = Around(WithHandles()) with { Rotation = rotation };
        var before = new[] { OnScreen(path, 0), OnScreen(path, 1), OnScreen(path, 2) };

        var result = PathNodeEditing.SetNodeType(path, [1], NodeType.Symmetric);

        for (var i = 0; i < 3; i++)
        {
            var after = OnScreen(result, i);
            Assert.Equal(before[i].X, after.X, 6);
            Assert.Equal(before[i].Y, after.Y, 6);
        }
    }

    [Fact]
    public void SetNodeType_NodesNotInTheList_AreUntouched()
    {
        var path = PathShape.Create([new DocumentNode(0, 0, outX: 20, outY: 10), new DocumentNode(100, 0), new DocumentNode(200, 0)]);

        var result = PathNodeEditing.SetNodeType(path, [1], NodeType.Smooth).ToDocumentNodes();

        Assert.Equal((20, 10), (result[0].OutX!.Value, result[0].OutY!.Value));
        Assert.Equal(NodeType.Corner, result[0].Type);
    }
}
