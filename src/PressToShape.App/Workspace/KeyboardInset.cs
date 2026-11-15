using System;

namespace PressToShape.App.Workspace;

/// <summary>
/// How far the layout has to stop above the bottom of the screen so that the on-screen keyboard does not cover it. On Android the keyboard does
/// not resize the view, it only reports the area it covers, so the layout leaves that gap itself.
/// </summary>
internal static class KeyboardInset
{
    /// <summary>
    /// The gap at the bottom of the view, in the same units as the arguments.
    /// </summary>
    /// <param name="isKeyboardOpen">Whether the keyboard is shown.</param>
    /// <param name="viewBottom">The bottom edge of the view, measured from the top of the window.</param>
    /// <param name="coveredTop">The top edge of the area the keyboard covers, measured from the top of the window.</param>
    /// <returns>0 while the keyboard is closed or does not reach the view, otherwise how much of the view it covers.</returns>
    public static double Gap(bool isKeyboardOpen, double viewBottom, double coveredTop)
    {
        if (!isKeyboardOpen || !double.IsFinite(viewBottom) || !double.IsFinite(coveredTop))
        {
            return 0;
        }

        return Math.Max(0, viewBottom - coveredTop);
    }
}
