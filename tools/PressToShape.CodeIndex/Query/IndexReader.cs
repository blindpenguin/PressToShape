using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Query;

/// <summary>Reads index files on demand and keeps what it read. It never compiles anything.</summary>
internal sealed class IndexReader
{
    private readonly IIndexFileSource _source;
    private readonly Dictionary<string, SymbolsFile> _symbolFiles = new(StringComparer.Ordinal);
    private IndexManifest? _manifest;
    private IReadOnlyList<NameEntry>? _names;
    private Dictionary<string, NameEntry>? _namesById;
    private IReadOnlyList<RelationshipRecord>? _relationships;

    public IndexReader(IIndexFileSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    public IndexManifest Manifest => _manifest ??= ReadManifest();

    public IReadOnlyList<NameEntry> Names => _names ??= ReadNames();

    public NameEntry? EntryOf(string id)
    {
        _namesById ??= Names.GroupBy(n => n.Id, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        return _namesById.GetValueOrDefault(id);
    }

    public SymbolRecord? SymbolOf(string id) =>
        EntryOf(id) is { } entry ? Symbols(entry.IndexFile).Symbols.FirstOrDefault(s => s.Id == id) : null;

    public IReadOnlyList<SymbolRecord> MembersOf(string typeId) =>
        EntryOf(typeId) is { } entry ? Symbols(entry.IndexFile).Symbols.Where(s => s.ContainingId == typeId).ToList() : [];

    public SymbolsFile Symbols(string path)
    {
        if (!_symbolFiles.TryGetValue(path, out var file))
        {
            file = Deserialize<SymbolsFile>(path);
            CheckSchema(file.SchemaVersion, path);
            _symbolFiles[path] = file;
        }

        return file;
    }

    public IReadOnlyList<RelationshipRecord> Relationships => _relationships ??= Manifest.Projects
        .OrderBy(p => p.Name, StringComparer.Ordinal)
        .SelectMany(p =>
        {
            var path = IndexLayout.RelationshipsPath(p.Name);
            var file = Deserialize<RelationshipsFile>(path);
            CheckSchema(file.SchemaVersion, path);
            return file.Relationships;
        })
        .ToList();

    private IndexManifest ReadManifest()
    {
        var manifest = Deserialize<IndexManifest>(IndexLayout.ManifestPath);
        CheckSchema(manifest.SchemaVersion, IndexLayout.ManifestPath);
        return manifest;
    }

    private IReadOnlyList<NameEntry> ReadNames()
    {
        _ = Manifest;
        var bytes = _source.Read(IndexLayout.NamesPath) ?? throw Missing(IndexLayout.NamesPath);
        var entries = new List<NameEntry>();
        foreach (var line in Encoding.UTF8.GetString(bytes).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t');
            if (parts.Length != 5)
            {
                throw new IndexToolException($"{IndexLayout.NamesPath} is damaged. Regenerate the index with: {IndexHints.Regenerate}");
            }

            entries.Add(new NameEntry(parts[0], parts[1], parts[2], parts[3], parts[4]));
        }

        return entries;
    }

    private T Deserialize<T>(string path)
    {
        var bytes = _source.Read(path) ?? throw Missing(path);
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonIndexWriter.Options) ?? throw Damaged(path);
        }
        catch (JsonException)
        {
            throw Damaged(path);
        }
    }

    private static void CheckSchema(int version, string path)
    {
        if (version != IndexSchema.Version)
        {
            throw new IndexToolException(
                $"{path} has schema version {version}, but this tool reads version {IndexSchema.Version}. Regenerate the index with: {IndexHints.Regenerate}");
        }
    }

    private static IndexToolException Missing(string path) =>
        new($"The index file {path} was not found. Create the index with: {IndexHints.Regenerate}");

    private static IndexToolException Damaged(string path) =>
        new($"The index file {path} is damaged. Regenerate the index with: {IndexHints.Regenerate}");
}
