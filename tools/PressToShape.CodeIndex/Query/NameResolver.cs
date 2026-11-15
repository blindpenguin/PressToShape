using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.CodeIndex.Query;

/// <summary>The result of resolving a name or an id: the matches and whether they identify one symbol, several, or none.</summary>
internal sealed record Resolution(QueryStatus Status, IReadOnlyList<NameEntry> Matches);

/// <summary>
/// Finds the names entry for a query: an exact id, an id with the <c>T:</c> prefix left out, or an exact name. When several entries share a name and exactly one of them
/// is a C# type, the type wins (so <c>ShellView</c> means the class, not its XAML file). Other ties are ambiguous.
/// </summary>
internal sealed class NameResolver
{
    private readonly IndexReader _reader;

    public NameResolver(IndexReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public Resolution Resolve(string query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var names = _reader.Names;
        var matches = names.Where(n => n.Id == query || (!query.Contains(':', StringComparison.Ordinal) && n.Id == "T:" + query)).ToList();
        if (matches.Count == 0)
        {
            matches = names.Where(n => n.Name == query).ToList();
        }

        if (matches.Count > 1 && matches.Count(IsTypeEntry) == 1)
        {
            matches = matches.Where(IsTypeEntry).ToList();
        }

        return matches.Count switch
        {
            0 => new Resolution(QueryStatus.NotFound, []),
            1 => new Resolution(QueryStatus.Found, matches),
            _ => new Resolution(QueryStatus.Ambiguous, matches.OrderBy(m => m.Id, StringComparer.Ordinal).ToList()),
        };
    }

    public static bool IsTypeEntry(NameEntry n) => n.SymbolType is "Class" or "Interface" or "Record" or "Struct" or "Enum" or "Delegate";
}
