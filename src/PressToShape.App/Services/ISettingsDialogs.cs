using System.Threading.Tasks;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Services;

/// <summary>Shows the settings window: a modal window on the desktop, an overlay inside the layout where there are no windows.</summary>
internal interface ISettingsDialogs
{
    /// <summary>
    /// Shows the settings and returns when they are closed. The result is not returned: the caller listens to
    /// <see cref="SettingsViewModel.CloseRequested"/> and applies the values only after OK. A dialog closed in any other way counts as Cancel.
    /// </summary>
    Task ShowAsync(SettingsViewModel viewModel);
}
