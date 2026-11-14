using System;
using System.Collections.Immutable;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>
/// A named group of shapes that can be hidden or locked. Immutable: change it with <c>with</c>.
/// Records compare <see cref="Shapes"/> by reference, not by content.
/// </summary>
public sealed record Layer
{
    private Guid _id = Guid.NewGuid();
    private readonly string _name = string.Empty;
    private readonly ImmutableArray<Shape> _shapes = ImmutableArray<Shape>.Empty;

    /// <summary>Identifier, unique within a document.</summary>
    public Guid Id
    {
        get => _id;
        init => _id = value == Guid.Empty
            ? throw new ArgumentException("Id must not be empty.", nameof(Id))
            : value;
    }

    /// <summary>The layer's name. Not null, but may be empty. Core holds no default name, the caller provides it.</summary>
    public string Name
    {
        get => _name;
        init => _name = value ?? throw new ArgumentNullException(nameof(Name));
    }

    /// <summary>Whether the layer is shown. Hidden layers are not hit by <see cref="Document.HitTest"/>.</summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>Whether the layer is locked. Tools decide what a lock means, the model still reports hits.</summary>
    public bool IsLocked { get; init; }

    /// <summary>The shapes in stacking order, bottom first. Never the uninitialized default array.</summary>
    public ImmutableArray<Shape> Shapes
    {
        get => _shapes;
        init => _shapes = value.IsDefault ? ImmutableArray<Shape>.Empty : value;
    }
}
