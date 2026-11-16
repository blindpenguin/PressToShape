using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionQueriesGroupTests
{
    private static RectangleShape Rect(double x, double y = 0, double size = 10) => new() { X = x, Y = y, Width = size, Height = size };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static string[] Names(System.Collections.Generic.IEnumerable<Shape> shapes) => shapes.Select(s => s.Name).ToArray();

    [Fact]
    public void TargetFor_TopLevelHit_IsTheOutermostGroup()
    {
        var leaf = Rect(0) with { Name = "leaf" };
        var inner = Group(leaf) with { Name = "inner" };
        var outer = Group(inner, Rect(50)) with { Name = "outer" };

        var target = SelectionQueries.TargetFor(Build(outer), leaf);

        Assert.Equal(outer.Id, target!.Id);
    }

    [Fact]
    public void TargetFor_PlainShape_IsTheShapeItself()
    {
        var rect = Rect(0);

        Assert.Equal(rect.Id, SelectionQueries.TargetFor(Build(rect), rect)!.Id);
    }

    [Fact]
    public void TargetFor_EnteredOuterGroup_IsTheDirectChild()
    {
        var leaf = Rect(0);
        var inner = Group(leaf);
        var outer = Group(inner, Rect(50));

        var target = SelectionQueries.TargetFor(Build(outer), leaf, outer.Id);

        Assert.Equal(inner.Id, target!.Id);
    }

    [Fact]
    public void TargetFor_EnteredDirectParent_IsTheLeaf()
    {
        var leaf = Rect(0);
        var group = Group(leaf, Rect(50));

        Assert.Equal(leaf.Id, SelectionQueries.TargetFor(Build(group), leaf, group.Id)!.Id);
    }

    [Fact]
    public void TargetFor_LeafOutsideTheEnteredGroup_IsNull()
    {
        var outside = Rect(100);
        var group = Group(Rect(0));

        Assert.Null(SelectionQueries.TargetFor(Build(outside, group), outside, group.Id));
    }

    [Fact]
    public void Selectable_TopLevel_ListsGroupsNotTheirMembers()
    {
        var group = Group(Rect(0), Rect(20));
        var plain = Rect(100);

        var selectable = SelectionQueries.Selectable(Build(group, plain));

        Assert.Equal([group.Id, plain.Id], selectable.Select(s => s.Id));
    }

    [Fact]
    public void Selectable_EnteredGroup_ListsOnlyItsChildren()
    {
        var a = Rect(0);
        var b = Rect(20);
        var group = Group(a, b);

        var selectable = SelectionQueries.Selectable(Build(group, Rect(100)), group.Id);

        Assert.Equal([a.Id, b.Id], selectable.Select(s => s.Id));
    }

    [Fact]
    public void Selectable_HiddenAndLockedObjects_AreLeftOut()
    {
        var shown = Rect(0);
        var document = Build(shown, Rect(20) with { IsVisible = false }, Rect(40) with { IsLocked = true });

        Assert.Equal([shown.Id], SelectionQueries.Selectable(document).Select(s => s.Id));
    }

    [Fact]
    public void Selectable_EnteredGroupThatIsHiddenOrLocked_IsEmpty()
    {
        var hidden = Group(Rect(0)) with { IsVisible = false };
        var locked = Group(Rect(0)) with { IsLocked = true };

        Assert.Empty(SelectionQueries.Selectable(Build(hidden), hidden.Id));
        Assert.Empty(SelectionQueries.Selectable(Build(locked), locked.Id));
    }

    [Fact]
    public void ShapesTouching_Box_ReturnsGroupsAtTopLevelAndChildrenInside()
    {
        var a = Rect(0);
        var b = Rect(200);
        var group = Group(a, b);
        var document = Build(group);

        Assert.Equal([group.Id], SelectionQueries.ShapesTouching(document, -5, -5, 20, 20).Select(s => s.Id));
        Assert.Equal([a.Id], SelectionQueries.ShapesTouching(document, -5, -5, 20, 20, group.Id).Select(s => s.Id));
    }

    [Fact]
    public void IsSelectable_ChildOfHiddenOrLockedGroup_IsFalse()
    {
        var hiddenChild = Rect(0);
        var lockedChild = Rect(30);
        var document = Build(Group(hiddenChild) with { IsVisible = false }, Group(lockedChild) with { IsLocked = true });

        Assert.False(SelectionQueries.IsSelectable(document, hiddenChild.Id));
        Assert.False(SelectionQueries.IsSelectable(document, lockedChild.Id));
    }

    [Fact]
    public void IsSelectable_GroupAndChildInPlainGroup_IsTrue()
    {
        var child = Rect(0);
        var group = Group(child);
        var document = Build(group);

        Assert.True(SelectionQueries.IsSelectable(document, group.Id));
        Assert.True(SelectionQueries.IsSelectable(document, child.Id));
    }

    [Fact]
    public void SelectedLeaves_Group_ReturnsItsLeavesInOrder()
    {
        var a = Rect(0);
        var b = Rect(20);
        var c = Rect(40);
        var group = Group(a, Group(b));
        var document = Build(group, c);

        var leaves = SelectionQueries.SelectedLeaves(document, [c.Id, group.Id]);

        Assert.Equal([a.Id, b.Id, c.Id], leaves.Select(s => s.Id));
    }

    [Fact]
    public void SelectedLeaves_GroupAndItsChild_ListsTheChildOnce()
    {
        var a = Rect(0);
        var group = Group(a, Rect(20));

        var leaves = SelectionQueries.SelectedLeaves(Build(group), [group.Id, a.Id]);

        Assert.Equal(2, leaves.Count);
    }

    [Fact]
    public void SelectedShapes_NestedIds_AreFoundInDocumentOrder()
    {
        var a = Rect(0) with { Name = "a" };
        var group = Group(a) with { Name = "g" };
        var z = Rect(40) with { Name = "z" };

        var shapes = SelectionQueries.SelectedShapes(Build(group, z), [z.Id, a.Id, group.Id]);

        Assert.Equal(["g", "a", "z"], Names(shapes));
    }

    [Fact]
    public void SelectableWithParents_ReportsTheClosestGroup()
    {
        var a = Rect(0);
        var group = Group(a);
        var plain = Rect(40);

        var result = SelectionQueries.SelectableWithParents(Build(group, plain), new[] { a.Id, group.Id, plain.Id, Guid.NewGuid() }.ToHashSet());

        Assert.Equal(3, result.Count);
        Assert.Equal(group.Id, result[a.Id]);
        Assert.Null(result[group.Id]);
        Assert.Null(result[plain.Id]);
    }
}
