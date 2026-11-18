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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is SettingsViewModel viewModel)
        {
            // The view model decides OK or Cancel, the window only closes itself. Closing with the window's own button counts as Cancel.
            viewModel.CloseRequested += (_, _) => Close();
        }
    }
}
