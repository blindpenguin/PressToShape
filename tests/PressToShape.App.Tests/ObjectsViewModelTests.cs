using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class ObjectsViewModelTests
{
    private static RectangleShape Rect(string name = "", double x = 0) => new() { Name = name, X = x, Width = 10, Height = 10 };

    private static GroupShape Group(string name, params Shape[] children) => new() { Name = name, Children = [.. children] };

    private sealed record Fixture(EditorState State, ObjectEditor Editor, ObjectsViewModel List, Localizer Localizer);

    private static Fixture Create(Shape[] shapes, Localizer? localizer = null)
    {
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
        var editor = new ObjectEditor(state);
        localizer ??= FakeLocalizer.Create();
        return new Fixture(state, editor, new ObjectsViewModel(state, editor, localizer), localizer);
    }

    private static string[] Names(Fixture f) => f.List.Rows.Select(r => r.Name).ToArray();

    private static ObjectRowViewModel Row(Fixture f, Guid id) => f.List.Rows.Single(r => r.Id == id);

    [Fact]
    public void Rows_LayerThenObjectsTopmostFirst()
    {
        var f = Create([Rect("a"), Rect("b", 20), Rect("c", 40)]);

        Assert.Equal(["Layer", "c", "b", "a"], Names(f));
        Assert.Equal([0, 1, 1, 1], f.List.Rows.Select(r => r.Depth).ToArray());
        Assert.True(f.List.Rows[0].IsLayer);
        Assert.False(f.List.Rows[0].HasToggles);
        Assert.True(f.List.Rows[1].HasToggles);
    }

    [Fact]
    public void Group_AfterGrouping_IsShownClosed()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var f = Create([a, b]);
        f.State.SetSelection([a.Id, b.Id]);

        f.Editor.Group();

        Assert.Equal(["Layer", "Group"], Names(f));
        Assert.True(f.List.Rows[1].IsSelected);
        Assert.False(f.List.Rows[1].IsExpanded);
    }

    [Fact]
    public void Rows_Group_IsCollapsedAtFirstAndOpensToShowItsChildren()
    {
        var x = Rect("x");
        var group = Group("G", x, Rect("y", 20));
        var f = Create([group, Rect("top", 100)]);

        Assert.Equal(["Layer", "top", "G"], Names(f));
        Assert.False(Row(f, group.Id).IsExpanded);

        f.List.SetExpanded(Row(f, group.Id), true);

        Assert.Equal(["Layer", "top", "G", "y", "x"], Names(f));
        Assert.Equal([0, 1, 1, 2, 2], f.List.Rows.Select(r => r.Depth).ToArray());
        Assert.Equal(group.Id, Row(f, x.Id).ParentId);
    }

    [Fact]
    public void Rows_CollapsingTheLayer_HidesEverythingBelow()
    {
        var f = Create([Rect("a"), Rect("b", 20)]);

        f.List.SetExpanded(f.List.Rows[0], false);

        Assert.Equal(["Layer"], Names(f));
        f.List.SetExpanded(f.List.Rows[0], true);
        Assert.Equal(3, f.List.Rows.Count);
    }

    [Fact]
    public void Rows_NoName_ShowTheDefaultNameOfTheType()
    {
        var f = Create([Rect(), new EllipseShape { Width = 5, Height = 5 }, Group(string.Empty, Rect())]);

        Assert.Equal(["Layer", "Group", "Ellipse", "Rectangle"], Names(f));
        Assert.Equal(string.Empty, f.List.Rows[1].StoredName);
    }

    [Fact]
    public void Rows_DefaultNames_AreTranslated_AndFollowALanguageChange()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string>
        {
            ["Rectangle"] = "Rechteck",
            ["Ellipse"] = "Ellipse",
            ["object name|Group"] = "Gruppe",
        });
        var f = Create([Rect(), Group(string.Empty, Rect())], localizer);

        Assert.Equal(["Layer", "Group", "Rectangle"], Names(f));

        localizer.SetLanguage("de");

        Assert.Equal(["Layer", "Gruppe", "Rechteck"], Names(f));
    }

    [Fact]
    public void Update_FlagsAndNames_KeepTheRowInstances()
    {
        var a = Rect("a");
        var f = Create([a, Rect("b", 20)]);
        var before = f.List.Rows.ToArray();

        f.Editor.SetHidden([a.Id], true);
        f.Editor.SetLocked([a.Id], true);
        f.Editor.Rename(a.Id, "Sun");

        Assert.True(before.SequenceEqual(f.List.Rows));
        var row = Row(f, a.Id);
        Assert.Equal("Sun", row.Name);
        Assert.False(row.IsVisible);
        Assert.True(row.IsLocked);
        Assert.True(row.IsEffectivelyHidden);
    }

    [Fact]
    public void Update_StructureChange_ReusesTheRowsOfTheSurvivingObjects()
    {
        var a = Rect("a");
        var f = Create([a, Rect("b", 20)]);
        var rowOfA = Row(f, a.Id);

        f.State.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(d.Layers[0].Id, Rect("c", 40)));

        Assert.Equal(["Layer", "c", "b", "a"], Names(f));
        Assert.Same(rowOfA, Row(f, a.Id));
    }

    [Fact]
    public void ChildOfAHiddenOrLockedGroup_IsMarkedEffectivelyHiddenOrLocked_KeepingItsOwnFlags()
    {
        var child = Rect("child");
        var group = Group("G", child) with { IsVisible = false, IsLocked = true };
        var f = Create([group]);
        f.List.SetExpanded(Row(f, group.Id), true);

        var row = Row(f, child.Id);

        Assert.True(row.IsVisible);
        Assert.False(row.IsLocked);
        Assert.True(row.IsEffectivelyHidden);
        Assert.True(row.IsEffectivelyLocked);
    }

    [Fact]
    public void ExpandedState_SurvivesUndoAndRedoAsFarAsTheGroupExists()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var f = Create([a, b]);
        f.State.SetSelection([a.Id, b.Id]);
        f.Editor.Group();
        var group = f.State.Document.Layers[0].Shapes[0];
        f.List.SetExpanded(Row(f, group.Id), true);
        Assert.Equal(4, f.List.Rows.Count);

        f.State.Undo();
        Assert.Equal(["Layer", "b", "a"], Names(f));

        f.State.Redo();
        Assert.Equal(4, f.List.Rows.Count);
        Assert.True(Row(f, group.Id).IsExpanded);
    }

    [Fact]
    public void Selection_OnTheCanvas_MarksRowsAndOpensTheGroupsAroundThem()
    {
        var x = Rect("x");
        var group = Group("G", x, Rect("y", 20));
        var f = Create([group]);
        Assert.Equal(["Layer", "G"], Names(f));

        f.State.EnterGroup(group.Id);
        f.State.SelectOnly(x.Id);

        Assert.Equal(["Layer", "G", "y", "x"], Names(f));
        Assert.True(Row(f, x.Id).IsSelected);
        Assert.False(Row(f, group.Id).IsSelected);
        Assert.True(Row(f, group.Id).IsExpanded);

        f.State.ClearSelection();
        Assert.False(Row(f, x.Id).IsSelected);
    }

    [Fact]
    public void Select_RowOnTheTopLevel_SelectsTheObjectAndLeavesGroups()
    {
        var group = Group("G", Rect("x"));
        var plain = Rect("p", 100);
        var f = Create([group, plain]);
        f.State.EnterGroup(group.Id);

        f.List.Select(Row(f, plain.Id), extend: false);

        Assert.Null(f.State.EnteredGroupId);
        Assert.Equal(new[] { plain.Id }, f.State.SelectedShapeIds.ToArray());
        Assert.True(Row(f, plain.Id).IsCurrent);
    }

    [Fact]
    public void Select_RowInsideAGroup_EntersTheGroup()
    {
        var x = Rect("x");
        var group = Group("G", x, Rect("y", 20));
        var f = Create([group]);
        f.List.SetExpanded(Row(f, group.Id), true);

        f.List.Select(Row(f, x.Id), extend: false);

        Assert.Equal(group.Id, f.State.EnteredGroupId);
        Assert.Equal(new[] { x.Id }, f.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Select_WithExtend_AddsAndRemovesRowsOfTheSameParent_AndReplacesOtherwise()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var inner = Rect("i", 50);
        var group = Group("G", inner, Rect("j", 70));
        var f = Create([a, b, group]);
        f.List.Select(Row(f, a.Id), extend: false);

        f.List.Select(Row(f, b.Id), extend: true);
        Assert.Equal(new[] { a.Id, b.Id }, f.State.SelectedShapeIds.ToArray());

        f.List.Select(Row(f, a.Id), extend: true);
        Assert.Equal(new[] { b.Id }, f.State.SelectedShapeIds.ToArray());

        f.List.SetExpanded(Row(f, group.Id), true);
        f.List.Select(Row(f, inner.Id), extend: true);
        Assert.Equal(new[] { inner.Id }, f.State.SelectedShapeIds.ToArray());
        Assert.Equal(group.Id, f.State.EnteredGroupId);
    }

    [Fact]
    public void Select_LockedRow_OnlyMarksItAsCurrentAndKeepsTheCanvasSelection()
    {
        var a = Rect("a");
        var locked = Rect("locked", 20) with { IsLocked = true };
        var f = Create([a, locked]);
        f.List.Select(Row(f, a.Id), extend: false);

        f.List.Select(Row(f, locked.Id), extend: false);

        Assert.Equal(new[] { a.Id }, f.State.SelectedShapeIds.ToArray());
        Assert.True(Row(f, locked.Id).IsCurrent);
        Assert.False(Row(f, a.Id).IsCurrent);
    }

    [Fact]
    public void Select_ThenTheCanvasSelectionChanges_ClearsTheCurrentMark()
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var f = Create([a, b]);
        f.List.Select(Row(f, a.Id), extend: false);
        Assert.True(Row(f, a.Id).IsCurrent);

        f.State.SelectOnly(b.Id);

        Assert.All(f.List.Rows, row => Assert.False(row.IsCurrent));
    }

    [Fact]
    public void Toggles_OnALockedHiddenRow_StillWork()
    {
        var locked = Rect("locked") with { IsLocked = true, IsVisible = false };
        var f = Create([locked]);
        var row = Row(f, locked.Id);
        var steps = f.State.History.Steps.Count;

        row.ToggleLockCommand.Execute(null);
        row.ToggleVisibilityCommand.Execute(null);

        Assert.Equal(steps + 2, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.UnlockShapes, f.State.History.Steps[^2]);
        Assert.Equal(HistoryStepType.ShowShapes, f.State.History.Steps[^1]);
        Assert.False(row.IsLocked);
        Assert.True(row.IsVisible);
    }

    [Fact]
    public void Toggles_VisibleRow_HidesAndLocksIt()
    {
        var a = Rect("a");
        var f = Create([a]);
        var row = Row(f, a.Id);

        row.ToggleVisibilityCommand.Execute(null);
        row.ToggleLockCommand.Execute(null);

        Assert.Equal(HistoryStepType.HideShapes, f.State.History.Steps[^2]);
        Assert.Equal(HistoryStepType.LockShapes, f.State.History.Steps[^1]);
        Assert.False(row.IsVisible);
        Assert.True(row.IsLocked);
    }

    [Fact]
    public void Rename_CommitsAsOneStep_EmptyNameFallsBackToTheDefault()
    {
        var a = Rect("a");
        var f = Create([a]);
        var row = Row(f, a.Id);

        row.BeginRenameCommand.Execute(null);
        Assert.True(row.IsRenaming);
        Assert.Equal("a", row.EditText);
        row.EditText = "Sun";
        row.CommitRenameCommand.Execute(null);

        Assert.False(row.IsRenaming);
        Assert.Equal("Sun", row.Name);
        Assert.Equal(HistoryStepType.RenameShape, f.State.History.Steps[^1]);

        row.BeginRenameCommand.Execute(null);
        row.EditText = "  ";
        row.CommitRenameCommand.Execute(null);

        Assert.Equal("Rectangle", row.Name);
    }

    [Fact]
    public void Rename_Cancelled_ChangesNothing()
    {
        var a = Rect("a");
        var f = Create([a]);
        var row = Row(f, a.Id);
        var steps = f.State.History.Steps.Count;

        row.BeginRenameCommand.Execute(null);
        row.EditText = "x";
        row.CancelRenameCommand.Execute(null);
        row.CommitRenameCommand.Execute(null);

        Assert.False(row.IsRenaming);
        Assert.Equal("a", row.Name);
        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void Rename_LayerRow_IsNotPossible()
    {
        var f = Create([Rect("a")]);

        Assert.False(f.List.Rows[0].BeginRenameCommand.CanExecute(null));
    }

    [Fact]
    public void Reorder_WithinAParent_MovesTheObjectAboveTheTarget()
    {
        var a = Rect("a");
        var c = Rect("c", 40);
        var f = Create([a, Rect("b", 20), c]);

        Assert.True(f.List.CanReorder(Row(f, a.Id), Row(f, c.Id)));
        f.List.Reorder(Row(f, a.Id), Row(f, c.Id));

        Assert.Equal(["Layer", "a", "c", "b"], Names(f));
        Assert.Equal(HistoryStepType.ReorderShape, f.State.History.Steps[^1]);
    }

    [Fact]
    public void Reorder_AnotherParentOrALayerRow_IsIgnored()
    {
        var inner = Rect("i");
        var group = Group("G", inner);
        var a = Rect("a", 50);
        var f = Create([group, a]);
        f.List.SetExpanded(Row(f, group.Id), true);
        var steps = f.State.History.Steps.Count;

        Assert.False(f.List.CanReorder(Row(f, a.Id), Row(f, inner.Id)));
        Assert.False(f.List.CanReorder(Row(f, a.Id), f.List.Rows[0]));
        Assert.False(f.List.CanReorder(f.List.Rows[0], Row(f, a.Id)));
        f.List.Reorder(Row(f, a.Id), Row(f, inner.Id));

        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void Select_LayerRow_MakesTheLayerActive()
    {
        var f = Create([Rect("a")]);
        f.State.Edit(HistoryStepType.DrawRectangle, d => d.AddLayer(new Layer { Name = "Two" }));
        var first = f.List.Rows.Single(r => r.IsLayer && r.Name == "Layer");

        f.List.Select(first, extend: false);

        Assert.Equal(first.Id, f.State.ActiveLayerId);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var state = new EditorState();

        Assert.Throws<ArgumentNullException>(() => new ObjectsViewModel(null!, new ObjectEditor(state), FakeLocalizer.Create()));
        Assert.Throws<ArgumentNullException>(() => new ObjectsViewModel(state, null!, FakeLocalizer.Create()));
    }
}
