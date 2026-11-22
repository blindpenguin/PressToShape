using System;
using System.Collections.Generic;

namespace PressToShape.CodeIndex.Output;

/// <summary>
/// Notices when one run writes the same index file twice. Two source files can map to one index file name, and a second write would silently
/// replace the first. Names that differ only in case count as the same, because they are the same file on Windows and macOS.
/// </summary>
internal sealed class PathCollisionGuard
{
    private readonly Dictionary<string, string> _first = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Remembers a normalized index path.</summary>
    /// <exception cref="IndexToolException">The path, or a path that differs only in case, was registered before.</exception>
    public void Register(string normalizedPath)
    {
        ArgumentNullException.ThrowIfNull(normalizedPath);

        if (_first.TryGetValue(normalizedPath, out var first))
        {
            var also = string.Equals(first, normalizedPath, StringComparison.Ordinal) ? string.Empty : $" (also {first})";
            throw new IndexToolException($"Two source files produce the same index file: {normalizedPath}{also}. Rename one of the source files and run the command again.");
        }

        _first.Add(normalizedPath, normalizedPath);
    }
}
