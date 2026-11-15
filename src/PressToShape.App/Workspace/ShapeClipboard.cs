using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// The shapes that were copied or cut. A copy kept by the app: nothing goes to the system clipboard.
/// The shapes are immutable, so the clipboard holds them as they were when they were copied.
/// </summary>
internal sealed class ShapeClipboard
{
    /// <summary>The distance in document units that each paste moves the shapes further.</summary>
    public const double PasteStep = 10;

    private ImmutableArray<Shape> _shapes = ImmutableArray<Shape>.Empty;
    private int _pasteCount;

    /// <summary>Raised when the content changes.</summary>
    public event EventHandler? Changed;

    /// <summary>The copied shapes in document order.</summary>
    public IReadOnlyList<Shape> Shapes => _shapes;

    /// <summary>Whether there is nothing to paste.</summary>
    public bool IsEmpty => _shapes.IsEmpty;

    /// <summary>Replaces the content. The next paste starts at the first offset again.</summary>
    public void Set(IEnumerable<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        _shapes = [.. shapes];
        _pasteCount = 0;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Empties the clipboard.</summary>
    public void Clear() => Set([]);

    /// <summary>The offset for the next paste: 10 units for the first paste, 20 for the second, and so on.</summary>
    public double NextPasteOffset()
    {
        _pasteCount++;
        return PasteStep * _pasteCount;
    }
}
