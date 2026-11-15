using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.App.Services;
using Xunit;

namespace PressToShape.App.Tests;

public class StorageFileStoreTests
{
    private readonly StorageFileRegistry _registry = new();
    private readonly StorageFileStore _store;

    public StorageFileStoreTests() => _store = new StorageFileStore(_registry);

    private string Pick(FakePickedFile file) => _registry.Register(file);

    [Fact]
    public async Task Read_PickedFile_ReturnsTheBytes()
    {
        var reference = Pick(new FakePickedFile("a.p2s", [1, 2, 3]));

        Assert.Equal(new byte[] { 1, 2, 3 }, await _store.ReadAllBytesAsync(reference, 100));
    }

    [Fact]
    public async Task Read_EmptyFile_ReturnsNoBytes()
    {
        var reference = Pick(new FakePickedFile("a.p2s"));

        Assert.Empty(await _store.ReadAllBytesAsync(reference, 100));
    }

    [Fact]
    public async Task Read_FileExactlyAtTheLimit_IsRead()
    {
        var reference = Pick(new FakePickedFile("a.p2s", new byte[10]));

        Assert.Equal(10, (await _store.ReadAllBytesAsync(reference, 10)).Length);
    }

    [Fact]
    public async Task Read_FileOverTheLimit_ThrowsInvalidDataException()
    {
        var reference = Pick(new FakePickedFile("a.p2s", new byte[11]));

        await Assert.ThrowsAsync<InvalidDataException>(() => _store.ReadAllBytesAsync(reference, 10));
    }

    [Fact]
    public async Task Read_LargeFile_IsReadInSeveralChunks()
    {
        var content = Enumerable.Range(0, 300_000).Select(i => (byte)(i % 251)).ToArray();
        var reference = Pick(new FakePickedFile("big.p2s", content));

        Assert.Equal(content, await _store.ReadAllBytesAsync(reference, 1_000_000));
    }

    [Fact]
    public async Task Read_UnknownReference_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => _store.ReadAllBytesAsync("storage:unknown/a.p2s", 100));
    }

    [Fact]
    public async Task Read_PlatformFailure_BecomesAnIOException()
    {
        var reference = Pick(new FakePickedFile("a.p2s") { OpenReadFailure = new InvalidOperationException("provider died") });

        var error = await Assert.ThrowsAsync<IOException>(() => _store.ReadAllBytesAsync(reference, 100));

        Assert.IsType<InvalidOperationException>(error.InnerException);
    }

    [Fact]
    public async Task Write_ReplacesTheContent()
    {
        var file = new FakePickedFile("a.p2s", [9, 9, 9, 9, 9]);

        await _store.WriteAllBytesAtomicAsync(Pick(file), [1, 2]);

        Assert.Equal(new byte[] { 1, 2 }, file.Content);
    }

    [Fact]
    public async Task Write_NewEmptyFile_WritesTheBytes()
    {
        var file = new FakePickedFile("new.p2s");

        await _store.WriteAllBytesAtomicAsync(Pick(file), [4, 5, 6]);

        Assert.Equal(new byte[] { 4, 5, 6 }, file.Content);
    }

    [Fact]
    public async Task Write_FailingMidway_WhenTheRestoreFailsToo_StillThrowsTheOriginalError()
    {
        // The disk stays full: the new content fails after 2 bytes and the 4 bytes of the restore fail as well.
        var file = new FakePickedFile("a.p2s", [7, 7, 7, 7]) { FailWritingAfterBytes = 2 };

        var error = await Assert.ThrowsAsync<IOException>(() => _store.WriteAllBytesAtomicAsync(Pick(file), [1, 2, 3, 4, 5, 6]));

        Assert.Equal("The disk is full.", error.Message);
        Assert.Equal(2, file.WriteOpenCount);
    }

    [Fact]
    public async Task Write_FailingMidway_WhenTheRestoreCanSucceed_LeavesTheOldContent()
    {
        var file = new FakePickedFile("a.p2s", [7, 7]) { FailWritingAfterBytes = 2 };

        // The new content (6 bytes) fails after 2 bytes. The restore (2 bytes) fits into the limit of 2 bytes.
        await Assert.ThrowsAsync<IOException>(() => _store.WriteAllBytesAtomicAsync(Pick(file), [1, 2, 3, 4, 5, 6]));

        Assert.Equal(new byte[] { 7, 7 }, file.Content);
        Assert.Equal(2, file.WriteOpenCount);
    }

    [Fact]
    public async Task Write_WhenTheOldContentCannotBeRead_StillWrites()
    {
        var file = new FakePickedFile("a.p2s", [7]) { OpenReadFailure = new IOException("no read access") };

        await _store.WriteAllBytesAtomicAsync(Pick(file), [1, 2]);

        Assert.Equal(new byte[] { 1, 2 }, file.Content);
    }

    [Fact]
    public async Task Write_CancelledBeforeStarting_DoesNotTouchTheFile()
    {
        var file = new FakePickedFile("a.p2s", [7, 7]);
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _store.WriteAllBytesAtomicAsync(Pick(file), [1], source.Token));

        Assert.Equal(new byte[] { 7, 7 }, file.Content);
        Assert.Equal(0, file.WriteOpenCount);
    }

    [Fact]
    public async Task Write_UnknownReference_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => _store.WriteAllBytesAtomicAsync("storage:unknown/a.p2s", [1]));
    }

    [Fact]
    public async Task Write_NullBytes_Throws()
    {
        var reference = Pick(new FakePickedFile("a.p2s"));

        await Assert.ThrowsAsync<ArgumentNullException>(() => _store.WriteAllBytesAtomicAsync(reference, null!));
    }

    [Fact]
    public void Constructor_NullRegistry_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new StorageFileStore(null!));
    }
}
