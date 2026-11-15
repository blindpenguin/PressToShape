namespace PressToShape.App.Workspace;

/// <summary>
/// The box around the selection, in document units: one shape gives its own (possibly turned) box, several shapes give the
/// upright box around all of them.
/// </summary>
/// <param name="CenterX">Center, horizontal.</param>
/// <param name="CenterY">Center, vertical.</param>
/// <param name="Width">Width before turning.</param>
/// <param name="Height">Height before turning.</param>
/// <param name="Rotation">Degrees, clockwise on the screen. Always 0 for several shapes.</param>
/// <param name="IsSingle">Whether the frame belongs to exactly one shape.</param>
internal sealed record SelectionFrame(double CenterX, double CenterY, double Width, double Height, double Rotation, bool IsSingle);
