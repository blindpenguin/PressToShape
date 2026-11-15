using System;
using System.IO;
using System.Linq;
using PressToShape.App.Services;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class LocalFileStoreTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "PressToShape.Tests." + Guid.NewGuid().ToString("N"));
    private readonly LocalFileStore _store = new();

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(_folder, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                }

                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // Temp cleanup is best effort.
        }
    }

    private string PathOf(string name) => Path.Combine(_folder, name);

    private string[] FilesIn(string folder) => Directory.GetFiles(folder).Select(Path.GetFileName).Cast<string>().OrderBy(n => n).ToArray();

    [Fact]
    public void Write_NewFileInNewFolder_CreatesBoth()
    {
        var path = Path.Combine(_folder, "sub", "a.p2s");

        _store.WriteAllBytesAtomic(path, [1, 2, 3]);

        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(path));
    }

    [Fact]
    public void Write_ExistingFile_IsReplaced()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), [9, 9, 9, 9, 9]);

        _store.WriteAllBytesAtomic(PathOf("a.p2s"), [1, 2]);

        Assert.Equal(new byte[] { 1, 2 }, File.ReadAllBytes(PathOf("a.p2s")));
    }

    [Fact]
    public void Write_Success_LeavesNoTemporaryFile()
    {
        _store.WriteAllBytesAtomic(PathOf("a.p2s"), [1]);
        _store.WriteAllBytesAtomic(PathOf("a.p2s"), [2]);

        Assert.Equal(new[] { "a.p2s" }, FilesIn(_folder));
    }

    [Fact]
    public void Write_TargetIsAnExistingDirectory_ThrowsAndLeavesNoTemporaryFile()
    {
        Directory.CreateDirectory(PathOf("target"));

        Assert.ThrowsAny<IOException>(() => _store.WriteAllBytesAtomic(PathOf("target"), [1]));

        Assert.Empty(FilesIn(_folder));
        Assert.True(Directory.Exists(PathOf("target")));
    }

    [Fact]
    public void Write_FolderIsAFile_Throws()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(PathOf("notafolder"), "x");

        Assert.ThrowsAny<IOException>(() => _store.WriteAllBytesAtomic(Path.Combine(PathOf("notafolder"), "a.p2s"), [1]));

        Assert.Equal("x", File.ReadAllText(PathOf("notafolder")));
    }

    [Fact]
    public void Write_FolderIsReadOnly_ThrowsAndKeepsTheOldFile()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), [7, 7, 7]);
        File.SetUnixFileMode(_folder, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        try
        {
            if (CanWriteDespiteMode())
            {
                return; // Running with rights that ignore the mode (for example root): nothing to check here.
            }

            Assert.ThrowsAny<Exception>(() => _store.WriteAllBytesAtomic(PathOf("a.p2s"), [1, 2]));
        }
        finally
        {
            File.SetUnixFileMode(_folder, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        Assert.Equal(new byte[] { 7, 7, 7 }, File.ReadAllBytes(PathOf("a.p2s")));
        Assert.Equal(new[] { "a.p2s" }, FilesIn(_folder));
    }

    private bool CanWriteDespiteMode()
    {
        try
        {
            var probe = PathOf("probe.tmp");
            File.WriteAllBytes(probe, []);
            File.Delete(probe);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    [Fact]
    public void Read_ExistingFile_ReturnsTheBytes()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), [4, 5, 6]);

        Assert.Equal(new byte[] { 4, 5, 6 }, _store.ReadAllBytes(PathOf("a.p2s"), 100));
    }

    [Fact]
    public void Read_EmptyFile_ReturnsNoBytes()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), []);

        Assert.Empty(_store.ReadAllBytes(PathOf("a.p2s"), 100));
    }

    [Fact]
    public void Read_FileOverTheLimit_ThrowsInvalidDataException()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), new byte[11]);

        Assert.Throws<InvalidDataException>(() => _store.ReadAllBytes(PathOf("a.p2s"), 10));
    }

    [Fact]
    public void Read_FileExactlyAtTheLimit_IsRead()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), new byte[10]);

        Assert.Equal(10, _store.ReadAllBytes(PathOf("a.p2s"), 10).Length);
    }

    [Fact]
    public void Read_MissingFile_ThrowsFileNotFoundException()
    {
        Directory.CreateDirectory(_folder);

        Assert.Throws<FileNotFoundException>(() => _store.ReadAllBytes(PathOf("missing.p2s"), 100));
    }

    [Fact]
    public void Read_MissingFolder_ThrowsDirectoryNotFoundException()
    {
        Assert.Throws<DirectoryNotFoundException>(() => _store.ReadAllBytes(Path.Combine(_folder, "no", "a.p2s"), 100));
    }

    [Fact]
    public void Exists_ReportsFilesOnly()
    {
        Directory.CreateDirectory(PathOf("dir"));
        File.WriteAllBytes(PathOf("a.p2s"), [1]);

        Assert.True(_store.Exists(PathOf("a.p2s")));
        Assert.False(_store.Exists(PathOf("missing.p2s")));
        Assert.False(_store.Exists(PathOf("dir")));
        Assert.False(_store.Exists(string.Empty));
    }

    [Fact]
    public void Arguments_EmptyPathOrNullBytes_Throw()
    {
        Assert.ThrowsAny<ArgumentException>(() => _store.WriteAllBytesAtomic(" ", [1]));
        Assert.Throws<ArgumentNullException>(() => _store.WriteAllBytesAtomic(PathOf("a.p2s"), null!));
        Assert.ThrowsAny<ArgumentException>(() => _store.ReadAllBytes(string.Empty, 1));
    }
}
