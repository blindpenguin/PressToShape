using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// The formatting of the next text that is created: what the user chose last. Editor state, not part of the document.
/// </summary>
/// <param name="FontFamily">The font family name.</param>
/// <param name="FontSize">The font size.</param>
/// <param name="IsBold">Whether the font is bold.</param>
/// <param name="IsItalic">Whether the font is italic.</param>
/// <param name="Alignment">The alignment of the lines.</param>
/// <param name="LineSpacing">The line spacing multiplier.</param>
/// <param name="LetterSpacing">The letter spacing.</param>
internal sealed record TextDefaults(
    string FontFamily,
    double FontSize,
    bool IsBold,
    bool IsItalic,
    TextAlignType Alignment,
    double LineSpacing,
    double LetterSpacing)
{
    /// <summary>The formatting before the user chose anything: the defaults of <see cref="TextShape"/>.</summary>
    public static TextDefaults Initial { get; } = new(TextShape.DefaultFontFamily, TextShape.DefaultFontSize, false, false, TextAlignType.Left, 1, 0);

    /// <summary>These defaults with the values of the change that are set. Invalid values are not checked here, the text checks them.</summary>
    public TextDefaults With(TextFormatChange change) => this with
    {
        FontFamily = change.FontFamily ?? FontFamily,
        FontSize = change.FontSize ?? FontSize,
        IsBold = change.IsBold ?? IsBold,
        IsItalic = change.IsItalic ?? IsItalic,
        Alignment = change.Alignment ?? Alignment,
        LineSpacing = change.LineSpacing ?? LineSpacing,
        LetterSpacing = change.LetterSpacing ?? LetterSpacing,
    };

    /// <summary>A new text at the position with these formatting values, the given content and appearance.</summary>
    public TextShape NewText(double x, double y, double width, double height, bool isArea, string content, Appearance appearance) => new()
    {
        X = x,
        Y = y,
        Width = width,
        Height = height,
        IsArea = isArea,
        Text = content,
        Appearance = appearance,
        FontFamily = FontFamily,
        FontSize = FontSize,
        IsBold = IsBold,
        IsItalic = IsItalic,
        Alignment = Alignment,
        LineSpacing = LineSpacing,
        LetterSpacing = LetterSpacing,
    };
}
