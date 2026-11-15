using Avalonia.Controls;
using PressToShape.App.ViewModels.Desktop;

namespace PressToShape.App.Views;

public partial class MainWindow : Window
{
    private bool _closeConfirmed;

    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    // Closing is cancelled first, because asking takes a while. If the user allows it, the window closes again.
    // When the operating system is shutting down there is no time to ask.
    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_closeConfirmed || e.CloseReason == WindowCloseReason.OSShutdown || DataContext is not DesktopShellViewModel shell)
        {
            return;
        }

        e.Cancel = true;
        if (await shell.ConfirmCloseAsync())
        {
            _closeConfirmed = true;
            Close();
        }
    }
}
