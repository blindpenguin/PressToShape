using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PressToShape.App.Settings;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class RecentFilesTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "PressToShape.Tests." + Guid.NewGuid().ToString("N"));

    private sealed class MemoryStore : ISettingsStore
    {
        public AppSettings Current { get; set; } = new();

        public int SaveCount { get; private set; }

        public AppSettings Load() => Current;

        public void Save(AppSettings settings)
        {
            Current = settings;
            SaveCount++;
        }
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // Temp cleanup is best effort.
        }
    }

    private static MemoryStore StoreWith(string json) => new() { Current = JsonSerializer.Deserialize<AppSettings>(json)! };

    [Fact]
    public void WithoutPersisting_AddAndRemove_DoNothing()
    {
        var store = new MemoryStore();
        var recent = new RecentFiles(store, persist: false);
        var raised = 0;
        recent.Changed += (_, _) => raised++;

        recent.Add("/a.p2s");
        recent.Remove("/a.p2s");

        Assert.Empty(recent.Paths);
        Assert.Equal(0, store.SaveCount);
        Assert.Equal(0, raised);
        Assert.Null(store.Current.RecentFiles);
    }

    [Fact]
    public void WithoutPersisting_AStoredList_IsIgnoredAndKept()
    {
        var store = StoreWith("""{"RecentFiles":["/old.p2s"]}""");
        var recent = new RecentFiles(store, persist: false);

        recent.Add("/new.p2s");

        Assert.Empty(recent.Paths);
        Assert.Equal(0, store.SaveCount);
        Assert.Equal("/old.p2s", Assert.Single(store.Current.RecentFiles!.Value.EnumerateArray()).GetString());
    }

    [Fact]
    public void WithPersisting_IsTheDefault()
    {
        var store = new MemoryStore();
        var recent = new RecentFiles(store);

        recent.Add("/a.p2s");

        Assert.Equal(new[] { "/a.p2s" }, recent.Paths);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public void New_WithoutStoredList_IsEmpty()
    {
        Assert.Empty(new RecentFiles(new MemoryStore()).Paths);
    }

    [Fact]
    public void Add_NewPaths_GoToTheTop()
    {
        var recent = new RecentFiles(new MemoryStore());

        recent.Add("/a.p2s");
        recent.Add("/b.p2s");

        Assert.Equal(new[] { "/b.p2s", "/a.p2s" }, recent.Paths);
    }

    [Fact]
    public void Add_ExistingPath_MovesToTheTopWithoutDuplicate()
    {
        var recent = new RecentFiles(new MemoryStore());
        recent.Add("/a.p2s");
        recent.Add("/b.p2s");
        recent.Add("/c.p2s");

        recent.Add("/a.p2s");

        Assert.Equal(new[] { "/a.p2s", "/c.p2s", "/b.p2s" }, recent.Paths);
    }

    [Fact]
    public void Add_PathsThatDifferOnlyInCase_AreDifferentEntries()
    {
        var recent = new RecentFiles(new MemoryStore());

        recent.Add("/a.p2s");
        recent.Add("/A.p2s");

        Assert.Equal(2, recent.Paths.Count);
    }

    [Fact]
    public void Add_MoreThanTheLimit_DropsTheOldest()
    {
        var recent = new RecentFiles(new MemoryStore());

        for (var i = 1; i <= 12; i++)
        {
            recent.Add($"/f{i}.p2s");
        }

        Assert.Equal(RecentFiles.MaxCount, recent.Paths.Count);
        Assert.Equal("/f12.p2s", recent.Paths[0]);
        Assert.Equal("/f3.p2s", recent.Paths[^1]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_BlankPath_IsIgnored(string path)
    {
        var store = new MemoryStore();
        var recent = new RecentFiles(store);

        recent.Add(path);

        Assert.Empty(recent.Paths);
        Assert.Equal(0, store.SaveCount);
    }

    [Fact]
    public void Remove_ExistingPath_RemovesIt()
    {
        var recent = new RecentFiles(new MemoryStore());
        recent.Add("/a.p2s");
        recent.Add("/b.p2s");

        recent.Remove("/a.p2s");

        Assert.Equal(new[] { "/b.p2s" }, recent.Paths);
    }

    [Fact]
    public void Changed_IsRaisedOnlyForRealChanges()
    {
        var store = new MemoryStore();
        var recent = new RecentFiles(store);
        var count = 0;
        recent.Changed += (_, _) => count++;

        recent.Add("/a.p2s");
        Assert.Equal(1, count);

        recent.Add("/a.p2s"); // already on top
        recent.Remove("/missing.p2s");
        recent.Add(" ");
        Assert.Equal(1, count);
        Assert.Equal(1, store.SaveCount);

        recent.Add("/b.p2s");
        recent.Remove("/b.p2s");
        Assert.Equal(3, count);
    }

    [Fact]
    public void Changes_ArePersistedAndReadByANewInstance()
    {
        var store = new MemoryStore();
        var first = new RecentFiles(store);
        first.Add("/a.p2s");
        first.Add("/b.p2s");

        var second = new RecentFiles(store);

        Assert.Equal(new[] { "/b.p2s", "/a.p2s" }, second.Paths);
    }

    [Fact]
    public void Save_KeepsTheLanguageAndUnknownEntries()
    {
        var store = StoreWith("{\"Language\":\"de\",\"Future\":{\"a\":1}}");
        var recent = new RecentFiles(store);

        recent.Add("/a.p2s");

        Assert.Equal("de", store.Current.Language);
        Assert.NotNull(store.Current.ExtraData);
        Assert.True(store.Current.ExtraData!.ContainsKey("Future"));
    }

    [Theory]
    [InlineData("{\"RecentFiles\":42}")]
    [InlineData("{\"RecentFiles\":\"text\"}")]
    [InlineData("{\"RecentFiles\":{\"a\":\"b\"}}")]
    [InlineData("{\"RecentFiles\":true}")]
    [InlineData("{\"RecentFiles\":null}")]
    [InlineData("{\"RecentFiles\":[]}")]
    [InlineData("{\"RecentFiles\":[1,2,null,{},[],\"\",\"  \"]}")]
    public void New_DamagedStoredValue_GivesAnEmptyList(string json)
    {
        Assert.Empty(new RecentFiles(StoreWith(json)).Paths);
    }

    [Fact]
    public void New_MixedStoredList_KeepsOnlyTheGoodPathsOnceAndAtMostTen()
    {
        var paths = string.Join(",", Enumerable.Range(1, 15).Select(i => $"\"/f{i}.p2s\""));
        var store = StoreWith("{\"RecentFiles\":[1,\"/x.p2s\",\"/x.p2s\",null,\"\"," + paths + "]}");

        var recent = new RecentFiles(store);

        Assert.Equal("/x.p2s", recent.Paths[0]);
        Assert.Equal(RecentFiles.MaxCount, recent.Paths.Count);
        Assert.Equal(recent.Paths.Distinct().Count(), recent.Paths.Count);
    }

    [Fact]
    public void RealSettingsFile_DamagedListDoesNotLoseTheLanguage_AndTheListSurvivesRestarts()
    {
        Directory.CreateDirectory(_folder);
        var path = Path.Combine(_folder, "settings.json");
        File.WriteAllText(path, "{\"Language\":\"de\",\"RecentFiles\":42}");
        var store = new JsonFileSettingsStore(path);
        var recent = new RecentFiles(store);
        Assert.Empty(recent.Paths);

        recent.Add("/a.p2s");
        recent.Add("/b.p2s");

        var again = new RecentFiles(new JsonFileSettingsStore(path));
        Assert.Equal(new[] { "/b.p2s", "/a.p2s" }, again.Paths);
        Assert.Equal("de", new JsonFileSettingsStore(path).Load().Language);
        Assert.Contains("\"RecentFiles\"", File.ReadAllText(path), StringComparison.Ordinal);
    }

    [Fact]
    public void New_NullStore_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RecentFiles(null!));
    }

    [Fact]
    public void Paths_AreAReadOnlyViewOfTheCurrentList()
    {
        var recent = new RecentFiles(new MemoryStore());
        IReadOnlyList<string> view = recent.Paths;

        recent.Add("/a.p2s");

        Assert.Equal(new[] { "/a.p2s" }, view);
    }
}
