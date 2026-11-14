using System;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// Root of the desktop layout. Creates the shared <see cref="EditorState"/> and the view model of every region,
/// so toolbar, tool column, menu, canvas, inspector and status bar always agree.
/// </summary>
internal sealed partial class DesktopShellViewModel : ViewModelBase
{
    private readonly IAppShutdown _shutdown;

    public DesktopShellViewModel(IAppShutdown shutdown)
    {
        _shutdown = shutdown ?? throw new ArgumentNullException(nameof(shutdown));

        State = new EditorState();
        Toolbox = new ToolboxViewModel(State);
        Canvas = new CanvasAreaViewModel(State);
        Inspector = new InspectorViewModel(State);
        Toolbar = new ToolbarViewModel(State);
        StatusBar = new StatusBarViewModel(State, FitToWindow);
        MenuBar = new MenuBarViewModel(State, shutdown, Inspector.ShowTab, FitToWindow);
    }

    public EditorState State { get; }

    public MenuBarViewModel MenuBar { get; }

    public ToolbarViewModel Toolbar { get; }

    public ToolboxViewModel Toolbox { get; }

    public CanvasAreaViewModel Canvas { get; }

    public InspectorViewModel Inspector { get; }

    public StatusBarViewModel StatusBar { get; }

    /// <summary>Zooms so the artboard fits the canvas area. Does nothing until the canvas has a size.</summary>
    public void FitToWindow() => State.ZoomToFit(Canvas.ViewportWidth, Canvas.ViewportHeight);

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
