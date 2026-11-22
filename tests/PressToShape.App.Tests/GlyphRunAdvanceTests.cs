using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using PressToShape.App.Controls;
using PressToShape.App.Services;
using Xunit;

namespace PressToShape.App.Tests;

// The outline of a stroked text is placed run after run. A run is as wide as the sum of the advances of its glyphs, which is not the width of its ink.
[Collection(HeadlessUi.CollectionName)]
public class GlyphRunAdvanceTests
{
    private static GlyphRun Run(params GlyphInfo[] glyphs)
    {
        var typeface = TextFonts.TypefaceFor("Test Font", false, false);
        Assert.True(FontManager.Current.TryGetGlyphTypeface(typeface, out var glyphTypeface));
        return new GlyphRun(glyphTypeface, 20, new string('a', glyphs.Length).AsMemory(), glyphs);
    }

    [Fact]
    public void Of_GlyphsWithAdvances_IsTheirSum() => HeadlessUi.Run(() =>
    {
        using var run = Run(new GlyphInfo(1, 0, 10), new GlyphInfo(2, 1, 12.5), new GlyphInfo(3, 2, 7.25));

        Assert.Equal(29.75, GlyphRunAdvance.Of(run), 9);
    });

    [Fact]
    public void Of_AdvancesWithLetterSpacing_IncludesTheSpacing() => HeadlessUi.Run(() =>
    {
        // A letter spacing of 3 is part of the shaped advance of every glyph.
        using var run = Run(new GlyphInfo(1, 0, 13), new GlyphInfo(2, 1, 15));

        Assert.Equal(28, GlyphRunAdvance.Of(run), 9);
    });

    [Fact]
    public void Of_GlyphOffsets_DoNotChangeTheAdvance() => HeadlessUi.Run(() =>
    {
        using var run = Run(new GlyphInfo(1, 0, 10, new Vector(4, -3)), new GlyphInfo(2, 1, 10, new Vector(-2, 5)));

        Assert.Equal(20, GlyphRunAdvance.Of(run), 9);
    });

    [Fact]
    public void Of_RunWithoutGlyphs_IsZero() => HeadlessUi.Run(() =>
    {
        using var run = Run();

        Assert.Equal(0, GlyphRunAdvance.Of(run));
    });

    [Fact]
    public void Of_ZeroAdvanceGlyphs_IsZero() => HeadlessUi.Run(() =>
    {
        using var run = Run(new GlyphInfo(1, 0, 0), new GlyphInfo(2, 1, 0));

        Assert.Equal(0, GlyphRunAdvance.Of(run));
    });
}
