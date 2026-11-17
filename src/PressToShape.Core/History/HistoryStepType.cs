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

    /// <summary>A path was drawn with the Pen.</summary>
    DrawPath,

    /// <summary>A point was added to a path.</summary>
    AddPathNode,

    /// <summary>A point was deleted from a path.</summary>
    DeletePathNode,

    /// <summary>A path was opened or closed, or its fill rule was changed.</summary>
    ChangePath,

    /// <summary>Points of a path were moved, with a drag, the arrow keys or typed numbers.</summary>
    MovePathPoints,

    /// <summary>A handle of a path point was moved.</summary>
    MovePathHandle,

    /// <summary>The type of path points was changed (corner, smooth, symmetric).</summary>
    ChangeNodeType,

    /// <summary>A segment of a path was bent by dragging it.</summary>
    BendSegment,

    /// <summary>A segment of a path was made straight or curved.</summary>
    ChangeSegment,

    /// <summary>Points were deleted from a path.</summary>
    DeletePathPoints,

    /// <summary>The corner radius of a rectangle was changed, by a field or by dragging a radius handle.</summary>
    ChangeCornerRadius,

    /// <summary>A layer was added.</summary>
    AddLayer,

    /// <summary>A layer was deleted.</summary>
    DeleteLayer,

    /// <summary>A layer was renamed.</summary>
    RenameLayer,

    /// <summary>A layer was moved to another place in the stacking order of the layers.</summary>
    ReorderLayer,

    /// <summary>A layer was duplicated.</summary>
    DuplicateLayer,

    /// <summary>A layer was merged into the layer below it.</summary>
    MergeLayers,

    /// <summary>Shapes were moved to another layer.</summary>
    MoveToLayer,

    /// <summary>Shapes were combined with the union operation into one path.</summary>
    Union,

    /// <summary>Shapes were combined with the difference operation into one path.</summary>
    Difference,

    /// <summary>Shapes were combined with the intersection operation into one path.</summary>
    Intersection,

    /// <summary>Shapes were combined with the exclusion operation into one path.</summary>
    Exclusion,

    /// <summary>Paths were joined into one compound path.</summary>
    Combine,

    /// <summary>A compound path was split into one path for each sub-path.</summary>
    BreakApart,

    /// <summary>Rectangles and ellipses were converted to paths.</summary>
    ConvertToPath,

    /// <summary>The direction of paths was reversed.</summary>
    ReverseDirection,
}
