namespace PressToShape.Core.History;

/// <summary>What a step of the history did. The app turns each value into a translated name.</summary>
public enum HistoryStepType
{
    /// <summary>A rectangle was drawn.</summary>
    DrawRectangle,

    /// <summary>An ellipse was drawn.</summary>
    DrawEllipse,

    /// <summary>A shape was moved.</summary>
    MoveShape,

    /// <summary>A shape was deleted.</summary>
    DeleteShape,

    /// <summary>A layer was hidden.</summary>
    HideLayer,

    /// <summary>A layer was shown.</summary>
    ShowLayer,

    /// <summary>A layer was locked.</summary>
    LockLayer,

    /// <summary>A layer was unlocked.</summary>
    UnlockLayer,
}
