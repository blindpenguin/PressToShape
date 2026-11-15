using System;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Output.Avalonia;

/// <summary>Value converters with the XAML places that use them. A converter that no XAML file uses is marked "not used".</summary>
internal static class ConvertersPage
{
    public static string Render(AvaloniaFacts facts, IndexAnalysis analysis)
    {
        var text = new StringBuilder("# Value converters\n\n");
        text.Append("Classes that implement `IValueConverter` or `IMultiValueConverter`. Uses are the XAML bindings that name the converter.\n");
        if (facts.Converters.Count == 0)
        {
            return text.Append("\nnone found\n").ToString();
        }

        foreach (var converter in facts.Converters.OrderBy(c => c.Id, StringComparer.Ordinal))
        {
            var uses = analysis.View.Relationships
                .Where(e => e.Type == RelationshipType.UsesConverter && e.ToId == converter.Id)
                .Select(e => e.File is null ? e.FromId : $"{e.File}:{e.Line}")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(u => u, StringComparer.Ordinal)
                .ToList();
            text.Append("\n## ").Append(converter.Name).Append("\n\n");
            text.Append('`').Append(converter.Id).Append("` at ").Append(converter.File).Append(':').Append(converter.Line);
            text.Append(", implements ").Append(string.Join(", ", converter.Interfaces.OrderBy(i => i, StringComparer.Ordinal).Select(i => $"`{i}`"))).Append(".\n");
            text.Append(PageText.Summary(converter.Summary)).Append("\n\n");
            text.Append(uses.Count == 0 ? "not used\n" : string.Concat(uses.Select(u => $"- used at {u}\n")));
        }

        return text.ToString();
    }
}
