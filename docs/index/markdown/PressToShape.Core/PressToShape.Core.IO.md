# PressToShape.Core.IO

Project: PressToShape.Core

## ProjectFile (class)

`public static class ProjectFile`

Source: `src/PressToShape.Core/IO/ProjectFile.cs:19-336`

PressToShape's own project format: UTF-8 JSON with a format name and a version number. Reading treats the content as untrusted: it is size-checked, parsed with a depth limit and validated by the document model itself. PressToShape.Core.IO.ProjectFile.Read never throws for bad content, it returns an error.

**Members**

- `public int FormatVersion` - The format version this code writes and reads.
- `public long MaxBytes` - The largest content PressToShape.Core.IO.ProjectFile.Read accepts, in bytes.
- `public byte[] Write(Document document)` - Writes the document as UTF-8 JSON without a byte order mark.
- `public ProjectFileResult Read(ReadOnlySpan<byte> utf8)` - Reads a document. Returns an error result for anything that is not a valid project file.

## ProjectFileError (enum)

`public enum ProjectFileError`

Source: `src/PressToShape.Core/IO/ProjectFileError.cs:4-20`

Why a project file could not be read. The app turns each value into a translated message.

**Members**

- `public NotProjectFile` - The content is not a PressToShape project file: not JSON, wrong format name or a missing entry.
- `public NewerVersion` - The file was written by a newer version of the format than this reader knows.
- `public TooLarge` - The content is larger than PressToShape.Core.IO.ProjectFile.MaxBytes.
- `public UnknownShapeType` - A shape has a type this version does not know.
- `public InvalidValue` - A value is not valid for the document model, for example a negative width or a duplicate id.

## ProjectFileResult (record)

`public sealed record ProjectFileResult : IEquatable<ProjectFileResult>`

Source: `src/PressToShape.Core/IO/ProjectFileResult.cs:9-17`

The outcome of reading a project file: either a document or an error.

**Members**

- `public ProjectFileResult(Document? Document, ProjectFileError? Error, string? Detail)` - The outcome of reading a project file: either a document or an error.
- `public string? Detail { get; init; }` - A technical English hint for logs, for example the entry that was wrong. Not meant to be shown as is.
- `public Document? Document { get; init; }` - The document, or null if reading failed.
- `public ProjectFileError? Error { get; init; }` - The reason for the failure, or null if reading succeeded.
- `public bool Succeeded { get; }` - Whether a document was read.
- `internal ProjectFileResult Success(Document document)` - _(no documentation)_
- `internal ProjectFileResult Failure(ProjectFileError error, string detail)` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.Core.Documents.Document`

## SvgExport (class)

`public static class SvgExport`

Source: `src/PressToShape.Core/IO/SvgExport.cs:16-172`

Exports a document as SVG 1.1. The artboard becomes the viewport, every visible layer one group (marked as an Inkscape layer), and every shape a rect or ellipse. There is no background and the output holds no scripts, links or external references.

**Members**

- `public byte[] ToUtf8(Document document)` - Writes the document as UTF-8 SVG without a byte order mark.
