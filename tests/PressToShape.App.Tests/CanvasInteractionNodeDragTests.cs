using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionNodeDragTests
{
    // Three nodes at y = 100, the middle one with handles at x = 70 and x = 130.
    private static PathShape Row(NodeType middle = NodeType.Symmetric, double rotation = 0)
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 100),
                new DocumentNode(100, 100, inX: 70, inY: 100, outX: 130, outY: 100, type: middle),
                new DocumentNode(200, 100),
            ]);
        return path with { Rotation = rotation };
    }

    private static (EditorState State, CanvasInteraction Interaction) Create(Tool tool, params Shape[] shapes)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes)) { ActiveTool = tool };
        return (state, new CanvasInteraction(state));
    }

    private static void Drag(CanvasInteraction interaction, double x1, double y1, double x2, double y2, bool shift = false, bool alt = false)
    {
        interaction.PointerPressed(x1, y1, shift);
        interaction.PointerMoved((x1 + x2) / 2, (y1 + y2) / 2, shift, alt);
        interaction.PointerMoved(x2, y2, shift, alt);
        interaction.PointerReleased(x2, y2, shift, alt);
    }

    private static PathShape Current(EditorState state, Shape shape) => Assert.IsType<PathShape>(state.Document.FindShape(shape.Id)!.Shape);

    private static (double X, double Y) OnScreen(PathShape path, int index)
    {
        var node = path.ToDocumentNodes()[index];
        return path.FromLocal(node.X, node.Y);
    }

    private static void SelectNodes(EditorState state, Shape shape, params int[] indices)
    {
        state.Select(shape.Id);
        state.SetNodeSelection(indices.Select(i => new NodeRef(shape.Id, i)));
    }

    // ---- moving nodes ----

    [Fact]
    public void DragANode_MovesItWithItsHandlesAsOneStep()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.DirectSelect, path);
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        Drag(interaction, 100, 100, 120, 70);

        var moved = Current(state, path).ToDocumentNodes()[1];
        Assert.Equal(120, moved.X, 6);
        Assert.Equal(70, moved.Y, 6);
        Assert.Equal(90, moved.InX!.Value, 6);
        Assert.Equal(150, moved.OutX!.Value, 6);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.MovePathPoints, state.History.Steps[^1]);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void DragANode_ThenUndo_PutsItBack()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);

        Drag(interaction, 100, 100, 150, 50);
        state.Undo();

        Assert.Equal(100, Current(state, path).ToDocumentNodes()[1].X, 6);
    }

    [Fact]
    public void DragANode_ThatIsNotSelected_MakesItTheOnlySelectedNode()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.DirectSelect, path);
        SelectNodes(state, path, 0);

        Drag(interaction, 100, 100, 110, 100);

        Assert.Equal([new NodeRef(path.Id, 1)], state.SelectedNodes);
        Assert.Equal(0, Current(state, path).ToDocumentNodes()[0].X, 6);
    }

    [Fact]
    public void DragOneOfSeveralSelectedNodes_MovesAllOfThemByTheSameDistance()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.DirectSelect, path);
        SelectNodes(state, path, 0, 1);

        Drag(interaction, 100, 100, 130, 80);

        var nodes = Current(state, path).ToDocumentNodes();
        Assert.Equal((30, 80), (Math.Round(nodes[0].X, 6), Math.Round(nodes[0].Y, 6)));
        Assert.Equal((130, 80), (Math.Round(nodes[1].X, 6), Math.Round(nodes[1].Y, 6)));
        Assert.Equal(200, nodes[2].X, 6);
        Assert.Equal(2, state.SelectedNodes.Count);
    }

    [Fact]
    public void Shift_LimitsTheMoveToTheLargerAxis()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);

        // Shift is held during the drag, not at the press (a Shift-click only changes the selection).
        interaction.PointerPressed(100, 100);
        interaction.PointerMoved(140, 108, shift: true);
        interaction.PointerReleased(140, 108, shift: true);

        var node = Current(state, path).ToDocumentNodes()[1];
        Assert.Equal(140, node.X, 6);
        Assert.Equal(100, node.Y, 6);
    }

    [Fact]
    public void DragBackToTheStart_MakesNoStep()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(100, 100);
        interaction.PointerMoved(150, 60);
        interaction.PointerMoved(100, 100);
        interaction.PointerReleased(100, 100);

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void ManySmallMoves_DoNotDrift()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);

        interaction.PointerPressed(100, 100);
        for (var i = 1; i <= 50; i++)
        {
            interaction.PointerMoved(100 + i * 0.4, 100);
        }

        interaction.PointerReleased(120, 100);

        Assert.Equal(120, Current(state, path).ToDocumentNodes()[1].X, 6);
    }

    [Fact]
    public void ShiftClickOnANode_DoesNotStartADrag()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        Drag(interaction, 100, 100, 160, 100, shift: true);

        Assert.Equal(steps, state.History.Steps.Count);
        Assert.Equal(100, Current(state, path).ToDocumentNodes()[1].X, 6);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(90)]
    public void DragANode_OfARotatedPath_MovesItByTheDistanceOnTheScreen(double rotation)
    {
        var path = Row(rotation: rotation);
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        var start = OnScreen(path, 1);

        Drag(interaction, start.X, start.Y, start.X + 25, start.Y + 10);

        var after = OnScreen(Current(state, path), 1);
        Assert.Equal(start.X + 25, after.X, 6);
        Assert.Equal(start.Y + 10, after.Y, 6);
        Assert.Equal(OnScreen(path, 0).X, OnScreen(Current(state, path), 0).X, 6);
    }

    // ---- moving handles ----

    [Fact]
    public void DragAHandle_OfASymmetricNode_MirrorsTheOtherHandle()
    {
        var path = Row(NodeType.Symmetric);
        var (state, interaction) = Create(Tool.DirectSelect, path);
        SelectNodes(state, path, 1);
        var steps = state.History.Steps.Count;

        Drag(interaction, 130, 100, 130, 160);

        var node = Current(state, path).ToDocumentNodes()[1];
        Assert.Equal((130, 160), (Math.Round(node.OutX!.Value, 6), Math.Round(node.OutY!.Value, 6)));
        Assert.Equal((70, 40), (Math.Round(node.InX!.Value, 6), Math.Round(node.InY!.Value, 6)));
        Assert.Equal(NodeType.Symmetric, node.Type);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.MovePathHandle, state.History.Steps[^1]);
    }

    [Fact]
    public void DragAHandle_OfASmoothNode_TheOtherKeepsItsLength()
    {
        var path = PathShape.Create(
            [new DocumentNode(0, 100), new DocumentNode(100, 100, inX: 80, inY: 100, outX: 140, outY: 100, type: NodeType.Smooth), new DocumentNode(200, 100)]);
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 1);

        Drag(interaction, 140, 100, 100, 160);

        var node = Current(state, path).ToDocumentNodes()[1];
        Assert.Equal(100, node.InX!.Value, 6);
        Assert.Equal(80, node.InY!.Value, 6);
    }

    [Fact]
    public void DragAHandle_OfACornerNode_MovesOnlyThatHandle()
    {
        var path = Row(NodeType.Corner);
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 1);

        Drag(interaction, 130, 100, 150, 130);

        var node = Current(state, path).ToDocumentNodes()[1];
        Assert.Equal((150, 130), (Math.Round(node.OutX!.Value, 6), Math.Round(node.OutY!.Value, 6)));
        Assert.Equal(70, node.InX!.Value, 6);
    }

    [Fact]
    public void DragAHandle_WithAlt_MovesOnlyThatHandleAndMakesACorner()
    {
        var path = Row(NodeType.Symmetric);
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 1);

        Drag(interaction, 130, 100, 150, 140, alt: true);

        var node = Current(state, path).ToDocumentNodes()[1];
        Assert.Equal(NodeType.Corner, node.Type);
        Assert.Equal(70, node.InX!.Value, 6);
        Assert.Equal(100, node.InY!.Value, 6);
    }

    [Fact]
    public void DragAHandle_ToWithinTwoPixelsOfItsAnchor_RemovesIt()
    {
        var path = Row(NodeType.Symmetric);
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 1);

        Drag(interaction, 130, 100, 101, 101);

        var node = Current(state, path).ToDocumentNodes()[1];
        Assert.False(node.HasOut);
        Assert.Equal(NodeType.Corner, node.Type);
    }

    [Fact]
    public void DragAHandle_KeepsTheDistanceBetweenThePointerAndTheHandle()
    {
        var path = Row(NodeType.Corner);
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 1);

        // The press is 3 units off the handle (130, 100), so the handle ends 3 units off the release point as well.
        Drag(interaction, 133, 100, 153, 100);

        Assert.Equal(150, Current(state, path).ToDocumentNodes()[1].OutX!.Value, 6);
    }

    [Fact]
    public void DragTheHandleOfANeighborTowardASelectedNode_MovesIt()
    {
        var path = PathShape.Create([new DocumentNode(0, 100, outX: 40, outY: 100), new DocumentNode(100, 100), new DocumentNode(200, 100)]);
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 1);

        Drag(interaction, 40, 100, 40, 60);

        Assert.Equal(60, Current(state, path).ToDocumentNodes()[0].OutY!.Value, 6);
    }

    [Fact]
    public void DragAHandle_OfARotatedPath_FollowsThePointerOnTheScreen()
    {
        var path = Row(NodeType.Corner, rotation: 40);
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 1);
        var onScreen = EditablePath.AsOnScreen(path).ToDocumentNodes()[1];
        var handle = (X: onScreen.OutX!.Value, Y: onScreen.OutY!.Value);

        Drag(interaction, handle.X, handle.Y, handle.X + 30, handle.Y - 20);

        var after = EditablePath.AsOnScreen(Current(state, path)).ToDocumentNodes()[1];
        Assert.Equal(handle.X + 30, after.OutX!.Value, 6);
        Assert.Equal(handle.Y - 20, after.OutY!.Value, 6);
    }

    // ---- bending ----

    [Fact]
    public void DragASegment_WithTheNodeTool_BendsItSoThePointFollows()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]);
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        Drag(interaction, 50, 2, 50, 42);

        var bent = Current(state, path);
        Assert.Equal(40, Bezier.Point(bent.Segments()[0], 0.5).Y, 2);
        Assert.Equal(0, bent.ToDocumentNodes()[0].Y, 6);
        Assert.Equal(100, bent.ToDocumentNodes()[1].X, 6);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.BendSegment, state.History.Steps[^1]);
    }

    [Fact]
    public void DragASegment_SelectsItsTwoNodes()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)]);
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);

        Drag(interaction, 100, 50, 130, 50);

        Assert.Equal([1, 2], state.SelectedNodes.Select(n => n.Index).OrderBy(i => i));
    }

    [Fact]
    public void DragASegment_ClosingSegmentOfAClosedPath_Bends()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)], isClosed: true);
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);

        // The closing segment runs from (100, 100) back to (0, 0). Its middle is at (50, 50) and is far from the other segments.
        Drag(interaction, 50, 50, 20, 80);

        var nodes = Current(state, path).ToDocumentNodes();
        Assert.True(nodes[2].HasOut);
        Assert.True(nodes[0].HasIn);
    }

    [Fact]
    public void DragASegment_WithDirectSelect_DoesNotBend()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]);
        var (state, interaction) = Create(Tool.DirectSelect, path);
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        Drag(interaction, 50, 2, 50, 42);

        Assert.Equal(steps, state.History.Steps.Count);
        Assert.False(Current(state, path).ToDocumentNodes()[0].HasOut);
    }

    // ---- rectangles and ellipses ----

    [Fact]
    public void DragACornerOfARectangle_ConvertsItToAPathInTheSameStep()
    {
        var rectangle = new RectangleShape { X = 10, Y = 10, Width = 100, Height = 60, Name = "Box" };
        var (state, interaction) = Create(Tool.DirectSelect, rectangle);
        state.Select(rectangle.Id);
        var steps = state.History.Steps.Count;

        Drag(interaction, 110, 70, 130, 100);

        var path = Assert.IsType<PathShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal("Box", path.Name);
        var corner = path.ToDocumentNodes()[2];
        Assert.Equal((130, 100), (Math.Round(corner.X, 6), Math.Round(corner.Y, 6)));
        Assert.Equal(steps + 1, state.History.Steps.Count);

        state.Undo();
        Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
    }

    [Fact]
    public void ClickingACornerOfARectangle_WithoutMoving_DoesNotConvertIt()
    {
        var rectangle = new RectangleShape { X = 10, Y = 10, Width = 100, Height = 60 };
        var (state, interaction) = Create(Tool.DirectSelect, rectangle);
        state.Select(rectangle.Id);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(110, 70);
        interaction.PointerReleased(110, 70);

        Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void DragANodeOfAnEllipse_ConvertsItAndMovesTheNode()
    {
        var ellipse = new EllipseShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var (state, interaction) = Create(Tool.Node, ellipse);
        state.Select(ellipse.Id);

        Drag(interaction, 50, 0, 50, -30);

        var path = Assert.IsType<PathShape>(state.Document.FindShape(ellipse.Id)!.Shape);
        Assert.Equal(-30, path.Y, 6);
    }

    // ---- interrupted drags and special cases ----

    [Fact]
    public void Abort_DuringANodeDrag_PutsTheDocumentBackAndMakesNoStep()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(100, 100);
        interaction.PointerMoved(160, 60);
        interaction.Abort();

        Assert.Equal(100, Current(state, path).ToDocumentNodes()[1].X, 6);
        Assert.Equal(steps, state.History.Steps.Count);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void Abort_DuringAHandleDragAndABend_PutsTheDocumentBack()
    {
        var path = Row(NodeType.Corner);
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 1);

        interaction.PointerPressed(130, 100);
        interaction.PointerMoved(180, 160);
        interaction.Abort();

        Assert.Equal(130, Current(state, path).ToDocumentNodes()[1].OutX!.Value, 6);
    }

    [Fact]
    public void ChangingTheToolDuringADrag_EndsItWithoutAnException()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);

        interaction.PointerPressed(100, 100);
        interaction.PointerMoved(160, 60);
        state.ActiveTool = Tool.Select;
        interaction.PointerMoved(180, 60);
        interaction.PointerReleased(180, 60);

        Assert.False(state.IsDragInProgress);
        Assert.IsType<PathShape>(state.Document.FindShape(path.Id)!.Shape);
    }

    [Fact]
    public void LockedPath_CannotBeDragged()
    {
        var path = Row() with { IsLocked = true };
        var (state, interaction) = Create(Tool.Node, path);
        state.SetSelection([path.Id]);
        var steps = state.History.Steps.Count;

        Drag(interaction, 100, 100, 150, 100);

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void ClickingOneOfSeveralSelectedNodes_WithoutMoving_StillReducesTheSelection()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 0, 1);

        interaction.PointerPressed(100, 100);
        interaction.PointerReleased(100, 100);

        Assert.Equal([new NodeRef(path.Id, 1)], state.SelectedNodes);
    }

    [Fact]
    public void DraggingOneOfSeveralSelectedNodes_KeepsTheSelectionAfterTheRelease()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        SelectNodes(state, path, 0, 1);

        Drag(interaction, 100, 100, 120, 100);

        Assert.Equal(2, state.SelectedNodes.Count);
    }
}
