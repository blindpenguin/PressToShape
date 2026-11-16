namespace PressToShape.App.ViewModels.Editor;

/// <summary>What a row of the object list stands for. It decides the icon and the default name.</summary>
internal enum ObjectRowType
{
    /// <summary>A layer. The row is a header with the layer's name, and has no toggles.</summary>
    Layer,

    /// <summary>A rectangle.</summary>
    Rectangle,

    /// <summary>An ellipse.</summary>
    Ellipse,

    /// <summary>A group, which has rows of its own below it.</summary>
    Group,

    /// <summary>A path.</summary>
    Path,
}
