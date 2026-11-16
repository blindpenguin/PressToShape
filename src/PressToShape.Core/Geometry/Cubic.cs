namespace PressToShape.Core.Geometry;

/// <summary>A cubic bezier segment in document units: from <paramref name="P0"/> to <paramref name="P3"/>, shaped by two handle points. A line is a cubic whose handle points lie on its end points.</summary>
/// <param name="P0">Start point.</param>
/// <param name="P1">Handle point of the start.</param>
/// <param name="P2">Handle point of the end.</param>
/// <param name="P3">End point.</param>
public readonly record struct Cubic((double X, double Y) P0, (double X, double Y) P1, (double X, double Y) P2, (double X, double Y) P3);
