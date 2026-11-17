namespace PressToShape.Core.Geometry;

/// <summary>
/// A point of a flattened outline together with where it came from: the curve point at <paramref name="T"/> of a segment.
/// The tags let a later step (the boolean operations) find out which pieces of the result are unchanged pieces of an original curve.
/// </summary>
/// <param name="X">The x position in document units.</param>
/// <param name="Y">The y position in document units.</param>
/// <param name="Operand">The index of the operand (the shape) the point comes from.</param>
/// <param name="Segment">The index of the segment, counted across all sub-paths of the operand.</param>
/// <param name="T">The position on the segment, from 0 (its start) to 1 (its end).</param>
public readonly record struct FlatPoint(double X, double Y, int Operand, int Segment, double T);
