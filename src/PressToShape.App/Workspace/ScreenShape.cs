using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// A shape ready to draw: position, size and stroke width are already in screen pixels.
/// <see cref="Stroke"/> is <c>null</c> when nothing should be stroked, which includes a stroke width of zero.
/// </summary>
/// <param name="Type">Rectangle, ellipse or path.</param>
/// <param name="X">Left edge in screen pixels.</param>
/// <param name="Y">Top edge in screen pixels.</param>
/// <param name="Width">Width in screen pixels.</param>
/// <param name="Height">Height in screen pixels.</param>
/// <param name="Fill">Fill color, or <c>null</c> for no fill.</param>
/// <param name="Stroke">Stroke color, or <c>null</c> for no stroke.</param>
/// <param name="StrokeWidth">Stroke width in screen pixels. Zero when there is no stroke.</param>
/// <param name="Opacity">Opacity of the whole shape, 0 to 1.</param>
/// <param name="RotationDegrees">Rotation in degrees, clockwise, around the center of the box. 0 for an upright shape.</param>
/// <param name="Path">The outline of a path, relative to the top left corner of the box. <c>null</c> for every type except <see cref="ScreenShapeType.Path"/>.</param>
/// <param name="Corners">The corner radii of a rectangle in screen pixels (the radii that are really used, see <see cref="RectangleShape.EffectiveRadii"/>). <c>null</c> for square corners and for every other type.</param>
/// <param name="IsDimmed">Whether the shape is drawn faded because a group is entered and the shape is not inside it.</param>
internal readonly record struct ScreenShape(
    ScreenShapeType Type,
    double X,
    double Y,
    double Width,
    double Height,
    Color? Fill,
    Color? Stroke,
    double StrokeWidth,
    double Opacity,
    double RotationDegrees = 0,
    bool IsDimmed = false,
    ScreenPath? Path = null,
    CornerRadii? Corners = null);
