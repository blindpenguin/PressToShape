using System;
using System.Collections.Generic;
using PressToShape.Core.Documents;
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
    private readonly double _rotation;
    private readonly string _name = string.Empty;

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

    /// <summary>
    /// Rotation in degrees around the center of the box. Positive angles turn clockwise on the screen (y grows downward).
    /// Finite. Stored normalized to at least 0 and less than 360. The default is 0.
    /// </summary>
    public double Rotation
    {
        get => _rotation;
        init => _rotation = Normalize(Guard.Finite(value, nameof(Rotation)));
    }

    /// <summary>Fill, stroke and opacity.</summary>
    public Appearance Appearance
    {
        get => _appearance;
        init => _appearance = value ?? throw new ArgumentNullException(nameof(Appearance));
    }

    /// <summary>The name the user gave the object. Not null. Empty means the object has no name of its own, and the app shows a default text.</summary>
    public string Name
    {
        get => _name;
        init => _name = value ?? throw new ArgumentNullException(nameof(Name));
    }

    /// <summary>Whether the object is shown. A hidden object is not drawn, not hit and not exported, and neither are the objects inside a hidden group.</summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>Whether the object is locked. Tools decide what a lock means, the model still reports hits.</summary>
    public bool IsLocked { get; init; }

    /// <summary>The shape itself if it is not a group, otherwise all shapes below the group that are not groups, bottom first.</summary>
    public IReadOnlyList<Shape> Leaves() => ShapeTree.Leaves(this);

    /// <summary>
    /// A copy of the shape, and of everything below it if it is a group, each with a new id from <paramref name="newId"/> and moved by the
    /// distance. For a group the children get their ids first, then the group.
    /// </summary>
    public Shape MovedCopy(Func<Guid> newId, double dx, double dy)
    {
        ArgumentNullException.ThrowIfNull(newId);

        return ShapeTree.Reidentify(ShapeTree.Translate(this, dx, dy), newId);
    }

    /// <summary>The unturned box of the shape. The stroke width and the rotation are not included.</summary>
    public Bounds Bounds => new(X, Y, Width, Height);

    /// <summary>Horizontal center of the box. The shape turns around it.</summary>
    public double CenterX => X + Width / 2;

    /// <summary>Vertical center of the box. The shape turns around it.</summary>
    public double CenterY => Y + Height / 2;

    /// <summary>The axis-aligned bounds of the turned box. Equal to <see cref="Bounds"/> without a rotation.</summary>
    public Bounds RotatedBounds
    {
        get
        {
            if (_rotation == 0)
            {
                return Bounds;
            }

            var (sin, cos) = SinCos(_rotation);
            var halfWidth = Width / 2;
            var halfHeight = Height / 2;

            // The turned box is symmetric around the center, so its half extents come from the absolute matrix entries.
            var extentX = Math.Abs(halfWidth * cos) + Math.Abs(halfHeight * sin);
            var extentY = Math.Abs(halfWidth * sin) + Math.Abs(halfHeight * cos);
            return new Bounds(CenterX - extentX, CenterY - extentY, extentX * 2, extentY * 2);
        }
    }

    /// <summary>Whether the point lies inside the turned shape's geometry or on its edge. The stroke is ignored.</summary>
    public bool Contains(double x, double y)
    {
        if (_rotation != 0)
        {
            // Turn the point the opposite way around the center, then test against the unturned shape.
            var (sin, cos) = SinCos(-_rotation);
            var dx = x - CenterX;
            var dy = y - CenterY;
            x = CenterX + dx * cos - dy * sin;
            y = CenterY + dx * sin + dy * cos;
        }

        return ContainsLocal(x, y);
    }

    /// <summary>Whether the point lies inside the shape's geometry before any rotation is applied.</summary>
    protected abstract bool ContainsLocal(double x, double y);

    // Sine and cosine of an angle in degrees, exact for multiples of 90 so quarter turns have no rounding noise.
    private protected static (double Sin, double Cos) SinCos(double degrees)
    {
        var quarter = degrees / 90;
        if (quarter == Math.Floor(quarter))
        {
            return (((long)quarter % 4 + 4) % 4) switch
            {
                0 => (0, 1),
                1 => (1, 0),
                2 => (0, -1),
                _ => (-1, 0),
            };
        }

        var radians = degrees * Math.PI / 180;
        return (Math.Sin(radians), Math.Cos(radians));
    }

    private static double Normalize(double degrees)
    {
        var result = degrees % 360;
        if (result < 0)
        {
            result += 360;
        }

        // A tiny negative angle can round up to exactly 360.
        return result >= 360 ? 0 : result;
    }
}
