using System.Collections.Generic;

namespace PressToShape.Core.Shapes;

/// <summary>One laid-out line of a text.</summary>
/// <param name="Text">The characters of the line, without the line break.</param>
/// <param name="X">Start of the line, relative to the left edge of the text's box, in document units. It follows the alignment.</param>
/// <param name="Baseline">Baseline of the line, relative to the top edge of the text's box, in document units.</param>
public readonly record struct LaidOutLine(string Text, double X, double Baseline);

/// <summary>The result of laying out a text.</summary>
/// <param name="Lines">The lines from top to bottom. Empty for an empty text.</param>
/// <param name="Width">Width of the widest line in document units.</param>
/// <param name="Height">Height of all lines together in document units.</param>
public sealed record TextLayoutResult(IReadOnlyList<LaidOutLine> Lines, double Width, double Height);

/// <summary>
/// Measures and wraps a text. The model has no text engine, so the app implements this with its own and tests use a fake.
/// Point text is not wrapped. Area text wraps at the width of its box.
/// </summary>
public interface ITextMeasurer
{
    /// <summary>Lays out the text with its content, font settings, alignment and (for area text) box width.</summary>
    TextLayoutResult Layout(TextShape text);
}
