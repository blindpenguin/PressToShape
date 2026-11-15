using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Reads the markup extension syntax of Avalonia XAML. It is a small scanner: names, positional and named arguments,
/// quoted text, nested extensions, backslash escapes, and brackets in paths such as <c>$parent[ItemsControl,2].Path</c>.
/// It never throws. Input that it does not understand, that is too long or that nests too deeply gives <c>false</c>.
/// </summary>
internal static class MarkupExtensionParser
{
    private const int MaxDepth = 16;
    private const int MaxLength = 10_000;

    public static bool TryParse(string? text, [NotNullWhen(true)] out MarkupExtension? extension)
    {
        extension = null;
        if (text is null || text.Length > MaxLength)
        {
            return false;
        }

        var scanner = new Scanner(text.Trim());
        if (!scanner.AtOpeningBrace)
        {
            return false;
        }

        var parsed = scanner.ParseExtension(0);
        if (parsed is null || !scanner.AtEnd)
        {
            return false;
        }

        extension = parsed;
        return true;
    }

    private sealed class Scanner
    {
        private readonly string _text;
        private int _position;

        public Scanner(string text) => _text = text;

        public bool AtOpeningBrace => _text.Length > 0 && _text[0] == '{' && !(_text.Length > 1 && _text[1] == '}');

        public bool AtEnd => _position >= _text.Length;

        public MarkupExtension? ParseExtension(int depth)
        {
            if (depth > MaxDepth)
            {
                return null;
            }

            _position++;
            SkipWhitespace();
            var name = ReadName();
            if (name.Length == 0)
            {
                return null;
            }

            var positional = new List<MarkupValue>();
            var named = new Dictionary<string, MarkupValue>(StringComparer.Ordinal);
            while (true)
            {
                SkipWhitespace();
                if (AtEnd)
                {
                    return null;
                }

                switch (_text[_position])
                {
                    case '}':
                        _position++;
                        return new MarkupExtension(name, positional, named);
                    case ',':
                        _position++;
                        continue;
                }

                if (!ParseArgument(depth, positional, named))
                {
                    return null;
                }

                SkipWhitespace();
                if (AtEnd || _text[_position] is not (',' or '}'))
                {
                    return null;
                }
            }
        }

        private bool ParseArgument(int depth, List<MarkupValue> positional, Dictionary<string, MarkupValue> named)
        {
            var first = _text[_position];
            if (first is '{' or '\'' or '"')
            {
                var value = ParseValue(depth);
                if (value is null)
                {
                    return false;
                }

                positional.Add(value);
                return true;
            }

            var token = ReadRaw(stopAtEquals: true);
            if (!AtEnd && _text[_position] == '=')
            {
                var key = token.Trim();
                if (!IsKey(key))
                {
                    return false;
                }

                _position++;
                SkipWhitespace();
                var value = ParseValue(depth);
                if (value is null)
                {
                    return false;
                }

                named[key] = value;
                return true;
            }

            var text = token.Trim();
            if (text.Length > 0)
            {
                positional.Add(new MarkupText(text));
            }

            return true;
        }

        private MarkupValue? ParseValue(int depth)
        {
            if (AtEnd)
            {
                return null;
            }

            switch (_text[_position])
            {
                case '{':
                    return ParseExtension(depth + 1) is { } nested ? new MarkupNested(nested) : null;
                case '\'' or '"':
                    return ReadQuoted() is { } quoted ? new MarkupText(quoted) : null;
                default:
                    return new MarkupText(ReadRaw(stopAtEquals: false).Trim());
            }
        }

        private string ReadName()
        {
            var start = _position;
            while (!AtEnd && !char.IsWhiteSpace(_text[_position]) && _text[_position] is not (',' or '}' or '{'))
            {
                _position++;
            }

            return _text[start.._position];
        }

        /// <summary>Reads up to a top-level comma, closing brace or (optionally) equals sign. Commas inside brackets and parentheses do not end the token.</summary>
        private string ReadRaw(bool stopAtEquals)
        {
            var text = new StringBuilder();
            var brackets = 0;
            while (!AtEnd)
            {
                var c = _text[_position];
                if (c == '\\' && _position + 1 < _text.Length)
                {
                    text.Append(_text[_position + 1]);
                    _position += 2;
                    continue;
                }

                if (brackets == 0 && (c is ',' or '}' || (stopAtEquals && c == '=')))
                {
                    break;
                }

                if (c is '[' or '(')
                {
                    brackets++;
                }
                else if (c is ']' or ')' && brackets > 0)
                {
                    brackets--;
                }

                text.Append(c);
                _position++;
            }

            return text.ToString();
        }

        private string? ReadQuoted()
        {
            var quote = _text[_position++];
            var text = new StringBuilder();
            while (!AtEnd)
            {
                var c = _text[_position++];
                if (c == '\\' && !AtEnd)
                {
                    text.Append(_text[_position++]);
                }
                else if (c == quote)
                {
                    return text.ToString();
                }
                else
                {
                    text.Append(c);
                }
            }

            return null;
        }

        private void SkipWhitespace()
        {
            while (!AtEnd && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
            }
        }

        private static bool IsKey(string key)
        {
            if (key.Length == 0)
            {
                return false;
            }

            foreach (var c in key)
            {
                if (!char.IsLetterOrDigit(c) && c is not ('_' or '.' or ':'))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
