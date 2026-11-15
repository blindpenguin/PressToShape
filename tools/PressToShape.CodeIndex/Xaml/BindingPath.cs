using System.Collections.Generic;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Where a binding path starts.</summary>
internal enum BindingSource
{
    /// <summary>The data context (the default).</summary>
    DataContext,

    /// <summary><c>$self</c>.</summary>
    Self,

    /// <summary><c>$parent</c> or <c>$parent[Type,level]</c>. <see cref="BindingPath.SourceText"/> holds the bracket text.</summary>
    Parent,

    /// <summary><c>#name</c>. <see cref="BindingPath.SourceText"/> holds the name.</summary>
    ElementName,
}

/// <summary>A parsed binding path. <c>Unsupported</c> describes the first part that the resolver does not model (indexer, cast, attached property, stream operator).</summary>
internal sealed record BindingPath(BindingSource Source, string? SourceText, IReadOnlyList<string> Segments, string? Unsupported, int Negations);
