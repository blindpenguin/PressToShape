using System;

namespace PressToShape.App.Workspace;

/// <summary>Zoom limits, preset steps and fit calculation. Pure logic, values are percent.</summary>
internal static class ZoomLevels
{
    public const double Min = 10;
    public const double Max = 6400;
    public const double Default = 100;

    /// <summary>Preset zoom steps in percent, ascending. Used by zoom in, zoom out and the zoom drop-down.</summary>
    public static readonly double[] Presets = [10, 25, 50, 75, 100, 150, 200, 300, 400, 800, 1600, 3200, 6400];

    private const double Epsilon = 1e-6;

    public static double Clamp(double percent)
    {
        if (double.IsNaN(percent))
        {
            return Default;
        }

        return Math.Clamp(percent, Min, Max);
    }

    /// <summary>The smallest preset above the current zoom, or <see cref="Max"/> if there is none.</summary>
    public static double NextIn(double current)
    {
        foreach (var preset in Presets)
        {
            if (preset > current + Epsilon)
            {
                return preset;
            }
        }

        return Max;
    }

    /// <summary>The largest preset below the current zoom, or <see cref="Min"/> if there is none.</summary>
    public static double NextOut(double current)
    {
        for (var i = Presets.Length - 1; i >= 0; i--)
        {
            if (Presets[i] < current - Epsilon)
            {
                return Presets[i];
            }
        }

        return Min;
    }

    /// <summary>
    /// The zoom at which the artboard fits into the viewport with a margin on every side.
    /// Returns null when any size is not positive, so callers can leave the zoom unchanged.
    /// </summary>
    public static double? FitToViewport(double viewportWidth, double viewportHeight, double artboardWidth, double artboardHeight, double margin = 24)
    {
        if (viewportWidth <= 0 || viewportHeight <= 0 || artboardWidth <= 0 || artboardHeight <= 0)
        {
            return null;
        }

        var availableWidth = Math.Max(viewportWidth - 2 * margin, 1);
        var availableHeight = Math.Max(viewportHeight - 2 * margin, 1);
        var fit = Math.Min(availableWidth / artboardWidth, availableHeight / artboardHeight) * 100;

        return Clamp(fit);
    }
}
