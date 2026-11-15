using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Tests;

/// <summary>An index folder in memory, with the stale-file removal of the real one, for command line tests.</summary>
internal sealed class MemoryIndexFolder : IIndexFolder
{
    private readonly SortedDictionary<string, byte[]> _files = new(StringComparer.Ordinal);
    private readonly HashSet<string> _written = [];

    public void Write(string relativePath, byte[] content)
    {
        _files[relativePath] = content;
        _written.Add(relativePath);
    }

    public byte[]? Read(string relativePath) => _files.GetValueOrDefault(relativePath);

    public IReadOnlyList<string> ListFiles() => _files.Keys.ToList();

    public IReadOnlyList<string> RemoveStale()
    {
        var stale = _files.Keys.Where(k => !_written.Contains(k)).ToList();
        stale.ForEach(k => _files.Remove(k));
        return stale;
    }
}
