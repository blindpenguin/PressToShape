using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasAreaViewModelGuideTests
{
    private static (EditorState State, CanvasAreaViewModel Canvas) Create(string language = "en")
    {
        var state = new EditorState();
        var canvas = new CanvasAreaViewModel(state, localizer: FakeLocalizer.Create(language)) { ViewportWidth = 2000, ViewportHeight = 1200 };
        return (state, canvas);
    }

    private static List<string?> Watch(CanvasAreaViewModel canvas)
    {
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        return raised;
    }

    [Fact]
    public void Defaults_SmartGuidesOffShowGuidesOn_NothingDrawn()
    {
        var (_, canvas) = Create();

        Assert.False(canvas.SmartGuides);
        Assert.True(canvas.ShowGuides);
        Assert.Empty(canvas.ScreenGuides);
        Assert.Empty(canvas.ScreenMarkers);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(50)]
    [InlineData(200)]
    public void ScreenGuides_ConvertWithZoomAndOrigin_AndCarrySelectedAndActive(double zoom)
    {
        var (state, canvas) = Create();
        var vertical = new Guide { Orientation = GuideOrientation.Vertical, Position = 100 };
        var horizontal = new Guide { Orientation = GuideOrientation.Horizontal, Position = 40 };
        state.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(vertical).AddGuide(horizontal));
        state.SetZoom(zoom);
        state.SelectGuide(horizontal.Id);
        state.ActiveGuideIds = [vertical.Id];

        var guides = canvas.ScreenGuides;

        var scale = zoom / 100;
        Assert.Equal(new ScreenRulerGuide(true, canvas.OriginX + 100 * scale, false, true), guides[0]);
        Assert.Equal(new ScreenRulerGuide(false, canvas.OriginY + 40 * scale, true, false), guides[1]);
    }

    [Fact]
    public void ScreenGuides_Hidden_WhenShowGuidesIsOff()
    {
        var (state, canvas) = Create();
        state.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 5 }));

        state.Snapping.ShowGuides = false;

        Assert.Empty(canvas.ScreenGuides);
    }

    [Fact]
    public void ScreenGuides_AndMarkers_RaiseWhenTheirSourcesChange()
    {
        var (state, canvas) = Create();
        var raised = Watch(canvas);
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 5 };

        state.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(guide));
        state.SelectGuide(guide.Id);
        state.ActiveGuideIds = [guide.Id];
        state.SmartMarkers = [new SmartMarker(SmartMarkerType.Distance, SnapAxis.X, 0, 10, 5, 10)];
        state.Snapping.ShowGuides = false;

        Assert.Contains(nameof(CanvasAreaViewModel.ScreenGuides), raised);
        Assert.Contains(nameof(CanvasAreaViewModel.ScreenMarkers), raised);
        Assert.Contains(nameof(CanvasAreaViewModel.ShowGuides), raised);
    }

    [Fact]
    public void SmartGuidesSetting_IsForwarded()
    {
        var (state, canvas) = Create();
        var raised = Watch(canvas);

        state.Snapping.SmartGuides = true;

        Assert.True(canvas.SmartGuides);
        Assert.Contains(nameof(CanvasAreaViewModel.SmartGuides), raised);
    }

    [Fact]
    public void ScreenMarkers_ConvertLinesAndPoints()
    {
        var (state, canvas) = Create();
        state.SmartMarkers =
        [
            new SmartMarker(SmartMarkerType.Distance, SnapAxis.X, 100, 150, 40, 50),
            new SmartMarker(SmartMarkerType.EqualGap, SnapAxis.Y, 10, 30, 70, 20),
            new SmartMarker(SmartMarkerType.PathPoint, SnapAxis.X, 5, 5, 6, double.NaN),
        ];

        var markers = canvas.ScreenMarkers;

        Assert.Equal(new ScreenSmartMarker(SmartMarkerType.Distance, true, canvas.OriginX + 100, canvas.OriginX + 150, canvas.OriginY + 40, "50"), markers[0]);
        Assert.Equal(new ScreenSmartMarker(SmartMarkerType.EqualGap, false, canvas.OriginY + 10, canvas.OriginY + 30, canvas.OriginX + 70, "20"), markers[1]);
        Assert.Null(markers[2].Text);
    }

    [Theory]
    [InlineData("en", 12.5, "12.5")]
    [InlineData("de", 12.5, "12,5")]
    [InlineData("en", 12.0, "12")]
    [InlineData("en", 12.345, "12.35")]
    [InlineData("en", 0.004, "0")]
    [InlineData("de", 1234.5, "1234,5")]
    public void ScreenMarkers_TextUsesTheNumberFormatOfTheLanguage(string language, double value, string expected)
    {
        var (state, canvas) = Create(language);
        state.SmartMarkers = [new SmartMarker(SmartMarkerType.Distance, SnapAxis.X, 0, 10, 5, value)];

        Assert.Equal(expected, canvas.ScreenMarkers[0].Text);
    }

    [Fact]
    public void RulerDrag_FromBeginToRelease_MakesAGuide()
    {
        var (state, canvas) = Create();
        canvas.BeginGuideDrag(GuideOrientation.Horizontal);

        canvas.GuidePointerMoved(300, 250);
        canvas.GuidePointerReleased(300, 250);

        var guide = Assert.Single(state.Document.Guides);
        Assert.Equal(GuideOrientation.Horizontal, guide.Orientation);
        Assert.Equal(250 - canvas.OriginY, guide.Position, 6);
        Assert.Equal(HistoryStepType.CreateGuide, state.History.Steps[^1]);
    }

    [Fact]
    public void RulerDrag_VerticalUsesTheX()
    {
        var (state, canvas) = Create();
        canvas.BeginGuideDrag(GuideOrientation.Vertical);

        canvas.GuidePointerMoved(600, 50);
        canvas.GuidePointerReleased(600, 50);

        Assert.Equal(600 - canvas.OriginX, state.Document.Guides[0].Position, 6);
    }

    [Theory]
    [InlineData(300, -5)]
    [InlineData(-5, 300)]
    [InlineData(2500, 300)]
    [InlineData(300, 1300)]
    public void RulerDrag_ReleasedOnARulerOrOutside_MakesNoGuide(double x, double y)
    {
        var (state, canvas) = Create();
        canvas.BeginGuideDrag(GuideOrientation.Horizontal);

        canvas.GuidePointerMoved(300, 250);
        canvas.GuidePointerReleased(x, y);

        Assert.Empty(state.Document.Guides);
    }

    [Fact]
    public void RulerDrag_Canceled_MakesNoGuide()
    {
        var (state, canvas) = Create();
        canvas.BeginGuideDrag(GuideOrientation.Horizontal);
        canvas.GuidePointerMoved(300, 250);

        canvas.GuidePointerReleased(300, 250, canceled: true);

        Assert.Empty(state.Document.Guides);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void GuidePointerMethods_WithoutADrag_DoNothing()
    {
        var (state, canvas) = Create();

        canvas.GuidePointerMoved(10, 10);
        canvas.GuidePointerReleased(10, 10);

        Assert.Empty(state.Document.Guides);
    }

    [Fact]
    public void Escape_DuringAGuideDrag_RestoresTheGuide()
    {
        var (state, canvas) = Create();
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 400 };
        state.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(guide));
        var (screenX, _) = (canvas.OriginX + 400, 0.0);
        canvas.PointerPressed(screenX, 100);
        canvas.PointerMoved(screenX + 50, 100);

        Assert.True(canvas.KeyPressed(CanvasKey.Escape, shift: false));

        Assert.Equal(400, state.Document.FindGuide(guide.Id)!.Position, 9);
    }

    [Fact]
    public void MovingAGuideWithThePointer_AndDroppingItOnTheRuler_DeletesIt()
    {
        var (state, canvas) = Create();
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 400 };
        state.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(guide));

        canvas.PointerPressed(canvas.OriginX + 400, 100);
        canvas.PointerMoved(canvas.OriginX + 350, 100);
        canvas.PointerReleased(-10, 100);

        Assert.Empty(state.Document.Guides);
        Assert.Equal(HistoryStepType.DeleteGuide, state.History.Steps[^1]);
    }

    [Fact]
    public void Cursor_OverAGuide_ShowsAResizeCursorForItsDirection_AndWhileMovingIt()
    {
        var (state, canvas) = Create();
        var vertical = new Guide { Orientation = GuideOrientation.Vertical, Position = 400 };
        var horizontal = new Guide { Orientation = GuideOrientation.Horizontal, Position = 300 };
        state.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(vertical).AddGuide(horizontal));

        canvas.PointerHover(canvas.OriginX + 400, canvas.OriginY + 50);
        Assert.Equal(CanvasCursorType.ResizeWestEast, canvas.CursorType);
        canvas.PointerHover(canvas.OriginX + 50, canvas.OriginY + 300);
        Assert.Equal(CanvasCursorType.ResizeNorthSouth, canvas.CursorType);
        canvas.PointerHover(canvas.OriginX + 50, canvas.OriginY + 50);
        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);
        canvas.PointerLeft();
        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);

        canvas.PointerPressed(canvas.OriginX + 400, canvas.OriginY + 50);
        Assert.Equal(CanvasCursorType.ResizeWestEast, canvas.CursorType);
    }

    [Fact]
    public void Cursor_HandToolOrHiddenGuides_ShowNoGuideCursor()
    {
        var (state, canvas) = Create();
        state.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 400 }));

        state.Snapping.ShowGuides = false;
        canvas.PointerHover(canvas.OriginX + 400, canvas.OriginY + 50);
        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);

        state.Snapping.ShowGuides = true;
        state.ActiveTool = Tool.Hand;
        canvas.PointerHover(canvas.OriginX + 400, canvas.OriginY + 50);
        Assert.Equal(CanvasCursorType.OpenHand, canvas.CursorType);
    }

    [Fact]
    public void DeleteSelection_DeletesASelectedGuide()
    {
        var (state, canvas) = Create();
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 400 };
        state.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(guide));
        state.SelectGuide(guide.Id);

        canvas.DeleteSelection();

        Assert.Empty(state.Document.Guides);
    }
}
