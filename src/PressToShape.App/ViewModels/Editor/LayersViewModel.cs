using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// State of the Layers tab: the layers of the document as header rows, and below each layer its shapes and groups as a tree, topmost
/// first. <see cref="Rows"/> is the flat list that the view shows: the rows whose groups and layer are expanded. The list follows the
/// document. When only names, flags or the selection changed, the existing rows are updated, so the list is not rebuilt under the
/// user's click, and which groups are open survives undo and redo as far as the groups still exist.
/// </summary>
internal sealed partial class LayersViewModel : LocalizedViewModel
{
    private readonly EditorState _state;
    private readonly ObjectEditor _objects;
    private readonly HashSet<Guid> _expanded = [];
    private readonly HashSet<Guid> _knownLayers = [];
    private List<ObjectRowViewModel> _all = [];

    public LayersViewModel(EditorState state, ObjectEditor objects, ILocalizer localizer)
        : base(localizer)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _objects = objects ?? throw new ArgumentNullException(nameof(objects));

        _state.PropertyChanged += OnStateChanged;
        Sync();
        ShowSelection(revealSelected: true);
    }

    /// <summary>The rows to show, topmost first: a layer, then what is in it, and so on. Rows below a closed group or layer are not in it.</summary>
    public ObservableCollection<ObjectRowViewModel> Rows { get; } = [];

    /// <summary>
    /// A row was clicked. A layer row makes its layer the active one. A shape or group row selects the object on the canvas: the group
    /// around it is entered, so the object can be selected, and the selection becomes this row, or with <paramref name="extend"/> grows
    /// by it when it has the same parent as the rest. A row that cannot be selected (locked or hidden) is only marked as the current row.
    /// </summary>
    public void Select(ObjectRowViewModel row, bool extend)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (row.IsLayer)
        {
            _state.ActiveLayerId = row.Id;
            MarkCurrent(row);
            return;
        }

        if (SelectionQueries.IsSelectable(_state.Document, row.Id))
        {
            SelectObject(row, extend);
        }

        // After the selection changed, because a change of the selection clears the mark.
        MarkCurrent(row);
    }

    private void SelectObject(ObjectRowViewModel row, bool extend)
    {
        // The group around the row is entered, so the row can be selected. A row on the top level of a layer needs no group.
        var parent = row.ParentId;
        Guid? group = _state.Document.FindShape(parent)?.Shape is GroupShape ? parent : null;
        if (_state.EnteredGroupId != group)
        {
            if (group is { } entering)
            {
                _state.EnterGroup(entering);
            }
            else
            {
                _state.LeaveAllGroups();
            }
        }

        if (extend && _state.SelectedShapeIds.Length > 0 && SelectedParentIs(parent))
        {
            _state.ToggleSelected(row.Id);
        }
        else
        {
            _state.SelectOnly(row.Id);
        }
    }

    private void MarkCurrent(ObjectRowViewModel row)
    {
        foreach (var other in _all)
        {
            other.IsCurrent = ReferenceEquals(other, row);
        }
    }

    /// <summary>Shows or hides the rows below a layer or group.</summary>
    public void SetExpanded(ObjectRowViewModel row, bool expanded)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (!row.HasChildren || row.IsExpanded == expanded)
        {
            return;
        }

        if (expanded)
        {
            _expanded.Add(row.Id);
        }
        else
        {
            _expanded.Remove(row.Id);
        }

        row.IsExpanded = expanded;
        RefreshRows();
    }

    /// <summary>Hides or shows the object of the row, as one step.</summary>
    public void SetHidden(ObjectRowViewModel row, bool hidden)
    {
        if (row.IsLayer)
        {
            _state.UpdateLayer(row.Id, layer => layer with { IsVisible = !hidden }, hidden ? HistoryStepType.HideLayer : HistoryStepType.ShowLayer);
        }
        else
        {
            _objects.SetHidden([row.Id], hidden);
        }
    }

    /// <summary>Locks or unlocks the object of the row, as one step.</summary>
    public void SetLocked(ObjectRowViewModel row, bool locked)
    {
        if (row.IsLayer)
        {
            _state.UpdateLayer(row.Id, layer => layer with { IsLocked = locked }, locked ? HistoryStepType.LockLayer : HistoryStepType.UnlockLayer);
        }
        else
        {
            _objects.SetLocked([row.Id], locked);
        }
    }

    /// <summary>Adding layers is not part of this slice, so this cannot execute yet.</summary>
    [RelayCommand(CanExecute = nameof(CanAddLayer))]
    private void AddLayer()
    {
    }

    private static bool CanAddLayer() => false;

    /// <summary>Gives the object of the row a name, as one step. An empty name removes the name.</summary>
    public void Rename(ObjectRowViewModel row, string name) => _objects.Rename(row.Id, name);

    /// <summary>Whether the dragged row can be dropped on the target row: both are objects with the same parent.</summary>
    public bool CanReorder(ObjectRowViewModel dragged, ObjectRowViewModel target) =>
        !dragged.IsLayer && !target.IsLayer && _objects.CanMoveAbove(dragged.Id, target.Id);

    /// <summary>Puts the dragged object directly above the target in the stacking order, as one step. Does nothing if that is not possible.</summary>
    public void Reorder(ObjectRowViewModel dragged, ObjectRowViewModel target)
    {
        if (CanReorder(dragged, target))
        {
            _objects.MoveAbove(dragged.Id, target.Id);
        }
    }

    protected override void OnLanguageRefreshed() => Sync();

    private bool SelectedParentIs(Guid parent) =>
        _all.Where(r => _state.SelectedShapeIds.Contains(r.Id)).All(r => r.ParentId == parent);

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.Document):
                Sync();
                ShowSelection(revealSelected: false);
                break;
            case nameof(EditorState.ActiveLayerId):
                ShowActiveLayer();
                break;
            case nameof(EditorState.SelectedShapeIds):
                // The mark of the last clicked row is for rows that cannot be selected. Once the selection changes it has done its job.
                foreach (var row in _all)
                {
                    row.IsCurrent = false;
                }

                ShowSelection(revealSelected: true);
                break;
        }
    }

    private void ShowActiveLayer()
    {
        foreach (var row in _all)
        {
            row.IsActiveLayer = row.IsLayer && row.Id == _state.ActiveLayerId;
        }
    }

    // Marks the selected rows and, when the selection changed, opens the groups around them so they can be seen.
    private void ShowSelection(bool revealSelected)
    {
        var selected = _state.SelectedShapeIds.ToHashSet();
        var changedStructure = false;
        foreach (var row in _all)
        {
            row.IsSelected = selected.Contains(row.Id);
            if (revealSelected && row.IsSelected)
            {
                changedStructure |= Reveal(row);
            }
        }

        if (changedStructure)
        {
            RefreshRows();
        }
    }

    // Opens every group and layer above the row. Returns whether anything was opened.
    private bool Reveal(ObjectRowViewModel row)
    {
        var opened = false;
        var parentId = row.ParentId;
        while (_all.FirstOrDefault(r => r.Id == parentId && r.HasChildren) is { } parent)
        {
            if (_expanded.Add(parent.Id))
            {
                parent.IsExpanded = true;
                opened = true;
            }

            if (parent.IsLayer)
            {
                break;
            }

            parentId = parent.ParentId;
        }

        return opened;
    }

    private void Sync()
    {
        var wanted = Describe(_state.Document);
        var sameStructure = _all.Count == wanted.Count
            && _all.Zip(wanted).All(pair => pair.First.Id == pair.Second.Id && pair.First.Depth == pair.Second.Depth && pair.First.ParentId == pair.Second.ParentId);

        if (!sameStructure)
        {
            // A row keeps its place in the tree for life, so a shape that moved into a group gets a new row.
            var existing = _all.ToDictionary(r => r.Id);
            _all = wanted
                .Select(w => existing.TryGetValue(w.Id, out var row) && row.Type == w.Type && row.Depth == w.Depth && row.ParentId == w.ParentId
                    ? row
                    : new ObjectRowViewModel(this, w.Id, w.Type, w.Depth, w.ParentId))
                .ToList();

            // A layer is open the first time it is seen. After that the user decides. The set also keeps the groups that are gone, so a group
            // that comes back by undo or redo is as open or closed as it was.
            foreach (var layer in _all.Where(r => r.IsLayer && _knownLayers.Add(r.Id)))
            {
                _expanded.Add(layer.Id);
            }
        }

        for (var i = 0; i < _all.Count; i++)
        {
            var w = wanted[i];
            _all[i].Update(w.Name, w.StoredName, w.IsVisible, w.IsLocked, w.EffectivelyHidden, w.EffectivelyLocked);
            _all[i].IsExpanded = _all[i].HasChildren && _expanded.Contains(_all[i].Id);
        }

        ShowActiveLayer();
        if (!sameStructure)
        {
            RefreshRows();
        }
    }

    // Replaces the content of Rows when the rows to show are not the ones that are shown.
    private void RefreshRows()
    {
        var open = _all.ToDictionary(r => r.Id, r => r.IsExpanded);
        var byId = _all.ToDictionary(r => r.Id);
        var visible = _all.Where(row => AllAncestorsOpen(row, open, byId)).ToList();
        if (Rows.SequenceEqual(visible))
        {
            return;
        }

        Rows.Clear();
        foreach (var row in visible)
        {
            Rows.Add(row);
        }
    }

    private static bool AllAncestorsOpen(ObjectRowViewModel row, Dictionary<Guid, bool> open, Dictionary<Guid, ObjectRowViewModel> byId)
    {
        var current = row;
        while (!current.IsLayer)
        {
            if (!byId.TryGetValue(current.ParentId, out var parent) || !open[parent.Id])
            {
                return false;
            }

            current = parent;
        }

        return true;
    }

    private sealed record Described(
        Guid Id,
        ObjectRowType Type,
        int Depth,
        Guid ParentId,
        string Name,
        string StoredName,
        bool IsVisible,
        bool IsLocked,
        bool EffectivelyHidden,
        bool EffectivelyLocked);

    // The rows of the document, topmost first: a layer and then everything in it, a group before what is in it.
    private List<Described> Describe(Document document)
    {
        var rows = new List<Described>();
        foreach (var layer in document.Layers.Reverse())
        {
            rows.Add(new Described(layer.Id, ObjectRowType.Layer, 0, layer.Id, layer.Name, layer.Name, layer.IsVisible, layer.IsLocked, !layer.IsVisible, layer.IsLocked));
            AddShapes(rows, layer.Shapes, layer.Id, 1, !layer.IsVisible, layer.IsLocked);
        }

        return rows;
    }

    private void AddShapes(List<Described> rows, IEnumerable<Shape> shapes, Guid parentId, int depth, bool hiddenAbove, bool lockedAbove)
    {
        foreach (var shape in shapes.Reverse())
        {
            var hidden = hiddenAbove || !shape.IsVisible;
            var locked = lockedAbove || shape.IsLocked;
            var type = shape switch
            {
                GroupShape => ObjectRowType.Group,
                EllipseShape => ObjectRowType.Ellipse,
                _ => ObjectRowType.Rectangle,
            };
            rows.Add(new Described(shape.Id, type, depth, parentId, shape.Name.Length > 0 ? shape.Name : DefaultName(type), shape.Name, shape.IsVisible, shape.IsLocked, hidden, locked));
            if (shape is GroupShape group)
            {
                AddShapes(rows, group.Children, group.Id, depth + 1, hidden, locked);
            }
        }
    }

    // The name of an object that has none. "Group" also is the command to group, which has another German text, so it has a context.
    private string DefaultName(ObjectRowType type) => type switch
    {
        ObjectRowType.Ellipse => Localizer.T(N("Ellipse")),
        ObjectRowType.Group => Localizer.TC("object name", "Group"),
        _ => Localizer.T(N("Rectangle")),
    };
}
