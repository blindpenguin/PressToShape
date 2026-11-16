using System;
using System.Linq;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathNodeEditingSegmentTests
{
    private static PathShape Curved(NodeType first = NodeType.Corner, NodeType second = NodeType.Corner) =>
        PathShape.Create(
            [
                new DocumentNode(0, 100, outX: 50, outY: 60, type: first),
                new DocumentNode(200, 100, inX: 150, inY: 60, type: second),
                new DocumentNode(300, 150),
            ]);

    private static PathShape Straight() =>
        PathShape.Create([new DocumentNode(0, 0), new DocumentNode(90, 0), new DocumentNode(90, 90)]);

    private static (double X, double Y) OnScreen(PathShape path, int index)
    {
        var node = path.ToDocumentNodes()[index];
        return path.FromLocal(node.X, node.Y);
    }

    [Theory]
    [InlineData(0.2)]
    [InlineData(0.35)]
    [InlineData(0.5)]
    [InlineData(0.8)]
    public void BendSegment_PointOnTheCurve_MovesByTheDistance(double t)
    {
        var path = Curved();
        var before = Bezier.Point(path.Segments()[0], t);

        var bent = PathNodeEditing.BendSegment(path, 0, t, 12, -30);

        var after = Bezier.Point(bent.Segments()[0], t);
        Assert.Equal(before.X + 12, after.X, 6);
        Assert.Equal(before.Y - 30, after.Y, 6);
    }

    [Fact]
    public void BendSegment_NodesDoNotMoveAndOtherSegmentsStay()
    {
        var path = Curved();

        var bent = PathNodeEditing.BendSegment(path, 0, 0.5, 0, 40);

        var before = path.ToDocumentNodes();
        var after = bent.ToDocumentNodes();
        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(before[i].X, after[i].X, 6);
            Assert.Equal(before[i].Y, after[i].Y, 6);
        }

        Assert.Equal(path.Segments()[1], bent.Segments()[1]);
    }

    [Fact]
    public void BendSegment_StraightSegment_BecomesACurveThroughTheMovedPoint()
    {
        var path = Straight();
        var before = Bezier.Point(path.Segments()[0], 0.5);

        var bent = PathNodeEditing.BendSegment(path, 0, 0.5, 0, 30);

        var nodes = bent.ToDocumentNodes();
        Assert.True(nodes[0].HasOut);
        Assert.True(nodes[1].HasIn);
        var after = Bezier.Point(bent.Segments()[0], 0.5);
        Assert.Equal(before.X, after.X, 6);
        Assert.Equal(before.Y + 30, after.Y, 6);
    }

    [Fact]
    public void BendSegment_ClosingSegmentOfAClosedPath_Works()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)], isClosed: true);
        var before = Bezier.Point(path.Segments()[2], 0.5);

        var bent = PathNodeEditing.BendSegment(path, 2, 0.5, -20, 0);

        var after = Bezier.Point(bent.Segments()[2], 0.5);
        Assert.Equal(before.X - 20, after.X, 6);
        Assert.True(bent.ToDocumentNodes()[2].HasOut);
        Assert.True(bent.ToDocumentNodes()[0].HasIn);
    }

    [Fact]
    public void BendSegment_SmoothAndSymmetricNodesAtTheEnds_BecomeCorners()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 100, inX: -50, inY: 100, outX: 50, outY: 100, type: NodeType.Symmetric),
                new DocumentNode(200, 100, inX: 150, inY: 100, outX: 250, outY: 100, type: NodeType.Smooth),
            ]);

        var bent = PathNodeEditing.BendSegment(path, 0, 0.5, 0, 40).ToDocumentNodes();

        Assert.Equal(NodeType.Corner, bent[0].Type);
        Assert.Equal(NodeType.Corner, bent[1].Type);
    }

    [Fact]
    public void BendSegment_NodeWhoseHandlesStillFitItsType_KeepsTheType()
    {
        // The node has only the handle of the segment, so a smooth type cannot be violated.
        var path = Curved(NodeType.Smooth, NodeType.Smooth);

        var bent = PathNodeEditing.BendSegment(path, 0, 0.5, 0, 40).ToDocumentNodes();

        Assert.Equal(NodeType.Smooth, bent[0].Type);
        Assert.Equal(NodeType.Smooth, bent[1].Type);
    }

    [Fact]
    public void BendSegment_NearAnEnd_StaysFiniteAndGentle()
    {
        var path = Curved();

        var bent = PathNodeEditing.BendSegment(path, 0, 0.02, 10, 10);

        var node = bent.ToDocumentNodes()[0];
        Assert.All(new[] { node.OutX!.Value, node.OutY!.Value }, v => Assert.True(double.IsFinite(v)));
        // With the weights of t = 0.15 a pull of 10 moves the handle by well under a hundred.
        Assert.True(Math.Abs(node.OutX!.Value - 50) < 60);
    }

    [Fact]
    public void BendSegment_NoDistance_ReturnsTheSamePath()
    {
        var path = Curved();

        Assert.Same(path, PathNodeEditing.BendSegment(path, 0, 0.5, 0, 0));
    }

    [Fact]
    public void BendSegment_ZeroLengthSegment_GivesNoNaN()
    {
        var path = PathShape.Create([new DocumentNode(10, 10), new DocumentNode(10, 10), new DocumentNode(50, 10)]);

        var bent = PathNodeEditing.BendSegment(path, 0, 0.5, 5, 5).ToDocumentNodes();

        Assert.All(bent, n => Assert.True(double.IsFinite(n.X) && double.IsFinite(n.Y)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void BendSegment_RotatedPath_AnchorsStayWhereTheyAreOnTheScreen(double rotation)
    {
        var path = Curved() with { Rotation = rotation };
        var before = new[] { OnScreen(path, 0), OnScreen(path, 1), OnScreen(path, 2) };

        var bent = PathNodeEditing.BendSegment(path, 0, 0.5, 0, 40);

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(before[i].X, OnScreen(bent, i).X, 6);
            Assert.Equal(before[i].Y, OnScreen(bent, i).Y, 6);
        }
    }

    [Fact]
    public void BendSegment_BadInput_Throws()
    {
        var path = Curved();

        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.BendSegment(path, 2, 0.5, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.BendSegment(path, -1, 0.5, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.BendSegment(path, 0, 0.5, double.NaN, 1));
    }

    [Fact]
    public void StraightenSegment_RemovesBothHandlesAndMakesCornersOfTheNodesThatLostOne()
    {
        var path = Curved(NodeType.Smooth, NodeType.Symmetric);

        var result = PathNodeEditing.StraightenSegment(path, 0).ToDocumentNodes();

        Assert.False(result[0].HasOut);
        Assert.False(result[1].HasIn);
        Assert.Equal(NodeType.Corner, result[0].Type);
        Assert.Equal(NodeType.Corner, result[1].Type);
    }

    [Fact]
    public void StraightenSegment_NodeThatLostNothing_KeepsItsType()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 0, outX: 30, outY: 0),
                new DocumentNode(100, 0, inX: 70, inY: 0, outX: 130, outY: 0, type: NodeType.Smooth),
                new DocumentNode(200, 0, type: NodeType.Corner),
            ]);

        // Segment 1 only has the out handle of node 1, the node at its end has no in handle.
        var result = PathNodeEditing.StraightenSegment(path, 1).ToDocumentNodes();

        Assert.False(result[1].HasOut);
        Assert.Equal(NodeType.Corner, result[1].Type);
        Assert.Equal(NodeType.Corner, result[2].Type);
        Assert.Equal(path.ToDocumentNodes()[0].OutX, result[0].OutX);
    }

    [Fact]
    public void StraightenSegment_AlreadyStraight_ReturnsTheSamePath()
    {
        var path = Straight();

        Assert.Same(path, PathNodeEditing.StraightenSegment(path, 0));
    }

    [Fact]
    public void StraightenSegment_ClosingSegment_RemovesItsHandles()
    {
        var path = PathShape.Create(
            [new DocumentNode(0, 0, inX: -20, inY: 0), new DocumentNode(100, 0), new DocumentNode(100, 100, outX: 60, outY: 100)], isClosed: true);

        var result = PathNodeEditing.StraightenSegment(path, 2).ToDocumentNodes();

        Assert.False(result[2].HasOut);
        Assert.False(result[0].HasIn);
    }

    [Fact]
    public void CurveSegment_StraightSegment_GetsHandlesAtThirdsOnTheLine()
    {
        var result = PathNodeEditing.CurveSegment(Straight(), 0).ToDocumentNodes();

        Assert.Equal(30, result[0].OutX!.Value, 6);
        Assert.Equal(0, result[0].OutY!.Value, 6);
        Assert.Equal(60, result[1].InX!.Value, 6);
        Assert.Equal(0, result[1].InY!.Value, 6);
    }

    [Fact]
    public void CurveSegment_StraightSegment_DoesNotChangeThePicture()
    {
        var path = Straight();
        var curved = PathNodeEditing.CurveSegment(path, 0);

        for (var i = 0; i <= 10; i++)
        {
            var point = Bezier.Point(curved.Segments()[0], i / 10.0);
            Assert.Equal(0, point.Y, 6);
            Assert.InRange(point.X, -1e-9, 90 + 1e-9);
        }
    }

    [Fact]
    public void CurveSegment_AlreadyCurved_ReturnsTheSamePath()
    {
        var path = Curved();

        Assert.Same(path, PathNodeEditing.CurveSegment(path, 0));
    }

    [Fact]
    public void CurveSegment_SegmentWithOneHandle_CountsAsCurvedAndStaysAsItIs()
    {
        var path = PathShape.Create([new DocumentNode(0, 0, outX: 20, outY: 10), new DocumentNode(90, 0)]);

        Assert.Same(path, PathNodeEditing.CurveSegment(path, 0));
    }

    [Fact]
    public void CurveSegment_NodeWhoseOtherHandleNoLongerFits_BecomesACorner()
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 0),
                new DocumentNode(90, 0, inX: 90, inY: 30, type: NodeType.Smooth),
                new DocumentNode(90, 90, outX: 90, outY: 60),
            ]);

        // Node 1 has an in handle that belongs to segment 0, and a handle created for segment 1 does not lie opposite to it.
        var result = PathNodeEditing.CurveSegment(path, 1).ToDocumentNodes();

        Assert.True(result[1].HasOut);
        Assert.True(NodeTypes.Satisfies(result[1]));
    }

    [Fact]
    public void CurveSegment_SegmentOfNoLength_ReturnsTheSamePath()
    {
        var path = PathShape.Create([new DocumentNode(5, 5), new DocumentNode(5, 5), new DocumentNode(50, 5)]);

        Assert.Same(path, PathNodeEditing.CurveSegment(path, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void StraightenAndCurve_RotatedPath_AnchorsStayWhereTheyAre(double rotation)
    {
        var path = Curved() with { Rotation = rotation };
        var before = OnScreen(path, 1);

        var straight = PathNodeEditing.StraightenSegment(path, 0);
        var curved = PathNodeEditing.CurveSegment(straight, 0);

        Assert.Equal(before.X, OnScreen(straight, 1).X, 6);
        Assert.Equal(before.Y, OnScreen(curved, 1).Y, 6);
    }

    [Fact]
    public void SegmentMethods_BadIndex_Throw()
    {
        var path = Curved();

        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.StraightenSegment(path, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathNodeEditing.CurveSegment(path, 5));
    }
}
