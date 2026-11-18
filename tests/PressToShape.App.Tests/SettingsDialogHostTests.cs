using System.Globalization;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SettingsDialogHostTests
{
    private sealed class FakeShutdown : IAppShutdown
    {
        public int Calls { get; private set; }

        public void Shutdown() => Calls++;
    }

    private static SettingsViewModel NewSettings() =>
        new(SnapPreferences.InMemory(), FakeLocalizer.Create(), CultureInfo.GetCultureInfo("en"), defaultShowRulers: false);

    private static (TabletShellViewModel Shell, SettingsDialogHost Host, FakeShutdown Shutdown) CreateShell()
    {
        var host = new SettingsDialogHost();
        var shutdown = new FakeShutdown();
        var services = TestFileServices.Create(settingsDialogs: new OverlaySettingsDialogs(host));
        var shell = new TabletShellViewModel(shutdown, FakeLocalizer.Create(), services, new MessageDialogHost(), host);
        return (shell, host, shutdown);
    }

    [Fact]
    public async Task ShowAsync_ViewModelClosesWithOk_ReturnsAndHidesTheSettings()
    {
        var host = new SettingsDialogHost();
        var settings = NewSettings();

        var showing = host.ShowAsync(settings);

        Assert.True(host.IsOpen);
        Assert.Same(settings, host.Current);
        settings.OkCommand.Execute(null);
        await showing;
        Assert.False(host.IsOpen);
        Assert.Null(host.Current);
    }

    [Fact]
    public async Task ShowAsync_ViewModelCancels_ReturnsAndHidesTheSettings()
    {
        var host = new SettingsDialogHost();
        var settings = NewSettings();

        var showing = host.ShowAsync(settings);
        settings.CancelCommand.Execute(null);
        await showing;

        Assert.False(host.IsOpen);
    }

    [Fact]
    public async Task Dismiss_SettingsOpen_ClosesThemWithoutAChoice()
    {
        var host = new SettingsDialogHost();
        var settings = NewSettings();
        var raised = false;
        settings.CloseRequested += (_, _) => raised = true;

        var showing = host.ShowAsync(settings);
        host.Dismiss();
        await showing;

        Assert.False(host.IsOpen);
        Assert.False(raised);
    }

    [Fact]
    public void Dismiss_NothingOpen_DoesNothing()
    {
        var host = new SettingsDialogHost();

        host.Dismiss();
        host.Back();

        Assert.False(host.IsOpen);
    }

    [Fact]
    public async Task ShowAsync_SecondRequest_WaitsForTheFirst()
    {
        var host = new SettingsDialogHost();
        var first = NewSettings();
        var second = NewSettings();

        var showingFirst = host.ShowAsync(first);
        var showingSecond = host.ShowAsync(second);

        Assert.Same(first, host.Current);
        Assert.False(showingSecond.IsCompleted);

        first.CancelCommand.Execute(null);
        await showingFirst;
        await Task.Yield();

        Assert.Same(second, host.Current);
        second.CancelCommand.Execute(null);
        await showingSecond;
        Assert.False(host.IsOpen);
    }

    [Fact]
    public async Task Back_OnAPage_GoesToTheListAndKeepsTheSettingsOpen()
    {
        var host = new SettingsDialogHost();
        var settings = NewSettings();
        var showing = host.ShowAsync(settings);
        settings.OpenCategoryCommand.Execute(settings.Categories[1]);

        host.Back();

        Assert.False(settings.IsPageOpen);
        Assert.True(host.IsOpen);

        host.Back();
        await showing;
        Assert.False(host.IsOpen);
    }

    [Fact]
    public async Task RequestBackAsync_SettingsOnAPage_GoesToTheListAndDoesNotLeave()
    {
        var (shell, host, shutdown) = CreateShell();
        var running = shell.OpenSettingsCommand.ExecuteAsync(null);
        host.Current!.OpenCategoryCommand.Execute(host.Current.Categories[2]);

        var left = await shell.RequestBackAsync();

        Assert.False(left);
        Assert.True(host.IsOpen);
        Assert.False(host.Current!.IsPageOpen);
        Assert.Equal(0, shutdown.Calls);

        await shell.RequestBackAsync();
        await running;
        Assert.False(host.IsOpen);
        Assert.Equal(0, shutdown.Calls);
    }

    [Fact]
    public async Task RequestBackAsync_SettingsOnTheList_CancelsWithoutApplying()
    {
        var (shell, host, _) = CreateShell();
        var running = shell.OpenSettingsCommand.ExecuteAsync(null);
        host.Current!.ShowGrid = true;

        await shell.RequestBackAsync();
        await running;

        Assert.False(host.IsOpen);
        Assert.False(shell.State.Snapping.ShowGrid);
    }

    [Fact]
    public async Task RequestLeaveAsync_SettingsOpen_ClosesThemAndStays()
    {
        var (shell, host, shutdown) = CreateShell();
        var running = shell.OpenSettingsCommand.ExecuteAsync(null);

        var left = await shell.RequestLeaveAsync();
        await running;

        Assert.False(left);
        Assert.False(host.IsOpen);
        Assert.Equal(0, shutdown.Calls);
    }

    [Fact]
    public async Task OverlaySettingsDialogs_Ok_AppliesTheValuesThroughTheShell()
    {
        var (shell, host, _) = CreateShell();
        var running = shell.OpenSettingsCommand.ExecuteAsync(null);
        host.Current!.SnapToGrid = true;

        host.Current.OkCommand.Execute(null);
        await running;

        Assert.True(shell.State.Snapping.SnapToGrid);
        Assert.False(host.IsOpen);
    }
}
