using System;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// Root of the desktop layout. Adds the menu bar, the toolbar and the status bar to what every shell has (see <see cref="EditorShellViewModel"/>).
/// </summary>
internal sealed partial class DesktopShellViewModel : EditorShellViewModel
{
    private readonly IAppShutdown _shutdown;

    public DesktopShellViewModel(IAppShutdown shutdown, ILocalizer localizer, FileServices services)
        : base(localizer, services)
    {
        _shutdown = shutdown ?? throw new ArgumentNullException(nameof(shutdown));

        Toolbar = new ToolbarViewModel(State, localizer, UndoCommand, RedoCommand);
        StatusBar = new StatusBarViewModel(State, localizer, FitToWindow, PathEditing);
        MenuBar = new MenuBarViewModel(State, shutdown, localizer, Inspector.ShowTab, FitToWindow, FileCommands, EditCommands, ObjectCommands, LayerCommands, PathCommands, Recent, UndoCommand, RedoCommand);
    }

    public MenuBarViewModel MenuBar { get; }

    public ToolbarViewModel Toolbar { get; }

    public StatusBarViewModel StatusBar { get; }

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
