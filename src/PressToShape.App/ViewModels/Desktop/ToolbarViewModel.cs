using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// The toolbar groups from the mockup, left to right: file actions, history, tool toggles,
/// horizontal alignment, transform fields, transform buttons, vertical alignment, other actions.
/// Undo and Redo work, the other buttons are disabled for now. The Select and Node toggles are wired to the active tool.
/// </summary>
internal sealed class ToolbarViewModel : ViewModelBase
{
    private readonly EditorState _state;

    public ToolbarViewModel(EditorState state, ILocalizer localizer, ICommand undo, ICommand redo)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(undo);
        ArgumentNullException.ThrowIfNull(redo);
        _state = state ?? throw new ArgumentNullException(nameof(state));

        FileActions =
        [
            Button(localizer, N("New"), MaterialIconKind.FileOutline),
            Button(localizer, N("Open"), MaterialIconKind.FolderOpenOutline),
            Button(localizer, N("Save"), MaterialIconKind.ContentSaveOutline),
        ];
        HistoryActions =
        [
            Button(localizer, N("Undo"), MaterialIconKind.Undo, undo),
            Button(localizer, N("Redo"), MaterialIconKind.Redo, redo),
        ];
        HorizontalAlignActions =
        [
            Button(localizer, N("Align Left"), MaterialIconKind.AlignHorizontalLeft),
            Button(localizer, N("Align Center"), MaterialIconKind.AlignHorizontalCenter),
            Button(localizer, N("Align Right"), MaterialIconKind.AlignHorizontalRight),
        ];
        TransformActions =
        [
            Button(localizer, N("Rotate 90°"), MaterialIconKind.RotateRight),
            Button(localizer, N("Flip Horizontal"), MaterialIconKind.FlipHorizontal),
        ];
        VerticalAlignActions =
        [
            Button(localizer, N("Align Top"), MaterialIconKind.AlignVerticalTop),
            Button(localizer, N("Align Middle"), MaterialIconKind.AlignVerticalCenter),
            Button(localizer, N("Align Bottom"), MaterialIconKind.AlignVerticalBottom),
        ];
        OtherActions =
        [
            Button(localizer, N("Distribute"), MaterialIconKind.DistributeHorizontalCenter),
            Button(localizer, N("Export"), MaterialIconKind.ExportVariant),
        ];

        _state.PropertyChanged += OnStateChanged;
    }

    public IReadOnlyList<ToolbarButtonViewModel> FileActions { get; }

    public IReadOnlyList<ToolbarButtonViewModel> HistoryActions { get; }

    public IReadOnlyList<ToolbarButtonViewModel> HorizontalAlignActions { get; }

    public IReadOnlyList<ToolbarButtonViewModel> TransformActions { get; }

    public IReadOnlyList<ToolbarButtonViewModel> VerticalAlignActions { get; }

    public IReadOnlyList<ToolbarButtonViewModel> OtherActions { get; }

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

    private static ToolbarButtonViewModel Button(ILocalizer localizer, string name, MaterialIconKind icon, ICommand? command = null) =>
        new(localizer, name, icon, command);

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
