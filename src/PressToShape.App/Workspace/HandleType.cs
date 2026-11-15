namespace PressToShape.App.Workspace;

/// <summary>The handles around a selection. The eight resize handles sit on the frame, <see cref="Rotate"/> outside the top edge.</summary>
internal enum HandleType
{
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left,
    Rotate,
}
