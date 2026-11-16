using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class RingMenuContentTests
{
    private readonly ObjectMenuCommands _objects = new(
        Command(), Command(), Command(), Command(), Command(), Command(), Command(), Command(), Command(), Command());

    private readonly EditMenuCommands _edit = new(
        Command(), Command(), Command(), Command(), Command(), Command(), Command(), Command());

    private static ICommand Command() => new RelayCommand(() => { });

    private Func<RingMenuMode, IReadOnlyList<RingSlotDefinition>> Create() => RingMenuContent.Create(_objects, _edit);

    [Fact]
    public void Selection_HasTheSevenSlotsInOrder()
    {
        var slots = Create()(RingMenuMode.Selection);

        Assert.Equal(["Group", "Ungroup", "Duplicate", "Delete", "Lock", "Hide", "Arrange"], slots.Select(s => s.Name));
    }

    [Fact]
    public void Selection_Slots_RunTheCommandsOfTheMenus()
    {
        var slots = Create()(RingMenuMode.Selection);

        Assert.Same(_objects.Group, slots[0].Command);
        Assert.Same(_objects.Ungroup, slots[1].Command);
        Assert.Same(_edit.Duplicate, slots[2].Command);
        Assert.Same(_edit.Delete, slots[3].Command);
        Assert.Same(_objects.Lock, slots[4].Command);
        Assert.Same(_objects.Hide, slots[5].Command);
    }

    [Fact]
    public void Selection_ArrangeSlot_IsASubmenuWithTheFourArrangeCommands()
    {
        var arrange = Create()(RingMenuMode.Selection)[6];

        Assert.Null(arrange.Command);
        Assert.NotNull(arrange.Children);
        Assert.Equal(["Bring to Front", "Bring Forward", "Send Backward", "Send to Back"], arrange.Children.Select(s => s.Name));
        Assert.Same(_objects.BringToFront, arrange.Children[0].Command);
        Assert.Same(_objects.BringForward, arrange.Children[1].Command);
        Assert.Same(_objects.SendBackward, arrange.Children[2].Command);
        Assert.Same(_objects.SendToBack, arrange.Children[3].Command);
    }

    [Fact]
    public void EmptyCanvas_HasPasteAndSelectAll()
    {
        var slots = Create()(RingMenuMode.EmptyCanvas);

        Assert.Equal(["Paste", "Select All"], slots.Select(s => s.Name));
        Assert.Same(_edit.Paste, slots[0].Command);
        Assert.Same(_edit.SelectAll, slots[1].Command);
    }

    [Fact]
    public void Slots_AreTheSameOnEveryCall()
    {
        var content = Create();

        Assert.Same(content(RingMenuMode.Selection), content(RingMenuMode.Selection));
    }

    [Fact]
    public void EveryLevel_HasUniqueNonEmptyNamesAndDefinedIcons()
    {
        foreach (var mode in new[] { RingMenuMode.Selection, RingMenuMode.EmptyCanvas })
        {
            foreach (var level in Levels(Create()(mode)))
            {
                Assert.All(level, slot =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(slot.Name));
                    Assert.True(Enum.IsDefined(slot.Icon));
                });
                Assert.Equal(level.Count, level.Select(s => s.Name).Distinct().Count());
            }
        }
    }

    [Fact]
    public void Create_WithoutCommands_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RingMenuContent.Create(null!, _edit));
        Assert.Throws<ArgumentNullException>(() => RingMenuContent.Create(_objects, null!));
    }

    private static IEnumerable<IReadOnlyList<RingSlotDefinition>> Levels(IReadOnlyList<RingSlotDefinition> top)
    {
        yield return top;
        foreach (var child in top.Where(s => s.Children is not null))
        {
            foreach (var level in Levels(child.Children!))
            {
                yield return level;
            }
        }
    }
}
