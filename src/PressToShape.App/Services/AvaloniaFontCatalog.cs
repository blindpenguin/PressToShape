using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Services;

/// <summary>
/// The font families of the system, with the bundled default font in front. Only the names are read: making a typeface of every font to
/// see whether it works loads all font files at startup, and a broken one crashes the native font code.
/// </summary>
internal sealed class AvaloniaFontCatalog : IFontCatalog
{
    private readonly Lazy<IReadOnlyList<string>> _families = new(Load);

    public IReadOnlyList<string> Families => _families.Value;

    private static IReadOnlyList<string> Load() =>
    [
        TextShape.DefaultFontFamily,
        .. FontManager.Current.SystemFonts
            .Select(font => font.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name) && !string.Equals(name, TextShape.DefaultFontFamily, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.CurrentCultureIgnoreCase),
    ];
}
