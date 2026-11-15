using System;
using System.Collections.Generic;

namespace PressToShape.App.Workspace;

/// <summary>A point in screen pixels.</summary>
internal readonly record struct ScreenPoint(double X, double Y);

/// <summary>A line in screen pixels.</summary>
internal readonly record struct ScreenLine(ScreenPoint From, ScreenPoint To);

/// <summary>A closed shape of straight lines in screen pixels, for example the turned outline of a shape. Always four points here.</summary>
internal sealed record ScreenPolygon(IReadOnlyList<ScreenPoint> Points)
{
    /// <summary>The corners of a box turned around its center, in the order top left, top right, bottom right, bottom left.</summary>
    public static ScreenPolygon FromBox(double centerX, double centerY, double width, double height, double rotationDegrees)
    {
        var (sin, cos) = Math.SinCos(rotationDegrees * Math.PI / 180);
        ScreenPoint Corner(double dx, double dy) => new(centerX + dx * cos - dy * sin, centerY + dx * sin + dy * cos);
        var hw = width / 2;
        var hh = height / 2;
        return new ScreenPolygon([Corner(-hw, -hh), Corner(hw, -hh), Corner(hw, hh), Corner(-hw, hh)]);
    }
}
