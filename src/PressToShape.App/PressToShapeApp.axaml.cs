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

    public override void Initialize()
    {
        // The localizer must exist before the XAML is loaded, because {loc:T} extensions read it while loading.
        var settingsStore = new JsonFileSettingsStore(JsonFileSettingsStore.DefaultPath);
        var localizer = new Localizer(
            new EmbeddedCatalogSource(),
            LanguagePreference.ChooseStartLanguage(settingsStore, CultureInfo.CurrentUICulture));
        LocalizationHost.Initialize(localizer);
        _languagePreference = LanguagePreference.Attach(settingsStore, localizer);

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new DesktopShellViewModel(new DesktopAppShutdown(), LocalizationHost.Localizer),
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
