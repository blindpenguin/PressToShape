using System;
using System.Collections.ObjectModel;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// State of the History tab: the start state and one row per step, oldest first. The selected row is the current state,
/// and selecting another row jumps there. The list is rebuilt only when the sequence of steps changed,
/// otherwise the rows are updated in place, so undo and redo do not disturb the list under the user's click.
/// </summary>
internal sealed class HistoryViewModel : ViewModelBase
{
    private readonly EditorState _state;
    private readonly ILocalizer _localizer;
    private HistoryItemViewModel? _selectedItem;
    private bool _updatingSelection;

    public HistoryViewModel(EditorState state, ILocalizer localizer)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _state.HistoryChanged += (_, _) => Sync();
        Sync();
    }

    /// <summary>The rows, oldest first. The first row is the start state.</summary>
    public ObservableCollection<HistoryItemViewModel> Items { get; } = [];

    /// <summary>
    /// The row of the current state. Setting another row jumps to its state, which may be refused (while a drag is in progress),
    /// and the selection then goes back to the current row. <c>null</c> is ignored: one row is always current.
    /// </summary>
    public HistoryItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (value is null || _updatingSelection || ReferenceEquals(value, _selectedItem))
            {
                return;
            }

            _state.JumpToStep(value.Index);
            ShowCurrent(force: true);
        }
    }

    private void Sync()
    {
        var steps = _state.History.Steps;
        var sameSteps = Items.Count == steps.Count + 1
            && Enumerable.Range(0, steps.Count).All(i => Items[i + 1].Type == steps[i]);

        if (!sameSteps)
        {
            Rebuild();
        }

        var current = _state.History.CurrentIndex;
        foreach (var item in Items)
        {
            item.IsCurrent = item.Index == current;
            item.IsRedoable = item.Index > current;
        }

        ShowCurrent(force: !sameSteps);
    }

    private void Rebuild()
    {
        // The list control sends null while its items are replaced, which the selection setter ignores.
        _updatingSelection = true;
        try
        {
            Items.Clear();
            Items.Add(new HistoryItemViewModel(_localizer, 0, null, HistoryStepNames.Start));
            var steps = _state.History.Steps;
            for (var i = 0; i < steps.Count; i++)
            {
                Items.Add(new HistoryItemViewModel(_localizer, i + 1, steps[i], HistoryStepNames.MessageId(steps[i])));
            }
        }
        finally
        {
            _updatingSelection = false;
        }
    }

    // Makes the selected row the current one. With force the list control is told even if the row object is the same.
    private void ShowCurrent(bool force)
    {
        var current = Items[_state.History.CurrentIndex];
        if (!force && ReferenceEquals(current, _selectedItem))
        {
            return;
        }

        _updatingSelection = true;
        try
        {
            _selectedItem = current;
            OnPropertyChanged(nameof(SelectedItem));
        }
        finally
        {
            _updatingSelection = false;
        }
    }
}
