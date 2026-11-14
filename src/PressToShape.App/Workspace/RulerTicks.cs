using System;
using System.Collections.Generic;
using System.Globalization;

namespace PressToShape.App.Workspace;

/// <summary>Computes ruler ticks for a given zoom. Pure logic, independent of drawing.</summary>
internal static class RulerTicks
{
    /// <summary>Minimum distance between two major ticks on screen, in pixels.</summary>
    public const double MinMajorSpacing = 60;

    /// <summary>Upper limit for the number of ticks returned, to protect against huge lengths.</summary>
    public const int MaxTickCount = 5000;

    private static readonly double[] BaseSteps = [1, 2, 5];

    /// <summary>
    /// Computes the ticks that are visible on a ruler.
    /// </summary>
    /// <param name="zoomPercent">Zoom in percent (100 means one document pixel per screen pixel).</param>
    /// <param name="originPx">Screen position along the ruler of document position 0.</param>
    /// <param name="lengthPx">Length of the ruler in screen pixels.</param>
    public static IReadOnlyList<RulerTick> Compute(double zoomPercent, double originPx, double lengthPx)
    {
        if (!double.IsFinite(zoomPercent) || !double.IsFinite(originPx) || !double.IsFinite(lengthPx)
            || zoomPercent <= 0 || lengthPx <= 0)
        {
            return [];
        }

        var scale = zoomPercent / 100.0;
        var (majorStep, divisions) = ChooseMajorStep(scale);
        var minorStep = majorStep / divisions;

        // Tick index i stands for document position i * minorStep.
        var firstIndex = (long)Math.Ceiling((0 - originPx) / scale / minorStep);
        var lastIndex = (long)Math.Floor((lengthPx - originPx) / scale / minorStep);
        if (lastIndex - firstIndex + 1 > MaxTickCount)
        {
            lastIndex = firstIndex + MaxTickCount - 1;
        }

        var ticks = new List<RulerTick>((int)Math.Max(0, lastIndex - firstIndex + 1));
        for (var i = firstIndex; i <= lastIndex; i++)
        {
            var documentPosition = i * minorStep;
            var screenPosition = originPx + documentPosition * scale;

            if (Mod(i, divisions) == 0)
            {
                ticks.Add(new RulerTick(screenPosition, FormatLabel(documentPosition), TickType.Major));
            }
            else if (divisions % 2 == 0 && Mod(i, divisions / 2) == 0)
            {
                ticks.Add(new RulerTick(screenPosition, null, TickType.Medium));
            }
            else
            {
                ticks.Add(new RulerTick(screenPosition, null, TickType.Minor));
            }
        }

        return ticks;
    }

    private static (double MajorStep, int Divisions) ChooseMajorStep(double scale)
    {
        // Smallest step from 1, 2, 5, 10, 20, 50, ... document pixels whose screen distance is large enough.
        for (var magnitude = 1.0; magnitude < 1e12; magnitude *= 10)
        {
            foreach (var baseStep in BaseSteps)
            {
                var step = baseStep * magnitude;
                if (step * scale >= MinMajorSpacing)
                {
                    return (step, DivisionsFor(baseStep));
                }
            }
        }

        return (1e12, 10);
    }

    private static int DivisionsFor(double baseStep) => baseStep switch
    {
        2 => 4,
        5 => 5,
        _ => 10,
    };

    private static long Mod(long value, long modulus) => ((value % modulus) + modulus) % modulus;

    private static string FormatLabel(double documentPosition) =>
        Math.Round(documentPosition).ToString("0", CultureInfo.InvariantCulture);
}
