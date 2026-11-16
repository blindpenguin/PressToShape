using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class FlagOperationsTests
{
    private static RectangleShape Rect(double x = 0) => new() { X = x, Width = 10, Height = 10 };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static IReadOnlySet<Guid> Ids(params Shape[] shapes) => shapes.Select(s => s.Id).ToHashSet();

    [Fact]
    public void SetHidden_Leaf_HidesOnlyThatLeaf()
    {
        var a = Rect();
        var b = Rect(20);

        var result = Build(a, b).SetHidden(Ids(a), true);

        Assert.False(result.FindShape(a.Id)!.Shape.IsVisible);
        Assert.True(result.FindShape(b.Id)!.Shape.IsVisible);
    }

    [Fact]
    public void SetHidden_False_ShowsAgain()
    {
        var a = Rect() with { IsVisible = false };

        var result = Build(a).SetHidden(Ids(a), false);

        Assert.True(result.FindShape(a.Id)!.Shape.IsVisible);
    }

    [Fact]
    public void SetHidden_Group_ChangesOnlyTheGroupsOwnFlag()
    {
        var leaf = Rect();
        var group = Group(leaf);

        var result = Build(group).SetHidden(Ids(group), true);

        Assert.False(result.FindShape(group.Id)!.Shape.IsVisible);
        var child = result.FindShape(leaf.Id)!;
        Assert.True(child.Shape.IsVisible);
        Assert.False(child.IsEffectivelyVisible);
    }

    [Fact]
    public void SetHidden_GroupAndItsChildTogether_ChangesBoth()
    {
        var leaf = Rect();
        var group = Group(leaf);

        var result = Build(group).SetHidden(Ids(group, leaf), true);

        Assert.False(result.FindShape(group.Id)!.Shape.IsVisible);
        Assert.False(result.FindShape(leaf.Id)!.Shape.IsVisible);
    }

    [Fact]
    public void SetHidden_KeepsIdsAndBoxOfGroups()
    {
        var group = Group(Rect(), Rect(30));

        var result = Build(group).SetHidden(Ids(group), true);

        var changed = result.FindShape(group.Id)!.Shape;
        Assert.Equal(group.Bounds, changed.Bounds);
    }

    [Fact]
    public void SetLocked_AndUnlock_Work()
    {
        var a = Rect();
        var document = Build(a);

        var locked = document.SetLocked(Ids(a), true);
        var unlocked = locked.SetLocked(Ids(a), false);

        Assert.True(locked.FindShape(a.Id)!.Shape.IsLocked);
        Assert.False(unlocked.FindShape(a.Id)!.Shape.IsLocked);
    }

    [Fact]
    public void SetLocked_Group_ChildIsEffectivelyLockedButKeepsItsFlag()
    {
        var leaf = Rect();
        var group = Group(leaf);

        var found = Build(group).SetLocked(Ids(group), true).FindShape(leaf.Id)!;

        Assert.False(found.Shape.IsLocked);
        Assert.True(found.IsEffectivelyLocked);
    }

    [Fact]
    public void SetHiddenAndLocked_NoChange_ReturnSameDocument()
    {
        var a = Rect();
        var document = Build(a);

        Assert.Same(document, document.SetHidden(Ids(a), false));
        Assert.Same(document, document.SetLocked(Ids(a), false));
        Assert.Same(document, document.SetHidden(new HashSet<Guid>(), true));
    }

    [Fact]
    public void SetHiddenAndLocked_UnknownId_Throws()
    {
        var document = Build(Rect());
        var unknown = new HashSet<Guid> { Guid.NewGuid() };

        Assert.Throws<KeyNotFoundException>(() => document.SetHidden(unknown, true));
        Assert.Throws<KeyNotFoundException>(() => document.SetLocked(unknown, true));
    }

    [Fact]
    public void ShowAll_NestedHiddenShapes_AreShown()
    {
        var deep = Rect() with { IsVisible = false };
        var inner = Group(deep) with { IsVisible = false };
        var outer = Group(inner, Rect(30) with { IsVisible = false });
        var top = Rect(60) with { IsVisible = false };

        var result = Build(outer, top).ShowAll();

        Assert.True(result.FindShape(deep.Id)!.IsEffectivelyVisible);
        Assert.True(result.FindShape(inner.Id)!.IsEffectivelyVisible);
        Assert.True(result.FindShape(top.Id)!.IsEffectivelyVisible);
        Assert.All(new[] { outer.Id, inner.Id, deep.Id, top.Id }, id => Assert.True(result.FindShape(id)!.Shape.IsVisible));
    }

    [Fact]
    public void UnlockAll_NestedLockedShapes_AreUnlocked()
    {
        var deep = Rect() with { IsLocked = true };
        var group = Group(deep) with { IsLocked = true };

        var result = Build(group).UnlockAll();

        Assert.False(result.FindShape(group.Id)!.Shape.IsLocked);
        Assert.False(result.FindShape(deep.Id)!.Shape.IsLocked);
    }

    [Fact]
    public void ShowAllAndUnlockAll_NothingToChange_ReturnSameDocument()
    {
        var document = Build(Group(Rect()), Rect(20));

        Assert.Same(document, document.ShowAll());
        Assert.Same(document, document.UnlockAll());
    }

    [Fact]
    public void ShowAllAndUnlockAll_DoNotTouchLayers()
    {
        var document = Build(Rect() with { IsVisible = false, IsLocked = true });
        document = document.UpdateLayer(document.Layers[0].Id, layer => layer with { IsVisible = false, IsLocked = true });

        var result = document.ShowAll().UnlockAll();

        Assert.False(result.Layers[0].IsVisible);
        Assert.True(result.Layers[0].IsLocked);
    }

    [Fact]
    public void Rename_Shape_SetsTrimmedName()
    {
        var a = Rect();

        var result = Build(a).Rename(a.Id, "  Sun ");

        Assert.Equal("Sun", result.FindShape(a.Id)!.Shape.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_EmptyOrBlank_ClearsTheName(string name)
    {
        var a = Rect() with { Name = "Old" };

        var result = Build(a).Rename(a.Id, name);

        Assert.Equal(string.Empty, result.FindShape(a.Id)!.Shape.Name);
    }

    [Fact]
    public void Rename_SameName_ReturnsSameDocument()
    {
        var a = Rect() with { Name = "Sun" };
        var document = Build(a);

        Assert.Same(document, document.Rename(a.Id, "Sun"));
    }

    [Fact]
    public void Rename_GroupInsideGroup_Works()
    {
        var inner = Group(Rect());
        var outer = Group(inner);

        var result = Build(outer).Rename(inner.Id, "Wheels");

        Assert.Equal("Wheels", result.FindShape(inner.Id)!.Shape.Name);
    }

    [Fact]
    public void Rename_UnknownIdOrNullName_Throws()
    {
        var a = Rect();
        var document = Build(a);

        Assert.Throws<KeyNotFoundException>(() => document.Rename(Guid.NewGuid(), "x"));
        Assert.Throws<ArgumentNullException>(() => document.Rename(a.Id, null!));
    }

    [Fact]
    public void FlagOperations_Result_LeaveOriginalUnchanged()
    {
        var a = Rect();
        var document = Build(a);

        _ = document.SetHidden(Ids(a), true).SetLocked(Ids(a), true).Rename(a.Id, "x");

        var original = document.FindShape(a.Id)!.Shape;
        Assert.True(original.IsVisible);
        Assert.False(original.IsLocked);
        Assert.Equal(string.Empty, original.Name);
    }
}
