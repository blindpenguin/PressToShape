using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;

namespace PressToShape.App.ViewModels;

/// <summary>
/// The content of a small modal dialog: a title, a message and a row of buttons. It holds no window code.
/// Choosing a button raises <see cref="CloseRequested"/> with that button's result.
/// The texts are translated once when the dialog is created, because the dialog is short-lived.
/// </summary>
internal sealed class MessageDialogViewModel
{
    private MessageDialogViewModel(string title, string message, Func<Action<int>, IReadOnlyList<MessageDialogButtonViewModel>> createButtons)
    {
        Title = title;
        Message = message;
        Buttons = createButtons(Choose);
    }

    /// <summary>Raised when a button was chosen. The value is the button's result.</summary>
    public event EventHandler<int>? CloseRequested;

    public string Title { get; }

    public string Message { get; }

    public IReadOnlyList<MessageDialogButtonViewModel> Buttons { get; }

    /// <summary>The result that counts when the dialog is closed without choosing a button.</summary>
    public int CancelResult { get; private init; }

    /// <summary>The question about unsaved changes. The results are the <see cref="UnsavedChoice"/> values.</summary>
    public static MessageDialogViewModel ForUnsavedChanges(ILocalizer localizer, string documentName)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(documentName);

        var message = string.Format(localizer.Culture, localizer.T("“{0}” has unsaved changes. Save them before continuing?"), documentName);
        var save = localizer.T("Save");
        var dontSave = localizer.T("Don't save");
        var cancel = localizer.T("Cancel");
        return new MessageDialogViewModel(
            localizer.T("Unsaved changes"),
            message,
            choose => [
                Button(save, (int)UnsavedChoice.Save, isDefault: true, isCancel: false, choose),
                Button(dontSave, (int)UnsavedChoice.DontSave, isDefault: false, isCancel: false, choose),
                Button(cancel, (int)UnsavedChoice.Cancel, isDefault: false, isCancel: true, choose),
            ])
        {
            CancelResult = (int)UnsavedChoice.Cancel,
        };
    }

    /// <summary>An error message with a single OK button.</summary>
    public static MessageDialogViewModel ForError(ILocalizer localizer, string message)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(message);

        var ok = localizer.T("OK");
        return new MessageDialogViewModel(
            localizer.T("Error"),
            message,
            choose => [Button(ok, 0, isDefault: true, isCancel: true, choose)])
        {
            CancelResult = 0,
        };
    }

    private static MessageDialogButtonViewModel Button(string text, int result, bool isDefault, bool isCancel, Action<int> choose) =>
        new(text, result, isDefault, isCancel, new RelayCommand(() => choose(result)));

    private void Choose(int result) => CloseRequested?.Invoke(this, result);
}
