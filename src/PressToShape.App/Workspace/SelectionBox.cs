namespace PressToShape.App.Workspace;

/// <summary>A rectangle in document units. Used for the selection box that is dragged on empty space.</summary>
/// <param name="X">Left edge.</param>
/// <param name="Y">Top edge.</param>
/// <param name="Width">Width, not negative.</param>
/// <param name="Height">Height, not negative.</param>
internal readonly record struct SelectionBox(double X, double Y, double Width, double Height);
