using System;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionGroupTests
{
    // Two squares side by side in a group G at x 100..130 and 160..190, and a plain square P far away.
    private sealed record Scene(EditorState State, CanvasInteraction Interaction, GroupShape Group, RectangleShape A, RectangleShape B, RectangleShape Plain);

    private static RectangleShape Square(double x, double y = 100, double rotation = 0) =>
        new() { X = x, Y = y, Width = 30, Height = 30, Rotation = rotation };

    private static Scene Create(double rotationOfA = 0)
    {
        var a = Square(100, rotation: rotationOfA);
        var b = Square(160);
        var plain = Square(400);
        var group = new GroupShape { Children = [a, b] };
        var document = Document.Create("Layer");
        document = document.AddShapes(document.Layers[0].Id, [group, plain]);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        return new Scene(state, new CanvasInteraction(state), group, a, b, plain);
    }

    private static void Click(CanvasInteraction interaction, double x, double y)
    {
        interaction.PointerPressed(x, y);
        interaction.PointerReleased(x, y);
    }

    private static Shape Find(EditorState state, Guid id) => state.Document.FindShape(id)!.Shape;

    [Fact]
    public void Click_OnAMember_SelectsTheOutermostGroup()
    {
        var scene = Create();

        Click(scene.Interaction, 110, 110);

        Assert.Equal([scene.Group.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Click_InTheGapOfAGroup_SelectsNothing()
    {
        var scene = Create();
        Click(scene.Interaction, 110, 110);

        Click(scene.Interaction, 145, 115);

        Assert.False(scene.State.HasSelection);
    }

    [Fact]
    public void ShiftClick_OnAMemberAndAPlainShape_TogglesTheGroupAndTheShape()
    {
        var scene = Create();
        Click(scene.Interaction, 410, 110);

        scene.Interaction.PointerPressed(110, 110, shift: true);
        scene.Interaction.PointerReleased(110, 110, shift: true);

        Assert.Equal([scene.Plain.Id, scene.Group.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void DoubleClick_OnAGroup_EntersItAndSelectsTheChildUnderThePointer()
    {
        var scene = Create();
        Click(scene.Interaction, 170, 110);

        scene.Interaction.DoubleClicked(170, 110);

        Assert.Equal(scene.Group.Id, scene.State.EnteredGroupId);
        Assert.Equal([scene.B.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Click_AfterEntering_SelectsChildrenAndIgnoresShapesOutsideTheGroup()
    {
        var scene = Create();
        scene.Interaction.DoubleClicked(110, 110);

        Click(scene.Interaction, 170, 110);
        Assert.Equal([scene.B.Id], scene.State.SelectedShapeIds.ToArray());

        Click(scene.Interaction, 410, 110);
        Assert.Null(scene.State.EnteredGroupId);
    }

    [Fact]
    public void DoubleClick_OnAPlainShape_DoesNothing()
    {
        var scene = Create();

        scene.Interaction.DoubleClicked(410, 110);

        Assert.Null(scene.State.EnteredGroupId);
    }

    [Fact]
    public void DoubleClick_OnANestedGroupInsideTheEnteredGroup_EntersTheInnerGroup()
    {
        var inner = new GroupShape { Children = [Square(100), Square(140)] };
        var outer = new GroupShape { Children = [inner, Square(200)] };
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [outer])) { ActiveTool = Tool.Select };
        var interaction = new CanvasInteraction(state);

        interaction.DoubleClicked(110, 110);
        Assert.Equal(outer.Id, state.EnteredGroupId);
        Assert.Equal([inner.Id], state.SelectedShapeIds.ToArray());

        interaction.DoubleClicked(110, 110);
        Assert.Equal(inner.Id, state.EnteredGroupId);
        Assert.Equal(1, state.SelectedCount);
    }

    [Fact]
    public void DoubleClick_OnEmptySpaceInsideAnEnteredGroup_LeavesIt()
    {
        var scene = Create();
        scene.Interaction.DoubleClicked(110, 110);

        scene.Interaction.DoubleClicked(700, 600);

        Assert.Null(scene.State.EnteredGroupId);
        Assert.Equal([scene.Group.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Click_OnEmptySpaceBesideTheEnteredGroup_LeavesIt()
    {
        var scene = Create();
        scene.Interaction.DoubleClicked(110, 110);

        Click(scene.Interaction, 700, 600);

        Assert.Null(scene.State.EnteredGroupId);
        Assert.Equal([scene.Group.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Click_OnEmptySpaceInsideTheBoxOfTheEnteredGroup_OnlyClearsTheSelection()
    {
        var scene = Create();
        scene.Interaction.DoubleClicked(110, 110);
        Click(scene.Interaction, 110, 110);

        Click(scene.Interaction, 145, 115);

        Assert.Equal(scene.Group.Id, scene.State.EnteredGroupId);
        Assert.False(scene.State.HasSelection);
    }

    [Fact]
    public void DoubleClick_WithAnotherTool_DoesNothing()
    {
        var scene = Create();
        scene.State.ActiveTool = Tool.Rectangle;

        scene.Interaction.DoubleClicked(110, 110);

        Assert.Null(scene.State.EnteredGroupId);
    }

    [Fact]
    public void Move_Group_IsOneStepAndMovesAllLeaves()
    {
        var scene = Create();
        var steps = scene.State.History.Steps.Count;

        scene.Interaction.PointerPressed(110, 110);
        scene.Interaction.PointerMoved(120, 115);
        scene.Interaction.PointerMoved(150, 130);
        scene.Interaction.PointerReleased(150, 130);

        Assert.Equal(steps + 1, scene.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.MoveShape, scene.State.History.Steps[^1]);
        Assert.Equal(140, Find(scene.State, scene.A.Id).X);
        Assert.Equal(200, Find(scene.State, scene.B.Id).X);
        Assert.Equal(120, Find(scene.State, scene.A.Id).Y);
        Assert.Equal(new Bounds(140, 120, 90, 30), Find(scene.State, scene.Group.Id).Bounds);
        Assert.Equal(400, Find(scene.State, scene.Plain.Id).X);
    }

    [Fact]
    public void Move_Group_UndoRestoresTheDocument()
    {
        var scene = Create();
        var before = scene.State.Document;

        scene.Interaction.PointerPressed(110, 110);
        scene.Interaction.PointerMoved(150, 130);
        scene.Interaction.PointerReleased(150, 130);
        scene.State.Undo();

        Assert.Same(before, scene.State.Document);
    }

    [Fact]
    public void Resize_Group_IsOneStepAndKeepsTheLayout()
    {
        var scene = Create();
        Click(scene.Interaction, 110, 110);
        var frame = SelectionGeometry.FrameOf([Find(scene.State, scene.Group.Id)])!;
        var (hx, hy) = SelectionGeometry.HandlePosition(frame, HandleType.Right, scene.State.ZoomPercent);
        var steps = scene.State.History.Steps.Count;

        scene.Interaction.PointerPressed(hx, hy);
        scene.Interaction.PointerMoved(hx + 45, hy);
        scene.Interaction.PointerReleased(hx + 90, hy);

        Assert.Equal(steps + 1, scene.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.ResizeShapes, scene.State.History.Steps[^1]);
        Assert.Equal(100, Find(scene.State, scene.A.Id).X);
        Assert.Equal(60, Find(scene.State, scene.A.Id).Width);
        Assert.Equal(220, Find(scene.State, scene.B.Id).X);
        Assert.Equal(new Bounds(100, 100, 180, 30), Find(scene.State, scene.Group.Id).Bounds);
    }

    [Fact]
    public void Rotate_Group_IsOneStepAndTheGroupStaysUpright()
    {
        var scene = Create();
        Click(scene.Interaction, 110, 110);
        var frame = SelectionGeometry.FrameOf([Find(scene.State, scene.Group.Id)])!;
        var (hx, hy) = SelectionGeometry.HandlePosition(frame, HandleType.Rotate, scene.State.ZoomPercent);
        var steps = scene.State.History.Steps.Count;

        scene.Interaction.PointerPressed(hx, hy);
        scene.Interaction.PointerMoved(hx + 100, hy + 100);
        scene.Interaction.PointerReleased(hx + 200, hy + 150);

        Assert.Equal(steps + 1, scene.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.RotateShapes, scene.State.History.Steps[^1]);
        Assert.NotEqual(0, Find(scene.State, scene.A.Id).Rotation);
        Assert.Equal(Find(scene.State, scene.A.Id).Rotation, Find(scene.State, scene.B.Id).Rotation, 6);
        Assert.Equal(0, Find(scene.State, scene.Group.Id).Rotation);
    }

    [Fact]
    public void Rubberband_AtTopLevel_SelectsGroupsNotMembers()
    {
        var scene = Create();

        scene.Interaction.PointerPressed(90, 90);
        scene.Interaction.PointerMoved(300, 200);
        scene.Interaction.PointerReleased(300, 200);

        Assert.Equal([scene.Group.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Rubberband_InsideAnEnteredGroup_SelectsItsChildren()
    {
        var scene = Create();
        scene.Interaction.DoubleClicked(110, 110);

        scene.Interaction.PointerPressed(90, 90);
        scene.Interaction.PointerMoved(500, 200);
        scene.Interaction.PointerReleased(500, 200);

        Assert.Equal([scene.A.Id, scene.B.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Click_OnALockedOrHiddenShape_SelectsWhatIsBelow()
    {
        var below = Square(100);
        var locked = Square(100) with { IsLocked = true };
        var hidden = Square(100) with { IsVisible = false };
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [below, hidden, locked])) { ActiveTool = Tool.Select };
        var interaction = new CanvasInteraction(state);

        Click(interaction, 110, 110);

        Assert.Equal([below.Id], state.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Click_OnAMemberOfALockedGroup_SelectsNothing()
    {
        var scene = Create();
        scene.State.Edit(HistoryStepType.LockShapes, d => d.SetLocked(new System.Collections.Generic.HashSet<Guid> { scene.Group.Id }, true));

        Click(scene.Interaction, 110, 110);

        Assert.False(scene.State.HasSelection);
    }

    [Fact]
    public void DeleteSelection_Group_RemovesTheWholeGroupAsOneStep()
    {
        var scene = Create();
        Click(scene.Interaction, 110, 110);
        var steps = scene.State.History.Steps.Count;

        scene.Interaction.DeleteSelection();

        Assert.Equal(steps + 1, scene.State.History.Steps.Count);
        Assert.Equal([scene.Plain.Id], scene.State.Document.Layers[0].Shapes.Select(s => s.Id).ToArray());
    }

    [Fact]
    public void Escape_InAnEnteredGroup_LeavesIt()
    {
        var scene = Create();
        var canvas = new CanvasAreaViewModel(scene.State) { ViewportWidth = 1000, ViewportHeight = 800 };
        scene.Interaction.DoubleClicked(110, 110);

        var handled = canvas.KeyPressed(CanvasKey.Escape, shift: false);

        Assert.True(handled);
        Assert.Null(scene.State.EnteredGroupId);
        Assert.Equal([scene.Group.Id], scene.State.SelectedShapeIds.ToArray());
    }
}
