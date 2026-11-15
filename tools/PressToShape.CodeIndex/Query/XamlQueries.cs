using System;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml;

namespace PressToShape.CodeIndex.Query;

/// <summary>The XAML side of the queries: finding the XAML file of a view class, and the XAML file of a XAML entry.</summary>
internal sealed class XamlQueries
{
    private readonly IndexReader _reader;

    public XamlQueries(IndexReader reader) => _reader = reader;

    /// <summary>The XAML file of a view class, through its verified <c>CodeBehind</c> relationship, or null when it has none.</summary>
    public XamlShow? ViewOfClass(string classId)
    {
        var edge = _reader.Relationships.FirstOrDefault(e => e.Type == RelationshipType.CodeBehind && e.ToId == classId);
        return edge is null || _reader.EntryOf(edge.FromId) is not { } entry ? null : ForEntry(entry);
    }

    /// <summary>The XAML file and entity of a names entry whose id is a XAML id.</summary>
    public XamlShow ForEntry(NameEntry entry)
    {
        var file = _reader.XamlFile(entry.IndexFile);
        return new XamlShow(file, entry.Id, entry.SymbolType);
    }

    /// <summary>A short readable name for any id: the name from <c>names.tsv</c>, the description of a XAML id, or the id itself.</summary>
    public string NameOf(string id) => _reader.EntryOf(id)?.Name ?? (XamlIds.IsXaml(id) ? XamlIds.Describe(id) : id);

    /// <summary>True when the id belongs to the XAML part of the index.</summary>
    public static bool IsXamlEntry(NameEntry entry) => XamlIds.IsXaml(entry.Id);

    /// <summary>The edges that start at the entity: for an element also those of its bindings and handlers (<c>id@Property</c>).</summary>
    public static Func<RelationshipRecord, bool> StartsAt(NameEntry entry) =>
        entry.SymbolType == "Element"
            ? e => e.FromId == entry.Id || e.FromId.StartsWith(entry.Id + "@", StringComparison.Ordinal)
            : e => e.FromId == entry.Id;
}
