using System;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionTextTests
{
    private sealed record Scene(EditorState State, CanvasInteraction Interaction);

    private static Scene Create(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        document = document.AddShapes(document.Layers[0].Id, shapes);
        var state = new EditorState(document) { ActiveTool = Tool.Text };
        return new Scene(state, new CanvasInteraction(state, new FakeTextMeasurer()));
    }

    private static TextShape Text(double x = 100, double y = 100, string content = "Hello") =>
        new() { X = x, Y = y, Width = 60, Height = 12, Text = content, FontSize = 10 };

    private static void Click(CanvasInteraction interaction, double x, double y)
    {
        interaction.PointerPressed(x, y);
        interaction.PointerReleased(x, y);
    }

    private static void Drag(CanvasInteraction interaction, double fromX, double fromY, double toX, double toY)
    {
        interaction.PointerPressed(fromX, fromY);
        interaction.PointerMoved(toX, toY);
        interaction.PointerReleased(toX, toY);
    }

    // The canvas takes screen pixels, the tests think in document units.
    private static void Click(CanvasAreaViewModel canvas, double x, double y)
    {
        var screenX = canvas.OriginX + x * canvas.ZoomPercent / 100;
        var screenY = canvas.OriginY + y * canvas.ZoomPercent / 100;
        canvas.PointerPressed(screenX, screenY);
        canvas.PointerReleased(screenX, screenY);
    }

    private static TextShape[] Texts(EditorState state) => [.. state.Document.Layers.SelectMany(layer => layer.Shapes).OfType<TextShape>()];

    private static void Type(Scene scene, string content)
    {
        scene.State.TextEdit!.SetContent(content);
        scene.Interaction.CommitTextEdit();
    }

    [Fact]
    public void Click_OnEmptyCanvas_StartsEditingANewPointTextAndLeavesTheDocumentAlone()
    {
        var scene = Create();
        var document = scene.State.Document;

        Click(scene.Interaction, 40, 50);

        var session = scene.State.TextEdit!;
        Assert.True(session.IsNew);
        Assert.False(session.Text.IsArea);
        Assert.Equal((40d, 50d), (session.Text.X, session.Text.Y));
        Assert.Same(document, scene.State.Document);
        Assert.True(scene.Interaction.IsEditingText);
    }

    [Fact]
    public void Drag_OnEmptyCanvas_StartsEditingANewAreaTextWithTheDraggedBox()
    {
        var scene = Create();

        Drag(scene.Interaction, 40, 50, 140, 90);

        var text = scene.State.TextEdit!.Text;
        Assert.True(text.IsArea);
        Assert.Equal((40d, 50d, 100d, 40d), (text.X, text.Y, text.Width, text.Height));
    }

    [Fact]
    public void Drag_FromTheBottomRightToTheTopLeft_UsesTheSameBox()
    {
        var scene = Create();

        Drag(scene.Interaction, 140, 90, 40, 50);

        var text = scene.State.TextEdit!.Text;
        Assert.Equal((40d, 50d, 100d, 40d), (text.X, text.Y, text.Width, text.Height));
    }

    [Fact]
    public void Drag_BelowTheClickThreshold_IsAPointText()
    {
        var scene = Create();

        Drag(scene.Interaction, 40, 50, 41, 51);

        Assert.False(scene.State.TextEdit!.Text.IsArea);
    }

    [Fact]
    public void Drag_WhileDragging_ShowsTheBoxAsTheSelectionBox()
    {
        var scene = Create();

        scene.Interaction.PointerPressed(40, 50);
        scene.Interaction.PointerMoved(100, 80);

        Assert.Equal(new SelectionBox(40, 50, 60, 30), scene.State.SelectionBox);
        scene.Interaction.PointerReleased(100, 80);
        Assert.Null(scene.State.SelectionBox);
    }

    [Fact]
    public void Drag_WithSnapToGrid_SnapsTheCorners()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;
        var spacing = scene.State.Snapping.GridSpacing;

        Drag(scene.Interaction, spacing + 1, spacing + 2, spacing * 4 - 1, spacing * 3 + 2);

        var text = scene.State.TextEdit!.Text;
        Assert.Equal((spacing, spacing, spacing * 3, spacing * 2), (text.X, text.Y, text.Width, text.Height));
    }

    [Fact]
    public void Click_WithSnapToGrid_SnapsThePosition()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;
        var spacing = scene.State.Snapping.GridSpacing;

        Click(scene.Interaction, spacing + 1, spacing - 1);

        Assert.Equal((spacing, spacing), (scene.State.TextEdit!.Text.X, scene.State.TextEdit.Text.Y));
    }

    [Fact]
    public void NewText_UsesTheRememberedFormatAndTheCurrentFillWithoutAStroke()
    {
        var scene = Create();
        scene.State.RememberTextFormat(new TextFormatChange(FontFamily: "Serif", FontSize: 33, IsBold: true));
        var fill = Color.FromRgb(9, 8, 7);
        scene.State.DefaultAppearance = scene.State.DefaultAppearance with { Fill = fill };

        Click(scene.Interaction, 10, 10);

        var text = scene.State.TextEdit!.Text;
        Assert.Equal(("Serif", 33d, true), (text.FontFamily, text.FontSize, text.IsBold));
        Assert.Equal(fill, text.Appearance.Fill);
        Assert.Null(text.Appearance.Stroke);
    }

    [Fact]
    public void Commit_NewTextWithContent_CreatesItSelectsItAndMeasuresItsBox()
    {
        var scene = Create();
        Click(scene.Interaction, 40, 50);
        var id = scene.State.TextEdit!.TextId;

        Type(scene, "Hello");

        var text = Assert.Single(Texts(scene.State));
        Assert.Equal(id, text.Id);
        Assert.Equal("Hello", text.Text);
        Assert.Equal(new FakeTextMeasurer().Layout(text).Width, text.Width);
        Assert.True(text.Height > 0);
        Assert.Equal([id], scene.State.SelectedShapeIds.ToArray());
        Assert.Null(scene.State.TextEdit);
    }

    [Fact]
    public void Commit_NewTextThatStaysEmpty_LeavesNothingBehind()
    {
        var scene = Create();
        var document = scene.State.Document;
        Click(scene.Interaction, 40, 50);

        scene.Interaction.CommitTextEdit();

        Assert.Same(document, scene.State.Document);
        Assert.False(scene.State.CanUndo);
        Assert.Empty(scene.State.SelectedShapeIds);
    }

    [Fact]
    public void Commit_NewText_IsOneUndoStepThatRemovesTheWholeText()
    {
        var scene = Create();
        Click(scene.Interaction, 40, 50);
        Type(scene, "Hello");

        scene.State.Undo();

        Assert.Empty(Texts(scene.State));
        Assert.False(scene.State.CanUndo);
    }

    [Fact]
    public void Commit_AreaText_KeepsTheDraggedBox()
    {
        var scene = Create();
        Drag(scene.Interaction, 40, 50, 140, 90);

        Type(scene, "Hello world, this wraps");

        var text = Assert.Single(Texts(scene.State));
        Assert.Equal((40d, 50d, 100d, 40d), (text.X, text.Y, text.Width, text.Height));
    }

    [Fact]
    public void Press_OnAText_EditsItAndSelectsIt()
    {
        var existing = Text();
        var scene = Create(existing);

        Click(scene.Interaction, 110, 105);

        var session = scene.State.TextEdit!;
        Assert.False(session.IsNew);
        Assert.Equal(existing.Id, session.TextId);
        Assert.Equal([existing.Id], scene.State.SelectedShapeIds.ToArray());
        Assert.Single(Texts(scene.State));
    }

    [Fact]
    public void Commit_ExistingTextWithOtherContent_ReplacesItAsOneStepAndKeepsTheFormat()
    {
        var existing = Text() with { FontSize = 20, IsBold = true };
        var scene = Create(existing);
        Click(scene.Interaction, 110, 105);

        Type(scene, "Changed text");

        var text = Assert.Single(Texts(scene.State));
        Assert.Equal("Changed text", text.Text);
        Assert.Equal((20d, true), (text.FontSize, text.IsBold));
        scene.State.Undo();
        Assert.Equal("Hello", Assert.Single(Texts(scene.State)).Text);
    }

    [Fact]
    public void Commit_ExistingTextUnchanged_MakesNoStep()
    {
        var scene = Create(Text());
        var document = scene.State.Document;
        Click(scene.Interaction, 110, 105);

        scene.Interaction.CommitTextEdit();

        Assert.Same(document, scene.State.Document);
    }

    [Fact]
    public void Commit_ExistingTextErased_RemovesIt()
    {
        var scene = Create(Text());
        Click(scene.Interaction, 110, 105);

        Type(scene, string.Empty);

        Assert.Empty(Texts(scene.State));
        Assert.Empty(scene.State.SelectedShapeIds);
    }

    [Fact]
    public void Press_OnALockedText_DoesNothing()
    {
        var scene = Create(Text() with { IsLocked = true });

        Click(scene.Interaction, 110, 105);

        Assert.Null(scene.State.TextEdit);
        Assert.Single(Texts(scene.State));
    }

    [Fact]
    public void Press_OnAHiddenText_StartsANewTextInstead()
    {
        var scene = Create(Text() with { IsVisible = false });

        Click(scene.Interaction, 110, 105);

        Assert.True(scene.State.TextEdit!.IsNew);
    }

    [Fact]
    public void Press_OnAShape_StartsANewTextOnTopOfIt()
    {
        var scene = Create(new RectangleShape { X = 0, Y = 0, Width = 200, Height = 200 });

        Click(scene.Interaction, 50, 50);

        Assert.True(scene.State.TextEdit!.IsNew);
    }

    [Fact]
    public void Press_InALockedLayer_CreatesNothing()
    {
        var scene = Create();
        scene.State.Edit(Core.History.HistoryStepType.LockLayer, document => document.UpdateLayer(scene.State.ActiveLayerId, layer => layer with { IsLocked = true }));

        Click(scene.Interaction, 50, 50);

        Assert.Null(scene.State.TextEdit);
    }

    [Fact]
    public void Press_WhileEditing_CommitsTheTextAndStartsAnother()
    {
        var scene = Create();
        Click(scene.Interaction, 40, 50);
        scene.State.TextEdit!.SetContent("First");

        Click(scene.Interaction, 300, 300);

        Assert.Equal("First", Assert.Single(Texts(scene.State)).Text);
        Assert.True(scene.State.TextEdit!.IsNew);
        Assert.Equal((300d, 300d), (scene.State.TextEdit.Text.X, scene.State.TextEdit.Text.Y));
    }

    [Fact]
    public void Press_WithTheSelectToolWhileEditing_CommitsTheText()
    {
        var scene = Create();
        Click(scene.Interaction, 40, 50);
        scene.State.TextEdit!.SetContent("First");
        scene.State.ActiveTool = Tool.Select;

        Assert.Null(scene.State.TextEdit);
        Assert.Equal("First", Assert.Single(Texts(scene.State)).Text);
    }

    [Fact]
    public void ChoosingAnotherTool_WhileEditing_CommitsTheText()
    {
        var scene = Create();
        Click(scene.Interaction, 40, 50);
        scene.State.TextEdit!.SetContent("Hello");

        scene.State.ActiveTool = Tool.Rectangle;

        Assert.Null(scene.State.TextEdit);
        Assert.Single(Texts(scene.State));
    }

    [Fact]
    public void Undo_WhileEditingAnExistingText_EndsTheEditingWhenTheTextIsGone()
    {
        var scene = Create();
        Click(scene.Interaction, 40, 50);
        Type(scene, "Hello");
        Click(scene.Interaction, 45, 55);
        Assert.NotNull(scene.State.TextEdit);

        scene.State.Undo();

        Assert.Null(scene.State.TextEdit);
    }

    [Fact]
    public void ReplaceDocument_WhileEditing_DropsTheEditing()
    {
        var scene = Create();
        Click(scene.Interaction, 40, 50);

        scene.State.ReplaceDocument(Document.Create("New"));

        Assert.Null(scene.State.TextEdit);
    }

    [Fact]
    public void CommitTextEdit_WithoutEditing_DoesNothing()
    {
        var scene = Create();
        var document = scene.State.Document;

        scene.Interaction.CommitTextEdit();

        Assert.Same(document, scene.State.Document);
    }

    [Fact]
    public void CanvasArea_HiddenShapeId_IsTheTextThatIsEdited()
    {
        var existing = Text();
        var scene = Create(existing);
        var canvas = new CanvasAreaViewModel(scene.State);

        Click(canvas, 110, 105);

        Assert.Equal(existing.Id, canvas.HiddenShapeId);
        scene.State.TextEdit = null;
        Assert.Null(canvas.HiddenShapeId);
    }

    [Fact]
    public void CanvasArea_HiddenShapeId_IsNullForANewText()
    {
        var scene = Create();
        var canvas = new CanvasAreaViewModel(scene.State);

        Click(canvas, 10, 10);

        Assert.NotNull(scene.State.TextEdit);
        Assert.Null(canvas.HiddenShapeId);
    }

    [Fact]
    public void DoubleClick_WithTheSelectToolOnAText_StartsEditingIt()
    {
        var existing = Text();
        var scene = Create(existing);
        scene.State.ActiveTool = Tool.Select;

        scene.Interaction.DoubleClicked(110, 105);

        Assert.Equal(existing.Id, scene.State.TextEdit!.TextId);
        Assert.False(scene.State.TextEdit.IsNew);
        Assert.Equal([existing.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void DoubleClick_WithTheSelectToolOnAText_ThenTyping_ReplacesTheContent()
    {
        var scene = Create(Text());
        scene.State.ActiveTool = Tool.Select;
        scene.Interaction.DoubleClicked(110, 105);

        Type(scene, "Edited");

        Assert.Equal("Edited", Assert.Single(Texts(scene.State)).Text);
        Assert.Equal(Tool.Select, scene.State.ActiveTool);
    }

    [Fact]
    public void DoubleClick_WithTheSelectToolOnARectangle_DoesNotStartEditing()
    {
        var scene = Create(new RectangleShape { X = 0, Y = 0, Width = 200, Height = 200 });
        scene.State.ActiveTool = Tool.Select;

        scene.Interaction.DoubleClicked(50, 50);

        Assert.Null(scene.State.TextEdit);
    }

    [Fact]
    public void DoubleClick_WithTheSelectToolOnALockedText_DoesNotStartEditing()
    {
        var scene = Create(Text() with { IsLocked = true });
        scene.State.ActiveTool = Tool.Select;

        scene.Interaction.DoubleClicked(110, 105);

        Assert.Null(scene.State.TextEdit);
    }

    [Fact]
    public void DoubleClick_OnATextInsideAGroup_EntersTheGroupFirstAndEditsOnTheNextDoubleClick()
    {
        var inGroup = Text();
        var other = new RectangleShape { X = 300, Y = 300, Width = 20, Height = 20 };
        var scene = Create(new GroupShape { Children = [inGroup, other] });
        scene.State.ActiveTool = Tool.Select;

        scene.Interaction.DoubleClicked(110, 105);
        Assert.Null(scene.State.TextEdit);
        Assert.NotNull(scene.State.EnteredGroupId);

        scene.Interaction.DoubleClicked(110, 105);
        Assert.Equal(inGroup.Id, scene.State.TextEdit!.TextId);
    }

    [Fact]
    public void DoubleClick_OnAnotherTextWhileEditing_CommitsTheFirstAndEditsTheSecond()
    {
        var first = Text(100, 100, "First");
        var second = Text(300, 300, "Second");
        var scene = Create(first, second);
        scene.State.ActiveTool = Tool.Select;
        scene.Interaction.DoubleClicked(110, 105);
        scene.State.TextEdit!.SetContent("First changed");

        scene.Interaction.DoubleClicked(310, 305);

        Assert.Equal(second.Id, scene.State.TextEdit!.TextId);
        Assert.Equal("First changed", Texts(scene.State).Single(text => text.Id == first.Id).Text);
    }
}
