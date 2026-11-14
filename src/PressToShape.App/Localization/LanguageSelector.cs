using System;
using System.Globalization;

namespace PressToShape.App.Localization;

/// <summary>The startup rule for the language: saved choice, then German for a German system, otherwise English.</summary>
internal static class LanguageSelector
{
    public static string Choose(string? savedCode, CultureInfo systemUiCulture)
    {
        ArgumentNullException.ThrowIfNull(systemUiCulture);

        if (SupportedLanguages.Find(savedCode) is { } saved)
        {
            return saved.Code;
        }

        if (SupportedLanguages.Find(systemUiCulture.TwoLetterISOLanguageName) is { } system)
        {
            return system.Code;
        }

        return SupportedLanguages.DefaultCode;
    }
}
