using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionGuideTests
{
    private static Guide Vertical(double x) => new() { Orientation = GuideOrientation.Vertical, Position = x };

    private static Guide Horizontal(double y) => new() { Orientation = GuideOrientation.Horizontal, Position = y };

    private static (EditorState State, CanvasInteraction Interaction) Make(Tool tool, params Guide[] guides)
    {
        var document = Document.Create("L");
        document = document.AddShape(document.Layers[0].Id, new RectangleShape { X = 100, Y = 100, Width = 100, Height = 100 });
        foreach (var guide in guides)
        {
            document = document.AddGuide(guide);
        }

        var state = new EditorState(document) { ActiveTool = tool };
        return (state, new CanvasInteraction(state));
    }

    [Fact]
    public void BeginGuideFromRuler_MovesAndEnds_MakeAGuide()
    {
        var (state, interaction) = Make(Tool.Select);
        var steps = state.History.Steps.Count;

        interaction.BeginGuideFromRuler(GuideOrientation.Horizontal);
        interaction.PointerMoved(50, 300);
        interaction.PointerMoved(60, 320);
        interaction.EndGuideDrag(60, 320, overCanvas: true);

        var guide = Assert.Single(state.Document.Guides);
        Assert.Equal((GuideOrientation.Horizontal, 320.0), (guide.Orientation, guide.Position));
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.CreateGuide, state.History.Steps[^1]);
        Assert.Equal(guide.Id, state.SelectedGuideId);
    }

    [Fact]
    public void BeginGuideFromRuler_Vertical_UsesTheX()
    {
        var (state, interaction) = Make(Tool.Select);

        interaction.BeginGuideFromRuler(GuideOrientation.Vertical);
        interaction.PointerMoved(250, 70);
        interaction.EndGuideDrag(250, 70, overCanvas: true);

        Assert.Equal((GuideOrientation.Vertical, 250.0), (state.Document.Guides[0].Orientation, state.Document.Guides[0].Position));
    }

    [Fact]
    public void WhileMaking_TheGuideFollowsThePointerInThePreview()
    {
        var (state, interaction) = Make(Tool.Select);
        interaction.BeginGuideFromRuler(GuideOrientation.Horizontal);

        interaction.PointerMoved(0, 120);
        Assert.Equal(120, state.Document.Guides[0].Position);
        interaction.PointerMoved(0, 180);

        Assert.Single(state.Document.Guides);
        Assert.Equal(180, state.Document.Guides[0].Position);
        Assert.True(state.IsDragInProgress);
        interaction.EndGuideDrag(0, 180, true);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void DroppedOutsideTheCanvas_MakesNoGuideAndNoStep()
    {
        var (state, interaction) = Make(Tool.Select);
        var steps = state.History.Steps.Count;

        interaction.BeginGuideFromRuler(GuideOrientation.Horizontal);
        interaction.PointerMoved(50, 300);
        interaction.EndGuideDrag(50, -5, overCanvas: false);

        Assert.Empty(state.Document.Guides);
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void BeginGuideFromRuler_SwitchesShowGuidesOn()
    {
        var (state, interaction) = Make(Tool.Select);
        state.Snapping.ShowGuides = false;

        interaction.BeginGuideFromRuler(GuideOrientation.Vertical);

        Assert.True(state.Snapping.ShowGuides);
        interaction.Abort();
    }

    [Fact]
    public void PressOnAGuide_Moves_AndKeepsTheGrabOffset()
    {
        var guide = Vertical(400);
        var (state, interaction) = Make(Tool.Select, guide);

        // Pressed 3 pixels left of the guide, then dragged 50 to the right: the guide moves by 50, it does not jump to the pointer.
        interaction.PointerPressed(397, 50);
        interaction.PointerMoved(447, 60);
        interaction.PointerReleased(447, 60);

        Assert.Equal(450, state.Document.FindGuide(guide.Id)!.Position, 9);
        Assert.Equal(HistoryStepType.MoveGuide, state.History.Steps[^1]);
    }

    [Fact]
    public void PressOnAGuide_SelectsIt_AndAClickMakesNoStep()
    {
        var guide = Horizontal(300);
        var (state, interaction) = Make(Tool.Select, guide);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(50, 302);
        interaction.PointerReleased(50, 302);

        Assert.Equal(guide.Id, state.SelectedGuideId);
        Assert.Equal(300, state.Document.FindGuide(guide.Id)!.Position, 9);
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Theory]
    [InlineData(false, 4, true)]
    [InlineData(false, 5, false)]
    [InlineData(true, 12, true)]
    [InlineData(true, 13, false)]
    public void HitDistance_IsFourForMouseAndTwelveForTouch(bool touch, double distance, bool hits)
    {
        var (_, interaction) = Make(Tool.Select, Vertical(400));

        var found = interaction.GuideAt(400 + distance, 10, touch ? InputPointerType.Touch : InputPointerType.Mouse);

        Assert.Equal(hits, found is not null);
    }

    [Fact]
    public void HitDistance_ScalesWithTheZoom()
    {
        var (state, interaction) = Make(Tool.Select, Vertical(400));
        state.SetZoom(200);

        Assert.NotNull(interaction.GuideAt(402, 10));
        Assert.Null(interaction.GuideAt(403, 10));
    }

    [Fact]
    public void MovedAndDroppedOutside_DeletesTheGuide_AsOneStep()
    {
        var guide = Vertical(400);
        var (state, interaction) = Make(Tool.Select, guide);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(400, 50);
        interaction.PointerMoved(300, 50);
        interaction.EndGuideDrag(-10, 50, overCanvas: false);

        Assert.Empty(state.Document.Guides);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.DeleteGuide, state.History.Steps[^1]);
        Assert.Null(state.SelectedGuideId);
    }

    [Fact]
    public void Abort_RestoresTheGuide_AndRecordsNothing()
    {
        var guide = Vertical(400);
        var (state, interaction) = Make(Tool.Select, guide);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(400, 50);
        interaction.PointerMoved(500, 50);
        interaction.Abort();

        Assert.Equal(400, state.Document.FindGuide(guide.Id)!.Position, 9);
        Assert.Equal(steps, state.History.Steps.Count);
        Assert.False(state.IsDragInProgress);

        interaction.BeginGuideFromRuler(GuideOrientation.Horizontal);
        interaction.PointerMoved(0, 200);
        interaction.Abort();
        Assert.Single(state.Document.Guides);
    }

    [Fact]
    public void UndoAndRedo_FollowTheGuideSteps()
    {
        var (state, interaction) = Make(Tool.Select);
        interaction.BeginGuideFromRuler(GuideOrientation.Horizontal);
        interaction.PointerMoved(0, 250);
        interaction.EndGuideDrag(0, 250, true);

        state.Undo();
        Assert.Empty(state.Document.Guides);
        state.Redo();

        Assert.Equal(250, Assert.Single(state.Document.Guides).Position);
    }

    [Fact]
    public void AGuideWinsOverAShapeOnlyWithinThePressDistance()
    {
        // The guide at x 150 crosses the rectangle (x 100..200). A press 10 away from it selects the rectangle, a press on it takes the guide.
        var guide = Vertical(150);
        var (state, interaction) = Make(Tool.Select, guide);

        interaction.PointerPressed(160, 150);
        interaction.PointerReleased(160, 150);
        Assert.Single(state.SelectedShapeIds);
        Assert.Null(state.SelectedGuideId);

        interaction.PointerPressed(151, 150);
        interaction.PointerReleased(151, 150);
        Assert.Equal(guide.Id, state.SelectedGuideId);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void ShowGuidesOff_GuidesAreIgnored()
    {
        var guide = Vertical(400);
        var (state, interaction) = Make(Tool.Select, guide);
        state.Snapping.ShowGuides = false;

        interaction.PointerPressed(400, 50);
        interaction.PointerReleased(400, 50);

        Assert.Null(state.SelectedGuideId);
        Assert.Null(interaction.GuideAt(400, 50));
    }

    [Fact]
    public void AnyToolMovesAGuide_ButThePenDrawingAndAColorPickDoNot()
    {
        var guide = Vertical(400);
        var (rectState, rectInteraction) = Make(Tool.Rectangle, guide);
        rectInteraction.PointerPressed(400, 50);
        rectInteraction.PointerMoved(420, 50);
        rectInteraction.PointerReleased(420, 50);
        Assert.Equal(420, rectState.Document.FindGuide(guide.Id)!.Position, 9);
        Assert.Single(rectState.Document.Layers[0].Shapes);

        var (penState, penInteraction) = Make(Tool.Pen, guide);
        penInteraction.PointerPressed(10, 10);
        penInteraction.PointerReleased(10, 10);
        Assert.True(penInteraction.IsPenDrawing);
        penInteraction.PointerPressed(400, 50);
        penInteraction.PointerReleased(400, 50);
        Assert.Null(penState.SelectedGuideId);

        var (pickState, pickInteraction) = Make(Tool.Select, guide);
        pickState.BeginColorPick(ColorPickTarget.Fill);
        pickInteraction.PointerPressed(400, 50);
        Assert.Null(pickState.SelectedGuideId);
    }

    [Fact]
    public void PenToolWithoutAPath_CanMoveAGuide()
    {
        var guide = Vertical(400);
        var (state, interaction) = Make(Tool.Pen, guide);

        interaction.PointerPressed(400, 50);
        interaction.PointerMoved(430, 50);
        interaction.PointerReleased(430, 50);

        Assert.Equal(430, state.Document.FindGuide(guide.Id)!.Position, 9);
        Assert.False(interaction.IsPenDrawing);
    }

    [Fact]
    public void ThePositionIsNotSnapped()
    {
        var (state, interaction) = Make(Tool.Select);
        state.Snapping.SnapToGrid = true;
        state.Snapping.SnapToObjects = true;

        interaction.BeginGuideFromRuler(GuideOrientation.Vertical);
        interaction.PointerMoved(103, 0);
        interaction.EndGuideDrag(103, 0, true);

        Assert.Equal(103, state.Document.Guides[0].Position, 9);
    }
}
