using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>The settings window of the shell.</summary>
internal abstract partial class EditorShellViewModel
{
    private bool _settingsOpen;

    /// <summary>Whether the settings window can be opened: the platform has one, and none is open right now.</summary>
    private bool CanOpenSettings() => _settingsDialogs is not null && !_settingsOpen;

    /// <summary>Opens the settings window on a copy of the settings. The values are applied only after OK.</summary>
    [RelayCommand(CanExecute = nameof(CanOpenSettings))]
    private async Task OpenSettingsAsync()
    {
        if (!CanOpenSettings())
        {
            return;
        }

        var viewModel = new SettingsViewModel(State.Snapping, _localizer, CultureInfo.CurrentUICulture, _showRulersByDefault);
        var accepted = false;
        viewModel.CloseRequested += (_, ok) => accepted = ok;

        _settingsOpen = true;
        OpenSettingsCommand.NotifyCanExecuteChanged();
        try
        {
            await _settingsDialogs!.ShowAsync(viewModel);
        }
        finally
        {
            _settingsOpen = false;
            OpenSettingsCommand.NotifyCanExecuteChanged();
        }

        if (accepted)
        {
            viewModel.Apply();
        }
    }
}
