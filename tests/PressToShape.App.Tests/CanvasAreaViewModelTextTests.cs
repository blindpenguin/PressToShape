using System.Collections.Generic;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelTextTests
{
    private static readonly FakeTextMeasurer Measurer = new();

    private static (EditorState State, CanvasAreaViewModel Canvas, List<string> Changed) Create(params Shape[] shapes)
    {
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, shapes);
        var state = new EditorState(document) { ActiveTool = Tool.Text };
        var canvas = new CanvasAreaViewModel(state, textMeasurer: Measurer);
        List<string> changed = [];
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);
        return (state, canvas, changed);
    }

    private static void Click(CanvasAreaViewModel canvas, double x, double y)
    {
        var screenX = canvas.OriginX + x * canvas.ZoomPercent / 100;
        var screenY = canvas.OriginY + y * canvas.ZoomPercent / 100;
        canvas.PointerPressed(screenX, screenY);
        canvas.PointerReleased(screenX, screenY);
    }

    [Fact]
    public void Initially_NoTextIsEdited()
    {
        var (_, canvas, _) = Create();

        Assert.False(canvas.IsEditingText);
        Assert.Equal(string.Empty, canvas.EditedText);
        Assert.Null(canvas.EditedTextShape);
        Assert.True(double.IsNaN(canvas.EditedTextLineHeight));
    }

    [Fact]
    public void ClickWithTheTextTool_StartsEditingAndNotifies()
    {
        var (_, canvas, changed) = Create();

        Click(canvas, 40, 50);

        Assert.True(canvas.IsEditingText);
        Assert.Contains(nameof(CanvasAreaViewModel.IsEditingText), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.EditedTextShape), changed);
        Assert.NotNull(canvas.EditedTextShape);
        Assert.Equal(ScreenShapeType.Text, canvas.EditedTextShape!.Value.Type);
    }

    [Fact]
    public void EditedText_Set_ChangesTheDraftAndMeasuresAPointText()
    {
        var (state, canvas, changed) = Create();
        Click(canvas, 40, 50);
        changed.Clear();

        canvas.EditedText = "Hello";

        Assert.Equal("Hello", canvas.EditedText);
        Assert.Equal("Hello", state.TextEdit!.Text.Text);
        var size = Measurer.Layout(state.TextEdit.Text);
        Assert.Equal((size.Width, size.Height), (state.TextEdit.Text.Width, state.TextEdit.Text.Height));
        Assert.Contains(nameof(CanvasAreaViewModel.EditedText), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.EditedTextShape), changed);
        var screen = canvas.EditedTextShape!.Value;
        Assert.Equal(size.Width * canvas.ZoomPercent / 100, screen.Width, 9);
        Assert.Equal(["Hello"], screen.Text!.Lines.Select(line => line.Text));
    }

    [Fact]
    public void EditedText_SetToTheSameValue_DoesNotNotify()
    {
        var (_, canvas, changed) = Create();
        Click(canvas, 40, 50);
        canvas.EditedText = "Hello";
        changed.Clear();

        canvas.EditedText = "Hello";

        Assert.Empty(changed);
    }

    [Fact]
    public void EditedText_Set_DoesNotChangeTheDocumentUntilCommit()
    {
        var (state, canvas, _) = Create();
        var document = state.Document;
        Click(canvas, 40, 50);

        canvas.EditedText = "Hello";

        Assert.Same(document, state.Document);
    }

    [Fact]
    public void EditedText_Set_WithoutEditing_DoesNothing()
    {
        var (state, canvas, changed) = Create();

        canvas.EditedText = "Hello";

        Assert.Empty(changed);
        Assert.Null(state.TextEdit);
    }

    [Fact]
    public void EditedText_AreaText_KeepsItsBox()
    {
        var (state, canvas, _) = Create();
        canvas.PointerPressed(canvas.OriginX + 10, canvas.OriginY + 10);
        canvas.PointerMoved(canvas.OriginX + 110, canvas.OriginY + 60);
        canvas.PointerReleased(canvas.OriginX + 110, canvas.OriginY + 60);

        canvas.EditedText = "some longer text that wraps";

        Assert.Equal((100d, 50d), (state.TextEdit!.Text.Width, state.TextEdit.Text.Height));
    }

    [Fact]
    public void CommitTextEditCommand_EndsTheEditingAndPutsTheTextIntoTheDocument()
    {
        var (state, canvas, changed) = Create();
        Click(canvas, 40, 50);
        canvas.EditedText = "Hello";
        changed.Clear();

        canvas.CommitTextEditCommand.Execute(null);

        Assert.False(canvas.IsEditingText);
        Assert.Null(canvas.EditedTextShape);
        Assert.Equal(string.Empty, canvas.EditedText);
        Assert.Contains(nameof(CanvasAreaViewModel.IsEditingText), changed);
        var text = Assert.Single(state.Document.Layers[0].Shapes.OfType<TextShape>());
        Assert.Equal("Hello", text.Text);
    }

    [Fact]
    public void EditedTextLineHeight_IsTheLineHeightOfTheFontAtTheZoom()
    {
        var (state, canvas, _) = Create();
        Click(canvas, 40, 50);
        state.RememberTextFormat(new TextFormatChange(FontSize: 10));
        state.TextEdit!.SetBox(0, 0, 0, 0);
        var text = state.TextEdit.Text with { FontSize = 10, LineSpacing = 2 };
        var session = new TextEditSession(text, isNew: true);
        state.TextEdit = session;

        Assert.Equal(FakeTextMeasurer.LineHeight(text) * canvas.ZoomPercent / 100, canvas.EditedTextLineHeight, 9);
    }

    [Fact]
    public void ZoomChange_UpdatesTheScreenShapeOfTheEditedText()
    {
        var (state, canvas, changed) = Create();
        Click(canvas, 40, 50);
        canvas.EditedText = "Hello";
        var before = canvas.EditedTextShape!.Value.Width;
        changed.Clear();

        state.ZoomPercent = 200;

        Assert.Contains(nameof(CanvasAreaViewModel.EditedTextShape), changed);
        Assert.Equal(before * 2, canvas.EditedTextShape!.Value.Width, 9);
    }

    [Fact]
    public void EditingAnExistingText_ShowsItsContentAndHidesItOnTheCanvas()
    {
        var existing = new TextShape { X = 100, Y = 100, Width = 60, Height = 12, Text = "Hello", FontSize = 10 };
        var (_, canvas, _) = Create(existing);

        Click(canvas, 110, 105);

        Assert.Equal("Hello", canvas.EditedText);
        Assert.Equal(existing.Id, canvas.HiddenShapeId);
    }

    [Fact]
    public void WithoutAMeasurer_TheLineHeightIsLeftToTheEditor()
    {
        var state = new EditorState { ActiveTool = Tool.Text };
        var canvas = new CanvasAreaViewModel(state);

        Click(canvas, 40, 50);

        Assert.True(double.IsNaN(canvas.EditedTextLineHeight));
    }
}
