using System;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasAreaViewModelSnapNudgeTests
{
    private static (EditorState State, CanvasAreaViewModel Canvas, RectangleShape Shape) Create(double x, double y, Tool tool = Tool.Select)
    {
        var shape = new RectangleShape { X = x, Y = y, Width = 50, Height = 40 };
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShape(document.Layers[0].Id, shape)) { ActiveTool = tool };
        state.Select(shape.Id);
        return (state, new CanvasAreaViewModel(state, new SelectionEditor(state)), shape);
    }

    private static Shape Find(EditorState state, Guid id) => state.Document.FindShape(id)!.Shape;

    [Fact]
    public void Arrow_SnapToGridOn_MovesTheTopLeftCornerToTheNextLine()
    {
        var (state, canvas, shape) = Create(103, 98);
        state.Snapping.SnapToGrid = true;

        canvas.KeyPressed(CanvasKey.Right, shift: false);
        Assert.Equal((110, 98), (Find(state, shape.Id).X, Find(state, shape.Id).Y));
        canvas.KeyPressed(CanvasKey.Down, shift: false);

        Assert.Equal((110, 100), (Find(state, shape.Id).X, Find(state, shape.Id).Y));
    }

    [Fact]
    public void Arrow_SnapToGridOn_ABoxOnTheLineMovesOneFullSpacing()
    {
        var (state, canvas, shape) = Create(100, 100);
        state.Snapping.SnapToGrid = true;
        state.Snapping.GridSpacing = 25;

        canvas.KeyPressed(CanvasKey.Left, shift: false);

        Assert.Equal(75, Find(state, shape.Id).X, 9);
    }

    [Fact]
    public void Arrow_SnapToGridOnWithShift_MovesTenSpacings()
    {
        var (state, canvas, shape) = Create(103, 98);
        state.Snapping.SnapToGrid = true;

        canvas.KeyPressed(CanvasKey.Right, shift: true);

        Assert.Equal(200, Find(state, shape.Id).X, 9);
    }

    [Fact]
    public void Arrow_SnapToGridOff_KeepsTheOldSteps()
    {
        var (state, canvas, shape) = Create(103, 98);
        state.Snapping.ShowGrid = true;

        canvas.KeyPressed(CanvasKey.Right, shift: false);
        canvas.KeyPressed(CanvasKey.Down, shift: true);

        Assert.Equal((104, 108), (Find(state, shape.Id).X, Find(state, shape.Id).Y));
    }

    [Fact]
    public void Arrow_SnapToObjectsOnly_DoesNotUseTheGrid()
    {
        var (state, canvas, shape) = Create(103, 98);
        state.Snapping.SnapToObjects = true;

        canvas.KeyPressed(CanvasKey.Right, shift: false);

        Assert.Equal(104, Find(state, shape.Id).X, 9);
    }

    [Fact]
    public void Arrow_SeveralPresses_AreOneUndoStep()
    {
        var (state, canvas, shape) = Create(103, 98);
        state.Snapping.SnapToGrid = true;
        var steps = state.History.Steps.Count;

        canvas.KeyPressed(CanvasKey.Right, shift: false);
        canvas.KeyPressed(CanvasKey.Right, shift: false);
        canvas.KeyPressed(CanvasKey.Down, shift: false);

        Assert.Equal((120, 100), (Find(state, shape.Id).X, Find(state, shape.Id).Y));
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.NudgeShapes, state.History.Steps.Last());
    }

    [Fact]
    public void Arrow_SeveralShapes_UseTheTopLeftOfTheirCommonBox()
    {
        var a = new RectangleShape { X = 103, Y = 140, Width = 20, Height = 20 };
        var b = new RectangleShape { X = 150, Y = 98, Width = 20, Height = 20 };
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [a, b])) { ActiveTool = Tool.Select };
        state.SetSelection([a.Id, b.Id]);
        state.Snapping.SnapToGrid = true;
        var canvas = new CanvasAreaViewModel(state, new SelectionEditor(state));

        canvas.KeyPressed(CanvasKey.Right, shift: false);

        // The common box starts at x 103: it moves by 7, both shapes together.
        Assert.Equal(110, Find(state, a.Id).X, 9);
        Assert.Equal(157, Find(state, b.Id).X, 9);
    }

    [Fact]
    public void Arrow_WithSelectedNodes_UsesTheTopLeftOfTheNodes()
    {
        var path = PathShape.Create([new DocumentNode(103, 98), new DocumentNode(203, 150), new DocumentNode(303, 98)]);
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShape(document.Layers[0].Id, path)) { ActiveTool = Tool.DirectSelect };
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 0), new NodeRef(path.Id, 1)]);
        state.Snapping.SnapToGrid = true;
        var canvas = new CanvasAreaViewModel(state, new SelectionEditor(state));

        canvas.KeyPressed(CanvasKey.Right, shift: false);
        canvas.KeyPressed(CanvasKey.Down, shift: false);

        var nodes = Assert.IsType<PathShape>(Find(state, path.Id)).ToDocumentNodes();
        Assert.Equal((110, 100), (Math.Round(nodes[0].X, 6), Math.Round(nodes[0].Y, 6)));
        Assert.Equal((210, 152), (Math.Round(nodes[1].X, 6), Math.Round(nodes[1].Y, 6)));
        Assert.Equal(303, nodes[2].X, 6);
    }

    [Fact]
    public void Arrow_SnapToGridOffWithNodes_KeepsTheOldStep()
    {
        var path = PathShape.Create([new DocumentNode(103, 98), new DocumentNode(203, 150)]);
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShape(document.Layers[0].Id, path)) { ActiveTool = Tool.Node };
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);
        var canvas = new CanvasAreaViewModel(state, new SelectionEditor(state));

        canvas.KeyPressed(CanvasKey.Right, shift: false);

        Assert.Equal(104, Assert.IsType<PathShape>(Find(state, path.Id)).ToDocumentNodes()[0].X, 6);
    }
}
