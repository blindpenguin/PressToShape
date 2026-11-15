using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// Geometry of the canvas area: where the artboard is drawn and where the ruler zero points are.
/// The view only reports the viewport size, everything else is derived here.
/// </summary>
internal sealed partial class CanvasAreaViewModel : ViewModelBase
{
    private readonly EditorState _state;
    private readonly CanvasInteraction _interaction;

    public CanvasAreaViewModel(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _interaction = new CanvasInteraction(_state);
        _state.PropertyChanged += OnStateChanged;
    }

    /// <summary>Width of the area that shows the artboard, in screen pixels. Set by the view.</summary>
    [ObservableProperty] public partial double ViewportWidth { get; set; }

    /// <summary>Height of the area that shows the artboard, in screen pixels. Set by the view.</summary>
    [ObservableProperty] public partial double ViewportHeight { get; set; }

    /// <summary>The document the canvas draws.</summary>
    public Document Document => _state.Document;

    /// <summary>The shape that is being drawn, in screen space, or <c>null</c>.</summary>
    public ScreenShape? Draft => _state.Draft is { } draft ? CanvasScene.BuildShape(draft, ZoomPercent, OriginX, OriginY) : null;

    /// <summary>The outline of the selected shape in screen space, or <c>null</c>.</summary>
    public ScreenRect? SelectionOutline =>
        CanvasScene.SelectionOutline(_state.Document, _state.SelectedShapeId, ZoomPercent, OriginX, OriginY);

    /// <summary>Whether the pointer should be a crosshair: the shape tools draw by dragging.</summary>
    public bool UseCrosshair => _state.ActiveTool is Tool.Rectangle or Tool.Ellipse;

    public double ZoomPercent => _state.ZoomPercent;

    public double ArtboardWidth => _state.ArtboardWidth;

    public double ArtboardHeight => _state.ArtboardHeight;

    public double ArtboardScreenWidth => _state.ArtboardWidth * _state.ZoomPercent / 100;

    public double ArtboardScreenHeight => _state.ArtboardHeight * _state.ZoomPercent / 100;

    /// <summary>Horizontal screen position of the artboard's left edge. The horizontal ruler's zero is here.</summary>
    public double OriginX => Math.Max(0, (ViewportWidth - ArtboardScreenWidth) / 2);

    /// <summary>Vertical screen position of the artboard's top edge. The vertical ruler's zero is here.</summary>
    public double OriginY => Math.Max(0, (ViewportHeight - ArtboardScreenHeight) / 2);

    /// <summary>The primary button went down. The position is in screen pixels of the canvas area.</summary>
    public void PointerPressed(double screenX, double screenY) =>
        _interaction.PointerPressed(ToDocumentX(screenX), ToDocumentY(screenY));

    /// <summary>The pointer moved. The position is in screen pixels of the canvas area.</summary>
    public void PointerMoved(double screenX, double screenY) =>
        _interaction.PointerMoved(ToDocumentX(screenX), ToDocumentY(screenY));

    /// <summary>The button went up. The position is in screen pixels of the canvas area.</summary>
    public void PointerReleased(double screenX, double screenY) =>
        _interaction.PointerReleased(ToDocumentX(screenX), ToDocumentY(screenY));

    /// <summary>Ends a drag without a result, for example when the pointer capture is lost.</summary>
    public void Cancel() => _interaction.Cancel();

    /// <summary>Removes the selected shape. Does nothing without a selection.</summary>
    public void DeleteSelection() => _interaction.DeleteSelection();

    partial void OnViewportWidthChanged(double value)
    {
        OnPropertyChanged(nameof(OriginX));
        NotifyOverlayChanged();
    }

    partial void OnViewportHeightChanged(double value)
    {
        OnPropertyChanged(nameof(OriginY));
        NotifyOverlayChanged();
    }

    private double ToDocumentX(double screenX) => CanvasMapping.ToDocument(screenX, OriginX, ZoomPercent);

    private double ToDocumentY(double screenY) => CanvasMapping.ToDocument(screenY, OriginY, ZoomPercent);

    private void NotifyOverlayChanged()
    {
        OnPropertyChanged(nameof(Draft));
        OnPropertyChanged(nameof(SelectionOutline));
    }

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.Document):
                OnPropertyChanged(nameof(Document));
                NotifyOverlayChanged();
                break;
            case nameof(EditorState.Draft):
            case nameof(EditorState.SelectedShapeId):
                NotifyOverlayChanged();
                break;
            case nameof(EditorState.ActiveTool):
                OnPropertyChanged(nameof(UseCrosshair));
                break;
            case nameof(EditorState.ZoomPercent):
                OnPropertyChanged(nameof(ZoomPercent));
                NotifyArtboardGeometryChanged();
                break;
            case nameof(EditorState.ArtboardWidth):
            case nameof(EditorState.ArtboardHeight):
                OnPropertyChanged(nameof(ArtboardWidth));
                OnPropertyChanged(nameof(ArtboardHeight));
                NotifyArtboardGeometryChanged();
                break;
        }
    }

    private void NotifyArtboardGeometryChanged()
    {
        OnPropertyChanged(nameof(ArtboardScreenWidth));
        OnPropertyChanged(nameof(ArtboardScreenHeight));
        OnPropertyChanged(nameof(OriginX));
        OnPropertyChanged(nameof(OriginY));
        NotifyOverlayChanged();
    }
}
