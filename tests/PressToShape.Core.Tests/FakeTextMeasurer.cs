using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Tests;

/// <summary>
/// A deterministic text engine for tests. Each character is <c>0.6 * FontSize + LetterSpacing</c> wide, a line is
/// <c>FontSize * LineSpacing * 1.2</c> high and the first baseline is one font size below the top. Area text wraps on spaces at the box width
/// (a word longer than the width stays whole on its own line), <c>\n</c> always breaks. Alignment is relative to the box width for area text
/// and to the widest line for point text.
/// </summary>
internal sealed class FakeTextMeasurer : ITextMeasurer
{
    public static double CharWidth(TextShape text) => 0.6 * text.FontSize + text.LetterSpacing;

    public static double LineHeight(TextShape text) => text.FontSize * text.LineSpacing * 1.2;

    public TextLayoutResult Layout(TextShape text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Text.Length == 0)
        {
            return new TextLayoutResult([], 0, 0);
        }

        var charWidth = CharWidth(text);
        var lineHeight = LineHeight(text);
        var texts = new List<string>();
        foreach (var paragraph in text.Text.Split('\n'))
        {
            if (text.IsArea)
            {
                texts.AddRange(Wrap(paragraph, charWidth, text.Width));
            }
            else
            {
                texts.Add(paragraph);
            }
        }

        var widest = texts.Max(line => line.Length * charWidth);
        var reference = text.IsArea ? text.Width : widest;
        List<LaidOutLine> lines = [];
        for (var i = 0; i < texts.Count; i++)
        {
            var lineWidth = texts[i].Length * charWidth;
            var x = text.Alignment switch
            {
                TextAlignType.Center => (reference - lineWidth) / 2,
                TextAlignType.Right => reference - lineWidth,
                _ => 0,
            };
            lines.Add(new LaidOutLine(texts[i], x, text.FontSize + i * lineHeight));
        }

        return new TextLayoutResult(lines, widest, texts.Count * lineHeight);
    }

    private static IEnumerable<string> Wrap(string paragraph, double charWidth, double width)
    {
        var current = string.Empty;
        foreach (var word in paragraph.Split(' '))
        {
            var candidate = current.Length == 0 ? word : current + " " + word;
            if (current.Length > 0 && candidate.Length * charWidth > width)
            {
                yield return current;
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        yield return current;
    }
}

/// <summary>A deterministic outliner for tests: one square contour for each character that is not white space.</summary>
internal sealed class FakeTextOutliner : ITextOutliner
{
    private readonly FakeTextMeasurer _measurer = new();

    public IReadOnlyList<DocumentSubPath>? Outline(TextShape text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var charWidth = FakeTextMeasurer.CharWidth(text);
        List<DocumentSubPath> contours = [];
        foreach (var line in _measurer.Layout(text).Lines)
        {
            for (var i = 0; i < line.Text.Length; i++)
            {
                if (char.IsWhiteSpace(line.Text[i]))
                {
                    continue;
                }

                var left = text.X + line.X + i * charWidth;
                var top = text.Y + line.Baseline - text.FontSize;
                contours.Add(new DocumentSubPath(
                    [
                        new DocumentNode(left, top),
                        new DocumentNode(left + charWidth, top),
                        new DocumentNode(left + charWidth, top + text.FontSize),
                        new DocumentNode(left, top + text.FontSize),
                    ],
                    isClosed: true));
            }
        }

        return contours.Count == 0 ? null : contours;
    }
}
