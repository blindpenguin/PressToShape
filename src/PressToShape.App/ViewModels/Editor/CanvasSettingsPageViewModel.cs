namespace PressToShape.App.ViewModels.Editor;

/// <summary>The page of the settings window for the Canvas category. It only gives its view the values of the <see cref="SettingsViewModel"/>.</summary>
internal sealed class CanvasSettingsPageViewModel(SettingsViewModel settings) : ViewModelBase
{
    public SettingsViewModel Settings { get; } = settings;
}
