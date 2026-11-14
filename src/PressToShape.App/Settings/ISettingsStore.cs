namespace PressToShape.App.Settings;

/// <summary>Loads and saves <see cref="AppSettings"/>. Neither operation throws.</summary>
internal interface ISettingsStore
{
    /// <summary>Returns the saved settings, or default settings if there are none or they cannot be read.</summary>
    AppSettings Load();

    /// <summary>Saves the settings. Failures are ignored: the settings then only apply to the current session.</summary>
    void Save(AppSettings settings);
}
