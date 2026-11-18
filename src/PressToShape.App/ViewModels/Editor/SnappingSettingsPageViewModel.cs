namespace PressToShape.App.ViewModels.Editor;

/// <summary>The page of the settings window for the Snapping category. It only gives its view the values of the <see cref="SettingsViewModel"/>.</summary>
internal sealed class SnappingSettingsPageViewModel(SettingsViewModel settings) : ViewModelBase
{
    public SettingsViewModel Settings { get; } = settings;
}
