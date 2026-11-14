using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace PressToShape.App.Views.Desktop;

/// <summary>Turns a shortcut text such as "Ctrl+Shift+S" into a <see cref="KeyGesture"/> so the menu can display it.</summary>
internal sealed class GestureTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text || text.Length == 0)
        {
            return null;
        }

        // The gesture parser cannot read a literal "+" or "-" as the final key, so name those keys.
        if (text.EndsWith("++", StringComparison.Ordinal))
        {
            text = text[..^1] + "OemPlus";
        }
        else if (text.EndsWith("+-", StringComparison.Ordinal))
        {
            text = text[..^1] + "OemMinus";
        }

        try
        {
            return KeyGesture.Parse(text);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or NotSupportedException)
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
