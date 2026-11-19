using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasSceneTextTests
{
    private static readonly FakeTextMeasurer Measurer = new();

    // Font size 10: a character is 6 wide, a line is 12 high, the first baseline is 10 (see FakeTextMeasurer).
    private static TextShape Text(string content = "ab\ncd") =>
        new() { X = 20, Y = 30, Width = 12, Height = 24, Text = content, FontSize = 10, FontFamily = "Serif", IsBold = true, LetterSpacing = 1 };

    private static (Document Document, Guid LayerId) Empty()
    {
        var document = Document.Create("L");
        return (document, document.Layers[0].Id);
    }

    private static ScreenShape Only(Document document, double zoom = 100, double originX = 0, double originY = 0, Guid? entered = null, Guid? hidden = null) =>
        Assert.Single(CanvasScene.Build(document, zoom, originX, originY, entered, hidden, Measurer));

    [Fact]
    public void Build_Text_IsATextShapeWithItsBoxAndLines()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text());

        var shape = Only(document);

        Assert.Equal(ScreenShapeType.Text, shape.Type);
        Assert.Equal((20d, 30d, 12d, 24d), (shape.X, shape.Y, shape.Width, shape.Height));
        Assert.NotNull(shape.Text);
        Assert.Equal(["ab", "cd"], shape.Text.Lines.Select(line => line.Text));
        Assert.Equal([10d, 22d], shape.Text.Lines.Select(line => line.Baseline));
        Assert.Equal("Serif", shape.Text.FontFamily);
        Assert.True(shape.Text.IsBold);
        Assert.False(shape.Text.IsItalic);
        Assert.Null(shape.Path);
    }

    [Fact]
    public void Build_Zoom_ScalesTheBoxTheLinesAndTheFont()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text());

        var shape = Only(document, zoom: 200, originX: 5, originY: 7);

        Assert.Equal((45d, 67d, 24d, 48d), (shape.X, shape.Y, shape.Width, shape.Height));
        Assert.Equal(20, shape.Text!.FontSize);
        Assert.Equal(2, shape.Text.LetterSpacing);
        Assert.Equal([20d, 44d], shape.Text.Lines.Select(line => line.Baseline));
    }

    [Fact]
    public void Build_Zoom_KeepsTheLineBreaksOfTheDocument()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text("aaa bbb") with { IsArea = true, Width = 24, Height = 30 });

        Assert.Equal(["aaa", "bbb"], Only(document, zoom: 50).Text!.Lines.Select(line => line.Text));
        Assert.Equal(["aaa", "bbb"], Only(document, zoom: 400).Text!.Lines.Select(line => line.Text));
    }

    [Fact]
    public void Build_LineX_FollowsTheAlignmentAndTheZoom()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text("abcd\nab") with { Alignment = TextAlignType.Right, LetterSpacing = 0 });

        var lines = Only(document, zoom: 200).Text!.Lines;

        Assert.Equal(0, lines[0].X);
        Assert.Equal(2 * 2 * 6, lines[1].X);
    }

    [Fact]
    public void Build_RotatedText_PassesTheRotationThrough()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text() with { Rotation = 45 });

        Assert.Equal(45, Only(document).RotationDegrees);
    }

    [Fact]
    public void Build_AppearanceOfTheText_IsUsedLikeForOtherShapes()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text() with
        {
            Appearance = new Appearance { Fill = Color.FromRgb(1, 2, 3), Stroke = Color.FromRgb(4, 5, 6), StrokeWidth = 2, Opacity = 0.5 },
        });

        var shape = Only(document, zoom: 200);

        Assert.Equal(Color.FromRgb(1, 2, 3), shape.Fill);
        Assert.Equal(Color.FromRgb(4, 5, 6), shape.Stroke);
        Assert.Equal(4, shape.StrokeWidth);
        Assert.Equal(0.5, shape.Opacity);
    }

    [Fact]
    public void Build_HiddenText_IsLeftOut()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text() with { IsVisible = false });

        Assert.Empty(CanvasScene.Build(document, 100, 0, 0, measurer: Measurer));
    }

    [Fact]
    public void Build_TextThatIsBeingEdited_IsLeftOutWhenItIsTheHiddenShape()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);

        Assert.Empty(CanvasScene.Build(document, 100, 0, 0, hiddenShapeId: text.Id, measurer: Measurer));
    }

    [Fact]
    public void Build_TextOutsideTheEnteredGroup_IsDimmed()
    {
        var (document, layerId) = Empty();
        var text = Text();
        var rectangle = new RectangleShape { Width = 5, Height = 5 };
        var other = new RectangleShape { Width = 5, Height = 5 };
        document = document.AddShapes(layerId, [text, rectangle, other]);
        var groupId = Guid.NewGuid();
        document = document.Group([rectangle.Id, other.Id], groupId);

        var shapes = CanvasScene.Build(document, 100, 0, 0, enteredGroupId: groupId, measurer: Measurer);

        Assert.True(shapes.Single(shape => shape.Type == ScreenShapeType.Text).IsDimmed);
        Assert.All(shapes.Where(shape => shape.Type != ScreenShapeType.Text), shape => Assert.False(shape.IsDimmed));
    }

    [Fact]
    public void Build_TextInsideAGroup_IsFlattenedAndDrawn()
    {
        var (document, layerId) = Empty();
        var text = Text();
        var rectangle = new RectangleShape { Width = 5, Height = 5 };
        document = document.AddShapes(layerId, [text, rectangle]);
        document = document.Group([text.Id, rectangle.Id], Guid.NewGuid());

        var shapes = CanvasScene.Build(document, 100, 0, 0, measurer: Measurer);

        Assert.Equal([ScreenShapeType.Text, ScreenShapeType.Rectangle], shapes.Select(shape => shape.Type));
    }

    [Fact]
    public void Build_EmptyText_HasNoLines()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text(string.Empty));

        Assert.Empty(Only(document).Text!.Lines);
    }

    [Fact]
    public void Build_WithoutAMeasurer_TextHasNoLines()
    {
        var (document, layerId) = Empty();
        document = TextOperations.Create(document, layerId, Text());

        var shape = Assert.Single(CanvasScene.Build(document, 100, 0, 0));

        Assert.Empty(shape.Text!.Lines);
    }

    [Fact]
    public void BuildShape_OtherShapes_HaveNoText()
    {
        var shape = CanvasScene.BuildShape(new RectangleShape { Width = 5, Height = 5 }, 100, 0, 0, Measurer);

        Assert.Null(shape.Text);
    }
}
