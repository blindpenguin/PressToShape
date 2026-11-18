using System.Text.Json;
using PressToShape.App.Settings;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SnapPreferencesTests
{
    private static JsonElement Json(string text) => JsonDocument.Parse(text).RootElement.Clone();

    [Fact]
    public void Constructor_EmptyStore_UsesDefaults()
    {
        var preferences = new SnapPreferences(new MemorySettingsStore());

        Assert.False(preferences.ShowGrid);
        Assert.False(preferences.SnapToGrid);
        Assert.False(preferences.SnapToObjects);
        Assert.Equal(10, preferences.GridSpacing);
        Assert.True(preferences.ShowRulers);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_EmptyStore_RulersFollowTheLayoutDefault(bool defaultShowRulers)
    {
        var preferences = new SnapPreferences(new MemorySettingsStore(), defaultShowRulers);

        Assert.Equal(defaultShowRulers, preferences.ShowRulers);
    }

    [Fact]
    public void Setters_Changed_AreRestoredByANewInstance()
    {
        var store = new MemorySettingsStore();
        var preferences = new SnapPreferences(store);

        preferences.ShowGrid = true;
        preferences.SnapToGrid = true;
        preferences.SnapToObjects = true;
        preferences.GridSpacing = 25.5;
        preferences.ShowRulers = false;
        var restored = new SnapPreferences(store);

        Assert.True(restored.ShowGrid);
        Assert.True(restored.SnapToGrid);
        Assert.True(restored.SnapToObjects);
        Assert.Equal(25.5, restored.GridSpacing);
        Assert.False(restored.ShowRulers);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.5")]
    [InlineData("1001")]
    [InlineData("-3")]
    [InlineData("\"abc\"")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("[1]")]
    public void Constructor_InvalidStoredSpacing_UsesTheDefault(string stored)
    {
        var store = new MemorySettingsStore { Current = new AppSettings { GridSpacing = Json(stored) } };

        Assert.Equal(10, new SnapPreferences(store).GridSpacing);
    }

    [Fact]
    public void Constructor_DamagedBooleans_UseTheDefaults()
    {
        var store = new MemorySettingsStore
        {
            Current = new AppSettings
            {
                ShowGrid = Json("\"yes\""),
                SnapToGrid = Json("1"),
                SnapToObjects = Json("{}"),
                ShowRulers = Json("\"no\""),
            },
        };

        var preferences = new SnapPreferences(store, defaultShowRulers: false);

        Assert.False(preferences.ShowGrid);
        Assert.False(preferences.SnapToGrid);
        Assert.False(preferences.SnapToObjects);
        Assert.False(preferences.ShowRulers);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    public void GridSpacing_AtTheLimits_IsAccepted(double value)
    {
        var preferences = new SnapPreferences(new MemorySettingsStore());

        preferences.GridSpacing = value;

        Assert.Equal(value, preferences.GridSpacing);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.99)]
    [InlineData(1000.5)]
    [InlineData(-5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void GridSpacing_Invalid_KeepsTheOldValueAndSavesNothing(double value)
    {
        var store = new MemorySettingsStore();
        var preferences = new SnapPreferences(store);
        var raised = 0;
        preferences.Changed += (_, _) => raised++;

        preferences.GridSpacing = value;

        Assert.Equal(10, preferences.GridSpacing);
        Assert.Null(store.Current.GridSpacing);
        Assert.Equal(0, raised);
    }

    [Fact]
    public void Setters_OtherSettingsEntries_AreKept()
    {
        var store = new MemorySettingsStore
        {
            Current = new AppSettings
            {
                Language = "de",
                RecentFiles = Json("[\"/a.pts\"]"),
                ExtraData = new() { ["Future"] = Json("42") },
            },
        };
        var preferences = new SnapPreferences(store);

        preferences.ShowGrid = true;

        Assert.Equal("de", store.Current.Language);
        Assert.Equal("/a.pts", store.Current.RecentFiles!.Value[0].GetString());
        Assert.Equal(42, store.Current.ExtraData!["Future"].GetInt32());
    }

    [Fact]
    public void Setters_RealChange_RaiseChangedOncePerChange()
    {
        var preferences = new SnapPreferences(new MemorySettingsStore());
        var raised = 0;
        preferences.Changed += (_, _) => raised++;

        preferences.ShowGrid = true;
        preferences.ShowGrid = true;
        preferences.SnapToGrid = true;

        Assert.Equal(2, raised);
    }

    [Fact]
    public void Setters_NotPersisting_SaveNothingButStillChange()
    {
        var store = new MemorySettingsStore();
        var preferences = new SnapPreferences(store, persist: false);
        var raised = 0;
        preferences.Changed += (_, _) => raised++;

        preferences.SnapToObjects = true;

        Assert.True(preferences.SnapToObjects);
        Assert.Null(store.Current.SnapToObjects);
        Assert.Equal(1, raised);
    }
}
