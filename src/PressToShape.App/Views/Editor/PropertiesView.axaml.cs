using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PressToShape.App.Controls;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using CoreColor = PressToShape.Core.Shapes.Color;

namespace PressToShape.App.Views.Editor;

/// <summary>
/// The Properties tab. The code-behind only forwards: the number fields (shared with the contextual bar of the tablet layout), the pickers' open and close,
/// the pick, add-to-palette and recent-color buttons of the pickers, the palette swatches (with the Shift state), and the opacity slider's drag. Every decision is in <see cref="PropertiesViewModel"/>.
/// </summary>
public partial class PropertiesView : UserControl
{
    // Button.Click has no key state: the strip remembers whether Shift was down when the pointer went down on a swatch.
    private bool _shiftOnSwatch;

    public PropertiesView()
    {
        InitializeComponent();

        PaletteStrip.AddHandler(PointerPressedEvent, OnSwatchPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);

        // A number field commits when it is left or Enter is pressed, and goes back to the shown value on Escape.
        NumberFieldBehavior.Attach(this);

        OpacitySlider.AddHandler(PointerPressedEvent, OnOpacityPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        OpacitySlider.AddHandler(PointerReleasedEvent, OnOpacityReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        OpacitySlider.PointerCaptureLost += OnOpacityCaptureLost;
        OpacitySlider.KeyDown += OnOpacityKeyDown;
        OpacitySlider.KeyUp += OnOpacityKeyUp;
        OpacitySlider.PropertyChanged += OnOpacitySliderPropertyChanged;
    }

    private PropertiesViewModel? Properties => DataContext as PropertiesViewModel;

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

    private void OnFillAddToPaletteClick(object? sender, RoutedEventArgs e) => Properties?.Palette.AddSwatch(CoreColorConverter.ToCore(FillColorView.Color));

    private void OnStrokeAddToPaletteClick(object? sender, RoutedEventArgs e) => Properties?.Palette.AddSwatch(CoreColorConverter.ToCore(StrokeColorView.Color));

    // The flyout closes first, which ends the picker edit, then the canvas waits for the click.
    private void OnFillPickClick(object? sender, RoutedEventArgs e)
    {
        FillSwatchButton.Flyout?.Hide();
        Properties?.StartPick(ColorPickTarget.Fill);
    }

    private void OnStrokePickClick(object? sender, RoutedEventArgs e)
    {
        StrokeSwatchButton.Flyout?.Hide();
        Properties?.StartPick(ColorPickTarget.Stroke);
    }

    // A recent color is chosen in the open picker, so it is previewed and recorded like any other pick of the picker.
    private void OnFillRecentClick(object? sender, RoutedEventArgs e)
    {
        if (ColorOf(sender) is { } color)
        {
            FillColorView.Color = CoreColorConverter.ToAvalonia(color);
        }
    }

    private void OnStrokeRecentClick(object? sender, RoutedEventArgs e)
    {
        if (ColorOf(sender) is { } color)
        {
            StrokeColorView.Color = CoreColorConverter.ToAvalonia(color);
        }
    }

    private void OnSwatchPointerPressed(object? sender, PointerPressedEventArgs e) =>
        _shiftOnSwatch = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

    private void OnSwatchClick(object? sender, RoutedEventArgs e)
    {
        var toStroke = _shiftOnSwatch;
        _shiftOnSwatch = false;
        if (ColorOf(sender) is { } color)
        {
            Properties?.Palette.Apply(color, toStroke);
        }
    }

    private void OnRemoveSwatchClick(object? sender, RoutedEventArgs e)
    {
        if (ColorOf(sender) is { } color)
        {
            Properties?.Palette.RemoveSwatch(color);
        }
    }

    private static CoreColor? ColorOf(object? sender) => (sender as Control)?.DataContext is CoreColor color ? color : null;
}
