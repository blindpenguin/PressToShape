using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// What every layout shell needs. Creates the shared <see cref="EditorState"/> and the view model of every region that the layouts
/// share (tool column, canvas, inspector), and owns the file, edit, undo and redo commands, so the regions of a layout always agree.
/// The desktop and the tablet layout derive from it and add their own regions.
/// </summary>
internal abstract partial class EditorShellViewModel : ViewModelBase
{
    private readonly ILocalizer _localizer;
    private readonly DocumentSession _session;
    private readonly DocumentFileService _files;
    private bool _initialFitDone;

    protected EditorShellViewModel(ILocalizer localizer, FileServices services)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        ArgumentNullException.ThrowIfNull(services);

        State = new EditorState(Document.Create(localizer.T("Layer 1")));
        _session = new DocumentSession(State);
        Recent = new RecentFiles(services.Settings, services.RemembersRecentFiles);
        _files = new DocumentFileService(State, _session, Recent, services.Files, services.Dialogs, services.Messages, localizer, FitToWindow);
        Toolbox = new ToolboxViewModel(State, localizer);
        Editor = new SelectionEditor(State);
        Canvas = new CanvasAreaViewModel(State, Editor);
        Inspector = new InspectorViewModel(State, localizer, Editor);
        FileCommands = new FileMenuCommands(
            NewCommand,
            OpenCommand,
            SaveCommand,
            SaveAsCommand,
            ExportSvgCommand,
            path => new AsyncRelayCommand(() => _files.OpenRecentAsync(path)));
        EditCommands = new EditMenuCommands(
            CutCommand,
            CopyCommand,
            PasteCommand,
            PasteInPlaceCommand,
            DuplicateCommand,
            DeleteCommand,
            SelectAllCommand,
            DeselectCommand);

        Canvas.PropertyChanged += OnCanvasPropertyChanged;
        _session.PropertyChanged += (_, _) => NotifyDocumentNameChanged();
        State.PropertyChanged += OnStateHistoryAvailabilityChanged;
        State.Clipboard.Changed += (_, _) => NotifyPasteCommands();
        localizer.LanguageChanged += (_, _) => NotifyDocumentNameChanged();
    }

    public EditorState State { get; }

    public ToolboxViewModel Toolbox { get; }

    /// <summary>The selection edits (copy, paste, nudge, property changes). One instance, shared by the canvas, the inspector and the menus.</summary>
    public SelectionEditor Editor { get; }

    public CanvasAreaViewModel Canvas { get; }

    public InspectorViewModel Inspector { get; }

    /// <summary>The file name of the document, or the translated word for "Untitled" while it has none.</summary>
    public string DocumentName => _session.Path is null ? _localizer.T("Untitled") : _session.DisplayName;

    /// <summary>Whether the document has changes that were not saved.</summary>
    public bool IsDirty => _session.IsDirty;

    /// <summary>The window title: an asterisk while there are unsaved changes, the file name (or "Untitled") and the product name.</summary>
    public string WindowTitle => $"{(IsDirty ? "* " : string.Empty)}{DocumentName} — PressToShape";

    /// <summary>The recently used files, for layouts that list them.</summary>
    protected RecentFiles Recent { get; }

    /// <summary>The commands behind the file items of a menu.</summary>
    protected FileMenuCommands FileCommands { get; }

    /// <summary>The commands behind the edit items of a menu.</summary>
    protected EditMenuCommands EditCommands { get; }

    /// <summary>Asks about unsaved changes before the window closes. Returns whether closing may go on.</summary>
    public Task<bool> ConfirmCloseAsync() => _files.ConfirmDiscardAsync();

    /// <summary>Zooms so the artboard fits the canvas area. Does nothing until the canvas has a size.</summary>
    public void FitToWindow() => State.ZoomToFit(Canvas.ViewportWidth, Canvas.ViewportHeight);

    private void NotifyDocumentNameChanged()
    {
        OnPropertyChanged(nameof(DocumentName));
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(WindowTitle));
    }

    /// <summary>Fits the artboard once, as soon as the canvas area reports a real size, so the app opens at fit-to-window.</summary>
    private void OnCanvasPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_initialFitDone
            || (e.PropertyName != nameof(CanvasAreaViewModel.ViewportWidth) && e.PropertyName != nameof(CanvasAreaViewModel.ViewportHeight))
            || Canvas.ViewportWidth <= 0
            || Canvas.ViewportHeight <= 0)
        {
            return;
        }

        _initialFitDone = true;
        FitToWindow();
    }

    // File actions. The menu items, the keyboard shortcuts and the toolbar all use these commands. A command does not run twice at once.
    [RelayCommand]
    private Task NewAsync() => _files.NewAsync();

    [RelayCommand]
    private Task OpenAsync() => _files.OpenAsync();

    [RelayCommand]
    private async Task SaveAsync() => await _files.SaveAsync();

    [RelayCommand]
    private async Task SaveAsAsync() => await _files.SaveAsAsync();

    [RelayCommand]
    private Task ExportSvgAsync() => _files.ExportSvgAsync();

    // Undo and redo. The menu items, the toolbar buttons and the keyboard shortcuts all use these commands.
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo() => State.Undo();

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo() => State.Redo();

    private bool CanUndo() => State.CanUndo;

    private bool CanRedo() => State.CanRedo;

    private void OnStateHistoryAvailabilityChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.CanUndo):
                UndoCommand.NotifyCanExecuteChanged();
                break;
            case nameof(EditorState.CanRedo):
                RedoCommand.NotifyCanExecuteChanged();
                break;
            case nameof(EditorState.HasSelection):
                CutCommand.NotifyCanExecuteChanged();
                CopyCommand.NotifyCanExecuteChanged();
                DuplicateCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
                DeselectCommand.NotifyCanExecuteChanged();
                break;
            case nameof(EditorState.Document):
                // A layer may have been locked or hidden, and shapes may have come or gone.
                NotifyPasteCommands();
                SelectAllCommand.NotifyCanExecuteChanged();
                break;
            case nameof(EditorState.ActiveLayerId):
                NotifyPasteCommands();
                break;
        }
    }

    private void NotifyPasteCommands()
    {
        PasteCommand.NotifyCanExecuteChanged();
        PasteInPlaceCommand.NotifyCanExecuteChanged();
    }

    // Selection and clipboard actions. The Edit menu items and the keyboard shortcuts use these commands.
    [RelayCommand(CanExecute = nameof(CanCut))]
    private void Cut() => Editor.Cut();

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void Copy() => Editor.Copy();

    [RelayCommand(CanExecute = nameof(CanPaste))]
    private void Paste() => Editor.Paste(inPlace: false);

    [RelayCommand(CanExecute = nameof(CanPaste))]
    private void PasteInPlace() => Editor.Paste(inPlace: true);

    [RelayCommand(CanExecute = nameof(CanDuplicate))]
    private void Duplicate() => Editor.Duplicate();

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Delete() => Canvas.DeleteSelection();

    [RelayCommand(CanExecute = nameof(HasSelectableShapes))]
    private void SelectAll() => Editor.SelectAll();

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Deselect() => Editor.Deselect();

    private bool CanCut() => Editor.CanCut;

    private bool CanCopy() => Editor.CanCopy;

    private bool CanPaste() => Editor.CanPaste;

    private bool CanDuplicate() => Editor.CanDuplicate;

    private bool HasSelection() => State.HasSelection;

    private bool HasSelectableShapes() => SelectionQueries.Selectable(State.Document).Count > 0;
}
