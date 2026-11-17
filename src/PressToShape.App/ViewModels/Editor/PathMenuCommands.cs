using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// What the items of the Path menu do. A command is disabled while it cannot run. <paramref name="ConvertToPath"/> and <paramref name="ReverseDirection"/>
/// are optional: a caller without them gets disabled items.
/// </summary>
internal sealed record PathMenuCommands(
    ICommand Union,
    ICommand Difference,
    ICommand Intersection,
    ICommand Exclusion,
    ICommand Combine,
    ICommand BreakApart,
    ICommand? ConvertToPath = null,
    ICommand? ReverseDirection = null)
{
    /// <summary>Six commands that can never run, for a view that has no real ones (a test, or a caller that does not offer the path operations).</summary>
    public static PathMenuCommands Disabled { get; } = CreateDisabled();

    private static PathMenuCommands CreateDisabled()
    {
        ICommand None() => new RelayCommand(() => { }, () => false);
        return new PathMenuCommands(None(), None(), None(), None(), None(), None());
    }
}
