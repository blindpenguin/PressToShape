using System;
using PressToShape.Core.History;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.Workspace;

/// <summary>The message ids (English texts) for the rows of the history list. Translate them with <c>ILocalizer.T</c> when shown.</summary>
internal static class HistoryStepNames
{
    /// <summary>The first row of the list: the state before the first kept step.</summary>
    public static string Start => N("Start");

    /// <summary>The message id for the name of a step type.</summary>
    public static string MessageId(HistoryStepType type) => type switch
    {
        HistoryStepType.DrawRectangle => N("Draw rectangle"),
        HistoryStepType.DrawEllipse => N("Draw ellipse"),
        HistoryStepType.MoveShape => N("Move shape"),
        HistoryStepType.DeleteShape => N("Delete shape"),
        HistoryStepType.HideLayer => N("Hide layer"),
        HistoryStepType.ShowLayer => N("Show layer"),
        HistoryStepType.LockLayer => N("Lock layer"),
        HistoryStepType.UnlockLayer => N("Unlock layer"),
        HistoryStepType.ResizeShapes => N("Resize shapes"),
        HistoryStepType.RotateShapes => N("Rotate shapes"),
        HistoryStepType.NudgeShapes => N("Nudge shapes"),
        HistoryStepType.PasteShapes => N("Paste shapes"),
        HistoryStepType.CutShapes => N("Cut shapes"),
        HistoryStepType.DuplicateShapes => N("Duplicate shapes"),
        HistoryStepType.ResetTransform => N("Reset transform"),
        HistoryStepType.ChangePosition => N("Change position"),
        HistoryStepType.ChangeSize => N("Change size"),
        HistoryStepType.ChangeRotation => N("Change rotation"),
        HistoryStepType.ChangeFill => N("Change fill"),
        HistoryStepType.ChangeStroke => N("Change stroke"),
        HistoryStepType.ChangeStrokeWidth => N("Change stroke width"),
        HistoryStepType.ChangeOpacity => N("Change opacity"),
        HistoryStepType.AddSwatch => N("Add swatch"),
        HistoryStepType.RemoveSwatch => N("Remove swatch"),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown history step type."),
    };
}
