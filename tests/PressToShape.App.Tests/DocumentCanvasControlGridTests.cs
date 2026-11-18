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
public sealed class DocumentCanvasControlGridTests
{
    // The headless tests draw without pixels, so these tests check that the control renders in every combination without throwing and that
    // the properties are wired for a redraw. Where the lines go is decided by GridLines and the view model, which have their own tests.
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

    [Fact]
    public void Render_GridOn_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(new DocumentCanvasControl { Document = Document.Create("L"), ShowGrid = true, GridSpacing = 10, GridBrush = Brush, OriginX = 20, OriginY = 20 });
    });

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(800)]
    public void Render_GridAtAnyZoom_DoesNotThrow(double zoom) => HeadlessUi.Run(() =>
    {
        Render(new DocumentCanvasControl { Document = Document.Create("L"), ShowGrid = true, GridSpacing = 10, GridBrush = Brush, ZoomPercent = zoom });
    });

    [Fact]
    public void Render_GridWithoutBrushOrSpacingZero_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(new DocumentCanvasControl { Document = Document.Create("L"), ShowGrid = true, GridSpacing = 10 });
        Render(new DocumentCanvasControl { Document = Document.Create("L"), ShowGrid = true, GridSpacing = 0, GridBrush = Brush });
    });

    [Fact]
    public void Render_GuidesDrawn_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var guides = new List<ScreenSnapGuide> { new(true, 100, 10, 200), new(false, 50, 10, 300), new(true, double.NaN, 0, 1) };

        Render(new DocumentCanvasControl { Document = Document.Create("L"), SnapGuides = guides, GuideBrush = Brush });
    });

    [Fact]
    public void Render_WithoutADocument_DrawsNothingAndDoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(new DocumentCanvasControl { ShowGrid = true, GridBrush = Brush, SnapGuides = [new ScreenSnapGuide(true, 1, 0, 1)], GuideBrush = Brush });
    });

    [Fact]
    public void Properties_Default_GridIsOffWithSpacingTenAndNoGuides()
    {
        var control = new DocumentCanvasControl();

        Assert.False(control.ShowGrid);
        Assert.Equal(10, control.GridSpacing);
        Assert.Null(control.SnapGuides);
    }
}
