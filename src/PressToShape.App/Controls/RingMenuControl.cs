using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;

namespace PressToShape.App.Controls;

/// <summary>
/// The ring context menu. It fills the canvas area (so a press beside the ring is caught here and closes it), and draws the wedges, their
/// icons and the center around the point where the ring was opened. It draws what <see cref="RingMenuViewModel"/> holds and forwards pointer
/// and key input to it; <see cref="RingGeometry"/> does the math. The colors and sizes come from the theme (see the styles of the classes
/// ringWedge, ringIcon, ringCenter and ringCenterText).
/// </summary>
internal sealed class RingMenuControl : Panel
{
    public static readonly StyledProperty<double> InnerRadiusProperty =
        AvaloniaProperty.Register<RingMenuControl, double>(nameof(InnerRadius), 34);

    public static readonly StyledProperty<double> OuterRadiusProperty =
        AvaloniaProperty.Register<RingMenuControl, double>(nameof(OuterRadius), 110);

    private readonly List<Path> _wedges = [];
    private readonly List<MaterialIcon> _icons = [];
    private readonly List<RingSlotViewModel> _observedSlots = [];
    private readonly Ellipse _center = new() { IsHitTestVisible = false };
    private readonly MaterialIcon _centerIcon = new() { IsHitTestVisible = false };
    private readonly TextBlock _centerText = new() { IsHitTestVisible = false };
    private RingMenuViewModel? _ring;
    private Point _ringCenter;
    private RingHit? _pressed;
    private int? _adoptedPointerId;

    public RingMenuControl()
    {
        Background = Brushes.Transparent;
        Focusable = true;
        IsVisible = false;
        _center.Classes.Add("ringCenter");
        _centerText.Classes.Add("ringCenterText");
        _centerIcon.Classes.Add("ringIcon");
    }

    /// <summary>The radius of the center disc. Wedges start here.</summary>
    public double InnerRadius
    {
        get => GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    /// <summary>The outer radius of the wedges.</summary>
    public double OuterRadius
    {
        get => GetValue(OuterRadiusProperty);
        set => SetValue(OuterRadiusProperty, value);
    }

    /// <summary>
    /// Takes over a pointer that is already down, for the long press of a finger or a pen: the ring opened under it. Its release then uses
    /// the slot it is on, and does nothing on the center (where it started) or beside the ring.
    /// </summary>
    public void AdoptPointer(IPointer pointer)
    {
        ArgumentNullException.ThrowIfNull(pointer);
        _adoptedPointerId = pointer.Id;
        pointer.Capture(this);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_ring is not null)
        {
            _ring.PropertyChanged -= OnRingPropertyChanged;
        }

        _ring = DataContext as RingMenuViewModel;
        if (_ring is not null)
        {
            _ring.PropertyChanged += OnRingPropertyChanged;
        }

        IsVisible = _ring?.IsOpen == true;
        Rebuild();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == InnerRadiusProperty || change.Property == OuterRadiusProperty)
        {
            Rebuild();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(child == _centerText ? new Size(InnerRadius * 1.6, InnerRadius * 2) : Size.Infinity);
        }

        return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var outer = OuterRadius;
        var inner = InnerRadius;
        var (x, y) = RingGeometry.ClampCenter(_ring?.CenterX ?? 0, _ring?.CenterY ?? 0, outer, finalSize.Width, finalSize.Height);
        _ringCenter = new Point(x, y);

        foreach (var wedge in _wedges)
        {
            wedge.Arrange(new Rect(x - outer, y - outer, 2 * outer, 2 * outer));
        }

        for (var i = 0; i < _icons.Count; i++)
        {
            var (dx, dy) = RingGeometry.SlotCenter(i, _icons.Count, inner, outer);
            ArrangeCentered(_icons[i], x + dx, y + dy);
        }

        _center.Arrange(new Rect(x - inner, y - inner, 2 * inner, 2 * inner));
        ArrangeCentered(_centerIcon, x, y);
        ArrangeCentered(_centerText, x, y);
        return finalSize;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_ring is { IsOpen: true })
        {
            var hit = HitAt(e);
            _ring.Hover(hit.IsSlot ? hit.Index : null);
            e.Handled = true;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _ring?.Hover(null);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_ring is not { IsOpen: true })
        {
            return;
        }

        e.Handled = true;
        var hit = HitAt(e);
        if (hit.IsOutside)
        {
            _ring.Close();
            return;
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _pressed = hit;
            e.Pointer.Capture(this);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        var pressed = _pressed;
        _pressed = null;
        var adopted = _adoptedPointerId == e.Pointer.Id;
        if (adopted)
        {
            _adoptedPointerId = null;
        }

        if (_ring is not { IsOpen: true })
        {
            return;
        }

        e.Handled = true;
        if (e.Pointer.Captured == this)
        {
            e.Pointer.Capture(null);
        }

        var hit = HitAt(e);
        if (adopted)
        {
            // The press that opened the ring started on the center, so only a slot counts.
            if (hit.IsSlot)
            {
                _ring.Activate(hit.Index);
            }

            return;
        }

        // A press and a release on the same wedge, or the same center, is a click. A release of the button that opened the ring is nothing.
        if (pressed is { } start && start == hit)
        {
            if (hit.IsSlot)
            {
                _ring.Activate(hit.Index);
            }
            else if (hit.IsCenter)
            {
                _ring.ActivateCenter();
            }
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _pressed = null;
        if (_adoptedPointerId == e.Pointer.Id)
        {
            _adoptedPointerId = null;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        // The canvas below does not zoom or scroll while the ring is open.
        e.Handled |= _ring is { IsOpen: true };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_ring is not { IsOpen: true })
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Left or Key.Up:
                _ring.MoveFocus(-1);
                break;
            case Key.Right or Key.Down:
                _ring.MoveFocus(1);
                break;
            case Key.Enter or Key.Space:
                if (_ring.FocusedIndex is { } focused)
                {
                    _ring.Activate(focused);
                }

                break;
            case Key.Escape:
                _ring.Back();
                break;
            case Key.Tab:
                // The focus stays in the ring until it closes.
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private RingHit HitAt(PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        return RingGeometry.HitTest(position.X - _ringCenter.X, position.Y - _ringCenter.Y, _wedges.Count, InnerRadius, OuterRadius);
    }

    private void OnRingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(RingMenuViewModel.IsOpen):
                IsVisible = _ring?.IsOpen == true;
                break;
            case nameof(RingMenuViewModel.Slots):
                Rebuild();
                break;
            case nameof(RingMenuViewModel.CenterX):
            case nameof(RingMenuViewModel.CenterY):
                InvalidateArrange();
                break;
            default:
                // Hover, focus, the center text and the center action, and an empty name for everything (a language change).
                Refresh();
                break;
        }
    }

    private void OnSlotPropertyChanged(object? sender, PropertyChangedEventArgs e) => Refresh();

    // The wedges and icons depend on the slots and on the radii, so they are made again when one of those changes.
    private void Rebuild()
    {
        foreach (var slot in _observedSlots)
        {
            slot.PropertyChanged -= OnSlotPropertyChanged;
        }

        _observedSlots.Clear();
        _wedges.Clear();
        _icons.Clear();
        Children.Clear();

        var slots = _ring?.Slots ?? [];
        for (var i = 0; i < slots.Count; i++)
        {
            var wedge = new Path { Data = WedgeGeometry(i, slots.Count), IsHitTestVisible = false };
            wedge.Classes.Add("ringWedge");
            var icon = new MaterialIcon { Kind = slots[i].Icon, IsHitTestVisible = false };
            icon.Classes.Add("ringIcon");
            _wedges.Add(wedge);
            _icons.Add(icon);
            slots[i].PropertyChanged += OnSlotPropertyChanged;
            _observedSlots.Add(slots[i]);
        }

        Children.AddRange(_wedges);
        Children.AddRange(_icons);
        Children.Add(_center);
        Children.Add(_centerIcon);
        Children.Add(_centerText);
        Refresh();
        InvalidateMeasure();
    }

    // Brings the state of the elements in line with the view model: unavailable, hovered and focused wedges, names, and the center.
    private void Refresh()
    {
        if (_ring is null)
        {
            return;
        }

        var slots = _ring.Slots;
        for (var i = 0; i < _wedges.Count && i < slots.Count; i++)
        {
            var available = slots[i].IsAvailable;
            SetClass(_wedges[i].Classes, "unavailable", !available);
            SetClass(_wedges[i].Classes, "hovered", _ring.HoveredIndex == i);
            SetClass(_wedges[i].Classes, "focused", _ring.FocusedIndex == i);
            SetClass(_icons[i].Classes, "unavailable", !available);
            _wedges[i].IsEnabled = available;
            AutomationProperties.SetName(_wedges[i], slots[i].Name);
        }

        var text = _ring.CenterText;
        _centerText.Text = text;
        _centerText.IsVisible = text.Length > 0;
        _centerIcon.IsVisible = text.Length == 0;
        _centerIcon.Kind = _ring.CanGoBack ? MaterialIconKind.ArrowULeftTop : MaterialIconKind.Close;
        AutomationProperties.SetName(_center, _ring.CenterActionName);
    }

    // The wedge as a ring segment, drawn in a box of twice the outer radius with the center of the ring in the middle of the box.
    private Geometry WedgeGeometry(int index, int count)
    {
        var outer = OuterRadius;
        var inner = InnerRadius;
        if (count == 1)
        {
            return new GeometryGroup
            {
                FillRule = FillRule.EvenOdd,
                Children =
                [
                    new EllipseGeometry(new Rect(0, 0, 2 * outer, 2 * outer)),
                    new EllipseGeometry(new Rect(outer - inner, outer - inner, 2 * inner, 2 * inner)),
                ],
            };
        }

        var (start, sweep) = RingGeometry.WedgeAngles(index, count);
        var end = start + sweep;
        var isLarge = sweep > 180;
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(PointAt(start, outer), true);
            context.ArcTo(PointAt(end, outer), new Size(outer, outer), 0, isLarge, SweepDirection.Clockwise);
            context.LineTo(PointAt(end, inner));
            context.ArcTo(PointAt(start, inner), new Size(inner, inner), 0, isLarge, SweepDirection.CounterClockwise);
            context.EndFigure(true);
        }

        return geometry;
    }

    private Point PointAt(double degrees, double radius)
    {
        var angle = degrees * Math.PI / 180;
        return new Point(OuterRadius + radius * Math.Sin(angle), OuterRadius - radius * Math.Cos(angle));
    }

    private static void ArrangeCentered(Control child, double x, double y)
    {
        var size = child.DesiredSize;
        child.Arrange(new Rect(x - size.Width / 2, y - size.Height / 2, size.Width, size.Height));
    }

    private static void SetClass(Classes classes, string name, bool on)
    {
        if (on && !classes.Contains(name))
        {
            classes.Add(name);
        }
        else if (!on)
        {
            classes.Remove(name);
        }
    }
}
