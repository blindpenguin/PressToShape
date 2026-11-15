using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// State shared by all regions of the editor shell: the document, its undo history, active tool, zoom and selection summary.
/// View models observe it through PropertyChanged, so every route that changes a value stays in sync.
/// This class is the only place that changes the document: edits go through <see cref="Edit"/>, a drag through
/// <see cref="Preview"/> and <see cref="Commit"/>, and a new or opened document through <see cref="ReplaceDocument"/>.
/// </summary>
internal sealed partial class EditorState : ObservableObject
{
    private double _zoomPercent = ZoomLevels.Default;

    /// <summary>Creates the state. Without a document it starts with an empty one: Core has no default layer name, the shell passes the real start document.</summary>
    public EditorState(Document? document = null)
    {
        Document = document ?? Document.Create(string.Empty);
        ActiveLayerId = Document.Layers[^1].Id;
        History = new DocumentHistory(Document);
    }

    /// <summary>Raised after the undo history changed: a step was recorded, undone or redone, or the history was reset.</summary>
    public event EventHandler? HistoryChanged;

    /// <summary>The undo history of the current document.</summary>
    public DocumentHistory History { get; }

    /// <summary>Whether a drag on the canvas is in progress. Undo, redo and jumping in the history are not possible then.</summary>
    [ObservableProperty] public partial bool IsDragInProgress { get; set; }

    /// <summary>Whether a step can be undone right now.</summary>
    public bool CanUndo => !IsDragInProgress && History.CanUndo;

    /// <summary>Whether a step can be redone right now.</summary>
    public bool CanRedo => !IsDragInProgress && History.CanRedo;

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

    /// <summary>
    /// Changes the document and records the change as one history step. If <paramref name="change"/> returns the
    /// same document instance, nothing changed and no step is recorded.
    /// </summary>
    public void Edit(HistoryStepType type, Func<Document, Document> change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var after = change(Document);
        if (ReferenceEquals(after, Document))
        {
            return;
        }

        Document = after;
        if (History.Record(type, after))
        {
            RaiseHistoryChanged();
        }
    }

    /// <summary>Replaces one layer of the document by the result of <paramref name="change"/>, as one history step.</summary>
    public void UpdateLayer(Guid layerId, Func<Layer, Layer> change, HistoryStepType type) =>
        Edit(type, document => document.UpdateLayer(layerId, change));

    /// <summary>Sets the document without recording a step, for a change that is still going on such as a drag. End it with <see cref="Commit"/>.</summary>
    public void Preview(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        Document = document;
    }

    /// <summary>Records the current document as one step, if it differs from the last recorded one.</summary>
    public void Commit(HistoryStepType type)
    {
        if (History.Record(type, Document))
        {
            RaiseHistoryChanged();
        }
    }

    /// <summary>Shows a different document (New, Open) and starts a fresh history.</summary>
    public void ReplaceDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        Document = document;
        History.Reset(document);
        RaiseHistoryChanged();
    }

    /// <summary>Goes one step back. Does nothing if there is no step or a drag is in progress.</summary>
    public void Undo()
    {
        if (CanUndo && History.Undo() is { } document)
        {
            ApplyFromHistory(document);
        }
    }

    /// <summary>Goes one step forward. Does nothing if there is no step or a drag is in progress.</summary>
    public void Redo()
    {
        if (CanRedo && History.Redo() is { } document)
        {
            ApplyFromHistory(document);
        }
    }

    /// <summary>
    /// Goes to the state at <paramref name="index"/> (0 is the start state). Does nothing for the current or an
    /// invalid index, or while a drag is in progress.
    /// </summary>
    public void JumpToStep(int index)
    {
        if (!IsDragInProgress && History.JumpTo(index) is { } document)
        {
            ApplyFromHistory(document);
        }
    }

    private void ApplyFromHistory(Document document)
    {
        Document = document;
        RaiseHistoryChanged();
    }

    private void RaiseHistoryChanged()
    {
        HistoryChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    partial void OnIsDragInProgressChanged(bool value)
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    /// <summary>
    /// How far the view is moved from its centered position, in screen pixels. Part of the view, not of the document:
    /// it never changes the document, the history or the unsaved state.
    /// </summary>
    [ObservableProperty] public partial double PanX { get; private set; }

    /// <summary>The vertical view offset, see <see cref="PanX"/>.</summary>
    [ObservableProperty] public partial double PanY { get; private set; }

    /// <summary>Width of the canvas area in screen pixels, or 0 while it is not known. Set through <see cref="SetViewport"/>.</summary>
    [ObservableProperty] public partial double ViewportWidth { get; private set; }

    /// <summary>Height of the canvas area in screen pixels, or 0 while it is not known. Set through <see cref="SetViewport"/>.</summary>
    [ObservableProperty] public partial double ViewportHeight { get; private set; }

    /// <summary>
    /// Zoom in percent. Values outside the allowed range are clamped. Setting it keeps the middle of the canvas area where it is.
    /// </summary>
    public double ZoomPercent
    {
        get => _zoomPercent;
        set => ZoomAround(value, ViewportWidth / 2, ViewportHeight / 2);
    }

    /// <summary>Tells the state how large the canvas area is. The view offset is limited again for the new size.</summary>
    public void SetViewport(double width, double height)
    {
        ViewportWidth = double.IsFinite(width) ? Math.Max(0, width) : 0;
        ViewportHeight = double.IsFinite(height) ? Math.Max(0, height) : 0;
        SetPan(PanX, PanY);
    }

    /// <summary>Moves the view to this offset, limited so that the artboard stays visible. A non-finite value leaves that axis unchanged.</summary>
    public void SetPan(double x, double y)
    {
        var scale = _zoomPercent / 100;
        if (double.IsFinite(x))
        {
            PanX = ViewGeometry.ClampPan(x, ViewportWidth, ArtboardWidth * scale);
        }
        else
        {
            PanX = ViewGeometry.ClampPan(PanX, ViewportWidth, ArtboardWidth * scale);
        }

        if (double.IsFinite(y))
        {
            PanY = ViewGeometry.ClampPan(y, ViewportHeight, ArtboardHeight * scale);
        }
        else
        {
            PanY = ViewGeometry.ClampPan(PanY, ViewportHeight, ArtboardHeight * scale);
        }
    }

    /// <summary>Moves the view by this distance, see <see cref="SetPan"/>.</summary>
    public void PanBy(double dx, double dy) => SetPan(PanX + (double.IsFinite(dx) ? dx : 0), PanY + (double.IsFinite(dy) ? dy : 0));

    /// <summary>
    /// Changes the zoom so that the point of the drawing under the screen position (<paramref name="anchorX"/>, <paramref name="anchorY"/>)
    /// of the canvas area stays under it. The zoom stays in its range, and the view offset stays limited.
    /// </summary>
    public void ZoomAround(double percent, double anchorX, double anchorY)
    {
        var newZoom = ZoomLevels.Clamp(percent);
        var oldZoom = _zoomPercent;
        if (newZoom == oldZoom)
        {
            return;
        }

        var newPanX = ViewGeometry.PanAfterZoom(PanX, ViewportWidth, oldZoom, newZoom, ArtboardWidth, anchorX);
        var newPanY = ViewGeometry.PanAfterZoom(PanY, ViewportHeight, oldZoom, newZoom, ArtboardHeight, anchorY);

        // The new zoom is in place before the offset changes, so observers of the offset already see the final zoom.
        _zoomPercent = newZoom;
        SetPan(newPanX, newPanY);
        OnPropertyChanged(nameof(ZoomPercent));
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

        // A different artboard size changes how far the view may move.
        SetPan(PanX, PanY);
    }

    public void SetZoom(double percent) => ZoomPercent = percent;

    public void ZoomIn() => ZoomPercent = ZoomLevels.NextIn(ZoomPercent);

    public void ZoomOut() => ZoomPercent = ZoomLevels.NextOut(ZoomPercent);

    public void ZoomToActualSize() => ZoomPercent = ZoomLevels.Default;

    /// <summary>
    /// Sets the zoom so the artboard fits the viewport and centers the artboard (the view offset becomes zero).
    /// The zoom stays unchanged if the viewport has no size yet.
    /// </summary>
    public void ZoomToFit(double viewportWidth, double viewportHeight)
    {
        PanX = 0;
        PanY = 0;

        var fit = ZoomLevels.FitToViewport(viewportWidth, viewportHeight, ArtboardWidth, ArtboardHeight);
        if (fit is { } zoom && zoom != _zoomPercent)
        {
            _zoomPercent = zoom;
            OnPropertyChanged(nameof(ZoomPercent));
        }
    }
}
