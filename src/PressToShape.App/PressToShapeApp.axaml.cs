using System;
using System.Globalization;
using Avalonia;
using Avalonia.Markup.Xaml;
using PressToShape.App.Localization;
using PressToShape.App.Settings;

namespace PressToShape.App;

public partial class PressToShapeApp : Application
{
    // Keeps the language preference (which saves the language on change) alive for the app's lifetime.
    private LanguagePreference? _languagePreference;
    private JsonFileSettingsStore? _settingsStore;

    /// <summary>
    /// Ends the app. The Android head sets it to the call that finishes the activity. It is read when the user leaves the app, so the head
    /// can set it after the layout was created. The desktop layout closes its window and does not use it.
    /// </summary>
    public Action? ExitRequested { get; set; }

    public override void Initialize()
    {
        // The localizer must exist before the XAML is loaded, because {loc:T} extensions read it while loading.
        _settingsStore = new JsonFileSettingsStore(JsonFileSettingsStore.DefaultPath);
        var localizer = new Localizer(
            new EmbeddedCatalogSource(),
            LanguagePreference.ChooseStartLanguage(_settingsStore, CultureInfo.CurrentUICulture));
        LocalizationHost.Initialize(localizer);
        _languagePreference = LanguagePreference.Attach(_settingsStore, localizer);

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AppLayout.Choose(
            ApplicationLifetime,
            _settingsStore ?? throw new InvalidOperationException("The settings store is created in Initialize."),
            LocalizationHost.Localizer,
            () => ExitRequested);

        base.OnFrameworkInitializationCompleted();
    }
}
