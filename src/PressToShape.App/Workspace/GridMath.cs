using System;

namespace PressToShape.App.Workspace;

/// <summary>
/// The arithmetic of the grid: the nearest line, the next line in a direction and the distance an arrow-key nudge moves an edge.
/// Pure functions. A line is always computed as <c>index * spacing</c>, never by adding spacings, so repeated use does not drift.
/// An input that is not finite, a spacing of 0 or less, or a direction other than -1 and 1 gives no change (0, or the value itself).
/// </summary>
internal static class GridMath
{
    // Absorbs floating point noise: 0.3 / 0.1 is 2.9999999999999996, and an edge that is on a line must not count as just before it.
    private const double Epsilon = 1e-9;

    /// <summary>The grid line nearest to the value. Halfway rounds away from zero.</summary>
    public static double Nearest(double value, double spacing)
    {
        if (!IsUsable(value, spacing))
        {
            return value;
        }

        return Math.Round(value / spacing, MidpointRounding.AwayFromZero) * spacing;
    }

    /// <summary>
    /// The first grid line strictly beyond the value in the direction (1 for increasing, -1 for decreasing). A value that is already on a
    /// line gives the line one spacing away.
    /// </summary>
    public static double NextLine(double value, double spacing, int direction)
    {
        if (!IsUsable(value, spacing) || direction is not (1 or -1))
        {
            return value;
        }

        return NextIndex(value, spacing, direction) * spacing;
    }

    /// <summary>
    /// How far an edge at <paramref name="edge"/> moves to land on a grid line: the next line in the direction, plus
    /// <c>steps - 1</c> further spacings. Returns 0 for a step count below 1.
    /// </summary>
    public static double NudgeDelta(double edge, double spacing, int direction, int steps)
    {
        if (!IsUsable(edge, spacing) || direction is not (1 or -1) || steps < 1)
        {
            return 0;
        }

        var index = NextIndex(edge, spacing, direction) + direction * (steps - 1);
        return index * spacing - edge;
    }

    private static double NextIndex(double value, double spacing, int direction)
    {
        var position = value / spacing;
        return direction > 0 ? Math.Floor(position + Epsilon) + 1 : Math.Ceiling(position - Epsilon) - 1;
    }

    private static bool IsUsable(double value, double spacing) =>
        double.IsFinite(value) && double.IsFinite(spacing) && spacing > 0;
}
