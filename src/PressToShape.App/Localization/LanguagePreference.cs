using System;
using System.Globalization;
using PressToShape.App.Settings;

namespace PressToShape.App.Localization;

/// <summary>
/// Connects the localizer to the settings: it chooses the start language and saves the language when it changes.
/// </summary>
internal sealed class LanguagePreference
{
    private readonly ISettingsStore _store;
    private readonly ILocalizer _localizer;

    private LanguagePreference(ISettingsStore store, ILocalizer localizer)
    {
        _store = store;
        _localizer = localizer;
    }

    /// <summary>The start language: the saved choice, otherwise German for a German system, otherwise English.</summary>
    public static string ChooseStartLanguage(ISettingsStore store, CultureInfo systemUiCulture)
    {
        ArgumentNullException.ThrowIfNull(store);
        return LanguageSelector.Choose(store.Load().Language, systemUiCulture);
    }

    /// <summary>Saves the language whenever it changes. Keep the returned object alive for the app's lifetime.</summary>
    public static LanguagePreference Attach(ISettingsStore store, ILocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(localizer);

        var preference = new LanguagePreference(store, localizer);
        localizer.LanguageChanged += preference.OnLanguageChanged;
        return preference;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Load first, so settings this version does not know about are kept.
        var settings = _store.Load();
        settings.Language = _localizer.LanguageCode;
        _store.Save(settings);
    }
}
