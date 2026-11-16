using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PressToShape.App.Views.Editor;

/// <summary>Turns the depth of a row of the object list into the width of the empty space in front of it. The view sets <see cref="Indent"/> from the theme.</summary>
internal sealed class ObjectIndentConverter : IValueConverter
{
    /// <summary>The width for one level, in device-independent pixels.</summary>
    public double Indent { get; set; } = 14;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int depth ? Math.Max(0, depth) * Indent : 0d;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
