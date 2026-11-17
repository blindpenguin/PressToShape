using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.ViewModels.Editor;

// The Path section of the Properties tab: open or closed, and the fill rule, of the selected paths. Other shapes in the selection
// are left alone. When the paths differ in a value, neither choice of that value is shown as set.
internal sealed partial class PropertiesViewModel
{
    private List<PathShape> _selectedPaths = [];

    /// <summary>Whether the selection has at least one path (also inside a selected group).</summary>
    public bool HasPathSelected => _selectedPaths.Count > 0;

    /// <summary>Whether every sub-path of every selected path is closed.</summary>
    public bool IsPathClosed => _selectedPaths.Count > 0 && _selectedPaths.All(path => path.SubPaths.All(subPath => subPath.IsClosed));

    /// <summary>Whether every sub-path of every selected path is open.</summary>
    public bool IsPathOpen => _selectedPaths.Count > 0 && _selectedPaths.All(path => path.SubPaths.All(subPath => !subPath.IsClosed));

    /// <summary>Whether every selected path uses the non-zero fill rule.</summary>
    public bool IsFillRuleNonZero => _selectedPaths.Count > 0 && _selectedPaths.All(path => path.FillRule == FillRule.NonZero);

    /// <summary>Whether every selected path uses the even-odd fill rule.</summary>
    public bool IsFillRuleEvenOdd => _selectedPaths.Count > 0 && _selectedPaths.All(path => path.FillRule == FillRule.EvenOdd);

    /// <summary>Closes the selected paths, as one step.</summary>
    [RelayCommand(CanExecute = nameof(HasPathSelected))]
    private void ClosePath() => SetClosed(true);

    /// <summary>Opens the selected paths, as one step.</summary>
    [RelayCommand(CanExecute = nameof(HasPathSelected))]
    private void OpenPath() => SetClosed(false);

    /// <summary>Gives the selected paths the non-zero fill rule, as one step.</summary>
    [RelayCommand(CanExecute = nameof(HasPathSelected))]
    private void UseNonZeroFill() => SetFillRule(FillRule.NonZero);

    /// <summary>Gives the selected paths the even-odd fill rule, as one step.</summary>
    [RelayCommand(CanExecute = nameof(HasPathSelected))]
    private void UseEvenOddFill() => SetFillRule(FillRule.EvenOdd);

    // Closing or opening changes every sub-path and can change the box (a curved closing segment), so the path is rebuilt, which also keeps a rotated path in place.
    private void SetClosed(bool closed) =>
        _editor.Apply(
            HistoryStepType.ChangePath,
            shape => shape is PathShape path && path.SubPaths.Any(subPath => subPath.IsClosed != closed)
                ? path.Rebuilt([.. path.ToDocumentSubPaths().Select(subPath => new DocumentSubPath(subPath.Nodes, closed))], path.FillRule)
                : shape);

    private void SetFillRule(FillRule rule) =>
        _editor.Apply(
            HistoryStepType.ChangePath,
            shape => shape is PathShape path && path.FillRule != rule ? path with { FillRule = rule } : shape);

    private void RefreshPathValues()
    {
        _selectedPaths = SelectionQueries.SelectedLeaves(_state.Document, _state.SelectedShapeIds).OfType<PathShape>().ToList();
        OnPropertyChanged(nameof(HasPathSelected));
        OnPropertyChanged(nameof(IsPathClosed));
        OnPropertyChanged(nameof(IsPathOpen));
        OnPropertyChanged(nameof(IsFillRuleNonZero));
        OnPropertyChanged(nameof(IsFillRuleEvenOdd));
        OnPropertyChanged(nameof(ShowPath));
        ClosePathCommand.NotifyCanExecuteChanged();
        OpenPathCommand.NotifyCanExecuteChanged();
        UseNonZeroFillCommand.NotifyCanExecuteChanged();
        UseEvenOddFillCommand.NotifyCanExecuteChanged();
    }
}
