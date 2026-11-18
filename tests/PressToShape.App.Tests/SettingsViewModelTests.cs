using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using PressToShape.App.Localization;
using PressToShape.App.Settings;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SettingsViewModelTests
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

    private sealed record Fixture(SettingsViewModel ViewModel, SnapPreferences Snapping, MemorySettingsStore Store, Localizer Localizer);

    private static Fixture Create(string language = "en", MemorySettingsStore? store = null)
    {
        store ??= new MemorySettingsStore();
        var localizer = FakeLocalizer.Create(language);
        var snapping = new SnapPreferences(store);
        return new Fixture(new SettingsViewModel(snapping, localizer, English, defaultShowRulers: true), snapping, store, localizer);
    }

    [Fact]
    public void Constructor_CurrentValues_AreCopied()
    {
        var store = new MemorySettingsStore();
        var seed = new SnapPreferences(store) { ShowGrid = true, SnapToObjects = true, GridSpacing = 25, ShowRulers = false };
        seed.ShowGuides = false;

        var fixture = Create("de", store);
        var viewModel = fixture.ViewModel;

        Assert.Equal("de", viewModel.SelectedLanguage.Code);
        Assert.True(viewModel.ShowGrid);
        Assert.True(viewModel.SnapToObjects);
        Assert.False(viewModel.ShowRulers);
        Assert.False(viewModel.ShowGuides);
        Assert.False(viewModel.SnapToGrid);
        Assert.False(viewModel.SmartGuides);
        Assert.Equal("25", viewModel.GridSpacingText);
        Assert.False(viewModel.HasChanges);
        Assert.True(viewModel.CanApply);
    }

    [Fact]
    public void Constructor_GermanLanguage_ShowsSpacingWithTheLocalizerCulture()
    {
        var store = new MemorySettingsStore();
        _ = new SnapPreferences(store) { GridSpacing = 12.5 };

        var fixture = Create("de", store);

        Assert.Equal("12,5", fixture.ViewModel.GridSpacingText);
    }

    [Fact]
    public void Setters_ChangedValues_WriteNothingBeforeApply()
    {
        var fixture = Create();
        var saves = fixture.Store.SaveCount;

        fixture.ViewModel.ShowGrid = true;
        fixture.ViewModel.SnapToGrid = true;
        fixture.ViewModel.GridSpacingText = "40";
        fixture.ViewModel.SelectedLanguage = fixture.ViewModel.Languages[1];

        Assert.False(fixture.Snapping.ShowGrid);
        Assert.False(fixture.Snapping.SnapToGrid);
        Assert.Equal(SnapPreferences.DefaultSpacing, fixture.Snapping.GridSpacing);
        Assert.Equal("en", fixture.Localizer.LanguageCode);
        Assert.Equal(saves, fixture.Store.SaveCount);
        Assert.True(fixture.ViewModel.HasChanges);
    }

    [Fact]
    public void Apply_ChangedValues_AreWrittenAndSavedOncePerValue()
    {
        var fixture = Create();
        var saves = fixture.Store.SaveCount;
        fixture.ViewModel.ShowGrid = true;
        fixture.ViewModel.SmartGuides = true;
        fixture.ViewModel.GridSpacingText = "40";

        fixture.ViewModel.Apply();

        Assert.True(fixture.Snapping.ShowGrid);
        Assert.True(fixture.Snapping.SmartGuides);
        Assert.Equal(40, fixture.Snapping.GridSpacing);
        Assert.Equal(saves + 3, fixture.Store.SaveCount);
        Assert.False(fixture.ViewModel.HasChanges);
    }

    [Fact]
    public void Apply_NothingChanged_DoesNotSave()
    {
        var fixture = Create();
        var saves = fixture.Store.SaveCount;

        fixture.ViewModel.Apply();

        Assert.Equal(saves, fixture.Store.SaveCount);
        Assert.False(fixture.ViewModel.HasChanges);
    }

    [Fact]
    public void Apply_OtherLanguage_SwitchesTheLanguage()
    {
        var fixture = Create();
        fixture.ViewModel.SelectedLanguage = fixture.ViewModel.Languages.Single(l => l.Code == "de");

        fixture.ViewModel.Apply();

        Assert.Equal("de", fixture.Localizer.LanguageCode);
    }

    [Fact]
    public void Apply_UnknownSettings_AreKept()
    {
        var store = new MemorySettingsStore();
        store.Save(new AppSettings { ExtraData = new Dictionary<string, JsonElement> { ["future"] = JsonDocument.Parse("42").RootElement.Clone() } });
        var fixture = Create(store: store);
        fixture.ViewModel.ShowGrid = true;

        fixture.ViewModel.Apply();

        Assert.True(store.Load().ExtraData!.ContainsKey("future"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("0.5")]
    [InlineData("1001")]
    [InlineData("NaN")]
    public void GridSpacingText_Invalid_SetsTheErrorAndDisablesApply(string text)
    {
        var fixture = Create();

        fixture.ViewModel.GridSpacingText = text;

        Assert.False(fixture.ViewModel.CanApply);
        Assert.True(fixture.ViewModel.HasGridSpacingError);
        Assert.Equal("Enter a number from 1 to 1000.", fixture.ViewModel.GridSpacingError);
    }

    [Theory]
    [InlineData("en", "12.5")]
    [InlineData("de", "12,5")]
    [InlineData("de", "12.5")]
    [InlineData("en", "1")]
    [InlineData("en", "1000")]
    public void GridSpacingText_Valid_ClearsTheError(string language, string text)
    {
        var fixture = Create(language);
        fixture.ViewModel.GridSpacingText = "abc";

        fixture.ViewModel.GridSpacingText = text;

        Assert.Null(fixture.ViewModel.GridSpacingError);
        Assert.True(fixture.ViewModel.CanApply);
    }

    [Fact]
    public void Apply_InvalidSpacing_KeepsTheOldSpacingAndStillAppliesTheRest()
    {
        var fixture = Create();
        fixture.ViewModel.GridSpacingText = "abc";
        fixture.ViewModel.ShowGrid = true;

        fixture.ViewModel.Apply();

        Assert.Equal(SnapPreferences.DefaultSpacing, fixture.Snapping.GridSpacing);
        Assert.True(fixture.Snapping.ShowGrid);
    }

    [Fact]
    public void GridSpacingText_Changed_RaisesPropertyChangedForTheErrorAndCanApply()
    {
        var fixture = Create();
        var raised = new List<string?>();
        fixture.ViewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        fixture.ViewModel.GridSpacingText = "abc";

        Assert.Contains("GridSpacingError", raised);
        Assert.Contains("HasGridSpacingError", raised);
        Assert.Contains("CanApply", raised);
        Assert.Contains("HasChanges", raised);
    }
}
