using System.Collections.Generic;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelNodeTests
{
    // The viewport has the size of the artboard at 100 percent, so a screen position is a document position.
    private sealed record Fixture(EditorState State, CanvasAreaViewModel Canvas, ManualTimeProvider Time, PathShape Path);

    private static Fixture Create(Tool tool, params Shape[] extra)
    {
        var path = PathShape.Create(
            [new DocumentNode(100, 200), new DocumentNode(300, 200), new DocumentNode(500, 200), new DocumentNode(500, 400)]);
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [path, .. extra])) { ActiveTool = tool };
        var time = new ManualTimeProvider();
        var canvas = new CanvasAreaViewModel(state, null, time) { ViewportWidth = 1200, ViewportHeight = 800 };
        state.Select(path.Id);
        return new Fixture(state, canvas, time, path);
    }

    private static PathShape Current(Fixture f) => (PathShape)f.State.Document.FindShape(f.Path.Id)!.Shape;

    private static void SelectNodes(Fixture f, params int[] indices) =>
        f.State.SetNodeSelection(indices.Select(i => new NodeRef(f.Path.Id, i)));

    // ---- double click and double tap ----

    [Fact]
    public void DoubleClick_OnACornerNodeWithTheNodeTool_MakesItSmooth()
    {
        var f = Create(Tool.Node);
        var steps = f.State.History.Steps.Count;

        f.Canvas.DoubleClick(300, 200);

        var node = Current(f).Nodes[1];
        Assert.Equal(NodeType.Smooth, node.Type);
        Assert.True(node.HasIn && node.HasOut);
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.ChangeNodeType, f.State.History.Steps[^1]);
        Assert.Equal([new NodeRef(f.Path.Id, 1)], f.State.SelectedNodes);
    }

    [Fact]
    public void DoubleClick_AgainOnTheNode_MakesItACorner()
    {
        var f = Create(Tool.Node);
        f.Canvas.DoubleClick(300, 200);

        f.Canvas.DoubleClick(300, 200);

        Assert.Equal(NodeType.Corner, Current(f).Nodes[1].Type);
    }

    [Fact]
    public void DoubleClick_OnASymmetricNode_MakesItACorner()
    {
        var f = Create(Tool.Node);
        f.State.Edit(HistoryStepType.ChangeNodeType, d => d.UpdateShapes(new Dictionary<System.Guid, Shape> { [f.Path.Id] = PathNodeEditing.SetNodeType(f.Path, [1], NodeType.Symmetric) }));

        f.Canvas.DoubleClick(300, 200);

        Assert.Equal(NodeType.Corner, Current(f).Nodes[1].Type);
    }

    [Fact]
    public void DoubleClick_AwayFromAnyNode_ChangesNothing()
    {
        var f = Create(Tool.Node);
        var steps = f.State.History.Steps.Count;

        f.Canvas.DoubleClick(700, 700);

        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void DoubleClick_WithDirectSelect_IsAPlainPressAndDoesNotConvert()
    {
        var f = Create(Tool.DirectSelect);
        var steps = f.State.History.Steps.Count;

        f.Canvas.DoubleClick(300, 200);

        Assert.Equal(NodeType.Corner, Current(f).Nodes[1].Type);
        Assert.Equal(steps, f.State.History.Steps.Count);
        Assert.Equal([new NodeRef(f.Path.Id, 1)], f.State.SelectedNodes);
    }

    [Fact]
    public void DoubleClick_OnACornerOfARectangle_ConvertsItAndMakesTheNodeSmooth()
    {
        var rectangle = new RectangleShape { X = 600, Y = 500, Width = 100, Height = 100 };
        var f = Create(Tool.Node, rectangle);
        f.State.Select(rectangle.Id);

        f.Canvas.DoubleClick(700, 500);

        var path = Assert.IsType<PathShape>(f.State.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal(NodeType.Smooth, path.Nodes[1].Type);
    }

    [Fact]
    public void DoubleTap_OnANodeWithTheNodeTool_ChangesItsType()
    {
        var f = Create(Tool.Node);
        double ScreenX(double x) => f.Canvas.OriginX + x;
        double ScreenY(double y) => f.Canvas.OriginY + y;

        f.Canvas.TouchPressed(1, ScreenX(300), ScreenY(200));
        f.Canvas.TouchReleased(1, ScreenX(300), ScreenY(200));
        f.Time.Advance(200);
        f.Canvas.TouchPressed(2, ScreenX(302), ScreenY(201));
        f.Canvas.TouchReleased(2, ScreenX(302), ScreenY(201));

        Assert.Equal(NodeType.Smooth, Current(f).Nodes[1].Type);
    }

    // ---- Delete ----

    [Fact]
    public void Delete_WithSelectedNodes_RemovesTheNodesAsOneStep()
    {
        var f = Create(Tool.Node);
        SelectNodes(f, 1);
        var steps = f.State.History.Steps.Count;

        f.Canvas.DeleteSelection();

        Assert.Equal(3, Current(f).Nodes.Length);
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.DeletePathPoints, f.State.History.Steps[^1]);
        Assert.Equal(f.Path.Id, f.State.SelectedShapeId);
    }

    [Fact]
    public void Delete_AllNodes_DeletesTheShape()
    {
        var f = Create(Tool.DirectSelect);
        SelectNodes(f, 0, 1, 2, 3);

        f.Canvas.DeleteSelection();

        Assert.Empty(f.State.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void Delete_WithoutSelectedNodes_DeletesTheShapeAsBefore()
    {
        var f = Create(Tool.Node);

        f.Canvas.DeleteSelection();

        Assert.Empty(f.State.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Equal(HistoryStepType.DeleteShape, f.State.History.Steps[^1]);
    }

    [Fact]
    public void Delete_WithTheSelectTool_StillDeletesTheShape()
    {
        var f = Create(Tool.Select);
        SelectNodes(f, 1);

        f.Canvas.DeleteSelection();

        Assert.Empty(f.State.Document.Layers.SelectMany(l => l.Shapes));
    }

    // ---- arrow keys ----

    [Fact]
    public void ArrowKeys_WithSelectedNodes_MoveTheNodesNotTheShape()
    {
        var f = Create(Tool.Node);
        SelectNodes(f, 1, 2);

        Assert.True(f.Canvas.KeyPressed(CanvasKey.Right, shift: false));
        Assert.True(f.Canvas.KeyPressed(CanvasKey.Down, shift: true));

        var nodes = Current(f).ToDocumentNodes();
        Assert.Equal(301, nodes[1].X, 6);
        Assert.Equal(210, nodes[1].Y, 6);
        Assert.Equal(501, nodes[2].X, 6);
        Assert.Equal(100, nodes[0].X, 6);
        Assert.Equal(200, nodes[0].Y, 6);
    }

    [Fact]
    public void ArrowKeys_Left_And_Up_MoveTheOtherWay()
    {
        var f = Create(Tool.DirectSelect);
        SelectNodes(f, 1);

        f.Canvas.KeyPressed(CanvasKey.Left, shift: true);
        f.Canvas.KeyPressed(CanvasKey.Up, shift: false);

        var node = Current(f).ToDocumentNodes()[1];
        Assert.Equal(290, node.X, 6);
        Assert.Equal(199, node.Y, 6);
    }

    [Fact]
    public void ArrowKeys_Repeated_AreOneStep()
    {
        var f = Create(Tool.Node);
        SelectNodes(f, 1);
        var steps = f.State.History.Steps.Count;

        f.Canvas.KeyPressed(CanvasKey.Right, shift: false);
        f.Canvas.KeyPressed(CanvasKey.Right, shift: false);
        f.Canvas.KeyPressed(CanvasKey.Right, shift: false);

        Assert.Equal(steps + 1, f.State.History.Steps.Count);
    }

    [Fact]
    public void ArrowKeys_WithoutSelectedNodes_MoveTheShapeAsBefore()
    {
        var f = Create(Tool.Node);

        f.Canvas.KeyPressed(CanvasKey.Right, shift: false);

        Assert.Equal(f.Path.X + 1, Current(f).X, 6);
        Assert.Equal(HistoryStepType.NudgeShapes, f.State.History.Steps[^1]);
    }

    [Fact]
    public void ArrowKeys_WithTheSelectTool_MoveTheShapeEvenIfNodesWereSelected()
    {
        var f = Create(Tool.Select);
        SelectNodes(f, 1);

        f.Canvas.KeyPressed(CanvasKey.Right, shift: false);

        Assert.Equal(f.Path.X + 1, Current(f).X, 6);
    }

    // ---- cursor ----

    [Fact]
    public void Cursor_OverANodeWithANodeTool_IsTheOpenHand()
    {
        var f = Create(Tool.Node);
        var raised = new List<string?>();
        f.Canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.Canvas.PointerHover(f.Canvas.OriginX + 300, f.Canvas.OriginY + 200);

        Assert.Equal(CanvasCursorType.OpenHand, f.Canvas.CursorType);
        Assert.Contains(nameof(CanvasAreaViewModel.CursorType), raised);
    }

    [Fact]
    public void Cursor_AwayFromEverything_IsTheArrow()
    {
        var f = Create(Tool.Node);
        f.Canvas.PointerHover(f.Canvas.OriginX + 300, f.Canvas.OriginY + 200);

        f.Canvas.PointerHover(f.Canvas.OriginX + 700, f.Canvas.OriginY + 700);

        Assert.Equal(CanvasCursorType.Arrow, f.Canvas.CursorType);
    }

    [Fact]
    public void Cursor_OverASegmentIsAGrabTargetForTheNodeToolButNotForDirectSelect()
    {
        var node = Create(Tool.Node);
        var direct = Create(Tool.DirectSelect);

        node.Canvas.PointerHover(node.Canvas.OriginX + 200, node.Canvas.OriginY + 201);
        direct.Canvas.PointerHover(direct.Canvas.OriginX + 200, direct.Canvas.OriginY + 201);

        Assert.Equal(CanvasCursorType.OpenHand, node.Canvas.CursorType);
        Assert.Equal(CanvasCursorType.Arrow, direct.Canvas.CursorType);
    }

    [Fact]
    public void Cursor_WhileDraggingANode_IsTheClosedHand_AndTheOpenHandAfterwards()
    {
        var f = Create(Tool.Node);
        var x = f.Canvas.OriginX;
        var y = f.Canvas.OriginY;

        f.Canvas.PointerPressed(x + 300, y + 200);
        f.Canvas.PointerMoved(x + 320, y + 180);
        var during = f.Canvas.CursorType;
        f.Canvas.PointerReleased(x + 320, y + 180);

        Assert.Equal(CanvasCursorType.ClosedHand, during);
        Assert.Equal(CanvasCursorType.OpenHand, f.Canvas.CursorType);
    }

    [Fact]
    public void Cursor_ForOtherTools_IsUnchanged()
    {
        var f = Create(Tool.Select);

        // Inside the selection frame and away from its handles (the middle of the top edge, at x = 300, is a handle).
        f.Canvas.PointerHover(f.Canvas.OriginX + 200, f.Canvas.OriginY + 300);

        Assert.Equal(CanvasCursorType.Arrow, f.Canvas.CursorType);
    }

    [Fact]
    public void Cursor_WhenThePointerLeaves_IsTheArrowAgain()
    {
        var f = Create(Tool.Node);
        f.Canvas.PointerHover(f.Canvas.OriginX + 300, f.Canvas.OriginY + 200);

        f.Canvas.PointerLeft();

        Assert.Equal(CanvasCursorType.Arrow, f.Canvas.CursorType);
    }
}
