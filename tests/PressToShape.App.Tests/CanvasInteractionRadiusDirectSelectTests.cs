using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionRadiusDirectSelectTests
{
    private static RectangleShape Rect(CornerRadii radii = default) =>
        new() { Id = Guid.NewGuid(), X = 100, Y = 50, Width = 200, Height = 100, Radii = radii };

    private static (EditorState State, CanvasInteraction Interaction, RectangleShape Rectangle) Create(Tool tool, RectangleShape rectangle)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShape(document.Layers[0].Id, rectangle)) { ActiveTool = tool };
        state.Select(rectangle.Id);
        return (state, new CanvasInteraction(state), rectangle);
    }

    private static void Drag(CanvasInteraction interaction, double x1, double y1, double x2, double y2)
    {
        interaction.PointerPressed(x1, y1);
        interaction.PointerMoved((x1 + x2) / 2, (y1 + y2) / 2);
        interaction.PointerMoved(x2, y2);
        interaction.PointerReleased(x2, y2);
    }

    [Fact]
    public void DragARadiusHandle_WithDirectSelect_ChangesTheRadii()
    {
        var (state, interaction, rectangle) = Create(Tool.DirectSelect, Rect());

        Drag(interaction, 124, 74, 137, 87);

        var current = Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal(CornerRadii.Uniform(25), current.Radii);
        Assert.Equal(HistoryStepType.ChangeCornerRadius, state.History.Steps[^1]);
    }

    [Fact]
    public void DragWithDirectSelect_ShiftChangesOnlyThatCorner()
    {
        var (state, interaction, rectangle) = Create(Tool.DirectSelect, Rect());

        interaction.PointerPressed(124, 74);
        interaction.PointerMoved(137, 87, shift: true);
        interaction.PointerReleased(137, 87, shift: true);

        var current = Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal(new CornerRadii(25, 0, 0, 0), current.Radii);
    }

    [Fact]
    public void Press_OnTheSameSpotWithTheNodeTool_StartsNoRadiusDragAndDoesNotChangeTheRadii()
    {
        var (state, interaction, rectangle) = Create(Tool.Node, Rect());

        Drag(interaction, 124, 74, 137, 87);

        Assert.False(interaction.IsRadiusDragging);
        var current = Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.True(current.Radii.IsZero);
    }

    [Fact]
    public void Press_OnARadiusHandleWithDirectSelect_WinsOverANodeAtTheSameSpot()
    {
        // The node of the square corner is at (100, 50). With a finger the radius handle at (124, 74) is grabbed from 36 px.
        var (state, interaction, rectangle) = Create(Tool.DirectSelect, Rect());

        interaction.PointerPressed(110, 60, false, InputPointerType.Touch);

        Assert.True(interaction.IsRadiusDragging);
        interaction.PointerReleased(110, 60);
        Assert.Empty(state.SelectedNodes);
        Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
    }

    [Fact]
    public void Press_OnANodeOfASquareRectangle_StillSelectsTheNode()
    {
        var (state, interaction, rectangle) = Create(Tool.DirectSelect, Rect());

        interaction.PointerPressed(100, 50);
        interaction.PointerReleased(100, 50);

        Assert.False(interaction.IsRadiusDragging);
        Assert.Equal([new NodeRef(rectangle.Id, 0)], state.SelectedNodes);
    }

    [Fact]
    public void DragANodeOfARoundedRectangle_ConvertsItToALookAlikePathInOneStep()
    {
        var (state, interaction, rectangle) = Create(Tool.DirectSelect, Rect(CornerRadii.Uniform(20)));
        var steps = state.History.Steps.Count;
        var expected = ShapeConversion.ToPath(rectangle)!;

        // The first node of the rounded outline is where the left edge meets the arc of the top left corner: (100, 70).
        Drag(interaction, 100, 70, 90, 70);

        var path = Assert.IsType<PathShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal(expected.SubPaths[0].Nodes.Length, path.SubPaths[0].Nodes.Length);
        Assert.Equal(8, path.SubPaths[0].Nodes.Length);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.MovePathPoints, state.History.Steps[^1]);
        Assert.Equal(rectangle.Id, path.Id);
    }

    [Fact]
    public void Undo_AfterEditingANodeOfARoundedRectangle_GivesBackTheRectangleWithItsRadii()
    {
        var (state, interaction, rectangle) = Create(Tool.DirectSelect, Rect(new CornerRadii(20, 0, 10, 30)));
        var (hx, hy) = (100.0, 50 + 20);

        Drag(interaction, hx, hy, hx - 10, hy);
        state.Undo();

        var back = Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal(new CornerRadii(20, 0, 10, 30), back.Radii);
        Assert.Equal(rectangle.Bounds, back.Bounds);
    }

    [Fact]
    public void RoundedRectangle_ShowsAllNodesOfItsOutlineAsSelectable()
    {
        var (state, _, rectangle) = Create(Tool.DirectSelect, Rect(CornerRadii.Uniform(20)));

        state.SetNodeSelection(Enumerable.Range(0, 8).Select(index => new NodeRef(rectangle.Id, index)));

        Assert.Equal(8, state.SelectedNodes.Count);
    }
}
