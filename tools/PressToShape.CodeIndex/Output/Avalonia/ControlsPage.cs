using System;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Avalonia.Model;

namespace PressToShape.CodeIndex.Output.Avalonia;

/// <summary>Custom controls with their Avalonia properties and routed events. A section without entries says "none found".</summary>
internal static class ControlsPage
{
    public static string Render(AvaloniaFacts facts)
    {
        var text = new StringBuilder("# Custom controls\n\n");
        text.Append("Classes that derive from `Avalonia.Controls.Control` and have no XAML file, read from their registration calls.\n");
        if (facts.Controls.Count == 0)
        {
            return text.Append("\nnone found\n").ToString();
        }

        foreach (var control in facts.Controls.OrderBy(c => c.Id, StringComparer.Ordinal))
        {
            text.Append("\n## ").Append(control.Name).Append("\n\n");
            text.Append('`').Append(control.Id).Append("` at ").Append(control.File).Append(':').Append(control.Line);
            text.Append(control.BaseType is null ? string.Empty : $", base `{control.BaseType}`").Append(".\n");
            text.Append(PageText.Summary(control.Summary)).Append("\n\n");

            Section(text, "Properties", control.Properties.OrderBy(p => p.Name, StringComparer.Ordinal).Select(p =>
                $"`{p.Name}` ({p.PropertyType}, `{p.ValueType}`, default {p.Default ?? "not set"}{(p.HasClrWrapper ? string.Empty : ", no CLR wrapper")}) at line {p.Line}"));
            Section(text, "Events", control.Events.OrderBy(e => e.Name, StringComparer.Ordinal).Select(e =>
                $"`{e.Name}` ({e.ArgumentType ?? "no argument type"}{(e.Routing is null ? string.Empty : ", " + e.Routing)}) at line {e.Line}"));
        }

        return text.ToString();
    }

    private static void Section(StringBuilder text, string title, System.Collections.Generic.IEnumerable<string> items)
    {
        var list = items.ToList();
        text.Append("**").Append(title).Append("**\n\n");
        text.Append(list.Count == 0 ? "none found\n\n" : string.Concat(list.Select(i => $"- {i}\n")) + "\n");
    }
}
