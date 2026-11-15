using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.Settings;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Views;
using PressToShape.App.Views.Tablet;

namespace PressToShape.App;

/// <summary>
/// Chooses the layout, in this one place, by the kind of application: an application with a window (Linux, Windows) shows the desktop layout, an
/// Android activity shows the tablet layout. The two layouts share the document, the tools and the file logic, and differ in the shell around them.
/// The choice takes what it needs as arguments, so it can be tested without the real settings file.
/// </summary>
internal static class AppLayout
{
    /// <param name="lifetime">The application lifetime of the platform.</param>
    /// <param name="settings">The settings file.</param>
    /// <param name="localizer">The translator.</param>
    /// <param name="exitRequested">Gives the call that ends the app (Android). It is read when the user leaves, so the head can set it late.</param>
    public static void Choose(IApplicationLifetime? lifetime, ISettingsStore settings, ILocalizer localizer, Func<Action?> exitRequested)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(exitRequested);

        if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new FileServices(
                settings,
                new LocalFileStore(),
                new AvaloniaFileDialogs(() => desktop.MainWindow, localizer),
                new AvaloniaMessageDialogs(() => desktop.MainWindow, localizer));
            desktop.MainWindow = new MainWindow
            {
                DataContext = new DesktopShellViewModel(new DesktopAppShutdown(), localizer, services),
            };
        }
        else if (lifetime is IActivityApplicationLifetime activity)
        {
            activity.MainViewFactory = TabletFactory(settings, localizer, exitRequested);
        }
    }

    /// <summary>The call that builds the tablet layout when the activity asks for its view. Nothing is built before that.</summary>
    internal static Func<Control> TabletFactory(ISettingsStore settings, ILocalizer localizer, Func<Action?> exitRequested)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(exitRequested);

        return () => CreateTabletShell(settings, localizer, exitRequested);
    }

    // The files of Android come from the system picker and have no path, so they are kept in a registry and read through it. Their references
    // do not survive a restart, so no recent files are kept. The dialogs are shown inside the layout, because there are no windows.
    internal static TabletShellView CreateTabletShell(ISettingsStore settings, ILocalizer localizer, Func<Action?> exitRequested)
    {
        var registry = new StorageFileRegistry();
        var dialogs = new MessageDialogHost();
        var view = new TabletShellView();
        var services = new FileServices(
            settings,
            new StorageFileStore(registry),
            new AvaloniaFileDialogs(() => TopLevel.GetTopLevel(view), localizer, registry),
            new OverlayMessageDialogs(dialogs, localizer),
            RemembersRecentFiles: false);
        view.DataContext = new TabletShellViewModel(new DelegateAppShutdown(() => exitRequested()?.Invoke()), localizer, services, dialogs);
        return view;
    }
}
