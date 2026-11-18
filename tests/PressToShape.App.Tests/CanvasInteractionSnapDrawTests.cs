using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSnapDrawTests
{
    private static (EditorState State, CanvasInteraction Interaction) Create(Tool tool = Tool.Rectangle, params Shape[] existing)
    {
        var document = Document.Create("Layer");
        document = document.AddShapes(document.Layers[0].Id, existing);
        var state = new EditorState(document) { ActiveTool = tool };
        return (state, new CanvasInteraction(state));
    }

    private static void Draw(CanvasInteraction interaction, double fromX, double fromY, double toX, double toY, bool shift = false, bool alt = false, bool ctrl = false)
    {
        interaction.PointerPressed(fromX, fromY, suspendSnap: ctrl);
        interaction.PointerMoved(toX, toY, shift, alt, ctrl);
        interaction.PointerReleased(toX, toY, shift, alt, ctrl);
    }

    private static Shape Drawn(EditorState state) => state.Document.Layers[0].Shapes.Last();

    [Fact]
    public void Rectangle_SnapToGrid_BothCornersSnapOnBothAxes()
    {
        var (state, interaction) = Create();
        state.Snapping.SnapToGrid = true;

        Draw(interaction, 103, 98, 251, 203);

        var shape = Drawn(state);
        Assert.Equal((100, 100, 150, 100), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Ellipse_SnapToGrid_Snaps()
    {
        var (state, interaction) = Create(Tool.Ellipse);
        state.Snapping.SnapToGrid = true;

        Draw(interaction, 103, 98, 251, 203);

        var shape = Assert.IsType<EllipseShape>(Drawn(state));
        Assert.Equal((100, 100, 150, 100), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Rectangle_SnapToObjects_CornerLinesUpWithAnotherShape()
    {
        var (state, interaction) = Create(Tool.Rectangle, new RectangleShape { X = 400, Y = 300, Width = 50, Height = 50 });
        state.Snapping.SnapToObjects = true;

        Draw(interaction, 100, 100, 396, 100 + 80);

        Assert.Equal(300, Drawn(state).Width, 9);
    }

    [Fact]
    public void Rectangle_ShiftAfterSnapping_KeepsASquare()
    {
        var (state, interaction) = Create();
        state.Snapping.SnapToGrid = true;

        // The end snaps to (250, 180): the square takes the larger side, 150.
        Draw(interaction, 103, 98, 253, 181, shift: true);

        var shape = Drawn(state);
        Assert.Equal((100, 100, 150, 150), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Rectangle_AltAfterSnapping_KeepsTheCenterAtTheSnappedStart()
    {
        var (state, interaction) = Create();
        state.Snapping.SnapToGrid = true;

        Draw(interaction, 203, 198, 241, 232, alt: true);

        var shape = Drawn(state);
        Assert.Equal(200, shape.CenterX, 9);
        Assert.Equal(200, shape.CenterY, 9);
        Assert.Equal(80, shape.Width, 9);
        Assert.Equal(60, shape.Height, 9);
    }

    [Fact]
    public void Rectangle_CtrlSuspendsSnapping()
    {
        var (state, interaction) = Create();
        state.Snapping.SnapToGrid = true;

        Draw(interaction, 103, 98, 251, 203, ctrl: true);

        var shape = Drawn(state);
        Assert.Equal((103, 98, 148, 105), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Rectangle_BothSnapsOff_DrawsAsBefore()
    {
        var (state, interaction) = Create();

        Draw(interaction, 103, 98, 251, 203);

        var shape = Drawn(state);
        Assert.Equal((103, 98, 148, 105), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Rectangle_IsOneUndoStepAndRemovesTheGuides()
    {
        var (state, interaction) = Create(Tool.Rectangle, new RectangleShape { X = 400, Y = 300, Width = 50, Height = 50 });
        state.Snapping.SnapToObjects = true;
        var steps = state.History.Steps.Count;
        interaction.PointerPressed(100, 100);

        interaction.PointerMoved(396, 180);
        Assert.NotEmpty(state.SnapGuides);
        interaction.PointerReleased(396, 180);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.DrawRectangle, state.History.Steps.Last());
        Assert.Empty(state.SnapGuides);
    }

    [Fact]
    public void Rectangle_Cancelled_RemovesTheGuides()
    {
        var (state, interaction) = Create(Tool.Rectangle, new RectangleShape { X = 400, Y = 300, Width = 50, Height = 50 });
        state.Snapping.SnapToObjects = true;
        interaction.PointerPressed(100, 100);
        interaction.PointerMoved(396, 180);

        interaction.Cancel();

        Assert.Empty(state.SnapGuides);
    }
}
