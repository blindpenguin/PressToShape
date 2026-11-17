using System;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class LayersViewModelLayerTests
{
    private static RectangleShape Rect(string name, double x = 0) => new() { Name = name, X = x, Width = 10, Height = 10 };

    private sealed record Fixture(EditorState State, LayersViewModel List, RectangleShape A, RectangleShape C);

    // Layers "One" (bottom, with a), "Two" (with c) and "Three" (top, empty). The active layer is "Three".
    private static Fixture Create()
    {
        var a = Rect("a");
        var c = Rect("c", 40);
        var document = Document.Create("One");
        document = document.AddShapes(document.Layers[0].Id, [a])
            .AddLayer(new Layer { Name = "Two", Shapes = [c] })
            .AddLayer(new Layer { Name = "Three" });
        var state = new EditorState(document);
        var localizer = FakeLocalizer.Create();
        return new Fixture(state, new LayersViewModel(state, new ObjectEditor(state), localizer), a, c);
    }

    private static ObjectRowViewModel LayerRow(Fixture f, string name) => f.List.Rows.Single(r => r.IsLayer && r.Name == name);

    private static ObjectRowViewModel Row(Fixture f, Guid id) => f.List.Rows.Single(r => r.Id == id);

    private static string LayerNames(Fixture f) => string.Join(",", f.State.Document.Layers.Select(l => l.Name));

    [Fact]
    public void Commands_InThreeLayerDocumentTopLayerActive_HaveTheExpectedCanExecute()
    {
        var f = Create();

        Assert.True(f.List.AddLayerCommand.CanExecute(null));
        Assert.True(f.List.DuplicateLayerCommand.CanExecute(null));
        Assert.True(f.List.DeleteLayerCommand.CanExecute(null));
        Assert.False(f.List.MoveLayerUpCommand.CanExecute(null));
        Assert.True(f.List.MoveLayerDownCommand.CanExecute(null));
        Assert.True(f.List.MergeLayerDownCommand.CanExecute(null));
    }

    [Fact]
    public void Commands_OneLayerDocument_CannotDeleteMoveOrMerge()
    {
        var state = new EditorState(Document.Create("Only"));
        var list = new LayersViewModel(state, new ObjectEditor(state), FakeLocalizer.Create());

        Assert.False(list.DeleteLayerCommand.CanExecute(null));
        Assert.False(list.MoveLayerUpCommand.CanExecute(null));
        Assert.False(list.MoveLayerDownCommand.CanExecute(null));
        Assert.False(list.MergeLayerDownCommand.CanExecute(null));
    }

    [Fact]
    public void Commands_AfterSelectingTheBottomLayer_ChangeCanExecuteAndRaiseTheEvent()
    {
        var f = Create();
        var raised = 0;
        f.List.MoveLayerUpCommand.CanExecuteChanged += (_, _) => raised++;

        f.List.Select(LayerRow(f, "One"), extend: false);

        Assert.True(f.List.MoveLayerUpCommand.CanExecute(null));
        Assert.False(f.List.MoveLayerDownCommand.CanExecute(null));
        Assert.False(f.List.MergeLayerDownCommand.CanExecute(null));
        Assert.True(raised > 0);
    }

    [Fact]
    public void MoveLayerUpCommand_Executed_ReordersTheLayerRows()
    {
        var f = Create();
        f.List.Select(LayerRow(f, "One"), extend: false);

        f.List.MoveLayerUpCommand.Execute(null);

        Assert.Equal("Two,One,Three", LayerNames(f));
        Assert.Equal(["Three", "One", "Two"], f.List.Rows.Where(r => r.IsLayer).Select(r => r.Name));
        Assert.Equal(HistoryStepType.ReorderLayer, f.State.History.Steps[^1]);
    }

    [Fact]
    public void Undo_AfterLayerCommands_RowsFollowTheDocument()
    {
        var f = Create();
        f.List.DuplicateLayerCommand.Execute(null);
        f.List.MergeLayerDownCommand.Execute(null);

        f.State.Undo();
        f.State.Undo();

        Assert.Equal(["Three", "Two", "One"], f.List.Rows.Where(r => r.IsLayer).Select(r => r.Name));
    }

    [Fact]
    public void DeleteLayerCommand_Executed_RemovesTheLayerRowAndItsObjects()
    {
        var f = Create();
        f.List.Select(LayerRow(f, "Two"), extend: false);

        f.List.DeleteLayerCommand.Execute(null);

        Assert.Equal("One,Three", LayerNames(f));
        Assert.DoesNotContain(f.List.Rows, r => r.Id == f.C.Id);
    }

    [Fact]
    public void LayerRow_CanBeRenamedThroughTheRowCommands()
    {
        var f = Create();
        var row = LayerRow(f, "Two");

        row.BeginRenameCommand.Execute(null);
        Assert.Equal("Two", row.EditText);
        row.EditText = "Sky";
        row.CommitRenameCommand.Execute(null);

        Assert.Equal("One,Sky,Three", LayerNames(f));
        Assert.Equal(HistoryStepType.RenameLayer, f.State.History.Steps[^1]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Two")]
    public void LayerRow_RenameWithEmptyOrSameName_KeepsTheName(string text)
    {
        var f = Create();
        var row = LayerRow(f, "Two");
        var steps = f.State.History.Steps.Count;

        row.BeginRenameCommand.Execute(null);
        row.EditText = text;
        row.CommitRenameCommand.Execute(null);

        Assert.Equal("One,Two,Three", LayerNames(f));
        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void LayerRow_RenameCancelled_KeepsTheName()
    {
        var f = Create();
        var row = LayerRow(f, "Two");

        row.BeginRenameCommand.Execute(null);
        row.EditText = "Sky";
        row.CancelRenameCommand.Execute(null);

        Assert.Equal("One,Two,Three", LayerNames(f));
        Assert.False(row.IsRenaming);
    }

    [Fact]
    public void Reorder_LayerOnLayer_PutsItDirectlyAboveTheTarget()
    {
        var f = Create();

        Assert.True(f.List.CanReorder(LayerRow(f, "One"), LayerRow(f, "Three")));
        f.List.Reorder(LayerRow(f, "One"), LayerRow(f, "Three"));

        Assert.Equal("Two,Three,One", LayerNames(f));
    }

    [Fact]
    public void Reorder_LayerOnItselfOrOnAnObject_IsRefused()
    {
        var f = Create();

        Assert.False(f.List.CanReorder(LayerRow(f, "One"), LayerRow(f, "One")));
        Assert.False(f.List.CanReorder(LayerRow(f, "One"), Row(f, f.C.Id)));
    }

    [Fact]
    public void Reorder_ObjectOnLayerRow_MovesOnlyThatObjectWhenItIsNotSelected()
    {
        var f = Create();

        Assert.True(f.List.CanReorder(Row(f, f.A.Id), LayerRow(f, "Two")));
        f.List.Reorder(Row(f, f.A.Id), LayerRow(f, "Two"));

        Assert.Equal([f.C.Id, f.A.Id], f.State.Document.Layers[1].Shapes.Select(s => s.Id));
        Assert.Equal(HistoryStepType.MoveToLayer, f.State.History.Steps[^1]);
    }

    [Fact]
    public void Reorder_ObjectOfTheSelectionOnLayerRow_MovesTheWholeSelection()
    {
        var f = Create();
        f.State.SetSelection([f.A.Id, f.C.Id]);

        f.List.Reorder(Row(f, f.A.Id), LayerRow(f, "Three"));

        Assert.Equal([f.A.Id, f.C.Id], f.State.Document.Layers[2].Shapes.Select(s => s.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Reorder_ObjectOnLockedOrHiddenLayer_IsRefused(bool locked)
    {
        var f = Create();
        var target = f.State.Document.Layers[1].Id;
        f.State.UpdateLayer(target, l => locked ? l with { IsLocked = true } : l with { IsVisible = false }, HistoryStepType.LockLayer);
        var steps = f.State.History.Steps.Count;

        Assert.False(f.List.CanReorder(Row(f, f.A.Id), LayerRow(f, "Two")));
        f.List.Reorder(Row(f, f.A.Id), LayerRow(f, "Two"));

        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void Reorder_LockedObjectOnLayerRow_IsRefused()
    {
        var f = Create();
        f.State.Edit(HistoryStepType.LockShapes, d => d.SetLocked(new System.Collections.Generic.HashSet<Guid> { f.A.Id }, true));

        Assert.False(f.List.CanReorder(Row(f, f.A.Id), LayerRow(f, "Two")));
    }
}
