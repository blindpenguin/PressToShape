using PressToShape.Core.Documents;

namespace PressToShape.Core.IO;

/// <summary>The outcome of reading a project file: either a document or an error.</summary>
/// <param name="Document">The document, or <c>null</c> if reading failed.</param>
/// <param name="Error">The reason for the failure, or <c>null</c> if reading succeeded.</param>
/// <param name="Detail">A technical English hint for logs, for example the entry that was wrong. Not meant to be shown as is.</param>
public sealed record ProjectFileResult(Document? Document, ProjectFileError? Error, string? Detail)
{
    /// <summary>Whether a document was read.</summary>
    public bool Succeeded => Document is not null;

    internal static ProjectFileResult Success(Document document) => new(document, null, null);

    internal static ProjectFileResult Failure(ProjectFileError error, string detail) => new(null, error, detail);
}
