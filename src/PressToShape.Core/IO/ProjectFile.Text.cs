using System;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.IO;

public static partial class ProjectFile
{
    private const string AlignCenterText = "center";
    private const string AlignRightText = "right";

    // Missing entries mean the defaults of a new text. A value outside the limits of TextShape makes the model throw, which Read reports as
    // an invalid value. The font name is kept as it is, so a font that is not installed is not lost.
    private static TextShape? BuildText(ShapeDto dto, Guid id, Appearance appearance, string name, bool visible, bool locked, out ProjectFileResult? failure)
    {
        failure = null;
        TextAlignType alignment;
        switch (dto.Align)
        {
            case null:
                alignment = TextAlignType.Left;
                break;
            case AlignCenterText:
                alignment = TextAlignType.Center;
                break;
            case AlignRightText:
                alignment = TextAlignType.Right;
                break;
            default:
                failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A text has an unknown alignment: " + dto.Align);
                return null;
        }

        return new TextShape
        {
            Id = id,
            X = dto.X!.Value,
            Y = dto.Y!.Value,
            Width = dto.Width!.Value,
            Height = dto.Height!.Value,
            Appearance = appearance,
            Rotation = dto.Rotation ?? 0,
            Name = name,
            IsVisible = visible,
            IsLocked = locked,
            Text = dto.Text ?? string.Empty,
            IsArea = dto.Area ?? false,
            FontFamily = dto.Font ?? TextShape.DefaultFontFamily,
            FontSize = dto.FontSize ?? TextShape.DefaultFontSize,
            IsBold = dto.Bold ?? false,
            IsItalic = dto.Italic ?? false,
            Alignment = alignment,
            LineSpacing = dto.LineSpacing ?? 1,
            LetterSpacing = dto.LetterSpacing ?? 0,
        };
    }

    private static void AddTextData(ShapeDto dto, TextShape text)
    {
        dto.Text = ReplaceLoneSurrogates(text.Text);
        dto.Area = text.IsArea ? true : null;
        dto.Font = ReplaceLoneSurrogates(text.FontFamily);
        dto.FontSize = text.FontSize;
        dto.Bold = text.IsBold ? true : null;
        dto.Italic = text.IsItalic ? true : null;
        dto.Align = text.Alignment switch
        {
            TextAlignType.Center => AlignCenterText,
            TextAlignType.Right => AlignRightText,
            _ => null,
        };
        dto.LineSpacing = text.LineSpacing == 1 ? null : text.LineSpacing;
        dto.LetterSpacing = text.LetterSpacing == 0 ? null : text.LetterSpacing;
    }
}
