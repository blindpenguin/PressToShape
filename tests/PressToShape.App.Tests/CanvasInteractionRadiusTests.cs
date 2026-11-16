using System;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionRadiusTests
{
    // 200 x 100 box at (100, 50), zoom 100: the handles sit at (124, 74), (276, 74), (276, 126) and (124, 126) while the radii are 0.
    private static RectangleShape Rect(CornerRadii radii = default) =>
        new() { Id = Guid.NewGuid(), X = 100, Y = 50, Width = 200, Height = 100, Radii = radii };

    private static (EditorState State, CanvasInteraction Interaction, RectangleShape Rectangle) Create(RectangleShape rectangle, Tool tool = Tool.Select)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShape(document.Layers[0].Id, rectangle)) { ActiveTool = tool };
        state.Select(rectangle.Id);
        return (state, new CanvasInteraction(state), rectangle);
    }

    private static RectangleShape Current(EditorState state, Shape shape) => Assert.IsType<RectangleShape>(state.Document.FindShape(shape.Id)!.Shape);

    private static void Drag(
        CanvasInteraction interaction, double x1, double y1, double x2, double y2, bool shift = false, InputPointerType input = InputPointerType.Mouse)
    {
        interaction.PointerPressed(x1, y1, false, input);
        interaction.PointerMoved((x1 + x2) / 2, (y1 + y2) / 2, shift);
        interaction.PointerMoved(x2, y2, shift);
        interaction.PointerReleased(x2, y2, shift);
    }

    [Fact]
    public void DragARadiusHandle_WithoutShift_SetsAllFourRadii()
    {
        var (state, interaction, rectangle) = Create(Rect());

        // Inset 37 on both sides is a radius of 25.
        Drag(interaction, 124, 74, 137, 87);

        Assert.Equal(CornerRadii.Uniform(25), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_WithShift_ChangesOnlyThatCorner()
    {
        var (state, interaction, rectangle) = Create(Rect(CornerRadii.Zero.With(Corner.BottomRight, 10)));

        Drag(interaction, 124, 74, 137, 87, shift: true);

        Assert.Equal(new CornerRadii(25, 0, 10, 0), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_WithAPen_BehavesLikeAMouse()
    {
        var (state, interaction, rectangle) = Create(Rect());

        Drag(interaction, 276, 126, 263, 113, input: InputPointerType.Pen);

        Assert.Equal(CornerRadii.Uniform(25), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_ByMouseWithTheToggleOff_StillChangesAllWithoutShift()
    {
        var (state, interaction, rectangle) = Create(Rect());
        state.LinkCornerRadii = false;

        Drag(interaction, 124, 74, 137, 87);

        Assert.Equal(CornerRadii.Uniform(25), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_ByMouseWithTheToggleOn_ChangesOneCornerWithShift()
    {
        var (state, interaction, rectangle) = Create(Rect());
        state.LinkCornerRadii = true;

        Drag(interaction, 124, 74, 137, 87, shift: true);

        Assert.Equal(new CornerRadii(25, 0, 0, 0), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_ByTouchWithTheToggleOn_ChangesAllFour()
    {
        var (state, interaction, rectangle) = Create(Rect());
        state.LinkCornerRadii = true;

        Drag(interaction, 124, 74, 137, 87, input: InputPointerType.Touch);

        Assert.Equal(CornerRadii.Uniform(25), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_ByTouchWithTheToggleOff_ChangesOnlyThatCorner()
    {
        var (state, interaction, rectangle) = Create(Rect());
        state.LinkCornerRadii = false;

        Drag(interaction, 124, 74, 137, 87, input: InputPointerType.Touch);

        Assert.Equal(new CornerRadii(25, 0, 0, 0), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_IsOneStepAndUndoPutsTheRadiiBack()
    {
        var (state, interaction, rectangle) = Create(Rect(new CornerRadii(5, 6, 7, 8)));
        var steps = state.History.Steps.Count;

        var (hx, hy) = RadiusHandles.Position(rectangle, Corner.TopLeft, 100);

        Drag(interaction, hx, hy, 150, 100);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.ChangeCornerRadius, state.History.Steps[^1]);
        state.Undo();
        Assert.Equal(new CornerRadii(5, 6, 7, 8), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_BackToTheStart_RecordsNoStep()
    {
        var (state, interaction, _) = Create(Rect());
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(124, 74);
        interaction.PointerMoved(150, 100);
        interaction.PointerMoved(124, 74);
        interaction.PointerReleased(124, 74);

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void DragARadiusHandle_JustPressedOffTheCenter_DoesNotChangeTheRadius()
    {
        var (state, interaction, rectangle) = Create(Rect(CornerRadii.Uniform(10)));
        var (hx, hy) = RadiusHandles.Position(rectangle, Corner.TopLeft, 100);

        interaction.PointerPressed(hx + 3, hy - 2);
        interaction.PointerMoved(hx + 3, hy - 2);
        interaction.PointerReleased(hx + 3, hy - 2);

        Assert.Equal(CornerRadii.Uniform(10), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_PastTheLimit_StopsAtHalfOfTheShorterSide()
    {
        var (state, interaction, rectangle) = Create(Rect());

        Drag(interaction, 124, 74, 250, 130);

        Assert.Equal(CornerRadii.Uniform(50), Current(state, rectangle).Radii);
    }

    [Fact]
    public void DragARadiusHandle_TowardTheCorner_StopsAtZero()
    {
        var (state, interaction, rectangle) = Create(Rect(CornerRadii.Uniform(30)));
        var (hx, hy) = RadiusHandles.Position(rectangle, Corner.TopLeft, 100);

        Drag(interaction, hx, hy, 90, 40);

        Assert.True(Current(state, rectangle).Radii.IsZero);
    }

    [Fact]
    public void DragARadiusHandle_DuringTheDrag_ShowsThePreviewAndMarksTheDrag()
    {
        var (state, interaction, rectangle) = Create(Rect());

        interaction.PointerPressed(124, 74);
        interaction.PointerMoved(137, 87);

        Assert.True(interaction.IsDragging);
        Assert.True(interaction.IsRadiusDragging);
        Assert.True(state.IsDragInProgress);
        Assert.Equal(CornerRadii.Uniform(25), Current(state, rectangle).Radii);
        interaction.PointerReleased(137, 87);
        Assert.False(interaction.IsRadiusDragging);
    }

    [Fact]
    public void Abort_DuringTheDrag_PutsTheRadiiBackWithoutAStep()
    {
        var (state, interaction, rectangle) = Create(Rect(CornerRadii.Uniform(5)));
        var steps = state.History.Steps.Count;
        var (hx, hy) = RadiusHandles.Position(rectangle, Corner.TopLeft, 100);

        interaction.PointerPressed(hx, hy);
        interaction.PointerMoved(150, 100);
        interaction.Abort();

        Assert.Equal(CornerRadii.Uniform(5), Current(state, rectangle).Radii);
        Assert.Equal(steps, state.History.Steps.Count);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void DragARadiusHandle_LeavesTheSelectionPositionAndSizeAlone()
    {
        var (state, interaction, rectangle) = Create(Rect());

        Drag(interaction, 124, 74, 150, 100);

        var current = Current(state, rectangle);
        Assert.Equal(rectangle.Bounds, current.Bounds);
        Assert.Equal(rectangle.Rotation, current.Rotation);
        Assert.Equal(rectangle.Id, Assert.Single(state.SelectedShapeIds));
    }

    [Fact]
    public void Press_ByTouchBetweenTheCornerAndTheRadiusHandle_GrabsTheRadiusHandleNotTheResizeHandle()
    {
        var (state, interaction, rectangle) = Create(Rect());

        // 14 px from both the corner resize handle at (100, 50) and the radius handle at (124, 74): both grab areas contain the point.
        interaction.PointerPressed(112, 62, false, InputPointerType.Touch);

        Assert.True(interaction.IsRadiusDragging);
        interaction.PointerReleased(112, 62);
        Assert.Equal(rectangle.Bounds, Current(state, rectangle).Bounds);
    }

    [Fact]
    public void Press_OnTheCornerResizeHandle_StillResizes()
    {
        var (state, interaction, rectangle) = Create(Rect());

        Drag(interaction, 100, 50, 80, 30);

        var current = Current(state, rectangle);
        Assert.Equal(80, current.X, 6);
        Assert.Equal(220, current.Width, 6);
    }

    [Fact]
    public void Press_InTheMiddleOfTheRectangle_StillMoves()
    {
        var (state, interaction, rectangle) = Create(Rect());

        Drag(interaction, 200, 100, 230, 110);

        var current = Current(state, rectangle);
        Assert.Equal(130, current.X, 6);
        Assert.True(current.Radii.IsZero);
    }

    [Fact]
    public void Press_OnTheHandleOfALockedRectangle_StartsNoRadiusDrag()
    {
        var (state, interaction, rectangle) = Create(Rect() with { IsLocked = true });

        interaction.PointerPressed(124, 74);

        Assert.False(interaction.IsRadiusDragging);
        interaction.PointerMoved(150, 100);
        interaction.PointerReleased(150, 100);
        Assert.Equal(CornerRadii.Zero, Current(state, rectangle).Radii);
    }

    [Fact]
    public void Press_OnASmallRectangle_StartsNoRadiusDrag()
    {
        var (state, interaction, rectangle) = Create(Rect() with { Height = 40 });

        interaction.PointerPressed(124, 74);

        Assert.False(interaction.IsRadiusDragging);
        interaction.PointerReleased(124, 74);
        Assert.Equal(CornerRadii.Zero, Current(state, rectangle).Radii);
    }

    [Fact]
    public void Press_WithTwoShapesSelected_StartsNoRadiusDrag()
    {
        var first = Rect();
        var second = Rect() with { X = 400 };
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [first, second])) { ActiveTool = Tool.Select };
        state.SetSelection([first.Id, second.Id]);
        var interaction = new CanvasInteraction(state);

        interaction.PointerPressed(124, 74);

        Assert.False(interaction.IsRadiusDragging);
        interaction.PointerReleased(124, 74);
    }

    [Fact]
    public void DragARadiusHandle_OnARotatedRectangle_UsesTheRotatedFrame()
    {
        var (state, interaction, rectangle) = Create(Rect() with { Rotation = 90 });
        var (hx, hy) = RadiusHandles.Position(rectangle, Corner.TopLeft, 100);
        var (tx, ty) = RadiusHandles.Position(rectangle with { Radii = CornerRadii.Uniform(30) }, Corner.TopLeft, 100);

        Drag(interaction, hx, hy, tx, ty);

        Assert.Equal(30, Current(state, rectangle).Radii.TopLeft, 6);
        Assert.Equal(90, Current(state, rectangle).Rotation);
    }

    [Fact]
    public void DragARadiusHandle_ZoomedIn_FollowsTheScreenInset()
    {
        var (state, interaction, rectangle) = Create(Rect());
        state.SetZoom(200);
        var (hx, hy) = RadiusHandles.Position(rectangle, Corner.TopLeft, 200);
        var (tx, ty) = RadiusHandles.Position(rectangle with { Radii = CornerRadii.Uniform(20) }, Corner.TopLeft, 200);

        Drag(interaction, hx, hy, tx, ty);

        Assert.Equal(CornerRadii.Uniform(20), Current(state, rectangle).Radii);
    }
}
