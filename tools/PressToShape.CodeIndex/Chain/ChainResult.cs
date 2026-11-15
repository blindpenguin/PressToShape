using System.Collections.Generic;
using PressToShape.CodeIndex.Query;

namespace PressToShape.CodeIndex.Chain;

internal enum ChainStatus
{
    Found,
    Ambiguous,
    NotFound,

    /// <summary>The symbol exists, but it is not a view, a view model or a command.</summary>
    NotSupported,
}

/// <summary>The answer of <c>chain</c>: the tree when found, otherwise the candidates or a message that says what is accepted.</summary>
internal sealed record ChainResult(ChainStatus Status, ChainNode? Root, IReadOnlyList<NameEntry> Candidates, string? Message);
