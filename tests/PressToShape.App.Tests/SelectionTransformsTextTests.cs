using System.Collections.Generic;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionTransformsTextTests
{
    // Font size 20, box 100 x 40 at (100, 100): the corners are at x 100..200 and y 100..140.
    private static TextShape PointText(double rotation = 0) =>
        new() { X = 100, Y = 100, Width = 100, Height = 40, Text = "text", FontSize = 20, LetterSpacing = 2, Rotation = rotation };

    private static (IReadOnlyList<Shape> Shapes, SelectionFrame Frame) Selection(params Shape[] shapes) => (shapes, SelectionGeometry.FrameOf(shapes)!);

    private static TextShape Result(IReadOnlyList<Shape> shapes, int index = 0) => Assert.IsType<TextShape>(shapes[index]);

    [Fact]
    public void Resize_PointTextBottomRightCorner_ScalesTheFontAndTheBoxUniformly()
    {
        var (shapes, frame) = Selection(PointText());

        // Pointer at twice the distance from the top left corner, in x only: the text still scales uniformly.
        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 300, 140, keepProportions: false, fromCenter: false);

        var text = Result(resized);
        Assert.Equal(40, text.FontSize, 9);
        Assert.Equal(4, text.LetterSpacing, 9);
        Assert.Equal((100d, 100d, 200d, 80d), (text.X, text.Y, text.Width, text.Height));
    }

    [Fact]
    public void Resize_PointTextEdgeHandle_ScalesFromThatAxis()
    {
        var (shapes, frame) = Selection(PointText());

        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.Right, 150, 120, keepProportions: false, fromCenter: false);

        var text = Result(resized);
        Assert.Equal(10, text.FontSize, 9);
        Assert.Equal((50d, 20d), (text.Width, text.Height));
        Assert.Equal(100, text.X, 9);
    }

    [Fact]
    public void Resize_PointTextMuchSmaller_StopsAtTheSmallestFontSize()
    {
        var (shapes, frame) = Selection(PointText());

        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 101, 101, keepProportions: false, fromCenter: false);

        Assert.Equal(TextShape.MinFontSize, Result(resized).FontSize);
    }

    [Fact]
    public void Resize_PointTextMuchBigger_StopsAtTheLargestFontSize()
    {
        var (shapes, frame) = Selection(PointText() with { FontSize = 1500 });

        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 100 + 100 * 10, 140, keepProportions: false, fromCenter: false);

        Assert.Equal(TextShape.MaxFontSize, Result(resized).FontSize);
    }

    [Fact]
    public void Resize_AreaText_ChangesOnlyTheBox()
    {
        var area = PointText() with { IsArea = true };
        var (shapes, frame) = Selection(area);

        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 300, 120, keepProportions: false, fromCenter: false);

        var text = Result(resized);
        Assert.Equal(20, text.FontSize);
        Assert.Equal(2, text.LetterSpacing);
        Assert.Equal((200d, 20d), (text.Width, text.Height));
    }

    [Fact]
    public void Resize_PointTextAndRectangle_ScaleUniformlyAndOnlyTheTextChangesItsFont()
    {
        var rectangle = new RectangleShape { X = 300, Y = 100, Width = 100, Height = 40 };
        var (shapes, frame) = Selection(PointText(), rectangle);

        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 700, 100 + 40, keepProportions: false, fromCenter: false);

        // The frame is 300 x 40: dragging to x 700 doubles the width, the height follows.
        Assert.Equal(40, Result(resized, 0).FontSize, 9);
        var scaledRectangle = Assert.IsType<RectangleShape>(resized[1]);
        Assert.Equal(scaledRectangle.Width / 100, scaledRectangle.Height / 40, 9);
    }

    [Fact]
    public void Resize_AreaTextAndRectangle_AreNotForcedToScaleUniformly()
    {
        var area = PointText() with { IsArea = true };
        var rectangle = new RectangleShape { X = 300, Y = 100, Width = 100, Height = 40 };
        var (shapes, frame) = Selection(area, rectangle);

        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 700, 100 + 40, keepProportions: false, fromCenter: false);

        Assert.Equal(40, ((TextShape)resized[0]).Height);
    }

    [Fact]
    public void Resize_RotatedPointText_ScalesTheFontLikeAnUprightOne()
    {
        var (shapes, frame) = Selection(PointText(rotation: 30));
        var corner = (X: frame.CenterX + 50 * System.Math.Cos(0.5235987755982988) - 20 * System.Math.Sin(0.5235987755982988),
                      Y: frame.CenterY + 50 * System.Math.Sin(0.5235987755982988) + 20 * System.Math.Cos(0.5235987755982988));
        var anchorX = frame.CenterX - 50 * System.Math.Cos(0.5235987755982988) + 20 * System.Math.Sin(0.5235987755982988);
        var anchorY = frame.CenterY - 50 * System.Math.Sin(0.5235987755982988) - 20 * System.Math.Cos(0.5235987755982988);

        // The pointer is at twice the distance from the anchor corner.
        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, anchorX + 2 * (corner.X - anchorX), anchorY + 2 * (corner.Y - anchorY), keepProportions: false, fromCenter: false);

        Assert.Equal(40, Result(resized).FontSize, 6);
        Assert.Equal(30, Result(resized).Rotation, 9);
    }

    [Fact]
    public void SetSize_SinglePointTextWidth_ScalesTheFontAndTheHeightFollows()
    {
        var (shapes, frame) = Selection(PointText());

        var resized = SelectionTransforms.SetSize(shapes, frame, 150, null, keepProportions: false);

        var text = Result(resized);
        Assert.Equal(30, text.FontSize, 9);
        Assert.Equal(3, text.LetterSpacing, 9);
        Assert.Equal((150d, 60d), (text.Width, text.Height));
    }

    [Fact]
    public void SetSize_PointTextHeight_ScalesTheFontAndTheWidthFollows()
    {
        var (shapes, frame) = Selection(PointText());

        var resized = SelectionTransforms.SetSize(shapes, frame, null, 20, keepProportions: false);

        Assert.Equal(10, Result(resized).FontSize, 9);
        Assert.Equal(50, Result(resized).Width, 9);
    }

    [Fact]
    public void SetSize_AreaText_KeepsItsFont()
    {
        var (shapes, frame) = Selection(PointText() with { IsArea = true });

        var resized = SelectionTransforms.SetSize(shapes, frame, 150, null, keepProportions: false);

        Assert.Equal(20, Result(resized).FontSize);
        Assert.Equal((150d, 40d), (Result(resized).Width, Result(resized).Height));
    }

    [Fact]
    public void HasPointText_OnlyForPointTexts()
    {
        Assert.True(SelectionTransforms.HasPointText([PointText()]));
        Assert.False(SelectionTransforms.HasPointText([PointText() with { IsArea = true }]));
        Assert.False(SelectionTransforms.HasPointText([new RectangleShape { Width = 1, Height = 1 }]));
    }

    [Fact]
    public void Resize_RectangleOnly_IsStillFree()
    {
        var rectangle = new RectangleShape { X = 100, Y = 100, Width = 100, Height = 40 };
        var (shapes, frame) = Selection(rectangle);

        var resized = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 300, 120, keepProportions: false, fromCenter: false);

        Assert.Equal((200d, 20d), (resized[0].Width, resized[0].Height));
    }

    // ---- the drag through CanvasInteraction ----

    [Fact]
    public void Drag_ResizeHandleOfAPointText_ScalesItAndTheBoxFollowsTheMeasuredSize()
    {
        var measurer = new FakeTextMeasurer();
        var text = new TextShape { X = 100, Y = 100, Text = "ab", FontSize = 10 };
        text = text with { Width = measurer.Layout(text).Width, Height = measurer.Layout(text).Height };
        var document = Core.Documents.Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [text]);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        state.SelectOnly(text.Id);
        var interaction = new CanvasInteraction(state, measurer);

        // The box is 12 x 12, so the bottom right handle is at (112, 112).
        interaction.PointerPressed(112, 112);
        interaction.PointerMoved(124, 124);
        interaction.PointerReleased(124, 124);

        var resized = Assert.IsType<TextShape>(state.Document.FindShape(text.Id)!.Shape);
        Assert.Equal(20, resized.FontSize, 6);
        Assert.Equal(measurer.Layout(resized).Width, resized.Width, 6);
        Assert.Equal(measurer.Layout(resized).Height, resized.Height, 6);
        Assert.Equal((100d, 100d), (resized.X, resized.Y));
    }
}
