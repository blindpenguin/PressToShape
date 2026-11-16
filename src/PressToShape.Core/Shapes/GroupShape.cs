using System;
using System.Collections.Immutable;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.Shapes;

/// <summary>
/// A group of shapes that is selected, moved and styled as one. Children keep their own document coordinates, so a group has no
/// transform. Its box is the union of the rotated bounds of its children, always upright (<see cref="Shape.Rotation"/> is 0), and
/// follows <see cref="Children"/> by itself. Do not set the box, the rotation or the appearance of a group: change its children.
/// Immutable: change it with <c>with</c>.
/// </summary>
public sealed record GroupShape : Shape
{
    private ImmutableArray<Shape> _children = ImmutableArray<Shape>.Empty;

    /// <summary>The children in stacking order, bottom first. At least one, none null, nested groups allowed.</summary>
    public required ImmutableArray<Shape> Children
    {
        get => _children;
        init
        {
            if (value.IsDefaultOrEmpty)
            {
                throw new ArgumentException("A group needs at least one child.", nameof(Children));
            }

            foreach (var child in value)
            {
                if (child is null)
                {
                    throw new ArgumentException("A child must not be null.", nameof(Children));
                }
            }

            _children = value;
            var box = UnionOfBounds(value);
            X = box.X;
            Y = box.Y;
            Width = box.Width;
            Height = box.Height;
            Rotation = 0;
        }
    }

    /// <summary>Whether the point lies on a visible child. A hidden child is not hit.</summary>
    protected override bool ContainsLocal(double x, double y)
    {
        for (var i = _children.Length - 1; i >= 0; i--)
        {
            if (_children[i].IsVisible && _children[i].Contains(x, y))
            {
                return true;
            }
        }

        return false;
    }

    private static Bounds UnionOfBounds(ImmutableArray<Shape> children)
    {
        var left = double.PositiveInfinity;
        var top = double.PositiveInfinity;
        var right = double.NegativeInfinity;
        var bottom = double.NegativeInfinity;
        foreach (var child in children)
        {
            var bounds = child.RotatedBounds;
            left = Math.Min(left, bounds.X);
            top = Math.Min(top, bounds.Y);
            right = Math.Max(right, bounds.X + bounds.Width);
            bottom = Math.Max(bottom, bounds.Y + bounds.Height);
        }

        return new Bounds(left, top, right - left, bottom - top);
    }
}
