using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Views.Editor;

public partial class CanvasAreaView : UserControl
{
    public CanvasAreaView()
    {
        InitializeComponent();

        // View-only concern: tell the view model how large the viewport is.
        Viewport.PropertyChanged += OnViewportPropertyChanged;

        // Mouse and keyboard are only forwarded: the view model decides what they do.
        Viewport.PointerPressed += OnViewportPointerPressed;
        Viewport.PointerMoved += OnViewportPointerMoved;
        Viewport.PointerReleased += OnViewportPointerReleased;
        Viewport.PointerCaptureLost += OnViewportPointerCaptureLost;
        Viewport.KeyDown += OnViewportKeyDown;
    }

    private void OnViewportPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not CanvasAreaViewModel viewModel
            || !e.GetCurrentPoint(Viewport).Properties.IsLeftButtonPressed)
        {
            return;
        }

        // Focus makes the Delete key reach this panel.
        Viewport.Focus();
        e.Pointer.Capture(Viewport);
        var position = e.GetPosition(Viewport);
        viewModel.PointerPressed(position.X, position.Y);
        e.Handled = true;
    }

    private void OnViewportPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is CanvasAreaViewModel viewModel && e.Pointer.Captured == Viewport)
        {
            var position = e.GetPosition(Viewport);
            viewModel.PointerMoved(position.X, position.Y);
        }
    }

    private void OnViewportPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is not CanvasAreaViewModel viewModel || e.Pointer.Captured != Viewport || e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

        var position = e.GetPosition(Viewport);
        viewModel.PointerReleased(position.X, position.Y);
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    // Also called after a normal release, where the drag has ended already and Cancel does nothing.
    private void OnViewportPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (DataContext is CanvasAreaViewModel viewModel)
        {
            viewModel.Cancel();
        }
    }

    private void OnViewportKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && DataContext is CanvasAreaViewModel viewModel)
        {
            viewModel.DeleteSelection();
            e.Handled = true;
        }
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
