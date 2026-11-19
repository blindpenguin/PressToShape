using System;
using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Services;

/// <summary>
/// Lays out a text with Avalonia's text engine (see <see cref="AvaloniaTextLayouts"/>). Point text is not wrapped, area text wraps at the
/// width of its box. A font family that is not installed gets the fallback font of the platform. If the engine cannot lay the text out, the
/// result is empty and the document is not touched.
/// </summary>
internal sealed class AvaloniaTextMeasurer : ITextMeasurer
{
    private static readonly TextLayoutResult Empty = new([], 0, 0);

    public TextLayoutResult Layout(TextShape text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Text.Length == 0)
        {
            return Empty;
        }

        try
        {
            return Measure(text);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or NotSupportedException)
        {
            // The text engine rejected the font or the text. Drawing the text without lines is better than failing the whole canvas.
            return Empty;
        }
    }

    private static TextLayoutResult Measure(TextShape text)
    {
        var layout = AvaloniaTextLayouts.Build(text);
        List<LaidOutLine> lines = new(layout.TextLines.Count);
        var top = 0.0;
        foreach (var line in layout.TextLines)
        {
            var length = Math.Max(0, line.Length - line.NewLineLength);
            lines.Add(new LaidOutLine(text.Text.Substring(line.FirstTextSourceIndex, length), line.Start, top + line.Baseline));
            top += line.Height;
        }

        return new TextLayoutResult(lines, layout.Width, top);
    }
}
