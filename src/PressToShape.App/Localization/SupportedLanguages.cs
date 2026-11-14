using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.App.Localization;

/// <summary>
/// The languages the app supports. To add a language: add its .po file to Translations/ and one entry here.
/// </summary>
internal static class SupportedLanguages
{
    public const string DefaultCode = "en";

    public static IReadOnlyList<LanguageInfo> All { get; } =
    [
        new("en", "English"),
        new("de", "Deutsch"),
    ];

    public static bool IsSupported(string? code) => Find(code) is not null;

    public static LanguageInfo? Find(string? code) =>
        string.IsNullOrWhiteSpace(code)
            ? null
            : All.FirstOrDefault(l => string.Equals(l.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
}
