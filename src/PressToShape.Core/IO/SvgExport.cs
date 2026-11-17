using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.IO;

/// <summary>
/// Exports a document as SVG 1.1. The artboard becomes the viewport, every visible layer one group
/// (marked as an Inkscape layer), every shape a <c>rect</c>, <c>ellipse</c> or <c>path</c> and every group a <c>g</c> around its children.
/// Hidden shapes and the shapes below hidden groups are left out. There is no background and
/// the output holds no scripts, links or external references.
/// </summary>
public static class SvgExport
{
    private const string SvgNamespace = "http://www.w3.org/2000/svg";
    private const string InkscapeNamespace = "http://www.inkscape.org/namespaces/inkscape";

    /// <summary>Writes the document as UTF-8 SVG without a byte order mark.</summary>
    public static byte[] ToUtf8(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        using var stream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            CheckCharacters = true,
            CloseOutput = false,
        };

        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("svg", SvgNamespace);
            writer.WriteAttributeString("xmlns", "inkscape", null, InkscapeNamespace);
            writer.WriteAttributeString("version", "1.1");
            writer.WriteAttributeString("width", Number(document.ArtboardWidth));
            writer.WriteAttributeString("height", Number(document.ArtboardHeight));
            writer.WriteAttributeString("viewBox", $"0 0 {Number(document.ArtboardWidth)} {Number(document.ArtboardHeight)}");

            for (var i = 0; i < document.Layers.Length; i++)
            {
                WriteLayer(writer, document.Layers[i], i + 1);
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return stream.ToArray();
    }

    private static void WriteLayer(XmlWriter writer, Layer layer, int number)
    {
        if (!layer.IsVisible)
        {
            return;
        }

        writer.WriteStartElement("g", SvgNamespace);
        writer.WriteAttributeString("id", "layer-" + number.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("inkscape", "groupmode", InkscapeNamespace, "layer");
        writer.WriteAttributeString("inkscape", "label", InkscapeNamespace, XmlSafe(layer.Name));

        foreach (var shape in layer.Shapes)
        {
            WriteShape(writer, shape);
        }

        writer.WriteEndElement();
    }

    private static void WriteShape(XmlWriter writer, Shape shape)
    {
        // A hidden shape is left out with everything below it. Locked shapes are exported like any other.
        if (!shape.IsVisible)
        {
            return;
        }

        switch (shape)
        {
            case GroupShape group:
                WriteGroup(writer, group);
                return;
            case RectangleShape { EffectiveRadii.IsUniform: false } rounded:
                // SVG has one radius for all corners of a rect, so different radii are written as a path with arcs.
                writer.WriteStartElement("path", SvgNamespace);
                writer.WriteAttributeString("d", RoundedRectangleData(rounded));
                break;
            case RectangleShape rectangle:
                writer.WriteStartElement("rect", SvgNamespace);
                writer.WriteAttributeString("x", Number(shape.X));
                writer.WriteAttributeString("y", Number(shape.Y));
                writer.WriteAttributeString("width", Number(shape.Width));
                writer.WriteAttributeString("height", Number(shape.Height));
                if (rectangle.EffectiveRadii.TopLeft > 0)
                {
                    var radius = Number(rectangle.EffectiveRadii.TopLeft);
                    writer.WriteAttributeString("rx", radius);
                    writer.WriteAttributeString("ry", radius);
                }

                break;
            case EllipseShape:
                writer.WriteStartElement("ellipse", SvgNamespace);
                writer.WriteAttributeString("cx", Number(shape.X + shape.Width / 2));
                writer.WriteAttributeString("cy", Number(shape.Y + shape.Height / 2));
                writer.WriteAttributeString("rx", Number(shape.Width / 2));
                writer.WriteAttributeString("ry", Number(shape.Height / 2));
                break;
            case PathShape path:
                writer.WriteStartElement("path", SvgNamespace);
                writer.WriteAttributeString("d", PathData(path));
                if (path.FillRule == FillRule.EvenOdd)
                {
                    writer.WriteAttributeString("fill-rule", "evenodd");
                }

                break;
            default:
                throw new NotSupportedException($"Shape type {shape.GetType().Name} cannot be exported.");
        }

        var appearance = shape.Appearance;
        if (appearance.Fill is { } fill)
        {
            writer.WriteAttributeString("fill", Hex(fill));
            WriteAlpha(writer, "fill-opacity", fill.A);
        }
        else
        {
            writer.WriteAttributeString("fill", "none");
        }

        if (appearance.Stroke is { } stroke && appearance.StrokeWidth > 0)
        {
            writer.WriteAttributeString("stroke", Hex(stroke));
            writer.WriteAttributeString("stroke-width", Number(appearance.StrokeWidth));
            WriteAlpha(writer, "stroke-opacity", stroke.A);
        }

        if (appearance.Opacity < 1)
        {
            writer.WriteAttributeString("opacity", Number(appearance.Opacity));
        }

        if (shape.Rotation != 0)
        {
            // The box center is the turning point, like in the editor.
            writer.WriteAttributeString("transform", $"rotate({Number(shape.Rotation)} {Number(shape.CenterX)} {Number(shape.CenterY)})");
        }

        writer.WriteEndElement();
    }

    // Absolute coordinates in the frame before the rotation, which the transform of the element turns. A segment without handles is a
    // line. An open path gets no Z, so SVG fills it as if closed but draws no stroke on the closing line.
    // The outline clockwise from the top left: a line to the start of each arc, then the arc (a quarter circle, sweep flag 1 for clockwise).
    private static string RoundedRectangleData(RectangleShape rectangle)
    {
        var corners = RoundedRectangleGeometry.Corners(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, rectangle.EffectiveRadii);
        var data = new StringBuilder();
        for (var i = 0; i < corners.Count; i++)
        {
            var corner = corners[i];
            data.Append(i == 0 ? "M " : " L ").Append(Number(corner.Start.X)).Append(' ').Append(Number(corner.Start.Y));
            if (corner.Radius > 0)
            {
                var radius = Number(corner.Radius);
                data.Append(" A ").Append(radius).Append(' ').Append(radius).Append(" 0 0 1 ")
                    .Append(Number(corner.End.X)).Append(' ').Append(Number(corner.End.Y));
            }
        }

        return data.Append(" Z").ToString();
    }

    // One "M ..." run for every sub-path, so a compound path is one path element.
    private static string PathData(PathShape path) =>
        string.Join(' ', path.ToDocumentSubPaths().Select(SubPathData));

    private static string SubPathData(DocumentSubPath subPath)
    {
        var nodes = subPath.Nodes;
        var data = new StringBuilder();
        data.Append("M ").Append(Number(nodes[0].X)).Append(' ').Append(Number(nodes[0].Y));

        var segmentCount = subPath.IsClosed ? nodes.Count : nodes.Count - 1;
        for (var i = 0; i < segmentCount; i++)
        {
            var from = nodes[i];
            var to = nodes[(i + 1) % nodes.Count];
            var isLastOfClosed = subPath.IsClosed && i == nodes.Count - 1;
            if (!from.HasOut && !to.HasIn)
            {
                // The closing line of a closed sub-path is drawn by Z.
                if (!isLastOfClosed)
                {
                    data.Append(" L ").Append(Number(to.X)).Append(' ').Append(Number(to.Y));
                }

                continue;
            }

            data.Append(" C ")
                .Append(Number(from.OutX ?? from.X)).Append(' ').Append(Number(from.OutY ?? from.Y)).Append(' ')
                .Append(Number(to.InX ?? to.X)).Append(' ').Append(Number(to.InY ?? to.Y)).Append(' ')
                .Append(Number(to.X)).Append(' ').Append(Number(to.Y));
        }

        if (subPath.IsClosed)
        {
            data.Append(" Z");
        }

        return data.ToString();
    }

    // The children keep their document coordinates, so the group needs no transform.
    private static void WriteGroup(XmlWriter writer, GroupShape group)
    {
        writer.WriteStartElement("g", SvgNamespace);
        if (group.Name.Length > 0)
        {
            writer.WriteAttributeString("inkscape", "label", InkscapeNamespace, XmlSafe(group.Name));
        }

        foreach (var child in group.Children)
        {
            WriteShape(writer, child);
        }

        writer.WriteEndElement();
    }

    private static void WriteAlpha(XmlWriter writer, string name, byte alpha)
    {
        if (alpha < 255)
        {
            writer.WriteAttributeString(name, Number(alpha / 255.0));
        }
    }

    private static string Hex(Color color) =>
        string.Create(CultureInfo.InvariantCulture, $"#{color.R:x2}{color.G:x2}{color.B:x2}");

    // A dot as decimal separator and no exponent, whatever the culture. Negative zero is written as 0.
    private static string Number(double value)
    {
        var text = value.ToString("0.###############", CultureInfo.InvariantCulture);
        return text == "-0" ? "0" : text;
    }

    // XML cannot hold control characters or lone surrogates at all, so they are replaced.
    private static string XmlSafe(string text)
    {
        var builder = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                builder.Append(c).Append(text[++i]);
            }
            else if (c is '\t' or '\n' or '\r' || (c >= ' ' && c <= '퟿') || (c >= '' && c <= '�'))
            {
                builder.Append(c);
            }
            else
            {
                builder.Append('�');
            }
        }

        return builder.ToString();
    }
}
