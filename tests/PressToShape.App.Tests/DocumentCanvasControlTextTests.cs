using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PressToShape.App.Controls;
using PressToShape.App.Services;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class DocumentCanvasControlTextTests
{
    // The headless tests draw without pixels (UseHeadlessDrawing), so these tests check that drawing a text does not throw for the different
    // cases, with the real text engine. What is drawn and where is decided by CanvasScene, which has its own tests.
    private static TextShape Text(string content = "Hello\nWorld") =>
        new() { X = 20, Y = 30, Width = 100, Height = 50, Text = content, FontSize = 24, FontFamily = "Test Font" };

    private static void Render(TextShape text, double zoom = 100, ITextMeasurer? measurer = null)
    {
        var document = Document.Create("L");
        document = TextOperations.Create(document, document.Layers[0].Id, text);
        var control = new DocumentCanvasControl
        {
            Document = document,
            ZoomPercent = zoom,
            TextMeasurer = measurer ?? new AvaloniaTextMeasurer(),
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
    public void Render_FilledText_DoesNotThrow() => HeadlessUi.Run(() => Render(Text()));

    [Fact]
    public void Render_StrokedTextWithACharacterOfAnotherScript_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(Text("ab \U0001F600 \u4e2d\u6587 cd") with { Appearance = new Appearance { Fill = Color.FromRgb(9, 9, 9), Stroke = Color.FromRgb(1, 2, 3), StrokeWidth = 2, Opacity = 1 }, LetterSpacing = 2 });
    });

    [Fact]
    public void Render_StrokedTextWithoutFill_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(Text() with { Appearance = new Appearance { Fill = null, Stroke = Color.FromRgb(1, 2, 3), StrokeWidth = 2, Opacity = 1 } });
    });

    [Fact]
    public void Render_FillAndStrokeRotatedWithOpacityAtZoom_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var text = Text() with
        {
            Rotation = 35,
            IsBold = true,
            IsItalic = true,
            LetterSpacing = 2,
            LineSpacing = 1.5,
            Alignment = TextAlignType.Center,
            Appearance = new Appearance { Fill = Color.FromRgb(9, 9, 9), Stroke = Color.FromRgb(200, 0, 0), StrokeWidth = 1, Opacity = 0.5 },
        };

        Render(text, zoom: 150);
    });

    [Fact]
    public void Render_AreaTextWithTheDefaultFont_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(Text("one two three four five") with { IsArea = true, FontFamily = TextShape.DefaultFontFamily });
    });

    [Fact]
    public void Render_EmptyTextAndTextWithEmptyLines_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        Render(Text(string.Empty));
        Render(Text("a\n\nb"));
    });

    [Fact]
    public void Render_NoNeedForAMeasurerToBeSet_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var document = Document.Create("L");
        document = TextOperations.Create(document, document.Layers[0].Id, Text());
        var control = new DocumentCanvasControl { Document = document, Width = 400, Height = 300 };
        var window = new Window { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        using var bitmap = new RenderTargetBitmap(new PixelSize(400, 300));
        bitmap.Render(control);
        window.Close();
    });
}
