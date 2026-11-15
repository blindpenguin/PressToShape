using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// Root of the desktop layout. Creates the shared <see cref="EditorState"/> and the view model of every region,
/// so toolbar, tool column, menu, canvas, inspector and status bar always agree.
/// </summary>
internal sealed partial class DesktopShellViewModel : ViewModelBase
{
    private readonly IAppShutdown _shutdown;
    private readonly ILocalizer _localizer;
    private readonly DocumentSession _session;
    private readonly DocumentFileService _files;
    private bool _initialFitDone;

    public DesktopShellViewModel(IAppShutdown shutdown, ILocalizer localizer, FileServices services)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _shutdown = shutdown ?? throw new ArgumentNullException(nameof(shutdown));
        ArgumentNullException.ThrowIfNull(services);

        State = new EditorState(Document.Create(localizer.T("Layer 1")));
        _session = new DocumentSession(State);
        var recent = new RecentFiles(services.Settings);
        _files = new DocumentFileService(State, _session, recent, services.Files, services.Dialogs, services.Messages, localizer, FitToWindow);
        Toolbox = new ToolboxViewModel(State, localizer);
        Canvas = new CanvasAreaViewModel(State);
        Inspector = new InspectorViewModel(State, localizer);
        Toolbar = new ToolbarViewModel(State, localizer);
        StatusBar = new StatusBarViewModel(State, localizer, FitToWindow);
        var fileCommands = new FileMenuCommands(
            NewCommand,
            OpenCommand,
            SaveCommand,
            SaveAsCommand,
            ExportSvgCommand,
            path => new AsyncRelayCommand(() => _files.OpenRecentAsync(path)));
        MenuBar = new MenuBarViewModel(State, shutdown, localizer, Inspector.ShowTab, FitToWindow, fileCommands, recent);

        Canvas.PropertyChanged += OnCanvasPropertyChanged;
        _session.PropertyChanged += (_, _) => OnPropertyChanged(nameof(WindowTitle));
        localizer.LanguageChanged += (_, _) => OnPropertyChanged(nameof(WindowTitle));
    }

    public EditorState State { get; }

    public MenuBarViewModel MenuBar { get; }

    public ToolbarViewModel Toolbar { get; }

    public ToolboxViewModel Toolbox { get; }

    public CanvasAreaViewModel Canvas { get; }

    public InspectorViewModel Inspector { get; }

    public StatusBarViewModel StatusBar { get; }

    /// <summary>The window title: an asterisk while there are unsaved changes, the file name (or "Untitled") and the product name.</summary>
    public string WindowTitle =>
        $"{(_session.IsDirty ? "* " : string.Empty)}{(_session.Path is null ? _localizer.T("Untitled") : _session.DisplayName)} — PressToShape";

    /// <summary>Asks about unsaved changes before the window closes. Returns whether closing may go on.</summary>
    public Task<bool> ConfirmCloseAsync() => _files.ConfirmDiscardAsync();

    /// <summary>Zooms so the artboard fits the canvas area. Does nothing until the canvas has a size.</summary>
    public void FitToWindow() => State.ZoomToFit(Canvas.ViewportWidth, Canvas.ViewportHeight);

    /// <summary>Fits the artboard once, as soon as the canvas area reports a real size, so the app opens at fit-to-window.</summary>
    private void OnCanvasPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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

    // Commands for window-level keyboard shortcuts. They do the same as the matching menu items.
    [RelayCommand]
    private void ZoomIn() => State.ZoomIn();

    [RelayCommand]
    private void ZoomOut() => State.ZoomOut();

    [RelayCommand]
    private void ActualSize() => State.ZoomToActualSize();

    [RelayCommand]
    private void FitArtboard() => FitToWindow();

    [RelayCommand]
    private void Quit() => _shutdown.Shutdown();
}
