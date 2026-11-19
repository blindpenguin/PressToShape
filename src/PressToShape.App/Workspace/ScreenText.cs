using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>One line of a text ready to draw. Positions are in screen pixels from the top left corner of the text's box.</summary>
/// <param name="Text">The characters of the line.</param>
/// <param name="X">Where the line starts, from the left edge of the box. It follows the alignment.</param>
/// <param name="Baseline">The baseline of the line, from the top edge of the box.</param>
internal readonly record struct ScreenTextLine(string Text, double X, double Baseline);

/// <summary>A text ready to draw: its lines and the font, with sizes already in screen pixels.</summary>
/// <param name="Lines">The lines from top to bottom. Empty for an empty text.</param>
/// <param name="FontFamily">The font family name of the document, see <c>TextFonts</c>.</param>
/// <param name="FontSize">The font size in screen pixels.</param>
/// <param name="IsBold">Whether the font is bold.</param>
/// <param name="IsItalic">Whether the font is italic.</param>
/// <param name="LetterSpacing">The letter spacing in screen pixels.</param>
/// <param name="Alignment">How the lines are aligned. The line positions follow it already, the editor needs it too.</param>
/// <param name="IsArea">Whether the text wraps at the width of its box.</param>
internal sealed record ScreenText(
    IReadOnlyList<ScreenTextLine> Lines,
    string FontFamily,
    double FontSize,
    bool IsBold,
    bool IsItalic,
    double LetterSpacing,
    TextAlignType Alignment = TextAlignType.Left,
    bool IsArea = false);
