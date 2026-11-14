using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace PressToShape.App.Views.Desktop;

/// <summary>Maps the view model flag IsCheckable to a radio toggle for menu items. Other items show no toggle.</summary>
internal sealed class CheckableToggleTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? MenuItemToggleType.Radio : MenuItemToggleType.None;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
