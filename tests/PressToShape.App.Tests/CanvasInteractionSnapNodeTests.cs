using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSnapNodeTests
{
    // Three nodes at y = 100: (0, 100), (100, 100) with handles at x 70 and 130, and (200, 100).
    private static PathShape Row() =>
        PathShape.Create(
            [
                new DocumentNode(0, 100),
                new DocumentNode(100, 100, inX: 70, inY: 100, outX: 130, outY: 100, type: NodeType.Symmetric),
                new DocumentNode(200, 100),
            ]);

    private static (EditorState State, CanvasInteraction Interaction) Create(params Shape[] shapes)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes)) { ActiveTool = Tool.DirectSelect };
        return (state, new CanvasInteraction(state));
    }

    private static void Drag(CanvasInteraction interaction, double x1, double y1, double x2, double y2, bool shift = false, bool ctrl = false)
    {
        interaction.PointerPressed(x1, y1, suspendSnap: ctrl);
        interaction.PointerMoved(x2, y2, shift, suspendSnap: ctrl);
        interaction.PointerReleased(x2, y2, shift, suspendSnap: ctrl);
    }

    private static void SelectNodes(EditorState state, Shape shape, params int[] indices)
    {
        state.Select(shape.Id);
        state.SetNodeSelection(indices.Select(i => new NodeRef(shape.Id, i)));
    }

    private static DocumentNode NodeOf(EditorState state, Shape shape, int index) =>
        Assert.IsType<PathShape>(state.Document.FindShape(shape.Id)!.Shape).ToDocumentNodes()[index];

    [Fact]
    public void Node_SnapToGrid_JumpsOntoTheLine()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToGrid = true;
        state.Select(path.Id);

        Drag(interaction, 100, 100, 123, 62);

        var node = NodeOf(state, path, 1);
        Assert.Equal((120, 60), (Math.Round(node.X, 6), Math.Round(node.Y, 6)));
    }

    [Fact]
    public void Node_SnapToObjects_LinesUpWithANodeOfAnotherPath()
    {
        var path = Row();
        var other = PathShape.Create([new DocumentNode(350, 300), new DocumentNode(500, 400)]);
        var (state, interaction) = Create(path, other);
        state.Snapping.SnapToObjects = true;
        state.Select(path.Id);

        Drag(interaction, 100, 100, 347, 180);

        Assert.Equal(350, NodeOf(state, path, 1).X, 6);
        Assert.Equal(180, NodeOf(state, path, 1).Y, 6);
    }

    [Fact]
    public void Node_SnapToObjects_LinesUpWithAnotherNodeOfTheSamePath()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToObjects = true;
        SelectNodes(state, path, 0);

        Drag(interaction, 0, 100, 103, 150);

        // The node at 100 is a target, so x lands on it.
        Assert.Equal(100, NodeOf(state, path, 0).X, 6);
    }

    [Fact]
    public void Node_TheDraggedNodeAndTheOwnBoxAreNoTargets()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToObjects = true;
        SelectNodes(state, path, 2);

        // The last node starts at 200, which is also the right edge of the path's box. Neither pulls it back.
        Drag(interaction, 200, 100, 206, 100);

        Assert.Equal(206, NodeOf(state, path, 2).X, 6);
    }

    [Fact]
    public void Node_SeveralNodes_MoveByTheSameSnappedDistance()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToGrid = true;
        SelectNodes(state, path, 0, 1);

        Drag(interaction, 100, 100, 114, 100);

        Assert.Equal(110, NodeOf(state, path, 1).X, 6);
        Assert.Equal(10, NodeOf(state, path, 0).X, 6);
    }

    [Fact]
    public void Node_ShiftAxisLock_StillWorksAfterSnapping()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToGrid = true;
        SelectNodes(state, path, 1);

        Drag(interaction, 100, 100, 143, 112, shift: true);

        var node = NodeOf(state, path, 1);
        Assert.Equal(140, node.X, 6);
        Assert.Equal(100, node.Y, 6);
    }

    [Fact]
    public void Handle_SnapToGrid_TheEndOfTheHandleLandsOnTheLine()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToGrid = true;
        SelectNodes(state, path, 1);

        Drag(interaction, 130, 100, 147, 103);

        var node = NodeOf(state, path, 1);
        Assert.Equal(150, node.OutX!.Value, 6);
        Assert.Equal(100, node.OutY!.Value, 6);
    }

    [Fact]
    public void Handle_TheNodeThatOwnsTheHandleIsNoTarget()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToObjects = true;
        SelectNodes(state, path, 1);

        // The handle end moves to x 104, near its own node at 100: that node must not pull it.
        Drag(interaction, 130, 100, 104, 160);

        Assert.Equal(104, NodeOf(state, path, 1).OutX!.Value, 6);
    }

    [Fact]
    public void Node_CtrlSuspendsSnapping()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToGrid = true;
        state.Select(path.Id);

        Drag(interaction, 100, 100, 123, 62, ctrl: true);

        var node = NodeOf(state, path, 1);
        Assert.Equal((123, 62), (Math.Round(node.X, 6), Math.Round(node.Y, 6)));
    }

    [Fact]
    public void Node_ShowsAGuideWhileDraggingAndClearsItOnRelease()
    {
        var path = Row();
        var other = PathShape.Create([new DocumentNode(350, 300), new DocumentNode(500, 400)]);
        var (state, interaction) = Create(path, other);
        state.Snapping.SnapToObjects = true;
        state.Select(path.Id);
        interaction.PointerPressed(100, 100);

        interaction.PointerMoved(347, 180);

        Assert.NotEmpty(state.SnapGuides);
        interaction.PointerReleased(347, 180);
        Assert.Empty(state.SnapGuides);
    }

    [Fact]
    public void Node_Aborted_RestoresTheNodesAndClearsTheGuides()
    {
        var path = Row();
        var other = PathShape.Create([new DocumentNode(350, 300), new DocumentNode(500, 400)]);
        var (state, interaction) = Create(path, other);
        state.Snapping.SnapToObjects = true;
        state.Select(path.Id);
        interaction.PointerPressed(100, 100);
        interaction.PointerMoved(347, 180);

        interaction.Abort();

        Assert.Equal(100, NodeOf(state, path, 1).X, 6);
        Assert.Empty(state.SnapGuides);
    }

    [Fact]
    public void Node_IsOneUndoStep()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToGrid = true;
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        Drag(interaction, 100, 100, 123, 62);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.MovePathPoints, state.History.Steps[^1]);
    }

    [Fact]
    public void Bend_DoesNotSnap()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Snapping.SnapToGrid = true;
        state.Snapping.SnapToObjects = true;
        state.Select(path.Id);

        interaction.PointerPressed(50, 100);
        interaction.PointerMoved(53, 123);
        Assert.Empty(state.SnapGuides);
        interaction.PointerReleased(53, 123);

        Assert.Empty(state.SnapGuides);
    }
}
