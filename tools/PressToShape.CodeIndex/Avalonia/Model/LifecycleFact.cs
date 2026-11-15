using System.Collections.Generic;

namespace PressToShape.CodeIndex.Avalonia.Model;

/// <summary>A lifecycle method of the application class, the types it creates, and the creations inside lifetime branches.</summary>
internal sealed record LifecycleFact(string MethodId, string Name, int Line, IReadOnlyList<CreatedFact> Created, IReadOnlyList<LifetimeBranchFact> Branches);
