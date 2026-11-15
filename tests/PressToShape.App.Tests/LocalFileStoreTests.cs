using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    public async Task Write_NewFileInNewFolder_CreatesBoth()
    {
        var path = Path.Combine(_folder, "sub", "a.p2s");

        await _store.WriteAllBytesAtomicAsync(path, [1, 2, 3]);

        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(path));
    }

    [Fact]
    public async Task Write_ExistingFile_IsReplaced()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), [9, 9, 9, 9, 9]);

        await _store.WriteAllBytesAtomicAsync(PathOf("a.p2s"), [1, 2]);

        Assert.Equal(new byte[] { 1, 2 }, File.ReadAllBytes(PathOf("a.p2s")));
    }

    [Fact]
    public async Task Write_Success_LeavesNoTemporaryFile()
    {
        await _store.WriteAllBytesAtomicAsync(PathOf("a.p2s"), [1]);
        await _store.WriteAllBytesAtomicAsync(PathOf("a.p2s"), [2]);

        Assert.Equal(new[] { "a.p2s" }, FilesIn(_folder));
    }

    [Fact]
    public async Task Write_TargetIsAnExistingDirectory_ThrowsAndLeavesNoTemporaryFile()
    {
        Directory.CreateDirectory(PathOf("target"));

        await Assert.ThrowsAnyAsync<IOException>(() => _store.WriteAllBytesAtomicAsync(PathOf("target"), [1]));

        Assert.Empty(FilesIn(_folder));
        Assert.True(Directory.Exists(PathOf("target")));
    }

    [Fact]
    public async Task Write_FolderIsAFile_Throws()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(PathOf("notafolder"), "x");

        await Assert.ThrowsAnyAsync<IOException>(() => _store.WriteAllBytesAtomicAsync(Path.Combine(PathOf("notafolder"), "a.p2s"), [1]));

        Assert.Equal("x", File.ReadAllText(PathOf("notafolder")));
    }

    [Fact]
    public async Task Write_FolderIsReadOnly_ThrowsAndKeepsTheOldFile()
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

            await Assert.ThrowsAnyAsync<Exception>(() => _store.WriteAllBytesAtomicAsync(PathOf("a.p2s"), [1, 2]));
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
    public async Task Read_ExistingFile_ReturnsTheBytes()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), [4, 5, 6]);

        Assert.Equal(new byte[] { 4, 5, 6 }, await _store.ReadAllBytesAsync(PathOf("a.p2s"), 100));
    }

    [Fact]
    public async Task Read_EmptyFile_ReturnsNoBytes()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), []);

        Assert.Empty(await _store.ReadAllBytesAsync(PathOf("a.p2s"), 100));
    }

    [Fact]
    public async Task Read_FileOverTheLimit_ThrowsInvalidDataException()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), new byte[11]);

        await Assert.ThrowsAsync<InvalidDataException>(() => _store.ReadAllBytesAsync(PathOf("a.p2s"), 10));
    }

    [Fact]
    public async Task Read_FileExactlyAtTheLimit_IsRead()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), new byte[10]);

        Assert.Equal(10, (await _store.ReadAllBytesAsync(PathOf("a.p2s"), 10)).Length);
    }

    [Fact]
    public async Task Read_MissingFile_ThrowsFileNotFoundException()
    {
        Directory.CreateDirectory(_folder);

        await Assert.ThrowsAsync<FileNotFoundException>(() => _store.ReadAllBytesAsync(PathOf("missing.p2s"), 100));
    }

    [Fact]
    public async Task Read_MissingFolder_ThrowsDirectoryNotFoundException()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => _store.ReadAllBytesAsync(Path.Combine(_folder, "no", "a.p2s"), 100));
    }

    [Fact]
    public async Task CancelledToken_ThrowsBeforeTouchingTheFile()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(PathOf("a.p2s"), [1, 2, 3]);
        using var source = new System.Threading.CancellationTokenSource();
        await source.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _store.WriteAllBytesAtomicAsync(PathOf("a.p2s"), [9], source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _store.ReadAllBytesAsync(PathOf("a.p2s"), 100, source.Token));

        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(PathOf("a.p2s")));
    }

    [Fact]
    public async Task Arguments_EmptyPathOrNullBytes_Throw()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _store.WriteAllBytesAtomicAsync(" ", [1]));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _store.WriteAllBytesAtomicAsync(PathOf("a.p2s"), null!));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _store.ReadAllBytesAsync(string.Empty, 1));
    }
}
