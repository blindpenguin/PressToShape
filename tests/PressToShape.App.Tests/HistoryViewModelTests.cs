using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class HistoryViewModelTests
{
    private static (EditorState State, HistoryViewModel History) Create(PressToShape.App.Localization.Localizer? localizer = null)
    {
        var state = new EditorState(Document.Create("L"));
        return (state, new HistoryViewModel(state, localizer ?? FakeLocalizer.Create()));
    }

    private static void Draw(EditorState state, HistoryStepType type = HistoryStepType.DrawRectangle) =>
        state.Edit(type, d => d.AddShape(state.ActiveLayerId, new RectangleShape { Width = 5, Height = 5 }));

    private static string[] Names(HistoryViewModel history) => history.Items.Select(i => i.Name).ToArray();

    [Fact]
    public void Initially_OnlyTheStartRowExistsAndItIsCurrent()
    {
        var (_, history) = Create();

        var row = Assert.Single(history.Items);
        Assert.Equal("Start", row.Name);
        Assert.True(row.IsCurrent);
        Assert.False(row.IsRedoable);
        Assert.Same(row, history.SelectedItem);
        Assert.Null(row.Type);
        Assert.Equal(0, row.Index);
    }

    [Fact]
    public void Rows_AfterEdits_ListTheStepsOldestFirstWithNames()
    {
        var (state, history) = Create();

        Draw(state, HistoryStepType.DrawRectangle);
        state.Edit(HistoryStepType.DrawEllipse, d => d.AddShape(state.ActiveLayerId, new EllipseShape { Width = 2, Height = 2 }));
        state.UpdateLayer(state.ActiveLayerId, l => l with { IsVisible = false }, HistoryStepType.HideLayer);

        Assert.Equal(new[] { "Start", "Draw rectangle", "Draw ellipse", "Hide layer" }, Names(history));
        Assert.Equal(new[] { 0, 1, 2, 3 }, history.Items.Select(i => i.Index));
        Assert.Equal(new[] { false, false, false, true }, history.Items.Select(i => i.IsCurrent));
        Assert.Same(history.Items[3], history.SelectedItem);
    }

    [Fact]
    public void EveryStepType_HasAnEnglishName()
    {
        var (state, history) = Create();
        foreach (var type in Enum.GetValues<HistoryStepType>())
        {
            Draw(state, type);
        }

        var expected = new[]
        {
            "Start", "Draw rectangle", "Draw ellipse", "Move shape", "Delete shape", "Hide layer", "Show layer", "Lock layer", "Unlock layer",
            "Resize shapes", "Rotate shapes", "Nudge shapes", "Paste shapes", "Cut shapes", "Duplicate shapes", "Reset transform",
            "Change position", "Change size", "Change rotation", "Change fill", "Change stroke", "Change stroke width", "Change opacity", "Add swatch", "Remove swatch",
            "Group shapes", "Ungroup shapes", "Bring to front", "Bring forward", "Send backward", "Send to back", "Reorder shape",
            "Lock shapes", "Unlock shapes", "Hide shapes", "Show shapes", "Unlock all", "Show all", "Rename shape",
            "Draw path", "Add path point", "Delete path point", "Change path",
            "Move path points", "Move handle", "Change node type", "Bend segment", "Change segment", "Delete path points",
        };
        Assert.Equal(expected, Names(history));
        Assert.Equal(Enum.GetValues<HistoryStepType>().Length + 1, expected.Length);
    }

    [Fact]
    public void Undo_MarksTheCurrentRowAndTheRedoableRowsInPlace()
    {
        var (state, history) = Create();
        Draw(state);
        Draw(state);
        Draw(state);
        var rows = history.Items.ToList();

        state.Undo();
        state.Undo();

        Assert.True(rows.SequenceEqual(history.Items));
        Assert.Equal(new[] { false, true, false, false }, history.Items.Select(i => i.IsCurrent));
        Assert.Equal(new[] { false, false, true, true }, history.Items.Select(i => i.IsRedoable));
        Assert.Same(history.Items[1], history.SelectedItem);

        state.Redo();
        Assert.Equal(new[] { false, false, true, false }, history.Items.Select(i => i.IsCurrent));
        Assert.Same(history.Items[2], history.SelectedItem);
    }

    [Fact]
    public void Selecting_ARow_JumpsToItsState()
    {
        var (state, history) = Create();
        var start = state.Document;
        Draw(state);
        var afterFirst = state.Document;
        Draw(state);

        history.SelectedItem = history.Items[1];
        Assert.Same(afterFirst, state.Document);
        Assert.Same(history.Items[1], history.SelectedItem);
        Assert.True(history.Items[1].IsCurrent);

        history.SelectedItem = history.Items[0];
        Assert.Same(start, state.Document);

        history.SelectedItem = history.Items[2];
        Assert.Equal(2, state.Document.Layers[0].Shapes.Length);
    }

    [Fact]
    public void Selecting_TheCurrentRow_ChangesNothing()
    {
        var (state, history) = Create();
        Draw(state);
        var document = state.Document;
        var raised = 0;
        state.HistoryChanged += (_, _) => raised++;

        history.SelectedItem = history.Items[1];

        Assert.Same(document, state.Document);
        Assert.Equal(0, raised);
    }

    [Fact]
    public void Selecting_Null_IsIgnored()
    {
        var (state, history) = Create();
        Draw(state);

        history.SelectedItem = null;

        Assert.Same(history.Items[1], history.SelectedItem);
        Assert.Single(state.History.Steps);
    }

    [Fact]
    public void Selecting_WhileADragIsInProgress_IsRefusedAndTheSelectionGoesBack()
    {
        var (state, history) = Create();
        Draw(state);
        Draw(state);
        state.IsDragInProgress = true;
        var document = state.Document;
        var raised = new List<string?>();
        history.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        history.SelectedItem = history.Items[0];

        Assert.Same(document, state.Document);
        Assert.Same(history.Items[2], history.SelectedItem);
        Assert.Contains(nameof(HistoryViewModel.SelectedItem), raised);
    }

    [Fact]
    public void AnEditAfterUndo_RebuildsWithoutTheDroppedSteps()
    {
        var (state, history) = Create();
        Draw(state);
        Draw(state);
        Draw(state);
        state.Undo();
        state.Undo();
        var oldRows = history.Items.ToList();

        state.Edit(HistoryStepType.MoveShape, d => d.AddShape(state.ActiveLayerId, new RectangleShape { Width = 1, Height = 1 }));

        Assert.Equal(new[] { "Start", "Draw rectangle", "Move shape" }, Names(history));
        Assert.False(oldRows.SequenceEqual(history.Items));
        Assert.Same(history.Items[2], history.SelectedItem);
        Assert.DoesNotContain(history.Items, i => i.IsRedoable);
    }

    [Fact]
    public void ReplaceDocument_NewOrOpen_LeavesOnlyTheStartRow()
    {
        var (state, history) = Create();
        Draw(state);
        Draw(state);

        state.ReplaceDocument(Document.Create("Other"));

        var row = Assert.Single(history.Items);
        Assert.Equal("Start", row.Name);
        Assert.True(row.IsCurrent);
        Assert.Same(row, history.SelectedItem);
    }

    [Fact]
    public void MoreThanTheLimit_KeepsAtMostOneHundredOneRows()
    {
        var (state, history) = Create();

        for (var i = 0; i < 130; i++)
        {
            Draw(state);
        }

        Assert.Equal(101, history.Items.Count);
        Assert.Equal("Start", history.Items[0].Name);
        Assert.True(history.Items[^1].IsCurrent);
        Assert.Equal(Enumerable.Range(0, 101), history.Items.Select(i => i.Index));
    }

    [Fact]
    public void Names_AreTranslatedAndFollowALanguageSwitch()
    {
        var german = new Dictionary<string, string> { ["Start"] = "Anfang", ["Draw rectangle"] = "Rechteck zeichnen" };
        var localizer = FakeLocalizer.Create("en", german);
        var (state, history) = Create(localizer);
        Draw(state);
        var raised = new List<string?>();
        history.Items[1].PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        localizer.SetLanguage("de");

        Assert.Equal(new[] { "Anfang", "Rechteck zeichnen" }, Names(history));
        Assert.Contains(string.Empty, raised);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var state = new EditorState();

        Assert.Throws<ArgumentNullException>(() => new HistoryViewModel(null!, FakeLocalizer.Create()));
        Assert.Throws<ArgumentNullException>(() => new HistoryViewModel(state, null!));
    }
}
