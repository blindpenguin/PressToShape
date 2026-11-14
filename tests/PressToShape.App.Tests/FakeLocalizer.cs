using System.Collections.Generic;
using PressToShape.App.Localization;

namespace PressToShape.App.Tests;

/// <summary>A dictionary-based translation catalog for tests.</summary>
internal sealed class FakeCatalog : ITranslationCatalog
{
    private readonly Dictionary<string, string> _entries;

    public FakeCatalog(Dictionary<string, string> entries) => _entries = entries;

    /// <summary>Keys are the message id, or "context|id" for texts with a context.</summary>
    public string? Translate(string text) => _entries.GetValueOrDefault(text);

    public string? Translate(string context, string text) => _entries.GetValueOrDefault(context + "|" + text);
}

/// <summary>A catalog source for tests: one dictionary per language code. Counts the loads.</summary>
internal sealed class FakeCatalogSource : ICatalogSource
{
    private readonly Dictionary<string, FakeCatalog> _catalogs = new();

    public int LoadCount { get; private set; }

    public FakeCatalogSource Add(string code, Dictionary<string, string> entries)
    {
        _catalogs[code] = new FakeCatalog(entries);
        return this;
    }

    public ITranslationCatalog? Load(string languageCode)
    {
        LoadCount++;
        return _catalogs.GetValueOrDefault(languageCode);
    }
}

/// <summary>Creates real <see cref="Localizer"/> instances with fake German translations.</summary>
internal static class FakeLocalizer
{
    public static Localizer Create(string initialLanguage = "en", Dictionary<string, string>? german = null)
    {
        var source = new FakeCatalogSource().Add("de", german ?? new Dictionary<string, string>());
        return new Localizer(source, initialLanguage);
    }
}
