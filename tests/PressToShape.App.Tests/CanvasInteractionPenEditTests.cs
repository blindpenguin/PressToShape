using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionPenEditTests
{
    private static readonly Guid LayerId = new("aaaaaaaa-0000-0000-0000-000000000001");

    private static PathShape Open() =>
        PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)]);

    private static (EditorState State, CanvasInteraction Interaction, PathShape Path) Create(PathShape? path = null, bool select = true)
    {
        var shape = path ?? Open();
        var document = (Document.Create("ignored") with { Layers = [new Layer { Id = LayerId, Name = "A" }] }).AddShape(LayerId, shape);
        var state = new EditorState(document) { ActiveTool = Tool.Pen };
        if (select)
        {
            state.Select(shape.Id);
        }

        return (state, new CanvasInteraction(state), shape);
    }

    private static void Click(CanvasInteraction interaction, double x, double y, InputPointerType type = InputPointerType.Mouse)
    {
        interaction.PointerPressed(x, y, inputType: type);
        interaction.PointerReleased(x, y);
    }

    private static PathShape Only(EditorState state) => Assert.IsType<PathShape>(Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes)));

    [Fact]
    public void Press_OnTheEndOfTheSelectedOpenPath_ContinuesIt()
    {
        var (state, interaction, path) = Create();
        var steps = state.History.Steps.Count;

        Click(interaction, 100, 100);

        Assert.True(interaction.IsPenDrawing);
        Assert.Equal(path.Id, state.PenContinuedId);
        Assert.Equal(steps, state.History.Steps.Count);
        Assert.IsType<PathShape>(state.Draft);
    }

    [Fact]
    public void Continue_FromTheEnd_AppendsNodesAndReplacesThePathInOneStep()
    {
        var (state, interaction, path) = Create();
        var steps = state.History.Steps.Count;

        Click(interaction, 100, 100);
        Click(interaction, 50, 150);
        interaction.FinishPen();

        var result = Only(state);
        Assert.Equal(path.Id, result.Id);
        Assert.Equal(4, result.Nodes.Length);
        Assert.Equal(150, result.ToDocumentNodes()[3].Y, 6);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(path.Id, state.SelectedShapeId);
        Assert.Null(state.PenContinuedId);
        Assert.Null(state.Draft);
    }

    [Fact]
    public void Continue_FromTheStart_ReversesThePathAndAppendsAtTheOldStart()
    {
        var (state, interaction, _) = Create();

        Click(interaction, 0, 0);
        Click(interaction, -50, 0);
        interaction.FinishPen();

        var nodes = Only(state).ToDocumentNodes();
        Assert.Equal(4, nodes.Count);
        Assert.Equal((100, 100), (nodes[0].X, nodes[0].Y));
        Assert.Equal((-50, 0), (nodes[3].X, nodes[3].Y));
    }

    [Fact]
    public void Continue_ThenUndo_RestoresTheOriginalPath()
    {
        var (state, interaction, path) = Create();

        Click(interaction, 100, 100);
        Click(interaction, 50, 150);
        interaction.FinishPen();
        state.Undo();

        Assert.Equal(3, Only(state).Nodes.Length);
        Assert.Equal(path.Id, Only(state).Id);
    }

    [Fact]
    public void Continue_KeepsTheLookNameAndFillRuleOfTheOldPath()
    {
        var styled = Open() with
        {
            Name = "Road",
            FillRule = FillRule.EvenOdd,
            Appearance = Appearance.Default with { StrokeWidth = 9 },
        };
        var (state, interaction, _) = Create(styled);

        Click(interaction, 100, 100);
        Click(interaction, 50, 150);
        interaction.FinishPen();

        var result = Only(state);
        Assert.Equal("Road", result.Name);
        Assert.Equal(FillRule.EvenOdd, result.FillRule);
        Assert.Equal(9, result.Appearance.StrokeWidth);
    }

    [Fact]
    public void Continue_DiscardedWithEscape_LeavesTheOriginalAlone()
    {
        var (state, interaction, path) = Create();
        var steps = state.History.Steps.Count;

        Click(interaction, 100, 100);
        Click(interaction, 50, 150);
        interaction.DiscardPen();

        Assert.Equal(path, Only(state));
        Assert.Equal(steps, state.History.Steps.Count);
        Assert.Null(state.PenContinuedId);
    }

    [Fact]
    public void Continue_FinishedAtOnceWithoutANewNode_MakesNoStep()
    {
        var (state, interaction, path) = Create();
        var steps = state.History.Steps.Count;

        Click(interaction, 100, 100);
        interaction.FinishPen();

        Assert.Equal(path, Only(state));
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void Continue_ThenClickOnTheOtherEnd_ClosesThePath()
    {
        var (state, interaction, _) = Create();

        Click(interaction, 100, 100);
        Click(interaction, 1, 1);

        var result = Only(state);
        Assert.True(result.IsClosed);
        Assert.Equal(3, result.Nodes.Length);
    }

    [Fact]
    public void Continue_ChangingTheTool_FinishesThePathWithItsNewNodes()
    {
        var (state, interaction, path) = Create();

        Click(interaction, 100, 100);
        Click(interaction, 50, 150);
        state.ActiveTool = Tool.Select;

        Assert.Equal(4, Only(state).Nodes.Length);
        Assert.Equal(path.Id, Only(state).Id);
    }

    [Fact]
    public void Press_OnAMiddleNode_DeletesItInOneStep()
    {
        var (state, interaction, _) = Create();
        var steps = state.History.Steps.Count;

        Click(interaction, 100, 0);

        Assert.Equal(2, Only(state).Nodes.Length);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.DeletePathNode, state.History.Steps[^1]);
        Assert.False(interaction.IsPenDrawing);
    }

    [Fact]
    public void Press_OnANodeOfAClosedPath_DeletesIt()
    {
        var closed = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)], isClosed: true);
        var (state, interaction, _) = Create(closed);

        Click(interaction, 0, 0);

        Assert.Equal(2, Only(state).Nodes.Length);
        Assert.True(Only(state).IsClosed);
    }

    [Fact]
    public void Delete_DownToOneNode_RemovesTheWholePath()
    {
        var closed = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)], isClosed: true);
        var (state, interaction, _) = Create(closed);

        Click(interaction, 0, 0);
        Click(interaction, 100, 100);

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Press_OnASegment_AddsANodeThereInOneStep()
    {
        var (state, interaction, _) = Create();
        var steps = state.History.Steps.Count;

        Click(interaction, 50, 1);

        var result = Only(state);
        Assert.Equal(4, result.Nodes.Length);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.AddPathNode, state.History.Steps[^1]);
        var added = result.ToDocumentNodes()[1];
        Assert.InRange(added.X, 30, 70);
        Assert.Equal(0, added.Y, 6);
        Assert.False(interaction.IsPenDrawing);
    }

    [Fact]
    public void Add_OnACurvedSegment_KeepsTheCurve()
    {
        var curve = PathShape.Create([new DocumentNode(0, 100, outX: 0, outY: 0), new DocumentNode(100, 100, inX: 100, inY: 0)]);
        var (state, interaction, _) = Create(curve);
        var onCurve = PressToShape.Core.Geometry.Bezier.Point(curve.Segments()[0], 0.5);

        Click(interaction, onCurve.X, onCurve.Y);

        var result = Only(state);
        Assert.Equal(3, result.Nodes.Length);
        var added = result.ToDocumentNodes()[1];
        Assert.Equal(onCurve.X, added.X, 3);
        Assert.Equal(onCurve.Y, added.Y, 3);
        Assert.True(added.HasIn);
        Assert.True(added.HasOut);
    }

    [Fact]
    public void Press_AwayFromThePath_DrawsANewPathAsBefore()
    {
        var (state, interaction, _) = Create();

        Click(interaction, 300, 300);
        Click(interaction, 400, 300);
        interaction.FinishPen();

        Assert.Equal(2, state.Document.Layers.SelectMany(l => l.Shapes).Count());
        Assert.Equal(3, state.Document.Layers.SelectMany(l => l.Shapes).OfType<PathShape>().First().Nodes.Length);
    }

    [Fact]
    public void Press_WithoutASelection_DrawsANewPath()
    {
        var (state, interaction, _) = Create(select: false);

        Click(interaction, 100, 100);

        Assert.True(interaction.IsPenDrawing);
        Assert.Null(state.PenContinuedId);
        Assert.Equal(3, Only(state).Nodes.Length);
    }

    [Fact]
    public void Press_OnALockedPath_ChangesNothingAndStartsANewPath()
    {
        var locked = Open() with { IsLocked = true };
        var (state, interaction, _) = Create(locked);

        Click(interaction, 100, 0);

        Assert.Equal(3, Only(state).Nodes.Length);
        Assert.True(interaction.IsPenDrawing);
        Assert.Null(state.PenContinuedId);
    }

    [Fact]
    public void Press_TwentyPixelsFromAnEnd_OnlyATouchContinues()
    {
        var (mouseState, mouse, _) = Create();
        var (touchState, touch, _) = Create();

        Click(mouse, 100, 120, InputPointerType.Mouse);
        Click(touch, 100, 120, InputPointerType.Touch);

        Assert.Null(mouseState.PenContinuedId);
        Assert.NotNull(touchState.PenContinuedId);
    }

    [Fact]
    public void Press_NearANodeWhenZoomedIn_UsesScreenPixels()
    {
        var (state, interaction, _) = Create();
        state.SetZoom(800);

        // 0.5 document units are 4 screen pixels at 800 percent: close enough to the end for a mouse.
        Click(interaction, 100.5, 100);

        Assert.NotNull(state.PenContinuedId);
    }

    [Fact]
    public void Continue_RotatedPath_BakesTheRotationIntoTheNodesAndKeepsThemOnScreen()
    {
        var rotated = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]) with { Rotation = 90 };
        var (state, interaction, _) = Create(rotated);

        // Turned by 90 degrees around (50, 0) the nodes appear at (50, -50) and (50, 50).
        Click(interaction, 50, 50);
        Click(interaction, 150, 50);
        interaction.FinishPen();

        var result = Only(state);
        Assert.Equal(0, result.Rotation);
        var nodes = result.ToDocumentNodes();
        Assert.Equal((50, -50), (Math.Round(nodes[0].X, 6), Math.Round(nodes[0].Y, 6)));
        Assert.Equal((50, 50), (Math.Round(nodes[1].X, 6), Math.Round(nodes[1].Y, 6)));
        Assert.Equal((150, 50), (nodes[2].X, nodes[2].Y));
    }

    [Fact]
    public void Add_OnARotatedPath_KeepsTheRotation()
    {
        var rotated = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0)]) with { Rotation = 90 };
        var (state, interaction, _) = Create(rotated);

        // The segment runs from (50, -50) to (50, 50) on the screen.
        Click(interaction, 51, 0);

        var result = Only(state);
        Assert.Equal(3, result.Nodes.Length);
        Assert.Equal(90, result.Rotation);
    }

    [Fact]
    public void Add_ThenUndo_RestoresThePath()
    {
        var (state, interaction, path) = Create();

        Click(interaction, 50, 1);
        state.Undo();

        Assert.Equal(path, Only(state));
    }
}
