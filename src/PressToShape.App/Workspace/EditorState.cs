using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    // The ids of the layers of the previous document, bottom first, to find the next active layer when the active one is gone.
    private ImmutableArray<Guid> _layerOrder;

    /// <summary>Creates the state. Without a document it starts with an empty one: Core has no default layer name, the shell passes the real start document.</summary>
    /// <param name="document">The start document.</param>
    /// <param name="snapping">The grid and snap settings. Without them the state keeps its own, in memory only.</param>
    public EditorState(Document? document = null, SnapPreferences? snapping = null)
    {
        Snapping = snapping ?? SnapPreferences.InMemory();
        Document = document ?? Document.Create(string.Empty);
        ActiveLayerId = Document.Layers[^1].Id;
        History = new DocumentHistory(Document);
    }

    /// <summary>Raised when a pick from the canvas ended on a shape: the target that asked for it and the color that was taken.</summary>
    public event Action<ColorPickTarget, Color>? ColorPicked;

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

    /// <summary>
    /// The ids of the selected shapes, in the order they were selected. Every id belongs to a shape of the current document whose
    /// layer is visible and not locked: the state removes ids that stop fulfilling that when the document changes.
    /// </summary>
    [ObservableProperty] public partial ImmutableArray<Guid> SelectedShapeIds { get; private set; } = ImmutableArray<Guid>.Empty;

    /// <summary>The id of the selected shape if exactly one shape is selected, otherwise <c>null</c>.</summary>
    public Guid? SelectedShapeId => SelectedShapeIds.Length == 1 ? SelectedShapeIds[0] : null;

    /// <summary>How many shapes are selected.</summary>
    public int SelectedCount => SelectedShapeIds.Length;

    /// <summary>Whether at least one shape is selected.</summary>
    public bool HasSelection => !SelectedShapeIds.IsEmpty;

    /// <summary>The shape that is being drawn right now. It is not part of the document until the drawing ends.</summary>
    [ObservableProperty] public partial Shape? Draft { get; set; }

    [ObservableProperty] public partial Tool ActiveTool { get; set; } = Tool.Select;

    /// <summary>Whether the next click on the canvas takes a color from a shape (the eyedropper), and for which color. Not part of the document.</summary>
    [ObservableProperty] public partial ColorPickTarget ColorPick { get; private set; }

    /// <summary>Starts a pick from the canvas for the fill or the stroke. A new pick replaces the one before.</summary>
    public void BeginColorPick(ColorPickTarget target) => ColorPick = target;

    /// <summary>Ends the pick without a color (Escape, or a click away from every shape).</summary>
    public void CancelColorPick() => ColorPick = ColorPickTarget.None;

    /// <summary>Ends the pick. A color is reported to <see cref="ColorPicked"/>, <c>null</c> ends the pick without a change.</summary>
    public void CompleteColorPick(Color? color)
    {
        var target = ColorPick;
        ColorPick = ColorPickTarget.None;
        if (target != ColorPickTarget.None && color is { } picked)
        {
            ColorPicked?.Invoke(target, picked);
        }
    }

    /// <summary>
    /// The appearance of the next shape that is drawn. It is editor state, not part of the document: it is not saved, not undone,
    /// and a new or opened document keeps it.
    /// </summary>
    [ObservableProperty] public partial Appearance DefaultAppearance { get; set; } = Appearance.Default;

    /// <summary>
    /// Whether the four corner radii of a rectangle are linked: a change of one radius in the Properties panel, or a drag of a radius handle
    /// by touch, changes all four. Editor state, not part of the document: it is not saved and not undone.
    /// </summary>
    [ObservableProperty] public partial bool LinkCornerRadii { get; set; } = true;

    /// <summary>The shapes that were copied or cut. Kept by the app, nothing goes to the system clipboard.</summary>
    public ShapeClipboard Clipboard { get; } = new();

    /// <summary>The selection box that is being dragged, in document units, or <c>null</c>. Not part of the document.</summary>
    [ObservableProperty] public partial SelectionBox? SelectionBox { get; set; }

    /// <summary>Artboard width of the current document.</summary>
    public double ArtboardWidth => Document.ArtboardWidth;

    /// <summary>Artboard height of the current document.</summary>
    public double ArtboardHeight => Document.ArtboardHeight;

    /// <summary>
    /// Changes the document and records the change as one history step. If <paramref name="change"/> returns the
    /// same document instance, nothing changed and no step is recorded.
    /// </summary>
    /// <param name="type">What kind of step this is.</param>
    /// <param name="change">The change to the document.</param>
    /// <param name="extend">
    /// If true and the last step has the same type (nothing to redo), that step is extended instead of adding a new one.
    /// </param>
    public void Edit(HistoryStepType type, Func<Document, Document> change, bool extend = false)
    {
        ArgumentNullException.ThrowIfNull(change);

        var after = change(Document);
        if (ReferenceEquals(after, Document))
        {
            return;
        }

        RememberSelection();
        Document = after;
        if (extend ? History.RecordOrExtend(type, after) : History.Record(type, after))
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

        // A new or opened document is unrelated to the old one, so the top layer is active, as at the start.
        ActiveLayerId = document.Layers[^1].Id;
        History.Reset(document);
        ResetPen();
        ClearNodeSelection();
        RaiseHistoryChanged();
    }

    /// <summary>Goes one step back. Does nothing if there is no step or a drag is in progress.</summary>
    public void Undo()
    {
        var from = History.CurrentIndex;
        if (CanUndo && History.Undo() is { } document)
        {
            ApplyFromHistory(document, from);
        }
    }

    /// <summary>Goes one step forward. Does nothing if there is no step or a drag is in progress.</summary>
    public void Redo()
    {
        var from = History.CurrentIndex;
        if (CanRedo && History.Redo() is { } document)
        {
            ApplyFromHistory(document, from);
        }
    }

    /// <summary>
    /// Goes to the state at <paramref name="index"/> (0 is the start state). Does nothing for the current or an
    /// invalid index, or while a drag is in progress.
    /// </summary>
    public void JumpToStep(int index)
    {
        var from = History.CurrentIndex;
        if (!IsDragInProgress && History.JumpTo(index) is { } document)
        {
            ApplyFromHistory(document, from);
        }
    }

    private void ApplyFromHistory(Document document, int fromIndex)
    {
        RememberSelection();
        Document = document;

        // Group, Ungroup and the path operations replace what is selected, so going over them brings back the selection of the state that is
        // reached. Every other step keeps the selection as far as its shapes still exist.
        var low = Math.Min(fromIndex, History.CurrentIndex);
        var high = Math.Max(fromIndex, History.CurrentIndex);
        if (Enumerable.Range(low, high - low).Any(i => ReplacesSelection(History.Steps[i])))
        {
            RestoreSelection(document);
        }

        RaiseHistoryChanged();
    }

    private static bool ReplacesSelection(HistoryStepType step) =>
        step is HistoryStepType.GroupShapes
            or HistoryStepType.UngroupShapes
            or HistoryStepType.Union
            or HistoryStepType.Difference
            or HistoryStepType.Intersection
            or HistoryStepType.Exclusion
            or HistoryStepType.Combine
            or HistoryStepType.BreakApart;

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
        if (shapeId is { } id)
        {
            SelectOnly(id);
        }
        else
        {
            ClearSelection();
        }
    }

    /// <summary>Makes this shape the only selected shape. Does nothing useful for an id that cannot be selected: the selection is then cleared.</summary>
    public void SelectOnly(Guid shapeId) => SetSelection([shapeId]);

    /// <summary>
    /// Replaces the selection by these shapes or groups, keeping their order. Ids that are unknown, repeated, hidden or locked (also through
    /// their layer or a group around them) are left out. A selection holds only shapes with the same parent, the same group or the top
    /// level of the layers: of ids with different parents, the ones with the parent of the first valid id stay.
    /// </summary>
    public void SetSelection(IEnumerable<Guid> shapeIds)
    {
        ArgumentNullException.ThrowIfNull(shapeIds);

        var wanted = shapeIds.ToHashSet();
        var selectable = SelectionQueries.SelectableWithParents(Document, wanted);
        var seen = new HashSet<Guid>();
        var ids = shapeIds.Where(id => selectable.ContainsKey(id) && seen.Add(id)).ToImmutableArray();
        if (ids.Length > 1)
        {
            var parent = selectable[ids[0]];
            ids = ids.Where(id => selectable[id] == parent).ToImmutableArray();
        }

        SetSelectionCore(ids);
    }

    /// <summary>Adds the shape to the selection, or removes it if it is selected. A shape that cannot be selected is ignored.</summary>
    public void ToggleSelected(Guid shapeId)
    {
        if (SelectedShapeIds.Contains(shapeId))
        {
            SetSelectionCore(SelectedShapeIds.Remove(shapeId));
        }
        else
        {
            SetSelection(SelectedShapeIds.Add(shapeId));
        }
    }

    /// <summary>Clears the selection.</summary>
    public void ClearSelection() => SetSelectionCore(ImmutableArray<Guid>.Empty);

    private void SetSelectionCore(ImmutableArray<Guid> ids)
    {
        if (ids.SequenceEqual(SelectedShapeIds))
        {
            return;
        }

        SelectedShapeIds = ids;
        if (!ids.IsEmpty)
        {
            SelectedGuideId = null;
        }

        ClearNodeSelection();
        OnPropertyChanged(nameof(SelectedShapeId));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));
    }

    partial void OnDocumentChanged(Document value)
    {
        PruneNodeSelection(value);
        if (SelectedGuideId is { } guideId && value.FindGuide(guideId) is null)
        {
            SelectedGuideId = null;
        }

        if (!SelectedShapeIds.IsEmpty)
        {
            // Shapes that are gone, or that are now hidden or locked (also through a layer or a group), leave the selection.
            var selectable = SelectionQueries.SelectableWithParents(value, SelectedShapeIds.ToHashSet());
            SetSelectionCore(SelectedShapeIds.Where(selectable.ContainsKey).ToImmutableArray());
        }

        LeaveEnteredGroupIfGone(value);

        if (value.Layers.All(l => l.Id != ActiveLayerId))
        {
            ActiveLayerId = NearestLayerId(value);
        }

        _layerOrder = [.. value.Layers.Select(l => l.Id)];

        OnPropertyChanged(nameof(ArtboardWidth));
        OnPropertyChanged(nameof(ArtboardHeight));

        // A different artboard size changes how far the view may move.
        SetPan(PanX, PanY);
    }

    // The active layer is gone. The layer that was below it takes over, so Undo of "Add layer" and the deletion of a layer land on the
    // layer the user came from. If no layer below is left, the lowest layer is the one that was above.
    private Guid NearestLayerId(Document value)
    {
        var index = _layerOrder.IsDefault ? -1 : _layerOrder.IndexOf(ActiveLayerId);
        if (index < 0)
        {
            return value.Layers[^1].Id;
        }

        for (var i = index - 1; i >= 0; i--)
        {
            if (value.Layers.Any(l => l.Id == _layerOrder[i]))
            {
                return _layerOrder[i];
            }
        }

        return value.Layers[0].Id;
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
