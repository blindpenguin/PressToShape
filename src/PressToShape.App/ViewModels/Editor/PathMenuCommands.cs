using System.Windows.Input;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>What the boolean, Combine and Break Apart items of the Path menu do. A command is disabled while it cannot run.</summary>
internal sealed record PathMenuCommands(
    ICommand Union,
    ICommand Difference,
    ICommand Intersection,
    ICommand Exclusion,
    ICommand Combine,
    ICommand BreakApart);
