namespace PressToShape.App.Workspace;

/// <summary>The geometry a <see cref="ScreenShape"/> is drawn with.</summary>
internal enum ScreenShapeType
{
    Rectangle,
    Ellipse,

    /// <summary>A path of lines and curves. The shape's <see cref="ScreenShape.Path"/> holds its outline.</summary>
    Path,
}
