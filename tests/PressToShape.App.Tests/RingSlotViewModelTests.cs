using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class RingSlotViewModelTests
{
    [Fact]
    public void Name_AfterALanguageChangeAndRefresh_IsTranslated()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Group"] = "Gruppieren" });
        var slot = new RingSlotViewModel(localizer, Command("Group", new RelayCommand(() => { })));
        var raised = new List<string?>();
        slot.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        localizer.SetLanguage("de");
        slot.Refresh();

        Assert.Equal("Gruppieren", slot.Name);
        Assert.Contains(nameof(RingSlotViewModel.Name), raised);
    }

    [Fact]
    public void IsAvailable_FollowsCanExecuteChanged()
    {
        var enabled = false;
        var command = new RelayCommand(() => { }, () => enabled);
        var slot = new RingSlotViewModel(FakeLocalizer.Create(), Command("Group", command));
        Assert.False(slot.IsAvailable);

        enabled = true;
        command.NotifyCanExecuteChanged();

        Assert.True(slot.IsAvailable);
    }

    [Fact]
    public void IsAvailable_OfASubmenu_IsTrueWhenOneChildCanRun()
    {
        var enabled = false;
        var blocked = new RelayCommand(() => { }, () => false);
        var sometimes = new RelayCommand(() => { }, () => enabled);
        var slot = new RingSlotViewModel(
            FakeLocalizer.Create(),
            RingSlotDefinition.WithChildren("Arrange", MaterialIconKind.Layers, [Command("Front", blocked), Command("Back", sometimes)]));
        Assert.False(slot.IsAvailable);

        enabled = true;
        sometimes.NotifyCanExecuteChanged();

        Assert.True(slot.IsAvailable);
        Assert.True(slot.HasChildren);
        Assert.Equal(2, slot.Children.Count);
    }

    [Fact]
    public void Execute_AvailableCommand_RunsItOnce()
    {
        var runs = 0;
        var slot = new RingSlotViewModel(FakeLocalizer.Create(), Command("Group", new RelayCommand(() => runs++)));

        var ran = slot.Execute();

        Assert.True(ran);
        Assert.Equal(1, runs);
    }

    [Fact]
    public void Execute_UnavailableCommand_DoesNotRun()
    {
        var runs = 0;
        var slot = new RingSlotViewModel(FakeLocalizer.Create(), Command("Group", new RelayCommand(() => runs++, () => false)));

        var ran = slot.Execute();

        Assert.False(ran);
        Assert.Equal(0, runs);
    }

    [Fact]
    public void Execute_CommandThatBecameUnavailableWithoutNotice_DoesNotRun()
    {
        var enabled = true;
        var runs = 0;
        var slot = new RingSlotViewModel(FakeLocalizer.Create(), Command("Group", new RelayCommand(() => runs++, () => enabled)));

        enabled = false;
        var ran = slot.Execute();

        Assert.False(ran);
        Assert.Equal(0, runs);
    }

    [Fact]
    public void Execute_SubmenuSlot_RunsNothing()
    {
        var slot = new RingSlotViewModel(
            FakeLocalizer.Create(),
            RingSlotDefinition.WithChildren("Arrange", MaterialIconKind.Layers, [Command("Front", new RelayCommand(() => { }))]));

        Assert.False(slot.Execute());
    }

    [Fact]
    public void Dispose_StopsListeningToTheCommand()
    {
        var enabled = false;
        var command = new RelayCommand(() => { }, () => enabled);
        var slot = new RingSlotViewModel(FakeLocalizer.Create(), Command("Group", command));
        slot.Dispose();

        enabled = true;
        command.NotifyCanExecuteChanged();

        Assert.False(slot.IsAvailable);
    }

    [Fact]
    public void WithCommand_InvalidInput_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => RingSlotDefinition.WithCommand(" ", MaterialIconKind.Layers, new RelayCommand(() => { })));
        Assert.Throws<ArgumentNullException>(() => RingSlotDefinition.WithCommand("Group", MaterialIconKind.Layers, null!));
    }

    [Fact]
    public void WithChildren_InvalidInput_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => RingSlotDefinition.WithChildren("Arrange", MaterialIconKind.Layers, []));
        Assert.Throws<ArgumentException>(() => RingSlotDefinition.WithChildren("", MaterialIconKind.Layers, [Command("Front", new RelayCommand(() => { }))]));
        Assert.Throws<ArgumentNullException>(() => RingSlotDefinition.WithChildren("Arrange", MaterialIconKind.Layers, null!));
    }

    [Fact]
    public void Commands_OfNestedSubmenus_ListsEveryCommand()
    {
        var first = new RelayCommand(() => { });
        var second = new RelayCommand(() => { });
        var nested = RingSlotDefinition.WithChildren(
            "Outer",
            MaterialIconKind.Layers,
            [Command("A", first), RingSlotDefinition.WithChildren("Inner", MaterialIconKind.Layers, [Command("B", second)])]);

        Assert.Equal([first, second], nested.Commands());
    }

    private static RingSlotDefinition Command(string name, System.Windows.Input.ICommand command) =>
        RingSlotDefinition.WithCommand(name, MaterialIconKind.Layers, command);
}
