using System.Collections.Generic;

namespace PressToShape.Core.Shapes;

/// <summary>Turns a text into the closed contours of its glyphs. The app implements this with its font engine, tests use a fake.</summary>
public interface ITextOutliner
{
    /// <summary>
    /// The closed glyph contours of the text in document coordinates (the position of the text's box is included, the rotation is not).
    /// Returns <c>null</c> when the text has no outlines: it is empty or blank, or the font cannot give them.
    /// </summary>
    IReadOnlyList<DocumentSubPath>? Outline(TextShape text);
}
