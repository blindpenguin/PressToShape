using Microsoft.CodeAnalysis;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// The result of resolving a XAML type name. <c>Symbol</c> is set when a C# type was found anywhere (solution or package).
/// <c>SolutionId</c> is set only when that type is declared in the solution. Other types are known by <c>Display</c> only.
/// </summary>
internal sealed record ResolvedType(INamedTypeSymbol? Symbol, string Display, string? SolutionId)
{
    public bool IsResolved => Symbol is not null;
}
