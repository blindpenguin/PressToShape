using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PressToShape.App.Tests;

/// <summary>One translation entry of a .po file.</summary>
internal sealed record PoEntry(string? Context, string Id, string Translation, bool IsFuzzy);

/// <summary>
/// A small reader for gettext .po files, so tests can check translations without the msgfmt tool.
/// It supports message contexts, multi-line strings, escapes and the fuzzy flag. The header entry (empty id) is skipped.
/// </summary>
internal static class PoFile
{
    public static IReadOnlyList<PoEntry> Read(string path) => Parse(File.ReadAllText(path, Encoding.UTF8));

    public static IReadOnlyList<PoEntry> Parse(string text)
    {
        var entries = new List<PoEntry>();
        string? context = null;
        string? id = null;
        StringBuilder? current = null;
        string? currentKeyword = null;
        string translation = string.Empty;
        var fuzzy = false;
        var hasEntry = false;

        void Finish()
        {
            if (hasEntry && id is not null && id.Length > 0)
            {
                entries.Add(new PoEntry(context, id, translation, fuzzy));
            }

            context = null;
            id = null;
            translation = string.Empty;
            fuzzy = false;
            hasEntry = false;
            current = null;
            currentKeyword = null;
        }

        void Commit()
        {
            if (current is null || currentKeyword is null)
            {
                return;
            }

            var value = current.ToString();
            switch (currentKeyword)
            {
                case "msgctxt": context = value; break;
                case "msgid": id = value; break;
                case "msgstr": translation = value; break;
            }

            current = null;
            currentKeyword = null;
        }

        foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();

            if (line.Length == 0)
            {
                Commit();
                Finish();
                continue;
            }

            if (line.StartsWith("#~", System.StringComparison.Ordinal))
            {
                continue; // obsolete entry
            }

            if (line.StartsWith('#'))
            {
                if (line.StartsWith("#,", System.StringComparison.Ordinal) && line.Contains("fuzzy", System.StringComparison.Ordinal))
                {
                    fuzzy = true;
                }

                continue;
            }

            if (line.StartsWith('"'))
            {
                current?.Append(Unescape(line));
                continue;
            }

            Commit();
            hasEntry = true;
            foreach (var keyword in new[] { "msgctxt", "msgid", "msgstr" })
            {
                if (line.StartsWith(keyword + " ", System.StringComparison.Ordinal) || line.StartsWith(keyword + "\"", System.StringComparison.Ordinal))
                {
                    currentKeyword = keyword;
                    current = new StringBuilder(Unescape(line[keyword.Length..].Trim()));
                    break;
                }
            }
        }

        Commit();
        Finish();
        return entries;
    }

    /// <summary>Removes the surrounding quotes of a .po string literal and resolves escapes.</summary>
    private static string Unescape(string quoted)
    {
        var text = quoted.Trim();
        if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
        {
            text = text[1..^1];
        }

        var result = new StringBuilder();
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\\' || i + 1 >= text.Length)
            {
                result.Append(text[i]);
                continue;
            }

            i++;
            result.Append(text[i] switch
            {
                'n' => '\n',
                't' => '\t',
                '"' => '"',
                '\\' => '\\',
                var other => other,
            });
        }

        return result.ToString();
    }
}
