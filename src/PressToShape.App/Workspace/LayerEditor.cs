using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.Core.Documents;
using PressToShape.Core.History;

namespace PressToShape.App.Workspace;

/// <summary>
/// Edits of the layers: add, delete, rename, reorder, duplicate, merge, and moving the selected objects to another layer. Each change
/// of the document is one history step. A command that would change nothing records no step, and its <c>Can...</c> property is false.
/// The commands without a layer id work on the active layer.
/// </summary>
internal sealed class LayerEditor
{
    private readonly EditorState _state;
    private readonly ILocalizer _localizer;
    private readonly Func<Guid> _newId;

    /// <param name="state">The editor state to work on.</param>
    /// <param name="localizer">Translates the default names of new layers.</param>
    /// <param name="idSource">Creates the ids of new layers and of copied shapes. Defaults to new GUIDs, tests pass their own.</param>
    public LayerEditor(EditorState state, ILocalizer localizer, Func<Guid>? idSource = null)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _newId = idSource ?? Guid.NewGuid;
    }

    /// <summary>Always true: a layer can be added to any document.</summary>
    public bool CanAdd => true;

    /// <summary>Whether the active layer can be deleted: a document keeps at least one layer.</summary>
    public bool CanDelete => _state.Document.Layers.Length > 1;

    /// <summary>Always true: the active layer can be duplicated.</summary>
    public bool CanDuplicate => true;

    /// <summary>Whether the active layer can be merged into the one below: there is one, and neither of the two is hidden or locked.</summary>
    public bool CanMerge
    {
        get
        {
            var index = ActiveIndex;
            var layers = _state.Document.Layers;
            return index > 0 && IsOpen(layers[index]) && IsOpen(layers[index - 1]);
        }
    }

    /// <summary>Whether a layer is above the active layer.</summary>
    public bool CanMoveUp => ActiveIndex < _state.Document.Layers.Length - 1;

    /// <summary>Whether a layer is below the active layer.</summary>
    public bool CanMoveDown => ActiveIndex > 0;

    /// <summary>Whether the selection can move to the layer above the one that holds it.</summary>
    public bool CanMoveSelectionUp => CanMoveSelectionBy(1);

    /// <summary>Whether the selection can move to the layer below the one that holds it.</summary>
    public bool CanMoveSelectionDown => CanMoveSelectionBy(-1);

    /// <summary>Whether the selection can move to this layer: something is selected and the layer exists and is neither hidden nor locked.</summary>
    public bool CanMoveSelectionTo(Guid layerId) =>
        _state.HasSelection && _state.Document.Layers.FirstOrDefault(l => l.Id == layerId) is { } layer && IsOpen(layer);

    /// <summary>
    /// Adds an empty layer above the active layer. It becomes the active layer, and the selection and the entered group are cleared.
    /// </summary>
    public void Add()
    {
        var layer = new Layer { Id = _newId(), Name = NextName() };
        var index = ActiveIndex + 1;
        _state.Edit(HistoryStepType.AddLayer, document => document.InsertLayer(index, layer));
        Activate(layer.Id);
    }

    /// <summary>Deletes the active layer with its objects. The layer below it becomes active, or the one above if there is none.</summary>
    public void Delete()
    {
        if (!CanDelete)
        {
            return;
        }

        var id = _state.ActiveLayerId;
        _state.Edit(HistoryStepType.DeleteLayer, document => document.RemoveLayer(id));
    }

    /// <summary>Inserts a copy of the active layer above it. The copy becomes the active layer, and the selection is cleared.</summary>
    public void Duplicate()
    {
        var original = _state.Document.Layers[ActiveIndex];
        var copyId = _newId();
        var name = string.Format(_localizer.Culture, _localizer.T("{0} copy"), original.Name);
        _state.Edit(HistoryStepType.DuplicateLayer, document => document.DuplicateLayer(original.Id, copyId, name, _newId));
        Activate(copyId);
    }

    /// <summary>Merges the active layer into the layer below it, which becomes the active layer. Does nothing if <see cref="CanMerge"/> is false.</summary>
    public void MergeDown()
    {
        if (!CanMerge)
        {
            return;
        }

        var id = _state.ActiveLayerId;
        var lower = _state.Document.Layers[ActiveIndex - 1].Id;
        _state.Edit(HistoryStepType.MergeLayers, document => document.MergeLayerDown(id));
        _state.ActiveLayerId = lower;
    }

    /// <summary>Moves the active layer one place up. Does nothing if it is the top layer.</summary>
    public void MoveUp() => MoveActiveTo(ActiveIndex + 1, CanMoveUp);

    /// <summary>Moves the active layer one place down. Does nothing if it is the bottom layer.</summary>
    public void MoveDown() => MoveActiveTo(ActiveIndex - 1, CanMoveDown);

    /// <summary>Puts a layer directly above another one, as one step. Does nothing for unknown or equal ids.</summary>
    public void MoveAbove(Guid layerId, Guid targetId)
    {
        var layers = _state.Document.Layers;
        if (layerId == targetId || layers.All(l => l.Id != layerId) || layers.All(l => l.Id != targetId))
        {
            return;
        }

        _state.Edit(HistoryStepType.ReorderLayer, document => document.MoveLayerAbove(layerId, targetId));
    }

    /// <summary>
    /// Gives a layer a name, as one step. A layer always has a name, so an empty or blank name, and the name the layer has, change nothing.
    /// </summary>
    public void Rename(Guid layerId, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var trimmed = name.Trim();
        if (trimmed.Length == 0 || _state.Document.Layers.FirstOrDefault(l => l.Id == layerId) is not { } layer || layer.Name == trimmed)
        {
            return;
        }

        _state.UpdateLayer(layerId, l => l with { Name = trimmed }, HistoryStepType.RenameLayer);
    }

    /// <summary>
    /// Moves the selected objects to the top of a layer, as one step. They stay selected. Does nothing if <see cref="CanMoveSelectionTo"/> is
    /// false or the objects are on top of the layer already.
    /// </summary>
    public void MoveSelectionToLayer(Guid layerId)
    {
        if (!CanMoveSelectionTo(layerId))
        {
            return;
        }

        var ids = _state.SelectedShapeIds.ToHashSet();
        _state.Edit(HistoryStepType.MoveToLayer, document =>
        {
            var moved = document.MoveShapesToLayer(ids, layerId);
            return SameOrder(document, moved) ? document : moved;
        });

        // The objects are top-level now, and the group that was entered may not hold them any more.
        _state.LeaveAllGroups();
    }

    /// <summary>Moves the selection to the layer above the one that holds it.</summary>
    public void MoveSelectionUp() => MoveSelectionBy(1);

    /// <summary>Moves the selection to the layer below the one that holds it.</summary>
    public void MoveSelectionDown() => MoveSelectionBy(-1);

    private int ActiveIndex
    {
        get
        {
            var layers = _state.Document.Layers;
            for (var i = 0; i < layers.Length; i++)
            {
                if (layers[i].Id == _state.ActiveLayerId)
                {
                    return i;
                }
            }

            return layers.Length - 1;
        }
    }

    private static bool IsOpen(Layer layer) => layer.IsVisible && !layer.IsLocked;

    private void Activate(Guid layerId)
    {
        _state.ActiveLayerId = layerId;
        _state.ClearSelection();
        _state.LeaveAllGroups();
    }

    private void MoveActiveTo(int index, bool allowed)
    {
        if (!allowed)
        {
            return;
        }

        var id = _state.ActiveLayerId;
        _state.Edit(HistoryStepType.ReorderLayer, document => document.MoveLayerToIndex(id, index));
    }

    // The layer that holds the whole selection, or null if nothing is selected or the selection is in several layers.
    private int? SelectionLayerIndex()
    {
        if (!_state.HasSelection)
        {
            return null;
        }

        var layerIds = _state.SelectedShapeIds.Select(id => _state.Document.FindShape(id)?.Layer.Id).Distinct().ToList();
        if (layerIds.Count != 1 || layerIds[0] is not { } layerId)
        {
            return null;
        }

        var layers = _state.Document.Layers;
        return layers.IndexOf(layers.First(l => l.Id == layerId));
    }

    private bool CanMoveSelectionBy(int step)
    {
        if (SelectionLayerIndex() is not { } index)
        {
            return false;
        }

        var target = index + step;
        var layers = _state.Document.Layers;
        return target >= 0 && target < layers.Length && IsOpen(layers[target]);
    }

    private void MoveSelectionBy(int step)
    {
        if (CanMoveSelectionBy(step) && SelectionLayerIndex() is { } index)
        {
            MoveSelectionToLayer(_state.Document.Layers[index + step].Id);
        }
    }

    // True if no layer has other shapes, or the same shapes in another order, at its top level.
    private static bool SameOrder(Document before, Document after) =>
        before.Layers.Length == after.Layers.Length
        && before.Layers.Zip(after.Layers).All(pair => pair.First.Shapes.Select(s => s.Id).SequenceEqual(pair.Second.Shapes.Select(s => s.Id)));

    // "Layer N" with the lowest N that no layer has as its name.
    private string NextName()
    {
        var used = _state.Document.Layers.Select(l => l.Name).ToHashSet();
        for (var number = 1; ; number++)
        {
            var name = string.Format(_localizer.Culture, _localizer.T("Layer {0}"), number);
            if (!used.Contains(name))
            {
                return name;
            }
        }
    }
}
