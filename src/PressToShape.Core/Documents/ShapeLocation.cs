using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>The result of looking up a shape: the shape and the layer that holds it.</summary>
/// <param name="Layer">The layer that holds the shape.</param>
/// <param name="Shape">The shape.</param>
public sealed record ShapeLocation(Layer Layer, Shape Shape);
