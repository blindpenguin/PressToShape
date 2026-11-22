using System.Globalization;
using Avalonia.Controls;
using PressToShape.App.ViewModels;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views;
using PressToShape.App.Views.Desktop;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

// A dialog window listens to the close request of its current view model only, however often the DataContext is set.
[Collection(HeadlessUi.CollectionName)]
public class DialogWindowCloseTests
{
    private static MessageDialogViewModel Message() => MessageDialogViewModel.ForError(FakeLocalizer.Create(), "Oops");

    private static SettingsViewModel Settings() =>
        new(new SnapPreferences(new MemorySettingsStore(), true), FakeLocalizer.Create(), CultureInfo.GetCultureInfo("en"), true);

    private static int ClosedCount(Window window, System.Action act)
    {
        var closed = 0;
        window.Closed += (_, _) => closed++;
        act();
        return closed;
    }

    [Fact]
    public void MessageDialog_ThePreviousViewModelRequestsClose_DoesNotCloseTheWindow() => HeadlessUi.Run(() =>
    {
        var window = new MessageDialogWindow();
        var first = Message();
        var second = Message();
        window.DataContext = first;
        window.DataContext = second;
        window.Show();

        var closed = ClosedCount(window, () => first.Buttons[0].Command.Execute(null));

        Assert.Equal(0, closed);
        Assert.True(window.IsVisible);
        window.Close();
    });

    [Fact]
    public void MessageDialog_TheCurrentViewModelRequestsClose_ClosesTheWindowOnce() => HeadlessUi.Run(() =>
    {
        var window = new MessageDialogWindow();
        var current = Message();
        window.DataContext = Message();
        window.DataContext = current;
        window.DataContext = current;
        window.Show();

        var closed = ClosedCount(window, () => current.Buttons[0].Command.Execute(null));

        Assert.Equal(1, closed);
    });

    [Fact]
    public void MessageDialog_AfterTheWindowClosed_AnotherRequestIsIgnored() => HeadlessUi.Run(() =>
    {
        var window = new MessageDialogWindow();
        var current = Message();
        window.DataContext = current;
        window.Show();
        window.Close();

        var closed = ClosedCount(window, () => current.Buttons[0].Command.Execute(null));

        Assert.Equal(0, closed);
    });

    [Fact]
    public void Settings_ThePreviousViewModelRequestsClose_DoesNotCloseTheWindow() => HeadlessUi.Run(() =>
    {
        var window = new SettingsWindow();
        var first = Settings();
        var second = Settings();
        window.DataContext = first;
        window.DataContext = second;
        window.Show();

        var closed = ClosedCount(window, () => first.CancelCommand.Execute(null));

        Assert.Equal(0, closed);
        Assert.True(window.IsVisible);
        window.Close();
    });

    [Fact]
    public void Settings_TheCurrentViewModelRequestsClose_ClosesTheWindowOnce() => HeadlessUi.Run(() =>
    {
        var window = new SettingsWindow();
        var current = Settings();
        window.DataContext = Settings();
        window.DataContext = current;
        window.DataContext = current;
        window.Show();

        var closed = ClosedCount(window, () => current.CancelCommand.Execute(null));

        Assert.Equal(1, closed);
    });
}
