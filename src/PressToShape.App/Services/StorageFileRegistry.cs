using System;
using System.Collections.Concurrent;
using Avalonia.Platform.Storage;

namespace PressToShape.App.Services;

/// <summary>
/// Remembers the files that the platform's file picker returned, and hands out a text reference for each. The rest of the app works with
/// file paths as text, but the Android picker gives no path, only a handle. A reference looks like <c>storage:&lt;id&gt;/&lt;file name&gt;</c>,
/// so code that shows "the name after the last slash" shows the real file name. References only live as long as the app runs.
/// </summary>
internal sealed class StorageFileRegistry
{
    private const string Prefix = "storage:";

    private readonly ConcurrentDictionary<string, IPickedFile> _files = new(StringComparer.Ordinal);

    /// <summary>Remembers a file from the platform's file picker and returns its reference.</summary>
    public string Register(IStorageFile file) => Register(new PickedStorageFile(file ?? throw new ArgumentNullException(nameof(file))));

    /// <summary>Remembers a picked file and returns its reference.</summary>
    public string Register(IPickedFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var reference = $"{Prefix}{Guid.NewGuid():N}/{SafeName(file.Name)}";
        _files[reference] = file;
        return reference;
    }

    /// <summary>Forgets the file of a reference, so its platform handle can be released. A reference that is unknown, or no reference, does nothing.</summary>
    public void Unregister(string reference)
    {
        if (reference is not null)
        {
            _files.TryRemove(reference, out _);
        }
    }

    /// <summary>Whether a path is a reference of this registry. Such a name is the user's choice in the system picker and is never changed by the app.</summary>
    public static bool IsReference(string path) => path is not null && path.StartsWith(Prefix, StringComparison.Ordinal);

    /// <summary>Finds the file of a reference that <see cref="Register"/> gave out.</summary>
    public bool TryGet(string reference, out IPickedFile? file)
    {
        file = null;
        return reference is not null && _files.TryGetValue(reference, out file);
    }

    // A name with a separator would break the "name after the last slash" rule.
    private static string SafeName(string name) => string.IsNullOrWhiteSpace(name) ? "file" : name.Replace('/', '_').Replace('\\', '_');
}
