using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Query;
using PressToShape.CodeIndex.Xaml;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Chain;

/// <summary>The view over the committed index files. It reads only what is asked for. It never compiles anything.</summary>
internal sealed class ReaderIndexView : IIndexView
{
    private readonly IndexReader _reader;
    private IReadOnlyList<XamlFileRecord>? _xamlFiles;

    public ReaderIndexView(IndexReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public IReadOnlyList<RelationshipRecord> Relationships => _reader.Relationships;

    /// <summary>Reads every XAML file of the index. Use <see cref="XamlFileOf"/> when only one is needed.</summary>
    public IReadOnlyList<XamlFileRecord> XamlFiles => _xamlFiles ??= _reader.Names
        .Where(n => n.SymbolType == "View")
        .Select(n => _reader.XamlFile(n.IndexFile))
        .OrderBy(f => f.Id, StringComparer.Ordinal)
        .ToList();

    public IReadOnlyList<ProjectInfo> Projects => _reader.Manifest.Projects.Select(p => new ProjectInfo(p.Name, p.XamlFileCount)).OrderBy(p => p.Name, StringComparer.Ordinal).ToList();

    public SymbolRecord? SymbolOf(string id)
    {
        if (_reader.SymbolOf(id) is { } symbol)
        {
            return symbol;
        }

        // names.tsv has no entries for constructors, so they are found through their type.
        var marker = id.IndexOf(".#c", StringComparison.Ordinal);
        if (marker < 0 || !id.StartsWith("M:", StringComparison.Ordinal))
        {
            return null;
        }

        return _reader.EntryOf("T:" + id[2..marker]) is { } typeEntry
            ? _reader.Symbols(typeEntry.IndexFile).Symbols.FirstOrDefault(s => s.Id == id)
            : null;
    }

    public IReadOnlyList<SymbolRecord> MembersOf(string typeId) => _reader.MembersOf(typeId).OrderBy(s => s.Id, StringComparer.Ordinal).ToList();

    public XamlFileRecord? XamlFileOf(string id) =>
        XamlIds.IsXaml(id) && _reader.EntryOf(id) is { SymbolType: "View" } entry ? _reader.XamlFile(entry.IndexFile) : null;
}
