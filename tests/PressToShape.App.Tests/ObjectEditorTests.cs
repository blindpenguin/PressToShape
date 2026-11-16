using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class ObjectEditorTests
{
    private static readonly Guid NewGroup = new("99999999-9999-9999-9999-999999999999");

    private static RectangleShape Rect(string name, double x = 0) => new() { Name = name, X = x, Width = 10, Height = 10 };

    private static GroupShape Group(string name, params Shape[] children) => new() { Name = name, Children = [.. children] };

    private static (EditorState State, ObjectEditor Editor) Create(Shape[] shapes, params Guid[] selected)
    {
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
        state.SetSelection(selected);
        return (state, new ObjectEditor(state, () => NewGroup));
    }

    private static string Order(EditorState state) => string.Concat(state.Document.Layers.SelectMany(l => l.Shapes).Select(s => s.Name));

    private static Guid[] Selected(EditorState state) => state.SelectedShapeIds.ToArray();

    [Fact]
    public void Group_TwoShapes_IsOneStepAndSelectsTheGroup()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var (state, editor) = Create([a, b, Rect("c", 40)], a.Id, b.Id);
        var steps = state.History.Steps.Count;

        editor.Group();

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.GroupShapes, state.History.Steps[^1]);
        Assert.Equal([NewGroup], Selected(state));
        Assert.IsType<GroupShape>(state.Document.FindShape(NewGroup)!.Shape);
    }

    [Fact]
    public void Group_UndoAndRedo_RestoreDocumentAndSelection()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var (state, editor) = Create([a, b], a.Id, b.Id);
        var before = state.Document;

        editor.Group();
        state.Undo();

        Assert.Same(before, state.Document);
        Assert.Equal([a.Id, b.Id], Selected(state));

        state.Redo();

        Assert.Equal([NewGroup], Selected(state));
    }

    [Fact]
    public void CanGroup_OneShapeNothingOrDifferentParents_IsFalse()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var c = Rect("c", 40);
        var inGroup = Rect("d", 60);
        var (state, editor) = Create([a, b, c, Group("G", inGroup)], a.Id);

        Assert.False(editor.CanGroup);

        state.ClearSelection();
        Assert.False(editor.CanGroup);

        state.SetSelection([a.Id, b.Id]);
        Assert.True(editor.CanGroup);
    }

    [Fact]
    public void CanGroup_ShapesOfDifferentLayers_IsFalse()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var document = Document.Create("One");
        document = document.AddShapes(document.Layers[0].Id, [a]);
        document = document.AddLayer(new Layer { Name = "Two", Shapes = [b] });
        var state = new EditorState(document);
        state.SetSelection([a.Id, b.Id]);
        var editor = new ObjectEditor(state);

        Assert.False(editor.CanGroup);

        editor.Group();

        Assert.Same(document, state.Document);
    }

    [Fact]
    public void Ungroup_Group_IsOneStepAndSelectsTheMembers()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var group = Group("G", a, b);
        var plain = Rect("c", 40);
        var (state, editor) = Create([group, plain], group.Id);
        var steps = state.History.Steps.Count;

        editor.Ungroup();

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.UngroupShapes, state.History.Steps[^1]);
        Assert.Equal("abc", Order(state));
        Assert.Equal([a.Id, b.Id], Selected(state));
    }

    [Fact]
    public void Ungroup_GroupAndPlainShape_KeepsThePlainShapeSelected()
    {
        var a = Rect("a");
        var group = Group("G", a);
        var plain = Rect("c", 40);
        var (state, editor) = Create([group, plain], group.Id, plain.Id);

        editor.Ungroup();

        Assert.Equal([a.Id, plain.Id], Selected(state));
    }

    [Fact]
    public void Ungroup_UndoRestoresTheGroupSelection()
    {
        var group = Group("G", Rect("a"), Rect("b", 20));
        var (state, editor) = Create([group], group.Id);

        editor.Ungroup();
        state.Undo();

        Assert.Equal([group.Id], Selected(state));
    }

    [Fact]
    public void CanUngroup_NoGroupSelected_IsFalseAndNothingHappens()
    {
        var a = Rect("a");
        var (state, editor) = Create([a], a.Id);
        var steps = state.History.Steps.Count;

        editor.Ungroup();

        Assert.False(editor.CanUngroup);
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Theory]
    [InlineData(ArrangeType.ToFront, HistoryStepType.BringToFront, "bcda")]
    [InlineData(ArrangeType.Forward, HistoryStepType.BringForward, "bacd")]
    public void Arrange_BottomShape_IsOneStepWithTheRightTypeAndKeepsTheSelection(ArrangeType type, HistoryStepType step, string expected)
    {
        var a = Rect("a");
        var (state, editor) = Create([a, Rect("b", 10), Rect("c", 20), Rect("d", 30)], a.Id);
        var steps = state.History.Steps.Count;

        Assert.True(editor.CanArrange(type));
        editor.Arrange(type);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(step, state.History.Steps[^1]);
        Assert.Equal(expected, Order(state));
        Assert.Equal([a.Id], Selected(state));
    }

    [Theory]
    [InlineData(ArrangeType.ToBack, HistoryStepType.SendToBack, "dabc")]
    [InlineData(ArrangeType.Backward, HistoryStepType.SendBackward, "abdc")]
    public void Arrange_TopShape_IsOneStepWithTheRightType(ArrangeType type, HistoryStepType step, string expected)
    {
        var d = Rect("d", 30);
        var (state, editor) = Create([Rect("a"), Rect("b", 10), Rect("c", 20), d], d.Id);

        editor.Arrange(type);

        Assert.Equal(step, state.History.Steps[^1]);
        Assert.Equal(expected, Order(state));
    }

    [Fact]
    public void Arrange_NothingWouldMove_IsDisabledAndRecordsNoStep()
    {
        var d = Rect("d", 30);
        var (state, editor) = Create([Rect("a"), d], d.Id);
        var steps = state.History.Steps.Count;

        Assert.False(editor.CanArrange(ArrangeType.ToFront));
        Assert.False(editor.CanArrange(ArrangeType.Forward));
        Assert.True(editor.CanArrange(ArrangeType.ToBack));
        editor.Arrange(ArrangeType.ToFront);

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void CanArrange_NothingSelected_IsFalse()
    {
        var (_, editor) = Create([Rect("a"), Rect("b", 20)]);

        Assert.False(editor.CanArrange(ArrangeType.ToFront));
    }

    [Fact]
    public void Arrange_InsideAnEnteredGroup_StaysInTheGroup()
    {
        var x = Rect("x");
        var group = Group("G", x, Rect("y", 20));
        var (state, editor) = Create([group, Rect("a", 50)]);
        state.EnterGroup(group.Id);
        state.SelectOnly(x.Id);

        editor.Arrange(ArrangeType.ToFront);

        Assert.Equal("Ga", Order(state));
        Assert.Equal("yx", string.Concat(((GroupShape)state.Document.Layers[0].Shapes[0]).Children.Select(c => c.Name)));
    }

    [Fact]
    public void LockSelection_LocksAndRemovesTheShapeFromTheSelection()
    {
        var a = Rect("a");
        var (state, editor) = Create([a, Rect("b", 20)], a.Id);

        Assert.True(editor.CanLockSelection);
        editor.LockSelection();

        Assert.Equal(HistoryStepType.LockShapes, state.History.Steps[^1]);
        Assert.True(state.Document.FindShape(a.Id)!.Shape.IsLocked);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void HideSelection_HidesAndRemovesTheShapeFromTheSelection()
    {
        var a = Rect("a");
        var (state, editor) = Create([a], a.Id);

        editor.HideSelection();

        Assert.Equal(HistoryStepType.HideShapes, state.History.Steps[^1]);
        Assert.False(state.Document.FindShape(a.Id)!.Shape.IsVisible);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void LockAndHideSelection_NothingSelected_AreDisabled()
    {
        var (_, editor) = Create([Rect("a")]);

        Assert.False(editor.CanLockSelection);
        Assert.False(editor.CanHideSelection);
    }

    [Fact]
    public void SetLockedAndSetHidden_ById_UnlockAndShowShapesThatCannotBeSelected()
    {
        var a = Rect("a") with { IsLocked = true, IsVisible = false };
        var (state, editor) = Create([a]);

        editor.SetLocked([a.Id], false);
        editor.SetHidden([a.Id], false);

        var shape = state.Document.FindShape(a.Id)!.Shape;
        Assert.False(shape.IsLocked);
        Assert.True(shape.IsVisible);
        Assert.Equal(HistoryStepType.UnlockShapes, state.History.Steps[^2]);
        Assert.Equal(HistoryStepType.ShowShapes, state.History.Steps[^1]);
    }

    [Fact]
    public void SetLocked_UnknownIdOrNoChange_RecordsNothing()
    {
        var a = Rect("a");
        var (state, editor) = Create([a]);
        var steps = state.History.Steps.Count;

        editor.SetLocked([Guid.NewGuid()], true);
        editor.SetLocked([a.Id], false);
        editor.SetHidden([a.Id], false);

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void ShowAllAndUnlockAll_ReachNestedObjects()
    {
        var deep = Rect("a") with { IsVisible = false, IsLocked = true };
        var group = Group("G", deep) with { IsVisible = false };
        var (state, editor) = Create([group]);

        Assert.True(editor.CanShowAll);
        Assert.True(editor.CanUnlockAll);
        editor.ShowAll();
        editor.UnlockAll();

        Assert.True(state.Document.FindShape(deep.Id)!.IsEffectivelyVisible);
        Assert.False(state.Document.FindShape(deep.Id)!.IsEffectivelyLocked);
        Assert.Equal(HistoryStepType.ShowAll, state.History.Steps[^2]);
        Assert.Equal(HistoryStepType.UnlockAll, state.History.Steps[^1]);
        Assert.False(editor.CanShowAll);
        Assert.False(editor.CanUnlockAll);
    }

    [Fact]
    public void ShowAllAndUnlockAll_NothingToChange_RecordNoStep()
    {
        var (state, editor) = Create([Rect("a")]);
        var steps = state.History.Steps.Count;

        editor.ShowAll();
        editor.UnlockAll();

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void Rename_Shape_IsOneStep_EmptyNameClears()
    {
        var a = Rect("a");
        var (state, editor) = Create([a]);

        editor.Rename(a.Id, " Sun ");
        Assert.Equal("Sun", state.Document.FindShape(a.Id)!.Shape.Name);
        Assert.Equal(HistoryStepType.RenameShape, state.History.Steps[^1]);

        editor.Rename(a.Id, "  ");
        Assert.Equal(string.Empty, state.Document.FindShape(a.Id)!.Shape.Name);
    }

    [Fact]
    public void Rename_UnknownIdOrSameName_RecordsNothing()
    {
        var a = Rect("a");
        var (state, editor) = Create([a]);
        var steps = state.History.Steps.Count;

        editor.Rename(Guid.NewGuid(), "x");
        editor.Rename(a.Id, "a");

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void MoveAbove_SameParent_IsOneReorderStep()
    {
        var a = Rect("a");
        var c = Rect("c", 40);
        var (state, editor) = Create([a, Rect("b", 20), c]);

        Assert.True(editor.CanMoveAbove(a.Id, c.Id));
        editor.MoveAbove(a.Id, c.Id);

        Assert.Equal("bca", Order(state));
        Assert.Equal(HistoryStepType.ReorderShape, state.History.Steps[^1]);
    }

    [Fact]
    public void MoveAbove_DifferentParentsOrSameShape_DoesNothing()
    {
        var a = Rect("a");
        var inside = Rect("x");
        var (state, editor) = Create([a, Group("G", inside)]);
        var steps = state.History.Steps.Count;

        Assert.False(editor.CanMoveAbove(a.Id, inside.Id));
        Assert.False(editor.CanMoveAbove(a.Id, a.Id));
        Assert.False(editor.CanMoveAbove(a.Id, Guid.NewGuid()));
        editor.MoveAbove(a.Id, inside.Id);

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void Constructor_NullState_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ObjectEditor(null!));
    }
}
