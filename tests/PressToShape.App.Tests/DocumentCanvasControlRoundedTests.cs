using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PressToShape.App.Controls;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class DocumentCanvasControlRoundedTests
{
    // The headless tests draw without pixels (UseHeadlessDrawing), so a pixel check of the corners is not possible here. These tests check
    // that drawing rounded rectangles does not throw. The radii that are drawn are decided by CanvasScene, which has its own tests.
    private static void Render(params Shape[] shapes)
    {
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, shapes);
        var control = new DocumentCanvasControl { Document = document, Width = 400, Height = 300 };
        var window = new Window { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        using var bitmap = new RenderTargetBitmap(new PixelSize(400, 300));
        bitmap.Render(control);
        window.Close();
    }

    [Fact]
    public void Render_RoundedRectangles_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(
            new RectangleShape { X = 10, Y = 10, Width = 100, Height = 60, Radii = CornerRadii.Uniform(15) },
            new RectangleShape { X = 150, Y = 10, Width = 100, Height = 60, Radii = new CornerRadii(0, 30, 5, 60) },
            new RectangleShape { X = 10, Y = 100, Width = 100, Height = 60, Radii = CornerRadii.Uniform(1000), Rotation = 90 });
    });

    [Fact]
    public void Render_RoundedRectangleWithoutFillOrStrokeAndWithOpacity_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(
            new RectangleShape { Width = 80, Height = 40, Radii = CornerRadii.Uniform(10), Appearance = new Appearance { Fill = null, Stroke = null } },
            new RectangleShape { X = 100, Width = 80, Height = 40, Radii = CornerRadii.Uniform(10), Appearance = Appearance.Default with { Opacity = 0.3, StrokeWidth = 6 } });
    });

    [Fact]
    public void Render_FlatRoundedRectangle_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(new RectangleShape { Width = 80, Height = 0, Radii = CornerRadii.Uniform(10) });
    });
}
