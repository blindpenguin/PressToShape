using System;

namespace PressToShape.App.Workspace;

/// <summary>
/// The math of the view over the drawing, on plain numbers in screen pixels: where the artboard is, how far the view may be moved,
/// and what a zoom around a point does. The view offset (pan) is the distance of the artboard from its centered position.
/// Non-finite input (devices can send it) never produces NaN or infinity.
/// </summary>
internal static class ViewGeometry
{
    /// <summary>The part of the artboard, in screen pixels, that must stay visible on each axis.</summary>
    public const double MinimumVisible = 100;

    /// <summary>The distance the view moves for a wheel delta of 1, in screen pixels.</summary>
    public const double WheelStep = 48;

    /// <summary>The zoom factor for a wheel delta of 1.</summary>
    public const double WheelZoomFactor = 1.1;

    /// <summary>The screen position of the artboard's edge on one axis. The artboard is centered, then moved by the offset.</summary>
    public static double Origin(double viewport, double artboardScreen, double pan) =>
        (Finite(viewport) - Finite(artboardScreen)) / 2 + Finite(pan);

    /// <summary>
    /// How far the offset may go in each direction, so that at least <see cref="MinimumVisible"/> pixels of the artboard
    /// (or all of it, if it is smaller) stay in view. Without a known viewport (zero or less) there is no limit.
    /// </summary>
    public static double PanLimit(double viewport, double artboardScreen)
    {
        if (!double.IsFinite(viewport) || viewport <= 0)
        {
            return double.PositiveInfinity;
        }

        var artboard = Math.Max(0, Finite(artboardScreen));
        return Math.Max(0, (viewport + artboard) / 2 - Math.Min(MinimumVisible, artboard));
    }

    /// <summary>Limits the offset to what <see cref="PanLimit"/> allows. A non-finite offset becomes zero.</summary>
    public static double ClampPan(double pan, double viewport, double artboardScreen)
    {
        var limit = PanLimit(viewport, artboardScreen);
        return Math.Clamp(Finite(pan), -limit, limit);
    }

    /// <summary>
    /// The offset that keeps the document point under <paramref name="anchor"/> (a screen position on this axis) at the same
    /// place when the zoom changes. The result is not limited yet, call <see cref="ClampPan"/> with the new artboard size.
    /// </summary>
    /// <param name="pan">The offset before the zoom change.</param>
    /// <param name="viewport">The size of the canvas area on this axis.</param>
    /// <param name="oldZoom">The zoom in percent before.</param>
    /// <param name="newZoom">The zoom in percent after.</param>
    /// <param name="artboardDocumentSize">The artboard size on this axis in document units.</param>
    /// <param name="anchor">The screen position that stays fixed.</param>
    public static double PanAfterZoom(double pan, double viewport, double oldZoom, double newZoom, double artboardDocumentSize, double anchor)
    {
        if (!double.IsFinite(oldZoom) || oldZoom <= 0 || !double.IsFinite(newZoom) || newZoom <= 0)
        {
            return Finite(pan);
        }

        var oldArtboard = Finite(artboardDocumentSize) * oldZoom / 100;
        var newArtboard = Finite(artboardDocumentSize) * newZoom / 100;
        var oldOrigin = Origin(viewport, oldArtboard, pan);
        var documentPoint = (Finite(anchor) - oldOrigin) / (oldZoom / 100);
        var newOrigin = Finite(anchor) - documentPoint * newZoom / 100;

        return newOrigin - (Finite(viewport) - newArtboard) / 2;
    }

    /// <summary>The zoom after a wheel turn: <c>zoom * 1.1^deltaY</c>, kept in the zoom range. Non-finite input leaves the zoom unchanged.</summary>
    public static double WheelZoom(double zoomPercent, double deltaY)
    {
        if (!double.IsFinite(zoomPercent) || !double.IsFinite(deltaY))
        {
            return ZoomLevels.Clamp(zoomPercent);
        }

        return ZoomLevels.Clamp(zoomPercent * Math.Pow(WheelZoomFactor, deltaY));
    }

    private static double Finite(double value) => double.IsFinite(value) ? value : 0;
}
