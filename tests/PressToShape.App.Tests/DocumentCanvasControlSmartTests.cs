using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PressToShape.App.Controls;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public sealed class DocumentCanvasControlSmartTests
{
    // The headless tests draw without pixels, so these tests check that the control renders every kind of marker and guide without throwing
    // and that the properties are wired for a redraw. Where the lines go is decided by the view model, which has its own tests.
    private static void Render(DocumentCanvasControl control)
    {
        control.Width = 400;
        control.Height = 300;
        var window = new Window { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        using var bitmap = new RenderTargetBitmap(new PixelSize(400, 300));
        bitmap.Render(control);
        window.Close();
    }

    private static readonly IBrush Brush = new SolidColorBrush(Colors.Red);

    private static DocumentCanvasControl Control() => new() { Document = Document.Create("L") };

    private static List<ScreenRulerGuide> Guides() => [new(true, 100, false, false), new(false, 50, true, false), new(true, 200, false, true), new(false, double.NaN, false, false)];

    private static List<ScreenSmartMarker> Markers() =>
    [
        new(SmartMarkerType.Distance, true, 10, 90, 40, "80"),
        new(SmartMarkerType.Distance, false, 10, 90, 40, "80"),
        new(SmartMarkerType.EqualGap, true, 100, 150, 60, "50"),
        new(SmartMarkerType.EqualSize, true, 20, 120, 100, "100"),
        new(SmartMarkerType.EqualSize, false, 20, 120, 100, null),
        new(SmartMarkerType.PathPoint, true, 50, 50, 60, null),
        new(SmartMarkerType.Distance, true, double.NaN, 10, 10, "bad"),
    ];

    [Fact]
    public void Render_RulerGuides_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var control = Control();
        control.RulerGuides = Guides();
        control.RulerGuideBrush = Brush;
        control.RulerGuideSelectedBrush = Brush;
        control.RulerGuideActiveBrush = Brush;

        Render(control);
    });

    [Fact]
    public void Render_EveryMarkerType_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var control = Control();
        control.SmartMarkers = Markers();
        control.SmartGuideBrush = Brush;
        control.SmartGuideTextBrush = Brush;
        control.SmartGuideLabelBackgroundBrush = Brush;

        Render(control);
    });

    [Fact]
    public void Render_WithoutBrushes_OrWithoutADocument_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var noBrushes = Control();
        noBrushes.RulerGuides = Guides();
        noBrushes.SmartMarkers = Markers();
        Render(noBrushes);

        var noDocument = new DocumentCanvasControl { RulerGuides = Guides(), SmartMarkers = Markers(), SmartGuideBrush = Brush, RulerGuideBrush = Brush };
        Render(noDocument);
    });

    [Fact]
    public void Render_MarkersWithTheLineBrushOnlyOrWithoutTextBrush_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var control = Control();
        control.SmartMarkers = Markers();
        control.SmartGuideBrush = Brush;

        Render(control);
    });

    [Fact]
    public void Properties_Default_AreEmpty()
    {
        var control = new DocumentCanvasControl();

        Assert.Null(control.RulerGuides);
        Assert.Null(control.SmartMarkers);
        Assert.Null(control.RulerGuideBrush);
        Assert.Null(control.SmartGuideBrush);
    }

    [Fact]
    public void ThemeBrushes_ExistForBothVariants() => HeadlessUi.Run(() =>
    {
        foreach (var key in new[] { "RulerGuideBrush", "RulerGuideSelectedBrush", "RulerGuideActiveBrush", "SmartGuideBrush", "SmartGuideTextBrush", "SmartGuideLabelBackgroundBrush" })
        {
            foreach (var variant in new[] { Avalonia.Styling.ThemeVariant.Light, Avalonia.Styling.ThemeVariant.Dark })
            {
                Assert.True(Application.Current!.TryGetResource(key, variant, out var value), $"{key} {variant}");
                Assert.IsAssignableFrom<IBrush>(value);
            }
        }
    });
}
