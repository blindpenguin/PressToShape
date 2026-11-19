using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

public class PathEditorTextTests
{
    private static TextShape Text(string content = "ab", double x = 0) =>
        new() { X = x, Y = 0, Width = 12, Height = 12, Text = content, FontSize = 10 };

    private static (EditorState State, PathEditor Editor) Create(bool withOutliner = true, params Shape[] shapes)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
        return (state, new PathEditor(state, new FakeMessageDialogs(), FakeLocalizer.Create(), textOutliner: withOutliner ? new FakeTextOutliner() : null));
    }

    private static void Select(EditorState state, params Shape[] shapes) => state.SetSelection(shapes.Select(shape => shape.Id));

    private static string LastStepName(EditorState state) => state.History.Steps[^1].ToString();

    [Fact]
    public void CanConvertText_TextSelected_IsTrue_NothingRectangleOrEmptyTextIsFalse()
    {
        var text = Text();
        var rectangle = new RectangleShape { X = 100, Width = 10, Height = 10 };
        var empty = Text(string.Empty, x: 200);
        var blank = Text("  ", x: 300);
        var (state, editor) = Create(true, text, rectangle, empty, blank);

        Assert.False(editor.CanConvertText);

        Select(state, rectangle);
        Assert.False(editor.CanConvertText);

        Select(state, empty, blank);
        Assert.False(editor.CanConvertText);

        Select(state, text);
        Assert.True(editor.CanConvertText);

        Select(state, text, rectangle);
        Assert.True(editor.CanConvertText);
    }

    [Fact]
    public void CanConvertText_LockedHiddenOrWithoutAnOutliner_IsFalse()
    {
        var locked = Text() with { IsLocked = true };
        var hidden = Text(x: 100) with { IsVisible = false };
        var plain = Text(x: 200);
        var (state, editor) = Create(true, locked, hidden, plain);
        Select(state, locked, hidden);
        Assert.False(editor.CanConvertText);

        var (state2, editorWithout) = Create(false, plain);
        Select(state2, plain);
        Assert.False(editorWithout.CanConvertText);
        Assert.False(editorWithout.CanConvert);
    }

    [Fact]
    public void CanConvertText_TextInASelectedGroup_IsTrue()
    {
        var text = Text();
        var group = new GroupShape { Children = [text, new RectangleShape { X = 50, Width = 5, Height = 5 }] };
        var (state, editor) = Create(true, group);

        Select(state, group);

        Assert.True(editor.CanConvertText);
    }

    [Fact]
    public void CanConvert_TextSelected_IsTrue()
    {
        var text = Text();
        var (state, editor) = Create(true, text);

        Select(state, text);

        Assert.True(editor.CanConvert);
    }

    [Fact]
    public void ConvertTextToPath_ReplacesTheTextByAPathAsOneStep_AndKeepsTheSelection()
    {
        var text = Text();
        var rectangle = new RectangleShape { X = 100, Width = 10, Height = 10 };
        var (state, editor) = Create(true, text, rectangle);
        Select(state, text, rectangle);
        var steps = state.History.Steps.Count;

        editor.ConvertTextToPath();

        Assert.IsType<PathShape>(state.Document.FindShape(text.Id)!.Shape);
        Assert.IsType<RectangleShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal("ConvertTextToPath", LastStepName(state));
        Assert.Equal([text.Id, rectangle.Id], state.SelectedShapeIds.ToArray());
        Assert.False(editor.CanConvertText);
    }

    [Fact]
    public void ConvertTextToPath_Undo_BringsTheTextBack()
    {
        var text = Text();
        var (state, editor) = Create(true, text);
        Select(state, text);
        editor.ConvertTextToPath();

        state.Undo();

        Assert.Equal(text, state.Document.FindShape(text.Id)!.Shape);
    }

    [Fact]
    public void ConvertTextToPath_NothingToConvert_DoesNothing()
    {
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var (state, editor) = Create(true, rectangle);
        Select(state, rectangle);
        var document = state.Document;

        editor.ConvertTextToPath();

        Assert.Same(document, state.Document);
    }

    [Fact]
    public void ConvertToPath_TextAndRectangle_ConvertsBothAsOneConvertToPathStep()
    {
        var text = Text();
        var rectangle = new RectangleShape { X = 100, Width = 10, Height = 10 };
        var (state, editor) = Create(true, text, rectangle);
        Select(state, text, rectangle);
        var steps = state.History.Steps.Count;

        editor.ConvertToPath();

        Assert.All(state.Document.Layers[0].Shapes, shape => Assert.IsType<PathShape>(shape));
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal("ConvertToPath", LastStepName(state));
    }

    [Fact]
    public void ConvertToPath_OnlyAText_IsAConvertTextToPathStep()
    {
        var text = Text();
        var (state, editor) = Create(true, text);
        Select(state, text);

        editor.ConvertToPath();

        Assert.IsType<PathShape>(state.Document.FindShape(text.Id)!.Shape);
        Assert.Equal("ConvertTextToPath", LastStepName(state));
    }

    [Fact]
    public void BooleanAndCombine_TextAmongTheSelection_AreDisabled()
    {
        var text = Text();
        var first = new RectangleShape { X = 0, Width = 20, Height = 20 };
        var second = new RectangleShape { X = 10, Width = 20, Height = 20 };
        var (state, editor) = Create(true, text, first, second);

        Select(state, first, second);
        Assert.True(editor.CanBoolean);
        Assert.True(editor.CanCombine);

        Select(state, first, second, text);
        Assert.False(editor.CanBoolean);
        Assert.False(editor.CanCombine);

        Select(state, text, first);
        Assert.False(editor.CanBoolean);
        Assert.False(editor.CanCombine);
    }

    [Fact]
    public void BooleanAndCombine_TextInsideASelectedGroup_AreDisabled()
    {
        var text = Text();
        var group = new GroupShape { Children = [text, new RectangleShape { X = 50, Width = 20, Height = 20 }] };
        var other = new RectangleShape { X = 10, Width = 20, Height = 20 };
        var (state, editor) = Create(true, group, other);

        Select(state, group, other);

        Assert.False(editor.CanBoolean);
    }

    [Fact]
    public void Shell_TheTextCommand_IsDisabledWithoutATextAndConvertsTheSelectedText()
    {
        var shell = new TestShell();
        var commands = shell.PathCommandsForTests;
        Assert.NotNull(commands.ConvertTextToPath);
        Assert.False(commands.ConvertTextToPath.CanExecute(null));
        var text = Text();
        shell.State.Edit(HistoryStepType.CreateText, d => d.AddShape(shell.State.ActiveLayerId, text));

        shell.State.SetSelection([text.Id]);

        Assert.True(commands.ConvertTextToPath.CanExecute(null));
        Assert.True(commands.ConvertToPath!.CanExecute(null));
        commands.ConvertTextToPath.Execute(null);
        Assert.IsType<PathShape>(shell.State.Document.FindShape(text.Id)!.Shape);
        Assert.False(commands.ConvertTextToPath.CanExecute(null));
    }
}
