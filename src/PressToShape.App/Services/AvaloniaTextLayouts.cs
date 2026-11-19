using System;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Services;

/// <summary>
/// Builds the Avalonia layout of a text. The measurer reads the lines from it, the outliner reads the glyphs, so both see the same layout
/// as the canvas.
/// </summary>
internal static class AvaloniaTextLayouts
{
    // The spacing multiplier scales the line height of the font itself, so that is measured first. Avalonia aligns the lines inside the
    // maximum width and ignores the alignment without one, so an aligned point text is laid out again with its own widest line as the width.
    public static TextLayout Build(TextShape text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var wraps = text.IsArea && text.Width > 0;
        var layout = Create(text, double.NaN, wraps ? text.Width : double.PositiveInfinity);
        var lineHeight = text.LineSpacing != 1 && layout.TextLines.Count > 0 ? layout.TextLines[0].Height * text.LineSpacing : double.NaN;
        var alignsToWidest = !wraps && text.Alignment != TextAlignType.Left;
        if (!double.IsNaN(lineHeight) || alignsToWidest)
        {
            layout = Create(text, lineHeight, wraps ? text.Width : alignsToWidest ? layout.Width : double.PositiveInfinity);
        }

        return layout;
    }

    private static TextLayout Create(TextShape text, double lineHeight, double maxWidth)
    {
        var typeface = TextFonts.TypefaceFor(text.FontFamily, text.IsBold, text.IsItalic);
        var alignment = text.Alignment switch
        {
            TextAlignType.Center => TextAlignment.Center,
            TextAlignType.Right => TextAlignment.Right,
            _ => TextAlignment.Left,
        };

        return new TextLayout(
            text.Text,
            typeface,
            text.FontSize,
            foreground: null,
            alignment,
            text.IsArea && text.Width > 0 ? TextWrapping.Wrap : TextWrapping.NoWrap,
            TextTrimming.None,
            textDecorations: null,
            FlowDirection.LeftToRight,
            maxWidth,
            maxHeight: double.PositiveInfinity,
            lineHeight,
            text.LetterSpacing,
            maxLines: 0,
            fontFeatures: null,
            textStyleOverrides: null);
    }
}
