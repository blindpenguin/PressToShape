using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace PressToShape.App.Tests;

/// <summary>A translatable text found in the sources: its message id, optional context and where it was found.</summary>
internal sealed record SourceText(string? Context, string Text, string File);

/// <summary>
/// Finds the translatable texts in the App project, the way a gettext extraction would:
/// <c>T("...")</c>, <c>N("...")</c> and <c>TC("context", "...")</c> in C#, and <c>{loc:T '...'}</c> in XAML.
/// </summary>
internal static class SourceTexts
{
    private static readonly Regex CSharpText = new("\\b(?:N|T)\\(\"((?:[^\"\\\\]|\\\\.)*)\"\\)", RegexOptions.Compiled);
    private static readonly Regex CSharpTextWithContext = new("\\bTC\\(\"((?:[^\"\\\\]|\\\\.)*)\",\\s*\"((?:[^\"\\\\]|\\\\.)*)\"\\)", RegexOptions.Compiled);
    private static readonly Regex XamlText = new("\\{loc:T '([^']*)'(?:\\s*,\\s*Context=([^}]*))?\\}", RegexOptions.Compiled);

    public static IReadOnlyList<SourceText> FindAll()
    {
        var root = RepoPaths.Combine("src/PressToShape.App");
        var result = new List<SourceText>();

        foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories).Where(IsSource))
        {
            var relative = Path.GetRelativePath(RepoPaths.Root, file);
            var content = File.ReadAllText(file);

            if (file.EndsWith(".cs", System.StringComparison.Ordinal))
            {
                result.AddRange(CSharpText.Matches(content).Select(m => new SourceText(null, UnescapeCSharp(m.Groups[1].Value), relative)));
                result.AddRange(CSharpTextWithContext.Matches(content)
                    .Select(m => new SourceText(UnescapeCSharp(m.Groups[1].Value), UnescapeCSharp(m.Groups[2].Value), relative)));
            }
            else
            {
                result.AddRange(XamlText.Matches(content).Select(m => new SourceText(
                    m.Groups[2].Success ? WebUtility.HtmlDecode(m.Groups[2].Value.Trim().Trim('\'')) : null,
                    WebUtility.HtmlDecode(m.Groups[1].Value),
                    relative)));
            }
        }

        return result;
    }

    private static bool IsSource(string path)
    {
        var separator = Path.DirectorySeparatorChar;
        if (path.Contains($"{separator}obj{separator}") || path.Contains($"{separator}bin{separator}"))
        {
            return false;
        }

        return path.EndsWith(".cs", System.StringComparison.Ordinal) || path.EndsWith(".axaml", System.StringComparison.Ordinal);
    }

    private static string UnescapeCSharp(string text) =>
        text.Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\n", "\n");
}
