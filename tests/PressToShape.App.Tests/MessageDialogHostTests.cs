using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.ViewModels;
using PressToShape.App.ViewModels.Tablet;
using Xunit;

namespace PressToShape.App.Tests;

public class MessageDialogHostTests
{
    private static MessageDialogViewModel Question(string name = "a.p2s") =>
        MessageDialogViewModel.ForUnsavedChanges(FakeLocalizer.Create(), name);

    /// <summary>Completes when the host shows the given dialog. Subscribes first, so it cannot miss the change.</summary>
    private static Task WaitUntilShown(MessageDialogHost host, MessageDialogViewModel dialog)
    {
        var shown = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ReferenceEquals(host.Current, dialog))
            {
                shown.TrySetResult();
            }
        }

        host.PropertyChanged += OnChanged;
        if (ReferenceEquals(host.Current, dialog))
        {
            shown.TrySetResult();
        }

        return shown.Task;
    }

    [Fact]
    public void NewHost_ShowsNothing()
    {
        var host = new MessageDialogHost();

        Assert.Null(host.Current);
        Assert.False(host.IsOpen);
    }

    [Fact]
    public async Task ShowAsync_ShowsTheDialog_AndCompletesWithTheChosenResult()
    {
        var host = new MessageDialogHost();
        var dialog = Question();

        var task = host.ShowAsync(dialog);

        Assert.Same(dialog, host.Current);
        Assert.True(host.IsOpen);
        Assert.False(task.IsCompleted);

        dialog.Buttons[1].Command.Execute(null);

        Assert.Equal(dialog.Buttons[1].Result, await task);
    }

    [Fact]
    public async Task ShowAsync_AfterTheResult_ClosesTheDialogAndNotifies()
    {
        var host = new MessageDialogHost();
        var dialog = Question();
        var raised = new List<string?>();
        host.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        var task = host.ShowAsync(dialog);
        dialog.Buttons[0].Command.Execute(null);
        await task;

        Assert.Null(host.Current);
        Assert.False(host.IsOpen);
        Assert.Contains(nameof(MessageDialogHost.Current), raised);
        Assert.Contains(nameof(MessageDialogHost.IsOpen), raised);
    }

    [Fact]
    public async Task SecondRequest_WaitsUntilTheFirstIsClosed()
    {
        var host = new MessageDialogHost();
        var first = Question("first.p2s");
        var second = Question("second.p2s");

        var firstTask = host.ShowAsync(first);
        var secondTask = host.ShowAsync(second);

        Assert.Same(first, host.Current);
        Assert.False(secondTask.IsCompleted);

        var secondShown = WaitUntilShown(host, second);
        first.Buttons[0].Command.Execute(null);
        await firstTask;
        await secondShown;

        Assert.Same(second, host.Current);
        second.Buttons[2].Command.Execute(null);
        Assert.Equal(second.Buttons[2].Result, await secondTask);
        Assert.Null(host.Current);
    }

    [Fact]
    public async Task Dismiss_ClosesTheDialogWithoutAChoice()
    {
        var host = new MessageDialogHost();
        var task = host.ShowAsync(Question());

        host.Dismiss();

        Assert.Null(await task);
        Assert.Null(host.Current);
    }

    [Fact]
    public void Dismiss_WithoutADialog_DoesNothing()
    {
        var host = new MessageDialogHost();

        host.Dismiss();

        Assert.Null(host.Current);
    }

    [Fact]
    public async Task ADismissedDialog_CannotCompleteTheNextOne()
    {
        var host = new MessageDialogHost();
        var first = Question("first.p2s");
        var firstTask = host.ShowAsync(first);
        host.Dismiss();
        await firstTask;

        var second = Question("second.p2s");
        var secondTask = host.ShowAsync(second);
        first.Buttons[0].Command.Execute(null);

        Assert.False(secondTask.IsCompleted);
        second.Buttons[1].Command.Execute(null);
        Assert.Equal(second.Buttons[1].Result, await secondTask);
    }

    [Fact]
    public async Task AfterADialogIsClosed_TheNextRequestShowsImmediately()
    {
        var host = new MessageDialogHost();
        var first = Question();
        var firstTask = host.ShowAsync(first);
        first.Buttons[0].Command.Execute(null);
        await firstTask;

        var second = Question();
        var secondTask = host.ShowAsync(second);

        Assert.Same(second, host.Current);
        second.Buttons.Single(b => b.IsCancel).Command.Execute(null);
        await secondTask;
    }

    [Fact]
    public async Task ShowAsync_NullDialog_Throws()
    {
        var host = new MessageDialogHost();

        await Assert.ThrowsAsync<ArgumentNullException>(() => host.ShowAsync(null!));
    }
}
