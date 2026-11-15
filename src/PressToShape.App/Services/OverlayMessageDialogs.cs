using System;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels;
using PressToShape.App.ViewModels.Tablet;

namespace PressToShape.App.Services;

/// <summary>
/// The questions and messages for a platform without windows: they are shown inside the layout through a <see cref="MessageDialogHost"/>.
/// The texts and the results are the same as in <see cref="AvaloniaMessageDialogs"/>.
/// </summary>
internal sealed class OverlayMessageDialogs : IMessageDialogs
{
    private readonly MessageDialogHost _host;
    private readonly ILocalizer _localizer;

    public OverlayMessageDialogs(MessageDialogHost host, ILocalizer localizer)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    public async Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName)
    {
        var dialog = MessageDialogViewModel.ForUnsavedChanges(_localizer, documentName, escapeCancels: false);
        var result = await _host.ShowAsync(dialog);

        // A dialog that was closed without a choice counts as Cancel: nothing is lost.
        return (UnsavedChoice)(result ?? dialog.CancelResult);
    }

    public async Task ShowErrorAsync(string message)
    {
        await _host.ShowAsync(MessageDialogViewModel.ForError(_localizer, message, escapeCancels: false));
    }
}
