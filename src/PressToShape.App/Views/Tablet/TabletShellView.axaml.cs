using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PressToShape.App.Controls;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;

namespace PressToShape.App.Views.Tablet;

/// <summary>
/// The root of the tablet layout. The code-behind has three view-only jobs that need the platform or the pointer: it hands the Back button to the
/// view model (after closing an open toolbar menu, which only the view knows about), it closes the panel when the canvas is touched, and it keeps
/// the layout above the on-screen keyboard. Avalonia already keeps the content out from under the system bars, and the keyboard
/// does not resize the view but only reports the area it covers (found on the device), so the bottom margin is set here.
/// </summary>
public partial class TabletShellView : UserControl
{
    private readonly BackRequestGuard _backGuard = new();
    private TopLevel? _topLevel;
    private TabletToolColumnViewModel? _column;

    public TabletShellView()
    {
        InitializeComponent();

        // The touch sizes are a dictionary in Theme.axaml, copied into this root's resources. The class "touch" is set in the XAML.
        TouchSizes.ApplyTo(this);

        // A touch on the canvas closes the panel. The tunnel handler only watches: the event is not marked handled, so the touch also reaches the canvas.
        CanvasArea.AddHandler(PointerPressedEvent, OnCanvasPressed, RoutingStrategies.Tunnel, handledEventsToo: true);

        // A menu or a flyout that closed a moment ago was closed by the Escape half of a Back key press, see BackRequestGuard.
        ToolbarView.MenuClosed += (_, _) => _backGuard.NoteClosed(Environment.TickCount64);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_column is not null)
        {
            _column.PropertyChanged -= OnColumnChanged;
        }

        _column = (DataContext as TabletShellViewModel)?.Column;
        if (_column is not null)
        {
            _column.PropertyChanged += OnColumnChanged;
        }
    }

    private void OnColumnChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TabletToolColumnViewModel.HasOpenFlyout) && _column is { HasOpenFlyout: false })
        {
            _backGuard.NoteClosed(Environment.TickCount64);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel is null)
        {
            return;
        }

        _topLevel.BackRequested += OnBackRequested;
        if (_topLevel.InputPane is { } inputPane)
        {
            inputPane.StateChanged += OnInputPaneStateChanged;
            UpdateKeyboardGap();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_topLevel is not null)
        {
            _topLevel.BackRequested -= OnBackRequested;
            if (_topLevel.InputPane is { } inputPane)
            {
                inputPane.StateChanged -= OnInputPaneStateChanged;
            }

            _topLevel = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    // The Back button. The event is handled so Android does not leave first. A menu of the toolbar is closed first and nothing else happens; otherwise the
    // view model decides (close a dialog, a tool flyout or the panel, ask about unsaved changes, or leave).
    private async void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TabletShellViewModel shell)
        {
            e.Handled = true;
            if (_backGuard.ConsumeIfSamePress(Environment.TickCount64))
            {
                return;
            }

            if (ToolbarView.TryCloseMenu())
            {
                return;
            }

            await shell.RequestBackAsync();
        }
    }

    private void OnCanvasPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is TabletShellViewModel shell)
        {
            shell.Panel.Close();
        }
    }

    private void OnInputPaneStateChanged(object? sender, Avalonia.Controls.Platform.InputPaneStateEventArgs e) => UpdateKeyboardGap();

    private void UpdateKeyboardGap()
    {
        if (_topLevel?.InputPane is not { } inputPane || this.TranslatePoint(new Point(0, Bounds.Height), _topLevel) is not { } bottom)
        {
            return;
        }

        var isOpen = inputPane.State == Avalonia.Controls.Platform.InputPaneState.Open;
        LayoutRoot.Margin = new Thickness(0, 0, 0, KeyboardInset.Gap(isOpen, bottom.Y, inputPane.OccludedRect.Y));
    }
}
