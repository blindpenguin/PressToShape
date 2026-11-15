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

    /// <summary>Shapes were resized.</summary>
    ResizeShapes,

    /// <summary>Shapes were rotated.</summary>
    RotateShapes,

    /// <summary>Shapes were moved with the arrow keys.</summary>
    NudgeShapes,

    /// <summary>Shapes were pasted.</summary>
    PasteShapes,

    /// <summary>Shapes were cut.</summary>
    CutShapes,

    /// <summary>Shapes were duplicated.</summary>
    DuplicateShapes,

    /// <summary>The rotation of shapes was reset.</summary>
    ResetTransform,

    /// <summary>The position of shapes was typed in.</summary>
    ChangePosition,

    /// <summary>The size of shapes was typed in.</summary>
    ChangeSize,

    /// <summary>The rotation of shapes was typed in.</summary>
    ChangeRotation,

    /// <summary>The fill of shapes was changed.</summary>
    ChangeFill,

    /// <summary>The stroke of shapes was changed.</summary>
    ChangeStroke,

    /// <summary>The stroke width of shapes was changed.</summary>
    ChangeStrokeWidth,

    /// <summary>The opacity of shapes was changed.</summary>
    ChangeOpacity,
    /// <summary>A colour was added to the palette.</summary>
    AddSwatch,

    /// <summary>A colour was removed from the palette.</summary>
    RemoveSwatch,
}
