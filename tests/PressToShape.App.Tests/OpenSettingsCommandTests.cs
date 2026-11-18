using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class OpenSettingsCommandTests
{
    private sealed class BareShell(ILocalizer localizer, FileServices services) : EditorShellViewModel(localizer, services);

    private static (BareShell Shell, FakeSettingsDialogs Dialogs, MemorySettingsStore Store) Create(bool withDialogs = true)
    {
        var dialogs = new FakeSettingsDialogs();
        var store = new MemorySettingsStore();
        var shell = new BareShell(FakeLocalizer.Create(), TestFileServices.Create(settings: store, settingsDialogs: withDialogs ? dialogs : null));
        return (shell, dialogs, store);
    }

    [Fact]
    public async Task OpenSettingsCommand_Ok_AppliesTheValues()
    {
        var (shell, dialogs, _) = Create();

        var running = shell.OpenSettingsCommand.ExecuteAsync(null);
        dialogs.Shown!.ShowGrid = true;
        dialogs.Shown.SnapToObjects = true;
        Assert.False(shell.State.Snapping.ShowGrid);
        dialogs.Close(ok: true);
        await running;

        Assert.True(shell.State.Snapping.ShowGrid);
        Assert.True(shell.State.Snapping.SnapToObjects);
    }

    [Fact]
    public async Task OpenSettingsCommand_Cancel_AppliesNothing()
    {
        var (shell, dialogs, store) = Create();

        var running = shell.OpenSettingsCommand.ExecuteAsync(null);
        dialogs.Shown!.ShowGrid = true;
        dialogs.Close(ok: false);
        await running;

        Assert.False(shell.State.Snapping.ShowGrid);
        Assert.Null(store.Load().ShowGrid);
    }

    [Fact]
    public async Task OpenSettingsCommand_ClosedWithoutChoice_AppliesNothing()
    {
        var (shell, dialogs, _) = Create();

        var running = shell.OpenSettingsCommand.ExecuteAsync(null);
        dialogs.Shown!.ShowGrid = true;
        dialogs.CloseWithoutChoice();
        await running;

        Assert.False(shell.State.Snapping.ShowGrid);
    }

    [Fact]
    public async Task OpenSettingsCommand_AlreadyOpen_IgnoresASecondCall()
    {
        var (shell, dialogs, _) = Create();

        var first = shell.OpenSettingsCommand.ExecuteAsync(null);
        Assert.False(shell.OpenSettingsCommand.CanExecute(null));
        shell.OpenSettingsCommand.Execute(null);
        await shell.OpenSettingsCommand.ExecuteAsync(null);

        Assert.Equal(1, dialogs.ShowCount);

        dialogs.Close(ok: false);
        await first;
        Assert.True(shell.OpenSettingsCommand.CanExecute(null));
    }

    [Fact]
    public async Task OpenSettingsCommand_AfterClosing_CanOpenAgainWithTheCurrentValues()
    {
        var (shell, dialogs, _) = Create();
        var first = shell.OpenSettingsCommand.ExecuteAsync(null);
        dialogs.Shown!.ShowGrid = true;
        dialogs.Close(ok: true);
        await first;

        var second = shell.OpenSettingsCommand.ExecuteAsync(null);

        Assert.Equal(2, dialogs.ShowCount);
        Assert.True(dialogs.Shown.ShowGrid);
        dialogs.Close(ok: false);
        await second;
    }

    [Fact]
    public async Task OpenSettingsCommand_DialogThrows_ResetsTheFlag()
    {
        var (shell, dialogs, _) = Create();
        dialogs.Throws = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => shell.OpenSettingsCommand.ExecuteAsync(null));

        Assert.True(shell.OpenSettingsCommand.CanExecute(null));
    }

    [Fact]
    public void OpenSettingsCommand_NoSettingsDialogs_CannotRun()
    {
        var (shell, _, _) = Create(withDialogs: false);

        Assert.False(shell.OpenSettingsCommand.CanExecute(null));
    }
}
