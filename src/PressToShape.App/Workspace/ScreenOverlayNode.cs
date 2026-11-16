namespace PressToShape.App.Workspace;

/// <summary>One node of the overlay of the node tools, in screen pixels of the canvas area.</summary>
/// <param name="X">Anchor x.</param>
/// <param name="Y">Anchor y.</param>
/// <param name="IsSelected">Whether the node is one of the selected nodes.</param>
/// <param name="InX">The shown in handle point, or <c>null</c> when there is none or it is not shown.</param>
/// <param name="InY">The shown in handle point, or <c>null</c>.</param>
/// <param name="OutX">The shown out handle point, or <c>null</c> when there is none or it is not shown.</param>
/// <param name="OutY">The shown out handle point, or <c>null</c>.</param>
internal readonly record struct ScreenOverlayNode(double X, double Y, bool IsSelected, double? InX = null, double? InY = null, double? OutX = null, double? OutY = null)
{
    /// <summary>Whether the in handle is shown.</summary>
    public bool HasIn => InX.HasValue && InY.HasValue;

    /// <summary>Whether the out handle is shown.</summary>
    public bool HasOut => OutX.HasValue && OutY.HasValue;
}
