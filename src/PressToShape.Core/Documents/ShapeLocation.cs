using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>The result of looking up a shape or group: the shape, the layer that holds it and the group it is in.</summary>
/// <param name="Layer">The layer that holds the shape.</param>
/// <param name="Shape">The shape or group.</param>
/// <param name="Parent">The closest group around the shape, or null if it is on the top level of the layer.</param>
/// <param name="IsEffectivelyVisible">Whether the shape is shown: it, every group around it and its layer are visible.</param>
/// <param name="IsEffectivelyLocked">Whether the shape is locked: it, a group around it or its layer is locked.</param>
public sealed record ShapeLocation(
    Layer Layer,
    Shape Shape,
    GroupShape? Parent = null,
    bool IsEffectivelyVisible = true,
    bool IsEffectivelyLocked = false);
