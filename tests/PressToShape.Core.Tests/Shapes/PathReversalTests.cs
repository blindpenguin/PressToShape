using System;
using System.Linq;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class PathReversalTests
{
    private static PathShape Open() =>
        PathShape.Create(
            [
                new DocumentNode(0, 0, outX: 20, outY: -10),
                new DocumentNode(100, 0, inX: 80, inY: 10, outX: 120, outY: -10, type: NodeType.Smooth),
                new DocumentNode(200, 50, inX: 180, inY: 40),
            ]);

    private static PathShape Closed() =>
        PathShape.Create(
            [
                new DocumentNode(0, 0, outX: 30, outY: 0),
                new DocumentNode(100, 0, inX: 70, inY: 0, outX: 110, outY: 20),
                new DocumentNode(100, 100),
                new DocumentNode(0, 100, inX: 20, inY: 100),
            ],
            isClosed: true,
            fillRule: FillRule.EvenOdd);

    private static (double X, double Y) OnScreen(PathShape path, int subPath, int node)
    {
        var local = path.ToDocumentSubPaths()[subPath].Nodes[node];
        return path.FromLocal(local.X, local.Y);
    }

    [Fact]
    public void Reverse_OpenPath_ReversesTheOrderAndSwapsTheHandles()
    {
        var path = Open();

        var result = PathReversal.Reverse(path);

        var nodes = result.ToDocumentSubPaths()[0].Nodes;
        Assert.Equal(3, nodes.Count);
        Assert.Equal((200.0, 50.0), (Math.Round(nodes[0].X, 6), Math.Round(nodes[0].Y, 6)));
        Assert.Equal((100.0, 0.0), (Math.Round(nodes[1].X, 6), Math.Round(nodes[1].Y, 6)));
        Assert.Equal((0.0, 0.0), (Math.Round(nodes[2].X, 6), Math.Round(nodes[2].Y, 6)));
        // The last node had only an in handle (180, 40), which is now its out handle.
        Assert.True(nodes[0].HasOut);
        Assert.False(nodes[0].HasIn);
        Assert.Equal(180, nodes[0].OutX!.Value, 6);
        // The middle node had in (80, 10) and out (120, -10): now the other way round.
        Assert.Equal(120, nodes[1].InX!.Value, 6);
        Assert.Equal(80, nodes[1].OutX!.Value, 6);
        // The first node had only an out handle, which is now its in handle.
        Assert.True(nodes[2].HasIn);
        Assert.False(nodes[2].HasOut);
        Assert.False(result.SubPaths[0].IsClosed);
    }

    [Fact]
    public void Reverse_ClosedPath_KeepsTheFirstNodeFirst()
    {
        var result = PathReversal.Reverse(Closed());

        var nodes = result.ToDocumentSubPaths()[0].Nodes;
        Assert.True(result.SubPaths[0].IsClosed);
        Assert.Equal(
            [(0.0, 0.0), (0.0, 100.0), (100.0, 100.0), (100.0, 0.0)],
            nodes.Select(n => (Math.Round(n.X, 6), Math.Round(n.Y, 6))));
        // The first node had only an out handle (30, 0), which is now its in handle.
        Assert.True(nodes[0].HasIn);
        Assert.False(nodes[0].HasOut);
        Assert.Equal(30, nodes[0].InX!.Value, 6);
    }

    [Fact]
    public void Reverse_CompoundPath_ReversesEachSubPathInPlaceAndKeepsTheirOrder()
    {
        var path = PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(10, 0), new DocumentNode(10, 10)], true),
                new DocumentSubPath([new DocumentNode(100, 0), new DocumentNode(120, 0), new DocumentNode(140, 0)], false),
            ]);

        var result = PathReversal.Reverse(path);

        var sub = result.ToDocumentSubPaths();
        Assert.Equal(2, sub.Count);
        Assert.True(sub[0].IsClosed);
        Assert.False(sub[1].IsClosed);
        Assert.Equal([0.0, 10.0, 10.0], sub[0].Nodes.Select(n => Math.Round(n.X, 6)));
        Assert.Equal([0.0, 10.0, 0.0], sub[0].Nodes.Select(n => Math.Round(n.Y, 6)));
        Assert.Equal([140.0, 120.0, 100.0], sub[1].Nodes.Select(n => Math.Round(n.X, 6)));
    }

    [Fact]
    public void Reverse_NodeTypes_StayValid()
    {
        var result = PathReversal.Reverse(Open());

        Assert.All(result.ToDocumentSubPaths()[0].Nodes, node => Assert.True(NodeTypes.Satisfies(node)));
        Assert.Equal(NodeType.Smooth, result.ToDocumentSubPaths()[0].Nodes[1].Type);
    }

    [Fact]
    public void Reverse_RotatedPath_KeepsEveryNodeAtTheSamePlaceOnTheScreen()
    {
        var path = Open() with { Rotation = 35 };
        var before = Enumerable.Range(0, 3).Select(i => OnScreen(path, 0, i)).ToList();

        var result = PathReversal.Reverse(path);

        Assert.Equal(35, result.Rotation);
        for (var i = 0; i < 3; i++)
        {
            var after = OnScreen(result, 0, 2 - i);
            Assert.Equal(before[i].X, after.X, 6);
            Assert.Equal(before[i].Y, after.Y, 6);
        }
    }

    [Fact]
    public void Reverse_LooksTheSame_HitTestsAgree()
    {
        var path = Closed() with { Appearance = new Appearance { Fill = Color.FromRgb(1, 2, 3) } };
        var result = PathReversal.Reverse(path);

        foreach (var (x, y) in new[] { (50.0, 50.0), (5.0, 95.0), (150.0, 50.0), (50.0, 200.0), (95.0, 5.0) })
        {
            Assert.Equal(path.Contains(x, y), result.Contains(x, y));
        }
    }

    [Fact]
    public void Reverse_KeepsIdAppearanceNameFlagsAndFillRule()
    {
        var path = Closed() with { Name = "Shape", Appearance = new Appearance { Fill = Color.FromRgb(9, 9, 9), StrokeWidth = 4 }, IsLocked = true };

        var result = PathReversal.Reverse(path);

        Assert.Equal(path.Id, result.Id);
        Assert.Equal("Shape", result.Name);
        Assert.Equal(path.Appearance, result.Appearance);
        Assert.Equal(FillRule.EvenOdd, result.FillRule);
        Assert.True(result.IsLocked);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reverse_Twice_GivesTheSameNodes(bool closed)
    {
        var path = closed ? Closed() : Open();

        var twice = PathReversal.Reverse(PathReversal.Reverse(path));

        Assert.Equal(path.SubPaths[0].IsClosed, twice.SubPaths[0].IsClosed);
        var expected = path.ToDocumentSubPaths()[0].Nodes;
        var actual = twice.ToDocumentSubPaths()[0].Nodes;
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].X, actual[i].X, 6);
            Assert.Equal(expected[i].Y, actual[i].Y, 6);
            Assert.Equal(expected[i].InX, actual[i].InX is { } inX ? Math.Round(inX, 6) : null);
            Assert.Equal(expected[i].OutX, actual[i].OutX is { } outX ? Math.Round(outX, 6) : null);
            Assert.Equal(expected[i].Type, actual[i].Type);
        }
    }

    [Fact]
    public void Reverse_DoesNotChangeTheInput_AndNullThrows()
    {
        var path = Open();
        var nodes = path.SubPaths[0].Nodes;

        PathReversal.Reverse(path);

        Assert.Equal(nodes, path.SubPaths[0].Nodes);
        Assert.Throws<ArgumentNullException>(() => PathReversal.Reverse(null!));
    }
}
