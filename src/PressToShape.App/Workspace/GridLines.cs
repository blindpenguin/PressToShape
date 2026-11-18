using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.App.Workspace;

/// <summary>The grid lines to draw, as document positions: the x of each vertical line and the y of each horizontal line.</summary>
internal sealed record GridLineSet(IReadOnlyList<double> Vertical, IReadOnlyList<double> Horizontal)
{
    /// <summary>No lines.</summary>
    public static GridLineSet Empty { get; } = new([], []);
}

/// <summary>
/// Chooses which grid lines to draw. Lines are at multiples of the spacing from the document origin. When they would be closer
/// than <see cref="MinPixels"/> on screen only every n-th line is drawn (n is 2, 5, 10, 20, 50, 100 and so on, the smallest that is far enough
/// apart), so the grid stays readable and the number of lines stays small at any zoom. Pure logic, independent of drawing.
/// </summary>
internal static class GridLines
{
    /// <summary>The smallest distance between two drawn lines on screen, in pixels.</summary>
    public const double MinPixels = 8;

    // The step multipliers go 1, 2, 5, 10, 20, 50, ... up to this decade, which is far beyond any zoom or spacing of the app.
    private const double MaxDecade = 1e12;

    // Protects against an unreasonable visible range. The visible canvas never needs more.
    private const int MaxLinesPerAxis = 2000;

    private static readonly double[] Bases = [1, 2, 5];

    /// <summary>The lines inside the visible range (document units). Empty for an input that is not finite, a spacing or zoom of 0 or less, or an empty range.</summary>
    public static GridLineSet Build(double spacing, double zoomPercent, double visibleLeft, double visibleTop, double visibleRight, double visibleBottom)
    {
        if (!double.IsFinite(spacing) || !double.IsFinite(zoomPercent) || spacing <= 0 || zoomPercent <= 0
            || !double.IsFinite(visibleLeft) || !double.IsFinite(visibleTop) || !double.IsFinite(visibleRight) || !double.IsFinite(visibleBottom)
            || visibleRight < visibleLeft || visibleBottom < visibleTop)
        {
            return GridLineSet.Empty;
        }

        var step = DrawnStep(spacing, zoomPercent / 100);
        return new GridLineSet(Lines(step, visibleLeft, visibleRight), Lines(step, visibleTop, visibleBottom));
    }

    /// <summary>
    /// The lines to draw for a canvas, in screen pixels: the x of each vertical line and the y of each horizontal one. The canvas shows the
    /// artboard origin at (<paramref name="originX"/>, <paramref name="originY"/>) and is <paramref name="width"/> by <paramref name="height"/> pixels.
    /// </summary>
    public static GridLineSet BuildOnScreen(double spacing, double zoomPercent, double originX, double originY, double width, double height) =>
        BuildInScreenRect(spacing, zoomPercent, originX, originY, 0, 0, width, height);

    /// <summary>
    /// The lines inside a rectangle of the screen, in screen pixels. The artboard origin is at (<paramref name="originX"/>, <paramref name="originY"/>).
    /// Used to draw the grid on the artboard only: the rectangle is the part of the artboard that is visible.
    /// </summary>
    public static GridLineSet BuildInScreenRect(double spacing, double zoomPercent, double originX, double originY, double left, double top, double right, double bottom)
    {
        if (!double.IsFinite(zoomPercent) || zoomPercent <= 0 || !double.IsFinite(originX) || !double.IsFinite(originY)
            || !double.IsFinite(left) || !double.IsFinite(top) || !double.IsFinite(right) || !double.IsFinite(bottom))
        {
            return GridLineSet.Empty;
        }

        var scale = zoomPercent / 100;
        var lines = Build(
            spacing,
            zoomPercent,
            CanvasMapping.ToDocument(left, originX, zoomPercent),
            CanvasMapping.ToDocument(top, originY, zoomPercent),
            CanvasMapping.ToDocument(right, originX, zoomPercent),
            CanvasMapping.ToDocument(bottom, originY, zoomPercent));
        return new GridLineSet(
            [.. lines.Vertical.Select(x => originX + x * scale)],
            [.. lines.Horizontal.Select(y => originY + y * scale)]);
    }

    /// <summary>The distance between drawn lines in document units: the spacing times the smallest multiplier that gives at least <see cref="MinPixels"/> on screen.</summary>
    public static double DrawnStep(double spacing, double scale)
    {
        for (var decade = 1.0; decade <= MaxDecade; decade *= 10)
        {
            foreach (var factor in Bases)
            {
                var step = spacing * factor * decade;
                if (step * scale >= MinPixels)
                {
                    return step;
                }
            }
        }

        return spacing * MaxDecade;
    }

    private static List<double> Lines(double step, double from, double to)
    {
        var lines = new List<double>();
        var first = (long)Math.Ceiling(from / step);
        var last = (long)Math.Floor(to / step);
        if (last - first + 1 > MaxLinesPerAxis)
        {
            last = first + MaxLinesPerAxis - 1;
        }

        for (var i = first; i <= last; i++)
        {
            lines.Add(i * step);
        }

        return lines;
    }
}
