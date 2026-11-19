using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class TextOperationsTests
{
    private static TextShape Text(string content = "Hello") => new() { X = 10, Y = 20, Width = 30, Height = 12, Text = content, FontSize = 10 };

    private static (Document Document, Guid LayerId) Empty()
    {
        var document = Document.Create("L");
        return (document, document.Layers[0].Id);
    }

    private static TextShape TextIn(Document document, Guid id) => Assert.IsType<TextShape>(document.FindShape(id)!.Shape);

    [Fact]
    public void Create_OnALayer_AddsTheTextAboveTheOthers()
    {
        var (document, layerId) = Empty();
        document = document.AddShape(layerId, new RectangleShape { Width = 5, Height = 5 });
        var text = Text();

        var result = TextOperations.Create(document, layerId, text);

        Assert.Equal(text, result.Layers[0].Shapes[^1]);
        Assert.Equal(2, result.Layers[0].Shapes.Length);
    }

    [Fact]
    public void Create_UnknownContainer_Throws()
    {
        var (document, _) = Empty();

        Assert.Throws<KeyNotFoundException>(() => TextOperations.Create(document, Guid.NewGuid(), Text()));
    }

    [Fact]
    public void SetText_NewContent_ReplacesItAndKeepsTheRest()
    {
        var (document, layerId) = Empty();
        var text = Text() with { FontSize = 33, IsBold = true };
        document = TextOperations.Create(document, layerId, text);

        var result = TextOperations.SetText(document, text.Id, "Bye\nnow");

        Assert.Equal(text with { Text = "Bye\nnow" }, TextIn(result, text.Id));
    }

    [Fact]
    public void SetText_SameContent_ReturnsTheSameDocument()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);

        Assert.Same(document, TextOperations.SetText(document, text.Id, "Hello"));
    }

    [Fact]
    public void SetText_UnknownId_Throws()
    {
        var (document, _) = Empty();

        Assert.Throws<KeyNotFoundException>(() => TextOperations.SetText(document, Guid.NewGuid(), "x"));
    }

    [Fact]
    public void SetText_NotAText_Throws()
    {
        var (document, layerId) = Empty();
        var rectangle = new RectangleShape { Width = 5, Height = 5 };
        document = document.AddShape(layerId, rectangle);

        Assert.Throws<ArgumentException>(() => TextOperations.SetText(document, rectangle.Id, "x"));
    }

    [Fact]
    public void SetFormat_OnlyTheSetValues_AreChanged()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);

        var result = TextOperations.SetFormat(document, [text.Id], new TextFormatChange(FontSize: 40, IsItalic: true, Alignment: TextAlignType.Center));

        var changed = TextIn(result, text.Id);
        Assert.Equal(40, changed.FontSize);
        Assert.True(changed.IsItalic);
        Assert.Equal(TextAlignType.Center, changed.Alignment);
        Assert.Equal(text.FontFamily, changed.FontFamily);
        Assert.Equal(text.LineSpacing, changed.LineSpacing);
        Assert.False(changed.IsBold);
    }

    [Fact]
    public void SetFormat_SeveralTexts_ChangesAllOfThem()
    {
        var (document, layerId) = Empty();
        var first = Text("a");
        var second = Text("b") with { FontSize = 50 };
        document = TextOperations.Create(TextOperations.Create(document, layerId, first), layerId, second);

        var result = TextOperations.SetFormat(document, [first.Id, second.Id], new TextFormatChange(FontFamily: "Serif", LetterSpacing: 2));

        Assert.All(new[] { first.Id, second.Id }, id =>
        {
            Assert.Equal("Serif", TextIn(result, id).FontFamily);
            Assert.Equal(2, TextIn(result, id).LetterSpacing);
        });
        Assert.Equal(50, TextIn(result, second.Id).FontSize);
    }

    [Fact]
    public void SetFormat_TextInsideASelectedGroup_IsChanged()
    {
        var (document, layerId) = Empty();
        var text = Text();
        var rectangle = new RectangleShape { Width = 5, Height = 5 };
        document = document.AddShapes(layerId, [text, rectangle]);
        var groupId = Guid.NewGuid();
        document = document.Group([text.Id, rectangle.Id], groupId);

        var result = TextOperations.SetFormat(document, [groupId], new TextFormatChange(IsBold: true));

        Assert.True(TextIn(result, text.Id).IsBold);
    }

    [Fact]
    public void SetFormat_OtherShapesAndUnknownIds_AreIgnored()
    {
        var (document, layerId) = Empty();
        var rectangle = new RectangleShape { Width = 5, Height = 5 };
        document = document.AddShape(layerId, rectangle);

        var result = TextOperations.SetFormat(document, [rectangle.Id, Guid.NewGuid()], new TextFormatChange(FontSize: 40));

        Assert.Same(document, result);
    }

    [Fact]
    public void SetFormat_LockedOrHiddenText_IsNotChanged()
    {
        var (document, layerId) = Empty();
        var locked = Text("a") with { IsLocked = true };
        var hidden = Text("b") with { IsVisible = false };
        document = document.AddShapes(layerId, [locked, hidden]);

        var result = TextOperations.SetFormat(document, [locked.Id, hidden.Id], new TextFormatChange(FontSize: 40));

        Assert.Same(document, result);
    }

    [Fact]
    public void SetFormat_NothingDiffers_ReturnsTheSameDocument()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);

        Assert.Same(document, TextOperations.SetFormat(document, [text.Id], new TextFormatChange(FontSize: text.FontSize)));
    }

    [Fact]
    public void SetFormat_InvalidValue_Throws()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);

        Assert.Throws<ArgumentOutOfRangeException>(() => TextOperations.SetFormat(document, [text.Id], new TextFormatChange(FontSize: 0)));
    }

    [Fact]
    public void ApplyLayout_PointText_TakesTheMeasuredSize()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);

        var result = TextOperations.ApplyLayout(document, text.Id, new TextLayoutResult([], 77, 24));

        var changed = TextIn(result, text.Id);
        Assert.Equal((10d, 20d, 77d, 24d), (changed.X, changed.Y, changed.Width, changed.Height));
    }

    [Fact]
    public void ApplyLayout_AreaText_KeepsItsBox()
    {
        var (document, layerId) = Empty();
        var text = Text() with { IsArea = true };
        document = TextOperations.Create(document, layerId, text);

        Assert.Same(document, TextOperations.ApplyLayout(document, text.Id, new TextLayoutResult([], 77, 24)));
    }

    [Fact]
    public void ApplyLayout_SameSize_ReturnsTheSameDocument()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);

        Assert.Same(document, TextOperations.ApplyLayout(document, text.Id, new TextLayoutResult([], 30, 12)));
    }

    [Theory]
    [InlineData(90)]
    [InlineData(30)]
    [InlineData(200)]
    public void ApplyLayout_RotatedText_KeepsTheTurnedTopLeftCorner(double rotation)
    {
        var (document, layerId) = Empty();
        var text = Text() with { Rotation = rotation };
        document = TextOperations.Create(document, layerId, text);

        var result = TextOperations.ApplyLayout(document, text.Id, new TextLayoutResult([], 80, 30));

        var changed = TextIn(result, text.Id);
        var (beforeX, beforeY) = TurnedTopLeft(text);
        var (afterX, afterY) = TurnedTopLeft(changed);
        Assert.Equal(beforeX, afterX, 9);
        Assert.Equal(beforeY, afterY, 9);
        Assert.Equal(80, changed.Width);
    }

    [Fact]
    public void ApplyLayout_UnknownId_Throws()
    {
        var (document, _) = Empty();

        Assert.Throws<KeyNotFoundException>(() => TextOperations.ApplyLayout(document, Guid.NewGuid(), new TextLayoutResult([], 1, 1)));
    }

    [Fact]
    public void TextFormatChange_ApplyTo_WithNothingSet_EqualsTheText()
    {
        var text = Text() with { FontFamily = "Serif", LetterSpacing = 3 };

        Assert.Equal(text, new TextFormatChange().ApplyTo(text));
    }

    private static (double X, double Y) TurnedTopLeft(TextShape text)
    {
        var radians = text.Rotation * Math.PI / 180;
        var dx = -text.Width / 2;
        var dy = -text.Height / 2;
        return (
            text.CenterX + dx * Math.Cos(radians) - dy * Math.Sin(radians),
            text.CenterY + dx * Math.Sin(radians) + dy * Math.Cos(radians));
    }
}
