# PressToShape.Core.IO

Project: PressToShape.Core

## ArtboardDto (class)

`internal sealed class ArtboardDto`

Source: `src/PressToShape.Core/IO/ProjectFileDtos.cs:39-48`

_(no documentation)_

**Members**

- `public double? Width { get; set; }` - _(no documentation)_
- `public double? Height { get; set; }` - _(no documentation)_

## FileDto (class)

`internal sealed class FileDto`

Source: `src/PressToShape.Core/IO/ProjectFileDtos.cs:7-26`

_(no documentation)_

**Members**

- `public string? Format { get; set; }` - _(no documentation)_
- `public int? Version { get; set; }` - _(no documentation)_
- `public ArtboardDto? Artboard { get; set; }` - _(no documentation)_
- `public List<LayerDto?>? Layers { get; set; }` - _(no documentation)_
- `public List<string?>? Palette { get; set; }` - _(no documentation)_
- `public List<GuideDto?>? Guides { get; set; }` - _(no documentation)_

## GuideDto (class)

`internal sealed class GuideDto`

Source: `src/PressToShape.Core/IO/ProjectFileDtos.cs:28-37`

_(no documentation)_

**Members**

- `public string? Orientation { get; set; }` - _(no documentation)_
- `public double? Position { get; set; }` - _(no documentation)_

## LayerDto (class)

`internal sealed class LayerDto`

Source: `src/PressToShape.Core/IO/ProjectFileDtos.cs:50-61`

_(no documentation)_

**Members**

- `public string? Id { get; set; }` - _(no documentation)_
- `public string? Name { get; set; }` - _(no documentation)_
- `public bool? Visible { get; set; }` - _(no documentation)_
- `public bool? Locked { get; set; }` - _(no documentation)_
- `public List<ShapeDto?>? Shapes { get; set; }` - _(no documentation)_

## NodeDto (class)

`internal sealed class NodeDto`

Source: `src/PressToShape.Core/IO/ProjectFileDtos.cs:162-192`

_(no documentation)_

**Members**

- `public double? X { get; set; }` - _(no documentation)_
- `public double? Y { get; set; }` - _(no documentation)_
- `public double? InX { get; set; }` - _(no documentation)_
- `public double? InY { get; set; }` - _(no documentation)_
- `public double? OutX { get; set; }` - _(no documentation)_
- `public double? OutY { get; set; }` - _(no documentation)_
- `public string? Type { get; set; }` - _(no documentation)_

## ProjectFile (class)

`public partial static class ProjectFile`

Source: `src/PressToShape.Core/IO/ProjectFile.Path.cs:9-170`

PressToShape's own project format: UTF-8 JSON with a format name and a version number. Reading treats the content as untrusted: it is size-checked, parsed with a depth limit and validated by the document model itself. PressToShape.Core.IO.ProjectFile.Read never throws for bad content, it returns an error.

**Members**

- `public int MaxPathNodes` - The most nodes one path in a file may have, over all its sub-paths together.
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

## ShapeDto (class)

`internal sealed class ShapeDto`

Source: `src/PressToShape.Core/IO/ProjectFileDtos.cs:63-147`

_(no documentation)_

**Members**

- `public string? Type { get; set; }` - _(no documentation)_
- `public string? Id { get; set; }` - _(no documentation)_
- `public double? X { get; set; }` - _(no documentation)_
- `public double? Y { get; set; }` - _(no documentation)_
- `public double? Width { get; set; }` - _(no documentation)_
- `public double? Height { get; set; }` - _(no documentation)_
- `public string? Fill { get; set; }` - _(no documentation)_
- `public string? Stroke { get; set; }` - _(no documentation)_
- `public double? StrokeWidth { get; set; }` - _(no documentation)_
- `public double? Opacity { get; set; }` - _(no documentation)_
- `public double? Rotation { get; set; }` - _(no documentation)_
- `public string? Name { get; set; }` - _(no documentation)_
- `public bool? Visible { get; set; }` - _(no documentation)_
- `public bool? Locked { get; set; }` - _(no documentation)_
- `public List<double?>? Radii { get; set; }` - _(no documentation)_
- `public List<NodeDto?>? Nodes { get; set; }` - _(no documentation)_
- `public List<SubPathDto?>? SubPaths { get; set; }` - _(no documentation)_
- `public bool? Closed { get; set; }` - _(no documentation)_
- `public string? FillRule { get; set; }` - _(no documentation)_
- `public List<ShapeDto?>? Children { get; set; }` - _(no documentation)_

## SubPathDto (class)

`internal sealed class SubPathDto`

Source: `src/PressToShape.Core/IO/ProjectFileDtos.cs:150-159`

_(no documentation)_

**Members**

- `public List<NodeDto?>? Nodes { get; set; }` - _(no documentation)_
- `public bool? Closed { get; set; }` - _(no documentation)_

## SvgExport (class)

`public static class SvgExport`

Source: `src/PressToShape.Core/IO/SvgExport.cs:18-284`

Exports a document as SVG 1.1. The artboard becomes the viewport, every visible layer one group (marked as an Inkscape layer), every shape a rect, ellipse or path and every group a g around its children. Hidden shapes and the shapes below hidden groups are left out. There is no background and the output holds no scripts, links or external references.

**Members**

- `public byte[] ToUtf8(Document document)` - Writes the document as UTF-8 SVG without a byte order mark.
