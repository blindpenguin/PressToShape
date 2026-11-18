using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// Shows the settings inside the layout, for a platform without windows. The layout shows <see cref="Current"/> over the editor while it is not
/// <c>null</c>. Only one is shown at a time: a second request waits until the first is closed.
/// </summary>
internal sealed partial class SettingsDialogHost : ViewModelBase
{
    private readonly SemaphoreSlim _turn = new(1, 1);
    private TaskCompletionSource? _pending;

    /// <summary>The settings that are shown right now, or <c>null</c>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOpen))]
    public partial SettingsViewModel? Current { get; private set; }

    public bool IsOpen => Current is not null;

    /// <summary>Shows the settings and returns when they are closed, by OK, by Cancel or by <see cref="Dismiss"/>.</summary>
    public async Task ShowAsync(SettingsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        await _turn.WaitAsync();
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnCloseRequested(object? sender, bool ok) => pending.TrySetResult();
        try
        {
            _pending = pending;
            viewModel.CloseRequested += OnCloseRequested;
            Current = viewModel;
            await pending.Task;
        }
        finally
        {
            viewModel.CloseRequested -= OnCloseRequested;
            Current = null;
            _pending = null;
            _turn.Release();
        }
    }

    /// <summary>The Back button: goes from a page to the list, and cancels from the list. Does nothing if nothing is shown.</summary>
    public void Back() => Current?.BackCommand.Execute(null);

    /// <summary>Closes the settings without applying anything. Does nothing if nothing is shown.</summary>
    public void Dismiss() => _pending?.TrySetResult();
}
