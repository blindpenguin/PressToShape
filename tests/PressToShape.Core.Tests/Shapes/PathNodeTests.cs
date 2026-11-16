using System;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathNodeTests
{
    [Fact]
    public void Create_AnchorOnly_HasNoHandles()
    {
        var node = new PathNode(0.25, 0.75);

        Assert.Equal(0.25, node.X);
        Assert.Equal(0.75, node.Y);
        Assert.False(node.HasIn);
        Assert.False(node.HasOut);
    }

    [Fact]
    public void Create_WithBothHandles_KeepsThem()
    {
        var node = new PathNode(0.5, 0.5, -0.25, 0.5, 1.25, 0.5);

        Assert.True(node.HasIn);
        Assert.True(node.HasOut);
        Assert.Equal(-0.25, node.InX);
        Assert.Equal(0.5, node.InY);
        Assert.Equal(1.25, node.OutX);
        Assert.Equal(0.5, node.OutY);
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.PositiveInfinity)]
    public void Create_NonFiniteAnchor_Throws(double x, double y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PathNode(x, y));
    }

    [Fact]
    public void Create_NonFiniteHandle_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PathNode(0, 0, outX: double.NaN, outY: 0));
    }

    [Fact]
    public void Create_HalfAHandle_Throws()
    {
        Assert.Throws<ArgumentException>(() => new PathNode(0, 0, inX: 0.5));
        Assert.Throws<ArgumentException>(() => new PathNode(0, 0, outY: 0.5));
    }

    [Fact]
    public void Equals_SameValues_AreEqual()
    {
        Assert.Equal(new PathNode(0.1, 0.2, outX: 0.3, outY: 0.4), new PathNode(0.1, 0.2, outX: 0.3, outY: 0.4));
        Assert.NotEqual(new PathNode(0.1, 0.2), new PathNode(0.1, 0.2, outX: 0.3, outY: 0.4));
    }

    [Fact]
    public void DocumentNode_ValidAndInvalidValues_FollowTheSameRules()
    {
        var node = new DocumentNode(10, 20, 5, 20, 15, 20);

        Assert.True(node.HasIn);
        Assert.True(node.HasOut);
        Assert.Equal(15, node.OutX);
        Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentNode(double.NaN, 0));
        Assert.Throws<ArgumentException>(() => new DocumentNode(0, 0, inY: 1));
    }

    [Fact]
    public void FillRule_Default_IsNonZero()
    {
        Assert.Equal(FillRule.NonZero, default(FillRule));
    }

    [Fact]
    public void Type_NotGiven_IsCorner()
    {
        Assert.Equal(NodeType.Corner, new PathNode(0, 0).Type);
        Assert.Equal(NodeType.Corner, new DocumentNode(0, 0).Type);
        Assert.Equal(NodeType.Corner, default(PathNode).Type);
    }

    [Theory]
    [InlineData(NodeType.Corner)]
    [InlineData(NodeType.Smooth)]
    [InlineData(NodeType.Symmetric)]
    public void Type_Given_IsKept(NodeType type)
    {
        var node = new PathNode(0.5, 0.5, 0.25, 0.5, 0.75, 0.5, type);
        var documentNode = new DocumentNode(5, 5, 2.5, 5, 7.5, 5, type);

        Assert.Equal(type, node.Type);
        Assert.Equal(type, documentNode.Type);
    }

    [Fact]
    public void Type_NotDefined_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PathNode(0, 0, type: (NodeType)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentNode(0, 0, type: (NodeType)(-1)));
    }

    [Fact]
    public void Equals_NodesThatDifferOnlyInType_AreNotEqual()
    {
        var corner = new PathNode(0.1, 0.2, 0.0, 0.2, 0.2, 0.2);
        var smooth = new PathNode(0.1, 0.2, 0.0, 0.2, 0.2, 0.2, NodeType.Smooth);

        Assert.NotEqual(corner, smooth);
        Assert.Equal(smooth, new PathNode(0.1, 0.2, 0.0, 0.2, 0.2, 0.2, NodeType.Smooth));
    }
}
