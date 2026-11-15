using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;

namespace PressToShape.App.Views.Editor;

public partial class CanvasAreaView : UserControl
{
    /// <summary>Whether the rulers (and the corner between them) are shown. The desktop layout shows them, the tablet layout does not.</summary>
    public static readonly StyledProperty<bool> ShowRulersProperty =
        AvaloniaProperty.Register<CanvasAreaView, bool>(nameof(ShowRulers), defaultValue: true);

    // The button that started the current pointer interaction. Further presses while it lasts are ignored.
    private MouseButton? _activeButton;

    // The fingers that are down on the viewport. Each finger is its own pointer with its own capture.
    private readonly HashSet<int> _touches = [];
    private CanvasAreaViewModel? _observed;
    private TopLevel? _window;

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
        Viewport.PointerExited += OnViewportPointerExited;
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

    public bool ShowRulers
    {
        get => GetValue(ShowRulersProperty);
        set => SetValue(ShowRulersProperty, value);
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
        if (DataContext is CanvasAreaViewModel touchViewModel && e.Pointer.Type == PointerType.Touch)
        {
            // Focus makes the Delete key reach this panel. The view model decides what one or two fingers do.
            Viewport.Focus();
            e.Pointer.Capture(Viewport);
            _touches.Add(e.Pointer.Id);
            var touchPosition = e.GetPosition(Viewport);
            touchViewModel.TouchPressed(e.Pointer.Id, touchPosition.X, touchPosition.Y);
            e.Handled = true;
            return;
        }

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
            viewModel.PointerPressed(position.X, position.Y, e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        }

        e.Handled = true;
    }

    private void OnViewportPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not CanvasAreaViewModel viewModel)
        {
            return;
        }

        var position = e.GetPosition(Viewport);
        if (e.Pointer.Type == PointerType.Touch)
        {
            if (_touches.Contains(e.Pointer.Id))
            {
                viewModel.TouchMoved(e.Pointer.Id, position.X, position.Y);
                e.Handled = true;
            }

            return;
        }

        if (e.Pointer.Captured == Viewport)
        {
            viewModel.PointerMoved(position.X, position.Y, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Alt));
        }
        else
        {
            viewModel.PointerHover(position.X, position.Y);
        }
    }

    private void OnViewportPointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is CanvasAreaViewModel viewModel && e.Pointer.Captured != Viewport)
        {
            viewModel.PointerLeft();
        }
    }

    private void OnViewportPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is CanvasAreaViewModel touchViewModel && e.Pointer.Type == PointerType.Touch)
        {
            // The finger is removed first, so that losing the capture below is not taken for a cancelled touch.
            if (_touches.Remove(e.Pointer.Id))
            {
                var touchPosition = e.GetPosition(Viewport);
                touchViewModel.TouchReleased(e.Pointer.Id, touchPosition.X, touchPosition.Y);
                e.Pointer.Capture(null);
                e.Handled = true;
            }

            return;
        }

        if (DataContext is not CanvasAreaViewModel viewModel || e.Pointer.Captured != Viewport || e.InitialPressMouseButton != _activeButton)
        {
            return;
        }

        var position = e.GetPosition(Viewport);
        _activeButton = null;
        viewModel.PointerReleased(position.X, position.Y, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Alt));
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    // Also called after a normal release, where the drag has ended already and Cancel does nothing.
    private void OnViewportPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (DataContext is not CanvasAreaViewModel viewModel)
        {
            return;
        }

        if (e.Pointer.Type == PointerType.Touch)
        {
            // A finger that is still down lost the capture, so the system or another control took it away.
            if (_touches.Remove(e.Pointer.Id))
            {
                viewModel.TouchCancelled();
            }

            return;
        }

        viewModel.Cancel();
    }

    private void OnViewportKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not CanvasAreaViewModel viewModel)
        {
            return;
        }

        if (e.Key == Key.Delete)
        {
            viewModel.DeleteSelection();
            e.Handled = true;
            return;
        }

        var key = e.Key switch
        {
            Key.Left => CanvasKey.Left,
            Key.Right => CanvasKey.Right,
            Key.Up => CanvasKey.Up,
            Key.Down => CanvasKey.Down,
            Key.Escape => CanvasKey.Escape,
            _ => (CanvasKey?)null,
        };

        // Only keys that changed something are handled, so other controls and shortcuts keep theirs.
        if (key is { } canvasKey && viewModel.KeyPressed(canvasKey, e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
        {
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

    // View-only concern: the Alt key switches the Zoom tool's cursor to zoom out. The window is listened to, so it also works while
    // the canvas does not have the focus, and handled events are included so that no other control can hide the key.
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _window = TopLevel.GetTopLevel(this);
        _window?.AddHandler(KeyDownEvent, OnWindowAltKey, RoutingStrategies.Tunnel, handledEventsToo: true);
        _window?.AddHandler(KeyUpEvent, OnWindowAltKey, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _window?.RemoveHandler(KeyDownEvent, OnWindowAltKey);
        _window?.RemoveHandler(KeyUpEvent, OnWindowAltKey);
        _window = null;

        base.OnDetachedFromVisualTree(e);
    }

    private void OnWindowAltKey(object? sender, KeyEventArgs e)
    {
        if ((e.Key is Key.LeftAlt or Key.RightAlt) && DataContext is CanvasAreaViewModel viewModel)
        {
            viewModel.IsAltHeld = e.RoutedEvent == KeyDownEvent;
        }
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_observed is not null)
        {
            _observed.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _touches.Clear();
        _observed = DataContext as CanvasAreaViewModel;
        if (_observed is { } viewModel)
        {
            // The area that grabs a handle for a finger comes from the theme, like every other size.
            if (this.TryFindResource("TouchHandleGrabSize", ActualThemeVariant, out var grabSize) && grabSize is double grab)
            {
                viewModel.TouchGrabSize = grab;
            }

            viewModel.ViewportWidth = Viewport.Bounds.Width;
            viewModel.ViewportHeight = Viewport.Bounds.Height;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            Viewport.Cursor = CursorFor(viewModel.CursorType);
        }
    }

    // The cursor image that fits the screen scale of the window. Before the view is in a window the scale counts as 100%.
    private Cursor CursorFor(CanvasCursorType type) => CanvasCursors.For(type, TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0);

    // View-only concern: the pointer shape follows the view model's cursor type.
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CanvasAreaViewModel.CursorType) && _observed is { } viewModel)
        {
            Viewport.Cursor = CursorFor(viewModel.CursorType);
        }
    }
}
