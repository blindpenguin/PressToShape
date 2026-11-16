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
        HistoryStepType.GroupShapes => N("Group shapes"),
        HistoryStepType.UngroupShapes => N("Ungroup shapes"),
        HistoryStepType.BringToFront => N("Bring to front"),
        HistoryStepType.BringForward => N("Bring forward"),
        HistoryStepType.SendBackward => N("Send backward"),
        HistoryStepType.SendToBack => N("Send to back"),
        HistoryStepType.ReorderShape => N("Reorder shape"),
        HistoryStepType.LockShapes => N("Lock shapes"),
        HistoryStepType.UnlockShapes => N("Unlock shapes"),
        HistoryStepType.HideShapes => N("Hide shapes"),
        HistoryStepType.ShowShapes => N("Show shapes"),
        HistoryStepType.UnlockAll => N("Unlock all"),
        HistoryStepType.ShowAll => N("Show all"),
        HistoryStepType.RenameShape => N("Rename shape"),
        HistoryStepType.DrawPath => N("Draw path"),
        HistoryStepType.AddPathNode => N("Add path point"),
        HistoryStepType.DeletePathNode => N("Delete path point"),
        HistoryStepType.ChangePath => N("Change path"),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown history step type."),
    };
}
