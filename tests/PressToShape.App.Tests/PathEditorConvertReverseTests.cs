using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PathEditorConvertReverseTests
{
    private static PathShape Open(double x = 0) =>
        PathShape.Create([new DocumentNode(x, 0), new DocumentNode(x + 50, 0), new DocumentNode(x + 100, 30)]);

    private static (EditorState State, PathEditor Editor) Create(params Shape[] shapes)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
        return (state, new PathEditor(state, new FakeMessageDialogs(), FakeLocalizer.Create()));
    }

    private static void Select(EditorState state, params Shape[] shapes) => state.SetSelection(shapes.Select(shape => shape.Id));

    [Fact]
    public void CanConvert_RectangleSelected_IsTrue_AndNothingOrOnlyAPathIsFalse()
    {
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var path = Open();
        var (state, editor) = Create(rectangle, path);

        Assert.False(editor.CanConvert);

        Select(state, path);
        Assert.False(editor.CanConvert);

        Select(state, rectangle);
        Assert.True(editor.CanConvert);

        Select(state, rectangle, path);
        Assert.True(editor.CanConvert);
    }

    [Fact]
    public void CanConvert_SelectedGroupWithAnEllipse_IsTrue_AndLockedOrHiddenShapesAreFalse()
    {
        var ellipse = new EllipseShape { Width = 10, Height = 10 };
        var group = new GroupShape { Children = [ellipse] };
        var locked = new RectangleShape { X = 100, Width = 10, Height = 10, IsLocked = true };
        var hidden = new RectangleShape { X = 200, Width = 10, Height = 10, IsVisible = false };
        var (state, editor) = Create(group, locked, hidden);

        Select(state, group);
        Assert.True(editor.CanConvert);

        Select(state, locked, hidden);
        Assert.False(editor.CanConvert);
    }

    [Fact]
    public void CanConvertAndCanReverse_WhileADragIsInProgress_AreFalse()
    {
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var path = Open(200);
        var (state, editor) = Create(rectangle, path);
        Select(state, rectangle, path);
        Assert.True(editor.CanConvert);
        Assert.True(editor.CanReverse);

        state.IsDragInProgress = true;

        Assert.False(editor.CanConvert);
        Assert.False(editor.CanReverse);
    }

    [Fact]
    public void CanReverse_PathSelected_IsTrue_AndRectangleOrNothingIsFalse()
    {
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var path = Open(200);
        var (state, editor) = Create(rectangle, path);

        Assert.False(editor.CanReverse);

        Select(state, rectangle);
        Assert.False(editor.CanReverse);

        Select(state, path);
        Assert.True(editor.CanReverse);
    }

    [Fact]
    public void ConvertToPath_IsOneNamedStep_KeepsTheSelection_AndUndoBringsTheRectangleBack()
    {
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var ellipse = new EllipseShape { X = 100, Width = 10, Height = 10 };
        var (state, editor) = Create(rectangle, ellipse);
        Select(state, rectangle, ellipse);
        var steps = state.History.Steps.Count;

        editor.ConvertToPath();

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.ConvertToPath, state.History.Steps[^1]);
        Assert.All(state.Document.Layers[0].Shapes, shape => Assert.IsType<PathShape>(shape));
        Assert.Equal([rectangle.Id, ellipse.Id], state.SelectedShapeIds.ToList());

        state.Undo();

        Assert.IsType<RectangleShape>(state.Document.Layers[0].Shapes[0]);
        Assert.IsType<EllipseShape>(state.Document.Layers[0].Shapes[1]);
    }

    [Fact]
    public void ConvertToPath_WhenItCannotRun_RecordsNoStep()
    {
        var path = Open();
        var (state, editor) = Create(path);
        Select(state, path);
        var steps = state.History.Steps.Count;

        editor.ConvertToPath();

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void ReverseDirection_IsOneNamedStep_KeepsTheSelection_AndUndoRestoresThePath()
    {
        var path = Open();
        var (state, editor) = Create(path);
        Select(state, path);
        var steps = state.History.Steps.Count;

        editor.ReverseDirection();

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.ReverseDirection, state.History.Steps[^1]);
        Assert.Equal([path.Id], state.SelectedShapeIds.ToList());
        var reversed = Assert.IsType<PathShape>(state.Document.Layers[0].Shapes[0]);
        Assert.Equal(100, reversed.ToDocumentSubPaths()[0].Nodes[0].X, 6);

        state.Undo();

        var restored = Assert.IsType<PathShape>(state.Document.Layers[0].Shapes[0]);
        Assert.Equal(0, restored.ToDocumentSubPaths()[0].Nodes[0].X, 6);
    }

    [Fact]
    public void ReverseDirection_WhenItCannotRun_RecordsNoStep()
    {
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var (state, editor) = Create(rectangle);
        Select(state, rectangle);
        var steps = state.History.Steps.Count;

        editor.ReverseDirection();

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void Shell_TheMenuCommands_AreTheShellCommands_AndRunTheOperations()
    {
        var shell = new TestShell();
        var rectangle = shell.AddRectangle(0, 0, 100, 50);
        var path = Open(300);
        shell.State.Edit(HistoryStepType.DrawPath, d => d.AddShape(shell.State.ActiveLayerId, path));
        var commands = shell.PathCommandsForTests;
        Assert.NotNull(commands.ConvertToPath);
        Assert.NotNull(commands.ReverseDirection);
        Assert.False(commands.ConvertToPath.CanExecute(null));
        Assert.False(commands.ReverseDirection.CanExecute(null));

        shell.State.SetSelection([rectangle.Id, path.Id]);

        Assert.True(commands.ConvertToPath.CanExecute(null));
        Assert.True(commands.ReverseDirection.CanExecute(null));

        commands.ConvertToPath.Execute(null);
        commands.ReverseDirection.Execute(null);

        var shapes = shell.State.Document.Layers[0].Shapes;
        Assert.All(shapes, shape => Assert.IsType<PathShape>(shape));
        Assert.False(commands.ConvertToPath.CanExecute(null));
        Assert.True(commands.ReverseDirection.CanExecute(null));
    }
}
