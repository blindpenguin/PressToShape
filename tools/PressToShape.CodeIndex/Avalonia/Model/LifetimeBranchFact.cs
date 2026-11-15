using System.Collections.Generic;

namespace PressToShape.CodeIndex.Avalonia.Model;

/// <summary>The part of a lifecycle method that runs for one application lifetime type, such as the classic desktop lifetime.</summary>
internal sealed record LifetimeBranchFact(string LifetimeType, int Line, IReadOnlyList<CreatedFact> Created);
