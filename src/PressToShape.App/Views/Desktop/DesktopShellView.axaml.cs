using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using PressToShape.App.ViewModels.Desktop;

namespace PressToShape.App.Views.Desktop;

public partial class DesktopShellView : UserControl
{
    public DesktopShellView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Single-key tool shortcuts. The view only decides whether the key press is meant for the shell
    /// (no modifiers, no text field focused). The view model decides which tool it selects.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled || DataContext is not DesktopShellViewModel shell)
        {
            return;
        }

        // A text box keeps its own keys (select all, copy, paste, undo ...), so none of the shortcuts below run while one has focus.
        if (TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is TextBox)
        {
            return;
        }

        if (EditShortcut(shell, e.Key, e.KeyModifiers) is { } command)
        {
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }

            e.Handled = true;
            return;
        }

        if (e.KeyModifiers != KeyModifiers.None && e.KeyModifiers != KeyModifiers.Shift)
        {
            return;
        }

        if (e.KeySymbol is { Length: 1 } symbol && shell.Toolbox.TrySelectByKey(symbol[0]))
        {
            e.Handled = true;
        }
    }

    // Undo, redo, clipboard, selection, group, arrange, lock and hide shortcuts. They are handled here and not as key bindings, because a key binding would
    // also run while a text box has focus and take the key from it.
    private static ICommand? EditShortcut(DesktopShellViewModel shell, Key key, KeyModifiers modifiers) => (modifiers, key) switch
    {
        (KeyModifiers.Control, Key.Z) => shell.UndoCommand,
        (KeyModifiers.Control | KeyModifiers.Shift, Key.Z) => shell.RedoCommand,
        (KeyModifiers.Control, Key.Y) => shell.RedoCommand,
        (KeyModifiers.Control, Key.X) => shell.CutCommand,
        (KeyModifiers.Control, Key.C) => shell.CopyCommand,
        (KeyModifiers.Control, Key.V) => shell.PasteCommand,
        (KeyModifiers.Control | KeyModifiers.Shift, Key.V) => shell.PasteInPlaceCommand,
        (KeyModifiers.Control, Key.D) => shell.DuplicateCommand,
        (KeyModifiers.Control, Key.A) => shell.SelectAllCommand,
        (KeyModifiers.Control | KeyModifiers.Shift, Key.A) => shell.DeselectCommand,
        (KeyModifiers.Control, Key.G) => shell.GroupCommand,
        (KeyModifiers.Control | KeyModifiers.Shift, Key.G) => shell.UngroupCommand,
        (KeyModifiers.None, Key.Home) => shell.BringToFrontCommand,
        (KeyModifiers.None, Key.PageUp) => shell.BringForwardCommand,
        (KeyModifiers.None, Key.PageDown) => shell.SendBackwardCommand,
        (KeyModifiers.None, Key.End) => shell.SendToBackCommand,
        (KeyModifiers.Control, Key.D2) => shell.LockCommand,
        (KeyModifiers.Control | KeyModifiers.Alt, Key.D2) => shell.UnlockAllCommand,
        (KeyModifiers.Control, Key.D3) => shell.HideCommand,
        (KeyModifiers.Control | KeyModifiers.Alt, Key.D3) => shell.ShowAllCommand,
        _ => null,
    };
}
