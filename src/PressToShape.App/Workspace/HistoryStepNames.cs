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
        HistoryStepType.MovePathPoints => N("Move path points"),
        HistoryStepType.MovePathHandle => N("Move handle"),
        HistoryStepType.ChangeNodeType => N("Change node type"),
        HistoryStepType.BendSegment => N("Bend segment"),
        HistoryStepType.ChangeSegment => N("Change segment"),
        HistoryStepType.DeletePathPoints => N("Delete path points"),
        HistoryStepType.ChangeCornerRadius => N("Change corner radius"),
        HistoryStepType.AddLayer => N("Add layer"),
        HistoryStepType.DeleteLayer => N("Delete layer"),
        HistoryStepType.RenameLayer => N("Rename layer"),
        HistoryStepType.ReorderLayer => N("Reorder layer"),
        HistoryStepType.DuplicateLayer => N("Duplicate layer"),
        HistoryStepType.MergeLayers => N("Merge layers"),
        HistoryStepType.MoveToLayer => N("Move to layer"),
        HistoryStepType.Union => N("Union"),
        HistoryStepType.Difference => N("Difference"),
        HistoryStepType.Intersection => N("Intersection"),
        HistoryStepType.Exclusion => N("Exclusion"),
        HistoryStepType.Combine => N("Combine"),
        HistoryStepType.BreakApart => N("Break apart"),
        HistoryStepType.ConvertToPath => N("Convert to path"),
        HistoryStepType.ReverseDirection => N("Reverse direction"),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown history step type."),
    };
}
