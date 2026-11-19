using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using PressToShape.App.Services;
using PressToShape.App.Workspace;

namespace PressToShape.App.Controls;

internal sealed partial class DocumentCanvasControl
{
    // Each line is drawn on its own at the position the scene gives it, so the line breaks are the ones of the document at any zoom. The fill
    // is the text of the line. The stroke is the outline of its glyphs, which a text layout does not draw by itself.
    private static void DrawText(DrawingContext context, ScreenShape shape)
    {
        if (shape.Text is not { Lines.Count: > 0 } text)
        {
            return;
        }

        IBrush? fill = shape.Fill is { } fillColor ? new ImmutableSolidColorBrush(ToAvalonia(fillColor)) : null;
        IPen? pen = shape.Stroke is { } strokeColor
            ? new ImmutablePen(new ImmutableSolidColorBrush(ToAvalonia(strokeColor)), shape.StrokeWidth)
            : null;
        if (fill is null && pen is null)
        {
            return;
        }

        var typeface = TextFonts.TypefaceFor(text.FontFamily, text.IsBold, text.IsItalic);
        foreach (var line in text.Lines)
        {
            if (line.Text.Length == 0)
            {
                continue;
            }

            using var layout = new TextLayout(
                line.Text,
                typeface,
                text.FontSize,
                fill,
                TextAlignment.Left,
                TextWrapping.NoWrap,
                TextTrimming.None,
                textDecorations: null,
                FlowDirection.LeftToRight,
                maxWidth: double.PositiveInfinity,
                maxHeight: double.PositiveInfinity,
                lineHeight: double.NaN,
                text.LetterSpacing,
                maxLines: 0,
                fontFeatures: null,
                textStyleOverrides: null);
            var top = new Point(shape.X + line.X, shape.Y + line.Baseline - layout.TextLines[0].Baseline);
            if (fill is not null)
            {
                layout.Draw(context, top);
            }

            if (pen is not null)
            {
                DrawGlyphOutlines(context, layout.TextLines[0], top, pen);
            }
        }
    }

    // The geometry of a glyph run has its baseline in the coordinates of the line, so it is moved by the top left corner of the line and by
    // the width of the runs before it.
    private static void DrawGlyphOutlines(DrawingContext context, TextLine line, Point top, IPen pen)
    {
        var x = top.X;
        foreach (var run in line.TextRuns)
        {
            if (run is not ShapedTextRun { GlyphRun: { } glyphRun })
            {
                continue;
            }

            using (context.PushTransform(Matrix.CreateTranslation(x, top.Y)))
            {
                context.DrawGeometry(null, pen, glyphRun.BuildGeometry());
            }

            x += glyphRun.Bounds.Width;
        }
    }
}
