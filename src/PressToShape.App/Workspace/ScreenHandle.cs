namespace PressToShape.App.Workspace;

/// <summary>A handle to draw: its type, its center and its side length, all in screen pixels.</summary>
internal readonly record struct ScreenHandle(HandleType Type, ScreenPoint Center, double Size);
