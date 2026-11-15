using System;
using System.IO;

namespace PressToShape.App.Services;

/// <summary>
/// The real file system. Writing goes to a temporary file in the same folder first and then replaces the target,
/// so a failed write cannot damage an existing file. Errors are thrown, the caller shows them to the user.
/// </summary>
internal sealed class LocalFileStore : IFileStore
{
    public byte[] ReadAllBytes(string path, long maxBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length > maxBytes)
        {
            throw new InvalidDataException($"The file has {stream.Length} bytes, the limit is {maxBytes}.");
        }

        var bytes = new byte[stream.Length];
        stream.ReadExactly(bytes);
        return bytes;
    }

    public void WriteAllBytesAtomic(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        var finished = false;
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, path, overwrite: true);
            finished = true;
        }
        finally
        {
            if (!finished)
            {
                TryDelete(temporaryPath);
            }
        }
    }

    public bool Exists(string path) => !string.IsNullOrWhiteSpace(path) && File.Exists(path);

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Nothing else to do for a leftover temporary file. The original error is what the caller needs.
        }
    }
}
