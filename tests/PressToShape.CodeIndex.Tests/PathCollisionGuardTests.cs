using System;
using System.IO;
using System.Text;
using PressToShape.CodeIndex;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

/// <summary>Two source files must never produce the same index file, also not on a file system that ignores the case of names.</summary>
public sealed class PathCollisionGuardTests : IDisposable
{
    private readonly string _top = Path.Combine(Path.GetTempPath(), "ptsh-collision-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_top))
        {
            Directory.Delete(_top, recursive: true);
        }
    }

    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);

    [Fact]
    public void Register_DifferentPaths_Pass()
    {
        var guard = new PathCollisionGuard();

        guard.Register("symbols/Core/A.json");
        guard.Register("symbols/Core/B.json");
        guard.Register("symbols/Other/A.json");
    }

    [Fact]
    public void Register_TheSamePathTwice_ThrowsAndNamesThePath()
    {
        var guard = new PathCollisionGuard();
        guard.Register("xaml/App/A.B.json");

        var ex = Assert.Throws<IndexToolException>(() => guard.Register("xaml/App/A.B.json"));

        Assert.Contains("xaml/App/A.B.json", ex.Message);
        Assert.Contains("Rename one of the source files", ex.Message);
    }

    [Fact]
    public void Register_PathThatDiffersOnlyInCase_ThrowsAndNamesBothPaths()
    {
        var guard = new PathCollisionGuard();
        guard.Register("symbols/Core/Foo.json");

        var ex = Assert.Throws<IndexToolException>(() => guard.Register("symbols/Core/foo.json"));

        Assert.Contains("symbols/Core/foo.json", ex.Message);
        Assert.Contains("symbols/Core/Foo.json", ex.Message);
    }

    [Fact]
    public void InMemoryFileSink_SecondWriteOfAPath_ThrowsAndKeepsTheFirstContent()
    {
        var sink = new InMemoryFileSink();
        sink.Write("a/x.json", Bytes("first"));

        Assert.Throws<IndexToolException>(() => sink.Write("a/x.json", Bytes("second")));

        Assert.Equal("first", Encoding.UTF8.GetString(sink.Files["a/x.json"]));
    }

    [Fact]
    public void InMemoryFileSink_PathThatDiffersOnlyInCase_Throws()
    {
        var sink = new InMemoryFileSink();
        sink.Write("a/Foo.json", Bytes("first"));

        Assert.Throws<IndexToolException>(() => sink.Write("a/foo.json", Bytes("second")));

        Assert.Single(sink.Files);
    }

    [Fact]
    public void AtomicFileWriter_SecondWriteOfAPath_ThrowsAndKeepsTheFirstFile()
    {
        var root = Path.Combine(_top, "index");
        var writer = new AtomicFileWriter(root);
        writer.Write("a/x.json", Bytes("first"));

        Assert.Throws<IndexToolException>(() => writer.Write("a/x.json", Bytes("second")));

        Assert.Equal("first", File.ReadAllText(Path.Combine(root, "a", "x.json")));
    }

    [Fact]
    public void AtomicFileWriter_PathThatDiffersOnlyInCase_ThrowsBeforeTouchingTheDisk()
    {
        var root = Path.Combine(_top, "index");
        var writer = new AtomicFileWriter(root);
        writer.Write("a/Foo.json", Bytes("first"));

        Assert.Throws<IndexToolException>(() => writer.Write("a/foo.json", Bytes("second")));

        Assert.Single(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
    }
}
