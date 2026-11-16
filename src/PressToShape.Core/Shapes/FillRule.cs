namespace PressToShape.Core.Shapes;

/// <summary>How the inside of a path is decided where it crosses itself.</summary>
public enum FillRule
{
    /// <summary>A point is inside if the path winds around it at all (SVG <c>nonzero</c>).</summary>
    NonZero,

    /// <summary>A point is inside if a ray from it crosses the path an odd number of times (SVG <c>evenodd</c>).</summary>
    EvenOdd,
}
