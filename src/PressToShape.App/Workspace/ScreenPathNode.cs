namespace PressToShape.App.Workspace;

/// <summary>A node of a <see cref="ScreenPath"/> in screen pixels, relative to the top left corner of the shape's box.</summary>
/// <param name="X">Anchor x.</param>
/// <param name="Y">Anchor y.</param>
/// <param name="InX">Handle point on the side of the previous node, or <c>null</c> for no handle.</param>
/// <param name="InY">Handle point on the side of the previous node, or <c>null</c> for no handle.</param>
/// <param name="OutX">Handle point on the side of the next node, or <c>null</c> for no handle.</param>
/// <param name="OutY">Handle point on the side of the next node, or <c>null</c> for no handle.</param>
internal readonly record struct ScreenPathNode(double X, double Y, double? InX = null, double? InY = null, double? OutX = null, double? OutY = null)
{
    /// <summary>Whether the node has an in handle.</summary>
    public bool HasIn => InX.HasValue && InY.HasValue;

    /// <summary>Whether the node has an out handle.</summary>
    public bool HasOut => OutX.HasValue && OutY.HasValue;
}
