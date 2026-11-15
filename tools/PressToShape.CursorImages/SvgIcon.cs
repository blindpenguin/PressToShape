using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using SkiaSharp;

namespace PressToShape.CursorImages;

/// <summary>
/// Reads a Lucide icon (a 24 x 24 SVG made of <c>path</c>, <c>circle</c> and <c>line</c> elements) into one Skia path. Anything
/// else is rejected, so a changed icon cannot be drawn wrong without anybody noticing.
/// </summary>
internal static class SvgIcon
{
    private const string ExpectedViewBox = "0 0 24 24";

    /// <summary>Reads the icon. Throws <see cref="InvalidDataException"/> with the file name for anything it cannot draw.</summary>
    public static SKPath Load(string file)
    {
        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"The icon file does not exist: {file}", file);
        }

        XDocument document;
        try
        {
            document = XDocument.Load(file);
        }
        catch (XmlException ex)
        {
            throw new InvalidDataException($"{file}: not valid XML ({ex.Message})", ex);
        }

        var root = document.Root;
        if (root is null || root.Name.LocalName != "svg")
        {
            throw new InvalidDataException($"{file}: the root element is not svg.");
        }

        var viewBox = (string?)root.Attribute("viewBox");
        if (viewBox != ExpectedViewBox)
        {
            throw new InvalidDataException($"{file}: the view box is '{viewBox}', expected '{ExpectedViewBox}'.");
        }

        var path = new SKPath();
        foreach (var element in root.Descendants())
        {
            AddElement(file, path, element);
        }

        return path;
    }

    private static void AddElement(string file, SKPath path, XElement element)
    {
        switch (element.Name.LocalName)
        {
            case "path":
                var data = Attribute(file, element, "d");
                var parsed = SKPath.ParseSvgPathData(data)
                    ?? throw new InvalidDataException($"{file}: the path data of a path element is not valid.");
                path.AddPath(parsed);
                break;
            case "circle":
                path.AddCircle(Number(file, element, "cx"), Number(file, element, "cy"), Number(file, element, "r"));
                break;
            case "line":
                path.MoveTo(Number(file, element, "x1"), Number(file, element, "y1"));
                path.LineTo(Number(file, element, "x2"), Number(file, element, "y2"));
                break;
            default:
                throw new InvalidDataException(
                    $"{file}: the element '{element.Name.LocalName}' is not supported. Only path, circle and line are drawn.");
        }
    }

    private static string Attribute(string file, XElement element, string name) =>
        (string?)element.Attribute(name)
        ?? throw new InvalidDataException($"{file}: the {element.Name.LocalName} element has no '{name}' attribute.");

    private static float Number(string file, XElement element, string name)
    {
        var text = Attribute(file, element, name);
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !float.IsFinite(value))
        {
            throw new InvalidDataException($"{file}: '{name}' of a {element.Name.LocalName} element is not a number: '{text}'.");
        }

        return value;
    }
}
