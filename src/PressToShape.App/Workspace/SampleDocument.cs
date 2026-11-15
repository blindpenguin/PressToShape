using System;
using PressToShape.App.Localization;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// TEMPORARY: a small demo drawing, so the running app shows something before the drawing tools exist.
/// Delete this file and its use in <c>DesktopShellViewModel</c> when the shape tools spec lands.
/// </summary>
internal static class SampleDocument
{
    private static readonly Color Ink = Color.FromRgb(0x1F, 0x29, 0x33);

    /// <summary>Creates the 1200 x 800 sample with a "Background" layer below a "Shapes" layer, named in the current language.</summary>
    public static Document Create(ILocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(localizer);

        var document = Document.Create(localizer.T("Background"));
        var backgroundId = document.Layers[0].Id;
        var shapes = new Layer { Name = localizer.T("Shapes") };

        return document
            .AddLayer(shapes)
            .AddShape(backgroundId, new RectangleShape
            {
                X = 100,
                Y = 100,
                Width = 1000,
                Height = 600,
                Appearance = new Appearance { Fill = Color.FromRgb(0xE8, 0xEE, 0xF4), Stroke = Color.FromRgb(0xA0, 0xAA, 0xB4), StrokeWidth = 2 },
            })
            .AddShape(backgroundId, new EllipseShape
            {
                X = 850,
                Y = 140,
                Width = 200,
                Height = 200,
                Appearance = new Appearance { Fill = Color.FromRgb(0xFF, 0xC8, 0x3C), Stroke = null },
            })
            .AddShape(shapes.Id, new RectangleShape
            {
                X = 200,
                Y = 250,
                Width = 360,
                Height = 240,
                Appearance = new Appearance { Fill = Color.FromRgb(0x4A, 0x90, 0xD9), Stroke = Ink, StrokeWidth = 4 },
            })
            .AddShape(shapes.Id, new EllipseShape
            {
                X = 420,
                Y = 330,
                Width = 380,
                Height = 300,
                Appearance = new Appearance { Fill = Color.FromRgb(0xE6, 0x5A, 0x5A), Stroke = null, Opacity = 0.5 },
            })
            .AddShape(shapes.Id, new EllipseShape
            {
                X = 780,
                Y = 420,
                Width = 260,
                Height = 160,
                Appearance = new Appearance { Fill = Color.FromRgb(0x5A, 0xBE, 0x78), Stroke = Ink, StrokeWidth = 4 },
            });
    }
}
