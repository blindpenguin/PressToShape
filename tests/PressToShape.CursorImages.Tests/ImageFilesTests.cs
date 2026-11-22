using System;
using System.IO;
using Xunit;

namespace PressToShape.CursorImages.Tests;

public sealed class ImageFilesTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "PressToShape.CursorImages.Tests", Guid.NewGuid().ToString("N"));

    public ImageFilesTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
    }

    // A folder in the place of the target makes the move fail, without any fault injection into the operating system.
    private string BlockedTarget()
    {
        var target = Path.Combine(_folder, "blocked.png");
        Directory.CreateDirectory(target);
        return target;
    }

    [Fact]
    public void WriteAtomic_MoveFailsAndTheCleanupFailsToo_ThrowsTheMoveException()
    {
        var target = BlockedTarget();

        var ex = Record.Exception(() => ImageFiles.WriteAtomic(target, [1, 2, 3], _ => throw new IOException("delete failed")));

        Assert.NotNull(ex);
        Assert.DoesNotContain("delete failed", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WriteAtomic_MoveFailsAndTheCleanupHasNoAccess_ThrowsTheMoveException()
    {
        var target = BlockedTarget();

        var ex = Record.Exception(() => ImageFiles.WriteAtomic(target, [1, 2, 3], _ => throw new UnauthorizedAccessException("no access to delete")));

        Assert.NotNull(ex);
        Assert.DoesNotContain("no access to delete", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WriteAtomic_MoveFails_RemovesTheTemporaryFileAndThrows()
    {
        var target = BlockedTarget();

        Assert.ThrowsAny<Exception>(() => ImageFiles.WriteAtomic(target, [1, 2, 3]));

        Assert.Empty(Directory.GetFiles(_folder, "*.tmp", SearchOption.AllDirectories));
    }

    [Fact]
    public void WriteAtomic_Works_WritesTheContentAndLeavesNoTemporaryFile()
    {
        var target = Path.Combine(_folder, "a.png");
        File.WriteAllBytes(target, [9]);

        ImageFiles.WriteAtomic(target, [1, 2, 3]);

        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(target));
        Assert.Empty(Directory.GetFiles(_folder, "*.tmp", SearchOption.AllDirectories));
    }
}
