using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace PressToShape.App.Localization;

/// <summary>
/// Loads the compiled catalog of a language from the embedded resource
/// PressToShape.App.Translations.CODE.mo. Never throws: a missing or unreadable catalog means English.
/// </summary>
internal sealed class EmbeddedCatalogSource : ICatalogSource
{
    private const string ResourcePrefix = "PressToShape.App.Translations.";

    private readonly Assembly _assembly;

    public EmbeddedCatalogSource()
        : this(typeof(EmbeddedCatalogSource).Assembly)
    {
    }

    public EmbeddedCatalogSource(Assembly assembly)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
    }

    public ITranslationCatalog? Load(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return null;
        }

        try
        {
            using var stream = _assembly.GetManifestResourceStream(ResourcePrefix + languageCode + ".mo");
            if (stream is null)
            {
                Trace.TraceWarning("No embedded translation catalog for language '{0}'.", languageCode);
                return null;
            }

            return new GettextCatalog(stream, CultureInfo.GetCultureInfo(languageCode));
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("Could not load the translation catalog for '{0}': {1}", languageCode, ex.Message);
            return null;
        }
    }
}
