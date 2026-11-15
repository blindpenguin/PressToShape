using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Reads XAML files from the repository folder. Paths cannot leave it.</summary>
internal sealed class DiskXamlFileSource : IXamlFileSource
{
    private static readonly string[] ExcludedFolders = ["bin", "obj"];

    private readonly string _repoRoot;

    public DiskXamlFileSource(string repoRoot)
    {
        ArgumentException.ThrowIfNullOrEmpty(repoRoot);
        _repoRoot = Path.GetFullPath(repoRoot);
    }

    public IReadOnlyList<string> List(string projectDirectory)
    {
        var directory = Path.Combine(_repoRoot, IndexPath.Normalize(projectDirectory).Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.EnumerateFiles(directory, "*.axaml", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(_repoRoot, f).Replace(Path.DirectorySeparatorChar, '/'))
            .Where(f => !f.Split('/').Any(segment => ExcludedFolders.Contains(segment, StringComparer.Ordinal)))
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();
    }

    public string Read(string repoRelativePath) =>
        File.ReadAllText(Path.Combine(_repoRoot, IndexPath.Normalize(repoRelativePath).Replace('/', Path.DirectorySeparatorChar)));
}
