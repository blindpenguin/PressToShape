using System;
using System.Collections.Generic;
using System.ComponentModel;
using Material.Icons;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// The toolbar groups from the mockup, left to right: file actions, history, tool toggles,
/// horizontal alignment, transform fields, transform buttons, vertical alignment, other actions.
/// All buttons are disabled for now. The Select and Node toggles are wired to the active tool.
/// </summary>
internal sealed class ToolbarViewModel : ViewModelBase
{
    private readonly EditorState _state;

    public ToolbarViewModel(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _state.PropertyChanged += OnStateChanged;
    }

    public IReadOnlyList<ToolbarButtonViewModel> FileActions { get; } =
    [
        new("New", MaterialIconKind.FileOutline),
        new("Open", MaterialIconKind.FolderOpenOutline),
        new("Save", MaterialIconKind.ContentSaveOutline),
    ];

    public IReadOnlyList<ToolbarButtonViewModel> HistoryActions { get; } =
    [
        new("Undo", MaterialIconKind.Undo),
        new("Redo", MaterialIconKind.Redo),
    ];

    public IReadOnlyList<ToolbarButtonViewModel> HorizontalAlignActions { get; } =
    [
        new("Align Left", MaterialIconKind.AlignHorizontalLeft),
        new("Align Center", MaterialIconKind.AlignHorizontalCenter),
        new("Align Right", MaterialIconKind.AlignHorizontalRight),
    ];

    public IReadOnlyList<ToolbarButtonViewModel> TransformActions { get; } =
    [
        new("Rotate 90°", MaterialIconKind.RotateRight),
        new("Flip Horizontal", MaterialIconKind.FlipHorizontal),
    ];

    public IReadOnlyList<ToolbarButtonViewModel> VerticalAlignActions { get; } =
    [
        new("Align Top", MaterialIconKind.AlignVerticalTop),
        new("Align Middle", MaterialIconKind.AlignVerticalCenter),
        new("Align Bottom", MaterialIconKind.AlignVerticalBottom),
    ];

    public IReadOnlyList<ToolbarButtonViewModel> OtherActions { get; } =
    [
        new("Distribute", MaterialIconKind.DistributeHorizontalCenter),
        new("Export", MaterialIconKind.ExportVariant),
    ];

    /// <summary>Two-way for the Select toggle. Unchecking has no effect, because one tool is always active.</summary>
    public bool IsSelectToolActive
    {
        get => _state.ActiveTool == Tool.Select;
        set => SetTool(Tool.Select, value, nameof(IsSelectToolActive));
    }

    /// <summary>Two-way for the Node toggle. Unchecking has no effect, because one tool is always active.</summary>
    public bool IsNodeToolActive
    {
        get => _state.ActiveTool == Tool.Node;
        set => SetTool(Tool.Node, value, nameof(IsNodeToolActive));
    }

    public bool HasSelection => _state.HasSelection;

    // Transform fields: empty and disabled without a selection. Values arrive with the document spec.
    public string X => string.Empty;

    public string Y => string.Empty;

    public string Width => string.Empty;

    public string Height => string.Empty;

    private void SetTool(Tool tool, bool isChecked, string propertyName)
    {
        if (isChecked)
        {
            _state.ActiveTool = tool;
        }
        else
        {
            // Keep the toggle checked if it is still the active tool.
            OnPropertyChanged(propertyName);
        }
    }

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.ActiveTool):
                OnPropertyChanged(nameof(IsSelectToolActive));
                OnPropertyChanged(nameof(IsNodeToolActive));
                break;
            case nameof(EditorState.HasSelection):
                OnPropertyChanged(nameof(HasSelection));
                break;
        }
    }
}
