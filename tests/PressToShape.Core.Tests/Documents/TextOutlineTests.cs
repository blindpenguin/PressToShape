using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class TextOutlineTests
{
    private static readonly FakeTextOutliner Outliner = new();

    // Font size 10: a character is 6 wide and 10 high.
    private static TextShape Text(string content = "ab") => new()
    {
        X = 10,
        Y = 20,
        Width = 12,
        Height = 12,
        Text = content,
        FontSize = 10,
        Name = "Title",
        Appearance = new Appearance { Fill = Color.FromRgb(9, 8, 7), Stroke = null, StrokeWidth = 3, Opacity = 0.5 },
    };

    private static (Document Document, Guid LayerId) Empty()
    {
        var document = Document.Create("L");
        return (document, document.Layers[0].Id);
    }

    private sealed class CountingOutliner(int count) : ITextOutliner
    {
        public IReadOnlyList<DocumentSubPath>? Outline(TextShape text) =>
            [.. Enumerable.Repeat(new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(1, 0), new DocumentNode(1, 1)], isClosed: true), count)];
    }

    [Fact]
    public void ConvertToOutlines_Text_BecomesOneEvenOddCompoundPathWithTheSameLook()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);

        var result = TextOperations.ConvertToOutlines(document, [text.Id], Outliner);

        var path = Assert.IsType<PathShape>(result.FindShape(text.Id)!.Shape);
        Assert.Equal(2, path.SubPaths.Length);
        Assert.Equal(FillRule.EvenOdd, path.FillRule);
        Assert.Equal(text.Appearance, path.Appearance);
        Assert.Equal("Title", path.Name);
        Assert.Equal(10, path.X, 9);
        Assert.Equal(20, path.Y, 9);
    }

    [Fact]
    public void ConvertToOutlines_TextInTheMiddle_KeepsItsPlaceInTheStack()
    {
        var (document, layerId) = Empty();
        var below = new RectangleShape { Width = 5, Height = 5 };
        var text = Text();
        var above = new RectangleShape { Width = 5, Height = 5 };
        document = document.AddShapes(layerId, [below, text, above]);

        var result = TextOperations.ConvertToOutlines(document, [text.Id], Outliner);

        Assert.Equal([below.Id, text.Id, above.Id], result.Layers[0].Shapes.Select(shape => shape.Id));
        Assert.IsType<PathShape>(result.Layers[0].Shapes[1]);
    }

    [Fact]
    public void ConvertToOutlines_RotatedText_LooksTheSameOnTheScreen()
    {
        var (document, layerId) = Empty();
        var text = Text("abc") with { Rotation = 30 };
        document = TextOperations.Create(document, layerId, text);

        var result = TextOperations.ConvertToOutlines(document, [text.Id], Outliner);

        var path = Assert.IsType<PathShape>(result.FindShape(text.Id)!.Shape);
        Assert.Equal(30, path.Rotation, 9);
        var expected = Outliner.Outline(text)!.Select(contour => contour.Nodes[0]).Select(node => Turn(node.X, node.Y, text.CenterX, text.CenterY, 30)).ToArray();
        var actual = path.WithRotationApplied().ToDocumentSubPaths().Select(contour => contour.Nodes[0]).Select(node => (node.X, node.Y)).ToArray();
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].X, actual[i].X, 6);
            Assert.Equal(expected[i].Y, actual[i].Y, 6);
        }
    }

    [Fact]
    public void ConvertToOutlines_EmptyOrBlankText_IsSkipped()
    {
        var (document, layerId) = Empty();
        var empty = Text(string.Empty);
        var blank = Text("   ");
        document = document.AddShapes(layerId, [empty, blank]);

        Assert.Same(document, TextOperations.ConvertToOutlines(document, [empty.Id, blank.Id], Outliner));
        Assert.Empty(TextOperations.ConvertibleTextIds(document, [empty.Id, blank.Id], Outliner));
    }

    [Fact]
    public void ConvertToOutlines_TooManyContours_IsSkipped()
    {
        var (document, layerId) = Empty();
        var text = Text();
        document = TextOperations.Create(document, layerId, text);
        var tooMany = new CountingOutliner(PathShape.MaxSubPaths + 1);

        Assert.Same(document, TextOperations.ConvertToOutlines(document, [text.Id], tooMany));
        Assert.Empty(TextOperations.ConvertibleTextIds(document, [text.Id], tooMany));
    }

    [Fact]
    public void ConvertToOutlines_LockedOrHiddenText_IsSkipped()
    {
        var (document, layerId) = Empty();
        var locked = Text() with { IsLocked = true };
        var hidden = Text() with { IsVisible = false };
        document = document.AddShapes(layerId, [locked, hidden]);

        Assert.Same(document, TextOperations.ConvertToOutlines(document, [locked.Id, hidden.Id], Outliner));
    }

    [Fact]
    public void ConvertToOutlines_TextInASelectedGroup_IsConverted()
    {
        var (document, layerId) = Empty();
        var text = Text();
        var rectangle = new RectangleShape { Width = 5, Height = 5 };
        document = document.AddShapes(layerId, [text, rectangle]);
        var groupId = Guid.NewGuid();
        document = document.Group([text.Id, rectangle.Id], groupId);

        var result = TextOperations.ConvertToOutlines(document, [groupId], Outliner);

        Assert.IsType<PathShape>(result.FindShape(text.Id)!.Shape);
        Assert.IsType<RectangleShape>(result.FindShape(rectangle.Id)!.Shape);
        Assert.Equal([text.Id], TextOperations.ConvertibleTextIds(document, [groupId], Outliner));
    }

    [Fact]
    public void ConvertToOutlines_SeveralTexts_ConvertsAllInOneDocumentChange()
    {
        var (document, layerId) = Empty();
        var first = Text("a");
        var second = Text("b");
        document = document.AddShapes(layerId, [first, second]);

        var result = TextOperations.ConvertToOutlines(document, [first.Id, second.Id, first.Id], Outliner);

        Assert.All(result.Layers[0].Shapes, shape => Assert.IsType<PathShape>(shape));
        Assert.Equal(2, TextOperations.ConvertibleTextIds(document, [first.Id, second.Id, first.Id], Outliner).Count);
    }

    [Fact]
    public void ConvertToOutlines_NothingSelected_ReturnsTheSameDocument()
    {
        var (document, _) = Empty();

        Assert.Same(document, TextOperations.ConvertToOutlines(document, [Guid.NewGuid()], Outliner));
    }

    private static (double X, double Y) Turn(double x, double y, double centerX, double centerY, double degrees)
    {
        var radians = degrees * Math.PI / 180;
        var dx = x - centerX;
        var dy = y - centerY;
        return (centerX + dx * Math.Cos(radians) - dy * Math.Sin(radians), centerY + dx * Math.Sin(radians) + dy * Math.Cos(radians));
    }
}
