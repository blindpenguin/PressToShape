using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
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
        var (state, interaction) = Create(Tool.Select, WithShapes(out _, out _, out var ellipse));
        state.Select(ellipse);

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
}
