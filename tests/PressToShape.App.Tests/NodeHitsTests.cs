using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class NodeHitsTests
{
    private static readonly IReadOnlySet<NodeRef> None = ImmutableHashSet<NodeRef>.Empty;

    // Three nodes: a corner at (0, 100), a smooth node at (100, 100) with handles at (70, 100) and (130, 100), a corner at (200, 100).
    private static PathShape Row(double rotation = 0)
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 100, outX: 30, outY: 60),
                new DocumentNode(100, 100, inX: 70, inY: 100, outX: 130, outY: 100, type: NodeType.Smooth),
                new DocumentNode(200, 100, inX: 170, inY: 60),
            ]);
        return path with { Rotation = rotation };
    }

    private static IReadOnlySet<NodeRef> Selected(PathShape path, params int[] indices) =>
        indices.Select(i => new NodeRef(path.Id, i)).ToImmutableHashSet();

    [Fact]
    public void Find_NearANode_HitsTheNode()
    {
        var path = Row();

        var hit = NodeHits.Find([path], None, 102, 99, 5, includeSegments: false);

        Assert.Equal(NodeHitType.Node, hit.Type);
        Assert.Equal(path.Id, hit.ShapeId);
        Assert.Equal(1, hit.Index);
    }

    [Fact]
    public void Find_TooFarFromEverything_IsAMiss()
    {
        var hit = NodeHits.Find([Row()], None, 100, 160, 5, includeSegments: true);

        Assert.Equal(NodeHitType.None, hit.Type);
    }

    [Fact]
    public void Find_HandleOfAnUnselectedNode_IsNotHit()
    {
        var path = Row();

        // The handle point of the middle node is at (130, 100). Without a selection nothing is shown there.
        var hit = NodeHits.Find([path], None, 130, 100, 5, includeSegments: false);

        Assert.Equal(NodeHitType.None, hit.Type);
    }

    [Fact]
    public void Find_HandleOfASelectedNode_IsHitWithItsSide()
    {
        var path = Row();

        var out_ = NodeHits.Find([path], Selected(path, 1), 131, 100, 5, includeSegments: false);
        var in_ = NodeHits.Find([path], Selected(path, 1), 70, 101, 5, includeSegments: false);

        Assert.Equal((NodeHitType.Handle, 1, NodeHandleSide.Out), (out_.Type, out_.Index, out_.Side));
        Assert.Equal((NodeHitType.Handle, 1, NodeHandleSide.In), (in_.Type, in_.Index, in_.Side));
    }

    [Fact]
    public void Find_HandleOfANeighborTowardASelectedNode_IsHit()
    {
        var path = Row();

        // Node 0 has an out handle at (30, 60) that belongs to the segment toward the selected node 1.
        var hit = NodeHits.Find([path], Selected(path, 1), 30, 60, 5, includeSegments: false);

        Assert.Equal((NodeHitType.Handle, 0, NodeHandleSide.Out), (hit.Type, hit.Index, hit.Side));
    }

    [Fact]
    public void Find_HandleOfANeighborAwayFromTheSelectedNode_IsNotHit()
    {
        var path = Row();

        // Node 2 has an in handle at (170, 60) that belongs to the segment toward node 1, so it is shown for node 1 only.
        // Selecting node 0 does not show it.
        var hit = NodeHits.Find([path], Selected(path, 0), 170, 60, 5, includeSegments: false);

        Assert.Equal(NodeHitType.None, hit.Type);
    }

    [Fact]
    public void Find_HandleOnTopOfANode_WinsOverTheNode()
    {
        var path = PathShape.Create(
            [new DocumentNode(0, 0), new DocumentNode(100, 100, inX: 98, inY: 100, outX: 150, outY: 100, type: NodeType.Corner), new DocumentNode(200, 0)]);

        var hit = NodeHits.Find([path], Selected(path, 1), 99, 100, 5, includeSegments: false);

        Assert.Equal(NodeHitType.Handle, hit.Type);
    }

    [Fact]
    public void Find_HandlesOutrankNodesEvenWhenTheNodeIsNearer()
    {
        var path = PathShape.Create(
            [new DocumentNode(0, 0), new DocumentNode(100, 100, inX: 104, inY: 100, type: NodeType.Corner), new DocumentNode(200, 0)]);

        var hit = NodeHits.Find([path], Selected(path, 1), 101, 100, 6, includeSegments: false);

        Assert.Equal(NodeHitType.Handle, hit.Type);
    }

    [Fact]
    public void Find_Segments_AreOnlyHitWhenAsked()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)]);

        var without = NodeHits.Find([path], None, 50, 2, 5, includeSegments: false);
        var with = NodeHits.Find([path], None, 50, 2, 5, includeSegments: true);

        Assert.Equal(NodeHitType.None, without.Type);
        Assert.Equal((NodeHitType.Segment, 0), (with.Type, with.Index));
    }

    [Fact]
    public void Find_SegmentHit_GivesTheCurveParameter()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]);

        var hit = NodeHits.Find([path], None, 50, 1, 5, includeSegments: true);

        // A line is a cubic with its handles on its ends, so the middle is at t = 0.5.
        Assert.Equal(0.5, hit.T, 2);
    }

    [Fact]
    public void Find_NodeBeatsASegmentNearIt()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)]);

        var hit = NodeHits.Find([path], None, 99, 1, 5, includeSegments: true);

        Assert.Equal(NodeHitType.Node, hit.Type);
    }

    [Fact]
    public void Find_CurvedSegment_IsHitOnTheCurve()
    {
        var path = PathShape.Create([new DocumentNode(0, 100, outX: 0, outY: 0), new DocumentNode(100, 100, inX: 100, inY: 0)]);
        var onCurve = PressToShape.Core.Geometry.Bezier.Point(path.Segments()[0], 0.5);

        var hit = NodeHits.Find([path], None, onCurve.X, onCurve.Y + 1, 5, includeSegments: true);

        Assert.Equal(NodeHitType.Segment, hit.Type);
    }

    [Theory]
    [InlineData(2.0, true)]
    [InlineData(5.0, true)]
    [InlineData(5.5, false)]
    public void Find_Tolerance_DecidesTheDistance(double distance, bool expected)
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]);

        var hit = NodeHits.Find([path], None, distance, 0, 5, includeSegments: false);

        Assert.Equal(expected, hit.Type == NodeHitType.Node);
    }

    [Fact]
    public void Find_ToleranceIsInDocumentUnits_SoAZoomedOutViewReachesFarther()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]);

        // 5 screen pixels are 5 units at 100 percent and 20 units at 25 percent.
        var near = NodeHits.Find([path], None, 12, 0, 5 / 1.0, includeSegments: false);
        var zoomedOut = NodeHits.Find([path], None, 12, 0, 5 / 0.25, includeSegments: false);

        Assert.Equal(NodeHitType.None, near.Type);
        Assert.Equal(NodeHitType.Node, zoomedOut.Type);
    }

    [Fact]
    public void Find_FingerRadius_IsLargerThanTheMouseRadius()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]);

        var mouse = NodeHits.Find([path], None, 10, 0, SelectionGeometry.GrabSize / 2, includeSegments: false);
        var finger = NodeHits.Find([path], None, 10, 0, SelectionGeometry.TouchGrabSize / 2, includeSegments: false);

        Assert.Equal(NodeHitType.None, mouse.Type);
        Assert.Equal(NodeHitType.Node, finger.Type);
    }

    [Fact]
    public void Find_RotatedPathAsOnScreen_HitsWhereTheNodeIsOnTheScreen()
    {
        var rotated = Row(90);
        var onScreen = EditablePath.AsOnScreen(rotated);
        var position = PathNodeEditing.NodePosition(rotated, 2);

        var hit = NodeHits.Find([onScreen], None, position.X, position.Y, 5, includeSegments: false);

        Assert.Equal((NodeHitType.Node, 2), (hit.Type, hit.Index));
        Assert.Equal(rotated.Id, hit.ShapeId);
    }

    [Fact]
    public void Find_SeveralPaths_TheTopmostWinsATie()
    {
        var below = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]);
        var above = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 50)]);

        var hit = NodeHits.Find([below, above], None, 0, 0, 5, includeSegments: false);

        Assert.Equal(above.Id, hit.ShapeId);
    }

    [Fact]
    public void Find_SeveralPaths_TheNearestNodeWinsOverTheTopmost()
    {
        var below = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]);
        var above = PathShape.Create([new DocumentNode(3, 0), new DocumentNode(100, 50)]);

        var hit = NodeHits.Find([below, above], None, 1, 0, 5, includeSegments: false);

        Assert.Equal(below.Id, hit.ShapeId);
    }

    [Fact]
    public void Find_SelectedNodesOfAnotherShape_ShowNoHandlesHere()
    {
        var path = Row();
        var other = new NodeRef(Guid.NewGuid(), 1);

        var hit = NodeHits.Find([path], new[] { other }.ToImmutableHashSet(), 130, 100, 5, includeSegments: false);

        Assert.Equal(NodeHitType.None, hit.Type);
    }

    [Fact]
    public void Find_RectangleAsEditablePath_HasFourCornerNodes()
    {
        var rectangle = new RectangleShape { X = 10, Y = 20, Width = 100, Height = 60 };
        var path = EditablePath.For(rectangle)!;

        var corners = new[] { (10.0, 20.0), (110.0, 20.0), (110.0, 80.0), (10.0, 80.0) };
        for (var i = 0; i < corners.Length; i++)
        {
            var hit = NodeHits.Find([path], None, corners[i].Item1, corners[i].Item2, 3, includeSegments: false);
            Assert.Equal((NodeHitType.Node, i), (hit.Type, hit.Index));
        }
    }

    [Fact]
    public void Find_NothingGiven_IsAMiss()
    {
        Assert.Equal(NodeHitType.None, NodeHits.Find([], None, 0, 0, 5, includeSegments: true).Type);
    }

    [Fact]
    public void VisibleHandles_OfASelectedNode_AreItsOwnAndTheNeighborsTowardIt()
    {
        var path = Row();

        var handles = NodeHits.VisibleHandles(path, new HashSet<int> { 1 });

        Assert.Equivalent(new[] { (1, NodeHandleSide.In), (1, NodeHandleSide.Out), (0, NodeHandleSide.Out), (2, NodeHandleSide.In) }, handles);
    }

    [Fact]
    public void VisibleHandles_OfTheFirstNodeOfAnOpenPath_HaveNoPreviousNeighbor()
    {
        var path = Row();

        var handles = NodeHits.VisibleHandles(path, new HashSet<int> { 0 });

        // Node 0 has only an out handle, and its next neighbor (node 1) has an in handle toward it.
        Assert.Equivalent(new[] { (0, NodeHandleSide.Out), (1, NodeHandleSide.In) }, handles);
    }

    [Fact]
    public void VisibleHandles_OfAClosedPath_WrapAround()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 0, inX: -20, inY: 0, outX: 20, outY: 0),
                new DocumentNode(100, 0, inX: 80, inY: 0, outX: 120, outY: 0),
                new DocumentNode(100, 100, inX: 120, inY: 100, outX: 80, outY: 100),
            ],
            isClosed: true);

        var handles = NodeHits.VisibleHandles(path, new HashSet<int> { 0 });

        // The previous neighbor of node 0 is node 2, its out handle belongs to the closing segment.
        Assert.Contains((2, NodeHandleSide.Out), handles);
        Assert.Contains((1, NodeHandleSide.In), handles);
    }

    [Fact]
    public void VisibleHandles_NothingSelected_IsEmpty()
    {
        Assert.Empty(NodeHits.VisibleHandles(Row(), new HashSet<int>()));
    }

    [Fact]
    public void AsOnScreen_KeepsIdAndTypes_AndReturnsTheSamePathWithoutRotation()
    {
        var path = Row();
        var rotated = Row(30);

        Assert.Same(path, EditablePath.AsOnScreen(path));
        var onScreen = EditablePath.AsOnScreen(rotated);
        Assert.Equal(rotated.Id, onScreen.Id);
        Assert.Equal(0, onScreen.Rotation);
        Assert.Equal(NodeType.Smooth, onScreen.Nodes[1].Type);
    }

    [Fact]
    public void EditablePath_For_PathRectangleEllipseAndOthers()
    {
        var path = Row();

        Assert.Same(path, EditablePath.For(path));
        Assert.Equal(4, EditablePath.For(new RectangleShape { Width = 1, Height = 1 })!.Nodes.Length);
        Assert.Equal(4, EditablePath.For(new EllipseShape { Width = 1, Height = 1 })!.Nodes.Length);
        Assert.Null(EditablePath.For(new GroupShape { Children = [new RectangleShape { Width = 1, Height = 1 }] }));
    }
}
