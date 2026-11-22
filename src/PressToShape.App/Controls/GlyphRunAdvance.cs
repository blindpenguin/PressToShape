using Avalonia.Media;

namespace PressToShape.App.Controls;

/// <summary>How far a glyph run moves the pen. It is the sum of the glyph advances, not the width of the ink (<see cref="GlyphRun.Bounds"/>).</summary>
internal static class GlyphRunAdvance
{
    /// <summary>The advance of all glyphs of the run, with the letter spacing that shaping put into them. 0 for a run without glyphs.</summary>
    public static double Of(GlyphRun glyphRun)
    {
        var advance = 0.0;
        foreach (var glyph in glyphRun.GlyphInfos)
        {
            advance += glyph.GlyphAdvance;
        }

        return advance;
    }
}
