using System.Collections.Generic;

namespace PressToShape.CodeIndex.Avalonia.Model;

/// <summary>A value converter: a class that implements <c>IValueConverter</c> or <c>IMultiValueConverter</c>.</summary>
internal sealed record ConverterFact(string Id, string Name, string File, int Line, string? Summary, IReadOnlyList<string> Interfaces);
