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

    /// <summary>Shapes were grouped.</summary>
    GroupShapes,

    /// <summary>Groups were ungrouped.</summary>
    UngroupShapes,

    /// <summary>Shapes were brought to the front.</summary>
    BringToFront,

    /// <summary>Shapes were brought forward.</summary>
    BringForward,

    /// <summary>Shapes were sent backward.</summary>
    SendBackward,

    /// <summary>Shapes were sent to the back.</summary>
    SendToBack,

    /// <summary>A shape was moved to another place in the stacking order, for example by dragging it in the object list.</summary>
    ReorderShape,

    /// <summary>Shapes were locked.</summary>
    LockShapes,

    /// <summary>Shapes were unlocked.</summary>
    UnlockShapes,

    /// <summary>Shapes were hidden.</summary>
    HideShapes,

    /// <summary>Shapes were shown.</summary>
    ShowShapes,

    /// <summary>Every shape was unlocked.</summary>
    UnlockAll,

    /// <summary>Every shape was shown.</summary>
    ShowAll,

    /// <summary>A shape was renamed.</summary>
    RenameShape,
}
