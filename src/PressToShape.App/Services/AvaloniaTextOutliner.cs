using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using PressToShape.Core.Shapes;
using SkiaSharp;

namespace PressToShape.App.Services;

/// <summary>
/// Gives the glyph contours of a text. Avalonia has no public way to read the contours of a geometry, so this takes the glyphs that Avalonia
/// shaped for the canvas (their ids and positions, see <see cref="AvaloniaTextLayouts"/>) and reads the outline of each glyph from the font
/// file with SkiaSharp. The outlines are exactly where the canvas draws the glyphs.
/// </summary>
/// <remarks>
/// A font collection file (TTC) is read as its first font, so glyphs of the other fonts of the collection come out wrong. Contours that
/// overlap are not merged, the compound path is filled even-odd.
/// </remarks>
internal sealed class AvaloniaTextOutliner : ITextOutliner
{
    // The slant Avalonia gives a font without an italic face. It was checked against the bounds of Avalonia's own text geometry.
    private const float ObliqueSkew = -0.3f;

    public IReadOnlyList<DocumentSubPath>? Outline(TextShape text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrWhiteSpace(text.Text))
        {
            return null;
        }

        try
        {
            var contours = Read(text);
            return contours.Count == 0 ? null : contours;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or NotSupportedException or IOException)
        {
            // The font cannot give its outlines (no file behind it, or a file Skia cannot read), so the text cannot be converted.
            return null;
        }
    }

    private static List<DocumentSubPath> Read(TextShape text)
    {
        var layout = AvaloniaTextLayouts.Build(text);
        List<DocumentSubPath> contours = [];
        using var fonts = new FontCache();
        var top = 0.0;
        foreach (var line in layout.TextLines)
        {
            var penX = text.X + line.Start;
            var baseline = text.Y + top + line.Baseline;
            foreach (var run in line.TextRuns)
            {
                if (run is ShapedTextRun { GlyphRun: { } glyphRun })
                {
                    var font = fonts.For(glyphRun);
                    foreach (var glyph in glyphRun.GlyphInfos)
                    {
                        using var path = font.GetGlyphPath(glyph.GlyphIndex);
                        if (path is not null)
                        {
                            contours.AddRange(GlyphOutlines.FromPath(path, penX + glyph.GlyphOffset.X, baseline + glyph.GlyphOffset.Y));
                        }

                        penX += glyph.GlyphAdvance;
                    }
                }
            }

            top += line.Height;
        }

        return contours;
    }

    // The Skia fonts of the glyph runs of one text, made once for each Avalonia typeface and size, and disposed together.
    private sealed class FontCache : IDisposable
    {
        private readonly Dictionary<(IPlatformTypeface Typeface, double Size), SKFont> _fonts = [];
        private readonly List<SKTypeface> _typefaces = [];

        public SKFont For(GlyphRun glyphRun)
        {
            var platform = glyphRun.GlyphTypeface.PlatformTypeface;
            if (_fonts.TryGetValue((platform, glyphRun.FontRenderingEmSize), out var cached))
            {
                return cached;
            }

            // The font file is copied: SkiaSharp may dispose the stream it is given, and the stream belongs to Avalonia.
            if (!platform.TryGetStream(out var stream))
            {
                throw new InvalidOperationException("The font has no file.");
            }

            byte[] bytes;
            using (stream)
            using (var copy = new MemoryStream())
            {
                stream.CopyTo(copy);
                bytes = copy.ToArray();
            }

            var typeface = SKTypeface.FromData(SKData.CreateCopy(bytes)) ?? throw new InvalidOperationException("Skia cannot read the font.");
            _typefaces.Add(typeface);
            var simulations = glyphRun.GlyphTypeface.FontSimulations;
            var font = new SKFont(typeface, (float)glyphRun.FontRenderingEmSize)
            {
                Hinting = SKFontHinting.None,
                Subpixel = true,
                Embolden = simulations.HasFlag(FontSimulations.Bold),
                SkewX = simulations.HasFlag(FontSimulations.Oblique) ? ObliqueSkew : 0,
            };
            _fonts[(platform, glyphRun.FontRenderingEmSize)] = font;
            return font;
        }

        public void Dispose()
        {
            foreach (var font in _fonts.Values)
            {
                font.Dispose();
            }

            foreach (var typeface in _typefaces)
            {
                typeface.Dispose();
            }
        }
    }
}
