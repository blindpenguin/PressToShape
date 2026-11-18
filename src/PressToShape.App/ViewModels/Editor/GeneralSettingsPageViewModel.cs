namespace PressToShape.App.ViewModels.Editor;

/// <summary>The page of the settings window for the General category. It only gives its view the values of the <see cref="SettingsViewModel"/>.</summary>
internal sealed class GeneralSettingsPageViewModel(SettingsViewModel settings) : ViewModelBase
{
    public SettingsViewModel Settings { get; } = settings;
}
