using System;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Tablet;
using Xunit;

namespace PressToShape.App.Tests;

public class OverlayMessageDialogsTests
{
    private readonly MessageDialogHost _host = new();
    private readonly OverlayMessageDialogs _dialogs;

    public OverlayMessageDialogsTests() => _dialogs = new OverlayMessageDialogs(_host, FakeLocalizer.Create());

    [Theory]
    [InlineData(0, (int)UnsavedChoice.Save)]
    [InlineData(1, (int)UnsavedChoice.DontSave)]
    [InlineData(2, (int)UnsavedChoice.Cancel)]
    public async Task AskUnsavedChanges_GivesTheChoiceOfTheButton(int button, int expected)
    {
        var task = _dialogs.AskUnsavedChangesAsync("plan.p2s");

        Assert.NotNull(_host.Current);
        _host.Current.Buttons[button].Command.Execute(null);

        Assert.Equal((UnsavedChoice)expected, await task);
    }

    [Fact]
    public async Task TheDialogsInsideTheLayout_AreNotCancelledByEscape_BecauseTheBackRequestDoesThat()
    {
        var question = _dialogs.AskUnsavedChangesAsync("plan.p2s");
        Assert.DoesNotContain(_host.Current!.Buttons, b => b.IsCancel);
        _host.Dismiss();
        await question;

        var error = _dialogs.ShowErrorAsync("Oops");
        Assert.DoesNotContain(_host.Current!.Buttons, b => b.IsCancel);
        _host.Dismiss();
        await error;
    }

    [Fact]
    public void TheWindowDialogs_StillCancelOnEscape()
    {
        var question = PressToShape.App.ViewModels.MessageDialogViewModel.ForUnsavedChanges(FakeLocalizer.Create(), "plan.p2s");
        var error = PressToShape.App.ViewModels.MessageDialogViewModel.ForError(FakeLocalizer.Create(), "Oops");

        Assert.Equal(new[] { "Cancel" }, question.Buttons.Where(b => b.IsCancel).Select(b => b.Text));
        Assert.Single(error.Buttons, b => b.IsCancel);
    }

    [Fact]
    public async Task AskUnsavedChanges_ShowsTheSameTextAsTheWindowDialog()
    {
        var task = _dialogs.AskUnsavedChangesAsync("plan.p2s");

        Assert.Equal("Unsaved changes", _host.Current!.Title);
        Assert.Equal("“plan.p2s” has unsaved changes. Save them before continuing?", _host.Current.Message);
        Assert.Equal(new[] { "Save", "Don't save", "Cancel" }, _host.Current.Buttons.Select(b => b.Text));

        _host.Dismiss();
        await task;
    }

    [Fact]
    public async Task AskUnsavedChanges_DismissedWithoutAChoice_CountsAsCancel()
    {
        var task = _dialogs.AskUnsavedChangesAsync("plan.p2s");

        _host.Dismiss();

        Assert.Equal(UnsavedChoice.Cancel, await task);
    }

    [Fact]
    public async Task ShowError_ShowsTheMessage_AndCompletesWhenOkIsChosen()
    {
        var task = _dialogs.ShowErrorAsync("Could not save “a”. Reason");

        Assert.Equal("Could not save “a”. Reason", _host.Current!.Message);
        Assert.False(task.IsCompleted);

        _host.Current.Buttons.Single().Command.Execute(null);
        await task;

        Assert.Null(_host.Current);
    }

    [Fact]
    public async Task AQuestionAfterAnError_WaitsForTheError()
    {
        var error = _dialogs.ShowErrorAsync("first");
        var question = _dialogs.AskUnsavedChangesAsync("a.p2s");

        Assert.Equal("first", _host.Current!.Message);
        Assert.False(question.IsCompleted);

        var questionShown = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _host.PropertyChanged += (_, _) =>
        {
            if (_host.Current?.Message.Contains("a.p2s", StringComparison.Ordinal) == true)
            {
                questionShown.TrySetResult();
            }
        };

        _host.Current.Buttons.Single().Command.Execute(null);
        await error;
        await questionShown.Task;

        _host.Dismiss();
        Assert.Equal(UnsavedChoice.Cancel, await question);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new OverlayMessageDialogs(null!, FakeLocalizer.Create()));
        Assert.Throws<ArgumentNullException>(() => new OverlayMessageDialogs(_host, null!));
    }
}
