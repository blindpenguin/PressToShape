using System;

namespace PressToShape.Core.Documents;

/// <summary>The direction of a guide line.</summary>
public enum GuideOrientation
{
    /// <summary>A line across the drawing at a y position.</summary>
    Horizontal,

    /// <summary>A line up and down the drawing at an x position.</summary>
    Vertical,
}

/// <summary>
/// A guide line that the user dragged out of a ruler. It is part of the document, but not of any layer: it has no look, cannot be selected as a
/// shape and is not exported. Immutable: change it with <c>with</c>.
/// </summary>
public sealed record Guide
{
    private Guid _id = Guid.NewGuid();
    private readonly double _position;

    /// <summary>Identifier, unique within a document. It is not saved: a guide that is loaded gets a new one.</summary>
    public Guid Id
    {
        get => _id;
        init => _id = value == Guid.Empty
            ? throw new ArgumentException("Id must not be empty.", nameof(Id))
            : value;
    }

    /// <summary>Whether the guide is a horizontal or a vertical line.</summary>
    public GuideOrientation Orientation { get; init; }

    /// <summary>Where the guide is, in document units: the y of a horizontal guide, the x of a vertical one. Finite.</summary>
    public double Position
    {
        get => _position;
        init => _position = Guard.Finite(value, nameof(Position));
    }
}
