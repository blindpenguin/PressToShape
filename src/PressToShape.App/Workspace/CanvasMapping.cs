namespace PressToShape.App.Workspace;

/// <summary>
/// The one mapping between screen pixels and document units. Drawing and picking both use it,
/// so a shape is always hit where it is drawn.
/// </summary>
internal static class CanvasMapping
{
    /// <summary>Converts a screen coordinate to a document coordinate.</summary>
    /// <param name="screen">Position in screen pixels, in the same space as <paramref name="origin"/>.</param>
    /// <param name="origin">Screen position of the artboard's edge on this axis.</param>
    /// <param name="zoomPercent">Zoom in percent. 100 means one document unit per screen pixel.</param>
    public static double ToDocument(double screen, double origin, double zoomPercent) =>
        (screen - origin) / (zoomPercent / 100);

    /// <summary>Converts a document coordinate to a screen coordinate.</summary>
    public static double ToScreen(double value, double origin, double zoomPercent) =>
        origin + value * zoomPercent / 100;
}
