using PressToShape.App.Services;
using Xunit;

namespace PressToShape.App.Tests;

public class StorageFileRegistryTests
{
    [Fact]
    public void Register_ReturnsAReferenceThatEndsWithTheFileName()
    {
        var registry = new StorageFileRegistry();

        var reference = registry.Register(new FakePickedFile("plan.p2s"));

        Assert.StartsWith("storage:", reference);
        Assert.Equal("plan.p2s", reference[(reference.LastIndexOf('/') + 1)..]);
    }

    [Fact]
    public void TryGet_ReturnsTheRegisteredFile()
    {
        var registry = new StorageFileRegistry();
        var file = new FakePickedFile("a.p2s");

        var reference = registry.Register(file);

        Assert.True(registry.TryGet(reference, out var found));
        Assert.Same(file, found);
    }

    [Fact]
    public void Register_TheSameFileTwice_GivesTwoWorkingReferences()
    {
        var registry = new StorageFileRegistry();
        var file = new FakePickedFile("a.p2s");

        var first = registry.Register(file);
        var second = registry.Register(file);

        Assert.NotEqual(first, second);
        Assert.True(registry.TryGet(first, out _));
        Assert.True(registry.TryGet(second, out _));
    }

    [Theory]
    [InlineData("storage:unknown/a.p2s")]
    [InlineData("/home/user/a.p2s")]
    [InlineData("")]
    public void TryGet_UnknownReference_ReturnsFalse(string reference)
    {
        var registry = new StorageFileRegistry();

        Assert.False(registry.TryGet(reference, out var found));
        Assert.Null(found);
    }

    [Theory]
    [InlineData("a/b.p2s", "a_b.p2s")]
    [InlineData("a\\b.p2s", "a_b.p2s")]
    [InlineData("", "file")]
    public void Register_NameWithSeparatorsOrEmpty_StillEndsWithASingleName(string name, string expectedEnd)
    {
        var registry = new StorageFileRegistry();

        var reference = registry.Register(new FakePickedFile(name));

        Assert.Equal(expectedEnd, reference[(reference.LastIndexOf('/') + 1)..]);
    }

    [Theory]
    [InlineData("storage:0123/a.p2s", true)]
    [InlineData("storage:", true)]
    [InlineData("/home/user/storage:a.p2s", false)]
    [InlineData("Storage:0123/a.p2s", false)]
    [InlineData("", false)]
    public void IsReference_RecognizesOnlyTheRegistryPrefix(string path, bool expected)
    {
        Assert.Equal(expected, StorageFileRegistry.IsReference(path));
    }

    [Fact]
    public void IsReference_ForARegisteredFile_IsTrue()
    {
        var registry = new StorageFileRegistry();

        Assert.True(StorageFileRegistry.IsReference(registry.Register(new FakePickedFile("a.p2s"))));
    }

    [Fact]
    public void Unregister_RegisteredReference_IsNoLongerFound()
    {
        var registry = new StorageFileRegistry();
        var reference = registry.Register(new FakePickedFile("a.p2s"));

        registry.Unregister(reference);

        Assert.False(registry.TryGet(reference, out var found));
        Assert.Null(found);
    }

    [Fact]
    public void Unregister_OneReference_KeepsTheOthers()
    {
        var registry = new StorageFileRegistry();
        var first = registry.Register(new FakePickedFile("a.p2s"));
        var second = registry.Register(new FakePickedFile("b.p2s"));

        registry.Unregister(first);

        Assert.True(registry.TryGet(second, out _));
    }

    [Fact]
    public void Unregister_UnknownOrNullReference_DoesNothing()
    {
        var registry = new StorageFileRegistry();
        var reference = registry.Register(new FakePickedFile("a.p2s"));

        registry.Unregister("storage:unknown/x.p2s");
        registry.Unregister(null!);
        registry.Unregister("/home/user/a.p2s");

        Assert.True(registry.TryGet(reference, out _));
    }
}
