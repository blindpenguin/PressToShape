namespace PressToShape.App.Workspace;

/// <summary>A ruler guide in screen pixels, ready to draw across the canvas.</summary>
/// <param name="IsVertical">Whether the guide is a vertical line (at an x) or a horizontal one (at a y).</param>
/// <param name="Position">The x of a vertical guide or the y of a horizontal one.</param>
/// <param name="IsSelected">Whether the user selected the guide.</param>
/// <param name="IsActive">Whether the current drag is snapped to the guide.</param>
internal readonly record struct ScreenRulerGuide(bool IsVertical, double Position, bool IsSelected, bool IsActive);

/// <summary>
/// A smart guide marker in screen pixels with its label text. A line along an axis from <paramref name="From"/> to <paramref name="To"/>
/// at <paramref name="Cross"/> on the other axis. A <see cref="SmartMarkerType.PathPoint"/> is the point (<paramref name="From"/>, <paramref name="Cross"/>)
/// of a horizontal marker.
/// </summary>
/// <param name="Type">What the marker shows.</param>
/// <param name="IsHorizontal">Whether the line runs left to right (its x values change) or up and down.</param>
/// <param name="From">Where the line starts along its direction.</param>
/// <param name="To">Where the line ends along its direction.</param>
/// <param name="Cross">The position across, on the other axis.</param>
/// <param name="Text">The number to show next to the line, in the number format of the language, or <c>null</c> for none.</param>
internal readonly record struct ScreenSmartMarker(SmartMarkerType Type, bool IsHorizontal, double From, double To, double Cross, string? Text);
