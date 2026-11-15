using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaColor = Avalonia.Media.Color;
using CoreColor = PressToShape.Core.Shapes.Color;

namespace PressToShape.App.Views.Editor;

/// <summary>Turns the Core color of a shape into an Avalonia color and back. Alpha is kept.</summary>
internal sealed class CoreColorConverter : IValueConverter
{
    public static CoreColorConverter Instance { get; } = new();

    public static AvaloniaColor ToAvalonia(CoreColor color) => AvaloniaColor.FromArgb(color.A, color.R, color.G, color.B);

    public static CoreColor ToCore(AvaloniaColor color) => new(color.R, color.G, color.B, color.A);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is CoreColor color ? ToAvalonia(color) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is AvaloniaColor color ? ToCore(color) : null;
}

/// <summary>Turns the Core color of a shape into a brush, for swatches. It does not convert back.</summary>
internal sealed class CoreColorBrushConverter : IValueConverter
{
    public static CoreColorBrushConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is CoreColor color ? new ImmutableSolidColorBrush(CoreColorConverter.ToAvalonia(color)) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
