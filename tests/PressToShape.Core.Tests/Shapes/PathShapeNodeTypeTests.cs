using System;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathShapeNodeTypeTests
{
    private static PathShape Typed() =>
        PathShape.Create(
            [
                new DocumentNode(10, 20, outX: 40, outY: 20, type: NodeType.Corner),
                new DocumentNode(110, 20, inX: 80, inY: 20, outX: 140, outY: 20, type: NodeType.Smooth),
                new DocumentNode(60, 120, inX: 70, inY: 120, outX: 50, outY: 120, type: NodeType.Symmetric),
            ]);

    private static (double X, double Y) Turn(PathShape shape, double x, double y)
    {
        var radians = shape.Rotation * Math.PI / 180;
        var dx = x - shape.CenterX;
        var dy = y - shape.CenterY;
        return (
            shape.CenterX + dx * Math.Cos(radians) - dy * Math.Sin(radians),
            shape.CenterY + dx * Math.Sin(radians) + dy * Math.Cos(radians));
    }

    [Fact]
    public void Create_NodesWithTypes_StoresTheTypes()
    {
        var path = Typed();

        Assert.Equal([NodeType.Corner, NodeType.Smooth, NodeType.Symmetric], [path.Nodes[0].Type, path.Nodes[1].Type, path.Nodes[2].Type]);
    }

    [Fact]
    public void ToDocumentNodes_AfterCreate_ReturnsTheTypes()
    {
        var nodes = Typed().ToDocumentNodes();

        Assert.Equal([NodeType.Corner, NodeType.Smooth, NodeType.Symmetric], [nodes[0].Type, nodes[1].Type, nodes[2].Type]);
    }

    [Fact]
    public void With_ResizedBox_KeepsTheTypes()
    {
        var path = Typed();

        var wider = path with { Width = path.Width * 3 };

        Assert.Equal(path.Nodes[1].Type, wider.Nodes[1].Type);
        Assert.Equal(path.Nodes[2].Type, wider.Nodes[2].Type);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(200)]
    public void Rebuilt_WithTypedNodes_KeepsTheTypesAndTheScreenPositions(double rotation)
    {
        var path = Typed() with { Rotation = rotation };
        var nodes = path.ToDocumentNodes();

        var rebuilt = path.Rebuilt(nodes, isClosed: false, FillRule.NonZero);

        var rebuiltNodes = rebuilt.ToDocumentNodes();
        for (var i = 0; i < nodes.Count; i++)
        {
            Assert.Equal(nodes[i].Type, rebuiltNodes[i].Type);
            var expected = Turn(path, nodes[i].X, nodes[i].Y);
            var actual = Turn(rebuilt, rebuiltNodes[i].X, rebuiltNodes[i].Y);
            Assert.Equal(expected.X, actual.X, 6);
            Assert.Equal(expected.Y, actual.Y, 6);
        }
    }

    [Fact]
    public void Rebuilt_ChangedNodeTypes_AreTheNewOnes()
    {
        var path = Typed();
        var nodes = path.ToDocumentNodes();
        var changed = new DocumentNode(nodes[0].X, nodes[0].Y, nodes[0].InX, nodes[0].InY, nodes[0].OutX, nodes[0].OutY, NodeType.Smooth);

        var rebuilt = path.Rebuilt([changed, nodes[1], nodes[2]], isClosed: false, FillRule.NonZero);

        Assert.Equal(NodeType.Smooth, rebuilt.Nodes[0].Type);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(-45)]
    public void ToLocal_PositionOnTheScreen_IsTheInverseOfTheRotation(double rotation)
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 40)]) with { Rotation = rotation };

        var screen = Turn(path, 30, 10);
        var local = path.ToLocal(screen.X, screen.Y);

        Assert.Equal(30, local.X, 6);
        Assert.Equal(10, local.Y, 6);
    }

    [Fact]
    public void ToLocal_WithoutRotation_ChangesNothing()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 40)]);

        Assert.Equal((12.5, 7.25), path.ToLocal(12.5, 7.25));
        Assert.Equal((3.0, -4.0), path.ToLocalDelta(3, -4));
    }

    [Fact]
    public void ToLocalDelta_QuarterTurn_TurnsTheDistanceBack()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 40)]) with { Rotation = 90 };

        // On the screen the shape is turned clockwise by 90 degrees, so a step to the right on the screen is a step up in its own frame.
        var local = path.ToLocalDelta(10, 0);

        Assert.Equal(0, local.Dx, 9);
        Assert.Equal(-10, local.Dy, 9);
    }
}
