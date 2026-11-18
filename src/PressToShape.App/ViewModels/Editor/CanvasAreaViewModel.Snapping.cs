using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

// What the canvas shows of the grid, the rulers and the snap guide lines. The values live in the editor state; this part only forwards them
// and converts the guide lines to screen pixels.
internal sealed partial class CanvasAreaViewModel
{
    /// <summary>Whether the grid is drawn.</summary>
    public bool ShowGrid => _state.Snapping.ShowGrid;

    /// <summary>The distance between grid lines in document units.</summary>
    public double GridSpacing => _state.Snapping.GridSpacing;

    /// <summary>Whether the rulers are shown.</summary>
    public bool ShowRulers => _state.Snapping.ShowRulers;

    /// <summary>The guide lines of the current drag in screen pixels. Empty without a snap to an object.</summary>
    public IReadOnlyList<ScreenSnapGuide> SnapGuidesOnScreen =>
        _state.SnapGuides.IsEmpty
            ? []
            : [.. _state.SnapGuides.Select(ToScreen)];

    private ScreenSnapGuide ToScreen(SnapGuide guide) => guide.Axis == SnapAxis.X
        ? new ScreenSnapGuide(true, CanvasMapping.ToScreen(guide.Position, OriginX, ZoomPercent), CanvasMapping.ToScreen(guide.From, OriginY, ZoomPercent), CanvasMapping.ToScreen(guide.To, OriginY, ZoomPercent))
        : new ScreenSnapGuide(false, CanvasMapping.ToScreen(guide.Position, OriginY, ZoomPercent), CanvasMapping.ToScreen(guide.From, OriginX, ZoomPercent), CanvasMapping.ToScreen(guide.To, OriginX, ZoomPercent));

    private void OnSnappingChanged(object? sender, System.EventArgs e)
    {
        OnPropertyChanged(nameof(ShowGrid));
        OnPropertyChanged(nameof(GridSpacing));
        OnPropertyChanged(nameof(ShowRulers));
    }
}
