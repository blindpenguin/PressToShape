using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace PressToShape.App.Services;

/// <summary>
/// Closes the application when it runs with a classic desktop lifetime. Does nothing otherwise.
/// It closes the main window instead of shutting down directly, so the window's closing handler can ask about unsaved changes.
/// </summary>
internal sealed class DesktopAppShutdown : IAppShutdown
{
    public void Shutdown()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Close();
        }
    }
}
