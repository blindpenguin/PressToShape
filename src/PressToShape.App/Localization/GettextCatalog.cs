using System;
using System.Globalization;
using System.IO;
using GetText;

namespace PressToShape.App.Localization;

/// <summary>A translation catalog backed by a compiled gettext .mo file, through GetText.NET.</summary>
internal sealed class GettextCatalog : ITranslationCatalog
{
    private readonly Catalog _catalog;

    /// <summary>Reads the catalog from a .mo stream. Throws if the stream is not a valid .mo file.</summary>
    public GettextCatalog(Stream moStream, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(moStream);
        ArgumentNullException.ThrowIfNull(culture);

        _catalog = new Catalog(moStream, culture);
    }

    public string? Translate(string text)
    {
        if (!_catalog.TranslationExists(text))
        {
            return null;
        }

        var translated = _catalog.GetString(text);
        return string.IsNullOrEmpty(translated) ? null : translated;
    }

    public string? Translate(string context, string text)
    {
        // GetParticularString returns the source text when there is no translation, so a result that differs
        // from the source text is a translation. A translation equal to the source text is also fine to treat as missing.
        var translated = _catalog.GetParticularString(context, text);
        return string.IsNullOrEmpty(translated) || string.Equals(translated, text, StringComparison.Ordinal)
            ? null
            : translated;
    }
}
