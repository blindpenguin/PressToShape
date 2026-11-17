using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PenDrawingTests
{
    private const double Near = 3;
    private const double Close = 6;

    private static PenPressResult Click(PenDrawing pen, double x, double y)
    {
        var result = pen.Press(x, y, Near, Close);
        pen.Release();
        return result;
    }

    private static PenDrawing ThreeNodes()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);
        Click(pen, 100, 0);
        Click(pen, 100, 100);
        return pen;
    }

    [Fact]
    public void Press_OnEmptyPath_AddsACornerNode()
    {
        var pen = new PenDrawing();

        var result = pen.Press(10, 20, Near, Close);

        Assert.Equal(PenPressResult.AddedNode, result);
        Assert.True(pen.IsDrawing);
        Assert.True(pen.IsPressing);
        var node = Assert.Single(pen.Nodes);
        Assert.Equal((10, 20), (node.X, node.Y));
        Assert.False(node.HasIn);
        Assert.False(node.HasOut);
    }

    [Fact]
    public void Click_SeveralTimes_AddsCornerNodesInOrder()
    {
        var pen = ThreeNodes();

        Assert.Equal(3, pen.Nodes.Count);
        Assert.Equal([0.0, 100.0, 100.0], pen.Nodes.Select(n => n.X));
        Assert.All(pen.Nodes, node => Assert.False(node.HasOut));
    }

    [Fact]
    public void Drag_AfterPress_MakesASmoothNodeWithMirroredHandles()
    {
        var pen = new PenDrawing();
        pen.Press(50, 50, Near, Close);

        pen.Drag(70, 60, threshold: 2);
        pen.Release();

        var node = pen.Nodes[0];
        Assert.Equal((70, 60), (node.OutX!.Value, node.OutY!.Value));
        Assert.Equal((30, 40), (node.InX!.Value, node.InY!.Value));
        Assert.False(pen.IsPressing);
    }

    [Fact]
    public void Drag_ShorterThanTheThreshold_LeavesACorner()
    {
        var pen = new PenDrawing();
        pen.Press(50, 50, Near, Close);

        pen.Drag(51, 50, threshold: 2);

        Assert.False(pen.Nodes[0].HasOut);
        Assert.False(pen.Nodes[0].HasIn);
    }

    [Fact]
    public void Drag_BackToTheNode_RemovesTheHandlesAgain()
    {
        var pen = new PenDrawing();
        pen.Press(50, 50, Near, Close);
        pen.Drag(80, 50, threshold: 2);

        pen.Drag(50, 50, threshold: 2);

        Assert.False(pen.Nodes[0].HasOut);
    }

    [Fact]
    public void Drag_ZeroLengthWithZeroThreshold_LeavesACorner()
    {
        var pen = new PenDrawing();
        pen.Press(50, 50, Near, Close);

        pen.Drag(50, 50, threshold: 0);

        Assert.False(pen.Nodes[0].HasOut);
    }

    [Fact]
    public void Drag_WithoutAPress_DoesNothing()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);

        pen.Drag(30, 30, threshold: 2);

        Assert.False(pen.Nodes[0].HasOut);
    }

    [Fact]
    public void Drag_OnlyChangesTheNodeThatWasJustAdded()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);
        pen.Press(100, 0, Near, Close);

        pen.Drag(120, 20, threshold: 2);

        Assert.False(pen.Nodes[0].HasOut);
        Assert.True(pen.Nodes[1].HasOut);
    }

    [Fact]
    public void PreviewShape_OneNodeAndNoPointer_IsNull()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);

        Assert.Null(pen.PreviewShape);
    }

    [Fact]
    public void PreviewShape_NodeAndPointerWithoutHandle_IsALine()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);

        pen.Hover(60, 40, Close);

        var preview = pen.PreviewShape!;
        Assert.Equal(2, preview.SubPaths[0].Nodes.Length);
        Assert.False(preview.ToDocumentNodes()[0].HasOut);
        Assert.Equal(new[] { 0.0, 60.0 }, preview.ToDocumentNodes().Select(n => n.X));
    }

    [Fact]
    public void PreviewShape_LastNodeWithAnOutHandle_IsACurve()
    {
        var pen = new PenDrawing();
        pen.Press(0, 0, Near, Close);
        pen.Drag(0, 50, threshold: 2);
        pen.Release();

        pen.Hover(100, 100, Close);

        var first = pen.PreviewShape!.ToDocumentNodes()[0];
        Assert.True(first.HasOut);
        Assert.Equal((0, 50), (first.OutX!.Value, first.OutY!.Value));
    }

    [Fact]
    public void PreviewShape_WhileTheButtonIsDown_ShowsOnlyTheNodes()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);
        pen.Press(100, 0, Near, Close);

        var preview = pen.PreviewShape!;

        Assert.Equal(2, preview.SubPaths[0].Nodes.Length);
        Assert.Equal(100, preview.ToDocumentNodes()[1].X);
    }

    [Fact]
    public void Press_OnTheFirstNodeOfThree_Closes()
    {
        var pen = ThreeNodes();

        var result = pen.Press(2, 1, Near, Close);

        Assert.Equal(PenPressResult.ClosedPath, result);
        Assert.Equal(3, pen.Nodes.Count);
    }

    [Fact]
    public void Press_OnTheFirstNodeOfTwo_AddsANodeInstead()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);
        Click(pen, 100, 0);

        var result = pen.Press(2, 1, Near, Close);

        Assert.Equal(PenPressResult.AddedNode, result);
        Assert.Equal(3, pen.Nodes.Count);
    }

    [Fact]
    public void Press_OnTheLastNode_FinishesWithoutAddingANode()
    {
        var pen = ThreeNodes();

        var result = pen.Press(101, 99, Near, Close);

        Assert.Equal(PenPressResult.FinishedPath, result);
        Assert.Equal(3, pen.Nodes.Count);
    }

    [Fact]
    public void Press_OnTheOnlyNode_FinishesAndFinishGivesNothing()
    {
        var pen = new PenDrawing();
        Click(pen, 10, 10);

        var result = pen.Press(10, 10, Near, Close);

        Assert.Equal(PenPressResult.FinishedPath, result);
        Assert.Null(pen.Finish(close: false));
        Assert.False(pen.IsDrawing);
    }

    [Fact]
    public void Hover_NearTheFirstNodeOfThree_HighlightsIt()
    {
        var pen = ThreeNodes();

        pen.Hover(3, 2, Close);
        var near = pen.Overlay!.HighlightFirst;
        pen.Hover(50, 50, Close);
        var far = pen.Overlay!.HighlightFirst;

        Assert.True(near);
        Assert.False(far);
    }

    [Fact]
    public void Hover_NearTheFirstNodeOfTwo_DoesNotHighlight()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);
        Click(pen, 100, 0);

        pen.Hover(1, 1, Close);

        Assert.False(pen.Overlay!.HighlightFirst);
    }

    [Fact]
    public void Finish_Open_ReturnsAPathWithTheNodes()
    {
        var pen = ThreeNodes();

        var path = pen.Finish(close: false);

        Assert.NotNull(path);
        Assert.Equal(3, path.SubPaths[0].Nodes.Length);
        Assert.False(path.SubPaths[0].IsClosed);
        Assert.False(pen.IsDrawing);
        Assert.Null(pen.Overlay);
    }

    [Fact]
    public void Finish_Closed_ReturnsAClosedPath()
    {
        var pen = ThreeNodes();

        var path = pen.Finish(close: true);

        Assert.True(path!.SubPaths[0].IsClosed);
    }

    [Fact]
    public void Finish_KeepsTheHandlesOfSmoothNodes()
    {
        var pen = new PenDrawing();
        pen.Press(0, 0, Near, Close);
        pen.Drag(0, 40, threshold: 2);
        pen.Release();
        Click(pen, 100, 100);

        var nodes = pen.Finish(close: false)!.ToDocumentNodes();

        Assert.Equal((0, 40), (nodes[0].OutX!.Value, nodes[0].OutY!.Value));
        Assert.Equal((0, -40), (nodes[0].InX!.Value, nodes[0].InY!.Value));
    }

    [Fact]
    public void Discard_AfterDrawing_EmptiesEverything()
    {
        var pen = ThreeNodes();
        pen.Hover(10, 10, Close);

        pen.Discard();

        Assert.False(pen.IsDrawing);
        Assert.Empty(pen.Nodes);
        Assert.Null(pen.PreviewShape);
        Assert.Null(pen.Overlay);
    }

    [Fact]
    public void Begin_WithExistingNodes_ContinuesAfterTheLastOne()
    {
        var pen = new PenDrawing();
        DocumentNode[] existing = [new(0, 0), new(100, 0)];

        pen.Begin(existing);
        Click(pen, 100, 100);

        Assert.Equal(3, pen.Nodes.Count);
        Assert.Equal(100, pen.Nodes[2].Y);
    }

    [Fact]
    public void Begin_PressOnItsLastNode_FinishesWithTheExistingNodes()
    {
        var pen = new PenDrawing();
        pen.Begin([new DocumentNode(0, 0), new DocumentNode(100, 0)]);

        var result = pen.Press(100, 0, Near, Close);

        Assert.Equal(PenPressResult.FinishedPath, result);
        Assert.Equal(2, pen.Finish(close: false)!.SubPaths[0].Nodes.Length);
    }

    [Fact]
    public void Begin_AfterEarlierDrawing_ReplacesTheOldNodes()
    {
        var pen = ThreeNodes();

        pen.Begin([new DocumentNode(5, 5), new DocumentNode(6, 6)]);

        Assert.Equal(2, pen.Nodes.Count);
        Assert.Equal(5, pen.Nodes[0].X);
    }

    [Fact]
    public void Overlay_AfterNodes_ListsThemWithTheirHandles()
    {
        var pen = new PenDrawing();
        pen.Press(10, 10, Near, Close);
        pen.Drag(30, 10, threshold: 2);

        var overlay = pen.Overlay!;

        Assert.Single(overlay.Nodes);
        Assert.True(overlay.Nodes[0].HasOut);
    }

    [Fact]
    public void AbortPress_AfterAPress_TakesBackTheNodeThatWasAdded()
    {
        var pen = new PenDrawing();
        Click(pen, 0, 0);
        pen.Press(100, 0, Near, Close);

        pen.AbortPress();

        Assert.Single(pen.Nodes);
        Assert.False(pen.IsPressing);
    }

    [Fact]
    public void AbortPress_WithoutAPress_DoesNothing()
    {
        var pen = ThreeNodes();

        pen.AbortPress();

        Assert.Equal(3, pen.Nodes.Count);
    }

    [Fact]
    public void PreviewShape_PointerExactlyOnTheLastNode_IsNull()
    {
        var pen = new PenDrawing();
        Click(pen, 10, 10);

        pen.Hover(10, 10, Close);

        Assert.Null(pen.PreviewShape);
    }

    [Fact]
    public void Press_AddsACornerNode()
    {
        var pen = new PenDrawing();

        pen.Press(10, 10, Near, Close);
        pen.Release();

        Assert.Equal(NodeType.Corner, pen.Nodes[0].Type);
    }

    [Fact]
    public void Drag_BeyondTheThreshold_MakesASymmetricNode()
    {
        var pen = new PenDrawing();
        pen.Press(50, 50, Near, Close);

        pen.Drag(80, 50, threshold: 2);
        pen.Release();

        var node = pen.Nodes[0];
        Assert.Equal(NodeType.Symmetric, node.Type);
        Assert.True(NodeTypes.Satisfies(node));
        Assert.Equal(NodeType.Symmetric, NodeTypes.Derive(node));
    }

    [Fact]
    public void Drag_BackUnderTheThreshold_IsACornerAgain()
    {
        var pen = new PenDrawing();
        pen.Press(50, 50, Near, Close);
        pen.Drag(80, 50, threshold: 2);

        pen.Drag(50, 51, threshold: 2);

        Assert.Equal(NodeType.Corner, pen.Nodes[0].Type);
        Assert.False(pen.Nodes[0].HasOut);
    }

    [Fact]
    public void Finish_KeepsTheTypesOfTheNodes()
    {
        var pen = new PenDrawing();
        pen.Press(0, 0, Near, Close);
        pen.Drag(0, 40, threshold: 2);
        pen.Release();
        pen.Press(100, 100, Near, Close);
        pen.Release();

        var path = pen.Finish(close: false)!;

        Assert.Equal([NodeType.Symmetric, NodeType.Corner], path.SubPaths[0].Nodes.Select(n => n.Type));
    }

    [Fact]
    public void Begin_WithTypedNodes_KeepsTheirTypesThroughFinish()
    {
        var pen = new PenDrawing();
        pen.Begin(
            [
                new DocumentNode(0, 0, outX: 20, outY: 0, type: NodeType.Corner),
                new DocumentNode(100, 0, inX: 80, inY: 0, outX: 120, outY: 0, type: NodeType.Smooth),
            ]);
        pen.Press(100, 100, Near, Close);
        pen.Release();

        var path = pen.Finish(close: false)!;

        Assert.Equal([NodeType.Corner, NodeType.Smooth, NodeType.Corner], path.SubPaths[0].Nodes.Select(n => n.Type));
    }
}
