using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// What the items of the Layer menu do. Each command runs a <see cref="LayerEditor"/> command and is disabled while that would change
/// nothing. Call <see cref="NotifyCanExecuteChanged"/> when the document, the active layer or the selection changed.
/// </summary>
internal sealed class LayerMenuCommands
{
    private readonly RelayCommand[] _all;

    public LayerMenuCommands(LayerEditor layers)
    {
        ArgumentNullException.ThrowIfNull(layers);

        var add = new RelayCommand(layers.Add, () => layers.CanAdd);
        var duplicate = new RelayCommand(layers.Duplicate, () => layers.CanDuplicate);
        var delete = new RelayCommand(layers.Delete, () => layers.CanDelete);
        var mergeDown = new RelayCommand(layers.MergeDown, () => layers.CanMerge);
        var moveUp = new RelayCommand(layers.MoveUp, () => layers.CanMoveUp);
        var moveDown = new RelayCommand(layers.MoveDown, () => layers.CanMoveDown);
        var moveSelectionUp = new RelayCommand(layers.MoveSelectionUp, () => layers.CanMoveSelectionUp);
        var moveSelectionDown = new RelayCommand(layers.MoveSelectionDown, () => layers.CanMoveSelectionDown);
        _all = [add, duplicate, delete, mergeDown, moveUp, moveDown, moveSelectionUp, moveSelectionDown];

        Add = add;
        Duplicate = duplicate;
        Delete = delete;
        MergeDown = mergeDown;
        MoveUp = moveUp;
        MoveDown = moveDown;
        MoveSelectionUp = moveSelectionUp;
        MoveSelectionDown = moveSelectionDown;
    }

    /// <summary>Adds an empty layer above the active layer.</summary>
    public ICommand Add { get; }

    /// <summary>Copies the active layer.</summary>
    public ICommand Duplicate { get; }

    /// <summary>Deletes the active layer. Disabled for the last layer.</summary>
    public ICommand Delete { get; }

    /// <summary>Merges the active layer into the one below.</summary>
    public ICommand MergeDown { get; }

    /// <summary>Moves the active layer up.</summary>
    public ICommand MoveUp { get; }

    /// <summary>Moves the active layer down.</summary>
    public ICommand MoveDown { get; }

    /// <summary>Moves the selected objects to the layer above.</summary>
    public ICommand MoveSelectionUp { get; }

    /// <summary>Moves the selected objects to the layer below.</summary>
    public ICommand MoveSelectionDown { get; }

    /// <summary>Asks every command to check again whether it can run.</summary>
    public void NotifyCanExecuteChanged()
    {
        foreach (var command in _all)
        {
            command.NotifyCanExecuteChanged();
        }
    }
}
