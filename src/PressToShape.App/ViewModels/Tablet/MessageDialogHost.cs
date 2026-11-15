using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// Shows message dialogs inside the layout, for a platform without windows. The layout shows <see cref="Current"/> over everything else
/// while it is not <c>null</c>. Only one dialog is shown at a time: a second request waits until the first is closed.
/// </summary>
internal sealed partial class MessageDialogHost : ViewModelBase
{
    private readonly SemaphoreSlim _turn = new(1, 1);
    private TaskCompletionSource<int?>? _pending;

    /// <summary>The dialog that is shown right now, or <c>null</c>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOpen))]
    public partial MessageDialogViewModel? Current { get; private set; }

    public bool IsOpen => Current is not null;

    /// <summary>Shows a dialog and waits for the result of the button that was chosen. Gives <c>null</c> if the dialog was dismissed without a choice.</summary>
    public async Task<int?> ShowAsync(MessageDialogViewModel dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        await _turn.WaitAsync();
        var pending = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnCloseRequested(object? sender, int result) => pending.TrySetResult(result);
        try
        {
            _pending = pending;
            dialog.CloseRequested += OnCloseRequested;
            Current = dialog;
            return await pending.Task;
        }
        finally
        {
            dialog.CloseRequested -= OnCloseRequested;
            Current = null;
            _pending = null;
            _turn.Release();
        }
    }

    /// <summary>Closes the dialog that is shown without a choice, for example when the Back button is pressed. Does nothing if there is none.</summary>
    public void Dismiss() => _pending?.TrySetResult(null);
}
