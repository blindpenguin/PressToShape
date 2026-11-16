using System.Collections.Generic;
using System.Linq;
using Material.Icons;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.Workspace;

/// <summary>The ordered list of tools, in the order shown in the mockup, with Illustrator-style shortcuts.</summary>
internal static class ToolDefinitions
{
    public static IReadOnlyList<ToolDefinition> All { get; } =
    [
        new(Tool.Select, N("Select"), 'V', MaterialIconKind.CursorDefault),
        new(Tool.DirectSelect, N("Direct Select"), 'A', MaterialIconKind.CursorDefaultOutline),
        new(Tool.Pen, N("Pen"), 'P', MaterialIconKind.Pen),
        new(Tool.Node, N("Node / Curve"), 'N', MaterialIconKind.VectorBezier),
        new(Tool.Text, N("Text"), 'T', MaterialIconKind.FormatText),
        new(Tool.Rectangle, N("Rectangle"), 'M', MaterialIconKind.SquareRoundedOutline),
        new(Tool.Ellipse, N("Ellipse"), 'L', MaterialIconKind.EllipseOutline),
        new(Tool.Eyedropper, N("Eyedropper"), 'I', MaterialIconKind.Eyedropper),
        new(Tool.Hand, N("Hand"), 'H', MaterialIconKind.HandBackLeftOutline),
        new(Tool.Zoom, N("Zoom"), 'Z', MaterialIconKind.Magnify),
    ];

    public static ToolDefinition Get(Tool tool) => All.First(d => d.Tool == tool);

    /// <summary>Finds the tool for a shortcut key, ignoring case. Returns null for unknown keys.</summary>
    public static ToolDefinition? FindByShortcut(char key)
    {
        var upper = char.ToUpperInvariant(key);
        return All.FirstOrDefault(d => d.ShortcutKey is { } k && char.ToUpperInvariant(k) == upper);
    }
}
