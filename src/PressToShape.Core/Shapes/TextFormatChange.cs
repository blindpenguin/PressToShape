namespace PressToShape.Core.Shapes;

/// <summary>
/// A change of the formatting of texts. Each value that is set replaces the one of the text, a value that is <c>null</c> leaves it as it is.
/// </summary>
/// <param name="FontFamily">The font family name, or <c>null</c>.</param>
/// <param name="FontSize">The font size, or <c>null</c>.</param>
/// <param name="IsBold">Bold on or off, or <c>null</c>.</param>
/// <param name="IsItalic">Italic on or off, or <c>null</c>.</param>
/// <param name="Alignment">The alignment, or <c>null</c>.</param>
/// <param name="LineSpacing">The line spacing multiplier, or <c>null</c>.</param>
/// <param name="LetterSpacing">The letter spacing, or <c>null</c>.</param>
public sealed record TextFormatChange(
    string? FontFamily = null,
    double? FontSize = null,
    bool? IsBold = null,
    bool? IsItalic = null,
    TextAlignType? Alignment = null,
    double? LineSpacing = null,
    double? LetterSpacing = null)
{
    /// <summary>The text with the values of this change. The same instance if nothing differs. Invalid values throw like on the text itself.</summary>
    public TextShape ApplyTo(TextShape text)
    {
        System.ArgumentNullException.ThrowIfNull(text);

        return text with
        {
            FontFamily = FontFamily ?? text.FontFamily,
            FontSize = FontSize ?? text.FontSize,
            IsBold = IsBold ?? text.IsBold,
            IsItalic = IsItalic ?? text.IsItalic,
            Alignment = Alignment ?? text.Alignment,
            LineSpacing = LineSpacing ?? text.LineSpacing,
            LetterSpacing = LetterSpacing ?? text.LetterSpacing,
        };
    }
}
