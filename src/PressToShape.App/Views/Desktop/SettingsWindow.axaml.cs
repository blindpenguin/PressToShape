using System;
using Avalonia.Controls;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Views.Desktop;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private SettingsViewModel? _viewModel;

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        Unsubscribe();
        if (DataContext is SettingsViewModel viewModel)
        {
            // The view model decides OK or Cancel, the window only closes itself. Closing with the window's own button counts as Cancel.
            _viewModel = viewModel;
            viewModel.CloseRequested += OnCloseRequested;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        Unsubscribe();
        base.OnClosed(e);
    }

    private void OnCloseRequested(object? sender, bool applied) => Close();

    private void Unsubscribe()
    {
        if (_viewModel is not null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
            _viewModel = null;
        }
    }
}
