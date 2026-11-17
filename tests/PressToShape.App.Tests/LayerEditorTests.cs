using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class LayerEditorTests
{
    private static RectangleShape Rect(string name, double x = 0) => new() { Name = name, X = x, Width = 10, Height = 10 };

    private sealed record Fixture(EditorState State, LayerEditor Editor, RectangleShape A, RectangleShape B, RectangleShape C);

    // Layers "Layer 1" (bottom, with a and b), "Layer 2" (with c) and "Layer 3" (top, empty). The active layer is the top one.
    private static Fixture Create(string language = "en")
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var c = Rect("c", 40);
        var document = Document.Create("Layer 1");
        document = document.AddShapes(document.Layers[0].Id, [a, b])
            .AddLayer(new Layer { Name = "Layer 2", Shapes = [c] })
            .AddLayer(new Layer { Name = "Layer 3" });
        var state = new EditorState(document);
        var localizer = FakeLocalizer.Create(language, new Dictionary<string, string> { ["Layer {0}"] = "Ebene {0}", ["{0} copy"] = "{0} Kopie" });
        return new Fixture(state, new LayerEditor(state, localizer), a, b, c);
    }

    private static string Names(EditorState state) => string.Join(",", state.Document.Layers.Select(l => l.Name));

    private static string ActiveName(EditorState state) => state.Document.Layers.Single(l => l.Id == state.ActiveLayerId).Name;

    private static void Activate(EditorState state, int index) => state.ActiveLayerId = state.Document.Layers[index].Id;

    private static HistoryStepType LastStep(EditorState state) => state.History.Steps[^1];

    [Fact]
    public void Add_AboveActiveLayer_IsOneStepAndActivatesTheNewLayer()
    {
        var f = Create();
        Activate(f.State, 1);
        var steps = f.State.History.Steps.Count;

        f.Editor.Add();

        Assert.Equal("Layer 1,Layer 2,Layer 4,Layer 3", Names(f.State));
        Assert.Equal("Layer 4", ActiveName(f.State));
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.AddLayer, LastStep(f.State));
    }

    [Fact]
    public void Add_WithSelectionAndEnteredGroup_ClearsBoth()
    {
        var f = Create();
        f.State.SetSelection([f.A.Id]);

        f.Editor.Add();

        Assert.False(f.State.HasSelection);
        Assert.Null(f.State.EnteredGroupId);
    }

    [Fact]
    public void Add_NameInUse_TakesTheLowestFreeNumber()
    {
        var f = Create();
        f.State.Edit(HistoryStepType.RenameLayer, d => d.UpdateLayer(d.Layers[0].Id, l => l with { Name = "x" }));

        f.Editor.Add();

        Assert.Contains("Layer 1", Names(f.State).Split(','));
    }

    [Fact]
    public void Add_InGerman_UsesTheTranslatedName()
    {
        var f = Create("de");

        f.Editor.Add();

        Assert.Equal("Ebene 1", ActiveName(f.State));
    }

    [Fact]
    public void Add_UndoAndRedo_RemoveAndRestoreTheLayer()
    {
        var f = Create();

        f.Editor.Add();
        f.State.Undo();

        Assert.Equal("Layer 1,Layer 2,Layer 3", Names(f.State));
        Assert.Equal("Layer 3", ActiveName(f.State));

        f.State.Redo();

        Assert.Equal("Layer 1,Layer 2,Layer 3,Layer 4", Names(f.State));
    }

    [Fact]
    public void Delete_MiddleActiveLayer_RemovesItWithShapesAndActivatesTheLayerBelow()
    {
        var f = Create();
        Activate(f.State, 1);
        f.State.SetSelection([f.A.Id]);
        var steps = f.State.History.Steps.Count;

        f.Editor.Delete();

        Assert.Equal("Layer 1,Layer 3", Names(f.State));
        Assert.Equal("Layer 1", ActiveName(f.State));
        Assert.Null(f.State.Document.FindShape(f.C.Id));
        Assert.Equal([f.A.Id], f.State.SelectedShapeIds.ToArray());
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.DeleteLayer, LastStep(f.State));
    }

    [Fact]
    public void Delete_LayerWithSelectedShape_DropsItFromTheSelection()
    {
        var f = Create();
        Activate(f.State, 0);
        f.State.SetSelection([f.A.Id, f.B.Id]);

        f.Editor.Delete();

        Assert.False(f.State.HasSelection);
    }

    [Fact]
    public void Delete_UndoRestoresLayerShapesAndPosition()
    {
        var f = Create();
        Activate(f.State, 1);

        f.Editor.Delete();
        f.State.Undo();

        Assert.Equal("Layer 1,Layer 2,Layer 3", Names(f.State));
        Assert.NotNull(f.State.Document.FindShape(f.C.Id));
    }

    [Fact]
    public void Delete_LastLayer_IsNotPossible()
    {
        var state = new EditorState(Document.Create("Only"));
        var editor = new LayerEditor(state, FakeLocalizer.Create());
        var steps = state.History.Steps.Count;

        Assert.False(editor.CanDelete);
        editor.Delete();

        Assert.Single(state.Document.Layers);
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void Rename_NewName_IsOneStep()
    {
        var f = Create();
        var steps = f.State.History.Steps.Count;

        f.Editor.Rename(f.State.Document.Layers[1].Id, "  Sky ");

        Assert.Equal("Layer 1,Sky,Layer 3", Names(f.State));
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.RenameLayer, LastStep(f.State));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Layer 2")]
    public void Rename_EmptyBlankOrSameName_ChangesNothing(string name)
    {
        var f = Create();
        var steps = f.State.History.Steps.Count;

        f.Editor.Rename(f.State.Document.Layers[1].Id, name);

        Assert.Equal("Layer 1,Layer 2,Layer 3", Names(f.State));
        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void Rename_UnknownLayer_IsIgnored()
    {
        var f = Create();
        var steps = f.State.History.Steps.Count;

        f.Editor.Rename(Guid.NewGuid(), "x");

        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void MoveUpAndDown_ActiveLayer_SwapWithNeighbourAndKeepActive()
    {
        var f = Create();
        Activate(f.State, 0);

        f.Editor.MoveUp();

        Assert.Equal("Layer 2,Layer 1,Layer 3", Names(f.State));
        Assert.Equal("Layer 1", ActiveName(f.State));
        Assert.Equal(HistoryStepType.ReorderLayer, LastStep(f.State));

        f.Editor.MoveDown();

        Assert.Equal("Layer 1,Layer 2,Layer 3", Names(f.State));
    }

    [Fact]
    public void MoveUpAndDown_AtTheEnds_AreDisabledAndRecordNothing()
    {
        var f = Create();
        var steps = f.State.History.Steps.Count;

        Assert.False(f.Editor.CanMoveUp);
        f.Editor.MoveUp();
        Activate(f.State, 0);
        Assert.False(f.Editor.CanMoveDown);
        f.Editor.MoveDown();

        Assert.Equal(steps, f.State.History.Steps.Count);
        Assert.Equal("Layer 1,Layer 2,Layer 3", Names(f.State));
    }

    [Fact]
    public void MoveAbove_LayerOnTarget_IsOneStepAndKeepsSelection()
    {
        var f = Create();
        f.State.SetSelection([f.A.Id]);

        f.Editor.MoveAbove(f.State.Document.Layers[0].Id, f.State.Document.Layers[2].Id);

        Assert.Equal("Layer 2,Layer 3,Layer 1", Names(f.State));
        Assert.Equal([f.A.Id], f.State.SelectedShapeIds.ToArray());
        Assert.Equal(HistoryStepType.ReorderLayer, LastStep(f.State));
    }

    [Fact]
    public void MoveAbove_SameOrUnknownIds_IsIgnored()
    {
        var f = Create();
        var steps = f.State.History.Steps.Count;
        var id = f.State.Document.Layers[0].Id;

        f.Editor.MoveAbove(id, id);
        f.Editor.MoveAbove(id, Guid.NewGuid());
        f.Editor.MoveAbove(f.State.Document.Layers[1].Id, id);

        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void Duplicate_ActiveLayer_InsertsCopyAboveSelectsItAndClearsSelection()
    {
        var f = Create();
        Activate(f.State, 0);
        f.State.SetSelection([f.A.Id]);
        var steps = f.State.History.Steps.Count;

        f.Editor.Duplicate();

        Assert.Equal("Layer 1,Layer 1 copy,Layer 2,Layer 3", Names(f.State));
        Assert.Equal("Layer 1 copy", ActiveName(f.State));
        Assert.False(f.State.HasSelection);
        Assert.Equal(2, f.State.Document.Layers[1].Shapes.Length);
        Assert.DoesNotContain(f.State.Document.Layers[1].Shapes, s => s.Id == f.A.Id || s.Id == f.B.Id);
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.DuplicateLayer, LastStep(f.State));
    }

    [Fact]
    public void Duplicate_InGerman_UsesTheTranslatedName()
    {
        var f = Create("de");

        f.Editor.Duplicate();

        Assert.Equal("Layer 3 Kopie", ActiveName(f.State));
    }

    [Fact]
    public void Duplicate_Undo_RemovesTheCopyAndActivatesTheOriginal()
    {
        var f = Create();
        Activate(f.State, 1);

        f.Editor.Duplicate();
        f.State.Undo();

        Assert.Equal("Layer 1,Layer 2,Layer 3", Names(f.State));
        Assert.Equal("Layer 2", ActiveName(f.State));
    }

    [Fact]
    public void MergeDown_ActiveLayer_MovesShapesToLowerLayerAndActivatesIt()
    {
        var f = Create();
        Activate(f.State, 1);
        f.State.SetSelection([f.C.Id]);
        var steps = f.State.History.Steps.Count;

        f.Editor.MergeDown();

        Assert.Equal("Layer 1,Layer 3", Names(f.State));
        Assert.Equal("Layer 1", ActiveName(f.State));
        Assert.Equal([f.A.Id, f.B.Id, f.C.Id], f.State.Document.Layers[0].Shapes.Select(s => s.Id));
        Assert.Equal([f.C.Id], f.State.SelectedShapeIds.ToArray());
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.MergeLayers, LastStep(f.State));
    }

    [Fact]
    public void MergeDown_Undo_RestoresBothLayers()
    {
        var f = Create();
        Activate(f.State, 1);

        f.Editor.MergeDown();
        f.State.Undo();

        Assert.Equal("Layer 1,Layer 2,Layer 3", Names(f.State));
        Assert.Single(f.State.Document.Layers[1].Shapes);
    }

    [Fact]
    public void MergeDown_BottomLayer_IsDisabled()
    {
        var f = Create();
        Activate(f.State, 0);

        Assert.False(f.Editor.CanMerge);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1, false)]
    [InlineData(0, true)]
    [InlineData(0, false)]
    public void MergeDown_LockedOrHiddenLayerOfTheTwo_IsDisabled(int layerIndex, bool locked)
    {
        var f = Create();
        Activate(f.State, 1);
        var target = f.State.Document.Layers[layerIndex].Id;

        f.State.UpdateLayer(target, l => locked ? l with { IsLocked = true } : l with { IsVisible = false }, HistoryStepType.LockLayer);

        Assert.False(f.Editor.CanMerge);
    }

    [Fact]
    public void MergeDown_OtherLayerLocked_StaysEnabled()
    {
        var f = Create();
        Activate(f.State, 1);
        f.State.UpdateLayer(f.State.Document.Layers[2].Id, l => l with { IsLocked = true }, HistoryStepType.LockLayer);

        Assert.True(f.Editor.CanMerge);
    }

    [Fact]
    public void MoveSelectionToLayer_Shapes_GoToTopOfTargetAndStaySelected()
    {
        var f = Create();
        f.State.SetSelection([f.A.Id]);
        var target = f.State.Document.Layers[1].Id;
        var steps = f.State.History.Steps.Count;

        f.Editor.MoveSelectionToLayer(target);

        Assert.Equal([f.C.Id, f.A.Id], f.State.Document.Layers[1].Shapes.Select(s => s.Id));
        Assert.Equal([f.B.Id], f.State.Document.Layers[0].Shapes.Select(s => s.Id));
        Assert.Equal([f.A.Id], f.State.SelectedShapeIds.ToArray());
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.MoveToLayer, LastStep(f.State));
    }

    [Fact]
    public void MoveSelectionToLayer_Undo_BringsTheShapeBack()
    {
        var f = Create();
        f.State.SetSelection([f.A.Id]);

        f.Editor.MoveSelectionToLayer(f.State.Document.Layers[1].Id);
        f.State.Undo();

        Assert.Equal([f.A.Id, f.B.Id], f.State.Document.Layers[0].Shapes.Select(s => s.Id));
    }

    [Fact]
    public void MoveSelectionToLayer_MemberOfEnteredGroup_LeavesGroupAndEnteredState()
    {
        var d = Rect("d", 60);
        var e = Rect("e", 80);
        var group = new GroupShape { Children = [d, e] };
        var document = Document.Create("L1");
        document = document.AddShapes(document.Layers[0].Id, [group]).AddLayer(new Layer { Name = "L2" });
        var state = new EditorState(document);
        var editor = new LayerEditor(state, FakeLocalizer.Create());
        state.EnterGroup(group.Id);
        state.SetSelection([d.Id]);

        editor.MoveSelectionToLayer(document.Layers[1].Id);

        Assert.Null(state.EnteredGroupId);
        Assert.Equal([d.Id], state.Document.Layers[1].Shapes.Select(s => s.Id));
        Assert.Equal([d.Id], state.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void MoveSelectionToLayer_AlreadyOnTop_RecordsNothing()
    {
        var f = Create();
        f.State.SetSelection([f.B.Id]);
        var steps = f.State.History.Steps.Count;

        f.Editor.MoveSelectionToLayer(f.State.Document.Layers[0].Id);

        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MoveSelectionToLayer_LockedOrHiddenTarget_IsRejected(bool locked)
    {
        var f = Create();
        f.State.SetSelection([f.A.Id]);
        var target = f.State.Document.Layers[1].Id;
        f.State.UpdateLayer(target, l => locked ? l with { IsLocked = true } : l with { IsVisible = false }, HistoryStepType.LockLayer);
        var steps = f.State.History.Steps.Count;

        Assert.False(f.Editor.CanMoveSelectionTo(target));
        f.Editor.MoveSelectionToLayer(target);

        Assert.Equal(steps, f.State.History.Steps.Count);
        Assert.Contains(f.State.Document.Layers[0].Shapes, s => s.Id == f.A.Id);
    }

    [Fact]
    public void MoveSelectionToLayer_NothingSelected_IsDisabled()
    {
        var f = Create();

        Assert.False(f.Editor.CanMoveSelectionTo(f.State.Document.Layers[1].Id));
        Assert.False(f.Editor.CanMoveSelectionUp);
        Assert.False(f.Editor.CanMoveSelectionDown);
    }

    [Fact]
    public void MoveSelectionUpAndDown_SelectionInOneLayer_MovesToTheNeighbourLayer()
    {
        var f = Create();
        f.State.SetSelection([f.C.Id]);

        Assert.True(f.Editor.CanMoveSelectionUp);
        Assert.True(f.Editor.CanMoveSelectionDown);
        f.Editor.MoveSelectionUp();

        Assert.Equal([f.C.Id], f.State.Document.Layers[2].Shapes.Select(s => s.Id));

        f.Editor.MoveSelectionDown();
        f.Editor.MoveSelectionDown();

        Assert.Equal([f.A.Id, f.B.Id, f.C.Id], f.State.Document.Layers[0].Shapes.Select(s => s.Id));
        Assert.False(f.Editor.CanMoveSelectionDown);
    }

    [Fact]
    public void MoveSelectionUpAndDown_SelectionInSeveralLayers_IsDisabled()
    {
        var f = Create();
        f.State.SetSelection([f.A.Id, f.C.Id]);

        Assert.False(f.Editor.CanMoveSelectionUp);
        Assert.False(f.Editor.CanMoveSelectionDown);
    }

    [Fact]
    public void MoveSelectionUp_NeighbourLayerLocked_IsDisabled()
    {
        var f = Create();
        f.State.SetSelection([f.C.Id]);
        f.State.UpdateLayer(f.State.Document.Layers[2].Id, l => l with { IsLocked = true }, HistoryStepType.LockLayer);

        Assert.False(f.Editor.CanMoveSelectionUp);
    }
}
