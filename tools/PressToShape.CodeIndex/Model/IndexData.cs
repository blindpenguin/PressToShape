using System.Collections.Generic;
using PressToShape.CodeIndex.Avalonia.Model;

namespace PressToShape.CodeIndex.Model;

/// <summary>
/// The complete result of one analysis run, ready to be written. <c>Avalonia</c> holds the facts that only the Markdown pages use. They are never written as JSON.
/// </summary>
internal sealed record IndexData(IReadOnlyList<ProjectIndex> Projects, IReadOnlyList<LoadProblem> Warnings, AvaloniaFacts? Avalonia = null);
