using System;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.Shapes;

/// <summary>
/// A drawable shape with a position, a size and an appearance. Immutable: change it with <c>with</c>.
/// Values are validated on creation and on <c>with</c>.
/// </summary>
public abstract record Shape
{
    private Guid _id = Guid.NewGuid();
    private readonly double _x;
    private readonly double _y;
    private readonly double _width;
    private readonly double _height;
    private readonly Appearance _appearance = Appearance.Default;

    // Private protected, so only shape types inside Core exist.
    private protected Shape()
    {
    }

    /// <summary>Identifier, unique within a document. Stays the same when the shape is edited.</summary>
    public Guid Id
    {
        get => _id;
        init => _id = value == Guid.Empty
            ? throw new ArgumentException("Id must not be empty.", nameof(Id))
            : value;
    }

    /// <summary>Left edge in document units.</summary>
    public double X
    {
        get => _x;
        init => _x = Guard.Finite(value, nameof(X));
    }

    /// <summary>Top edge in document units.</summary>
    public double Y
    {
        get => _y;
        init => _y = Guard.Finite(value, nameof(Y));
    }

    /// <summary>Width, finite and not negative. Zero is allowed.</summary>
    public double Width
    {
        get => _width;
        init => _width = Guard.FiniteNonNegative(value, nameof(Width));
    }

    /// <summary>Height, finite and not negative. Zero is allowed.</summary>
    public double Height
    {
        get => _height;
        init => _height = Guard.FiniteNonNegative(value, nameof(Height));
    }

    /// <summary>Fill, stroke and opacity.</summary>
    public Appearance Appearance
    {
        get => _appearance;
        init => _appearance = value ?? throw new ArgumentNullException(nameof(Appearance));
    }

    /// <summary>The geometry bounds. The stroke width is not included.</summary>
    public Bounds Bounds => new(X, Y, Width, Height);

    /// <summary>Whether the point lies inside the shape's geometry or on its edge. The stroke is ignored.</summary>
    public abstract bool Contains(double x, double y);
}
