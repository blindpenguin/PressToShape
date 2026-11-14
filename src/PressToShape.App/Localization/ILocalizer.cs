using System;
using System.Collections.Generic;
using System.Globalization;

namespace PressToShape.App.Localization;

/// <summary>Translates user-visible texts. The English source text is the message id.</summary>
internal interface ILocalizer
{
    /// <summary>The culture of the current language, used for number formatting.</summary>
    CultureInfo Culture { get; }

    /// <summary>The code of the current language, for example "en" or "de".</summary>
    string LanguageCode { get; }

    IReadOnlyList<LanguageInfo> Languages { get; }

    /// <summary>Translates a text. Returns the English text when there is no translation.</summary>
    string T(string text);

    /// <summary>Translates a text with a message context, for equal texts that need different translations.</summary>
    string TC(string context, string text);

    /// <summary>Switches the language. Unknown codes and the current language are ignored.</summary>
    void SetLanguage(string code);

    /// <summary>Raised after the language has changed.</summary>
    event EventHandler? LanguageChanged;
}
