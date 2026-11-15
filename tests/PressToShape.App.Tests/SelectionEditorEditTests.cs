using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionEditorEditTests
{
    private static (EditorState State, SelectionEditor Editor, Guid[] Ids) Create(int count = 2)
    {
        var state = new EditorState();
        var ids = TestSelection.AddAndSelect(state, count);
        return (state, new SelectionEditor(state), ids);
    }

    private static double X(EditorState state, Guid id) => state.Document.FindShape(id)!.Shape.X;

    [Fact]
    public void Nudge_MovesAllSelectedShapes_AsOneStep()
    {
        var (state, editor, ids) = Create();
        var steps = state.History.Steps.Count;

        editor.Nudge(1, 0);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.NudgeShapes, state.History.Steps[^1]);
        Assert.Equal(new[] { 1.0, 21.0 }, ids.Select(id => X(state, id)));
        Assert.Equal(ids, state.SelectedShapeIds);
    }

    [Fact]
    public void Nudge_Repeated_MergesIntoOneStep_AndUndoRestoresTheStart()
    {
        var (state, editor, ids) = Create(1);
        var steps = state.History.Steps.Count;

        editor.Nudge(1, 0);
        editor.Nudge(1, 0);
        editor.Nudge(10, 0);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(12, X(state, ids[0]));
        state.Undo();
        Assert.Equal(0, X(state, ids[0]));
    }

    [Fact]
    public void Nudge_AfterAnotherEdit_StartsANewStep()
    {
        var (state, editor, ids) = Create(1);
        editor.Nudge(1, 0);
        editor.Apply(HistoryStepType.ChangeOpacity, s => s with { Appearance = s.Appearance with { Opacity = 0.5 } });

        editor.Nudge(1, 0);

        Assert.Equal(new[] { HistoryStepType.NudgeShapes, HistoryStepType.ChangeOpacity, HistoryStepType.NudgeShapes }, state.History.Steps.TakeLast(3));
    }

    [Fact]
    public void Nudge_WithAnotherSelection_StartsANewStep()
    {
        var (state, editor, ids) = Create(2);
        state.SetSelection([ids[0]]);
        editor.Nudge(1, 0);

        state.SetSelection([ids[1]]);
        editor.Nudge(1, 0);

        Assert.Equal(new[] { HistoryStepType.NudgeShapes, HistoryStepType.NudgeShapes }, state.History.Steps.TakeLast(2));
        Assert.Equal(1, X(state, ids[0]));
        Assert.Equal(21, X(state, ids[1]));
    }

    [Fact]
    public void Nudge_AfterUndo_StartsANewStep()
    {
        var (state, editor, ids) = Create(1);
        editor.Nudge(1, 0);
        state.Undo();

        editor.Nudge(1, 0);

        Assert.Equal(1, X(state, ids[0]));
        Assert.Single(state.History.Steps, t => t == HistoryStepType.NudgeShapes);
        Assert.False(state.CanRedo);
    }

    [Fact]
    public void Nudge_WithoutSelectionOrDistance_DoesNothing()
    {
        var (state, editor, ids) = Create(1);
        var document = state.Document;

        editor.Nudge(0, 0);
        state.ClearSelection();
        editor.Nudge(5, 5);

        Assert.Same(document, state.Document);
        Assert.Equal(0, X(state, ids[0]));
    }

    [Fact]
    public void Nudge_CanLeaveTheArtboard()
    {
        var (state, editor, ids) = Create(1);

        editor.Nudge(-5000, 0);

        Assert.Equal(-5000, X(state, ids[0]));
    }

    [Fact]
    public void Apply_ChangesAllSelectedShapes_AndKeepsTheSelection()
    {
        var (state, editor, ids) = Create(3);
        state.SetSelection([ids[0], ids[2]]);

        editor.Apply(HistoryStepType.ChangeSize, s => s with { Width = 77 });

        Assert.Equal(new[] { 77.0, 10, 77 }, ids.Select(id => state.Document.FindShape(id)!.Shape.Width));
        Assert.Equal(HistoryStepType.ChangeSize, state.History.Steps[^1]);
        Assert.Equal(new[] { ids[0], ids[2] }, state.SelectedShapeIds);
    }

    [Fact]
    public void Apply_UnchangedResult_RecordsNoStep()
    {
        var (state, editor, _) = Create();
        var steps = state.History.Steps.Count;
        var document = state.Document;

        editor.Apply(HistoryStepType.ChangeSize, s => s);
        editor.Apply(HistoryStepType.ChangeSize, s => s with { Width = s.Width });

        Assert.Equal(steps, state.History.Steps.Count);
        Assert.Same(document, state.Document);
    }

    [Fact]
    public void ApplyTransform_ReplacesShapesTogether_AndKeepsIds()
    {
        var (state, editor, ids) = Create(2);

        editor.ApplyTransform(HistoryStepType.ResizeShapes, shapes => shapes.Select(s => s with { X = s.X * 2 }).ToList());

        Assert.Equal(new[] { 0.0, 40 }, ids.Select(id => X(state, id)));
        Assert.Equal(ids, state.SelectedShapeIds);
        Assert.Equal(HistoryStepType.ResizeShapes, state.History.Steps[^1]);
    }

    [Fact]
    public void ApplyTransform_Unchanged_RecordsNoStep_AndWithoutSelectionDoesNothing()
    {
        var (state, editor, _) = Create(2);
        var steps = state.History.Steps.Count;

        editor.ApplyTransform(HistoryStepType.ResizeShapes, shapes => shapes);
        state.ClearSelection();
        editor.ApplyTransform(HistoryStepType.ResizeShapes, _ => throw new InvalidOperationException("not called"));

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void ApplyTransform_WrongShapes_Throws()
    {
        var (_, editor, _) = Create(2);

        Assert.Throws<InvalidOperationException>(() => editor.ApplyTransform(HistoryStepType.ResizeShapes, shapes => shapes.Take(1).ToList()));
    }

    [Fact]
    public void NullDelegates_Throw()
    {
        var (_, editor, _) = Create();

        Assert.Throws<ArgumentNullException>(() => editor.Apply(HistoryStepType.ChangeFill, null!));
        Assert.Throws<ArgumentNullException>(() => editor.ApplyTransform(HistoryStepType.ChangeFill, null!));
    }
}
