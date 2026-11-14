namespace PressToShape.App.Localization;

/// <summary>Finds the translation catalog of a language. English has no catalog, because the source text is English.</summary>
internal interface ICatalogSource
{
    /// <summary>Loads the catalog for a language code such as "de". Returns null when there is none.</summary>
    ITranslationCatalog? Load(string languageCode);
}
