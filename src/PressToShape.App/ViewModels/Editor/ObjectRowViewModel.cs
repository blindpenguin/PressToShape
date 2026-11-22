using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// One row of the object list: a layer header, a shape or a group. The values are display values that <see cref="LayersViewModel"/> copies
/// from the document. The commands ask the list to change the document, they never change these values themselves.
/// </summary>
internal sealed partial class ObjectRowViewModel : ViewModelBase
{
    private readonly LayersViewModel _owner;

    public ObjectRowViewModel(LayersViewModel owner, Guid id, ObjectRowType type, int depth, Guid parentId)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Id = id;
        Type = type;
        Depth = depth;
        ParentId = parentId;
    }

    /// <summary>The id of the layer, shape or group.</summary>
    public Guid Id { get; }

    /// <summary>What the row stands for.</summary>
    public ObjectRowType Type { get; }

    /// <summary>How deep the row is: 0 for a layer, 1 for what is on the top level of a layer, 2 below a group, and so on.</summary>
    public int Depth { get; }

    /// <summary>The id of the layer or group that holds this row. The layer's own row has its own id here.</summary>
    public Guid ParentId { get; }

    /// <summary>Whether the row is a layer header.</summary>
    public bool IsLayer => Type == ObjectRowType.Layer;

    /// <summary>Whether the row is a group, so it can be opened and closed.</summary>
    public bool IsGroup => Type == ObjectRowType.Group;

    /// <summary>Whether the row is a rectangle.</summary>
    public bool IsRectangle => Type == ObjectRowType.Rectangle;

    /// <summary>Whether the row is an ellipse.</summary>
    public bool IsEllipse => Type == ObjectRowType.Ellipse;

    /// <summary>Whether the row is a path.</summary>
    public bool IsPath => Type == ObjectRowType.Path;

    /// <summary>Whether the row is a text.</summary>
    public bool IsText => Type == ObjectRowType.Text;

    /// <summary>Whether the row has an expander: layers and groups.</summary>
    public bool HasChildren => IsLayer || IsGroup;

    /// <summary>Whether the row can be renamed in the list. Every row can: layers, groups and shapes.</summary>
    public bool CanRename => true;

    /// <summary>The name that is shown: the name the user gave, or the translated name of the type.</summary>
    [ObservableProperty] public partial string Name { get; private set; } = string.Empty;

    /// <summary>The name the user gave, or an empty text. This is what the rename field starts with.</summary>
    [ObservableProperty] public partial string StoredName { get; private set; } = string.Empty;

    /// <summary>Whether the row's own hide flag is not set.</summary>
    [ObservableProperty] public partial bool IsVisible { get; private set; } = true;

    /// <summary>Whether the row's own lock flag is set.</summary>
    [ObservableProperty] public partial bool IsLocked { get; private set; }

    /// <summary>Whether the row is hidden through itself, a group around it or its layer, so it is not drawn.</summary>
    [ObservableProperty] public partial bool IsEffectivelyHidden { get; private set; }

    /// <summary>Whether the row is locked through itself, a group around it or its layer, so it cannot be selected on the canvas.</summary>
    [ObservableProperty] public partial bool IsEffectivelyLocked { get; private set; }

    /// <summary>Whether the layer is the active one, the layer new shapes go into. Only layer rows are ever active.</summary>
    [ObservableProperty] public partial bool IsActiveLayer { get; internal set; }

    /// <summary>Whether the object is part of the selection on the canvas.</summary>
    [ObservableProperty] public partial bool IsSelected { get; internal set; }

    /// <summary>Whether the row was the last one clicked in the list. A locked row cannot be selected on the canvas, so this shows it.</summary>
    [ObservableProperty] public partial bool IsCurrent { get; internal set; }

    /// <summary>Whether the rows below this one are shown. Only for rows with <see cref="HasChildren"/>.</summary>
    [ObservableProperty] public partial bool IsExpanded { get; internal set; }

    /// <summary>Whether a dragged row would land above this row if it was dropped now.</summary>
    [ObservableProperty] public partial bool IsDropTarget { get; internal set; }

    /// <summary>Whether the name is being edited.</summary>
    [ObservableProperty] public partial bool IsRenaming { get; private set; }

    /// <summary>The text of the rename field while <see cref="IsRenaming"/>.</summary>
    [ObservableProperty] public partial string EditText { get; set; } = string.Empty;

    /// <summary>Copies the display values. Called when the document changed.</summary>
    public void Update(string name, string storedName, bool isVisible, bool isLocked, bool effectivelyHidden, bool effectivelyLocked)
    {
        Name = name;
        StoredName = storedName;
        IsVisible = isVisible;
        IsLocked = isLocked;
        IsEffectivelyHidden = effectivelyHidden;
        IsEffectivelyLocked = effectivelyLocked;
    }

    /// <summary>Starts editing the name.</summary>
    [RelayCommand(CanExecute = nameof(CanRename))]
    public void BeginRename()
    {
        EditText = StoredName;
        IsRenaming = true;
    }

    /// <summary>Gives the object or layer the typed name as one step and ends the editing. An empty name removes the name of an object, and a layer keeps its name.</summary>
    [RelayCommand]
    public void CommitRename()
    {
        if (!IsRenaming)
        {
            return;
        }

        IsRenaming = false;
        _owner.Rename(this, EditText);
    }

    /// <summary>Ends the editing and keeps the old name.</summary>
    [RelayCommand]
    public void CancelRename() => IsRenaming = false;

    [RelayCommand]
    private void ToggleVisibility() => _owner.SetHidden(this, hidden: IsVisible);

    [RelayCommand]
    private void ToggleLock() => _owner.SetLocked(this, locked: !IsLocked);

    [RelayCommand]
    private void ToggleExpanded() => _owner.SetExpanded(this, !IsExpanded);
}
