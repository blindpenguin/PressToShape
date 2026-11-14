namespace PressToShape.Core.Geometry;

/// <summary>An axis-aligned rectangle in document units. Origin is the artboard's top-left corner, y grows downward.</summary>
/// <param name="X">Left edge.</param>
/// <param name="Y">Top edge.</param>
/// <param name="Width">Width, not negative.</param>
/// <param name="Height">Height, not negative.</param>
public readonly record struct Bounds(double X, double Y, double Width, double Height);
