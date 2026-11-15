using System;
using Avalonia.Controls.Primitives;

namespace PressToShape.App.Controls;

/// <summary>
/// A toggle button for something whose state lives in a view model: a tool in the tool column, or the icon that shows whether a panel is open.
/// Its checked state comes only from the binding. A tap runs the command and does not flip the button by itself, so tapping the active tool again
/// keeps it active (a plain toggle button would show it as off while the tool is still the active one, and would also lose its binding).
/// It looks like a plain toggle button, so the tool styles apply.
/// </summary>
public class ToolToggleButton : ToggleButton
{
    // The theme and the styles of a toggle button apply.
    protected override Type StyleKeyOverride => typeof(ToggleButton);

    protected override void Toggle()
    {
    }
}
