using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// Edits of the structure of the drawing: group and ungroup, arrange, lock and hide single objects, and names. Each change of the
/// document is one history step. A command that would change nothing records no step, and its <c>Can...</c> property is false.
/// </summary>
internal sealed class ObjectEditor
{
    private readonly EditorState _state;
    private readonly Func<Guid> _newId;

    /// <param name="state">The editor state to work on.</param>
    /// <param name="idSource">Creates the id of a new group. Defaults to new GUIDs, tests pass their own.</param>
    public ObjectEditor(EditorState state, Func<Guid>? idSource = null)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _newId = idSource ?? Guid.NewGuid;
    }

    /// <summary>Whether at least two shapes with the same layer and the same group are selected.</summary>
    public bool CanGroup
    {
        get
        {
            var ids = _state.SelectedShapeIds;
            if (ids.Length < 2)
            {
                return false;
            }

            var first = _state.Document.FindShape(ids[0]);
            return first is not null && ids.Skip(1).All(id =>
                _state.Document.FindShape(id) is { } location && location.Layer.Id == first.Layer.Id && location.Parent?.Id == first.Parent?.Id);
        }
    }

    /// <summary>Whether a group is selected.</summary>
    public bool CanUngroup => SelectedShapes().Any(shape => shape is GroupShape);

    /// <summary>Whether the selection has something to hide.</summary>
    public bool CanHideSelection => SelectedShapes().Any(shape => shape.IsVisible);

    /// <summary>Whether the selection has something to lock.</summary>
    public bool CanLockSelection => SelectedShapes().Any(shape => !shape.IsLocked);

    /// <summary>Whether any shape or group of the document, nested ones included, is hidden.</summary>
    public bool CanShowAll => !ReferenceEquals(_state.Document.ShowAll(), _state.Document);

    /// <summary>Whether any shape or group of the document, nested ones included, is locked.</summary>
    public bool CanUnlockAll => !ReferenceEquals(_state.Document.UnlockAll(), _state.Document);

    /// <summary>Whether arranging the selection changes the order of anything.</summary>
    public bool CanArrange(ArrangeType type) =>
        _state.HasSelection && !ReferenceEquals(_state.Document.Arrange(_state.SelectedShapeIds.ToHashSet(), type), _state.Document);

    /// <summary>Whether the shape can be moved directly above the target: both exist, are different and have the same layer and group.</summary>
    public bool CanMoveAbove(Guid id, Guid targetId) =>
        id != targetId
        && _state.Document.FindShape(id) is { } shape
        && _state.Document.FindShape(targetId) is { } target
        && shape.Layer.Id == target.Layer.Id
        && shape.Parent?.Id == target.Parent?.Id;

    /// <summary>Replaces the selected shapes by a new group, which becomes the selection. Does nothing if <see cref="CanGroup"/> is false.</summary>
    public void Group()
    {
        if (!CanGroup)
        {
            return;
        }

        var ids = _state.SelectedShapeIds;
        var groupId = _newId();
        _state.Edit(HistoryStepType.GroupShapes, document => document.Group(ids, groupId));
        _state.SetSelection([groupId]);
    }

    /// <summary>Replaces the selected groups by their shapes, which become the selection together with the other selected shapes.</summary>
    public void Ungroup()
    {
        if (!CanUngroup)
        {
            return;
        }

        var selected = SelectedShapes();
        var groups = selected.OfType<GroupShape>().Select(group => group.Id).ToHashSet();
        var after = selected.SelectMany(shape => shape is GroupShape group ? group.Children.Select(child => child.Id) : [shape.Id]).ToList();
        _state.Edit(HistoryStepType.UngroupShapes, document => document.Ungroup(groups));
        _state.SetSelection(after);
    }

    /// <summary>Moves the selected shapes in the stacking order of their layer or group. The selection stays.</summary>
    public void Arrange(ArrangeType type)
    {
        var ids = _state.SelectedShapeIds.ToHashSet();
        _state.Edit(StepOf(type), document => document.Arrange(ids, type));
    }

    /// <summary>Puts a shape directly above another one that has the same layer and group, as one step. Does nothing if that is not possible.</summary>
    public void MoveAbove(Guid id, Guid targetId)
    {
        if (CanMoveAbove(id, targetId))
        {
            _state.Edit(HistoryStepType.ReorderShape, document => document.MoveAbove(id, targetId));
        }
    }

    /// <summary>Locks the selected shapes. They leave the selection, because locked shapes cannot be selected.</summary>
    public void LockSelection() => SetLocked(_state.SelectedShapeIds, locked: true);

    /// <summary>Hides the selected shapes. They leave the selection, because hidden shapes cannot be selected.</summary>
    public void HideSelection() => SetHidden(_state.SelectedShapeIds, hidden: true);

    /// <summary>Locks or unlocks the shapes with these ids, as one step. Unknown ids are ignored.</summary>
    public void SetLocked(IEnumerable<Guid> ids, bool locked)
    {
        var existing = Existing(ids);
        _state.Edit(locked ? HistoryStepType.LockShapes : HistoryStepType.UnlockShapes, document => document.SetLocked(existing, locked));
    }

    /// <summary>Hides or shows the shapes with these ids, as one step. Unknown ids are ignored.</summary>
    public void SetHidden(IEnumerable<Guid> ids, bool hidden)
    {
        var existing = Existing(ids);
        _state.Edit(hidden ? HistoryStepType.HideShapes : HistoryStepType.ShowShapes, document => document.SetHidden(existing, hidden));
    }

    /// <summary>Shows every shape and group of the document, nested ones included, as one step.</summary>
    public void ShowAll() => _state.Edit(HistoryStepType.ShowAll, document => document.ShowAll());

    /// <summary>Unlocks every shape and group of the document, nested ones included, as one step.</summary>
    public void UnlockAll() => _state.Edit(HistoryStepType.UnlockAll, document => document.UnlockAll());

    /// <summary>Gives a shape or group a name, as one step. An empty or blank name removes the name. Does nothing for an unknown id.</summary>
    public void Rename(Guid id, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_state.Document.FindShape(id) is not null)
        {
            _state.Edit(HistoryStepType.RenameShape, document => document.Rename(id, name));
        }
    }

    private IReadOnlyList<Shape> SelectedShapes() => SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds);

    private IReadOnlySet<Guid> Existing(IEnumerable<Guid> ids)
    {
        var wanted = ids.ToHashSet();
        return SelectionQueries.SelectedShapes(_state.Document, wanted).Select(shape => shape.Id).ToHashSet();
    }

    private static HistoryStepType StepOf(ArrangeType type) => type switch
    {
        ArrangeType.ToFront => HistoryStepType.BringToFront,
        ArrangeType.Forward => HistoryStepType.BringForward,
        ArrangeType.Backward => HistoryStepType.SendBackward,
        ArrangeType.ToBack => HistoryStepType.SendToBack,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
