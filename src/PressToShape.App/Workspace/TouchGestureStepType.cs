namespace PressToShape.App.Workspace;

/// <summary>What the canvas has to do after a touch event, as decided by <see cref="TouchGestureTracker"/>.</summary>
internal enum TouchGestureStepType
{
    /// <summary>Nothing. The finger is not used.</summary>
    Ignore,

    /// <summary>The event is not a finger (mouse or pen). The caller handles it as usual.</summary>
    NotTracked,

    /// <summary>The first finger went down: the active tool acts, like a mouse press at <c>X</c>, <c>Y</c>.</summary>
    Press,

    /// <summary>The first finger moved: like a mouse move at <c>X</c>, <c>Y</c>.</summary>
    Move,

    /// <summary>The first finger went up: like a mouse release at <c>X</c>, <c>Y</c>.</summary>
    Release,

    /// <summary>The one-finger action that was running must be cancelled without a result, because a second finger arrived or the gesture was cancelled.</summary>
    CancelPress,

    /// <summary>Two fingers moved: zoom to <c>TargetZoom</c> around the previous midpoint, then move the view by the midpoint's change.</summary>
    Gesture,
}
