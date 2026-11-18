namespace PressToShape.App.Workspace;

/// <summary>A snap guide line in screen pixels, ready to draw. A vertical line stands at an x and reaches from <paramref name="From"/> to <paramref name="To"/> in y.</summary>
/// <param name="IsVertical">Whether the line is vertical (a guide of the x axis) or horizontal (a guide of the y axis).</param>
/// <param name="Position">The x of a vertical line or the y of a horizontal one.</param>
/// <param name="From">Where the line starts along its direction.</param>
/// <param name="To">Where the line ends along its direction.</param>
internal readonly record struct ScreenSnapGuide(bool IsVertical, double Position, double From, double To);
