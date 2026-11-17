using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>What the boolean, Combine and Break Apart items of the Path menu do. A command is disabled while it cannot run.</summary>
internal sealed record PathMenuCommands(
    ICommand Union,
    ICommand Difference,
    ICommand Intersection,
    ICommand Exclusion,
    ICommand Combine,
    ICommand BreakApart)
{
    /// <summary>Six commands that can never run, for a view that has no real ones (a test, or a caller that does not offer the path operations).</summary>
    public static PathMenuCommands Disabled { get; } = CreateDisabled();

    private static PathMenuCommands CreateDisabled()
    {
        ICommand None() => new RelayCommand(() => { }, () => false);
        return new PathMenuCommands(None(), None(), None(), None(), None(), None());
    }
}
