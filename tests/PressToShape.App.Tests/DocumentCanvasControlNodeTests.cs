using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class DocumentCanvasControlNodeTests
{
    // The headless tests draw without pixels (UseHeadlessDrawing), so these tests check that drawing the overlay does not throw and that
    // the binding delivers it. What is shown is decided by NodeOverlayBuilder, which has its own tests.
    private static void Render(ScreenNodeOverlay? overlay, double handleSize = 8)
    {
        var control = new DocumentCanvasControl
        {
            Document = Document.Create("L"),
            NodeOverlay = overlay,
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

    [Fact]
    public void Render_OverlayWithSelectedAndUnselectedNodesAndHandles_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(new ScreenNodeOverlay(
        [
            new ScreenOverlayNode(50, 50, IsSelected: false, OutX: 80, OutY: 20),
            new ScreenOverlayNode(150, 50, IsSelected: true, InX: 120, InY: 50, OutX: 180, OutY: 50),
            new ScreenOverlayNode(250, 150, IsSelected: false, InX: 230, InY: 120),
        ]));
    });

    [Fact]
    public void Render_TouchSizedHandles_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(new ScreenNodeOverlay([new ScreenOverlayNode(100, 100, IsSelected: true, OutX: 150, OutY: 100)]), handleSize: 18);
    });

    [Fact]
    public void Render_EmptyOrMissingOverlay_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(new ScreenNodeOverlay([]));
        Render(null);
    });

    [Fact]
    public void Render_OverlayWithoutBrushes_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var control = new DocumentCanvasControl
        {
            Document = Document.Create("L"),
            NodeOverlay = new ScreenNodeOverlay([new ScreenOverlayNode(10, 10, true)]),
            Width = 100,
            Height = 100,
        };
        var window = new Window { Content = control, Width = 100, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        using var bitmap = new RenderTargetBitmap(new PixelSize(100, 100));

        bitmap.Render(control);

        window.Close();
    });

    [Fact]
    public void Binding_DeliversTheNodeOverlayOfTheViewModelToTheControl() => HeadlessUi.Run(() =>
    {
        var path = PathShape.Create([new DocumentNode(0, 100), new DocumentNode(100, 100), new DocumentNode(200, 100)]);
        var document = Document.Create("L");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [path])) { ActiveTool = Tool.Node };
        state.Select(path.Id);
        var viewModel = new CanvasAreaViewModel(state) { ViewportWidth = 800, ViewportHeight = 600 };
        var view = new CanvasAreaView { DataContext = viewModel };
        var window = new Window { Content = view, Width = 800, Height = 600 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var control = view.GetVisualDescendants().OfType<DocumentCanvasControl>().Single();

        Assert.NotNull(control.NodeOverlay);
        Assert.Equal(3, control.NodeOverlay.Nodes.Count);

        state.ActiveTool = Tool.Select;
        Dispatcher.UIThread.RunJobs();

        Assert.Null(control.NodeOverlay);
        window.Close();
    });
}
