using System.Collections.Generic;
using System.Linq;
using Material.Icons;

namespace PressToShape.App.Workspace;

/// <summary>The ordered list of tools, in the order shown in the mockup, with Illustrator-style shortcuts.</summary>
internal static class ToolDefinitions
{
    public static IReadOnlyList<ToolDefinition> All { get; } =
    [
        new(Tool.Select, "Select", 'V', MaterialIconKind.CursorDefault),
        new(Tool.DirectSelect, "Direct Select", 'A', MaterialIconKind.CursorDefaultOutline),
        new(Tool.Pen, "Pen", 'P', MaterialIconKind.Pen),
        new(Tool.Node, "Node / Curve", 'N', MaterialIconKind.VectorBezier),
        new(Tool.Text, "Text", 'T', MaterialIconKind.FormatText),
        new(Tool.Rectangle, "Rectangle", 'M', MaterialIconKind.SquareRoundedOutline),
        new(Tool.Ellipse, "Ellipse", 'L', MaterialIconKind.EllipseOutline),
        new(Tool.Star, "Star", null, MaterialIconKind.StarOutline),
        new(Tool.Spiral, "Spiral", null, MaterialIconKind.Cyclone),
        new(Tool.Eyedropper, "Eyedropper", 'I', MaterialIconKind.Eyedropper),
        new(Tool.Hand, "Hand", 'H', MaterialIconKind.HandBackLeftOutline),
        new(Tool.Zoom, "Zoom", 'Z', MaterialIconKind.Magnify),
    ];

    public static ToolDefinition Get(Tool tool) => All.First(d => d.Tool == tool);

    /// <summary>Finds the tool for a shortcut key, ignoring case. Returns null for unknown keys.</summary>
    public static ToolDefinition? FindByShortcut(char key)
    {
        var upper = char.ToUpperInvariant(key);
        return All.FirstOrDefault(d => d.ShortcutKey is { } k && char.ToUpperInvariant(k) == upper);
    }
}
