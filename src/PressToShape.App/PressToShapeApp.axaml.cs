using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.Settings;
using PressToShape.App.ViewModels;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.Views;

namespace PressToShape.App;

public partial class PressToShapeApp : Application
{
    // Keeps the language preference (which saves the language on change) alive for the app's lifetime.
    private LanguagePreference? _languagePreference;
    private JsonFileSettingsStore? _settingsStore;

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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var localizer = LocalizationHost.Localizer;
            var services = new FileServices(
                _settingsStore ?? throw new InvalidOperationException("The settings store is created in Initialize."),
                new LocalFileStore(),
                new AvaloniaFileDialogs(() => desktop.MainWindow, localizer),
                new AvaloniaMessageDialogs(() => desktop.MainWindow, localizer));
            desktop.MainWindow = new MainWindow
            {
                DataContext = new DesktopShellViewModel(new DesktopAppShutdown(), localizer, services),
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
        {
            activityLifetime.MainViewFactory = () => new MainView { DataContext = new MainViewModel() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            singleViewLifetime.MainView = new MainView
            {
                DataContext = new MainViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
