using System;

namespace PressToShape.App.Workspace;

/// <summary>Which sections of the Properties tab are relevant for a tool.</summary>
internal static class PropertiesSections
{
    /// <summary>
    /// The sections to show for <paramref name="tool"/>. A rule and not a table. The throwing arm only covers values
    /// outside the enum (the compiler asks for it). A test covers every <see cref="Tool"/> value, so a new tool fails there until it is placed here.
    /// </summary>
    public static PropertiesSection For(Tool tool) => tool switch
    {
        Tool.Select or Tool.DirectSelect =>
            PropertiesSection.Transform | PropertiesSection.Appearance | PropertiesSection.Align | PropertiesSection.QuickActions,
        Tool.Rectangle or Tool.Ellipse or Tool.Star or Tool.Spiral or Tool.Text => PropertiesSection.Appearance,
        Tool.Pen or Tool.Node => PropertiesSection.Appearance | PropertiesSection.Path,
        Tool.Eyedropper or Tool.Hand or Tool.Zoom => PropertiesSection.None,
        _ => throw new ArgumentOutOfRangeException(nameof(tool), tool, null),
    };
}
