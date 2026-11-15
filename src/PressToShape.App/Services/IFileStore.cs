namespace PressToShape.App.Services;

/// <summary>Reads and writes whole files. Lets the file actions be tested without a real file system.</summary>
internal interface IFileStore
{
    /// <summary>Reads a whole file.</summary>
    /// <exception cref="System.IO.InvalidDataException">The file is larger than <paramref name="maxBytes"/>. Nothing is read then.</exception>
    /// <exception cref="System.IO.IOException">The file cannot be read, for example because it does not exist.</exception>
    byte[] ReadAllBytes(string path, long maxBytes);

    /// <summary>
    /// Writes a whole file so that an existing file is only replaced once the new content is complete.
    /// If writing fails, the existing file stays as it was and no partial file stays behind.
    /// </summary>
    /// <exception cref="System.IO.IOException">The file cannot be written.</exception>
    void WriteAllBytesAtomic(string path, byte[] bytes);

    /// <summary>Whether a file exists at the path.</summary>
    bool Exists(string path);
}
