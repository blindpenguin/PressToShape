using System.Collections.Generic;

namespace PressToShape.CodeIndex.Avalonia.Model;

/// <summary>An application class (derives from <c>Avalonia.Application</c>) with the lifecycle methods that create objects.</summary>
internal sealed record ApplicationFact(string Id, string Name, string File, IReadOnlyList<LifecycleFact> Overrides);
