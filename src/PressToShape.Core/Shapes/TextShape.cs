using System;

namespace PressToShape.Core.Shapes;

/// <summary>
/// A text with one font setting for the whole object. Point text (<see cref="IsArea"/> false) grows with its content and its box is
/// the measured extent, which the app writes after each measure. Area text wraps at the width of its box. The model does not measure or
/// wrap, see <c>ITextMeasurer</c>.
/// </summary>
public sealed record TextShape : Shape
{
    /// <summary>The longest text the model accepts, in characters.</summary>
    public const int MaxTextLength = 100_000;

    /// <summary>The longest font family name the model accepts, in characters.</summary>
    public const int MaxFontFamilyLength = 200;

    /// <summary>The smallest font size.</summary>
    public const double MinFontSize = 1;

    /// <summary>The largest font size.</summary>
    public const double MaxFontSize = 2000;

    /// <summary>The smallest line spacing multiplier.</summary>
    public const double MinLineSpacing = 0.5;

    /// <summary>The largest line spacing multiplier.</summary>
    public const double MaxLineSpacing = 5;

    /// <summary>The smallest letter spacing.</summary>
    public const double MinLetterSpacing = -50;

    /// <summary>The largest letter spacing.</summary>
    public const double MaxLetterSpacing = 500;

    /// <summary>The font family of a new text.</summary>
    public const string DefaultFontFamily = "Inter";

    /// <summary>The font size of a new text.</summary>
    public const double DefaultFontSize = 24;

    private readonly string _text = string.Empty;
    private readonly string _fontFamily = DefaultFontFamily;
    private readonly double _fontSize = DefaultFontSize;
    private readonly double _lineSpacing = 1;
    private readonly double _letterSpacing;

    /// <summary>The content. Not null, may be empty, may hold <c>\n</c> as a line break. At most <see cref="MaxTextLength"/> characters.</summary>
    public string Text
    {
        get => _text;
        init
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Text));
            if (value.Length > MaxTextLength)
            {
                throw new ArgumentException($"A text can have at most {MaxTextLength} characters.", nameof(Text));
            }

            _text = value;
        }
    }

    /// <summary>Whether this is area text (wraps at the box width) and not point text.</summary>
    public bool IsArea { get; init; }

    /// <summary>The font family name as the user chose it, kept even when the font is not installed. Not empty.</summary>
    public string FontFamily
    {
        get => _fontFamily;
        init
        {
            ArgumentNullException.ThrowIfNull(value, nameof(FontFamily));
            if (value.Length is 0 or > MaxFontFamilyLength || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"A font family needs from 1 to {MaxFontFamilyLength} characters.", nameof(FontFamily));
            }

            _fontFamily = value;
        }
    }

    /// <summary>Font size in document units, from <see cref="MinFontSize"/> to <see cref="MaxFontSize"/>.</summary>
    public double FontSize
    {
        get => _fontSize;
        init => _fontSize = Guard.FiniteInRange(value, MinFontSize, MaxFontSize, nameof(FontSize));
    }

    /// <summary>Whether the font is bold.</summary>
    public bool IsBold { get; init; }

    /// <summary>Whether the font is italic.</summary>
    public bool IsItalic { get; init; }

    /// <summary>The alignment of the lines.</summary>
    public TextAlignType Alignment { get; init; }

    /// <summary>Line spacing as a multiplier of the font's line height, from <see cref="MinLineSpacing"/> to <see cref="MaxLineSpacing"/>. 1 is the font's own.</summary>
    public double LineSpacing
    {
        get => _lineSpacing;
        init => _lineSpacing = Guard.FiniteInRange(value, MinLineSpacing, MaxLineSpacing, nameof(LineSpacing));
    }

    /// <summary>Extra space after each character in document units, from <see cref="MinLetterSpacing"/> to <see cref="MaxLetterSpacing"/>. 0 is the font's own.</summary>
    public double LetterSpacing
    {
        get => _letterSpacing;
        init => _letterSpacing = Guard.FiniteInRange(value, MinLetterSpacing, MaxLetterSpacing, nameof(LetterSpacing));
    }

    /// <inheritdoc />
    protected override bool ContainsLocal(double x, double y) =>
        x >= X && x <= X + Width && y >= Y && y <= Y + Height;
}
