using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SettingsViewModelNavigationTests
{
    private static SettingsViewModel Create(bool defaultShowRulers = true, string systemCulture = "en", MemorySettingsStore? store = null)
    {
        var snapping = new SnapPreferences(store ?? new MemorySettingsStore(), defaultShowRulers);
        return new SettingsViewModel(snapping, FakeLocalizer.Create(), CultureInfo.GetCultureInfo(systemCulture), defaultShowRulers);
    }

    private static SettingsCategoryViewModel Category(SettingsViewModel viewModel, SettingsCategoryType type) =>
        viewModel.Categories.Single(c => c.Type == type);

    [Fact]
    public void Constructor_Categories_AreGeneralCanvasSnappingWithGeneralSelected()
    {
        var viewModel = Create();

        Assert.Equal(["General", "Canvas", "Snapping"], viewModel.Categories.Select(c => c.Title));
        Assert.Same(viewModel.Categories[0], viewModel.SelectedCategory);
        Assert.False(viewModel.IsPageOpen);
    }

    [Fact]
    public void Constructor_Pages_AreTheMatchingPageViewModelsOfTheSameSettings()
    {
        var viewModel = Create();

        Assert.Equal(
            [typeof(GeneralSettingsPageViewModel), typeof(CanvasSettingsPageViewModel), typeof(SnappingSettingsPageViewModel)],
            viewModel.Categories.Select(c => c.Page.GetType()));
        Assert.Same(viewModel, ((CanvasSettingsPageViewModel)Category(viewModel, SettingsCategoryType.Canvas).Page).Settings);
    }

    [Fact]
    public void SelectedCategory_Changed_KeepsTheEnteredValues()
    {
        var viewModel = Create();
        viewModel.ShowGrid = true;
        viewModel.GridSpacingText = "33";

        viewModel.SelectedCategory = Category(viewModel, SettingsCategoryType.Snapping);
        viewModel.SelectedCategory = Category(viewModel, SettingsCategoryType.General);

        Assert.True(viewModel.ShowGrid);
        Assert.Equal("33", viewModel.GridSpacingText);
    }

    [Fact]
    public void RestoreDefaults_Canvas_ResetsOnlyTheCanvasValues()
    {
        var viewModel = Create(defaultShowRulers: true);
        viewModel.ShowRulers = false;
        viewModel.ShowGrid = true;
        viewModel.GridSpacingText = "50";
        viewModel.SnapToGrid = true;
        viewModel.SelectedLanguage = viewModel.Languages.Single(l => l.Code == "de");
        viewModel.SelectedCategory = Category(viewModel, SettingsCategoryType.Canvas);

        viewModel.RestoreDefaultsCommand.Execute(null);

        Assert.True(viewModel.ShowRulers);
        Assert.False(viewModel.ShowGrid);
        Assert.Equal("10", viewModel.GridSpacingText);
        Assert.True(viewModel.SnapToGrid);
        Assert.Equal("de", viewModel.SelectedLanguage.Code);
    }

    [Fact]
    public void RestoreDefaults_Canvas_UsesTheRulerDefaultOfTheLayout()
    {
        var viewModel = Create(defaultShowRulers: false);
        viewModel.ShowRulers = true;
        viewModel.SelectedCategory = Category(viewModel, SettingsCategoryType.Canvas);

        viewModel.RestoreDefaultsCommand.Execute(null);

        Assert.False(viewModel.ShowRulers);
    }

    [Fact]
    public void RestoreDefaults_Snapping_ResetsOnlyTheSnappingValues()
    {
        var viewModel = Create();
        viewModel.SnapToGrid = true;
        viewModel.SnapToObjects = true;
        viewModel.SmartGuides = true;
        viewModel.ShowGuides = false;
        viewModel.ShowGrid = true;
        viewModel.SelectedCategory = Category(viewModel, SettingsCategoryType.Snapping);

        viewModel.RestoreDefaultsCommand.Execute(null);

        Assert.False(viewModel.SnapToGrid);
        Assert.False(viewModel.SnapToObjects);
        Assert.False(viewModel.SmartGuides);
        Assert.True(viewModel.ShowGuides);
        Assert.True(viewModel.ShowGrid);
    }

    [Theory]
    [InlineData("de", "de")]
    [InlineData("fr", "en")]
    public void RestoreDefaults_General_ChoosesTheLanguageOfTheSystem(string systemCulture, string expected)
    {
        var viewModel = Create(systemCulture: systemCulture);
        viewModel.SelectedLanguage = viewModel.Languages.Single(l => l.Code != expected);

        viewModel.RestoreDefaultsCommand.Execute(null);

        Assert.Equal(expected, viewModel.SelectedLanguage.Code);
    }

    [Fact]
    public void RestoreDefaults_AnyCategory_AppliesNothing()
    {
        var store = new MemorySettingsStore();
        var viewModel = Create(store: store);
        var saves = store.SaveCount;
        viewModel.ShowGrid = true;
        viewModel.SelectedCategory = Category(viewModel, SettingsCategoryType.Canvas);

        viewModel.RestoreDefaultsCommand.Execute(null);

        Assert.Equal(saves, store.SaveCount);
    }

    [Fact]
    public void RestoreDefaults_Canvas_ClearsAnInvalidSpacing()
    {
        var viewModel = Create();
        viewModel.GridSpacingText = "abc";
        viewModel.SelectedCategory = Category(viewModel, SettingsCategoryType.Canvas);

        viewModel.RestoreDefaultsCommand.Execute(null);

        Assert.True(viewModel.CanApply);
        Assert.False(Category(viewModel, SettingsCategoryType.Canvas).HasError);
    }

    [Fact]
    public void OkCommand_ValidValues_RaisesCloseRequestedWithTrue()
    {
        var viewModel = Create();
        var results = new List<bool>();
        viewModel.CloseRequested += (_, ok) => results.Add(ok);

        viewModel.OkCommand.Execute(null);

        Assert.Equal([true], results);
    }

    [Fact]
    public void OkCommand_InvalidSpacing_CannotRunAndRaisesNothing()
    {
        var viewModel = Create();
        var results = new List<bool>();
        viewModel.CloseRequested += (_, ok) => results.Add(ok);

        viewModel.GridSpacingText = "0";

        Assert.False(viewModel.OkCommand.CanExecute(null));
        viewModel.OkCommand.Execute(null);
        Assert.Empty(results);

        viewModel.GridSpacingText = "5";
        Assert.True(viewModel.OkCommand.CanExecute(null));
    }

    [Fact]
    public void CancelCommand_Always_RaisesCloseRequestedWithFalse()
    {
        var viewModel = Create();
        viewModel.GridSpacingText = "abc";
        var results = new List<bool>();
        viewModel.CloseRequested += (_, ok) => results.Add(ok);

        viewModel.CancelCommand.Execute(null);

        Assert.Equal([false], results);
    }

    [Fact]
    public void GridSpacingText_Invalid_MarksTheCanvasCategory()
    {
        var viewModel = Create();

        viewModel.GridSpacingText = "abc";
        Assert.True(Category(viewModel, SettingsCategoryType.Canvas).HasError);
        Assert.False(Category(viewModel, SettingsCategoryType.General).HasError);

        viewModel.GridSpacingText = "20";
        Assert.False(Category(viewModel, SettingsCategoryType.Canvas).HasError);
    }

    [Fact]
    public void OpenCategoryCommand_Category_ShowsItsPage()
    {
        var viewModel = Create();
        var snapping = Category(viewModel, SettingsCategoryType.Snapping);

        viewModel.OpenCategoryCommand.Execute(snapping);

        Assert.True(viewModel.IsPageOpen);
        Assert.Same(snapping, viewModel.SelectedCategory);
    }

    [Fact]
    public void BackCommand_OnAPage_ShowsTheListAndKeepsTheValues()
    {
        var viewModel = Create();
        var results = new List<bool>();
        viewModel.CloseRequested += (_, ok) => results.Add(ok);
        viewModel.OpenCategoryCommand.Execute(Category(viewModel, SettingsCategoryType.Canvas));
        viewModel.ShowGrid = true;

        viewModel.BackCommand.Execute(null);

        Assert.False(viewModel.IsPageOpen);
        Assert.True(viewModel.ShowGrid);
        Assert.Empty(results);
    }

    [Fact]
    public void BackCommand_OnTheList_Cancels()
    {
        var viewModel = Create();
        var results = new List<bool>();
        viewModel.CloseRequested += (_, ok) => results.Add(ok);

        viewModel.BackCommand.Execute(null);

        Assert.Equal([false], results);
    }
}
