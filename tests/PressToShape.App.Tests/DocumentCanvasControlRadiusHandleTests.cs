using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PressToShape.App.Controls;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class DocumentCanvasControlRadiusHandleTests
{
    // The headless tests draw without pixels (UseHeadlessDrawing), so these tests check that drawing the radius handles does not throw.
    // Where the handles are is decided by RadiusHandles and SelectionOverlay, which have their own tests.
    private static void Render(SelectionOverlay? overlay, double handleSize = 8)
    {
        var control = new DocumentCanvasControl
        {
            Document = Document.Create("L"),
            Overlay = overlay,
            SelectionBrush = Brushes.Purple,
            SelectionHandleFillBrush = Brushes.White,
            HandleSize = handleSize,
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

    private static SelectionOverlay OverlayOf(Tool tool)
    {
        var rectangle = new RectangleShape { X = 50, Y = 50, Width = 200, Height = 100, Radii = CornerRadii.Uniform(20) };
        var document = Document.Create("L");
        document = document.AddShape(document.Layers[0].Id, rectangle);
        return SelectionOverlay.Build(document, [rectangle.Id], tool, 100, 0, 0);
    }

    [Fact]
    public void Render_SelectOverlayWithRadiusHandles_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var overlay = OverlayOf(Tool.Select);

        Assert.Equal(4, overlay.RadiusHandles.Count);
        Render(overlay);
    });

    [Fact]
    public void Render_DirectSelectOverlayWithRadiusHandlesAtTouchSize_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var overlay = OverlayOf(Tool.DirectSelect);

        Assert.Equal(4, overlay.RadiusHandles.Count);
        Render(overlay, handleSize: 18);
    });

    [Fact]
    public void Render_OverlayWithoutRadiusHandles_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(SelectionOverlay.Empty);
        Render(null);
    });
}
