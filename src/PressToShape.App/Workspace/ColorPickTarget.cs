namespace PressToShape.App.Workspace;

/// <summary>Which color a pick from the canvas (the eyedropper) will set.</summary>
internal enum ColorPickTarget
{
    /// <summary>No pick is going on.</summary>
    None,

    /// <summary>The fill.</summary>
    Fill,

    /// <summary>The stroke.</summary>
    Stroke,
}
