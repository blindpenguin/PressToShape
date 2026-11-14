using Material.Icons;

namespace PressToShape.App.Workspace;

/// <summary>Display data for one tool: name, optional single-key shortcut and icon.</summary>
internal sealed record ToolDefinition(Tool Tool, string Name, char? ShortcutKey, MaterialIconKind Icon)
{
    /// <summary>Tooltip text such as "Select (V)", or just the name when there is no shortcut.</summary>
    public string ToolTip => ShortcutKey is { } key ? $"{Name} ({char.ToUpperInvariant(key)})" : Name;
}
