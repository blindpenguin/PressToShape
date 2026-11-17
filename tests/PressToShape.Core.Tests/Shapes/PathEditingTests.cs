using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathEditingTests
{
    private static DocumentNode Corner(double x, double y) => new(x, y);

    private static PathShape Curve(bool closed = false) =>
        PathShape.Create(
            [
                new DocumentNode(0, 100, outX: 0, outY: -40),
                new DocumentNode(100, 100, inX: 100, inY: -40, outX: 180, outY: 100),
                Corner(200, 150),
            ],
            closed);

    private static (double X, double Y) Visible(PathShape shape, DocumentNode node)
    {
        var radians = shape.Rotation * Math.PI / 180;
        var dx = node.X - shape.CenterX;
        var dy = node.Y - shape.CenterY;
        return (
            shape.CenterX + dx * Math.Cos(radians) - dy * Math.Sin(radians),
            shape.CenterY + dx * Math.Sin(radians) + dy * Math.Cos(radians));
    }

    // How far the farthest sample of the original curve is from the edited curve.
    private static double MaxDistance(PathShape original, PathShape edited)
    {
        var worst = 0.0;
        var editedSegments = edited.Segments();
        foreach (var segment in original.Segments())
        {
            for (var i = 0; i <= 20; i++)
            {
                var sample = Bezier.Point(segment, i / 20.0);
                var best = double.PositiveInfinity;
                foreach (var candidate in editedSegments)
                {
                    var t = Bezier.NearestParameter(candidate, sample.X, sample.Y);
                    var near = Bezier.Point(candidate, t);
                    best = Math.Min(best, Math.Sqrt(Math.Pow(near.X - sample.X, 2) + Math.Pow(near.Y - sample.Y, 2)));
                }

                worst = Math.Max(worst, best);
            }
        }

        return worst;
    }

    [Fact]
    public void InsertNode_OnALine_AddsACornerOnTheLine()
    {
        var line = PathShape.Create([Corner(0, 0), Corner(100, 0), Corner(100, 100)]);

        var result = PathEditing.InsertNode(line, 0, 0.25);

        Assert.Equal(4, result.SubPaths[0].Nodes.Length);
        var nodes = result.ToDocumentNodes();
        // t is the parameter of the cubic, in which a line is not evenly spaced: 100 * (3t^2 - 2t^3) at t = 0.25.
        Assert.Equal(15.625, nodes[1].X, 9);
        Assert.Equal(0, nodes[1].Y, 9);
        Assert.False(nodes[1].HasIn);
        Assert.False(nodes[1].HasOut);
    }

    [Theory]
    [InlineData(0, 0.5)]
    [InlineData(0, 0.2)]
    [InlineData(1, 0.7)]
    public void InsertNode_OnACurve_KeepsTheCurve(int segment, double t)
    {
        var path = Curve();

        var result = PathEditing.InsertNode(path, segment, t);

        Assert.Equal(path.SubPaths[0].Nodes.Length + 1, result.SubPaths[0].Nodes.Length);
        Assert.True(MaxDistance(path, result) < 1e-6, "the curve must not change");
        Assert.True(MaxDistance(result, path) < 1e-6, "and must not grow either");
    }

    [Fact]
    public void InsertNode_OnTheClosingSegmentOfAClosedPath_AppendsTheNode()
    {
        var path = Curve(closed: true);

        var result = PathEditing.InsertNode(path, 2, 0.5);

        Assert.Equal(4, result.SubPaths[0].Nodes.Length);
        Assert.True(result.SubPaths[0].IsClosed);
        Assert.True(MaxDistance(path, result) < 1e-6);
    }

    [Fact]
    public void InsertNode_BadArguments_Throw()
    {
        var open = Curve();
        var closed = Curve(closed: true);

        Assert.Throws<ArgumentOutOfRangeException>(() => PathEditing.InsertNode(open, 2, 0.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathEditing.InsertNode(open, -1, 0.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathEditing.InsertNode(open, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathEditing.InsertNode(open, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathEditing.InsertNode(closed, 3, 0.5));
    }

    [Fact]
    public void RemoveNode_MiddleNode_NeighborsConnect()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(50, 80), Corner(100, 0), Corner(150, 50)]);

        var result = PathEditing.RemoveNode(path, 1);

        Assert.NotNull(result);
        Assert.Equal(3, result.SubPaths[0].Nodes.Length);
        var nodes = result.ToDocumentNodes();
        Assert.Equal(100, nodes[1].X, 9);
        Assert.Equal(0, nodes[1].Y, 9);
    }

    [Fact]
    public void RemoveNode_EndNodeOfAnOpenPath_ShortensThePath()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(50, 80), Corner(100, 0)]);

        var first = PathEditing.RemoveNode(path, 0);
        var last = PathEditing.RemoveNode(path, 2);

        Assert.NotNull(first);
        Assert.NotNull(last);
        Assert.Equal(50, first.ToDocumentNodes()[0].X, 9);
        Assert.Equal(50, last.ToDocumentNodes()[1].X, 9);
    }

    [Fact]
    public void RemoveNode_AtTwoNodes_ReturnsNull()
    {
        var path = PathShape.Create([Corner(0, 0), Corner(50, 80)]);

        Assert.Null(PathEditing.RemoveNode(path, 0));
    }

    [Fact]
    public void RemoveNode_BadIndex_Throws()
    {
        var path = Curve();

        Assert.Throws<ArgumentOutOfRangeException>(() => PathEditing.RemoveNode(path, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathEditing.RemoveNode(path, -1));
    }

    [Fact]
    public void Continue_AtTheEnd_ReturnsTheNodesAsTheyAre()
    {
        var path = Curve();

        var nodes = PathEditing.Continue(path, path.NodeCount - 1);

        Assert.Equal(3, nodes.Count);
        Assert.Equal(200, nodes[2].X, 9);
        Assert.Equal(180, nodes[1].OutX!.Value, 9);
    }

    [Fact]
    public void Continue_AtTheStart_ReversesTheOrderAndSwapsTheHandles()
    {
        var path = Curve();

        var nodes = PathEditing.Continue(path, 0);

        Assert.Equal(3, nodes.Count);
        Assert.Equal(0, nodes[2].X, 9);
        Assert.Equal(100, nodes[1].X, 9);
        // The in handle of the old second node (100, -40) is now its out handle, and its out handle (180, 100) is now the in handle.
        Assert.Equal(100, nodes[1].OutX!.Value, 9);
        Assert.Equal(-40, nodes[1].OutY!.Value, 9);
        Assert.Equal(180, nodes[1].InX!.Value, 9);
        // The old first node had only an out handle, which is now its in handle.
        Assert.True(nodes[2].HasIn);
        Assert.False(nodes[2].HasOut);
    }

    [Fact]
    public void Continue_ClosedPath_Throws()
    {
        Assert.Throws<ArgumentException>(() => PathEditing.Continue(Curve(closed: true), 0));
    }

    [Theory]
    [InlineData(40)]
    [InlineData(135)]
    public void InsertAndRemove_RotatedPath_KeepEveryOriginalNodeWhereItWas(double rotation)
    {
        var path = Curve() with { Rotation = rotation };
        var before = path.ToDocumentNodes().Select(n => Visible(path, n)).ToList();

        var inserted = PathEditing.InsertNode(path, 0, 0.5);
        var insertedNodes = inserted.ToDocumentNodes();
        var afterInsert = new[] { 0, 2, 3 }.Select(i => Visible(inserted, insertedNodes[i])).ToList();

        var removed = PathEditing.RemoveNode(path, 2);
        Assert.NotNull(removed);
        var removedNodes = removed.ToDocumentNodes();
        var afterRemove = new[] { 0, 1 }.Select(i => Visible(removed, removedNodes[i])).ToList();

        for (var i = 0; i < afterInsert.Count; i++)
        {
            Assert.Equal(before[i].X, afterInsert[i].X, 6);
            Assert.Equal(before[i].Y, afterInsert[i].Y, 6);
        }

        for (var i = 0; i < afterRemove.Count; i++)
        {
            Assert.Equal(before[i].X, afterRemove[i].X, 6);
            Assert.Equal(before[i].Y, afterRemove[i].Y, 6);
        }
    }

    [Fact]
    public void InsertNode_OnACurve_GivesTheNewNodeTheSmoothType()
    {
        var result = PathEditing.InsertNode(Curve(), 0, 0.5);

        Assert.Equal(NodeType.Smooth, result.SubPaths[0].Nodes[1].Type);
    }

    [Fact]
    public void InsertNode_OnALine_GivesTheNewNodeTheCornerType()
    {
        var line = PathShape.Create([Corner(0, 0), Corner(100, 0), Corner(100, 100)]);

        var result = PathEditing.InsertNode(line, 0, 0.5);

        Assert.Equal(NodeType.Corner, result.SubPaths[0].Nodes[1].Type);
    }

    [Fact]
    public void InsertNode_NewHandlesOnACurve_LieOnTheTangent()
    {
        var result = PathEditing.InsertNode(Curve(), 0, 0.4);

        Assert.True(NodeTypes.Satisfies(result.SubPaths[0].Nodes[1]));
    }

    [Fact]
    public void InsertNode_KeepsTheTypesOfTheNeighbors()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 100, outX: 0, outY: -40, type: NodeType.Corner),
                new DocumentNode(100, 100, inX: 100, inY: -40, outX: 180, outY: 100, type: NodeType.Corner),
                new DocumentNode(200, 150, type: NodeType.Corner),
            ]);

        var result = PathEditing.InsertNode(path, 0, 0.5);

        Assert.Equal(NodeType.Corner, result.SubPaths[0].Nodes[0].Type);
        Assert.Equal(NodeType.Corner, result.SubPaths[0].Nodes[2].Type);
        Assert.Equal(NodeType.Corner, result.SubPaths[0].Nodes[3].Type);
    }

    [Fact]
    public void InsertNode_NextToASymmetricNode_TheSymmetricNodeBecomesSmoothBecauseItsHandleIsShorter()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 0, inX: -50, inY: 0, outX: 50, outY: 0, type: NodeType.Symmetric),
                new DocumentNode(100, 100, inX: 100, inY: 50, type: NodeType.Corner),
            ]);

        var result = PathEditing.InsertNode(path, 0, 0.5);

        Assert.Equal(NodeType.Smooth, result.SubPaths[0].Nodes[0].Type);
        Assert.True(NodeTypes.Satisfies(result.SubPaths[0].Nodes[0]));
    }

    [Fact]
    public void Continue_AtTheStart_KeepsTheTypesOfTheReversedNodes()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 0, outX: 20, outY: 0, type: NodeType.Corner),
                new DocumentNode(100, 0, inX: 80, inY: 0, outX: 120, outY: 0, type: NodeType.Symmetric),
                new DocumentNode(200, 0, inX: 180, inY: 0, type: NodeType.Smooth),
            ]);

        var nodes = PathEditing.Continue(path, 0);

        Assert.Equal([NodeType.Smooth, NodeType.Symmetric, NodeType.Corner], nodes.Select(n => n.Type));
    }

    [Fact]
    public void RemoveNode_KeepsTheTypesOfTheOtherNodes()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 0, type: NodeType.Corner),
                new DocumentNode(50, 50, inX: 30, inY: 50, outX: 70, outY: 50, type: NodeType.Symmetric),
                new DocumentNode(100, 0, inX: 80, inY: 0, outX: 130, outY: 0, type: NodeType.Smooth),
            ]);

        var result = PathEditing.RemoveNode(path, 0)!;

        Assert.Equal([NodeType.Symmetric, NodeType.Smooth], result.SubPaths[0].Nodes.Select(n => n.Type));
    }
}
