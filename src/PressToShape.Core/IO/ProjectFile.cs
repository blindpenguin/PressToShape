using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.IO;

/// <summary>
/// PressToShape's own project format: UTF-8 JSON with a format name and a version number.
/// Reading treats the content as untrusted: it is size-checked, parsed with a depth limit and validated
/// by the document model itself. <see cref="Read"/> never throws for bad content, it returns an error.
/// </summary>
public static class ProjectFile
{
    /// <summary>The format version this code writes and reads.</summary>
    public const int FormatVersion = 1;

    /// <summary>The largest content <see cref="Read"/> accepts, in bytes.</summary>
    public const long MaxBytes = 64L * 1024 * 1024;

    private const string FormatName = "PressToShape";
    private const string RectangleType = "rectangle";
    private const string EllipseType = "ellipse";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        MaxDepth = 16,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false,
    };

    /// <summary>Writes the document as UTF-8 JSON without a byte order mark.</summary>
    public static byte[] Write(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var file = new FileDto
        {
            Format = FormatName,
            Version = FormatVersion,
            Artboard = new ArtboardDto { Width = document.ArtboardWidth, Height = document.ArtboardHeight },
            Layers = new List<LayerDto?>(document.Layers.Select(ToDto)),
        };

        return JsonSerializer.SerializeToUtf8Bytes(file, Options);
    }

    /// <summary>Reads a document. Returns an error result for anything that is not a valid project file.</summary>
    public static ProjectFileResult Read(ReadOnlySpan<byte> utf8)
    {
        if (utf8.Length > MaxBytes)
        {
            return ProjectFileResult.Failure(ProjectFileError.TooLarge, $"Content has {utf8.Length} bytes, the limit is {MaxBytes}.");
        }

        FileDto? file;
        try
        {
            file = JsonSerializer.Deserialize<FileDto>(utf8, Options);
        }
        catch (Exception ex) when (IsParseFailure(ex))
        {
            return ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "Not valid JSON for this format: " + ex.Message);
        }

        if (file is null || file.Format != FormatName || file.Version is not { } version || version < 1)
        {
            return ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "Missing or wrong format name or version.");
        }

        if (version > FormatVersion)
        {
            return ProjectFileResult.Failure(ProjectFileError.NewerVersion, $"File version {version}, this reader knows {FormatVersion}.");
        }

        try
        {
            return Build(file);
        }
        catch (ArgumentException ex)
        {
            return ProjectFileResult.Failure(ProjectFileError.InvalidValue, ex.Message);
        }
    }

    private static bool IsParseFailure(Exception ex) =>
        ex is JsonException or FormatException or InvalidOperationException or NotSupportedException or OverflowException;

    private static ProjectFileResult Build(FileDto file)
    {
        if (file.Artboard is not { Width: { } width, Height: { } height } || file.Layers is null || file.Layers.Count == 0)
        {
            return ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "Missing artboard or layers.");
        }

        var layers = ImmutableArray.CreateBuilder<Layer>();
        foreach (var layerDto in file.Layers)
        {
            if (layerDto is null || layerDto.Name is null || layerDto.Visible is null || layerDto.Locked is null || layerDto.Shapes is null)
            {
                return ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A layer misses an entry.");
            }

            if (!TryParseId(layerDto.Id, out var layerId))
            {
                return ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A layer has no valid id.");
            }

            var shapes = ImmutableArray.CreateBuilder<Shape>();
            foreach (var shapeDto in layerDto.Shapes)
            {
                var shape = BuildShape(shapeDto, out var failure);
                if (shape is null)
                {
                    return failure!;
                }

                shapes.Add(shape);
            }

            layers.Add(new Layer
            {
                Id = layerId,
                Name = layerDto.Name,
                IsVisible = layerDto.Visible.Value,
                IsLocked = layerDto.Locked.Value,
                Shapes = shapes.ToImmutable(),
            });
        }

        var document = Document.Create(string.Empty) with { ArtboardWidth = width, ArtboardHeight = height, Layers = layers.ToImmutable() };
        return ProjectFileResult.Success(document);
    }

    private static Shape? BuildShape(ShapeDto? dto, out ProjectFileResult? failure)
    {
        failure = null;
        if (dto is null || dto.Type is null || dto.X is null || dto.Y is null || dto.Width is null || dto.Height is null
            || dto.StrokeWidth is null || dto.Opacity is null)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A shape misses an entry.");
            return null;
        }

        if (!TryParseId(dto.Id, out var id))
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A shape has no valid id.");
            return null;
        }

        if (!TryParseColor(dto.Fill, out var fill) || !TryParseColor(dto.Stroke, out var stroke))
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A shape has an invalid color.");
            return null;
        }

        var appearance = new Appearance { Fill = fill, Stroke = stroke, StrokeWidth = dto.StrokeWidth.Value, Opacity = dto.Opacity.Value };
        switch (dto.Type)
        {
            case RectangleType:
                return new RectangleShape { Id = id, X = dto.X.Value, Y = dto.Y.Value, Width = dto.Width.Value, Height = dto.Height.Value, Appearance = appearance };
            case EllipseType:
                return new EllipseShape { Id = id, X = dto.X.Value, Y = dto.Y.Value, Width = dto.Width.Value, Height = dto.Height.Value, Appearance = appearance };
            default:
                failure = ProjectFileResult.Failure(ProjectFileError.UnknownShapeType, "Unknown shape type: " + dto.Type);
                return null;
        }
    }

    private static LayerDto ToDto(Layer layer) => new()
    {
        Id = layer.Id.ToString("D", CultureInfo.InvariantCulture),
        Name = ReplaceLoneSurrogates(layer.Name),
        Visible = layer.IsVisible,
        Locked = layer.IsLocked,
        Shapes = new List<ShapeDto?>(layer.Shapes.Select(ToDto)),
    };

    private static ShapeDto ToDto(Shape shape) => new()
    {
        Type = shape switch
        {
            RectangleShape => RectangleType,
            EllipseShape => EllipseType,
            _ => throw new NotSupportedException($"Shape type {shape.GetType().Name} cannot be saved."),
        },
        Id = shape.Id.ToString("D", CultureInfo.InvariantCulture),
        X = shape.X,
        Y = shape.Y,
        Width = shape.Width,
        Height = shape.Height,
        Fill = ToText(shape.Appearance.Fill),
        Stroke = ToText(shape.Appearance.Stroke),
        StrokeWidth = shape.Appearance.StrokeWidth,
        Opacity = shape.Appearance.Opacity,
    };

    // A lone surrogate cannot be written as UTF-8 JSON. It is replaced, like text encoders do.
    private static string ReplaceLoneSurrogates(string text)
    {
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (char.IsHighSurrogate(chars[i]) && i + 1 < chars.Length && char.IsLowSurrogate(chars[i + 1]))
            {
                i++;
            }
            else if (char.IsSurrogate(chars[i]))
            {
                chars[i] = '�';
            }
        }

        return new string(chars);
    }

    private static string? ToText(Color? color) =>
        color is { } c ? string.Create(CultureInfo.InvariantCulture, $"#{c.R:x2}{c.G:x2}{c.B:x2}{c.A:x2}") : null;

    private static bool TryParseId(string? text, out Guid id)
    {
        id = Guid.Empty;
        return text is not null && Guid.TryParseExact(text, "D", out id) && id != Guid.Empty;
    }

    // Null text means "no color". Anything else must be #rrggbbaa.
    private static bool TryParseColor(string? text, out Color? color)
    {
        color = null;
        if (text is null)
        {
            return true;
        }

        if (text.Length != 9 || text[0] != '#' || !text.Skip(1).All(Uri.IsHexDigit))
        {
            return false;
        }

        byte Channel(int index) => byte.Parse(text.AsSpan(1 + index * 2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        color = new Color(Channel(0), Channel(1), Channel(2), Channel(3));
        return true;
    }

    private sealed class FileDto
    {
        [JsonPropertyName("format")] public string? Format { get; set; }

        [JsonPropertyName("version")] public int? Version { get; set; }

        [JsonPropertyName("artboard")] public ArtboardDto? Artboard { get; set; }

        [JsonPropertyName("layers")] public List<LayerDto?>? Layers { get; set; }
    }

    private sealed class ArtboardDto
    {
        [JsonPropertyName("width")] public double? Width { get; set; }

        [JsonPropertyName("height")] public double? Height { get; set; }
    }

    private sealed class LayerDto
    {
        [JsonPropertyName("id")] public string? Id { get; set; }

        [JsonPropertyName("name")] public string? Name { get; set; }

        [JsonPropertyName("visible")] public bool? Visible { get; set; }

        [JsonPropertyName("locked")] public bool? Locked { get; set; }

        [JsonPropertyName("shapes")] public List<ShapeDto?>? Shapes { get; set; }
    }

    private sealed class ShapeDto
    {
        [JsonPropertyName("type")] public string? Type { get; set; }

        [JsonPropertyName("id")] public string? Id { get; set; }

        [JsonPropertyName("x")] public double? X { get; set; }

        [JsonPropertyName("y")] public double? Y { get; set; }

        [JsonPropertyName("width")] public double? Width { get; set; }

        [JsonPropertyName("height")] public double? Height { get; set; }

        [JsonPropertyName("fill")] public string? Fill { get; set; }

        [JsonPropertyName("stroke")] public string? Stroke { get; set; }

        [JsonPropertyName("strokeWidth")] public double? StrokeWidth { get; set; }

        [JsonPropertyName("opacity")] public double? Opacity { get; set; }
    }
}
