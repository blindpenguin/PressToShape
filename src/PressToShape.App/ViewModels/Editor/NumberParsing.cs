using System.Globalization;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>Parses the numbers that the user types into a number box.</summary>
internal static class NumberParsing
{
    private const NumberStyles Styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    /// <summary>
    /// Parses a finite number with the given culture. A dot is accepted as the decimal separator in every culture. Surrounding white space is ignored.
    /// Gives <c>false</c> and 0 for empty text, text that is not a number, <c>NaN</c>, infinity and numbers with thousands separators.
    /// </summary>
    public static bool TryParse(string? text, CultureInfo culture, out double value)
    {
        var trimmed = (text ?? string.Empty).Trim();
        if ((double.TryParse(trimmed, Styles, culture, out value) || double.TryParse(trimmed, Styles, CultureInfo.InvariantCulture, out value))
            && double.IsFinite(value))
        {
            return true;
        }

        value = 0;
        return false;
    }
}
