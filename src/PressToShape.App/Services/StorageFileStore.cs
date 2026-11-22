using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PressToShape.App.Services;

/// <summary>
/// Reads and writes the files that the user picked (see <see cref="StorageFileRegistry"/>). A picked file cannot be replaced
/// through a temporary file, because the app has no access to its folder, and opening it for writing empties it at once. So a write first reads the
/// old content, and if the write fails the old content is written back. If the app is killed in the middle of a write, the file can stay cut short.
/// Errors of the platform are passed on as <see cref="IOException"/>, so callers handle them like any other file error.
/// </summary>
internal sealed class StorageFileStore : IFileStore
{
    private const int BufferSize = 81920;

    private readonly StorageFileRegistry _registry;

    public StorageFileStore(StorageFileRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task<byte[]> ReadAllBytesAsync(string path, long maxBytes, CancellationToken cancellationToken = default)
    {
        var file = Resolve(path);
        try
        {
            await using var stream = await file.OpenReadAsync();
            return await ReadLimitedAsync(stream, maxBytes, cancellationToken);
        }
        catch (Exception ex) when (ex is not (OperationCanceledException or InvalidDataException or IOException))
        {
            throw new IOException(ex.Message, ex);
        }
    }

    public async Task WriteAllBytesAtomicAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        var file = Resolve(path);
        cancellationToken.ThrowIfCancellationRequested();

        var previous = await TryReadPreviousContentAsync(file, cancellationToken);
        try
        {
            var stream = await file.OpenWriteAsync();
            await using (stream)
            {
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // The old content is already gone when the stream was opened, so put it back. This must not be cancelled.
            await TryRestoreAsync(file, previous);
            if (ex is OperationCanceledException or IOException)
            {
                throw;
            }

            throw new IOException(ex.Message, ex);
        }
    }

    public void Release(string path) => _registry.Unregister(path);

    private IPickedFile Resolve(string path)
    {
        if (!_registry.TryGet(path, out var file) || file is null)
        {
            throw new FileNotFoundException("The file is not known to this app run.", path);
        }

        return file;
    }

    private static async Task<byte[]> ReadLimitedAsync(Stream stream, long maxBytes, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[BufferSize];
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > maxBytes)
            {
                throw new InvalidDataException($"The file is larger than the limit of {maxBytes} bytes.");
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    // Without the old content a failed write cannot be undone, but saving must still work, so a failed read only means "no backup".
    private static async Task<byte[]?> TryReadPreviousContentAsync(IPickedFile file, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await file.OpenReadAsync();
            return await ReadLimitedAsync(stream, int.MaxValue, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return null;
        }
    }

    private static async Task TryRestoreAsync(IPickedFile file, byte[]? previous)
    {
        if (previous is null)
        {
            return;
        }

        try
        {
            var stream = await file.OpenWriteAsync();
            await using (stream)
            {
                await stream.WriteAsync(previous, CancellationToken.None);
                await stream.FlushAsync(CancellationToken.None);
            }
        }
        catch (Exception)
        {
            // Nothing more can be done. The caller reports the original error, which is the one the user needs.
        }
    }
}
