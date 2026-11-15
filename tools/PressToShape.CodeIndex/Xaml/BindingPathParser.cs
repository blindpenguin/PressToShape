using System;
using System.Collections.Generic;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Splits an Avalonia binding path into its source and property segments. It never throws.</summary>
internal static class BindingPathParser
{
    public static BindingPath Parse(string? path)
    {
        var text = (path ?? string.Empty).Trim();
        var negations = 0;
        while (text.StartsWith('!'))
        {
            negations++;
            text = text[1..].TrimStart();
        }

        var source = BindingSource.DataContext;
        string? sourceText = null;
        if (text.StartsWith("$self", StringComparison.Ordinal))
        {
            source = BindingSource.Self;
            text = text["$self".Length..];
        }
        else if (text.StartsWith("$parent", StringComparison.Ordinal))
        {
            source = BindingSource.Parent;
            text = text["$parent".Length..];
            if (text.StartsWith('['))
            {
                var close = text.IndexOf(']', StringComparison.Ordinal);
                if (close < 0)
                {
                    return new BindingPath(source, null, [], "an unclosed [ after $parent", negations);
                }

                sourceText = text[1..close].Trim();
                text = text[(close + 1)..];
            }
        }
        else if (text.StartsWith('#'))
        {
            source = BindingSource.ElementName;
            var end = 1;
            while (end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] == '_'))
            {
                end++;
            }

            sourceText = text[1..end];
            text = text[end..];
        }

        if (text.StartsWith('.'))
        {
            text = text[1..];
        }

        return ReadSegments(source, sourceText, text, negations);
    }

    private static BindingPath ReadSegments(BindingSource source, string? sourceText, string text, int negations)
    {
        var segments = new List<string>();
        var position = 0;
        while (position < text.Length)
        {
            if (text[position] == '(')
            {
                return Stopped(source, sourceText, segments, negations, $"an attached property or cast {text[position..]}");
            }

            var start = position;
            while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_'))
            {
                position++;
            }

            if (position == start)
            {
                return Stopped(source, sourceText, segments, negations, $"the unexpected text '{text[position..]}'");
            }

            segments.Add(text[start..position]);
            if (position == text.Length)
            {
                break;
            }

            switch (text[position])
            {
                case '.':
                    position++;
                    break;
                case '[':
                    return Stopped(source, sourceText, segments, negations, $"an indexer {text[position..]}");
                case '^':
                    return Stopped(source, sourceText, segments, negations, "the stream operator ^");
                default:
                    return Stopped(source, sourceText, segments, negations, $"the unexpected text '{text[position..]}'");
            }
        }

        return new BindingPath(source, sourceText, segments, null, negations);
    }

    private static BindingPath Stopped(BindingSource source, string? sourceText, List<string> segments, int negations, string what) =>
        new(source, sourceText, segments, what, negations);
}
