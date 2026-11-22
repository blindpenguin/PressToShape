using System;
using System.IO;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

/// <summary>The only tests that touch the file system, each in its own unique temporary folder that is deleted afterwards.</summary>
public sealed class AtomicFileWriterTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ptsh-index-" + Guid.NewGuid().ToString("N"), "index");

    public void Dispose()
    {
        var top = Path.GetDirectoryName(_root)!;
        if (Directory.Exists(top))
        {
            Directory.Delete(top, recursive: true);
        }
    }

    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);

    [Fact]
    public void Write_NewFile_CreatesFoldersAndContent()
    {
        var writer = new AtomicFileWriter(_root);

        writer.Write("symbols/Core/N.json", Bytes("{}"));

        Assert.Equal("{}", File.ReadAllText(Path.Combine(_root, "symbols", "Core", "N.json")));
    }

    [Fact]
    public void Write_ExistingFile_IsReplacedAndNoTemporaryFileRemains()
    {
        // The file of an earlier run: a writer writes each path once, so the replacement comes from the writer of the next run.
        new AtomicFileWriter(_root).Write("a.json", Bytes("old"));

        new AtomicFileWriter(_root).Write("a.json", Bytes("new"));

        Assert.Equal("new", File.ReadAllText(Path.Combine(_root, "a.json")));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp", SearchOption.AllDirectories));
    }

    [Fact]
    public void Write_TargetCannotBeReplaced_LeavesNoTemporaryFile()
    {
        var writer = new AtomicFileWriter(_root);
        Directory.CreateDirectory(Path.Combine(_root, "blocked.json"));

        Assert.ThrowsAny<IOException>(() => writer.Write("blocked.json", Bytes("x")));

        Assert.Empty(Directory.GetFiles(_root, "*.tmp", SearchOption.AllDirectories));
    }

    [Theory]
    [InlineData("../outside.json")]
    [InlineData("a/../../outside.json")]
    [InlineData("/rooted.json")]
    [InlineData("C:/rooted.json")]
    [InlineData("")]
    [InlineData("a//b.json")]
    public void Write_PathOutsideRoot_IsRejectedAndNothingIsWritten(string path)
    {
        var writer = new AtomicFileWriter(_root);

        Assert.Throws<ArgumentException>(() => writer.Write(path, Bytes("x")));

        Assert.False(Directory.Exists(Path.GetDirectoryName(_root)!) && Directory.GetFiles(Path.GetDirectoryName(_root)!, "*", SearchOption.AllDirectories).Any());
    }

    [Fact]
    public void RemoveStale_DeletesFilesNotWrittenAndEmptyFolders()
    {
        Directory.CreateDirectory(Path.Combine(_root, "old"));
        File.WriteAllText(Path.Combine(_root, "old", "gone.json"), "x");
        File.WriteAllText(Path.Combine(_root, "left.tmp"), "x");
        var writer = new AtomicFileWriter(_root);
        writer.Write("keep.json", Bytes("k"));

        var removed = writer.RemoveStale();

        Assert.Equal(["left.tmp", "old/gone.json"], removed);
        Assert.True(File.Exists(Path.Combine(_root, "keep.json")));
        Assert.False(Directory.Exists(Path.Combine(_root, "old")));
    }

    [Fact]
    public void RemoveStale_NeverTouchesFilesOutsideTheRoot()
    {
        var outside = Path.Combine(Path.GetDirectoryName(_root)!, "neighbor.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(outside, "keep me");
        var writer = new AtomicFileWriter(_root);

        writer.RemoveStale();

        Assert.True(File.Exists(outside));
    }

    [Fact]
    public void RemoveStale_RootDoesNotExist_ReturnsNothing()
    {
        Assert.Empty(new AtomicFileWriter(_root).RemoveStale());
    }

    [Fact]
    public void InMemoryFileSink_Write_KeepsFilesSortedAndRejectsBadPaths()
    {
        var sink = new InMemoryFileSink();

        sink.Write("b.json", Bytes("b"));
        sink.Write("a/x.json", Bytes("a"));

        Assert.Equal(["a/x.json", "b.json"], sink.Files.Keys);
        Assert.Throws<ArgumentException>(() => sink.Write("../x", Bytes("x")));
    }

    [Fact]
    public void Write_FileWithTheOldFixedTemporaryNameExists_IsNeitherChangedNorRemoved()
    {
        Directory.CreateDirectory(_root);
        var foreign = Path.Combine(_root, "a.json.tmp");
        File.WriteAllText(foreign, "temporary file of another run");
        var writer = new AtomicFileWriter(_root);

        writer.Write("a.json", Bytes("new"));

        Assert.Equal("temporary file of another run", File.ReadAllText(foreign));
        Assert.Equal("new", File.ReadAllText(Path.Combine(_root, "a.json")));
    }

    [Fact]
    public void RemoveStale_LeftoverUniqueTemporaryFile_IsRemovedAndReported()
    {
        Directory.CreateDirectory(_root);
        var leftover = "a.json." + Guid.NewGuid().ToString("N") + ".tmp";
        File.WriteAllText(Path.Combine(_root, leftover), "half");
        var writer = new AtomicFileWriter(_root);
        writer.Write("a.json", Bytes("new"));

        var removed = writer.RemoveStale();

        Assert.Equal([leftover], removed);
        Assert.Equal(["a.json"], Directory.GetFiles(_root).Select(Path.GetFileName));
    }

    [Fact]
    public void Write_TwoWritesToDifferentFiles_UseDifferentTemporaryNames()
    {
        var writer = new AtomicFileWriter(_root);
        writer.Write("a.json", Bytes("a"));
        writer.Write("b.json", Bytes("b"));

        Assert.Empty(Directory.GetFiles(_root, "*.tmp", SearchOption.AllDirectories));
        Assert.Equal(["a.json", "b.json"], Directory.GetFiles(_root).Select(Path.GetFileName).Order(StringComparer.Ordinal));
    }
}
