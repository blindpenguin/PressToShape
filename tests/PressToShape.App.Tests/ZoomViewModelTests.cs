using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class ZoomViewModelTests
{
    private static (EditorState State, Localizer Localizer, ZoomViewModel Zoom, List<int> FitCalls) Create()
    {
        var state = new EditorState();
        var localizer = FakeLocalizer.Create();
        var fitCalls = new List<int>();
        return (state, localizer, new ZoomViewModel(state, localizer, () => fitCalls.Add(1)), fitCalls);
    }

    [Fact]
    public void Defaults_Show100Percent()
    {
        var (_, _, zoom, _) = Create();

        Assert.Equal(100, zoom.ZoomPercent);
        Assert.Equal("100%", zoom.ZoomText);
    }

    [Fact]
    public void ZoomInAndOutCommands_StepThroughThePresets()
    {
        var (_, _, zoom, _) = Create();

        zoom.ZoomInCommand.Execute(null);
        Assert.Equal("150%", zoom.ZoomText);

        zoom.ZoomOutCommand.Execute(null);
        zoom.ZoomOutCommand.Execute(null);
        Assert.Equal("75%", zoom.ZoomText);
    }

    [Fact]
    public void SetZoomCommand_AppliesAValue()
    {
        var (state, _, zoom, _) = Create();

        zoom.SetZoomCommand.Execute(400.0);

        Assert.Equal(400, state.ZoomPercent);
        Assert.Equal("400%", zoom.ZoomText);
    }

    [Fact]
    public void FitCommand_InvokesTheCallbackOnce()
    {
        var (_, _, zoom, fitCalls) = Create();

        zoom.FitToWindowCommand.Execute(null);

        Assert.Single(fitCalls);
    }

    [Fact]
    public void Presets_AreTheZoomLevels_AndTheirCommandsSetTheZoom()
    {
        var (state, _, zoom, _) = Create();

        Assert.Equal(ZoomLevels.Presets, zoom.ZoomPresets.Select(p => p.Percent));

        zoom.ZoomPresets.Single(p => p.Percent == 25).SelectCommand.Execute(null);

        Assert.Equal(25, state.ZoomPercent);
    }

    [Fact]
    public void ZoomChangedElsewhere_RaisesNotificationsForTheValueAndTheText()
    {
        var (state, _, zoom, _) = Create();
        var raised = new List<string?>();
        zoom.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.SetZoom(200);

        Assert.Contains(nameof(ZoomViewModel.ZoomPercent), raised);
        Assert.Contains(nameof(ZoomViewModel.ZoomText), raised);
    }

    [Fact]
    public void ZoomText_FollowsTheDecimalSeparatorOfTheLanguage()
    {
        var (state, localizer, zoom, _) = Create();
        state.SetZoom(80.75);
        Assert.Equal("80.75%", zoom.ZoomText);

        localizer.SetLanguage("de");

        Assert.Equal("80,75%", zoom.ZoomText);
    }

    [Fact]
    public void WholeNumberPresets_LookTheSameInEveryLanguage()
    {
        var (_, localizer, zoom, _) = Create();

        localizer.SetLanguage("de");

        Assert.Equal("100%", zoom.ZoomPresets.Single(p => p.Percent == 100).Text);
    }

    [Theory]
    [InlineData(100, "100%")]
    [InlineData(133.333, "133.33%")]
    [InlineData(12.5, "12.5%")]
    public void FormatZoom_UsesTheGivenCulture(double percent, string expected)
    {
        Assert.Equal(expected, ZoomViewModel.FormatZoom(percent, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(expected.Replace('.', ','), ZoomViewModel.FormatZoom(percent, new System.Globalization.CultureInfo("de-DE")));
    }
}
