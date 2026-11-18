using System.Text.Json;
using PressToShape.App.Settings;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SnapPreferencesSmartTests
{
    private static JsonElement Json(string text) => JsonDocument.Parse(text).RootElement.Clone();

    [Fact]
    public void Defaults_SmartGuidesOff_ShowGuidesOn()
    {
        var preferences = new SnapPreferences(new MemorySettingsStore());

        Assert.False(preferences.SmartGuides);
        Assert.True(preferences.ShowGuides);
    }

    [Fact]
    public void Setters_AreRestoredByANewInstance()
    {
        var store = new MemorySettingsStore();
        var preferences = new SnapPreferences(store);

        preferences.SmartGuides = true;
        preferences.ShowGuides = false;
        var restored = new SnapPreferences(store);

        Assert.True(restored.SmartGuides);
        Assert.False(restored.ShowGuides);
    }

    [Theory]
    [InlineData("\"yes\"")]
    [InlineData("1")]
    [InlineData("{}")]
    [InlineData("null")]
    public void DamagedValues_GiveTheDefaults(string stored)
    {
        var store = new MemorySettingsStore { Current = new AppSettings { SmartGuides = Json(stored), ShowGuides = Json(stored) } };

        var preferences = new SnapPreferences(store);

        Assert.False(preferences.SmartGuides);
        Assert.True(preferences.ShowGuides);
    }

    [Fact]
    public void Setters_OtherEntriesSurvive()
    {
        var store = new MemorySettingsStore
        {
            Current = new AppSettings { Language = "de", ShowGrid = Json("true"), ExtraData = new() { ["Future"] = Json("42") } },
        };
        var preferences = new SnapPreferences(store);

        preferences.SmartGuides = true;

        Assert.Equal("de", store.Current.Language);
        Assert.True(new SnapPreferences(store).ShowGrid);
        Assert.Equal(42, store.Current.ExtraData!["Future"].GetInt32());
    }

    [Fact]
    public void Setters_RaiseChangedOncePerRealChange()
    {
        var preferences = new SnapPreferences(new MemorySettingsStore());
        var raised = 0;
        preferences.Changed += (_, _) => raised++;

        preferences.SmartGuides = true;
        preferences.SmartGuides = true;
        preferences.ShowGuides = false;
        preferences.ShowGuides = false;

        Assert.Equal(2, raised);
    }

    [Fact]
    public void Switches_DoNotChangeTheSnapSettings()
    {
        var preferences = new SnapPreferences(new MemorySettingsStore(), persist: false);

        preferences.SmartGuides = true;
        preferences.ShowGuides = false;

        Assert.Equal(new SnapSettings(false, false, 10), SnapSettings.From(preferences));
    }
}
