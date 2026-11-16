using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>A radius handle to draw: the corner it belongs to, its center and its side length, in screen pixels.</summary>
internal readonly record struct ScreenRadiusHandle(Corner Corner, ScreenPoint Center, double Size);
