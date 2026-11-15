using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PressToShape.App.Settings;

namespace PressToShape.App.Workspace;

/// <summary>
/// The list of recently used files, most recent first, kept in the settings file. It holds each path once and at most
/// <see cref="MaxCount"/> paths. The stored value is untrusted: anything that is not a list of paths is ignored.
/// </summary>
internal sealed class RecentFiles
{
    /// <summary>The most paths the list keeps.</summary>
    public const int MaxCount = 10;

    private readonly ISettingsStore _store;
    private readonly List<string> _paths;

    public RecentFiles(ISettingsStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _paths = ReadPaths(_store.Load().RecentFiles);
    }

    /// <summary>Raised after the list changed.</summary>
    public event EventHandler? Changed;

    /// <summary>The paths, most recent first.</summary>
    public IReadOnlyList<string> Paths => _paths;

    /// <summary>Puts a path at the top. An existing entry for the same path moves up. Blank paths are ignored.</summary>
    public void Add(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || (_paths.Count > 0 && _paths[0] == path))
        {
            return;
        }

        _paths.RemoveAll(p => p == path);
        _paths.Insert(0, path);
        if (_paths.Count > MaxCount)
        {
            _paths.RemoveRange(MaxCount, _paths.Count - MaxCount);
        }

        Persist();
    }

    /// <summary>Removes a path. Does nothing if it is not in the list.</summary>
    public void Remove(string path)
    {
        if (_paths.RemoveAll(p => p == path) > 0)
        {
            Persist();
        }
    }

    private void Persist()
    {
        // Load first, so the language and settings this version does not know about are kept.
        var settings = _store.Load();
        settings.RecentFiles = JsonSerializer.SerializeToElement(_paths);
        _store.Save(settings);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static List<string> ReadPaths(JsonElement? stored)
    {
        var paths = new List<string>();
        if (stored is not { ValueKind: JsonValueKind.Array } array)
        {
            return paths;
        }

        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String && item.GetString() is { } path && !string.IsNullOrWhiteSpace(path) && !paths.Contains(path))
            {
                paths.Add(path);
            }
        }

        return paths.Take(MaxCount).ToList();
    }
}
