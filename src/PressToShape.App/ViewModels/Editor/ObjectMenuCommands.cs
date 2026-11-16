using System.Windows.Input;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>What the Group, Arrange, Lock and Hide items of the Object menu do. A command is disabled while it would change nothing.</summary>
internal sealed record ObjectMenuCommands(
    ICommand Group,
    ICommand Ungroup,
    ICommand BringToFront,
    ICommand BringForward,
    ICommand SendBackward,
    ICommand SendToBack,
    ICommand Lock,
    ICommand UnlockAll,
    ICommand Hide,
    ICommand ShowAll);
