using System.Collections.Generic;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Chain;

/// <summary>
/// One node of a chain: a label, the id and source location it stands for, how sure the index is about the link that leads to it (when it is a link), the reason, and its children.
/// The tree is printed as text or written as JSON.
/// </summary>
internal sealed record ChainNode(string Label, string? Id, string? Location, Confidence? Confidence, string? Reason, IReadOnlyList<ChainNode> Children)
{
    public static ChainNode Of(string label, string? id = null, string? location = null, Confidence? confidence = null, string? reason = null, IEnumerable<ChainNode>? children = null) =>
        new(label, id, location, confidence, reason, children is null ? [] : [.. children]);
}
