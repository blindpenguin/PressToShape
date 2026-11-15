using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Xaml;

namespace PressToShape.CodeIndex.Tests;

/// <summary>A XAML file source over a dictionary of repo-relative paths. Used by the XAML tests, so no disk is involved.</summary>
internal sealed class InMemoryXamlFileSource : IXamlFileSource
{
    private readonly SortedDictionary<string, string> _files = new(StringComparer.Ordinal);

    public InMemoryXamlFileSource Add(string path, string text)
    {
        _files[path] = text;
        return this;
    }

    public IReadOnlyList<string> List(string projectDirectory) =>
        _files.Keys.Where(k => k.StartsWith(projectDirectory.TrimEnd('/') + "/", StringComparison.Ordinal)).ToList();

    public string Read(string repoRelativePath) =>
        _files.TryGetValue(repoRelativePath, out var text) ? text : throw new System.IO.FileNotFoundException(repoRelativePath);
}
