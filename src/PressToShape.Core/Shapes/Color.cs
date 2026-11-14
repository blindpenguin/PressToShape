namespace PressToShape.Core.Shapes;

/// <summary>An sRGB color with 8 bits per channel. Alpha 255 is fully opaque.</summary>
/// <param name="R">Red channel, 0 to 255.</param>
/// <param name="G">Green channel, 0 to 255.</param>
/// <param name="B">Blue channel, 0 to 255.</param>
/// <param name="A">Alpha channel, 0 (transparent) to 255 (opaque).</param>
public readonly record struct Color(byte R, byte G, byte B, byte A)
{
    /// <summary>Creates a fully opaque color from red, green and blue.</summary>
    public static Color FromRgb(byte r, byte g, byte b) => new(r, g, b, 255);
}
