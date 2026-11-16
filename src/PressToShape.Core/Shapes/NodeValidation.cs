using System;

namespace PressToShape.Core.Shapes;

/// <summary>The checks shared by <see cref="PathNode"/> and <see cref="DocumentNode"/>, so both refuse the same values.</summary>
internal static class NodeValidation
{
    /// <summary>Checks the anchor and the two optional handle points. A handle point is given with both numbers or with none.</summary>
    public static void Check(double x, double y, double? inX, double? inY, double? outX, double? outY)
    {
        Guard.Finite(x, nameof(x));
        Guard.Finite(y, nameof(y));
        CheckHandle(inX, inY, "in");
        CheckHandle(outX, outY, "out");
    }

    private static void CheckHandle(double? handleX, double? handleY, string name)
    {
        if (handleX.HasValue != handleY.HasValue)
        {
            throw new ArgumentException($"The {name} handle needs both numbers or none.");
        }

        if (handleX.HasValue)
        {
            Guard.Finite(handleX.Value, name + "X");
            Guard.Finite(handleY!.Value, name + "Y");
        }
    }
}
