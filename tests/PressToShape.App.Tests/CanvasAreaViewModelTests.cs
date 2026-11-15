using System.Collections.Generic;
using PressToShape.App.ViewModels.Editor;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelTests
{
    private static (EditorState State, CanvasAreaViewModel Canvas) Create(double width, double height)
    {
        var state = new EditorState();
        var canvas = new CanvasAreaViewModel(state) { ViewportWidth = width, ViewportHeight = height };
        return (state, canvas);
    }

    [Fact]
    public void ArtboardScreenSize_FollowsZoom()
    {
        var (state, canvas) = Create(2000, 1200);

        Assert.Equal(1200, canvas.ArtboardScreenWidth);
        Assert.Equal(800, canvas.ArtboardScreenHeight);

        state.SetZoom(50);

        Assert.Equal(600, canvas.ArtboardScreenWidth);
        Assert.Equal(400, canvas.ArtboardScreenHeight);
    }

    [Fact]
    public void Artboard_IsCentered_WhenSmallerThanViewport()
    {
        var (_, canvas) = Create(2000, 1200);

        Assert.Equal(400, canvas.OriginX);
        Assert.Equal(200, canvas.OriginY);
    }

    [Fact]
    public void Origin_StaysCentered_WhenViewportOrZoomChanges()
    {
        var (state, canvas) = Create(1000, 700);
        state.SetZoom(50); // artboard 600 x 400

        Assert.Equal(200, canvas.OriginX);
        Assert.Equal(150, canvas.OriginY);

        canvas.ViewportWidth = 1400;

        Assert.Equal(400, canvas.OriginX);
    }

    [Fact]
    public void Origin_IsNeverNegative_WhenArtboardIsLargerThanViewport()
    {
        var (_, canvas) = Create(800, 500);

        Assert.Equal(0, canvas.OriginX);
        Assert.Equal(0, canvas.OriginY);
    }

    [Fact]
    public void ZeroViewport_GivesZeroOrigin_WithoutErrors()
    {
        var (state, canvas) = Create(0, 0);

        state.SetZoom(200);

        Assert.Equal(0, canvas.OriginX);
        Assert.Equal(0, canvas.OriginY);
        Assert.Equal(2400, canvas.ArtboardScreenWidth);
    }

    [Fact]
    public void ZoomChange_RaisesNotificationsForDerivedValues()
    {
        var (state, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.SetZoom(200);

        Assert.Contains(nameof(CanvasAreaViewModel.ZoomPercent), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.ArtboardScreenWidth), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginX), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginY), changed);
    }

    [Fact]
    public void ViewportChange_RaisesOriginNotifications()
    {
        var (_, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        canvas.ViewportWidth = 2500;
        canvas.ViewportHeight = 1500;

        Assert.Contains(nameof(CanvasAreaViewModel.OriginX), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginY), changed);
    }

    [Fact]
    public void ArtboardSizeChange_UpdatesScreenSize()
    {
        var (state, canvas) = Create(2000, 1200);

        state.Document = state.Document.WithArtboardSize(1920, 1080);

        Assert.Equal(1920, canvas.ArtboardScreenWidth);
        Assert.Equal(1080, canvas.ArtboardScreenHeight);
    }

    [Fact]
    public void Document_IsTheDocumentOfTheState()
    {
        var (state, canvas) = Create(2000, 1200);

        Assert.Same(state.Document, canvas.Document);
    }

    [Fact]
    public void DocumentChange_RaisesDocumentNotification()
    {
        var (state, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        var newDocument = state.Document.UpdateLayer(state.Document.Layers[0].Id, l => l with { IsVisible = false });

        state.Document = newDocument;

        Assert.Contains(nameof(CanvasAreaViewModel.Document), changed);
        Assert.Same(newDocument, canvas.Document);
    }

    [Fact]
    public void NewArtboardSize_ChangesArtboardPropertiesAndOrigins()
    {
        var (state, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.Document = state.Document.WithArtboardSize(1000, 600);

        Assert.Equal(1000, canvas.ArtboardWidth);
        Assert.Equal(600, canvas.ArtboardHeight);
        Assert.Equal(500, canvas.OriginX);
        Assert.Equal(300, canvas.OriginY);
        Assert.Contains(nameof(CanvasAreaViewModel.ArtboardWidth), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginX), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginY), changed);
    }

    [Fact]
    public void ZoomChange_DoesNotRaiseDocumentNotification()
    {
        var (state, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.SetZoom(200);

        Assert.DoesNotContain(nameof(CanvasAreaViewModel.Document), changed);
    }

    private static List<string?> Record(CanvasAreaViewModel canvas)
    {
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        return raised;
    }

    [Fact]
    public void Pointer_DrawAtZoom200WithAnOrigin_CreatesTheShapeAtTheDocumentPosition()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Rectangle;
        state.SetZoom(200);
        var (originX, originY) = (canvas.OriginX, canvas.OriginY);

        canvas.PointerPressed(originX + 100, originY + 60);
        canvas.PointerMoved(originX + 300, originY + 160);
        canvas.PointerReleased(originX + 500, originY + 260);

        var shape = Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Equal((50, 30, 200, 100), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Draft_DuringADrag_IsInScreenSpaceAndNullAfterwards()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Ellipse;
        state.SetZoom(200);
        var origin = (canvas.OriginX, canvas.OriginY);
        var raised = Record(canvas);

        canvas.PointerPressed(origin.OriginX + 20, origin.OriginY + 20);
        canvas.PointerMoved(origin.OriginX + 120, origin.OriginY + 70);

        var draft = canvas.Draft!.Value;
        Assert.Equal(ScreenShapeType.Ellipse, draft.Type);
        Assert.Equal((origin.OriginX + 20, origin.OriginY + 20, 100, 50), (draft.X, draft.Y, draft.Width, draft.Height));
        Assert.Contains(nameof(CanvasAreaViewModel.Draft), raised);

        canvas.PointerReleased(origin.OriginX + 120, origin.OriginY + 70);

        Assert.Null(canvas.Draft);
    }

    [Fact]
    public void SelectionOutline_FollowsSelectionZoomAndOrigin()
    {
        var (state, canvas) = Create(2000, 1200);
        var shape = new RectangleShape { X = 10, Y = 20, Width = 30, Height = 40 };
        state.Document = state.Document.AddShape(state.ActiveLayerId, shape);
        var raised = Record(canvas);

        Assert.Null(canvas.SelectionOutline);

        state.Select(shape.Id);

        Assert.Contains(nameof(CanvasAreaViewModel.SelectionOutline), raised);
        Assert.Equal(new ScreenRect(canvas.OriginX + 10, canvas.OriginY + 20, 30, 40), canvas.SelectionOutline);

        raised.Clear();
        state.SetZoom(50);

        Assert.Contains(nameof(CanvasAreaViewModel.SelectionOutline), raised);
        Assert.Equal(new ScreenRect(canvas.OriginX + 5, canvas.OriginY + 10, 15, 20), canvas.SelectionOutline);
    }

    [Fact]
    public void SelectionOutline_ViewportResize_Notifies()
    {
        var (_, canvas) = Create(2000, 1200);
        var raised = Record(canvas);

        canvas.ViewportWidth = 1500;

        Assert.Contains(nameof(CanvasAreaViewModel.SelectionOutline), raised);
    }

    [Fact]
    public void UseCrosshair_FollowsTheTool()
    {
        var (state, canvas) = Create(2000, 1200);
        var raised = Record(canvas);

        Assert.False(canvas.UseCrosshair);

        state.ActiveTool = Tool.Rectangle;
        Assert.True(canvas.UseCrosshair);
        Assert.Contains(nameof(CanvasAreaViewModel.UseCrosshair), raised);

        state.ActiveTool = Tool.Ellipse;
        Assert.True(canvas.UseCrosshair);

        state.ActiveTool = Tool.Select;
        Assert.False(canvas.UseCrosshair);
    }

    [Fact]
    public void DeleteSelection_SelectedShape_RemovesIt()
    {
        var (state, canvas) = Create(2000, 1200);
        var shape = new RectangleShape { Width = 10, Height = 10 };
        state.Document = state.Document.AddShape(state.ActiveLayerId, shape);
        state.Select(shape.Id);

        canvas.DeleteSelection();

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Null(canvas.SelectionOutline);
    }

    [Fact]
    public void Cancel_DuringADrag_ClearsTheDraft()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Rectangle;
        canvas.PointerPressed(canvas.OriginX + 10, canvas.OriginY + 10);
        canvas.PointerMoved(canvas.OriginX + 90, canvas.OriginY + 90);

        canvas.Cancel();

        Assert.Null(canvas.Draft);
    }
}
