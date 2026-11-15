using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Localization;
using PressToShape.Core.History;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// One row of the History tab: the start state (<see cref="Type"/> is <c>null</c>) or a step.
/// The name is translated when it is read, so it follows a language switch.
/// </summary>
internal sealed partial class HistoryItemViewModel : LocalizedViewModel
{
    private readonly string _messageId;

    public HistoryItemViewModel(ILocalizer localizer, int index, HistoryStepType? type, string messageId)
        : base(localizer)
    {
        Index = index;
        Type = type;
        _messageId = messageId;
    }

    /// <summary>The position in the history: 0 is the start state, k is the state after the k-th step.</summary>
    public int Index { get; }

    /// <summary>The step type, or <c>null</c> for the start row.</summary>
    public HistoryStepType? Type { get; }

    /// <summary>The translated name of the row.</summary>
    public string Name => Localizer.T(_messageId);

    /// <summary>Whether this is the current state of the document.</summary>
    [ObservableProperty] public partial bool IsCurrent { get; set; }

    /// <summary>Whether this state lies after the current one, so redo leads to it.</summary>
    [ObservableProperty] public partial bool IsRedoable { get; set; }
}
