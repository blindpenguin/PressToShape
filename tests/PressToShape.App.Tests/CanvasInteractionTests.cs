using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionTests
{
    private static readonly Guid Bottom = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Top = new("aaaaaaaa-0000-0000-0000-000000000002");

    private static (EditorState State, CanvasInteraction Interaction) Create(Tool tool = Tool.Rectangle, Document? document = null)
    {
        var doc = document ?? Document.Create("ignored") with { Layers = [new Layer { Id = Bottom, Name = "Bottom" }, new Layer { Id = Top, Name = "Top" }] };
        var state = new EditorState(doc) { ActiveTool = tool };
        return (state, new CanvasInteraction(state));
    }

    private static void Drag(CanvasInteraction interaction, double x1, double y1, double x2, double y2)
    {
        interaction.PointerPressed(x1, y1);
        interaction.PointerMoved((x1 + x2) / 2, (y1 + y2) / 2);
        interaction.PointerReleased(x2, y2);
    }

    private static Shape OnlyShape(EditorState state) => Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));

    // ---- drawing ----

    [Fact]
    public void Draw_Rectangle_AddsARectangleOnTheActiveLayer()
    {
        var (state, interaction) = Create(Tool.Rectangle);

        Drag(interaction, 10, 20, 110, 70);

        var shape = Assert.IsType<RectangleShape>(OnlyShape(state));
        Assert.Equal((10, 20, 100, 50), (shape.X, shape.Y, shape.Width, shape.Height));
        Assert.Single(state.Document.Layers[1].Shapes);
        Assert.Empty(state.Document.Layers[0].Shapes);
    }

    [Fact]
    public void Draw_Ellipse_AddsAnEllipse()
    {
        var (state, interaction) = Create(Tool.Ellipse);

        Drag(interaction, 0, 0, 40, 30);

        var shape = Assert.IsType<EllipseShape>(OnlyShape(state));
        Assert.Equal((0, 0, 40, 30), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Theory]
    [InlineData(100, 100, 40, 60)]
    [InlineData(100, 100, 160, 40)]
    [InlineData(100, 100, 40, 160)]
    [InlineData(100, 100, 160, 160)]
    public void Draw_AnyDragDirection_GivesTheNormalizedRectangle(double x1, double y1, double x2, double y2)
    {
        var (state, interaction) = Create();

        Drag(interaction, x1, y1, x2, y2);

        var shape = OnlyShape(state);
        Assert.Equal(Math.Min(x1, x2), shape.X);
        Assert.Equal(Math.Min(y1, y2), shape.Y);
        Assert.Equal(Math.Abs(x2 - x1), shape.Width);
        Assert.Equal(Math.Abs(y2 - y1), shape.Height);
    }

    [Fact]
    public void Draw_UsesTheCoreDefaultAppearanceAndANewId()
    {
        var (state, interaction) = Create();

        Drag(interaction, 0, 0, 50, 50);
        Drag(interaction, 0, 0, 60, 60);

        var shapes = state.Document.Layers[1].Shapes;
        Assert.All(shapes, s => Assert.Equal(Appearance.Default, s.Appearance));
        Assert.NotEqual(shapes[0].Id, shapes[1].Id);
    }

    [Fact]
    public void Draw_Twice_PutsTheNewShapeOnTop()
    {
        var (state, interaction) = Create();

        Drag(interaction, 0, 0, 50, 50);
        Drag(interaction, 10, 10, 90, 90);

        Assert.Equal(90 - 10, state.Document.Layers[1].Shapes[1].Width);
    }

    [Fact]
    public void Draw_Finished_SelectsTheNewShapeAndKeepsTheTool()
    {
        var (state, interaction) = Create(Tool.Ellipse);

        Drag(interaction, 0, 0, 50, 50);

        Assert.Equal(OnlyShape(state).Id, state.SelectedShapeId);
        Assert.True(state.HasSelection);
        Assert.Equal(Tool.Ellipse, state.ActiveTool);
    }

    [Fact]
    public void Draw_InTheMiddleOfTheDrag_ShowsTheDraftOutsideTheDocument()
    {
        var (state, interaction) = Create();

        interaction.PointerPressed(10, 10);
        interaction.PointerMoved(60, 40);

        Assert.True(interaction.IsDragging);
        Assert.Equal((10, 10, 50, 30), (state.Draft!.X, state.Draft.Y, state.Draft.Width, state.Draft.Height));
        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));

        interaction.PointerReleased(60, 40);

        Assert.Null(state.Draft);
        Assert.False(interaction.IsDragging);
    }

    [Theory]
    [InlineData(100, 1.9, 50)]
    [InlineData(100, 50, 1.9)]
    [InlineData(100, 0, 0)]
    [InlineData(50, 3.9, 50)]
    [InlineData(200, 0.9, 50)]
    public void Draw_SmallerThanTwoScreenPixels_AddsNothing(double zoom, double width, double height)
    {
        var (state, interaction) = Create();
        state.SetZoom(zoom);

        Drag(interaction, 10, 10, 10 + width, 10 + height);

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.False(state.HasSelection);
        Assert.Null(state.Draft);
    }

    [Theory]
    [InlineData(100, 2, 2)]
    [InlineData(50, 4, 4)]
    [InlineData(200, 1, 1)]
    public void Draw_ExactlyTwoScreenPixels_AddsTheShape(double zoom, double width, double height)
    {
        var (state, interaction) = Create();
        state.SetZoom(zoom);

        Drag(interaction, 10, 10, 10 + width, 10 + height);

        Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void Draw_HiddenActiveLayer_CreatesNothing()
    {
        var (state, interaction) = Create();
        state.Document = state.Document.UpdateLayer(Top, l => l with { IsVisible = false });

        Drag(interaction, 0, 0, 50, 50);

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Null(state.Draft);
    }

    [Fact]
    public void Draw_LockedActiveLayer_CreatesNothing()
    {
        var (state, interaction) = Create();
        state.Document = state.Document.UpdateLayer(Top, l => l with { IsLocked = true });

        interaction.PointerPressed(0, 0);
        interaction.PointerMoved(30, 30);

        Assert.Null(state.Draft);
        Assert.False(interaction.IsDragging);

        interaction.PointerReleased(50, 50);

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void Draw_LayerLockedDuringTheDrag_CreatesNothing()
    {
        var (state, interaction) = Create();

        interaction.PointerPressed(0, 0);
        state.Document = state.Document.UpdateLayer(Top, l => l with { IsLocked = true });
        interaction.PointerReleased(50, 50);

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void Draw_OnTheOtherLayerWhenItIsActive_UsesThatLayer()
    {
        var (state, interaction) = Create();
        state.ActiveLayerId = Bottom;

        Drag(interaction, 0, 0, 50, 50);

        Assert.Single(state.Document.Layers[0].Shapes);
        Assert.Empty(state.Document.Layers[1].Shapes);
    }

    [Fact]
    public void Draw_OutsideTheArtboard_IsCreatedAtTheGivenPosition()
    {
        var (state, interaction) = Create();

        Drag(interaction, -100, 900, -20, 950);

        var shape = OnlyShape(state);
        Assert.Equal((-100, 900, 80, 50), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Draw_ToolChangedDuringTheDrag_DropsTheDraftAndAddsNothing()
    {
        var (state, interaction) = Create();
        interaction.PointerPressed(0, 0);
        interaction.PointerMoved(30, 30);

        state.ActiveTool = Tool.Select;
        interaction.PointerMoved(40, 40);
        interaction.PointerReleased(50, 50);

        Assert.Null(state.Draft);
        Assert.False(interaction.IsDragging);
        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void Cancel_DuringDrawing_DropsTheDraft()
    {
        var (state, interaction) = Create();
        interaction.PointerPressed(0, 0);
        interaction.PointerMoved(30, 30);

        interaction.Cancel();
        interaction.PointerReleased(50, 50);

        Assert.Null(state.Draft);
        Assert.False(interaction.IsDragging);
        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void OtherTools_Drag_DoNothing()
    {
        foreach (var tool in new[] { Tool.Pen, Tool.Hand, Tool.Zoom, Tool.Text })
        {
            var (state, interaction) = Create(tool);

            Drag(interaction, 0, 0, 50, 50);

            Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
            Assert.False(interaction.IsDragging);
            Assert.Null(state.Draft);
        }
    }

    [Fact]
    public void MoveAndRelease_WithoutPress_DoNothing()
    {
        var (state, interaction) = Create();

        interaction.PointerMoved(10, 10);
        interaction.PointerReleased(10, 10);

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Null(state.Draft);
    }

    [Fact]
    public void Constructor_NullState_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CanvasInteraction(null!));
    }

    // ---- selecting ----

    private static Document WithShapes(out Guid back, out Guid front, out Guid ellipse)
    {
        var backShape = new RectangleShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var frontShape = new RectangleShape { X = 50, Y = 50, Width = 100, Height = 100 };
        var round = new EllipseShape { X = 300, Y = 0, Width = 100, Height = 100 };
        back = backShape.Id;
        front = frontShape.Id;
        ellipse = round.Id;
        return (Document.Create("ignored") with { Layers = [new Layer { Id = Bottom, Name = "Bottom" }, new Layer { Id = Top, Name = "Top" }] })
            .AddShape(Bottom, backShape)
            .AddShape(Top, frontShape)
            .AddShape(Top, round);
    }

    [Fact]
    public void Select_ClickOnOverlap_SelectsTheTopmostShape()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));

        interaction.PointerPressed(75, 75);
        interaction.PointerReleased(75, 75);

        Assert.Equal(front, state.SelectedShapeId);
        Assert.True(state.HasSelection);
    }

    [Fact]
    public void Select_ClickOnTheBackShapeOnly_SelectsIt()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out var back, out _, out _));

        interaction.PointerPressed(10, 10);
        interaction.PointerReleased(10, 10);

        Assert.Equal(back, state.SelectedShapeId);
    }

    [Fact]
    public void Select_ClickInTheCornerOfAnEllipse_Misses()
    {
        // Another shape is selected: the ellipse's own corner handle would otherwise be under the pointer.
        var (state, interaction) = Create(Tool.Select, WithShapes(out var back, out _, out _));
        state.Select(back);

        interaction.PointerPressed(301, 1);
        interaction.PointerReleased(301, 1);

        Assert.Null(state.SelectedShapeId);
    }

    [Fact]
    public void Select_ClickOnEmptySpace_ClearsTheSelection()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out var back, out _, out _));
        state.Select(back);

        interaction.PointerPressed(700, 700);
        interaction.PointerReleased(700, 700);

        Assert.Null(state.SelectedShapeId);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Select_TopShapeInLockedLayer_SelectsTheUnlockedShapeBelow()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out var back, out _, out _));
        state.Document = state.Document.UpdateLayer(Top, l => l with { IsLocked = true });

        interaction.PointerPressed(75, 75);
        interaction.PointerReleased(75, 75);

        Assert.Equal(back, state.SelectedShapeId);
    }

    [Fact]
    public void Select_OnlyLockedShapeUnderThePointer_ClearsTheSelection()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out var back, out _, out _));
        state.Select(back);
        state.Document = state.Document.UpdateLayer(Top, l => l with { IsLocked = true });

        interaction.PointerPressed(120, 120);
        interaction.PointerReleased(120, 120);

        Assert.Null(state.SelectedShapeId);
    }

    [Fact]
    public void Select_ShapeInHiddenLayer_IsSkipped()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out var back, out _, out _));
        state.Document = state.Document.UpdateLayer(Top, l => l with { IsVisible = false });

        interaction.PointerPressed(75, 75);
        interaction.PointerReleased(75, 75);

        Assert.Equal(back, state.SelectedShapeId);
    }

    // ---- moving ----

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(50)]
    public void Move_DragSelectedShape_FollowsThePointerByTheSameDocumentDistance(double zoom)
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));
        state.SetZoom(zoom);

        interaction.PointerPressed(75, 75);
        interaction.PointerMoved(100, 90);
        var during = state.Document.FindShape(front)!.Shape;
        interaction.PointerReleased(125, 105);

        Assert.Equal((75, 65), (during.X, during.Y));
        var moved = state.Document.FindShape(front)!.Shape;
        Assert.Equal((100, 80), (moved.X, moved.Y));
        Assert.Equal(front, state.SelectedShapeId);
    }

    [Fact]
    public void Move_ManySmallMoves_DoNotDrift()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));

        interaction.PointerPressed(75, 75);
        for (var i = 1; i <= 1000; i++)
        {
            interaction.PointerMoved(75 + i * 0.1, 75);
        }

        interaction.PointerReleased(175, 75);

        Assert.Equal(150, state.Document.FindShape(front)!.Shape.X, precision: 9);
    }

    [Fact]
    public void Move_OnlyThePositionChanges()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));
        var before = state.Document.FindShape(front)!;

        Drag(interaction, 75, 75, 175, 275);

        var after = state.Document.FindShape(front)!;
        Assert.Equal((before.Shape.Width, before.Shape.Height), (after.Shape.Width, after.Shape.Height));
        Assert.Equal(before.Shape.Appearance, after.Shape.Appearance);
        Assert.Equal(before.Shape.Id, after.Shape.Id);
        Assert.Equal(before.Layer.Id, after.Layer.Id);
        Assert.Equal(0, state.Document.Layers[1].Shapes.IndexOf(after.Shape));
    }

    [Fact]
    public void Move_EllipseKeepsItsType()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out _, out var ellipse));

        Drag(interaction, 350, 50, 360, 70);

        Assert.IsType<EllipseShape>(state.Document.FindShape(ellipse)!.Shape);
        Assert.Equal(310, state.Document.FindShape(ellipse)!.Shape.X);
    }

    [Fact]
    public void Move_PointerFarOutsideTheCanvas_StillFollows()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));

        interaction.PointerPressed(75, 75);
        interaction.PointerMoved(-5000, 9000);
        interaction.PointerReleased(-5000, 9000);

        Assert.Equal(50 - 5075, state.Document.FindShape(front)!.Shape.X);
        Assert.Equal(50 + 8925, state.Document.FindShape(front)!.Shape.Y);
    }

    [Fact]
    public void Move_PressOnEmptySpaceAndDrag_MovesNothing()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out var back, out _, out _));
        var before = state.Document;
        state.Select(back);

        Drag(interaction, 700, 700, 800, 800);

        Assert.Same(before, state.Document);
        Assert.Null(state.SelectedShapeId);
        Assert.False(interaction.IsDragging);
    }

    [Fact]
    public void Move_ShapeInLockedLayer_IsNotMoved()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));
        state.Document = state.Document.UpdateLayer(Top, l => l with { IsLocked = true });

        Drag(interaction, 140, 140, 240, 240);

        Assert.Equal(50, state.Document.FindShape(front)!.Shape.X);
    }

    [Fact]
    public void Move_Cancel_KeepsTheReachedPositionAndEndsTheDrag()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));
        interaction.PointerPressed(75, 75);
        interaction.PointerMoved(95, 75);

        interaction.Cancel();
        interaction.PointerMoved(300, 75);
        interaction.PointerReleased(300, 75);

        Assert.Equal(70, state.Document.FindShape(front)!.Shape.X);
        Assert.False(interaction.IsDragging);
    }

    // ---- deleting ----

    [Fact]
    public void DeleteSelection_SelectedShape_RemovesItAndClearsTheSelection()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));
        state.Select(front);

        interaction.DeleteSelection();

        Assert.Null(state.Document.FindShape(front));
        Assert.Null(state.SelectedShapeId);
        Assert.False(state.HasSelection);
        Assert.Equal(2, state.Document.Layers.SelectMany(l => l.Shapes).Count());
    }

    [Fact]
    public void DeleteSelection_NothingSelected_DoesNothing()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out _, out _));
        var before = state.Document;

        interaction.DeleteSelection();

        Assert.Same(before, state.Document);
    }

    // ---- history steps ----

    [Fact]
    public void Draw_Rectangle_IsOneStepOfTheRightType()
    {
        var (state, interaction) = Create(Tool.Rectangle);
        var before = state.Document;

        Drag(interaction, 0, 0, 50, 50);

        Assert.Equal(new[] { HistoryStepType.DrawRectangle }, state.History.Steps);
        Assert.Same(state.Document, state.History.Current);
        state.Undo();
        Assert.Same(before, state.Document);
    }

    [Fact]
    public void Draw_Ellipse_IsOneStepOfTheRightType()
    {
        var (state, interaction) = Create(Tool.Ellipse);

        Drag(interaction, 0, 0, 50, 50);

        Assert.Equal(new[] { HistoryStepType.DrawEllipse }, state.History.Steps);
    }

    [Fact]
    public void Draw_TooSmallOrBlockedOrCancelled_IsNoStep()
    {
        var (state, interaction) = Create(Tool.Rectangle);
        Drag(interaction, 0, 0, 1, 1);

        state.Document = state.Document.UpdateLayer(Top, l => l with { IsLocked = true });
        Drag(interaction, 0, 0, 50, 50);

        state.Document = state.Document.UpdateLayer(Top, l => l with { IsLocked = false });
        interaction.PointerPressed(0, 0);
        interaction.PointerMoved(30, 30);
        interaction.Cancel();
        interaction.PointerReleased(50, 50);

        Assert.Empty(state.History.Steps);
    }

    [Fact]
    public void Move_ManyPointerMoves_AreOneMoveStep_AndUndoGoesBackToTheStart()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));
        state.ReplaceDocument(state.Document);
        var before = state.Document;

        interaction.PointerPressed(75, 75);
        for (var i = 1; i <= 50; i++)
        {
            interaction.PointerMoved(75 + i, 75 + i);
            Assert.Empty(state.History.Steps);
        }

        interaction.PointerReleased(150, 150);

        Assert.Equal(new[] { HistoryStepType.MoveShape }, state.History.Steps);
        Assert.Equal(125, state.Document.FindShape(front)!.Shape.X);
        state.Undo();
        Assert.Same(before, state.Document);
        Assert.Equal(50, state.Document.FindShape(front)!.Shape.X);
        state.Redo();
        Assert.Equal(125, state.Document.FindShape(front)!.Shape.X);
    }

    [Fact]
    public void Move_ClickWithoutMovement_IsNoStep_AndKeepsTheDocumentInstance()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out _, out _));
        var before = state.Document;

        interaction.PointerPressed(75, 75);
        interaction.PointerReleased(75, 75);

        Assert.Empty(state.History.Steps);
        Assert.Same(before, state.Document);
    }

    [Fact]
    public void Move_PressOnEmptySpaceAndDrag_IsNoStep()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out _, out _));

        Drag(interaction, 700, 700, 800, 800);

        Assert.Empty(state.History.Steps);
    }

    [Fact]
    public void Move_CancelAfterMovement_IsOneStepWithTheReachedPosition()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));
        interaction.PointerPressed(75, 75);
        interaction.PointerMoved(95, 75);

        interaction.Cancel();

        Assert.Equal(new[] { HistoryStepType.MoveShape }, state.History.Steps);
        Assert.Equal(70, state.Document.FindShape(front)!.Shape.X);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void Move_ToolChangeAfterMovement_IsOneStep()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out _, out _));
        interaction.PointerPressed(75, 75);
        interaction.PointerMoved(95, 75);

        state.ActiveTool = Tool.Rectangle;
        interaction.PointerMoved(120, 75);
        interaction.PointerReleased(120, 75);

        Assert.Equal(new[] { HistoryStepType.MoveShape }, state.History.Steps);
    }

    [Fact]
    public void Move_CancelWithoutMovement_IsNoStep()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out _, out _));
        interaction.PointerPressed(75, 75);

        interaction.Cancel();

        Assert.Empty(state.History.Steps);
    }

    [Fact]
    public void Delete_IsOneStep_AndUndoBringsTheShapeBack()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out var front, out _));
        state.Select(front);
        var before = state.Document;

        interaction.DeleteSelection();

        Assert.Equal(new[] { HistoryStepType.DeleteShape }, state.History.Steps);
        state.Undo();
        Assert.Same(before, state.Document);
        Assert.NotNull(state.Document.FindShape(front));
    }

    [Fact]
    public void Delete_WithoutSelection_IsNoStep()
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out _, out _));

        interaction.DeleteSelection();

        Assert.Empty(state.History.Steps);
    }

    [Fact]
    public void DragFlag_IsSetDuringEveryDragAndClearedAtTheEnd()
    {
        var (state, interaction) = Create(Tool.Rectangle, WithShapes(out _, out _, out _));

        interaction.PointerPressed(400, 400);
        Assert.True(state.IsDragInProgress);
        interaction.PointerReleased(450, 450);
        Assert.False(state.IsDragInProgress);

        state.ActiveTool = Tool.Select;
        interaction.PointerPressed(75, 75);
        Assert.True(state.IsDragInProgress);
        interaction.PointerReleased(80, 80);
        Assert.False(state.IsDragInProgress);

        // Empty space starts a selection box, which is a drag as well.
        interaction.PointerPressed(700, 700);
        Assert.True(state.IsDragInProgress);
        interaction.PointerReleased(700, 700);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void Undo_DuringADrag_IsRefused()
    {
        var (state, interaction) = Create(Tool.Rectangle);
        Drag(interaction, 0, 0, 50, 50);
        interaction.PointerPressed(100, 100);
        var document = state.Document;

        state.Undo();

        Assert.Same(document, state.Document);
        Assert.False(state.CanUndo);
        interaction.PointerReleased(100, 100);
        Assert.True(state.CanUndo);
    }

    // ---- multi-selection, selection box, moving several shapes ----

    private static void Click(CanvasInteraction interaction, double x, double y, bool shift = false)
    {
        interaction.PointerPressed(x, y, shift);
        interaction.PointerReleased(x, y, shift);
    }

    private static void Box(CanvasInteraction interaction, double x1, double y1, double x2, double y2, bool shift = false)
    {
        interaction.PointerPressed(x1, y1, shift);
        interaction.PointerMoved((x1 + x2) / 2, (y1 + y2) / 2, shift);
        interaction.PointerReleased(x2, y2, shift);
    }

    private static (EditorState State, CanvasInteraction Interaction, Guid Back, Guid Front, Guid Ellipse) SelectFixture(double zoom = 100)
    {
        var (state, interaction) = Create(Tool.Select, WithShapes(out var back, out var front, out var ellipse));
        state.SetZoom(zoom);
        return (state, interaction, back, front, ellipse);
    }

    [Fact]
    public void ShiftClick_AddsAndRemovesShapes_AndMovesNothing()
    {
        var (state, interaction, back, _, ellipse) = SelectFixture();
        var steps = state.History.Steps.Count;

        Click(interaction, 10, 10);
        Click(interaction, 350, 50, shift: true);
        Assert.Equal(new[] { back, ellipse }, state.SelectedShapeIds);

        Click(interaction, 10, 10, shift: true);
        Assert.Equal(new[] { ellipse }, state.SelectedShapeIds);
        Assert.Equal(steps, state.History.Steps.Count);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void ShiftPress_OnASelectedShape_RemovesItAndADragAfterwardsMovesNothing()
    {
        var (state, interaction, back, front, _) = SelectFixture();
        state.SetSelection([back, front]);
        var document = state.Document;

        interaction.PointerPressed(10, 10, shift: true);
        interaction.PointerMoved(60, 60, shift: true);
        interaction.PointerReleased(60, 60, shift: true);

        Assert.Equal(new[] { front }, state.SelectedShapeIds);
        Assert.Same(document, state.Document);
    }

    [Fact]
    public void ClickOnEmptySpace_ClearsTheSelection_ButShiftClickKeepsIt()
    {
        var (state, interaction, back, _, _) = SelectFixture();
        Click(interaction, 10, 10);

        Click(interaction, 700, 700, shift: true);
        Assert.Equal(new[] { back }, state.SelectedShapeIds);

        Click(interaction, 700, 700);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void ClickOnAnUnselectedShape_SelectsOnlyIt()
    {
        var (state, interaction, back, _, ellipse) = SelectFixture();
        state.SetSelection([back, ellipse]);

        Click(interaction, 140, 140); // only the front shape is there

        Assert.Single(state.SelectedShapeIds);
        Assert.NotEqual(back, state.SelectedShapeId);
    }

    [Fact]
    public void PressOnASelectedShape_KeepsTheSelection_AndMovesAllOfThemAsOneStep()
    {
        var (state, interaction, back, front, ellipse) = SelectFixture();
        state.SetSelection([back, front, ellipse]);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(10, 10);
        Assert.Equal(3, state.SelectedCount);
        interaction.PointerMoved(20, 15);
        interaction.PointerMoved(40, 30);
        Assert.True(state.IsDragInProgress);
        Assert.Equal(steps, state.History.Steps.Count);
        interaction.PointerReleased(40, 30);

        var shapes = state.Document.Layers.SelectMany(l => l.Shapes).ToList();
        Assert.Equal(new[] { 30.0, 80, 330 }, shapes.Select(sh => sh.X));
        Assert.Equal(new[] { 20.0, 70, 20 }, shapes.Select(sh => sh.Y));
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.MoveShape, state.History.Steps[^1]);
        Assert.Equal(3, state.SelectedCount);

        state.Undo();
        Assert.Equal(new[] { 0.0, 50, 300 }, state.Document.Layers.SelectMany(l => l.Shapes).Select(sh => sh.X));
    }

    [Fact]
    public void ClickOnASelectedShape_WithoutMovement_MakesNoStep_AndKeepsTheOthers()
    {
        var (state, interaction, back, front, _) = SelectFixture();
        state.SetSelection([back, front]);
        var steps = state.History.Steps.Count;
        var document = state.Document;

        Click(interaction, 10, 10);

        Assert.Equal(new[] { back, front }, state.SelectedShapeIds);
        Assert.Same(document, state.Document);
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void MovingBackToTheStart_RestoresThePositionsWithoutAStep()
    {
        var (state, interaction, back, front, _) = SelectFixture();
        state.SetSelection([back, front]);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(10, 10);
        interaction.PointerMoved(60, 60);
        interaction.PointerMoved(10, 10);
        interaction.PointerReleased(10, 10);

        Assert.Equal(new[] { 0.0, 50 }, state.Document.Layers.SelectMany(l => l.Shapes).Take(2).Select(sh => sh.X));
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(200)]
    public void MovingSeveralShapes_UsesDocumentUnits_AtAnyZoom(double zoom)
    {
        var (state, interaction, back, front, _) = SelectFixture(zoom);
        state.SetSelection([back, front]);

        // Away from the handles of the selection.
        interaction.PointerPressed(40, 40);
        interaction.PointerReleased(60, 40);

        Assert.Equal(new[] { 20.0, 70 }, state.Document.Layers.SelectMany(l => l.Shapes).Take(2).Select(sh => sh.X));
    }

    [Fact]
    public void SelectionBox_SelectsTouchedShapes_AndShowsTheBoxWhileDragging()
    {
        var (state, interaction, back, front, ellipse) = SelectFixture();

        interaction.PointerPressed(-10, -10);
        interaction.PointerMoved(60, 60);
        Assert.Equal(new SelectionBox(-10, -10, 70, 70), state.SelectionBox);
        Assert.True(state.IsDragInProgress);
        interaction.PointerReleased(60, 60);

        Assert.Equal(new[] { back, front }, state.SelectedShapeIds);
        Assert.Null(state.SelectionBox);
        Assert.False(state.IsDragInProgress);
        Assert.DoesNotContain(ellipse, state.SelectedShapeIds);
    }

    [Fact]
    public void SelectionBox_DraggedBackwards_Works()
    {
        var (state, interaction, _, _, ellipse) = SelectFixture();

        Box(interaction, 450, 150, 350, 50);

        Assert.Equal(new[] { ellipse }, state.SelectedShapeIds);
    }

    [Fact]
    public void SelectionBox_WithShift_AddsToTheSelection()
    {
        var (state, interaction, back, front, ellipse) = SelectFixture();
        Click(interaction, 10, 10);

        Box(interaction, 340, -10, 450, 150, shift: true);

        Assert.Equal(new[] { back, ellipse }, state.SelectedShapeIds);
        Assert.DoesNotContain(front, state.SelectedShapeIds);
    }

    [Fact]
    public void SelectionBox_WithoutShift_ReplacesTheSelection()
    {
        var (state, interaction, back, _, ellipse) = SelectFixture();
        Click(interaction, 10, 10);

        Box(interaction, 340, 200, 450, 300);
        Assert.Empty(state.SelectedShapeIds);

        Click(interaction, 10, 10);
        Box(interaction, 340, -10, 450, 150);
        Assert.Equal(new[] { ellipse }, state.SelectedShapeIds);
        Assert.DoesNotContain(back, state.SelectedShapeIds);
    }

    [Fact]
    public void TinyBox_IsAClick_ThatClearsTheSelection()
    {
        var (state, interaction, _, _, _) = SelectFixture();
        Click(interaction, 10, 10);

        interaction.PointerPressed(700, 700);
        interaction.PointerMoved(701, 701);
        interaction.PointerReleased(702, 702);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void TinyBox_IsMeasuredInScreenPixels()
    {
        var (state, interaction, back, _, _) = SelectFixture(zoom: 25);
        Click(interaction, 10, 10);
        Assert.Equal(new[] { back }, state.SelectedShapeIds);

        // 8 units at 25% are 2 screen pixels: a click, even though the box touches nothing special.
        interaction.PointerPressed(700, 700);
        interaction.PointerReleased(708, 708);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void SelectionBox_SkipsHiddenAndLockedLayers()
    {
        var (state, interaction, _, _, _) = SelectFixture();
        state.Document = state.Document.UpdateLayer(Bottom, l => l with { IsLocked = true }).UpdateLayer(Top, l => l with { IsVisible = false });

        Box(interaction, -10, -10, 450, 450);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void ClickOnAShapeOfALockedLayer_ActsLikeEmptySpace()
    {
        var (state, interaction, back, front, _) = SelectFixture();
        Click(interaction, 140, 140);
        Assert.Equal(front, state.SelectedShapeId);
        state.Document = state.Document.UpdateLayer(Top, l => l with { IsLocked = true });
        Click(interaction, 10, 10);
        Assert.Equal(new[] { back }, state.SelectedShapeIds);

        Click(interaction, 140, 140);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void CancelAndToolChange_DuringABox_DropTheBoxAndKeepTheSelection()
    {
        var (state, interaction, back, _, _) = SelectFixture();
        Click(interaction, 10, 10);

        interaction.PointerPressed(300, 300);
        interaction.PointerMoved(500, 500);
        interaction.Cancel();
        Assert.Null(state.SelectionBox);
        Assert.False(state.IsDragInProgress);
        Assert.Equal(new[] { back }, state.SelectedShapeIds);

        interaction.PointerPressed(300, 300);
        interaction.PointerMoved(500, 500);
        state.ActiveTool = Tool.Rectangle;
        interaction.PointerMoved(510, 510);
        Assert.Null(state.SelectionBox);
        Assert.False(state.IsDragInProgress);
        Assert.Equal(new[] { back }, state.SelectedShapeIds);
    }

    [Fact]
    public void CancelDuringAMoveOfSeveralShapes_KeepsTheReachedPositionAsOneStep()
    {
        var (state, interaction, back, front, _) = SelectFixture();
        state.SetSelection([back, front]);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(10, 10);
        interaction.PointerMoved(30, 10);
        interaction.Cancel();

        Assert.Equal(new[] { 20.0, 70 }, state.Document.Layers.SelectMany(l => l.Shapes).Take(2).Select(sh => sh.X));
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void SelectionChanges_CreateNoHistorySteps()
    {
        var (state, interaction, _, _, _) = SelectFixture();

        Click(interaction, 10, 10);
        Click(interaction, 350, 50, shift: true);
        Box(interaction, -10, -10, 450, 450);
        Click(interaction, 700, 700);

        Assert.Empty(state.History.Steps);
    }

    // ---- handles: resize, rotate, hover, delete all ----

    private static (EditorState State, CanvasInteraction Interaction, Guid Id) HandleFixture(double zoom = 100, double rotation = 0)
    {
        var shape = new RectangleShape { X = 100, Y = 100, Width = 200, Height = 100, Rotation = rotation };
        var document = Document.Create("x");
        document = document.AddShape(document.Layers[0].Id, shape);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        state.SetZoom(zoom);
        state.SelectOnly(shape.Id);
        return (state, new CanvasInteraction(state), shape.Id);
    }

    private static (double X, double Y) HandleAt(EditorState state, HandleType handle) =>
        SelectionGeometry.HandlePosition(SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(state.Document, state.SelectedShapeIds))!, handle, state.ZoomPercent);

    private static Shape Only(EditorState state) => Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));

    [Fact]
    public void ResizeDrag_BottomRight_ChangesTheSizeAsOneStep_AndUndoRestores()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.BottomRight);

        interaction.PointerPressed(hx, hy);
        Assert.True(state.IsDragInProgress);
        interaction.PointerMoved(hx + 20, hy + 10);
        interaction.PointerMoved(hx + 50, hy + 30);
        interaction.PointerReleased(hx + 50, hy + 30);

        var shape = Only(state);
        Assert.Equal((100, 100, 250, 130), (shape.X, shape.Y, shape.Width, shape.Height));
        Assert.Equal(new[] { HistoryStepType.ResizeShapes }, state.History.Steps);
        Assert.False(state.IsDragInProgress);
        Assert.Equal(new[] { state.SelectedShapeIds[0] }, state.SelectedShapeIds);

        state.Undo();
        Assert.Equal((200, 100), (Only(state).Width, Only(state).Height));
    }

    [Fact]
    public void ResizeDrag_AnEdgeHandle_ChangesOnlyOneSide()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.Left);

        interaction.PointerPressed(hx, hy);
        interaction.PointerReleased(hx - 40, hy + 500);

        var shape = Only(state);
        Assert.Equal((60, 100, 240, 100), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void ResizeDrag_PressedAFewPixelsOffTheHandle_DoesNotJump()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.BottomRight);

        interaction.PointerPressed(hx + 3, hy - 2);
        interaction.PointerMoved(hx + 3, hy - 2);
        interaction.PointerReleased(hx + 3, hy - 2);

        Assert.Equal((200, 100), (Only(state).Width, Only(state).Height));
        Assert.Empty(state.History.Steps);
    }

    [Fact]
    public void ResizeDrag_ShiftKeepsTheRatio_AltResizesAroundTheCenter()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.BottomRight);

        interaction.PointerPressed(hx, hy);
        interaction.PointerMoved(hx + 100, hy + 10, shift: true);
        interaction.PointerReleased(hx + 100, hy + 10, shift: true);
        Assert.Equal(2, Only(state).Width / Only(state).Height, 6);
        Assert.Equal(300, Only(state).Width, 6);

        state.Undo();
        interaction.PointerPressed(hx, hy);
        interaction.PointerMoved(hx + 40, hy + 20, alt: true);
        interaction.PointerReleased(hx + 40, hy + 20, alt: true);
        var shape = Only(state);
        Assert.Equal(200, shape.CenterX, 6);
        Assert.Equal(150, shape.CenterY, 6);
        Assert.Equal(280, shape.Width, 6);
    }

    [Fact]
    public void ResizeDrag_PastTheOppositeEdge_Flips()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.Right);

        interaction.PointerPressed(hx, hy);
        interaction.PointerReleased(50, hy);

        var shape = Only(state);
        Assert.Equal((50, 50), (shape.X, shape.Width));
    }

    [Fact]
    public void ResizeDrag_OfARotatedShape_FollowsTheTurnedFrame()
    {
        var (state, interaction, _) = HandleFixture(rotation: 90);
        var (hx, hy) = HandleAt(state, HandleType.Right); // turned 90 degrees: the Right handle is below the center

        interaction.PointerPressed(hx, hy);
        interaction.PointerReleased(hx, hy + 30);

        var shape = Only(state);
        Assert.Equal(230, shape.Width, 6);
        Assert.Equal(100, shape.Height, 6);
        Assert.Equal(90, shape.Rotation, 6);
    }

    [Fact]
    public void ResizeDrag_OfSeveralShapes_ScalesThemTogether()
    {
        var (state, interaction, back, front, _) = SelectFixture();
        state.SetSelection([back, front]); // box 0..150 x 0..150
        var (hx, hy) = HandleAt(state, HandleType.BottomRight);

        interaction.PointerPressed(hx, hy);
        interaction.PointerReleased(hx + 150, hy + 150);

        var shapes = state.Document.Layers.SelectMany(l => l.Shapes).Take(2).ToList();
        Assert.Equal((0, 0, 200, 200), (shapes[0].X, shapes[0].Y, shapes[0].Width, shapes[0].Height));
        Assert.Equal((100, 100, 200, 200), (shapes[1].X, shapes[1].Y, shapes[1].Width, shapes[1].Height));
        Assert.Equal(new[] { HistoryStepType.ResizeShapes }, state.History.Steps);
    }

    [Fact]
    public void RotateDrag_TheHandleFollowsThePointer_AsOneStep()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.Rotate);

        interaction.PointerPressed(hx, hy);
        Assert.True(state.IsDragInProgress);
        interaction.PointerMoved(500, 150);
        interaction.PointerReleased(500, 150);

        Assert.Equal(90, Only(state).Rotation, 6);
        Assert.Equal(new[] { HistoryStepType.RotateShapes }, state.History.Steps);
        state.Undo();
        Assert.Equal(0, Only(state).Rotation);
    }

    [Fact]
    public void RotateDrag_ShiftSnapsToFifteenDegrees_AndAStartAngleIsRespected()
    {
        var (state, interaction, _) = HandleFixture(rotation: 40);
        var (hx, hy) = HandleAt(state, HandleType.Rotate);
        var radians = (40 + 22) * Math.PI / 180;

        interaction.PointerPressed(hx, hy);
        interaction.PointerMoved(200 + 300 * Math.Sin(radians), 150 - 300 * Math.Cos(radians), shift: true);
        interaction.PointerReleased(200 + 300 * Math.Sin(radians), 150 - 300 * Math.Cos(radians), shift: true);

        Assert.Equal(60, Only(state).Rotation, 6);
    }

    [Fact]
    public void RotateDrag_OfSeveralShapes_TurnsThemAroundTheFrameCenter()
    {
        var (state, interaction, back, _, ellipse) = SelectFixture();
        state.SetSelection([back, ellipse]);
        var frame = SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(state.Document, state.SelectedShapeIds))!;
        var (hx, hy) = HandleAt(state, HandleType.Rotate);

        interaction.PointerPressed(hx, hy);
        interaction.PointerReleased(frame.CenterX + 400, frame.CenterY);

        var shapes = state.Document.Layers.SelectMany(l => l.Shapes).Where(sh => sh.Id == back || sh.Id == ellipse).ToList();
        Assert.All(shapes, sh => Assert.Equal(90, sh.Rotation, 6));
        Assert.Equal(new[] { HistoryStepType.RotateShapes }, state.History.Steps);
    }

    [Fact]
    public void ClickOnAHandle_WithoutMovement_MakesNoStep()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.Top);
        var document = state.Document;

        interaction.PointerPressed(hx, hy);
        interaction.PointerReleased(hx, hy);

        Assert.Same(document, state.Document);
        Assert.Empty(state.History.Steps);
    }

    [Fact]
    public void HandleHit_WinsOverAShapeBelow()
    {
        var (state, interaction, id) = HandleFixture();
        var other = new RectangleShape { X = 280, Y = 180, Width = 100, Height = 100 };
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, other));
        state.SelectOnly(id);
        var (hx, hy) = HandleAt(state, HandleType.BottomRight); // (300, 200) is inside the other shape as well

        interaction.PointerPressed(hx, hy);
        interaction.PointerMoved(hx + 10, hy);
        interaction.PointerReleased(hx + 10, hy);

        Assert.Equal(new[] { id }, state.SelectedShapeIds);
        Assert.Equal(210, state.Document.FindShape(id)!.Shape.Width, 6);
        Assert.Equal(280, state.Document.FindShape(other.Id)!.Shape.X);
    }

    [Fact]
    public void HandleSize_IsConstantOnTheScreen_SoItIsGrabbableAtLowZoom()
    {
        var (state, interaction, _) = HandleFixture(zoom: 25);
        var (hx, hy) = HandleAt(state, HandleType.BottomRight);

        // 4 screen pixels away at 25% is 16 document units.
        interaction.PointerPressed(hx + 16, hy + 16);
        interaction.PointerReleased(hx + 16 + 40, hy + 16);

        Assert.Equal(240, Only(state).Width, 6);
    }

    [Fact]
    public void CancelOrToolChange_DuringAResize_KeepTheReachedStateAsOneStep()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.BottomRight);

        interaction.PointerPressed(hx, hy);
        interaction.PointerMoved(hx + 50, hy);
        interaction.Cancel();
        Assert.Equal(250, Only(state).Width, 6);
        Assert.Equal(new[] { HistoryStepType.ResizeShapes }, state.History.Steps);

        var (hx2, hy2) = HandleAt(state, HandleType.BottomRight);
        interaction.PointerPressed(hx2, hy2);
        interaction.PointerMoved(hx2 + 10, hy2);
        state.ActiveTool = Tool.Rectangle;
        interaction.PointerMoved(hx2 + 20, hy2);
        Assert.False(state.IsDragInProgress);
        Assert.Equal(2, state.History.Steps.Count);
    }

    [Fact]
    public void HoverHandle_OnlyWithoutADragAndWithTheSelectTool()
    {
        var (state, interaction, _) = HandleFixture();
        var (hx, hy) = HandleAt(state, HandleType.Top);

        Assert.Equal(HandleType.Top, interaction.HoverHandle(hx, hy));
        Assert.Null(interaction.HoverHandle(0, 0));

        interaction.PointerPressed(hx, hy);
        Assert.Null(interaction.HoverHandle(hx, hy));
        interaction.Cancel();

        state.ActiveTool = Tool.Rectangle;
        Assert.Null(interaction.HoverHandle(hx, hy));

        state.ActiveTool = Tool.Select;
        state.ClearSelection();
        Assert.Null(interaction.HoverHandle(hx, hy));
    }

    [Fact]
    public void DeleteSelection_OfSeveralShapes_IsOneStep_AndClearsTheSelection()
    {
        var (state, interaction, back, front, ellipse) = SelectFixture();
        state.SetSelection([back, ellipse]);

        interaction.DeleteSelection();

        Assert.Equal(new[] { front }, state.Document.Layers.SelectMany(l => l.Shapes).Select(sh => sh.Id));
        Assert.Equal(new[] { HistoryStepType.DeleteShape }, state.History.Steps);
        Assert.False(state.HasSelection);

        state.Undo();
        Assert.Equal(3, state.Document.Layers.SelectMany(l => l.Shapes).Count());
    }

    [Fact]
    public void DeleteSelection_WithoutSelection_DoesNothing()
    {
        var (state, interaction, _, _, _) = SelectFixture();

        interaction.DeleteSelection();

        Assert.Empty(state.History.Steps);
    }
}

