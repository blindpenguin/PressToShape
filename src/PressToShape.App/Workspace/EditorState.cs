using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// State shared by all regions of the editor shell: the document, active tool, zoom and selection summary.
/// View models observe it through PropertyChanged, so every route that changes a value stays in sync.
/// </summary>
internal sealed partial class EditorState : ObservableObject
{
    private double _zoomPercent = ZoomLevels.Default;

    /// <summary>Creates the state. Without a document it starts with an empty one: Core has no default layer name, the shell passes the real start document.</summary>
    public EditorState(Document? document = null)
    {
        Document = document ?? Document.Create(string.Empty);
        ActiveLayerId = Document.Layers[^1].Id;
    }

    /// <summary>The current document. It is immutable: an edit replaces it by a new version, which notifies observers.</summary>
    [ObservableProperty] public partial Document Document { get; set; }

    /// <summary>The layer new shapes go into. Always the id of a layer of the current document.</summary>
    [ObservableProperty] public partial Guid ActiveLayerId { get; set; }

    /// <summary>The selected shape, or <c>null</c>. Always the id of a shape of the current document.</summary>
    [ObservableProperty] public partial Guid? SelectedShapeId { get; private set; }

    /// <summary>The shape that is being drawn right now. It is not part of the document until the drawing ends.</summary>
    [ObservableProperty] public partial Shape? Draft { get; set; }

    [ObservableProperty] public partial Tool ActiveTool { get; set; } = Tool.Select;

    [ObservableProperty] public partial bool HasSelection { get; set; }

    /// <summary>A description of the selection, such as "1 selected (Group)". Empty without a selection: the status bar shows its own translated text then.</summary>
    [ObservableProperty] public partial string SelectionSummary { get; set; } = string.Empty;

    /// <summary>Artboard width of the current document.</summary>
    public double ArtboardWidth => Document.ArtboardWidth;

    /// <summary>Artboard height of the current document.</summary>
    public double ArtboardHeight => Document.ArtboardHeight;

    /// <summary>Replaces one layer of the document by the result of <paramref name="change"/>.</summary>
    public void UpdateLayer(Guid layerId, Func<Layer, Layer> change) =>
        Document = Document.UpdateLayer(layerId, change);

    /// <summary>Zoom in percent. Values outside the allowed range are clamped.</summary>
    public double ZoomPercent
    {
        get => _zoomPercent;
        set => SetProperty(ref _zoomPercent, ZoomLevels.Clamp(value));
    }

    /// <summary>Selects a shape, or clears the selection with <c>null</c>.</summary>
    public void Select(Guid? shapeId)
    {
        SelectedShapeId = shapeId;
        HasSelection = shapeId is not null;
    }

    partial void OnDocumentChanged(Document value)
    {
        if (SelectedShapeId is { } selected && value.FindShape(selected) is null)
        {
            Select(null);
        }

        if (value.Layers.All(l => l.Id != ActiveLayerId))
        {
            ActiveLayerId = value.Layers[^1].Id;
        }

        OnPropertyChanged(nameof(ArtboardWidth));
        OnPropertyChanged(nameof(ArtboardHeight));
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
