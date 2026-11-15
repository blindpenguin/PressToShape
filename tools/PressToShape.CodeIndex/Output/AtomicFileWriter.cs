using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PressToShape.CodeIndex.Output;

/// <summary>Writes files below one root folder. Each file is written to a temporary name first and then moved over the target, so an aborted run never leaves a half-written file.</summary>
internal sealed class AtomicFileWriter : IIndexFileSink
{
    private const string TemporarySuffix = ".tmp";

    private readonly string _root;
    private readonly HashSet<string> _written = new(StringComparer.Ordinal);

    public AtomicFileWriter(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootDirectory);
        _root = Path.GetFullPath(rootDirectory);
    }

    public void Write(string relativePath, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var normalized = IndexPath.Normalize(relativePath);
        var target = Path.Combine(_root, normalized.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

        var temporary = target + TemporarySuffix;
        try
        {
            File.WriteAllBytes(temporary, content);
            File.Move(temporary, target, overwrite: true);
        }
        catch
        {
            TryDelete(temporary);
            throw;
        }

        _written.Add(normalized);
    }

    /// <summary>Deletes every file below the root that this writer did not write, and folders that become empty. Only the root folder is touched.</summary>
    public IReadOnlyList<string> RemoveStale()
    {
        var removed = new List<string>();
        if (!Directory.Exists(_root))
        {
            return removed;
        }

        foreach (var file in Directory.GetFiles(_root, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(_root, file).Replace(Path.DirectorySeparatorChar, '/');
            if (!_written.Contains(relative))
            {
                File.Delete(file);
                removed.Add(relative);
            }
        }

        foreach (var directory in Directory.GetDirectories(_root, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
        {
            if (!Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }

        return removed;
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // The original exception is the one that matters.
        }
        catch (UnauthorizedAccessException)
        {
            // Same as above.
        }
    }
}
