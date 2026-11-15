using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels;
using PressToShape.App.Views;

namespace PressToShape.App.Services;

/// <summary>Shows the questions and messages as modal dialogs over the main window.</summary>
internal sealed class AvaloniaMessageDialogs : IMessageDialogs
{
    private readonly Func<Window?> _owner;
    private readonly ILocalizer _localizer;

    public AvaloniaMessageDialogs(Func<Window?> owner, ILocalizer localizer)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    public async Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName)
    {
        var viewModel = MessageDialogViewModel.ForUnsavedChanges(_localizer, documentName);
        var result = await ShowAsync(viewModel);

        // Without an owner window nothing can be shown, and no answer counts as Cancel: nothing is lost.
        return (UnsavedChoice)(result ?? viewModel.CancelResult);
    }

    public async Task ShowErrorAsync(string message)
    {
        await ShowAsync(MessageDialogViewModel.ForError(_localizer, message));
    }

    private async Task<int?> ShowAsync(MessageDialogViewModel viewModel)
    {
        if (_owner() is not { } owner)
        {
            return null;
        }

        var window = new MessageDialogWindow { DataContext = viewModel };
        return await window.ShowDialog<int?>(owner);
    }
}
