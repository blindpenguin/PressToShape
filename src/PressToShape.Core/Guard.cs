using System;

namespace PressToShape.Core;

/// <summary>Argument checks shared by the document model, so every type reports invalid values the same way.</summary>
internal static class Guard
{
    public static double Finite(double value, string name)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a finite number.");
        }

        return value;
    }

    public static double FiniteNonNegative(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a finite number that is not negative.");
        }

        return value;
    }

    public static double FinitePositive(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a finite number greater than zero.");
        }

        return value;
    }

    public static double FiniteInRange(double value, double min, double max, string name)
    {
        if (!double.IsFinite(value) || value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a finite number from {min} to {max}.");
        }

        return value;
    }
}
