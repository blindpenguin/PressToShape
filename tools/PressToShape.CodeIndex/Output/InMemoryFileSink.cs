using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Query;

namespace PressToShape.CodeIndex.Output;

/// <summary>Collects the files of a run in memory. Used by <c>check</c> and by tests, so nothing is written to disk.</summary>
internal sealed class InMemoryFileSink : IIndexFileSink, IIndexFileSource
{
    private readonly SortedDictionary<string, byte[]> _files = new(System.StringComparer.Ordinal);

    public IReadOnlyDictionary<string, byte[]> Files => _files;

    public byte[]? Read(string relativePath) => _files.GetValueOrDefault(IndexPath.Normalize(relativePath));

    public IReadOnlyList<string> ListFiles() => _files.Keys.ToList();

    public void Write(string relativePath, byte[] content)
    {
        System.ArgumentNullException.ThrowIfNull(content);
        _files[IndexPath.Normalize(relativePath)] = content;
    }
}
