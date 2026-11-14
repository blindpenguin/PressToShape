namespace PressToShape.App.Localization;

/// <summary>A set of translations for one language. Returns null when a text has no translation.</summary>
internal interface ITranslationCatalog
{
    string? Translate(string text);

    string? Translate(string context, string text);
}
