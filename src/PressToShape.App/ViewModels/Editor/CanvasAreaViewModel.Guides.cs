using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;

namespace PressToShape.App.ViewModels.Editor;

// The ruler guides and the smart guide markers on the canvas: converted to screen pixels, plus the drag from a ruler and the cursor over a guide.
internal sealed partial class CanvasAreaViewModel
{
    // The orientation of the guide under the mouse, for the cursor.
    private GuideOrientation? _hoverGuide;

    /// <summary>Whether the smart guides (labels, equal spacing, equal size, path points) are on.</summary>
    public bool SmartGuides => _state.Snapping.SmartGuides;

    /// <summary>Whether the ruler guides are drawn, can be moved and are snapped to.</summary>
    public bool ShowGuides => _state.Snapping.ShowGuides;

    /// <summary>The ruler guides in screen pixels. Empty while the guides are hidden.</summary>
    public IReadOnlyList<ScreenRulerGuide> ScreenGuides =>
        !_state.Snapping.ShowGuides || _state.Document.Guides.IsEmpty
            ? []
            : [.. _state.Document.Guides.Select(guide => new ScreenRulerGuide(
                guide.Orientation == GuideOrientation.Vertical,
                guide.Orientation == GuideOrientation.Vertical
                    ? CanvasMapping.ToScreen(guide.Position, OriginX, ZoomPercent)
                    : CanvasMapping.ToScreen(guide.Position, OriginY, ZoomPercent),
                guide.Id == _state.SelectedGuideId,
                _state.ActiveGuideIds.Contains(guide.Id)))];

    /// <summary>The markers and labels of the smart guides for the current drag, in screen pixels.</summary>
    public IReadOnlyList<ScreenSmartMarker> ScreenMarkers =>
        _state.SmartMarkers.IsEmpty ? [] : [.. _state.SmartMarkers.Select(ToScreen)];

    private ScreenSmartMarker ToScreen(SmartMarker marker)
    {
        var horizontal = marker.Axis == SnapAxis.X;
        var (fromOrigin, crossOrigin) = horizontal ? (OriginX, OriginY) : (OriginY, OriginX);
        return new ScreenSmartMarker(
            marker.Type,
            horizontal,
            CanvasMapping.ToScreen(marker.From, fromOrigin, ZoomPercent),
            CanvasMapping.ToScreen(marker.To, fromOrigin, ZoomPercent),
            CanvasMapping.ToScreen(marker.Cross, crossOrigin, ZoomPercent),
            double.IsFinite(marker.Value) ? Format(marker.Value) : null);
    }

    // A distance or a size with at most two decimals and no trailing zeros, in the number format of the language.
    private string Format(double value) =>
        (Math.Round(value, 2, MidpointRounding.AwayFromZero) == 0 ? 0 : Math.Round(value, 2, MidpointRounding.AwayFromZero))
            .ToString("0.##", _localizer?.Culture ?? CultureInfo.InvariantCulture);

    /// <summary>
    /// The user pressed a ruler and starts a guide: a horizontal ruler makes a horizontal guide, a vertical ruler a vertical one. The guide follows
    /// the pointer until <see cref="GuidePointerReleased"/>. Positions of the next two calls are in screen pixels relative to the canvas area.
    /// </summary>
    public void BeginGuideDrag(GuideOrientation orientation) => _interaction.BeginGuideFromRuler(orientation);

    /// <summary>The pointer moved during a drag from a ruler. Does nothing without one.</summary>
    public void GuidePointerMoved(double screenX, double screenY)
    {
        if (_interaction.IsGuideDragging)
        {
            _interaction.PointerMoved(ToDocumentX(screenX), ToDocumentY(screenY));
        }
    }

    /// <summary>
    /// The button went up after a drag from a ruler. The guide stays when the pointer is over the canvas, otherwise (on a ruler or outside) it is
    /// not made. With <paramref name="canceled"/> (Escape, a lost pointer) nothing is made.
    /// </summary>
    public void GuidePointerReleased(double screenX, double screenY, bool canceled = false)
    {
        if (!_interaction.IsGuideDragging)
        {
            return;
        }

        if (canceled)
        {
            _interaction.Abort();
            return;
        }

        _interaction.EndGuideDrag(ToDocumentX(screenX), ToDocumentY(screenY), IsOverCanvas(screenX, screenY));
    }

    // Whether a position (relative to the canvas area) is inside it. Outside is over a ruler or beyond the canvas.
    private bool IsOverCanvas(double screenX, double screenY) =>
        screenX >= 0 && screenY >= 0 && screenX <= ViewportWidth && screenY <= ViewportHeight;

    // Remembers the guide under the mouse, so the cursor shows that it can be moved. Hand and Zoom use the mouse for the view.
    private void SetGuideHover(double screenX, double screenY)
    {
        GuideOrientation? hover = null;
        if (!_panning && _state.ActiveTool is not (Tool.Hand or Tool.Zoom))
        {
            hover = _interaction.GuideAt(ToDocumentX(screenX), ToDocumentY(screenY))?.Orientation;
        }

        SetGuideHover(hover);
    }

    private void SetGuideHover(GuideOrientation? hover)
    {
        if (_hoverGuide != hover)
        {
            _hoverGuide = hover;
            OnPropertyChanged(nameof(CursorType));
        }
    }

    private void NotifyGuidesChanged()
    {
        OnPropertyChanged(nameof(ScreenGuides));
        OnPropertyChanged(nameof(ScreenMarkers));
    }
}
