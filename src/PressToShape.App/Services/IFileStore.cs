using System.Threading;
using System.Threading.Tasks;

namespace PressToShape.App.Services;

/// <summary>Reads and writes whole files. Lets the file actions be tested without a real file system, and lets a platform without file paths supply its own storage.</summary>
internal interface IFileStore
{
    /// <summary>Reads a whole file.</summary>
    /// <exception cref="System.IO.InvalidDataException">The file is larger than <paramref name="maxBytes"/>. Nothing is read then.</exception>
    /// <exception cref="System.IO.IOException">The file cannot be read, for example because it does not exist.</exception>
    Task<byte[]> ReadAllBytesAsync(string path, long maxBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a whole file so that an existing file is only replaced once the new content is complete.
    /// If writing fails, the existing file stays as it was and no partial file stays behind.
    /// </summary>
    /// <exception cref="System.IO.IOException">The file cannot be written.</exception>
    Task WriteAllBytesAtomicAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tells the store that a path will not be used again, so it can free what it holds for it (the file of a system picker). Does nothing for
    /// a path that the store does not hold. The default does nothing, because a normal file path holds nothing.
    /// </summary>
    void Release(string path)
    {
    }
}
