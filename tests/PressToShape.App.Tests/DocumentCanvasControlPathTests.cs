using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PressToShape.App.Controls;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class DocumentCanvasControlPathTests
{
    // The headless tests draw without pixels (UseHeadlessDrawing), so these tests check that drawing a path does not throw for the
    // different cases. What is drawn is decided by PathOutline and CanvasScene, which have their own tests.
    private static Document DocumentWith(params Shape[] shapes)
    {
        var document = Document.Create("L");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static void Render(Document document, double zoom = 100)
    {
        var control = new DocumentCanvasControl { Document = document, ZoomPercent = zoom, Width = 400, Height = 300 };
        var window = new Window { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        using var bitmap = new RenderTargetBitmap(new PixelSize(400, 300));
        bitmap.Render(control);
        window.Close();
    }

    private static PathShape Curve(bool closed, FillRule rule = FillRule.NonZero) =>
        PathShape.Create(
            [new DocumentNode(20, 20, outX: 20, outY: 100), new DocumentNode(200, 150, inX: 100, inY: 150), new DocumentNode(260, 30)],
            closed,
            rule);

    [Fact]
    public void Render_OpenAndClosedPaths_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(DocumentWith(Curve(false), Curve(true), Curve(true, FillRule.EvenOdd)));
    });

    [Fact]
    public void Render_RotatedPathWithOpacityAndNoFill_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var path = Curve(false) with { Rotation = 40, Appearance = new Appearance { Fill = null, Stroke = Color.FromRgb(1, 2, 3), StrokeWidth = 3, Opacity = 0.4 } };

        Render(DocumentWith(path), zoom: 150);
    });

    [Fact]
    public void Render_StraightLinePathAndPathWithoutStroke_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var line = PathShape.Create([new DocumentNode(10, 50), new DocumentNode(200, 50)]);
        var unstroked = Curve(true) with { Appearance = Appearance.Default with { Stroke = null } };

        Render(DocumentWith(line, unstroked));
    });

    [Fact]
    public void Render_PathAsDraft_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var draft = CanvasScene.BuildShape(Curve(false), 100, 0, 0);
        var control = new DocumentCanvasControl { Document = Document.Create("L"), Draft = draft, Width = 400, Height = 300 };
        var window = new Window { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        using var bitmap = new RenderTargetBitmap(new PixelSize(400, 300));

        bitmap.Render(control);

        window.Close();
    });

    [Fact]
    public void Render_PenOverlayWithAndWithoutNodes_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var smooth = new ScreenPathNode(50, 50, 30, 50, 70, 50);
        var overlays = new[]
        {
            new ScreenPenOverlay([], false),
            new ScreenPenOverlay([smooth, new ScreenPathNode(150, 50), new ScreenPathNode(150, 150)], HighlightFirst: false),
            new ScreenPenOverlay([smooth, new ScreenPathNode(150, 50), new ScreenPathNode(150, 150)], HighlightFirst: true),
        };

        foreach (var overlay in overlays)
        {
            var control = new DocumentCanvasControl
            {
                Document = Document.Create("L"),
                PenOverlay = overlay,
                SelectionBrush = Avalonia.Media.Brushes.Purple,
                SelectionHandleFillBrush = Avalonia.Media.Brushes.White,
                HandleSize = 18,
                Width = 400,
                Height = 300,
            };
            var window = new Window { Content = control, Width = 400, Height = 300 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            using var bitmap = new RenderTargetBitmap(new PixelSize(400, 300));

            bitmap.Render(control);

            window.Close();
        }
    });

    [Fact]
    public void Render_HiddenShapeId_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var path = Curve(false);
        var document = DocumentWith(path);
        var control = new DocumentCanvasControl { Document = document, HiddenShapeId = path.Id, Width = 400, Height = 300 };
        var window = new Window { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        using var bitmap = new RenderTargetBitmap(new PixelSize(400, 300));

        bitmap.Render(control);

        window.Close();
    });

    [Fact]
    public void ScreenPenOverlay_From_MapsNodesHandlesAndTheHighlightToTheScreen()
    {
        var overlay = new PenOverlay([new DocumentNode(10, 20, 5, 20, 15, 20), new DocumentNode(30, 40)], HighlightFirst: true);

        var screen = ScreenPenOverlay.From(overlay, zoomPercent: 200, originX: 100, originY: 50);

        Assert.True(screen.HighlightFirst);
        Assert.Equal((120, 90), (screen.Nodes[0].X, screen.Nodes[0].Y));
        Assert.Equal((110, 90), (screen.Nodes[0].InX!.Value, screen.Nodes[0].InY!.Value));
        Assert.Equal((130, 90), (screen.Nodes[0].OutX!.Value, screen.Nodes[0].OutY!.Value));
        Assert.Equal((160, 130), (screen.Nodes[1].X, screen.Nodes[1].Y));
        Assert.False(screen.Nodes[1].HasIn);
    }
}
