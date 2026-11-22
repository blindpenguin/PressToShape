using System;
using Avalonia.Threading;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.Views;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

// The close handler of the window is an async void method, where an exception would end the app.
[Collection(HeadlessUi.CollectionName)]
public class MainWindowCloseTests
{
    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static (MainWindow Window, DesktopShellViewModel Shell, FakeMessageDialogs Messages) Show()
    {
        var messages = new FakeMessageDialogs();
        var shell = new DesktopShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create(messages: messages));
        shell.State.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(shell.State.ActiveLayerId, new RectangleShape { Width = 10, Height = 10 }));
        var window = new MainWindow { DataContext = shell };
        window.Show();
        return (window, shell, messages);
    }

    [Fact]
    public void Close_TheUnsavedQuestionFails_KeepsTheWindowOpenAndShowsAnError() => HeadlessUi.Run(() =>
    {
        var (window, _, messages) = Show();
        messages.AskFailure = new InvalidOperationException("broken");
        var closed = false;
        window.Closed += (_, _) => closed = true;

        window.Close();
        for (var i = 0; i < 100; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        Assert.False(closed);
        Assert.True(window.IsVisible);
        Assert.Equal(["The operation could not be done."], messages.Errors);
        window.Hide();
    });

    [Fact]
    public void Close_TheUserDiscards_ClosesTheWindow() => HeadlessUi.Run(() =>
    {
        var (window, _, messages) = Show();
        messages.Answer = UnsavedChoice.DontSave;
        var closed = false;
        window.Closed += (_, _) => closed = true;

        window.Close();
        for (var i = 0; i < 100 && !closed; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        Assert.True(closed);
        Assert.Empty(messages.Errors);
    });
}
