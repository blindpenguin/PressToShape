namespace PressToShape.Core.Shapes;

/// <summary>An axis-aligned rectangle.</summary>
public sealed record RectangleShape : Shape
{
    /// <inheritdoc />
    protected override bool ContainsLocal(double x, double y) =>
        x >= X && x <= X + Width && y >= Y && y <= Y + Height;
}
