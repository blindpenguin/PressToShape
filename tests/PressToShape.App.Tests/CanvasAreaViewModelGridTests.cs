using System.Collections.Generic;
using System.Collections.Immutable;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasAreaViewModelGridTests
{
    private static (EditorState State, CanvasAreaViewModel Canvas) Create()
    {
        var state = new EditorState();
        var canvas = new CanvasAreaViewModel(state) { ViewportWidth = 2000, ViewportHeight = 1200 };
        return (state, canvas);
    }

    private static List<string?> Watch(CanvasAreaViewModel canvas)
    {
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        return raised;
    }

    [Fact]
    public void Properties_Default_AreGridOffSpacingTenRulersOn()
    {
        var (_, canvas) = Create();

        Assert.False(canvas.ShowGrid);
        Assert.Equal(10, canvas.GridSpacing);
        Assert.True(canvas.ShowRulers);
        Assert.Empty(canvas.SnapGuidesOnScreen);
    }

    [Fact]
    public void Properties_FollowThePreferences_AndRaiseTheChange()
    {
        var (state, canvas) = Create();
        var raised = Watch(canvas);

        state.Snapping.ShowGrid = true;
        state.Snapping.GridSpacing = 25;
        state.Snapping.ShowRulers = false;

        Assert.True(canvas.ShowGrid);
        Assert.Equal(25, canvas.GridSpacing);
        Assert.False(canvas.ShowRulers);
        Assert.Contains(nameof(CanvasAreaViewModel.ShowGrid), raised);
        Assert.Contains(nameof(CanvasAreaViewModel.GridSpacing), raised);
        Assert.Contains(nameof(CanvasAreaViewModel.ShowRulers), raised);
    }

    [Fact]
    public void SnapGuidesOnScreen_ConvertsToScreenPixels_AtFullZoom()
    {
        var (state, canvas) = Create();

        state.SnapGuides = [new SnapGuide(SnapAxis.X, 100, 20, 80), new SnapGuide(SnapAxis.Y, 50, 10, 60)];

        var guides = canvas.SnapGuidesOnScreen;
        Assert.Equal(new ScreenSnapGuide(true, canvas.OriginX + 100, canvas.OriginY + 20, canvas.OriginY + 80), guides[0]);
        Assert.Equal(new ScreenSnapGuide(false, canvas.OriginY + 50, canvas.OriginX + 10, canvas.OriginX + 60), guides[1]);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(200)]
    public void SnapGuidesOnScreen_FollowTheZoom(double zoom)
    {
        var (state, canvas) = Create();
        state.SnapGuides = [new SnapGuide(SnapAxis.X, 100, 20, 80)];

        state.SetZoom(zoom);

        var guide = canvas.SnapGuidesOnScreen[0];
        var scale = zoom / 100;
        Assert.Equal(canvas.OriginX + 100 * scale, guide.Position, 9);
        Assert.Equal(canvas.OriginY + 20 * scale, guide.From, 9);
        Assert.Equal(canvas.OriginY + 80 * scale, guide.To, 9);
    }

    [Fact]
    public void SnapGuidesOnScreen_FollowThePan()
    {
        var (state, canvas) = Create();
        state.SnapGuides = [new SnapGuide(SnapAxis.X, 100, 0, 10)];
        var before = canvas.SnapGuidesOnScreen[0].Position;
        var raised = Watch(canvas);

        state.SetPan(state.PanX + 30, state.PanY);

        Assert.Equal(before + 30, canvas.SnapGuidesOnScreen[0].Position, 9);
        Assert.Contains(nameof(CanvasAreaViewModel.SnapGuidesOnScreen), raised);
    }

    [Fact]
    public void SnapGuidesOnScreen_ChangeRaisesTheProperty_AndClearingEmptiesIt()
    {
        var (state, canvas) = Create();
        var raised = Watch(canvas);

        state.SnapGuides = [new SnapGuide(SnapAxis.X, 100, 0, 10)];
        state.SnapGuides = ImmutableArray<SnapGuide>.Empty;

        Assert.Equal(2, raised.FindAll(name => name == nameof(CanvasAreaViewModel.SnapGuidesOnScreen)).Count);
        Assert.Empty(canvas.SnapGuidesOnScreen);
    }
}
