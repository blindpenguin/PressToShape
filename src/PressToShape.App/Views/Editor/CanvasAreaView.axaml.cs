using Avalonia;
using Avalonia.Controls;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Views.Editor;

public partial class CanvasAreaView : UserControl
{
    public CanvasAreaView()
    {
        InitializeComponent();

        // View-only concern: tell the view model how large the viewport is.
        Viewport.PropertyChanged += OnViewportPropertyChanged;
    }

    private void OnViewportPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty && DataContext is CanvasAreaViewModel viewModel)
        {
            viewModel.ViewportWidth = Viewport.Bounds.Width;
            viewModel.ViewportHeight = Viewport.Bounds.Height;
        }
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is CanvasAreaViewModel viewModel)
        {
            viewModel.ViewportWidth = Viewport.Bounds.Width;
            viewModel.ViewportHeight = Viewport.Bounds.Height;
        }
    }
}
