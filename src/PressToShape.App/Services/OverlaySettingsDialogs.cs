using System;
using System.Threading.Tasks;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.ViewModels.Tablet;

namespace PressToShape.App.Services;

/// <summary>Shows the settings for a platform without windows: inside the layout, through a <see cref="SettingsDialogHost"/>.</summary>
internal sealed class OverlaySettingsDialogs : ISettingsDialogs
{
    private readonly SettingsDialogHost _host;

    public OverlaySettingsDialogs(SettingsDialogHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public Task ShowAsync(SettingsViewModel viewModel) => _host.ShowAsync(viewModel);
}
