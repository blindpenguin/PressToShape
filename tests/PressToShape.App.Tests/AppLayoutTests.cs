using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Views;
using PressToShape.App.Views.Tablet;
using Xunit;

namespace PressToShape.App.Tests;

// The application class chooses the layout by the kind of application. The choice takes its dependencies as arguments, so these tests use fakes
// and never touch the user's settings file. They create real windows and views, so they run with the other headless UI tests.
// Avalonia does not allow its Android lifetime to be faked, so that branch is tested through the factory it installs (TabletFactory) and checked
// on the device.
[Collection(HeadlessUi.CollectionName)]
public class AppLayoutTests
{
    // The task is finished by the UI thread, so blocking that thread to wait for it would never end. The dispatcher runs its jobs instead.
    private static T Wait<T>(Task<T> task)
    {
        for (var i = 0; i < 1000 && !task.IsCompleted; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        Assert.True(task.IsCompleted, "The task did not finish.");
        return task.Result;
    }

    private static TabletShellViewModel CreateTablet(Func<Action?> exit, PressToShape.App.Settings.ISettingsStore? settings = null)
    {
        var factory = AppLayout.TabletFactory(settings ?? new MemorySettingsStore(), FakeLocalizer.Create(), exit);
        var view = Assert.IsType<TabletShellView>(factory());
        return Assert.IsType<TabletShellViewModel>(view.DataContext);
    }

    [Fact]
    public void AnApplicationWithAWindow_GetsTheDesktopLayout() => HeadlessUi.Run(() =>
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();

        AppLayout.Choose(lifetime, new MemorySettingsStore(), FakeLocalizer.Create(), () => null);

        var window = Assert.IsType<MainWindow>(lifetime.MainWindow);
        Assert.IsType<DesktopShellViewModel>(window.DataContext);
    });

    [Fact]
    public void WithoutALifetime_NothingHappens() => HeadlessUi.Run(() =>
    {
        AppLayout.Choose(null, new MemorySettingsStore(), FakeLocalizer.Create(), () => null);
    });

    [Fact]
    public void TheTabletFactory_BuildsTheTabletLayout_OnlyWhenCalled() => HeadlessUi.Run(() =>
    {
        var exitReads = 0;
        var factory = AppLayout.TabletFactory(new MemorySettingsStore(), FakeLocalizer.Create(), () =>
        {
            exitReads++;
            return null;
        });

        Assert.Equal(0, exitReads);

        var view = Assert.IsType<TabletShellView>(factory());
        Assert.IsType<TabletShellViewModel>(view.DataContext);
    });

    [Fact]
    public void EveryCallOfTheFactory_GivesAnIndependentLayout() => HeadlessUi.Run(() =>
    {
        var factory = AppLayout.TabletFactory(new MemorySettingsStore(), FakeLocalizer.Create(), () => null);

        var first = (TabletShellView)factory();
        var second = (TabletShellView)factory();

        Assert.NotSame(first, second);
        Assert.NotSame(first.DataContext, second.DataContext);
    });

    [Fact]
    public void LeavingTheTabletLayout_CallsTheExitThatTheHeadSetLate() => HeadlessUi.Run(() =>
    {
        Action? exit = null;
        var exits = 0;
        var shell = CreateTablet(() => exit);

        exit = () => exits++; // the Android head sets it after the layout exists
        var left = Wait(shell.RequestLeaveAsync());

        Assert.True(left);
        Assert.Equal(1, exits);
    });

    [Fact]
    public void LeavingBeforeTheHeadSetTheExit_DoesNotCrash() => HeadlessUi.Run(() =>
    {
        var shell = CreateTablet(() => null);

        var left = Wait(shell.RequestLeaveAsync());

        Assert.True(left); // the app was told to end, there was only nobody to hear it
    });

    [Fact]
    public void TheTabletLayout_ShowsItsDialogsInsideTheLayout_NotInAWindow() => HeadlessUi.Run(() =>
    {
        var shell = CreateTablet(() => null);
        shell.State.Edit(
            PressToShape.Core.History.HistoryStepType.DrawRectangle,
            d => d.AddShape(shell.State.ActiveLayerId, new PressToShape.Core.Shapes.RectangleShape { Width = 5, Height = 5 }));

        var leaving = shell.RequestLeaveAsync();

        Assert.True(shell.Dialogs.IsOpen);
        Assert.Equal("Unsaved changes", shell.Dialogs.Current!.Title);
        shell.Dialogs.Dismiss();
        Assert.False(Wait(leaving));
    });

    [Fact]
    public void TheTabletLayout_UsesTheGivenSettings_ForTheLanguageAndNotTheRealFile() => HeadlessUi.Run(() =>
    {
        var settings = new MemorySettingsStore();

        var shell = CreateTablet(() => null, settings);

        Assert.NotNull(shell);
        Assert.Null(settings.Current.RecentFiles); // nothing was kept: recent files are off on Android
    });

    [Fact]
    public void NullArguments_Throw() => HeadlessUi.Run(() =>
    {
        Assert.Throws<ArgumentNullException>(() => AppLayout.Choose(null, null!, FakeLocalizer.Create(), () => null));
        Assert.Throws<ArgumentNullException>(() => AppLayout.Choose(null, new MemorySettingsStore(), null!, () => null));
        Assert.Throws<ArgumentNullException>(() => AppLayout.Choose(null, new MemorySettingsStore(), FakeLocalizer.Create(), null!));
        Assert.Throws<ArgumentNullException>(() => AppLayout.TabletFactory(null!, FakeLocalizer.Create(), () => null));
        Assert.Throws<ArgumentNullException>(() => AppLayout.TabletFactory(new MemorySettingsStore(), null!, () => null));
        Assert.Throws<ArgumentNullException>(() => AppLayout.TabletFactory(new MemorySettingsStore(), FakeLocalizer.Create(), null!));
    });
}
