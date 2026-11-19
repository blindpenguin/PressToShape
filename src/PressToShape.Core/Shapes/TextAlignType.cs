namespace PressToShape.Core.Shapes;

/// <summary>How the lines of a text are aligned. For point text the lines align to each other, for area text to the box.</summary>
public enum TextAlignType
{
    /// <summary>Lines start at the left edge. The default.</summary>
    Left,

    /// <summary>Lines are centered.</summary>
    Center,

    /// <summary>Lines end at the right edge.</summary>
    Right,
}
