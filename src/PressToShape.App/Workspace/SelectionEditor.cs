using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// Edits that work on the selected shapes and do not need the mouse: select all, deselect, copy, cut, paste, duplicate
/// (and, in the same class, nudging and property edits). Every change of the document is one history step.
/// </summary>
internal sealed class SelectionEditor
{
    private readonly EditorState _state;
    private readonly Func<Guid> _newId;
    private Document? _lastNudgeDocument;
    private System.Collections.Immutable.ImmutableArray<Guid> _lastNudgeSelection = System.Collections.Immutable.ImmutableArray<Guid>.Empty;

    /// <param name="state">The editor state to work on.</param>
    /// <param name="idSource">Creates the ids of pasted and duplicated shapes. Defaults to new GUIDs, tests pass their own.</param>
    public SelectionEditor(EditorState state, Func<Guid>? idSource = null)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _newId = idSource ?? Guid.NewGuid;
    }

    /// <summary>Whether there is a selection to copy.</summary>
    public bool CanCopy => _state.HasSelection;

    /// <summary>Whether there is a selection to cut.</summary>
    public bool CanCut => _state.HasSelection;

    /// <summary>Whether there is a selection to duplicate.</summary>
    public bool CanDuplicate => _state.HasSelection;

    /// <summary>Whether something can be pasted: the clipboard has shapes and the active layer is visible and not locked.</summary>
    public bool CanPaste => !_state.Clipboard.IsEmpty && ActiveLayerAcceptsShapes();

    /// <summary>Selects every shape of every visible, unlocked layer.</summary>
    public void SelectAll() => _state.SetSelection(SelectionQueries.Selectable(_state.Document).Select(s => s.Id));

    /// <summary>Clears the selection.</summary>
    public void Deselect() => _state.ClearSelection();

    /// <summary>Puts the selected shapes on the clipboard. The document stays unchanged.</summary>
    public void Copy()
    {
        if (CanCopy)
        {
            _state.Clipboard.Set(SelectedShapes());
        }
    }

    /// <summary>Copies the selected shapes and removes them from the document, as one step.</summary>
    public void Cut()
    {
        if (!CanCut)
        {
            return;
        }

        var shapes = SelectedShapes();
        _state.Clipboard.Set(shapes);
        _state.Edit(HistoryStepType.CutShapes, document => shapes.Aggregate(document, (d, shape) => d.RemoveShape(shape.Id)));
    }

    /// <summary>
    /// Adds copies of the clipboard shapes to the top of the active layer, as one step, and selects them. Each paste moves the copies
    /// 10 units further right and down, unless <paramref name="inPlace"/> is true.
    /// </summary>
    public void Paste(bool inPlace)
    {
        if (!CanPaste)
        {
            return;
        }

        var offset = inPlace ? 0 : _state.Clipboard.NextPasteOffset();
        var copies = _state.Clipboard.Shapes.Select(shape => Moved(shape, offset)).ToList();
        var layerId = _state.ActiveLayerId;
        _state.Edit(HistoryStepType.PasteShapes, document => copies.Aggregate(document, (d, copy) => d.AddShape(layerId, copy)));
        _state.SetSelection(copies.Select(c => c.Id));
    }

    /// <summary>
    /// Adds copies of the selected shapes, each at the top of its own layer, 10 units right and down, as one step, and selects them.
    /// The clipboard is not touched.
    /// </summary>
    public void Duplicate()
    {
        if (!CanDuplicate)
        {
            return;
        }

        var originals = SelectedShapes();
        var copies = originals
            .Select(shape => (LayerId: _state.Document.FindShape(shape.Id)!.Layer.Id, Copy: Moved(shape, ShapeClipboard.PasteStep)))
            .ToList();
        _state.Edit(HistoryStepType.DuplicateShapes, document => copies.Aggregate(document, (d, c) => d.AddShape(c.LayerId, c.Copy)));
        _state.SetSelection(copies.Select(c => c.Copy.Id));
    }

    /// <summary>
    /// Moves the selected shapes by the distance in document units, not limited to the artboard. Repeated nudges of the same
    /// selection become one history step, as long as the document was not changed by anything else in between.
    /// </summary>
    public void Nudge(double dx, double dy)
    {
        if (!_state.HasSelection || (dx == 0 && dy == 0))
        {
            return;
        }

        var selection = _state.SelectedShapeIds;
        var extend = ReferenceEquals(_lastNudgeDocument, _state.Document) && selection.SequenceEqual(_lastNudgeSelection);
        var before = _state.Document;
        Edit(HistoryStepType.NudgeShapes, shape => shape with { X = shape.X + dx, Y = shape.Y + dy }, extend);
        if (!ReferenceEquals(before, _state.Document))
        {
            _lastNudgeDocument = _state.Document;
            _lastNudgeSelection = selection;
        }
    }

    /// <summary>Applies the change to every selected shape as one step. No step if no shape changed.</summary>
    public void Apply(HistoryStepType type, Func<Shape, Shape> change)
    {
        ArgumentNullException.ThrowIfNull(change);

        Edit(type, change, extend: false);
    }

    /// <summary>
    /// Replaces the selected shapes by the returned list, which must have the same shapes (same ids) in the same order, as one step.
    /// For changes that need all shapes together. No step if nothing changed.
    /// </summary>
    public void ApplyTransform(HistoryStepType type, Func<IReadOnlyList<Shape>, IReadOnlyList<Shape>> change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var current = SelectedShapes();
        if (current.Count == 0)
        {
            return;
        }

        var result = change(current);
        if (result.Count != current.Count || current.Where((shape, i) => shape.Id != result[i].Id).Any())
        {
            throw new InvalidOperationException("The change must return the same shapes in the same order.");
        }

        var replacements = result.ToDictionary(shape => shape.Id);
        _state.Edit(type, document => ReplaceShapes(document, current, replacements));
    }

    private void Edit(HistoryStepType type, Func<Shape, Shape> change, bool extend)
    {
        var current = SelectedShapes();
        if (current.Count == 0)
        {
            return;
        }

        var replacements = current.ToDictionary(shape => shape.Id, change);
        _state.Edit(type, document => ReplaceShapes(document, current, replacements), extend);
    }

    private static Document ReplaceShapes(Document document, IReadOnlyList<Shape> current, Dictionary<Guid, Shape> replacements)
    {
        foreach (var shape in current)
        {
            var replacement = replacements[shape.Id];
            if (!Equals(shape, replacement))
            {
                document = document.UpdateShape(shape.Id, _ => replacement);
            }
        }

        return document;
    }

    private Shape Moved(Shape shape, double offset) => shape with { Id = _newId(), X = shape.X + offset, Y = shape.Y + offset };

    private IReadOnlyList<Shape> SelectedShapes() => SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds);

    private bool ActiveLayerAcceptsShapes() =>
        _state.Document.Layers.FirstOrDefault(l => l.Id == _state.ActiveLayerId) is { IsVisible: true, IsLocked: false };
}
