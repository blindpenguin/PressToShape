using System.Collections.Generic;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
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
}
