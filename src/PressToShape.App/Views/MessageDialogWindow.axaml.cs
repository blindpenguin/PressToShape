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

    private MessageDialogViewModel? _viewModel;

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        Unsubscribe();
        if (DataContext is MessageDialogViewModel viewModel)
        {
            // The view model decides the result, the window only closes itself with it.
            _viewModel = viewModel;
            viewModel.CloseRequested += OnCloseRequested;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        Unsubscribe();
        base.OnClosed(e);
    }

    private void OnCloseRequested(object? sender, int result) => Close(result);

    private void Unsubscribe()
    {
        if (_viewModel is not null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
            _viewModel = null;
        }
    }
}
