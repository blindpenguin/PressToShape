using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Views.Editor;

public partial class CanvasAreaView : UserControl
{
    // The button that started the current pointer interaction. Further presses while it lasts are ignored.
    private MouseButton? _activeButton;
    private CanvasAreaViewModel? _observed;

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
        Viewport.PointerWheelChanged += OnViewportPointerWheelChanged;
        Viewport.KeyDown += OnViewportKeyDown;

        // The bars only report what the user did. The view model decides what that means.
        HorizontalBar.Scroll += (_, e) =>
        {
            if (DataContext is CanvasAreaViewModel viewModel)
            {
                viewModel.HorizontalScrollValue = e.NewValue;
            }
        };
        VerticalBar.Scroll += (_, e) =>
        {
            if (DataContext is CanvasAreaViewModel viewModel)
            {
                viewModel.VerticalScrollValue = e.NewValue;
            }
        };
    }

    private void OnViewportPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not CanvasAreaViewModel viewModel)
        {
            return;
        }

        var position = e.GetPosition(Viewport);
        var control = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        e.Handled = viewModel.Wheel(e.Delta.X, e.Delta.Y, control, shift, position.X, position.Y);
    }

    private void OnViewportPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(Viewport).Properties;
        if (DataContext is not CanvasAreaViewModel viewModel
            || e.Pointer.Captured == Viewport
            || (!properties.IsLeftButtonPressed && !properties.IsMiddleButtonPressed))
        {
            return;
        }

        // Focus makes the Delete key reach this panel.
        Viewport.Focus();
        e.Pointer.Capture(Viewport);
        var position = e.GetPosition(Viewport);
        if (properties.IsMiddleButtonPressed)
        {
            _activeButton = MouseButton.Middle;
            viewModel.MiddlePressed(position.X, position.Y);
        }
        else
        {
            _activeButton = MouseButton.Left;
            viewModel.PointerPressed(position.X, position.Y, e.KeyModifiers.HasFlag(KeyModifiers.Alt));
        }

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
        if (DataContext is not CanvasAreaViewModel viewModel || e.Pointer.Captured != Viewport || e.InitialPressMouseButton != _activeButton)
        {
            return;
        }

        var position = e.GetPosition(Viewport);
        _activeButton = null;
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

        if (_observed is not null)
        {
            _observed.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _observed = DataContext as CanvasAreaViewModel;
        if (_observed is { } viewModel)
        {
            viewModel.ViewportWidth = Viewport.Bounds.Width;
            viewModel.ViewportHeight = Viewport.Bounds.Height;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            Viewport.Cursor = CanvasCursors.For(viewModel.CursorType);
        }
    }

    // View-only concern: the pointer shape follows the view model's cursor type.
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CanvasAreaViewModel.CursorType) && _observed is { } viewModel)
        {
            Viewport.Cursor = CanvasCursors.For(viewModel.CursorType);
        }
    }
}
