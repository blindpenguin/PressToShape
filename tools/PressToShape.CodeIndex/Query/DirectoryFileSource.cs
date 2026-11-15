using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Query;

/// <summary>Reads the index from a folder on disk.</summary>
internal sealed class DirectoryFileSource : IIndexFileSource
{
    private readonly string _root;

    public DirectoryFileSource(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootDirectory);
        _root = Path.GetFullPath(rootDirectory);
    }

    public byte[]? Read(string relativePath)
    {
        var path = Path.Combine(_root, IndexPath.Normalize(relativePath).Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    public IReadOnlyList<string> ListFiles() => !Directory.Exists(_root)
        ? []
        : Directory.GetFiles(_root, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(_root, f).Replace(Path.DirectorySeparatorChar, '/'))
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();
}
