using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PressToShape.App.Services;

/// <summary>
/// The real file system. Writing goes to a temporary file in the same folder first and then replaces the target,
/// so a failed write cannot damage an existing file. Errors are thrown, the caller shows them to the user.
/// The file work runs on a pool thread, so the calling (UI) thread never waits for the disk.
/// </summary>
internal sealed class LocalFileStore : IFileStore
{
    public Task<byte[]> ReadAllBytesAsync(string path, long maxBytes, CancellationToken cancellationToken = default) =>
        Task.Run(() => ReadAllBytes(path, maxBytes), cancellationToken);

    public Task WriteAllBytesAtomicAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) =>
        Task.Run(() => WriteAllBytesAtomic(path, bytes), cancellationToken);

    private static byte[] ReadAllBytes(string path, long maxBytes)
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

    private static void WriteAllBytesAtomic(string path, byte[] bytes)
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
