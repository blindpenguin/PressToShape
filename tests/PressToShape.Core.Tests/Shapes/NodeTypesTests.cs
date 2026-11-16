using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class NodeTypesTests
{
    private static DocumentNode Node(double? inX, double? inY, double? outX, double? outY, NodeType type = NodeType.Corner) =>
        new(10, 10, inX, inY, outX, outY, type);

    [Fact]
    public void Derive_NoHandles_IsCorner()
    {
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(new DocumentNode(10, 10)));
    }

    [Fact]
    public void Derive_OneHandle_IsCorner()
    {
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(null, null, 20, 10)));
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(0, 10, null, null)));
    }

    [Fact]
    public void Derive_OppositeAndEqual_IsSymmetric()
    {
        Assert.Equal(NodeType.Symmetric, NodeTypes.Derive(Node(0, 10, 20, 10)));
        Assert.Equal(NodeType.Symmetric, NodeTypes.Derive(Node(7, 7, 13, 13)));
    }

    [Fact]
    public void Derive_OppositeAndUnequal_IsSmooth()
    {
        Assert.Equal(NodeType.Smooth, NodeTypes.Derive(Node(8, 10, 25, 10)));
    }

    [Fact]
    public void Derive_SameSide_IsCorner()
    {
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(20, 10, 30, 10)));
    }

    [Fact]
    public void Derive_AtAnAngle_IsCorner()
    {
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(0, 10, 20, 15)));
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(10, 0, 20, 10)));
    }

    [Fact]
    public void Derive_ZeroLengthHandle_IsCorner()
    {
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(10, 10, 20, 10)));
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(10, 10, 10, 10)));
    }

    [Fact]
    public void Derive_FloatingPointNoise_StillCountsAsOnOneLine()
    {
        var node = Node(0, 10 + 1e-10, 20, 10);

        Assert.Equal(NodeType.Symmetric, NodeTypes.Derive(node));
    }

    [Fact]
    public void Derive_SlightlyOffTheLine_IsCorner()
    {
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(0, 10.5, 20, 10)));
    }

    [Fact]
    public void Derive_NodeOfTheStoredKind_IgnoresTheStoredType()
    {
        Assert.Equal(NodeType.Symmetric, NodeTypes.Derive(Node(0, 10, 20, 10, NodeType.Corner)));
        Assert.Equal(NodeType.Corner, NodeTypes.Derive(Node(20, 10, 30, 10, NodeType.Smooth)));
    }

    [Fact]
    public void Derive_PathNode_GivesTheSameAnswer()
    {
        Assert.Equal(NodeType.Symmetric, NodeTypes.Derive(new PathNode(0.5, 0.5, 0.25, 0.5, 0.75, 0.5)));
        Assert.Equal(NodeType.Smooth, NodeTypes.Derive(new PathNode(0.5, 0.5, 0.4, 0.5, 0.9, 0.5)));
    }

    [Theory]
    [InlineData(NodeType.Corner)]
    [InlineData(NodeType.Smooth)]
    [InlineData(NodeType.Symmetric)]
    public void Satisfies_NodesWithAtMostOneHandle_IsTrueForEveryType(NodeType type)
    {
        Assert.True(NodeTypes.Satisfies(Node(null, null, null, null, type)));
        Assert.True(NodeTypes.Satisfies(Node(null, null, 20, 30, type)));
    }

    [Fact]
    public void Satisfies_Corner_IsAlwaysTrue()
    {
        Assert.True(NodeTypes.Satisfies(Node(20, 10, 30, 40, NodeType.Corner)));
    }

    [Fact]
    public void Satisfies_Smooth_NeedsOppositeHandlesOnOneLine()
    {
        Assert.True(NodeTypes.Satisfies(Node(8, 10, 25, 10, NodeType.Smooth)));
        Assert.False(NodeTypes.Satisfies(Node(20, 10, 25, 10, NodeType.Smooth)));
        Assert.False(NodeTypes.Satisfies(Node(0, 10, 20, 15, NodeType.Smooth)));
    }

    [Fact]
    public void Satisfies_Symmetric_AlsoNeedsEqualLengths()
    {
        Assert.True(NodeTypes.Satisfies(Node(0, 10, 20, 10, NodeType.Symmetric)));
        Assert.False(NodeTypes.Satisfies(Node(8, 10, 25, 10, NodeType.Symmetric)));
        Assert.False(NodeTypes.Satisfies(Node(0, 10, 20, 15, NodeType.Symmetric)));
    }

    [Fact]
    public void Satisfies_ZeroLengthHandle_OnlyTheLengthsCanDisagree()
    {
        Assert.True(NodeTypes.Satisfies(Node(10, 10, 20, 10, NodeType.Smooth)));
        Assert.False(NodeTypes.Satisfies(Node(10, 10, 20, 10, NodeType.Symmetric)));
        Assert.True(NodeTypes.Satisfies(Node(10, 10, 10, 10, NodeType.Symmetric)));
    }

    [Fact]
    public void Satisfies_PathNode_UsesItsStoredType()
    {
        Assert.True(NodeTypes.Satisfies(new PathNode(0.5, 0.5, 0.25, 0.5, 0.75, 0.5, NodeType.Symmetric)));
        Assert.False(NodeTypes.Satisfies(new PathNode(0.5, 0.5, 0.4, 0.5, 0.9, 0.5, NodeType.Symmetric)));
    }
}
