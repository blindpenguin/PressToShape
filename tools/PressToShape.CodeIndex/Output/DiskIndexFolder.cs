using System.Collections.Generic;
using PressToShape.CodeIndex.Query;

namespace PressToShape.CodeIndex.Output;

/// <summary>The index folder on disk. Writes are atomic per file.</summary>
internal sealed class DiskIndexFolder : IIndexFolder
{
    private readonly AtomicFileWriter _writer;
    private readonly DirectoryFileSource _reader;

    public DiskIndexFolder(string rootDirectory)
    {
        _writer = new AtomicFileWriter(rootDirectory);
        _reader = new DirectoryFileSource(rootDirectory);
    }

    public void Write(string relativePath, byte[] content) => _writer.Write(relativePath, content);

    public byte[]? Read(string relativePath) => _reader.Read(relativePath);

    public IReadOnlyList<string> ListFiles() => _reader.ListFiles();

    public IReadOnlyList<string> RemoveStale() => _writer.RemoveStale();
}
