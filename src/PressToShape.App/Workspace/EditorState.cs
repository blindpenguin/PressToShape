using CommunityToolkit.Mvvm.ComponentModel;

namespace PressToShape.App.Workspace;

/// <summary>
/// State shared by all regions of the editor shell: active tool, zoom, selection summary and artboard size.
/// View models observe it through PropertyChanged, so every route that changes a value stays in sync.
/// </summary>
internal sealed partial class EditorState : ObservableObject
{
    private double _zoomPercent = ZoomLevels.Default;

    [ObservableProperty] public partial Tool ActiveTool { get; set; } = Tool.Select;

    [ObservableProperty] public partial bool HasSelection { get; set; }

    /// <summary>A description of the selection, such as "1 selected (Group)". Empty without a selection: the status bar shows its own translated text then.</summary>
    [ObservableProperty] public partial string SelectionSummary { get; set; } = string.Empty;

    [ObservableProperty] public partial double ArtboardWidth { get; set; } = 1200;

    [ObservableProperty] public partial double ArtboardHeight { get; set; } = 800;

    /// <summary>Zoom in percent. Values outside the allowed range are clamped.</summary>
    public double ZoomPercent
    {
        get => _zoomPercent;
        set => SetProperty(ref _zoomPercent, ZoomLevels.Clamp(value));
    }

    public void SetZoom(double percent) => ZoomPercent = percent;

    public void ZoomIn() => ZoomPercent = ZoomLevels.NextIn(ZoomPercent);

    public void ZoomOut() => ZoomPercent = ZoomLevels.NextOut(ZoomPercent);

    public void ZoomToActualSize() => ZoomPercent = ZoomLevels.Default;

    /// <summary>Sets the zoom so the artboard fits the viewport. Does nothing if the viewport has no size yet.</summary>
    public void ZoomToFit(double viewportWidth, double viewportHeight)
    {
        var fit = ZoomLevels.FitToViewport(viewportWidth, viewportHeight, ArtboardWidth, ArtboardHeight);
        if (fit is { } zoom)
        {
            ZoomPercent = zoom;
        }
    }
}
