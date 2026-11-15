using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using PressToShape.App.Workspace;

namespace PressToShape.App.Controls;

/// <summary>
/// The button of a tool group in the tablet tool column. A tap selects the current tool of the group (see <see cref="ToolToggleButton"/>). Holding the pointer down for a while runs
/// <see cref="LongPressCommand"/> (which opens the flyout of the group), and the click that follows the release is then swallowed, so a long
/// press never also selects the tool. The decision is made by <see cref="LongPressTracker"/>; this control only passes it pointer events
/// and the time. It looks like a plain toggle button, so the tool styles apply to it.
/// </summary>
public sealed class ToolGroupButton : ToolToggleButton
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(50);

    public static readonly StyledProperty<ICommand?> LongPressCommandProperty =
        AvaloniaProperty.Register<ToolGroupButton, ICommand?>(nameof(LongPressCommand));

    private readonly LongPressTracker _tracker = new();
    private DispatcherTimer? _timer;

    /// <summary>The time source. Tests replace it so they need no real time.</summary>
    internal Func<long> Clock { get; set; } = () => Environment.TickCount64;

    /// <summary>Run when the button was held down long enough.</summary>
    public ICommand? LongPressCommand
    {
        get => GetValue(LongPressCommandProperty);
        set => SetValue(LongPressCommandProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var position = e.GetPosition(this);
            _tracker.Press(position.X, position.Y, Clock());
            StartTimer();
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var position = e.GetPosition(this);
        _tracker.Move(position.X, position.Y);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        EndPress();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        EndPress();
    }

    protected override void OnClick()
    {
        if (_tracker.ConsumeFired())
        {
            return;
        }

        base.OnClick();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopTimer();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>One tick of the timer. Public to the tests only.</summary>
    internal void OnTimerTick()
    {
        if (_tracker.Tick(Clock()))
        {
            StopTimer();
            if (LongPressCommand is { } command && command.CanExecute(null))
            {
                command.Execute(null);
            }
        }
        else if (!_tracker.IsWatching)
        {
            StopTimer();
        }
    }

    // A fired long press stays known (for OnClick) until the next press.
    private void EndPress()
    {
        _tracker.Release();
        StopTimer();
    }

    private void StartTimer()
    {
        _timer ??= new DispatcherTimer(TickInterval, DispatcherPriority.Input, (_, _) => OnTimerTick());
        _timer.Start();
    }

    private void StopTimer() => _timer?.Stop();
}
