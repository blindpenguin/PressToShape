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
