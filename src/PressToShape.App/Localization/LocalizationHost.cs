using System;

namespace PressToShape.App.Localization;

/// <summary>
/// Holds the application's localizer for XAML markup extensions, which cannot receive injected services.
/// View models must not use this class: they get an <see cref="ILocalizer"/> through their constructor.
/// </summary>
internal static class LocalizationHost
{
    private static ILocalizer? _localizer;

    /// <summary>The application localizer. Before startup sets it, this is an English localizer without catalogs.</summary>
    public static ILocalizer Localizer => _localizer ??= new Localizer(new NoCatalogSource());

    public static void Initialize(ILocalizer localizer)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    private sealed class NoCatalogSource : ICatalogSource
    {
        public ITranslationCatalog? Load(string languageCode) => null;
    }
}
