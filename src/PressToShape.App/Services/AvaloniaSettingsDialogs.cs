using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Desktop;

namespace PressToShape.App.Services;

/// <summary>Shows the settings as a modal window over the main window.</summary>
internal sealed class AvaloniaSettingsDialogs : ISettingsDialogs
{
    private readonly Func<Window?> _owner;

    public AvaloniaSettingsDialogs(Func<Window?> owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public async Task ShowAsync(SettingsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        // Without an owner window nothing can be shown, which counts as Cancel: nothing changes.
        if (_owner() is not { } owner)
        {
            return;
        }

        var window = new SettingsWindow { DataContext = viewModel };
        await window.ShowDialog(owner);
    }
}
