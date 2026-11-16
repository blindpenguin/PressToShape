using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Views.Editor;

/// <summary>
/// The Layers tab. The view only turns pointer input into calls of the view model: a press selects, a double-tap renames, and a drag
/// (after a long press for a finger) drops an object above another one of the same group.
/// </summary>
public partial class LayersView : UserControl
{
    // The pointer has to move this far (device-independent pixels) before a press becomes a drag.
    private const double DragDistance = 6;

    private ObjectRowViewModel? _pressed;
    private ObjectRowViewModel? _dropTarget;
    private Point _pressPoint;
    private bool _armed;
    private bool _dragging;

    public LayersView()
    {
        InitializeComponent();

        RowList.AddHandler(PointerPressedEvent, OnRowPressed, RoutingStrategies.Tunnel);
        RowList.AddHandler(PointerMovedEvent, OnRowMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        RowList.AddHandler(PointerReleasedEvent, OnRowReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        RowList.AddHandler(PointerCaptureLostEvent, OnCaptureLost, RoutingStrategies.Bubble, handledEventsToo: true);
        RowList.AddHandler(InputElement.DoubleTappedEvent, OnRowDoubleTapped, RoutingStrategies.Bubble);
        RowList.AddHandler(InputElement.HoldingEvent, OnRowHolding, RoutingStrategies.Bubble);
        RowList.AddHandler(LostFocusEvent, OnLostFocus, RoutingStrategies.Bubble);
    }

    private LayersViewModel? List => DataContext as LayersViewModel;

    // The indent of a level comes from the theme, so the tablet layout gets its larger value.
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (Resources["IndentConverter"] is ObjectIndentConverter converter
            && this.TryFindResource("RowIndentSize", ActualThemeVariant, out var indent)
            && indent is double size)
        {
            converter.Indent = size;
        }
    }

    // The row under a visual and whether the pointer is on a control of the row (a button or the name field) that handles the input itself.
    private static (ObjectRowViewModel? Row, bool OnControl) RowAt(object? source)
    {
        var onControl = false;
        for (var visual = source as Visual; visual is not null; visual = visual.GetVisualParent())
        {
            if (visual is Button or TextBox)
            {
                onControl = true;
            }

            if (visual is Control { DataContext: ObjectRowViewModel row })
            {
                return (row, onControl);
            }
        }

        return (null, onControl);
    }

    private void OnRowPressed(object? sender, PointerPressedEventArgs e)
    {
        if (List is not { } list || !e.GetCurrentPoint(RowList).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var (row, onControl) = RowAt(e.Source);
        if (row is null || onControl)
        {
            return;
        }

        list.Select(row, extend: e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        _pressed = row;
        _pressPoint = e.GetPosition(RowList);
        _dragging = false;

        // A finger scrolls the list, so it only drags after a long press (see OnRowHolding).
        _armed = e.Pointer.Type != PointerType.Touch;
    }

    private void OnRowHolding(object? sender, HoldingRoutedEventArgs e)
    {
        if (_pressed is not null && e.HoldingState == HoldingState.Started)
        {
            _armed = true;
        }
    }

    private void OnRowMoved(object? sender, PointerEventArgs e)
    {
        if (List is not { } list || _pressed is not { } dragged || !_armed)
        {
            return;
        }

        var position = e.GetPosition(RowList);
        if (!_dragging)
        {
            if (Math.Abs(position.X - _pressPoint.X) < DragDistance && Math.Abs(position.Y - _pressPoint.Y) < DragDistance)
            {
                return;
            }

            _dragging = true;
            e.Pointer.Capture(RowList);
        }

        var target = RowAt(RowList.InputHitTest(position) as Visual).Row;
        var valid = target is not null && list.CanReorder(dragged, target) ? target : null;
        SetDropTarget(valid);
    }

    private void OnRowReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragging && List is { } list && _pressed is { } dragged && _dropTarget is { } target)
        {
            list.Reorder(dragged, target);
        }

        EndDrag(e.Pointer);
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e) => EndDrag(null);

    private void EndDrag(IPointer? pointer)
    {
        SetDropTarget(null);
        _pressed = null;
        _dragging = false;
        _armed = false;
        if (pointer?.Captured == RowList)
        {
            pointer.Capture(null);
        }
    }

    private void SetDropTarget(ObjectRowViewModel? target)
    {
        if (ReferenceEquals(_dropTarget, target))
        {
            return;
        }

        if (_dropTarget is not null)
        {
            _dropTarget.IsDropTarget = false;
        }

        _dropTarget = target;
        if (target is not null)
        {
            target.IsDropTarget = true;
        }
    }

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        var (row, onControl) = RowAt(e.Source);
        if (row is null || onControl || !row.BeginRenameCommand.CanExecute(null))
        {
            return;
        }

        row.BeginRenameCommand.Execute(null);

        // The name field only exists in the visible tree after the next layout.
        Dispatcher.UIThread.Post(() => FocusNameField(row), DispatcherPriority.Background);
        e.Handled = true;
    }

    private void FocusNameField(ObjectRowViewModel row)
    {
        foreach (var field in RowList.GetVisualDescendants())
        {
            if (field is TextBox { IsVisible: true } box && ReferenceEquals(box.DataContext, row))
            {
                box.Focus();
                box.SelectAll();
                return;
            }
        }
    }

    // Leaving the name field keeps what was typed.
    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (e.Source is TextBox { DataContext: ObjectRowViewModel { IsRenaming: true } row })
        {
            row.CommitRenameCommand.Execute(null);
        }
    }
}
