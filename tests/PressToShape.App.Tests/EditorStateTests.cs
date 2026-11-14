using System.Collections.Generic;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class EditorStateTests
{
    [Fact]
    public void NewState_HasExpectedDefaults()
    {
        var state = new EditorState();

        Assert.Equal(Tool.Select, state.ActiveTool);
        Assert.Equal(100, state.ZoomPercent);
        Assert.False(state.HasSelection);
        Assert.Equal(string.Empty, state.SelectionSummary);
        Assert.Equal(1200, state.ArtboardWidth);
        Assert.Equal(800, state.ArtboardHeight);
    }

    [Fact]
    public void SetZoom_OutsideRange_IsClamped()
    {
        var state = new EditorState();

        state.SetZoom(1);
        Assert.Equal(ZoomLevels.Min, state.ZoomPercent);

        state.SetZoom(99999);
        Assert.Equal(ZoomLevels.Max, state.ZoomPercent);
    }

    [Fact]
    public void ZoomIn_AndZoomOut_StepThroughPresets()
    {
        var state = new EditorState();

        state.ZoomIn();
        Assert.Equal(150, state.ZoomPercent);

        state.ZoomOut();
        state.ZoomOut();
        Assert.Equal(75, state.ZoomPercent);
    }

    [Fact]
    public void ZoomIn_AtMaximum_StaysAtMaximum()
    {
        var state = new EditorState();
        state.SetZoom(ZoomLevels.Max);

        state.ZoomIn();

        Assert.Equal(ZoomLevels.Max, state.ZoomPercent);
    }

    [Fact]
    public void ZoomToActualSize_Sets100()
    {
        var state = new EditorState();
        state.SetZoom(400);

        state.ZoomToActualSize();

        Assert.Equal(100, state.ZoomPercent);
    }

    [Fact]
    public void ZoomToFit_UsesArtboardAndViewport()
    {
        var state = new EditorState();

        state.ZoomToFit(1248, 848); // available 1200 x 800 -> exactly the artboard size

        Assert.Equal(100, state.ZoomPercent, precision: 6);
    }

    [Fact]
    public void ZoomToFit_WithoutViewportSize_DoesNothing()
    {
        var state = new EditorState();
        state.SetZoom(200);

        state.ZoomToFit(0, 0);

        Assert.Equal(200, state.ZoomPercent);
    }

    [Fact]
    public void Changes_RaisePropertyChanged()
    {
        var state = new EditorState();
        var changed = new List<string?>();
        state.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.ActiveTool = Tool.Pen;
        state.SetZoom(200);
        state.HasSelection = true;
        state.SelectionSummary = "1 selected";

        Assert.Contains(nameof(EditorState.ActiveTool), changed);
        Assert.Contains(nameof(EditorState.ZoomPercent), changed);
        Assert.Contains(nameof(EditorState.HasSelection), changed);
        Assert.Contains(nameof(EditorState.SelectionSummary), changed);
    }

    [Fact]
    public void SetZoom_ToSameValue_DoesNotRaisePropertyChanged()
    {
        var state = new EditorState();
        var count = 0;
        state.PropertyChanged += (_, _) => count++;

        state.SetZoom(100);

        Assert.Equal(0, count);
    }
}
