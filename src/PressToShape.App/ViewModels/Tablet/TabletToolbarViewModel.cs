using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// The compact toolbar of the tablet layout: the name of the document (with a star while it has unsaved changes), undo and redo,
/// Duplicate and Delete, the icons that open the panel, the zoom, and the overflow menu. Everything else of the desktop toolbar and
/// menu bar is in the overflow menu.
/// </summary>
internal sealed class TabletToolbarViewModel : ViewModelBase
{
    private readonly EditorShellViewModel _shell;

    public TabletToolbarViewModel(
        ILocalizer localizer,
        EditorShellViewModel shell,
        ICommand undo,
        ICommand redo,
        OverflowMenuViewModel overflowMenu,
        TabletPanelViewModel panel)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(undo);
        ArgumentNullException.ThrowIfNull(redo);
        _shell = shell ?? throw new ArgumentNullException(nameof(shell));
        OverflowMenu = overflowMenu ?? throw new ArgumentNullException(nameof(overflowMenu));
        ArgumentNullException.ThrowIfNull(panel);

        UndoButton = new ToolbarButtonViewModel(localizer, N("Undo"), MaterialIconKind.Undo, undo);
        RedoButton = new ToolbarButtonViewModel(localizer, N("Redo"), MaterialIconKind.Redo, redo);
        DuplicateButton = new ToolbarButtonViewModel(localizer, N("Duplicate"), MaterialIconKind.ContentCopy, shell.DuplicateCommand);
        DeleteButton = new ToolbarButtonViewModel(localizer, N("Delete"), MaterialIconKind.TrashCanOutline, shell.DeleteCommand);
        PanelButtons =
        [
            new PanelButtonViewModel(localizer, InspectorTab.Properties, N("Properties"), MaterialIconKind.TuneVariant, panel),
            new PanelButtonViewModel(localizer, InspectorTab.Layers, N("Layers"), MaterialIconKind.LayersOutline, panel),
            new PanelButtonViewModel(localizer, InspectorTab.History, N("History"), MaterialIconKind.History, panel),
        ];

        // The toolbar lives as long as the shell, so these subscriptions are never removed.
        _shell.PropertyChanged += OnShellChanged;
    }

    /// <summary>The document name, with "* " in front while the document has unsaved changes.</summary>
    public string DocumentTitle => (_shell.IsDirty ? "* " : string.Empty) + _shell.DocumentName;

    public ToolbarButtonViewModel UndoButton { get; }

    public ToolbarButtonViewModel RedoButton { get; }

    /// <summary>Duplicates the selection. Enabled only while something is selected.</summary>
    public ToolbarButtonViewModel DuplicateButton { get; }

    /// <summary>Deletes the selection (one undo step). Enabled only while something is selected.</summary>
    public ToolbarButtonViewModel DeleteButton { get; }

    /// <summary>The icons that open the panel: Properties, Layers, History.</summary>
    public IReadOnlyList<PanelButtonViewModel> PanelButtons { get; }

    public OverflowMenuViewModel OverflowMenu { get; }

    private void OnShellChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EditorShellViewModel.DocumentName) or nameof(EditorShellViewModel.IsDirty) or null or "")
        {
            OnPropertyChanged(nameof(DocumentTitle));
        }
    }
}
