using System;
using System.Linq;
using System.Windows.Input;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class ObjectCommandsTests
{
    private sealed class FakeShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static (DesktopShellViewModel Shell, Guid[] Ids) Create(int count)
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        var ids = TestSelection.AddAndSelect(shell.State, count);
        return (shell, ids);
    }

    private static MenuItemViewModel Item(DesktopShellViewModel shell, string header)
    {
        static System.Collections.Generic.IEnumerable<MenuItemViewModel> Flatten(System.Collections.Generic.IEnumerable<MenuItemViewModel> items) =>
            items.SelectMany(i => new[] { i }.Concat(Flatten(i.Children)));

        return Flatten(shell.MenuBar.Menus).Single(i => i.Header.Replace("_", string.Empty) == header);
    }

    private static void Run(ICommand command)
    {
        Assert.True(command.CanExecute(null));
        command.Execute(null);
    }

    [Fact]
    public void Commands_NothingSelected_AreDisabled()
    {
        var (shell, _) = Create(3);
        shell.State.ClearSelection();

        Assert.False(shell.GroupCommand.CanExecute(null));
        Assert.False(shell.UngroupCommand.CanExecute(null));
        Assert.False(shell.BringToFrontCommand.CanExecute(null));
        Assert.False(shell.SendToBackCommand.CanExecute(null));
        Assert.False(shell.LockCommand.CanExecute(null));
        Assert.False(shell.HideCommand.CanExecute(null));
        Assert.False(shell.UnlockAllCommand.CanExecute(null));
        Assert.False(shell.ShowAllCommand.CanExecute(null));
    }

    [Fact]
    public void Group_TwoSelectedShapes_GroupsThemAsOneStepAndUngroupBringsThemBack()
    {
        var (shell, ids) = Create(3);
        shell.State.SetSelection(ids.Take(2));
        var steps = shell.State.History.Steps.Count;

        Assert.False(shell.UngroupCommand.CanExecute(null));
        Run(shell.GroupCommand);

        Assert.Equal(steps + 1, shell.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.GroupShapes, shell.State.History.Steps[^1]);
        Assert.IsType<GroupShape>(shell.State.Document.FindShape(shell.State.SelectedShapeIds.Single())!.Shape);
        Assert.False(shell.GroupCommand.CanExecute(null));
        Assert.True(shell.UngroupCommand.CanExecute(null));

        Run(shell.UngroupCommand);

        Assert.Equal(HistoryStepType.UngroupShapes, shell.State.History.Steps[^1]);
        Assert.Equal(ids.Take(2), shell.State.SelectedShapeIds);
    }

    [Fact]
    public void Group_OneSelectedShape_IsDisabled()
    {
        var (shell, ids) = Create(2);
        shell.State.SetSelection(ids.Take(1));

        Assert.False(shell.GroupCommand.CanExecute(null));
    }

    [Fact]
    public void Arrange_BottomShape_BringToFrontAndForwardAreEnabledSendBackwardIsNot()
    {
        var (shell, ids) = Create(3);
        shell.State.SetSelection(ids.Take(1));

        Assert.True(shell.BringToFrontCommand.CanExecute(null));
        Assert.True(shell.BringForwardCommand.CanExecute(null));
        Assert.False(shell.SendBackwardCommand.CanExecute(null));
        Assert.False(shell.SendToBackCommand.CanExecute(null));

        Run(shell.BringToFrontCommand);

        Assert.Equal(HistoryStepType.BringToFront, shell.State.History.Steps[^1]);
        Assert.Equal(ids[0], shell.State.Document.Layers.SelectMany(l => l.Shapes).Last().Id);
        Assert.False(shell.BringToFrontCommand.CanExecute(null));
        Assert.True(shell.SendToBackCommand.CanExecute(null));
    }

    [Fact]
    public void Arrange_SelectionChange_RaisesCanExecuteChanged()
    {
        var (shell, ids) = Create(3);
        shell.State.SetSelection(ids.Take(1));
        var raised = 0;
        shell.BringToFrontCommand.CanExecuteChanged += (_, _) => raised++;

        shell.State.SetSelection(ids.Skip(2));

        Assert.True(raised > 0);
        Assert.False(shell.BringToFrontCommand.CanExecute(null));
    }

    [Fact]
    public void Lock_SelectedShape_LocksItAndEnablesUnlockAll()
    {
        var (shell, ids) = Create(2);
        shell.State.SetSelection(ids.Take(1));
        Assert.False(shell.UnlockAllCommand.CanExecute(null));

        Run(shell.LockCommand);

        Assert.Equal(HistoryStepType.LockShapes, shell.State.History.Steps[^1]);
        Assert.False(shell.State.HasSelection);
        Assert.True(shell.UnlockAllCommand.CanExecute(null));

        Run(shell.UnlockAllCommand);

        Assert.Equal(HistoryStepType.UnlockAll, shell.State.History.Steps[^1]);
        Assert.False(shell.UnlockAllCommand.CanExecute(null));
    }

    [Fact]
    public void Hide_SelectedShape_HidesItAndEnablesShowAll()
    {
        var (shell, ids) = Create(2);
        shell.State.SetSelection(ids.Take(1));
        Assert.False(shell.ShowAllCommand.CanExecute(null));

        Run(shell.HideCommand);

        Assert.Equal(HistoryStepType.HideShapes, shell.State.History.Steps[^1]);
        Assert.True(shell.ShowAllCommand.CanExecute(null));

        Run(shell.ShowAllCommand);

        Assert.Equal(HistoryStepType.ShowAll, shell.State.History.Steps[^1]);
        Assert.True(shell.State.Document.FindShape(ids[0])!.IsEffectivelyVisible);
    }

    [Theory]
    [InlineData("Bring to Front", "Home")]
    [InlineData("Bring Forward", "PageUp")]
    [InlineData("Send Backward", "PageDown")]
    [InlineData("Send to Back", "End")]
    [InlineData("Group", "Ctrl+G")]
    [InlineData("Ungroup", "Ctrl+Shift+G")]
    [InlineData("Lock", "Ctrl+2")]
    [InlineData("Unlock All", "Ctrl+Alt+2")]
    [InlineData("Hide", "Ctrl+3")]
    [InlineData("Show All", "Ctrl+Alt+3")]
    public void ObjectMenu_Item_IsBoundToItsCommandAndShowsTheShortcut(string header, string gesture)
    {
        var (shell, _) = Create(2);

        var item = Item(shell, header);

        Assert.NotNull(item.Command);
        Assert.Equal(gesture, item.Gesture);
    }

    [Fact]
    public void ObjectMenu_ItemsFollowTheCommands()
    {
        var (shell, ids) = Create(2);
        shell.State.SetSelection(ids);

        Assert.True(Item(shell, "Group").IsEnabled);
        Assert.False(Item(shell, "Ungroup").IsEnabled);

        shell.State.ClearSelection();

        Assert.False(Item(shell, "Group").IsEnabled);
    }

    [Fact]
    public void UndoOfGroup_ReenablesGroupAndDisablesUngroup()
    {
        var (shell, ids) = Create(2);
        shell.State.SetSelection(ids);
        Run(shell.GroupCommand);

        shell.State.Undo();

        Assert.Equal(ids, shell.State.SelectedShapeIds);
        Assert.True(shell.GroupCommand.CanExecute(null));
        Assert.False(shell.UngroupCommand.CanExecute(null));
    }

    [Fact]
    public void QuickActions_GroupAndUngroupButtons_FollowTheSelection()
    {
        var (shell, ids) = Create(2);
        var properties = shell.Inspector.Properties;
        shell.State.SetSelection(ids.Take(1));
        var raised = 0;
        properties.GroupCommand.CanExecuteChanged += (_, _) => raised++;
        Assert.False(properties.GroupCommand.CanExecute(null));

        shell.State.SetSelection(ids);
        Assert.True(raised > 0);
        Run(properties.GroupCommand);

        Assert.Equal(HistoryStepType.GroupShapes, shell.State.History.Steps[^1]);
        Assert.True(properties.UngroupCommand.CanExecute(null));
        Assert.False(properties.GroupCommand.CanExecute(null));

        Run(properties.UngroupCommand);

        Assert.Equal(HistoryStepType.UngroupShapes, shell.State.History.Steps[^1]);
        Assert.False(properties.UngroupCommand.CanExecute(null));
    }
}
