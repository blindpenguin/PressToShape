using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class EditorStateGroupTests
{
    private static RectangleShape Rect(double x) => new() { X = x, Width = 10, Height = 10 };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static EditorState StateWith(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
    }

    private static void Edit(EditorState state, Func<Document, Document> change) => state.Edit(HistoryStepType.MoveShape, change);

    [Fact]
    public void EnterGroup_Group_SetsEnteredGroup()
    {
        var group = Group(Rect(0), Rect(20));
        var state = StateWith(group);

        Assert.True(state.EnterGroup(group.Id));

        Assert.Equal(group.Id, state.EnteredGroupId);
    }

    [Fact]
    public void EnterGroup_PlainShapeUnknownIdHiddenOrLockedGroup_DoesNothing()
    {
        var plain = Rect(0);
        var hidden = Group(Rect(30)) with { IsVisible = false };
        var locked = Group(Rect(60)) with { IsLocked = true };
        var state = StateWith(plain, hidden, locked);

        Assert.False(state.EnterGroup(plain.Id));
        Assert.False(state.EnterGroup(Guid.NewGuid()));
        Assert.False(state.EnterGroup(hidden.Id));
        Assert.False(state.EnterGroup(locked.Id));
        Assert.Null(state.EnteredGroupId);
    }

    [Fact]
    public void EnterGroup_SelectedGroup_ClearsTheSelectionOfTheGroupItself()
    {
        var group = Group(Rect(0));
        var state = StateWith(group);
        state.SelectOnly(group.Id);

        state.EnterGroup(group.Id);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void EnterGroup_SelectedChildren_StaySelected()
    {
        var a = Rect(0);
        var group = Group(a, Rect(20));
        var state = StateWith(group);
        state.EnterGroup(group.Id);
        state.SelectOnly(a.Id);

        state.EnterGroup(group.Id);

        Assert.Equal(new Guid[] { a.Id }, state.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void LeaveGroup_EnteredGroup_LeavesAndSelectsIt()
    {
        var group = Group(Rect(0), Rect(20));
        var state = StateWith(group);
        state.EnterGroup(group.Id);

        state.LeaveGroup();

        Assert.Null(state.EnteredGroupId);
        Assert.Equal(new Guid[] { group.Id }, state.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void LeaveGroup_NestedGroup_EntersTheOuterGroup()
    {
        var inner = Group(Rect(0));
        var outer = Group(inner, Rect(40));
        var state = StateWith(outer);
        state.EnterGroup(outer.Id);
        state.EnterGroup(inner.Id);

        state.LeaveGroup();

        Assert.Equal(outer.Id, state.EnteredGroupId);
        Assert.Equal(new Guid[] { inner.Id }, state.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void LeaveGroup_NothingEntered_DoesNothing()
    {
        var state = StateWith(Rect(0));

        state.LeaveGroup();

        Assert.Null(state.EnteredGroupId);
    }

    [Fact]
    public void EnteredGroup_Undo_ClearedWhenGroupIsGone()
    {
        var a = Rect(0);
        var b = Rect(20);
        var state = StateWith(a, b);
        Edit(state, d => d.Group([a.Id, b.Id], Guid.NewGuid()));
        var group = (GroupShape)state.Document.Layers[0].Shapes[0];
        state.EnterGroup(group.Id);

        state.Undo();

        Assert.Null(state.EnteredGroupId);
    }

    [Fact]
    public void EnteredGroup_Delete_Clears()
    {
        var group = Group(Rect(0), Rect(20));
        var state = StateWith(group);
        state.EnterGroup(group.Id);

        Edit(state, d => d.RemoveShape(group.Id));

        Assert.Null(state.EnteredGroupId);
    }

    [Fact]
    public void EnteredGroup_Ungroup_Clears()
    {
        var group = Group(Rect(0), Rect(20));
        var state = StateWith(group);
        state.EnterGroup(group.Id);

        Edit(state, d => d.Ungroup(new HashSet<Guid> { group.Id }));

        Assert.Null(state.EnteredGroupId);
    }

    [Fact]
    public void EnteredGroup_HiddenOrLocked_Clears()
    {
        var group = Group(Rect(0), Rect(20));
        var state = StateWith(group);

        state.EnterGroup(group.Id);
        Edit(state, d => d.SetHidden(new HashSet<Guid> { group.Id }, true));
        Assert.Null(state.EnteredGroupId);

        Edit(state, d => d.SetHidden(new HashSet<Guid> { group.Id }, false));
        state.EnterGroup(group.Id);
        Edit(state, d => d.SetLocked(new HashSet<Guid> { group.Id }, true));
        Assert.Null(state.EnteredGroupId);
    }

    [Fact]
    public void EnteredGroup_ChangeInsideTheGroup_StaysEntered()
    {
        var a = Rect(0);
        var group = Group(a, Rect(20));
        var state = StateWith(group);
        state.EnterGroup(group.Id);

        Edit(state, d => d.UpdateShape(a.Id, s => s with { X = 5 }));

        Assert.Equal(group.Id, state.EnteredGroupId);
    }

    [Fact]
    public void ReplaceDocument_NewDocument_ClearsTheEnteredGroup()
    {
        var group = Group(Rect(0));
        var state = StateWith(group);
        state.EnterGroup(group.Id);

        state.ReplaceDocument(Document.Create("New"));

        Assert.Null(state.EnteredGroupId);
    }

    [Fact]
    public void SetSelection_IdsWithDifferentParents_KeepsTheFirstParentsIds()
    {
        var a = Rect(0);
        var b = Rect(20);
        var outside = Rect(100);
        var group = Group(a, b);
        var state = StateWith(group, outside);

        state.SetSelection([outside.Id, a.Id]);
        Assert.Equal(new Guid[] { outside.Id }, state.SelectedShapeIds.ToArray());

        state.SetSelection([a.Id, outside.Id, b.Id]);
        Assert.Equal(new Guid[] { a.Id, b.Id }, state.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void SetSelection_GroupAndPlainShapeOnTopLevel_BothSelected()
    {
        var group = Group(Rect(0));
        var plain = Rect(100);
        var state = StateWith(group, plain);

        state.SetSelection([group.Id, plain.Id]);

        Assert.Equal(new Guid[] { group.Id, plain.Id }, state.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void SetSelection_ChildOfHiddenGroup_IsLeftOut()
    {
        var child = Rect(0);
        var state = StateWith(Group(child) with { IsVisible = false });

        state.SetSelection([child.Id]);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Selection_GroupGetsHidden_LeavesTheSelection()
    {
        var group = Group(Rect(0));
        var state = StateWith(group);
        state.SelectOnly(group.Id);

        Edit(state, d => d.SetHidden(new HashSet<Guid> { group.Id }, true));

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Selection_ChildSelectedThenGroupLocked_LeavesTheSelection()
    {
        var child = Rect(0);
        var group = Group(child, Rect(20));
        var state = StateWith(group);
        state.EnterGroup(group.Id);
        state.SelectOnly(child.Id);

        Edit(state, d => d.SetLocked(new HashSet<Guid> { group.Id }, true));

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Selection_AllMembersRemoved_GroupDisappearsFromTheSelection()
    {
        var a = Rect(0);
        var group = Group(a);
        var state = StateWith(group);
        state.SelectOnly(group.Id);

        Edit(state, d => d.RemoveShape(a.Id));

        Assert.False(state.HasSelection);
        Assert.Empty(state.Document.Layers[0].Shapes);
    }
}
