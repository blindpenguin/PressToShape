using System;

namespace PressToShape.CodeIndex.Loading;

/// <summary>Removes machine-specific paths from messages, so the index never contains them.</summary>
internal static class PathScrubber
{
    public static string Scrub(string text, string repoRoot, string? homeDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(repoRoot);

        var result = Replace(text, repoRoot, "<repo>");
        var home = homeDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return string.IsNullOrEmpty(home) ? result : Replace(result, home, "~");
    }

    private static string Replace(string text, string path, string replacement)
    {
        var trimmed = path.TrimEnd('/', '\\');
        if (trimmed.Length == 0)
        {
            return text;
        }

        return text
            .Replace(trimmed, replacement, StringComparison.Ordinal)
            .Replace(trimmed.Replace('/', '\\'), replacement, StringComparison.Ordinal);
    }
}
