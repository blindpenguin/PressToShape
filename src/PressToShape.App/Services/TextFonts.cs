using System;
using System.Collections.Concurrent;
using Avalonia.Media;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Services;

/// <summary>Turns the font settings of a text into the typeface Avalonia draws with.</summary>
internal static class TextFonts
{
    // The default font of a new text is the bundled Inter. Avalonia finds it only by its resource name, not by the name "Inter" (which
    // would give the platform's fallback font), so the default name is mapped here, for the measuring, the drawing and the editor alike.
    private const string BundledInter = "fonts:Inter#Inter";

    // The fonts are registered when the application starts, so the answer does not change. Where the bundled font is not registered (a test
    // application), asking for it would throw while text is laid out, so the plain name is used then.
    private static readonly Lazy<bool> BundledAvailable = new(() => FontManager.Current.TryGetGlyphTypeface(new Typeface(BundledInter), out _));

    private static readonly ConcurrentDictionary<(string Name, bool Bold, bool Italic), bool> Usable = new();

    /// <summary>
    /// The typeface for the font settings. A font that is on the system but that Avalonia cannot use (for example a Type 1 font such as
    /// "Courier 10 Pitch") would throw when text is laid out or drawn, so the default font is used for it instead. The name in the document
    /// is not changed.
    /// </summary>
    public static Typeface TypefaceFor(string name, bool bold, bool italic)
    {
        var family = IsUsable(name, bold, italic) ? FamilyFor(name) : FamilyFor(TextShape.DefaultFontFamily);
        return new Typeface(family, italic ? FontStyle.Italic : FontStyle.Normal, bold ? FontWeight.Bold : FontWeight.Normal);
    }

    /// <summary>Whether Avalonia can make a glyph typeface for the font in this style. The answer is remembered.</summary>
    public static bool IsUsable(string name, bool bold, bool italic) =>
        Usable.GetOrAdd((name, bold, italic), key =>
        {
            try
            {
                var typeface = new Typeface(FamilyFor(key.Name), key.Italic ? FontStyle.Italic : FontStyle.Normal, key.Bold ? FontWeight.Bold : FontWeight.Normal);
                return FontManager.Current.TryGetGlyphTypeface(typeface, out _);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or NotSupportedException)
            {
                return false;
            }
        });

    public static FontFamily FamilyFor(string name) => FamilyFor(name, BundledAvailable.Value);

    /// <summary>The family for the name, given whether the bundled font is registered.</summary>
    internal static FontFamily FamilyFor(string name, bool bundledAvailable)
    {
        ArgumentNullException.ThrowIfNull(name);

        return bundledAvailable && string.Equals(name, TextShape.DefaultFontFamily, StringComparison.OrdinalIgnoreCase)
            ? new FontFamily(BundledInter)
            : new FontFamily(name);
    }
}
