using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSmartNodeTests
{
    // Three nodes at y = 100, the middle one with handles at x 70 and 130.
    private static PathShape Row() =>
        PathShape.Create(
            [
                new DocumentNode(0, 100),
                new DocumentNode(100, 100, inX: 70, inY: 100, outX: 130, outY: 100, type: NodeType.Symmetric),
                new DocumentNode(200, 100),
            ]);

    private static PathShape Line(double x1, double y1, double x2, double y2) => PathShape.Create([new DocumentNode(x1, y1), new DocumentNode(x2, y2)]);

    // Other paths: a line whose middle is (400, 0), and two lines that cross at (400, 400) (not the middle of either).
    private static Shape[] Others() => [Line(300, 0, 500, 0), Line(300, 300, 600, 600), Line(300, 600, 450, 300)];

    private static (EditorState State, CanvasInteraction Interaction) Create(PathShape path, bool smart = true)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [path, .. Others()])) { ActiveTool = Tool.DirectSelect };
        state.Snapping.SmartGuides = smart;
        return (state, new CanvasInteraction(state));
    }

    private static void SelectNodes(EditorState state, Shape shape, params int[] indices)
    {
        state.Select(shape.Id);
        state.SetNodeSelection(indices.Select(i => new NodeRef(shape.Id, i)));
    }

    private static DocumentNode NodeOf(EditorState state, Shape shape, int index) =>
        Assert.IsType<PathShape>(state.Document.FindShape(shape.Id)!.Shape).ToDocumentNodes()[index];

    private static void Drag(CanvasInteraction interaction, double x1, double y1, double x2, double y2, bool ctrl = false)
    {
        interaction.PointerPressed(x1, y1, suspendSnap: ctrl);
        interaction.PointerMoved(x2, y2, suspendSnap: ctrl);
        interaction.PointerReleased(x2, y2, suspendSnap: ctrl);
    }

    [Fact]
    public void Node_SnapsToTheMiddleOfASegmentOfAnotherPath()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Select(path.Id);

        Drag(interaction, 100, 100, 403, 3);

        var node = NodeOf(state, path, 1);
        Assert.Equal((400.0, 0.0), (Math.Round(node.X, 6), Math.Round(node.Y, 6)));
    }

    [Fact]
    public void Node_SnapsToTheCrossingOfTwoOtherPaths()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Select(path.Id);

        Drag(interaction, 100, 100, 404, 398);

        var node = NodeOf(state, path, 1);
        Assert.Equal(400, node.X, 3);
        Assert.Equal(400, node.Y, 3);
    }

    [Fact]
    public void Node_TheEditedPathsOwnSegmentsAreNoTargets()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        SelectNodes(state, path, 2);

        // The middle of the first segment of this path is (50, 100). The last node is dragged near it and must not snap to it.
        Drag(interaction, 200, 100, 53, 103);

        var node = NodeOf(state, path, 2);
        Assert.Equal((53.0, 103.0), (Math.Round(node.X, 6), Math.Round(node.Y, 6)));
    }

    [Fact]
    public void SeveralNodes_KeepTheirOffsets()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        SelectNodes(state, path, 0, 1);

        Drag(interaction, 100, 100, 403, 3);

        Assert.Equal(400, NodeOf(state, path, 1).X, 6);
        Assert.Equal(300, NodeOf(state, path, 0).X, 6);
        Assert.Equal(0, NodeOf(state, path, 0).Y, 6);
    }

    [Fact]
    public void Handle_EndSnapsToAPathPoint()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        SelectNodes(state, path, 1);

        Drag(interaction, 130, 100, 403, 3);

        var node = NodeOf(state, path, 1);
        Assert.Equal(400, node.OutX!.Value, 6);
        Assert.Equal(0, node.OutY!.Value, 6);
    }

    [Fact]
    public void Marker_ShowsWhileDraggingAndClearsOnReleaseAndAbort()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Select(path.Id);
        interaction.PointerPressed(100, 100);

        interaction.PointerMoved(403, 3);

        var marker = Assert.Single(state.SmartMarkers);
        Assert.Equal(SmartMarkerType.PathPoint, marker.Type);
        Assert.Equal((400.0, 0.0), (marker.From, marker.Cross));
        interaction.Abort();
        Assert.Empty(state.SmartMarkers);
        Assert.Equal(100, NodeOf(state, path, 1).X, 6);

        interaction.PointerPressed(100, 100);
        interaction.PointerMoved(403, 3);
        interaction.PointerReleased(403, 3);
        Assert.Empty(state.SmartMarkers);
    }

    [Fact]
    public void Off_Ctrl_AndBending_DoNotSnap()
    {
        var offPath = Row();
        var (offState, offInteraction) = Create(offPath, smart: false);
        offState.Select(offPath.Id);
        Drag(offInteraction, 100, 100, 403, 3);
        Assert.Equal(403, NodeOf(offState, offPath, 1).X, 6);

        var ctrlPath = Row();
        var (ctrlState, ctrlInteraction) = Create(ctrlPath);
        ctrlState.Select(ctrlPath.Id);
        Drag(ctrlInteraction, 100, 100, 403, 3, ctrl: true);
        Assert.Equal(403, NodeOf(ctrlState, ctrlPath, 1).X, 6);

        var bendPath = Row();
        var (bendState, bendInteraction) = Create(bendPath);
        bendState.Select(bendPath.Id);
        bendInteraction.PointerPressed(50, 100);
        bendInteraction.PointerMoved(403, 3);
        Assert.Empty(bendState.SmartMarkers);
        bendInteraction.PointerReleased(403, 3);
    }

    [Fact]
    public void Drag_IsOneUndoStep()
    {
        var path = Row();
        var (state, interaction) = Create(path);
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        Drag(interaction, 100, 100, 403, 3);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.MovePathPoints, state.History.Steps[^1]);
    }
}
