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

    private bool _panning;
    private double _panStartScreenX;
    private double _panStartScreenY;
    private double _panStartX;
    private double _panStartY;

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

    /// <summary>The pointer shape for the active tool. A closed hand while the view is being dragged.</summary>
    public CanvasCursorType CursorType => _panning
        ? CanvasCursorType.ClosedHand
        : _state.ActiveTool switch
        {
            Tool.Rectangle or Tool.Ellipse => CanvasCursorType.Crosshair,
            Tool.Hand => CanvasCursorType.OpenHand,
            Tool.Zoom => CanvasCursorType.Magnifier,
            _ => CanvasCursorType.Arrow,
        };

    /// <summary>Smallest value of the horizontal scroll bar: minus the furthest the view offset may go.</summary>
    public double HorizontalScrollMinimum => -HorizontalScrollMaximum;

    /// <summary>Largest value of the horizontal scroll bar: the furthest the view offset may go.</summary>
    public double HorizontalScrollMaximum => ScrollLimit(ViewGeometry.PanLimit(ViewportWidth, ArtboardScreenWidth));

    /// <summary>The horizontal scroll bar's value: the view offset, negated, because the bar's value grows when the view moves right. Setting it moves the view.</summary>
    public double HorizontalScrollValue
    {
        get => 0 - _state.PanX;
        set => _state.SetPan(0 - value, _state.PanY);
    }

    /// <summary>The visible width, which sets the size of the horizontal thumb.</summary>
    public double HorizontalScrollViewportSize => ViewportWidth;

    /// <summary>Whether the view can be moved horizontally at all.</summary>
    public bool CanScrollHorizontally => HorizontalScrollMaximum > 0;

    /// <summary>Smallest value of the vertical scroll bar.</summary>
    public double VerticalScrollMinimum => -VerticalScrollMaximum;

    /// <summary>Largest value of the vertical scroll bar.</summary>
    public double VerticalScrollMaximum => ScrollLimit(ViewGeometry.PanLimit(ViewportHeight, ArtboardScreenHeight));

    /// <summary>The vertical scroll bar's value, see <see cref="HorizontalScrollValue"/>.</summary>
    public double VerticalScrollValue
    {
        get => 0 - _state.PanY;
        set => _state.SetPan(_state.PanX, 0 - value);
    }

    /// <summary>The visible height, which sets the size of the vertical thumb.</summary>
    public double VerticalScrollViewportSize => ViewportHeight;

    /// <summary>Whether the view can be moved vertically at all.</summary>
    public bool CanScrollVertically => VerticalScrollMaximum > 0;

    /// <summary>Whether the view is being dragged right now.</summary>
    public bool IsPanning => _panning;

    public double ZoomPercent => _state.ZoomPercent;

    public double ArtboardWidth => _state.ArtboardWidth;

    public double ArtboardHeight => _state.ArtboardHeight;

    public double ArtboardScreenWidth => _state.ArtboardWidth * _state.ZoomPercent / 100;

    public double ArtboardScreenHeight => _state.ArtboardHeight * _state.ZoomPercent / 100;

    /// <summary>
    /// Horizontal screen position of the artboard's left edge, where the horizontal ruler's zero is. The artboard is centered
    /// (also when it is larger than the canvas area) and then moved by the view offset.
    /// </summary>
    public double OriginX => ViewGeometry.Origin(ViewportWidth, ArtboardScreenWidth, _state.PanX);

    /// <summary>Vertical screen position of the artboard's top edge. The vertical ruler's zero is here.</summary>
    public double OriginY => ViewGeometry.Origin(ViewportHeight, ArtboardScreenHeight, _state.PanY);

    /// <summary>
    /// The primary button went down. The position is in screen pixels of the canvas area. With the Hand tool this starts
    /// dragging the view, with the Zoom tool it zooms one step in (with <paramref name="alt"/>: out) around the position,
    /// with the other tools it is handled by the tool.
    /// </summary>
    public void PointerPressed(double screenX, double screenY, bool alt = false)
    {
        switch (_state.ActiveTool)
        {
            case Tool.Hand:
                _interaction.Cancel();
                BeginPan(screenX, screenY);
                break;
            case Tool.Zoom:
                _interaction.Cancel();
                var zoom = alt ? ZoomLevels.NextOut(_state.ZoomPercent) : ZoomLevels.NextIn(_state.ZoomPercent);
                _state.ZoomAround(zoom, screenX, screenY);
                break;
            default:
                _interaction.PointerPressed(ToDocumentX(screenX), ToDocumentY(screenY));
                break;
        }
    }

    /// <summary>The middle button went down: drags the view with any tool, unless another drag is in progress.</summary>
    public void MiddlePressed(double screenX, double screenY)
    {
        if (!_state.IsDragInProgress)
        {
            BeginPan(screenX, screenY);
        }
    }

    /// <summary>The pointer moved. The position is in screen pixels of the canvas area.</summary>
    public void PointerMoved(double screenX, double screenY)
    {
        if (_panning)
        {
            // Absolute from the press, so the point under the pointer stays under it however often the pointer moved.
            _state.SetPan(_panStartX + (screenX - _panStartScreenX), _panStartY + (screenY - _panStartScreenY));
            return;
        }

        _interaction.PointerMoved(ToDocumentX(screenX), ToDocumentY(screenY));
    }

    /// <summary>The button went up. The position is in screen pixels of the canvas area.</summary>
    public void PointerReleased(double screenX, double screenY)
    {
        if (_panning)
        {
            PointerMoved(screenX, screenY);
            EndPan();
            return;
        }

        _interaction.PointerReleased(ToDocumentX(screenX), ToDocumentY(screenY));
    }

    /// <summary>Ends a drag without a result, for example when the pointer capture is lost.</summary>
    public void Cancel()
    {
        if (_panning)
        {
            EndPan();
            return;
        }

        _interaction.Cancel();
    }

    /// <summary>
    /// The wheel (or a touchpad scroll) turned over the canvas. With Ctrl it zooms around the pointer, otherwise it moves the view,
    /// Shift moving it horizontally. A positive delta moves the view toward the top or the left.
    /// </summary>
    /// <returns>Whether the event changed the view, so the caller marks it as handled.</returns>
    public bool Wheel(double deltaX, double deltaY, bool control, bool shift, double screenX, double screenY)
    {
        if (_state.IsDragInProgress
            || !double.IsFinite(deltaX) || !double.IsFinite(deltaY) || !double.IsFinite(screenX) || !double.IsFinite(screenY))
        {
            return false;
        }

        if (control)
        {
            var zoom = ViewGeometry.WheelZoom(_state.ZoomPercent, deltaY);
            if (deltaY == 0 || zoom == _state.ZoomPercent)
            {
                return false;
            }

            _state.ZoomAround(zoom, screenX, screenY);
            return true;
        }

        var (dx, dy) = shift && deltaX == 0 ? (deltaY, 0.0) : (deltaX, deltaY);
        var (beforeX, beforeY) = (_state.PanX, _state.PanY);
        _state.PanBy(dx * ViewGeometry.WheelStep, dy * ViewGeometry.WheelStep);
        return _state.PanX != beforeX || _state.PanY != beforeY;
    }

    private void BeginPan(double screenX, double screenY)
    {
        _panning = true;
        _panStartScreenX = screenX;
        _panStartScreenY = screenY;
        _panStartX = _state.PanX;
        _panStartY = _state.PanY;
        _state.IsDragInProgress = true;
        OnPropertyChanged(nameof(IsPanning));
        OnPropertyChanged(nameof(CursorType));
    }

    private void EndPan()
    {
        _panning = false;
        _state.IsDragInProgress = false;
        OnPropertyChanged(nameof(IsPanning));
        OnPropertyChanged(nameof(CursorType));
    }

    /// <summary>Removes the selected shape. Does nothing without a selection.</summary>
    public void DeleteSelection() => _interaction.DeleteSelection();

    partial void OnViewportWidthChanged(double value)
    {
        _state.SetViewport(value, ViewportHeight);
        OnPropertyChanged(nameof(OriginX));
        NotifyOverlayChanged();
        NotifyScrollChanged();
    }

    partial void OnViewportHeightChanged(double value)
    {
        _state.SetViewport(ViewportWidth, value);
        OnPropertyChanged(nameof(OriginY));
        NotifyOverlayChanged();
        NotifyScrollChanged();
    }

    private double ToDocumentX(double screenX) => CanvasMapping.ToDocument(screenX, OriginX, ZoomPercent);

    private double ToDocumentY(double screenY) => CanvasMapping.ToDocument(screenY, OriginY, ZoomPercent);

    // Without a known canvas size the offset has no limit, and the scroll bar has no range.
    private static double ScrollLimit(double limit) => double.IsFinite(limit) ? limit : 0;

    private void NotifyScrollChanged()
    {
        OnPropertyChanged(nameof(HorizontalScrollMinimum));
        OnPropertyChanged(nameof(HorizontalScrollMaximum));
        OnPropertyChanged(nameof(HorizontalScrollValue));
        OnPropertyChanged(nameof(HorizontalScrollViewportSize));
        OnPropertyChanged(nameof(CanScrollHorizontally));
        OnPropertyChanged(nameof(VerticalScrollMinimum));
        OnPropertyChanged(nameof(VerticalScrollMaximum));
        OnPropertyChanged(nameof(VerticalScrollValue));
        OnPropertyChanged(nameof(VerticalScrollViewportSize));
        OnPropertyChanged(nameof(CanScrollVertically));
    }

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
                if (_panning)
                {
                    // Changing the tool during a pan ends it.
                    EndPan();
                }

                OnPropertyChanged(nameof(CursorType));
                break;
            case nameof(EditorState.ZoomPercent):
                OnPropertyChanged(nameof(ZoomPercent));
                NotifyArtboardGeometryChanged();
                break;
            case nameof(EditorState.PanX):
            case nameof(EditorState.PanY):
                OnPropertyChanged(nameof(OriginX));
                OnPropertyChanged(nameof(OriginY));
                NotifyOverlayChanged();
                NotifyScrollChanged();
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
        NotifyScrollChanged();
    }
}
