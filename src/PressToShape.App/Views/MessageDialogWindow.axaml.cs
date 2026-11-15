using System;
using Avalonia.Controls;
using PressToShape.App.ViewModels;

namespace PressToShape.App.Views;

public partial class MessageDialogWindow : Window
{
    public MessageDialogWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MessageDialogViewModel viewModel)
        {
            // The view model decides the result, the window only closes itself with it.
            viewModel.CloseRequested += (_, result) => Close(result);
        }
    }
}
