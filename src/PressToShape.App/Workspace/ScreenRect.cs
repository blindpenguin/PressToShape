namespace PressToShape.App.Workspace;

/// <summary>A rectangle in screen pixels, for example the outline of the selected shape.</summary>
internal readonly record struct ScreenRect(double X, double Y, double Width, double Height);
