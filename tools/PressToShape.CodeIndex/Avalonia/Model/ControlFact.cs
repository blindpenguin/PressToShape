using System.Collections.Generic;

namespace PressToShape.CodeIndex.Avalonia.Model;

/// <summary>A custom control: a class deriving from <c>Avalonia.Controls.Control</c> that has no XAML file.</summary>
internal sealed record ControlFact(
    string Id,
    string Name,
    string? BaseType,
    string File,
    int Line,
    string? Summary,
    IReadOnlyList<AvaloniaPropertyFact> Properties,
    IReadOnlyList<RoutedEventFact> Events);
