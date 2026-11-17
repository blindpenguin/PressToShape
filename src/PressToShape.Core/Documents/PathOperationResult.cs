using System;
using System.Collections.Generic;

namespace PressToShape.Core.Documents;

/// <summary>Why a path operation did nothing.</summary>
public enum PathOperationFailure
{
    /// <summary>The shapes cannot be used: fewer than needed, a shape of the wrong type, a group, an open path, a hidden or locked shape, or too many sub-paths.</summary>
    Invalid,

    /// <summary>The result covers no area, so nothing would be left.</summary>
    Empty,
}

/// <summary>The result of a path operation on a document: the new document and the shapes it made, or the reason it did nothing.</summary>
public sealed record PathOperationResult
{
    private PathOperationResult(Document? document, IReadOnlyList<Guid> newShapeIds, PathOperationFailure? failure)
    {
        Document = document;
        NewShapeIds = newShapeIds;
        Failure = failure;
    }

    /// <summary>The new document, or <c>null</c> if the operation failed.</summary>
    public Document? Document { get; }

    /// <summary>The ids of the shapes the operation made, in stacking order. Empty if it failed.</summary>
    public IReadOnlyList<Guid> NewShapeIds { get; }

    /// <summary>Why the operation did nothing, or <c>null</c> if it worked.</summary>
    public PathOperationFailure? Failure { get; }

    /// <summary>Whether the operation worked.</summary>
    public bool Succeeded => Failure is null;

    /// <summary>A result of an operation that worked.</summary>
    public static PathOperationResult Success(Document document, IReadOnlyList<Guid> newShapeIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(newShapeIds);
        return new PathOperationResult(document, newShapeIds, null);
    }

    /// <summary>A result of an operation that did nothing.</summary>
    public static PathOperationResult Fail(PathOperationFailure failure) => new(null, [], failure);
}
