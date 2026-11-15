using System.Windows.Input;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>What the clipboard, delete and selection items of the Edit menu do. Undo and Redo are passed on their own.</summary>
internal sealed record EditMenuCommands(
    ICommand Cut,
    ICommand Copy,
    ICommand Paste,
    ICommand PasteInPlace,
    ICommand Duplicate,
    ICommand Delete,
    ICommand SelectAll,
    ICommand Deselect);
