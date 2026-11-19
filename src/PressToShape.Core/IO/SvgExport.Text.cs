using System.Xml;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.IO;

public static partial class SvgExport
{
    // The start of the element and the font attributes. The appearance and the transform follow from WriteShape, then the lines.
    private static void WriteTextStart(XmlWriter writer, TextShape text)
    {
        writer.WriteStartElement("text", SvgNamespace);
        writer.WriteAttributeString("xml", "space", null, "preserve");
        writer.WriteAttributeString("font-family", XmlSafe(text.FontFamily));
        writer.WriteAttributeString("font-size", Number(text.FontSize));
        if (text.IsBold)
        {
            writer.WriteAttributeString("font-weight", "bold");
        }

        if (text.IsItalic)
        {
            writer.WriteAttributeString("font-style", "italic");
        }

        if (text.LetterSpacing != 0)
        {
            writer.WriteAttributeString("letter-spacing", Number(text.LetterSpacing));
        }

        var anchor = text.Alignment switch
        {
            TextAlignType.Center => "middle",
            TextAlignType.Right => "end",
            _ => null,
        };
        if (anchor is not null)
        {
            writer.WriteAttributeString("text-anchor", anchor);
        }
    }

    // One tspan per line with an absolute position. With an anchor the line is placed by the center or the right edge of the reference
    // width (the box for area text, the widest line for point text), which is how the measurer aligns the lines.
    private static void WriteTextLines(XmlWriter writer, TextShape text, ITextMeasurer measurer)
    {
        var layout = measurer.Layout(text);
        var reference = text.IsArea ? text.Width : layout.Width;
        foreach (var line in layout.Lines)
        {
            var x = text.Alignment switch
            {
                TextAlignType.Center => text.X + reference / 2,
                TextAlignType.Right => text.X + reference,
                _ => text.X + line.X,
            };
            writer.WriteStartElement("tspan", SvgNamespace);
            writer.WriteAttributeString("x", Number(x));
            writer.WriteAttributeString("y", Number(text.Y + line.Baseline));
            writer.WriteString(XmlSafe(line.Text));
            writer.WriteEndElement();
        }
    }
}
