using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.PathOperations;

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
        Objects = new ObjectEditor(State);
        PathEditing = new PathEditor(State, services.Messages, localizer);
        LayerEditing = new LayerEditor(State, localizer);
        var ring = new RingMenuViewModel(localizer);
        Canvas = new CanvasAreaViewModel(State, Editor, ring: ring);
        Inspector = new InspectorViewModel(State, localizer, Editor, Objects, LayerEditing);
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

        ObjectCommands = new ObjectMenuCommands(
            GroupCommand,
            UngroupCommand,
            BringToFrontCommand,
            BringForwardCommand,
            SendBackwardCommand,
            SendToBackCommand,
            LockCommand,
            UnlockAllCommand,
            HideCommand,
            ShowAllCommand);

        PathCommands = new PathMenuCommands(
            UnionShapesCommand,
            DifferenceShapesCommand,
            IntersectShapesCommand,
            ExcludeShapesCommand,
            CombinePathsCommand,
            BreakApartPathCommand);

        LayerCommands = new LayerMenuCommands(LayerEditing);

        ring.Content = RingMenuContent.Create(ObjectCommands, EditCommands);

        PathEditing.BusyChanged += (_, _) => NotifyObjectCommands();
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

    /// <summary>The edits of the structure: group, arrange, lock, hide, rename. One instance, shared by the menus and the object list.</summary>
    public ObjectEditor Objects { get; }

    /// <summary>The path operations (boolean operations, Combine, Break Apart) on the selection. One instance, shared by the menus.</summary>
    public PathEditor PathEditing { get; }

    /// <summary>The editor for the layers. The Layers panel and the Layer menu share it.</summary>
    public LayerEditor LayerEditing { get; }

    /// <summary>The commands of the boolean, Combine and Break Apart items of the Path menu.</summary>
    public PathMenuCommands PathCommands { get; }

    /// <summary>The commands of the Layer menu.</summary>
    public LayerMenuCommands LayerCommands { get; }

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

    /// <summary>The commands behind the group, arrange, lock and hide items of a menu.</summary>
    public ObjectMenuCommands ObjectCommands { get; }

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
                NotifyObjectCommands();
                LayerCommands.NotifyCanExecuteChanged();
                break;
            case nameof(EditorState.SelectedShapeIds):
            case nameof(EditorState.IsDragInProgress):
                NotifyObjectCommands();
                LayerCommands.NotifyCanExecuteChanged();
                break;
            case nameof(EditorState.ActiveLayerId):
                NotifyPasteCommands();
                LayerCommands.NotifyCanExecuteChanged();
                break;
        }
    }

    private void NotifyObjectCommands()
    {
        GroupCommand.NotifyCanExecuteChanged();
        UngroupCommand.NotifyCanExecuteChanged();
        BringToFrontCommand.NotifyCanExecuteChanged();
        BringForwardCommand.NotifyCanExecuteChanged();
        SendBackwardCommand.NotifyCanExecuteChanged();
        SendToBackCommand.NotifyCanExecuteChanged();
        LockCommand.NotifyCanExecuteChanged();
        UnlockAllCommand.NotifyCanExecuteChanged();
        HideCommand.NotifyCanExecuteChanged();
        ShowAllCommand.NotifyCanExecuteChanged();
        UnionShapesCommand.NotifyCanExecuteChanged();
        DifferenceShapesCommand.NotifyCanExecuteChanged();
        IntersectShapesCommand.NotifyCanExecuteChanged();
        ExcludeShapesCommand.NotifyCanExecuteChanged();
        CombinePathsCommand.NotifyCanExecuteChanged();
        BreakApartPathCommand.NotifyCanExecuteChanged();
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

    // Group, arrange, lock and hide. The Object menu items, the keyboard shortcuts and the tablet menu use these commands.
    [RelayCommand(CanExecute = nameof(CanGroup))]
    private void Group() => Objects.Group();

    [RelayCommand(CanExecute = nameof(CanUngroup))]
    private void Ungroup() => Objects.Ungroup();

    [RelayCommand(CanExecute = nameof(CanBringToFront))]
    private void BringToFront() => Objects.Arrange(ArrangeType.ToFront);

    [RelayCommand(CanExecute = nameof(CanBringForward))]
    private void BringForward() => Objects.Arrange(ArrangeType.Forward);

    [RelayCommand(CanExecute = nameof(CanSendBackward))]
    private void SendBackward() => Objects.Arrange(ArrangeType.Backward);

    [RelayCommand(CanExecute = nameof(CanSendToBack))]
    private void SendToBack() => Objects.Arrange(ArrangeType.ToBack);

    [RelayCommand(CanExecute = nameof(CanLock))]
    private void Lock() => Objects.LockSelection();

    [RelayCommand(CanExecute = nameof(CanUnlockAll))]
    private void UnlockAll() => Objects.UnlockAll();

    [RelayCommand(CanExecute = nameof(CanHide))]
    private void Hide() => Objects.HideSelection();

    [RelayCommand(CanExecute = nameof(CanShowAll))]
    private void ShowAll() => Objects.ShowAll();

    // The boolean operations, Combine and Break Apart. The Path menu items use these commands.
    [RelayCommand(CanExecute = nameof(CanBoolean))]
    private Task UnionShapes() => PathEditing.BooleanAsync(PathOperationType.Union);

    [RelayCommand(CanExecute = nameof(CanBoolean))]
    private Task DifferenceShapes() => PathEditing.BooleanAsync(PathOperationType.Difference);

    [RelayCommand(CanExecute = nameof(CanBoolean))]
    private Task IntersectShapes() => PathEditing.BooleanAsync(PathOperationType.Intersection);

    [RelayCommand(CanExecute = nameof(CanBoolean))]
    private Task ExcludeShapes() => PathEditing.BooleanAsync(PathOperationType.Exclusion);

    [RelayCommand(CanExecute = nameof(CanCombine))]
    private Task CombinePaths() => PathEditing.CombineAsync();

    [RelayCommand(CanExecute = nameof(CanBreakApart))]
    private Task BreakApartPath() => PathEditing.BreakApartAsync();

    private bool CanBoolean() => PathEditing.CanBoolean;

    private bool CanCombine() => PathEditing.CanCombine;

    private bool CanBreakApart() => PathEditing.CanBreakApart;

    private bool CanGroup() => Objects.CanGroup;

    private bool CanUngroup() => Objects.CanUngroup;

    private bool CanBringToFront() => Objects.CanArrange(ArrangeType.ToFront);

    private bool CanBringForward() => Objects.CanArrange(ArrangeType.Forward);

    private bool CanSendBackward() => Objects.CanArrange(ArrangeType.Backward);

    private bool CanSendToBack() => Objects.CanArrange(ArrangeType.ToBack);

    private bool CanLock() => Objects.CanLockSelection;

    private bool CanUnlockAll() => Objects.CanUnlockAll;

    private bool CanHide() => Objects.CanHideSelection;

    private bool CanShowAll() => Objects.CanShowAll;
}
