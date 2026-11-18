using System.Collections.Generic;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class StatusBarViewModelSnapTests
{
    private static (EditorState State, StatusBarViewModel Bar) Create(string language = "en", Dictionary<string, string>? translations = null)
    {
        var state = new EditorState();
        return (state, new StatusBarViewModel(state, FakeLocalizer.Create(language, translations), () => { }));
    }

    [Fact]
    public void Toggles_StartWithTheStateOfThePreferences()
    {
        var (state, bar) = Create();

        Assert.False(bar.ShowGrid);
        Assert.False(bar.SnapToGrid);
        Assert.False(bar.SnapToObjects);
        Assert.Equal("10", bar.GridSpacingField.Text);
        Assert.False(state.Snapping.ShowGrid);
    }

    [Fact]
    public void Toggles_Set_ChangeThePreferences()
    {
        var (state, bar) = Create();

        bar.ShowGrid = true;
        bar.SnapToGrid = true;
        bar.SnapToObjects = true;

        Assert.True(state.Snapping.ShowGrid);
        Assert.True(state.Snapping.SnapToGrid);
        Assert.True(state.Snapping.SnapToObjects);
    }

    [Fact]
    public void Toggles_ChangedElsewhere_RaisePropertyChanged()
    {
        var (state, bar) = Create();
        var raised = new List<string?>();
        bar.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.Snapping.ShowGrid = true;
        state.Snapping.SnapToGrid = true;
        state.Snapping.SnapToObjects = true;

        Assert.True(bar.ShowGrid);
        Assert.True(bar.SnapToGrid);
        Assert.True(bar.SnapToObjects);
        Assert.Contains(nameof(StatusBarViewModel.ShowGrid), raised);
        Assert.Contains(nameof(StatusBarViewModel.SnapToGrid), raised);
        Assert.Contains(nameof(StatusBarViewModel.SnapToObjects), raised);
    }

    [Fact]
    public void Spacing_ValidText_IsApplied()
    {
        var (state, bar) = Create();

        bar.GridSpacingField.Text = "25";
        bar.GridSpacingField.Commit();

        Assert.Equal(25, state.Snapping.GridSpacing);
        Assert.Equal("25", bar.GridSpacingField.Text);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1000")]
    [InlineData("2.5")]
    public void Spacing_AtTheLimitsAndDecimals_IsApplied(string text)
    {
        var (state, bar) = Create();

        bar.GridSpacingField.Text = text;
        bar.GridSpacingField.Commit();

        Assert.Equal(double.Parse(text, System.Globalization.CultureInfo.InvariantCulture), state.Snapping.GridSpacing);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("0.5")]
    [InlineData("-5")]
    [InlineData("1000.5")]
    [InlineData("5000")]
    public void Spacing_InvalidText_IsRefusedAndTheOldValueComesBack(string text)
    {
        var (state, bar) = Create();

        bar.GridSpacingField.Text = text;
        bar.GridSpacingField.Commit();

        Assert.Equal(10, state.Snapping.GridSpacing);
        Assert.Equal("10", bar.GridSpacingField.Text);
    }

    [Fact]
    public void Spacing_ChangedElsewhere_UpdatesTheField()
    {
        var (state, bar) = Create();

        state.Snapping.GridSpacing = 40;

        Assert.Equal("40", bar.GridSpacingField.Text);
    }

    [Fact]
    public void Spacing_InGerman_ShowsAndAcceptsTheDecimalComma()
    {
        var (state, bar) = Create("de");

        bar.GridSpacingField.Text = "2,5";
        bar.GridSpacingField.Commit();

        Assert.Equal(2.5, state.Snapping.GridSpacing);
        Assert.Equal("2,5", bar.GridSpacingField.Text);
    }
}
