using System;
using System.Globalization;
using PressToShape.App.Localization;
using PressToShape.App.Settings;
using Xunit;

namespace PressToShape.App.Tests;

public class LanguagePreferenceTests
{
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

    [Fact]
    public void ChooseStartLanguage_UsesTheSavedLanguage()
    {
        var store = new MemoryStore { Current = new AppSettings { Language = "de" } };

        Assert.Equal("de", LanguagePreference.ChooseStartLanguage(store, new CultureInfo("en-US")));
    }

    [Fact]
    public void ChooseStartLanguage_WithoutSavedLanguage_UsesTheSystemLanguage()
    {
        var store = new MemoryStore();

        Assert.Equal("de", LanguagePreference.ChooseStartLanguage(store, new CultureInfo("de-DE")));
        Assert.Equal("en", LanguagePreference.ChooseStartLanguage(store, new CultureInfo("fr-FR")));
    }

    [Fact]
    public void ChooseStartLanguage_IgnoresAnUnsupportedSavedLanguage()
    {
        var store = new MemoryStore { Current = new AppSettings { Language = "klingon" } };

        Assert.Equal("de", LanguagePreference.ChooseStartLanguage(store, new CultureInfo("de-DE")));
    }

    [Fact]
    public void Attach_SavesTheLanguageWhenItChanges()
    {
        var store = new MemoryStore();
        var localizer = FakeLocalizer.Create("en");
        var preference = LanguagePreference.Attach(store, localizer);

        localizer.SetLanguage("de");

        Assert.Equal("de", store.Current.Language);
        Assert.Equal(1, store.SaveCount);
        GC.KeepAlive(preference);
    }

    [Fact]
    public void Attach_DoesNotSaveWhenNothingChanges()
    {
        var store = new MemoryStore();
        var localizer = FakeLocalizer.Create("en");
        var preference = LanguagePreference.Attach(store, localizer);

        localizer.SetLanguage("en");
        localizer.SetLanguage("xx");

        Assert.Equal(0, store.SaveCount);
        GC.KeepAlive(preference);
    }

    [Fact]
    public void Attach_KeepsUnknownSettings()
    {
        var store = new MemoryStore();
        store.Current.ExtraData = new() { ["Future"] = System.Text.Json.JsonSerializer.SerializeToElement(123) };
        var localizer = FakeLocalizer.Create("en");
        var preference = LanguagePreference.Attach(store, localizer);

        localizer.SetLanguage("de");

        Assert.NotNull(store.Current.ExtraData);
        Assert.True(store.Current.ExtraData!.ContainsKey("Future"));
        GC.KeepAlive(preference);
    }

    [Fact]
    public void LocalizationHost_DefaultsToAnEnglishLocalizer_AndCanBeReplaced()
    {
        var original = LocalizationHost.Localizer;
        try
        {
            Assert.Equal("en", LocalizationHost.Localizer.LanguageCode);
            Assert.Equal("Select", LocalizationHost.Localizer.T("Select"));

            var german = FakeLocalizer.Create("de", new() { ["Select"] = "Auswählen" });
            LocalizationHost.Initialize(german);

            Assert.Same(german, LocalizationHost.Localizer);
        }
        finally
        {
            LocalizationHost.Initialize(original);
        }
    }
}
