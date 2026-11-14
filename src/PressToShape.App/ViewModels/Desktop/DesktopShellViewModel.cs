using System;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
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
    private bool _initialFitDone;

    public DesktopShellViewModel(IAppShutdown shutdown, ILocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        _shutdown = shutdown ?? throw new ArgumentNullException(nameof(shutdown));

        State = new EditorState();
        Toolbox = new ToolboxViewModel(State, localizer);
        Canvas = new CanvasAreaViewModel(State);
        Inspector = new InspectorViewModel(State, localizer);
        Toolbar = new ToolbarViewModel(State, localizer);
        StatusBar = new StatusBarViewModel(State, localizer, FitToWindow);
        MenuBar = new MenuBarViewModel(State, shutdown, localizer, Inspector.ShowTab, FitToWindow);

        Canvas.PropertyChanged += OnCanvasPropertyChanged;
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
