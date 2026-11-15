using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Services;
using PressToShape.App.ViewModels;
using Xunit;

namespace PressToShape.App.Tests;

public class MessageDialogViewModelTests
{
    [Fact]
    public void ForUnsavedChanges_HasSaveDontSaveAndCancelInThatOrder()
    {
        var dialog = MessageDialogViewModel.ForUnsavedChanges(FakeLocalizer.Create(), "plan.p2s");

        Assert.Equal(new[] { "Save", "Don't save", "Cancel" }, dialog.Buttons.Select(b => b.Text));
        Assert.Equal(
            new[] { (int)UnsavedChoice.Save, (int)UnsavedChoice.DontSave, (int)UnsavedChoice.Cancel },
            dialog.Buttons.Select(b => b.Result));
    }

    [Fact]
    public void ForUnsavedChanges_SaveIsTheDefaultAndCancelIsTheEscapeButton()
    {
        var dialog = MessageDialogViewModel.ForUnsavedChanges(FakeLocalizer.Create(), "plan.p2s");

        Assert.Equal(new[] { true, false, false }, dialog.Buttons.Select(b => b.IsDefault));
        Assert.Equal(new[] { false, false, true }, dialog.Buttons.Select(b => b.IsCancel));
        Assert.Equal((int)UnsavedChoice.Cancel, dialog.CancelResult);
    }

    [Fact]
    public void ForUnsavedChanges_MessageNamesTheDocument()
    {
        var dialog = MessageDialogViewModel.ForUnsavedChanges(FakeLocalizer.Create(), "plan.p2s");

        Assert.Equal("Unsaved changes", dialog.Title);
        Assert.Equal("“plan.p2s” has unsaved changes. Save them before continuing?", dialog.Message);
    }

    [Fact]
    public void ForUnsavedChanges_UsesTheCurrentLanguage()
    {
        var german = new Dictionary<string, string>
        {
            ["Save"] = "Speichern",
            ["Don't save"] = "Nicht speichern",
            ["Cancel"] = "Abbrechen",
            ["Unsaved changes"] = "Nicht gespeicherte Änderungen",
            ["“{0}” has unsaved changes. Save them before continuing?"] = "„{0}“ hat Änderungen. Speichern?",
        };

        var dialog = MessageDialogViewModel.ForUnsavedChanges(FakeLocalizer.Create("de", german), "a.p2s");

        Assert.Equal(new[] { "Speichern", "Nicht speichern", "Abbrechen" }, dialog.Buttons.Select(b => b.Text));
        Assert.Equal("„a.p2s“ hat Änderungen. Speichern?", dialog.Message);
        Assert.Equal("Nicht gespeicherte Änderungen", dialog.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ChoosingAButton_RaisesCloseRequestedWithItsResult(int index)
    {
        var dialog = MessageDialogViewModel.ForUnsavedChanges(FakeLocalizer.Create(), "a.p2s");
        var results = new List<int>();
        dialog.CloseRequested += (_, result) => results.Add(result);

        dialog.Buttons[index].Command.Execute(null);

        Assert.Equal(new[] { dialog.Buttons[index].Result }, results);
    }

    [Fact]
    public void ForError_HasASingleDefaultAndCancelOkButton()
    {
        var dialog = MessageDialogViewModel.ForError(FakeLocalizer.Create(), "Could not save “a”. Reason");

        var ok = Assert.Single(dialog.Buttons);
        Assert.Equal("OK", ok.Text);
        Assert.True(ok.IsDefault);
        Assert.True(ok.IsCancel);
        Assert.Equal("Error", dialog.Title);
        Assert.Equal("Could not save “a”. Reason", dialog.Message);
        Assert.Equal(ok.Result, dialog.CancelResult);
    }

    [Fact]
    public void ForError_ChoosingOk_RaisesCloseRequested()
    {
        var dialog = MessageDialogViewModel.ForError(FakeLocalizer.Create(), "x");
        var raised = 0;
        dialog.CloseRequested += (_, _) => raised++;

        dialog.Buttons[0].Command.Execute(null);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Factories_NullArguments_Throw()
    {
        var localizer = FakeLocalizer.Create();

        Assert.Throws<ArgumentNullException>(() => MessageDialogViewModel.ForUnsavedChanges(null!, "a"));
        Assert.Throws<ArgumentNullException>(() => MessageDialogViewModel.ForUnsavedChanges(localizer, null!));
        Assert.Throws<ArgumentNullException>(() => MessageDialogViewModel.ForError(null!, "a"));
        Assert.Throws<ArgumentNullException>(() => MessageDialogViewModel.ForError(localizer, null!));
    }
}
