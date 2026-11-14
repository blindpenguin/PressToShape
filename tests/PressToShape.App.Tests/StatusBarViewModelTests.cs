using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class StatusBarViewModelTests
{
    private static (EditorState State, StatusBarViewModel Bar, List<int> FitCalls) Create()
    {
        var state = new EditorState();
        var fitCalls = new List<int>();
        var bar = new StatusBarViewModel(state, FakeLocalizer.Create(), () => fitCalls.Add(1));
        return (state, bar, fitCalls);
    }

    [Fact]
    public void Defaults_ShowNothingSelected_And100Percent()
    {
        var (_, bar, _) = Create();

        Assert.Equal("Nothing selected", bar.SelectionSummary);
        Assert.False(bar.ShowPathLength);
        Assert.Equal("100%", bar.ZoomText);
    }

    [Theory]
    [InlineData(100, "100%")]
    [InlineData(10, "10%")]
    [InlineData(6400, "6400%")]
    [InlineData(133.333, "133.33%")]
    [InlineData(12.5, "12.5%")]
    public void FormatZoom_UsesInvariantCulture(double percent, string expected)
    {
        Assert.Equal(expected, StatusBarViewModel.FormatZoom(percent, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ZoomCommands_ChangeTheZoomText()
    {
        var (_, bar, _) = Create();

        bar.ZoomInCommand.Execute(null);
        Assert.Equal("150%", bar.ZoomText);

        bar.ZoomOutCommand.Execute(null);
        bar.ZoomOutCommand.Execute(null);
        Assert.Equal("75%", bar.ZoomText);
    }

    [Fact]
    public void SetZoomCommand_AppliesAPreset()
    {
        var (state, bar, _) = Create();

        bar.SetZoomCommand.Execute(400.0);

        Assert.Equal(400, state.ZoomPercent);
        Assert.Equal("400%", bar.ZoomText);
    }

    [Fact]
    public void ZoomChangedElsewhere_UpdatesTheText()
    {
        var (state, bar, _) = Create();
        var raised = false;
        bar.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(StatusBarViewModel.ZoomText);

        state.SetZoom(25);

        Assert.True(raised);
        Assert.Equal("25%", bar.ZoomText);
    }

    [Fact]
    public void FitCommand_InvokesCallback()
    {
        var (_, bar, fitCalls) = Create();

        bar.FitToWindowCommand.Execute(null);

        Assert.Single(fitCalls);
    }

    [Fact]
    public void HandButton_ActivatesHandTool()
    {
        var (state, bar, _) = Create();

        bar.ActivateHandToolCommand.Execute(null);

        Assert.Equal(Tool.Hand, state.ActiveTool);
    }

    [Fact]
    public void Presets_AreTheZoomLevels()
    {
        var (_, bar, _) = Create();

        Assert.Equal(ZoomLevels.Presets, bar.ZoomPresets.Select(p => p.Percent));
        Assert.Equal("100%", bar.ZoomPresets.Single(p => p.Percent == 100).Text);
    }

    [Fact]
    public void PresetCommand_SetsTheZoom()
    {
        var (state, bar, _) = Create();

        bar.ZoomPresets.Single(p => p.Percent == 25).SelectCommand.Execute(null);

        Assert.Equal(25, state.ZoomPercent);
        Assert.Equal("25%", bar.ZoomText);
    }

    [Fact]
    public void SelectionSummaryChange_RaisesNotification()
    {
        var (state, bar, _) = Create();
        var raised = false;
        bar.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(StatusBarViewModel.SelectionSummary);

        state.HasSelection = true;
        state.SelectionSummary = "1 selected (Group)";

        Assert.True(raised);
        Assert.Equal("1 selected (Group)", bar.SelectionSummary);
    }

    private static (EditorState State, Localizer Localizer, StatusBarViewModel Bar) CreateWithGerman()
    {
        var state = new EditorState();
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Nothing selected"] = "Nichts ausgewählt" });
        var bar = new StatusBarViewModel(state, localizer, () => { });
        return (state, localizer, bar);
    }

    [Fact]
    public void NothingSelected_IsTranslated_AndRefreshesOnLanguageChange()
    {
        var (_, localizer, bar) = CreateWithGerman();
        var raised = new List<string?>();
        bar.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        Assert.Equal("Nothing selected", bar.SelectionSummary);

        localizer.SetLanguage("de");

        Assert.Equal("Nichts ausgewählt", bar.SelectionSummary);
        Assert.Contains(string.Empty, raised);
    }

    [Fact]
    public void WithASelection_TheSummaryIsTheStateText()
    {
        var (state, localizer, bar) = CreateWithGerman();
        localizer.SetLanguage("de");

        state.SelectionSummary = "1 ausgewählt (Gruppe)";
        state.HasSelection = true;

        Assert.Equal("1 ausgewählt (Gruppe)", bar.SelectionSummary);

        state.HasSelection = false;

        Assert.Equal("Nichts ausgewählt", bar.SelectionSummary);
    }

    [Fact]
    public void ZoomText_UsesTheDecimalSeparatorOfTheLanguage()
    {
        var (state, localizer, bar) = CreateWithGerman();
        state.SetZoom(80.75);
        Assert.Equal("80.75%", bar.ZoomText);

        localizer.SetLanguage("de");

        Assert.Equal("80,75%", bar.ZoomText);

        localizer.SetLanguage("en");

        Assert.Equal("80.75%", bar.ZoomText);
    }

    [Fact]
    public void WholeNumberZoom_LooksTheSameInBothLanguages()
    {
        var (_, localizer, bar) = CreateWithGerman();

        localizer.SetLanguage("de");

        Assert.Equal("100%", bar.ZoomText);
        Assert.Equal("100%", bar.ZoomPresets.Single(p => p.Percent == 100).Text);
    }

    [Fact]
    public void FormatZoom_GermanCulture_UsesAComma()
    {
        Assert.Equal("133,33%", StatusBarViewModel.FormatZoom(133.333, new System.Globalization.CultureInfo("de-DE")));
    }
}
