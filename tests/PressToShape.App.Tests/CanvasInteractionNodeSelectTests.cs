using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionNodeSelectTests
{
    // Four nodes on a line at y = 100: x = 0, 100, 200, 300. The path is filled (closed) so that its inside can be clicked.
    private static PathShape Row()
    {
        return PathShape.Create(
            [new DocumentNode(0, 100), new DocumentNode(100, 100), new DocumentNode(200, 100), new DocumentNode(300, 300)],
            isClosed: true);
    }

    private static (EditorState State, CanvasInteraction Interaction) Create(Tool tool, params Shape[] shapes)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes)) { ActiveTool = tool };
        return (state, new CanvasInteraction(state));
    }

    private static void Click(CanvasInteraction interaction, double x, double y, bool shift = false, InputPointerType type = InputPointerType.Mouse)
    {
        interaction.PointerPressed(x, y, shift, type);
        interaction.PointerReleased(x, y, shift);
    }

    private static void Drag(CanvasInteraction interaction, double x1, double y1, double x2, double y2, bool shift = false)
    {
        interaction.PointerPressed(x1, y1, shift);
        interaction.PointerMoved((x1 + x2) / 2, (y1 + y2) / 2);
        interaction.PointerMoved(x2, y2);
        interaction.PointerReleased(x2, y2, shift);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Click_OnANode_SelectsOnlyThatNode(bool nodeTool)
    {
        var path = Row();
        var (state, interaction) = Create(nodeTool ? Tool.Node : Tool.DirectSelect, path);
        state.Select(path.Id);

        Click(interaction, 101, 99);

        Assert.Equal([new NodeRef(path.Id, 1)], state.SelectedNodes);
    }

    [Fact]
    public void Click_OnAnotherNode_ReplacesTheSelection()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        Click(interaction, 0, 100);

        Click(interaction, 200, 100);

        Assert.Equal([new NodeRef(path.Id, 2)], state.SelectedNodes);
    }

    [Fact]
    public void ShiftClick_AddsAndRemovesNodes()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.DirectSelect, path);
        state.Select(path.Id);
        Click(interaction, 0, 100);

        Click(interaction, 100, 100, shift: true);
        Assert.Equal(2, state.SelectedNodes.Count);

        Click(interaction, 0, 100, shift: true);
        Assert.Equal([new NodeRef(path.Id, 1)], state.SelectedNodes);
    }

    [Fact]
    public void Click_OnOneOfSeveralSelectedNodesWithoutAMove_SelectsOnlyThatNode()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 0), new NodeRef(path.Id, 1), new NodeRef(path.Id, 2)]);

        Click(interaction, 100, 100);

        Assert.Equal([new NodeRef(path.Id, 1)], state.SelectedNodes);
    }

    [Fact]
    public void Press_OnOneOfSeveralSelectedNodes_KeepsTheSelectionUntilTheButtonGoesUp()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 0), new NodeRef(path.Id, 1)]);

        interaction.PointerPressed(100, 100);

        Assert.Equal(2, state.SelectedNodes.Count);
    }

    [Fact]
    public void Click_OnANodeOfAnUnselectedPath_DoesNotSelectNodes()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);

        // Nothing is selected, so no nodes are shown: the click picks the shape under the pointer instead (the node lies on its edge).
        Click(interaction, 100, 100);

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void Click_InsideAShape_PicksItAndClearsTheNodes()
    {
        var path = Row();
        var rectangle = new RectangleShape { X = 500, Y = 500, Width = 100, Height = 100 };
        var (state, interaction) = Create(Tool.DirectSelect, path, rectangle);
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        Click(interaction, 550, 550);

        Assert.Equal(rectangle.Id, state.SelectedShapeId);
        Assert.Empty(state.SelectedNodes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Click_OnAShapeInsideAGroup_PicksTheShapeWithoutEnteringTheGroup(bool nodeTool)
    {
        var tool = nodeTool ? Tool.Node : Tool.DirectSelect;
        var first = new RectangleShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var second = new RectangleShape { X = 200, Y = 0, Width = 100, Height = 100 };
        var group = new GroupShape { Children = [first, second] };
        var (state, interaction) = Create(tool, group);

        Click(interaction, 250, 50);

        Assert.Equal(second.Id, state.SelectedShapeId);
        Assert.Null(state.EnteredGroupId);
    }

    [Fact]
    public void Click_OnAShapeInsideALockedGroup_DoesNothing()
    {
        var inner = new RectangleShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var group = new GroupShape { Children = [inner], IsLocked = true };
        var (state, interaction) = Create(Tool.DirectSelect, group);

        Click(interaction, 50, 50);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void ShiftClick_OnAnotherShape_AddsItToTheSelection()
    {
        var first = new RectangleShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var second = new RectangleShape { X = 200, Y = 0, Width = 100, Height = 100 };
        var (state, interaction) = Create(Tool.DirectSelect, first, second);
        Click(interaction, 50, 50);

        Click(interaction, 250, 50, shift: true);

        Assert.Equal(2, state.SelectedShapeIds.Length);
    }

    [Fact]
    public void Drag_OnAShape_DoesNotMoveIt()
    {
        var rectangle = new RectangleShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var (state, interaction) = Create(Tool.DirectSelect, rectangle);

        Drag(interaction, 50, 50, 150, 150);

        Assert.Equal(0, ((RectangleShape)state.Document.FindShape(rectangle.Id)!.Shape).X);
        Assert.Equal(rectangle.Id, state.SelectedShapeId);
    }

    [Fact]
    public void RubberBand_AroundNodesOfTheSelectedPath_SelectsThem()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);

        // The box starts on empty space and covers the nodes at x = 100 and x = 200 on y = 100, not the ones at x = 0 and x = 300.
        Drag(interaction, 50, 60, 250, 140);

        Assert.Equal([new NodeRef(path.Id, 1), new NodeRef(path.Id, 2)], state.SelectedNodes.OrderBy(n => n.Index));
    }

    [Fact]
    public void RubberBand_WithShift_AddsToTheSelectedNodes()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.DirectSelect, path);
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        Drag(interaction, 150, 60, 250, 140, shift: true);

        Assert.Equal([0, 2], state.SelectedNodes.Select(n => n.Index).OrderBy(i => i));
    }

    [Fact]
    public void RubberBand_ThatStaysTiny_ClearsTheNodeSelection()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Node, path);
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 1)]);

        Click(interaction, 700, 700);

        Assert.Empty(state.SelectedNodes);
        Assert.Equal(path.Id, state.SelectedShapeId);
    }

    [Fact]
    public void RubberBand_WithNothingSelected_SelectsShapes()
    {
        var first = new RectangleShape { X = 0, Y = 0, Width = 50, Height = 50 };
        var second = new RectangleShape { X = 400, Y = 400, Width = 50, Height = 50 };
        var (state, interaction) = Create(Tool.DirectSelect, first, second);

        Drag(interaction, -10, -10, 100, 100);

        Assert.Equal(first.Id, state.SelectedShapeId);
    }

    [Fact]
    public void SelectingNodes_OfSeveralSelectedPaths_WorksForBoth()
    {
        var first = Row();
        var second = Row();
        var (state, interaction) = Create(Tool.Node, first, second);
        state.SetSelection([first.Id, second.Id]);

        Drag(interaction, 50, 60, 250, 140);

        Assert.Equal(4, state.SelectedNodes.Count);
        Assert.Equal(2, state.SelectedNodes.Select(n => n.ShapeId).Distinct().Count());
    }

    [Fact]
    public void HiddenOrLockedPath_ShowsAndSelectsNoNodes()
    {
        var locked = Row() with { IsLocked = true };
        var (state, interaction) = Create(Tool.Node, locked);
        state.SetSelection([locked.Id]);

        Click(interaction, 100, 100);

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void Rectangle_ShowsItsFourCornersAsNodes_AndStaysARectangle()
    {
        var rectangle = new RectangleShape { X = 10, Y = 10, Width = 100, Height = 60 };
        var (state, interaction) = Create(Tool.DirectSelect, rectangle);
        state.Select(rectangle.Id);

        Click(interaction, 110, 70);

        Assert.Equal([new NodeRef(rectangle.Id, 2)], state.SelectedNodes);
        Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
    }

    [Fact]
    public void Ellipse_ShowsItsFourNodes()
    {
        var ellipse = new EllipseShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var (state, interaction) = Create(Tool.Node, ellipse);
        state.Select(ellipse.Id);

        Click(interaction, 50, 0);

        Assert.Equal([new NodeRef(ellipse.Id, 0)], state.SelectedNodes);
    }

    [Fact]
    public void PathInsideASelectedGroup_ShowsItsNodes()
    {
        var path = Row();
        var group = new GroupShape { Children = [path, new RectangleShape { X = 600, Y = 600, Width = 10, Height = 10 }] };
        var (state, interaction) = Create(Tool.Node, group);
        state.Select(group.Id);

        Click(interaction, 100, 100);

        Assert.Equal([new NodeRef(path.Id, 1)], state.SelectedNodes);
    }

    [Fact]
    public void Touch_UsesTheLargerGrabArea()
    {
        var path = Row();
        var (mouseState, mouse) = Create(Tool.Node, path);
        var (touchState, touch) = Create(Tool.Node, path);
        mouseState.Select(path.Id);
        touchState.Select(path.Id);

        Click(mouse, 100, 120, type: InputPointerType.Mouse);
        Click(touch, 100, 120, type: InputPointerType.Touch);

        Assert.Empty(mouseState.SelectedNodes);
        Assert.Equal([new NodeRef(path.Id, 1)], touchState.SelectedNodes);
    }

    [Fact]
    public void Zoom_ChangesTheGrabDistanceInDocumentUnits()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.DirectSelect, path);
        state.Select(path.Id);
        state.SetZoom(800);

        // At 800 percent 3 document units are 24 screen pixels: too far for a mouse. At 100 percent it would be close enough.
        // (Direct Select, because with the Node tool the point lies on a segment, which is a hit of its own.)
        Click(interaction, 103, 100);

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void SelectTool_AndPen_AreNotAffected()
    {
        var path = Row();
        var (state, interaction) = Create(Tool.Select, path);
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 1)]);

        Click(interaction, 100, 100);

        // The Select tool does not touch the node selection by itself (it is cleared only when the selection or the tool changes).
        Assert.Equal(path.Id, state.SelectedShapeId);
    }

    [Fact]
    public void ChangingTheShapeSelection_ClearsTheNodeSelection()
    {
        var path = Row();
        var rectangle = new RectangleShape { X = 500, Y = 500, Width = 50, Height = 50 };
        var (state, interaction) = Create(Tool.DirectSelect, path, rectangle);
        state.Select(path.Id);
        Click(interaction, 100, 100);

        Click(interaction, 520, 520);

        Assert.Empty(state.SelectedNodes);
    }
}
