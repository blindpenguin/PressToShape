using System.Collections.Generic;

namespace PressToShape.CodeIndex.Avalonia.Model;

/// <summary>Facts about the Avalonia parts of the solution: custom controls, value converters and the application class. Used only by the documentation pages.</summary>
internal sealed record AvaloniaFacts(
    IReadOnlyList<ControlFact> Controls,
    IReadOnlyList<ConverterFact> Converters,
    IReadOnlyList<ApplicationFact> Applications)
{
    public static AvaloniaFacts Empty { get; } = new([], [], []);
}
