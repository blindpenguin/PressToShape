using System;
using System.Collections.Generic;
using System.Globalization;

namespace PressToShape.App.Localization;

/// <summary>
/// Holds the current language and translates texts through the catalog of that language.
/// English has no catalog: the English source text is returned unchanged.
/// </summary>
internal sealed class Localizer : ILocalizer
{
    private readonly ICatalogSource _source;
    private ITranslationCatalog? _catalog;

    public Localizer(ICatalogSource source, string initialLanguage = SupportedLanguages.DefaultCode)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));

        var language = SupportedLanguages.Find(initialLanguage) ?? SupportedLanguages.Find(SupportedLanguages.DefaultCode)!;
        LanguageCode = language.Code;
        Culture = CultureInfo.GetCultureInfo(language.Code);
        _catalog = LoadCatalog(language.Code);
    }

    public event EventHandler? LanguageChanged;

    public CultureInfo Culture { get; private set; }

    public string LanguageCode { get; private set; }

    public IReadOnlyList<LanguageInfo> Languages => SupportedLanguages.All;

    /// <summary>
    /// Marks a text as translatable without translating it now. Use it in data tables, then translate with
    /// <see cref="T"/> when the text is shown. The marker lets the translation checks find the text.
    /// </summary>
    public static string N(string text) => text;

    public string T(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return _catalog?.Translate(text) ?? text;
    }

    public string TC(string context, string text)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(text);
        return _catalog?.Translate(context, text) ?? text;
    }

    public void SetLanguage(string code)
    {
        var language = SupportedLanguages.Find(code);
        if (language is null || string.Equals(language.Code, LanguageCode, StringComparison.Ordinal))
        {
            return;
        }

        LanguageCode = language.Code;
        Culture = CultureInfo.GetCultureInfo(language.Code);
        _catalog = LoadCatalog(language.Code);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private ITranslationCatalog? LoadCatalog(string code) =>
        string.Equals(code, SupportedLanguages.DefaultCode, StringComparison.Ordinal) ? null : _source.Load(code);
}
