using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PressToShape.Core.IO;

// The shapes of the project file as JSON. Optional entries are written only when they differ from the default, so older files stay valid.
internal sealed class FileDto
{
    [JsonPropertyName("format")] public string? Format { get; set; }

    [JsonPropertyName("version")] public int? Version { get; set; }

    [JsonPropertyName("artboard")] public ArtboardDto? Artboard { get; set; }

    [JsonPropertyName("layers")] public List<LayerDto?>? Layers { get; set; }

    // Optional: written only for a document with swatches, and missing means an empty palette.
    [JsonPropertyName("palette")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string?>? Palette { get; set; }

    // Optional: written only for a document with guides, and missing means no guides.
    [JsonPropertyName("guides")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<GuideDto?>? Guides { get; set; }
}

internal sealed class GuideDto
{
    [JsonPropertyName("orientation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Orientation { get; set; }

    [JsonPropertyName("position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Position { get; set; }
}

internal sealed class ArtboardDto
{
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Width { get; set; }

    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Height { get; set; }
}

internal sealed class LayerDto
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("visible")] public bool? Visible { get; set; }

    [JsonPropertyName("locked")] public bool? Locked { get; set; }

    [JsonPropertyName("shapes")] public List<ShapeDto?>? Shapes { get; set; }
}

internal sealed class ShapeDto
{
    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("x")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? X { get; set; }

    [JsonPropertyName("y")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Y { get; set; }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Width { get; set; }

    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Height { get; set; }

    [JsonPropertyName("fill")] public string? Fill { get; set; }

    [JsonPropertyName("stroke")] public string? Stroke { get; set; }

    [JsonPropertyName("strokeWidth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? StrokeWidth { get; set; }

    [JsonPropertyName("opacity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Opacity { get; set; }

    // Optional: written only for a rotated shape, and missing means no rotation.
    [JsonPropertyName("rotation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Rotation { get; set; }

    // Optional: written only for a shape with a name, and missing means no name.
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    // Optional: written only for a hidden shape, and missing means visible.
    [JsonPropertyName("visible")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Visible { get; set; }

    // Optional: written only for a locked shape, and missing means unlocked.
    [JsonPropertyName("locked")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Locked { get; set; }

    // Only for a rectangle: the corner radii as entered, top left, top right, bottom right, bottom left. Written only when one is not 0,
    // and missing means square corners.
    [JsonPropertyName("radii")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double?>? Radii { get; set; }

    // Only for a path: its nodes, as fractions of the box (see PathNode).
    [JsonPropertyName("nodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<NodeDto?>? Nodes { get; set; }

    // Only for a path with more than one sub-path: its sub-paths. Then the path has no "nodes" and no "closed" of its own.
    [JsonPropertyName("subPaths")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SubPathDto?>? SubPaths { get; set; }

    // Only for a path: written only when the path is closed, and missing means open.
    [JsonPropertyName("closed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Closed { get; set; }

    // Only for a path: written only as "evenodd", and missing means the non-zero rule.
    [JsonPropertyName("fillRule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FillRule { get; set; }

    // Only for a group: its children, bottom first. A group has no box, no rotation and no appearance of its own in the file.
    [JsonPropertyName("children")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ShapeDto?>? Children { get; set; }
}

// A sub-path of a compound path: its nodes and whether it is closed (written only when it is, missing means open).
internal sealed class SubPathDto
{
    [JsonPropertyName("nodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<NodeDto?>? Nodes { get; set; }

    [JsonPropertyName("closed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Closed { get; set; }
}

// A node of a path. The handle points are written only for a node that has them, and then in pairs.
internal sealed class NodeDto
{
    [JsonPropertyName("x")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? X { get; set; }

    [JsonPropertyName("y")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Y { get; set; }

    [JsonPropertyName("inX")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? InX { get; set; }

    [JsonPropertyName("inY")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? InY { get; set; }

    [JsonPropertyName("outX")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OutX { get; set; }

    [JsonPropertyName("outY")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OutY { get; set; }

    // Optional: written only when the type differs from the one that follows from the handles, and missing means that derived type.
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }
}
