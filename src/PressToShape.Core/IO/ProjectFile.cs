using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
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
    public const int FormatVersion = 2;

    /// <summary>The largest content <see cref="Read"/> accepts, in bytes.</summary>
    public const long MaxBytes = 64L * 1024 * 1024;

    private const string FormatName = "PressToShape";
    private const string RectangleType = "rectangle";
    private const string EllipseType = "ellipse";
    private const string GroupType = "group";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        MaxDepth = 256,
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
            Palette = document.Palette.IsEmpty ? null : new List<string?>(document.Palette.Select(color => ToText(color))),
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
                var shape = BuildShape(shapeDto, 0, out var failure);
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

        var palette = ImmutableArray.CreateBuilder<Color>();
        foreach (var text in file.Palette ?? [])
        {
            // A swatch is always a color: null ("no color") is not allowed here.
            if (text is null || !TryParseColor(text, out var swatch) || swatch is not { } color)
            {
                return ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A palette entry is not a color.");
            }

            palette.Add(color);
        }

        var document = Document.Create(string.Empty) with
        {
            ArtboardWidth = width,
            ArtboardHeight = height,
            Layers = layers.ToImmutable(),
            Palette = palette.ToImmutable(),
        };
        return ProjectFileResult.Success(document);
    }

    private static Shape? BuildShape(ShapeDto? dto, int groupDepth, out ProjectFileResult? failure)
    {
        failure = null;
        if (dto is null || dto.Type is null)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A shape misses an entry.");
            return null;
        }

        if (!TryParseId(dto.Id, out var id))
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A shape has no valid id.");
            return null;
        }

        return dto.Type == GroupType ? BuildGroup(dto, id, groupDepth, out failure) : BuildPlainShape(dto, id, out failure);
    }

    private static GroupShape? BuildGroup(ShapeDto dto, Guid id, int groupDepth, out ProjectFileResult? failure)
    {
        failure = null;
        if (dto.Children is null)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A group misses its children.");
            return null;
        }

        if (groupDepth + 1 > ShapeTree.MaxDepth)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, $"Groups are nested deeper than {ShapeTree.MaxDepth} levels.");
            return null;
        }

        if (dto.Children.Count == 0)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A group has no children.");
            return null;
        }

        var children = ImmutableArray.CreateBuilder<Shape>(dto.Children.Count);
        foreach (var childDto in dto.Children)
        {
            var child = BuildShape(childDto, groupDepth + 1, out failure);
            if (child is null)
            {
                return null;
            }

            children.Add(child);
        }

        // The box of a group follows from its children, so the file has none.
        return new GroupShape { Id = id, Children = children.MoveToImmutable(), Name = dto.Name ?? string.Empty, IsVisible = dto.Visible ?? true, IsLocked = dto.Locked ?? false };
    }

    private static Shape? BuildPlainShape(ShapeDto dto, Guid id, out ProjectFileResult? failure)
    {
        failure = null;
        if (dto.X is null || dto.Y is null || dto.Width is null || dto.Height is null || dto.StrokeWidth is null || dto.Opacity is null)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A shape misses an entry.");
            return null;
        }

        if (!TryParseColor(dto.Fill, out var fill) || !TryParseColor(dto.Stroke, out var stroke))
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A shape has an invalid color.");
            return null;
        }

        var appearance = new Appearance { Fill = fill, Stroke = stroke, StrokeWidth = dto.StrokeWidth.Value, Opacity = dto.Opacity.Value };
        var name = dto.Name ?? string.Empty;
        var visible = dto.Visible ?? true;
        var locked = dto.Locked ?? false;
        switch (dto.Type)
        {
            case RectangleType:
                return new RectangleShape { Id = id, X = dto.X.Value, Y = dto.Y.Value, Width = dto.Width.Value, Height = dto.Height.Value, Appearance = appearance, Rotation = dto.Rotation ?? 0, Name = name, IsVisible = visible, IsLocked = locked };
            case EllipseType:
                return new EllipseShape { Id = id, X = dto.X.Value, Y = dto.Y.Value, Width = dto.Width.Value, Height = dto.Height.Value, Appearance = appearance, Rotation = dto.Rotation ?? 0, Name = name, IsVisible = visible, IsLocked = locked };
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

    private static ShapeDto ToDto(Shape shape)
    {
        var dto = new ShapeDto
        {
            Id = shape.Id.ToString("D", CultureInfo.InvariantCulture),
            Name = shape.Name.Length == 0 ? null : ReplaceLoneSurrogates(shape.Name),
            Visible = shape.IsVisible ? null : false,
            Locked = shape.IsLocked ? true : null,
        };

        if (shape is GroupShape group)
        {
            dto.Type = GroupType;
            dto.Children = new List<ShapeDto?>(group.Children.Select(ToDto));
            return dto;
        }

        dto.Type = shape switch
        {
            RectangleShape => RectangleType,
            EllipseShape => EllipseType,
            _ => throw new NotSupportedException($"Shape type {shape.GetType().Name} cannot be saved."),
        };
        dto.X = shape.X;
        dto.Y = shape.Y;
        dto.Width = shape.Width;
        dto.Height = shape.Height;
        dto.Fill = ToText(shape.Appearance.Fill);
        dto.Stroke = ToText(shape.Appearance.Stroke);
        dto.StrokeWidth = shape.Appearance.StrokeWidth;
        dto.Opacity = shape.Appearance.Opacity;
        dto.Rotation = shape.Rotation == 0 ? null : shape.Rotation;
        return dto;
    }

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
}
