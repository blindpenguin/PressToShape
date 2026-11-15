using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Views.Editor;

/// <summary>
/// The Properties tab. The code-behind only forwards: focus, Enter and Escape of the number fields, the pickers' open and close,
/// and the opacity slider's drag. Every decision is in <see cref="PropertiesViewModel"/>.
/// </summary>
public partial class PropertiesView : UserControl
{
    public PropertiesView()
    {
        InitializeComponent();

        // A number field commits when it is left or Enter is pressed, and goes back to the shown value on Escape. Each text box
        // carries its field view model in Tag. The handlers sit on the view, so keys in other text boxes keep their meaning.
        AddHandler(GotFocusEvent, OnFieldGotFocus);
        AddHandler(LostFocusEvent, OnFieldLostFocus);
        AddHandler(KeyDownEvent, OnFieldKeyDown, RoutingStrategies.Tunnel);

        OpacitySlider.AddHandler(PointerPressedEvent, OnOpacityPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        OpacitySlider.AddHandler(PointerReleasedEvent, OnOpacityReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        OpacitySlider.PointerCaptureLost += OnOpacityCaptureLost;
        OpacitySlider.KeyDown += OnOpacityKeyDown;
        OpacitySlider.KeyUp += OnOpacityKeyUp;
        OpacitySlider.PropertyChanged += OnOpacitySliderPropertyChanged;
    }

    private PropertiesViewModel? Properties => DataContext as PropertiesViewModel;

    private static NumberFieldViewModel? FieldOf(RoutedEventArgs e) => (e.Source as TextBox)?.Tag as NumberFieldViewModel;

    private void OnFieldGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (FieldOf(e) is { } field)
        {
            field.IsEditing = true;
        }
    }

    private void OnFieldLostFocus(object? sender, RoutedEventArgs e)
    {
        if (FieldOf(e) is { } field)
        {
            field.IsEditing = false;
            field.Commit();
        }
    }

    private void OnFieldKeyDown(object? sender, KeyEventArgs e)
    {
        if (FieldOf(e) is not { } field)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            field.Commit();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            field.Revert();
            e.Handled = true;
        }
    }

    private void OnFillFlyoutOpened(object? sender, System.EventArgs e)
    {
        if (Properties is { } properties)
        {
            FillColorView.Color = CoreColorConverter.ToAvalonia(properties.Fill.Color);
            properties.Fill.BeginEdit();
        }
    }

    private void OnFillFlyoutClosed(object? sender, System.EventArgs e) => Properties?.Fill.EndEdit();

    private void OnFillColorChanged(object? sender, ColorChangedEventArgs e) => Properties?.Fill.Update(CoreColorConverter.ToCore(e.NewColor));

    private void OnFillNoneClick(object? sender, RoutedEventArgs e) => Properties?.Fill.ToggleNone();

    private void OnStrokeFlyoutOpened(object? sender, System.EventArgs e)
    {
        if (Properties is { } properties)
        {
            StrokeColorView.Color = CoreColorConverter.ToAvalonia(properties.Stroke.Color);
            properties.Stroke.BeginEdit();
        }
    }

    private void OnStrokeFlyoutClosed(object? sender, System.EventArgs e) => Properties?.Stroke.EndEdit();

    private void OnStrokeColorChanged(object? sender, ColorChangedEventArgs e) => Properties?.Stroke.Update(CoreColorConverter.ToCore(e.NewColor));

    private void OnStrokeNoneClick(object? sender, RoutedEventArgs e) => Properties?.Stroke.ToggleNone();

    private void OnOpacityPressed(object? sender, PointerPressedEventArgs e) => Properties?.BeginOpacityDrag();

    private void OnOpacityReleased(object? sender, PointerReleasedEventArgs e) => Properties?.EndOpacityDrag();

    private void OnOpacityCaptureLost(object? sender, PointerCaptureLostEventArgs e) => Properties?.EndOpacityDrag();

    private void OnOpacityKeyDown(object? sender, KeyEventArgs e) => Properties?.BeginOpacityDrag();

    private void OnOpacityKeyUp(object? sender, KeyEventArgs e) => Properties?.EndOpacityDrag();

    private void OnOpacitySliderPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Avalonia.Controls.Primitives.RangeBase.ValueProperty && e.NewValue is double value)
        {
            Properties?.PreviewOpacity(value);
        }
    }
}
