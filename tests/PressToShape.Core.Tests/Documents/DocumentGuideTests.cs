using System;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public sealed class DocumentGuideTests
{
    private static Guide Horizontal(double position) => new() { Orientation = GuideOrientation.Horizontal, Position = position };

    private static Guide Vertical(double position) => new() { Orientation = GuideOrientation.Vertical, Position = position };

    [Fact]
    public void Guides_NewDocument_AreEmpty()
    {
        Assert.Empty(Document.Create("L").Guides);
    }

    [Fact]
    public void AddGuide_AppendsInOrder()
    {
        var first = Horizontal(10);
        var second = Vertical(20);

        var document = Document.Create("L").AddGuide(first).AddGuide(second);

        Assert.Equal([first, second], document.Guides.ToArray());
    }

    [Fact]
    public void AddGuide_SameIdTwice_Throws()
    {
        var guide = Horizontal(10);
        var document = Document.Create("L").AddGuide(guide);

        Assert.ThrowsAny<ArgumentException>(() => document.AddGuide(guide with { Position = 50 }));
    }

    [Fact]
    public void AddGuide_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Document.Create("L").AddGuide(null!));
    }

    [Fact]
    public void MoveGuide_ChangesOnlyThatGuide()
    {
        var a = Horizontal(10);
        var b = Vertical(20);
        var document = Document.Create("L").AddGuide(a).AddGuide(b);

        var moved = document.MoveGuide(a.Id, 99);

        Assert.Equal(99, moved.FindGuide(a.Id)!.Position);
        Assert.Equal(GuideOrientation.Horizontal, moved.FindGuide(a.Id)!.Orientation);
        Assert.Equal(20, moved.FindGuide(b.Id)!.Position);
        Assert.Equal(10, document.FindGuide(a.Id)!.Position);
    }

    [Fact]
    public void MoveGuide_UnknownIdOrSamePosition_ReturnsTheSameDocument()
    {
        var guide = Horizontal(10);
        var document = Document.Create("L").AddGuide(guide);

        Assert.Same(document, document.MoveGuide(Guid.NewGuid(), 5));
        Assert.Same(document, document.MoveGuide(guide.Id, 10));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void MoveGuide_NonFinitePosition_Throws(double position)
    {
        var guide = Horizontal(10);
        var document = Document.Create("L").AddGuide(guide);

        Assert.ThrowsAny<ArgumentException>(() => document.MoveGuide(guide.Id, position));
    }

    [Fact]
    public void RemoveGuide_RemovesIt_AndUnknownIdChangesNothing()
    {
        var a = Horizontal(10);
        var b = Vertical(20);
        var document = Document.Create("L").AddGuide(a).AddGuide(b);

        var removed = document.RemoveGuide(a.Id);

        Assert.Equal([b], removed.Guides.ToArray());
        Assert.Same(document, document.RemoveGuide(Guid.NewGuid()));
    }

    [Fact]
    public void FindGuide_UnknownId_ReturnsNull()
    {
        Assert.Null(Document.Create("L").FindGuide(Guid.NewGuid()));
    }

    [Fact]
    public void Guide_NonFinitePositionOrEmptyId_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new Guide { Position = double.NaN });
        Assert.ThrowsAny<ArgumentException>(() => new Guide { Position = double.NegativeInfinity });
        Assert.ThrowsAny<ArgumentException>(() => new Guide { Id = Guid.Empty });
    }

    [Fact]
    public void Guides_Init_RejectsDuplicateIdsAndNulls()
    {
        var guide = Horizontal(1);
        var document = Document.Create("L");

        Assert.ThrowsAny<ArgumentException>(() => document with { Guides = [guide, guide] });
        Assert.ThrowsAny<ArgumentException>(() => document with { Guides = [null!] });
    }

    [Fact]
    public void Guides_DefaultArray_MeansNone()
    {
        var document = Document.Create("L") with { Guides = default };

        Assert.Empty(document.Guides);
    }

    [Fact]
    public void Guides_SurviveOtherEdits_AndLeaveLayersAlone()
    {
        var guide = Horizontal(10);
        var document = Document.Create("L").AddGuide(guide);
        var layers = document.Layers;

        var edited = document
            .AddShape(document.Layers[0].Id, new RectangleShape { X = 1, Y = 2, Width = 3, Height = 4 })
            .WithArtboardSize(500, 400)
            .AddSwatch(Color.FromRgb(1, 2, 3));

        Assert.Equal([guide], edited.Guides.ToArray());
        Assert.Equal(layers, document.AddGuide(Vertical(5)).Layers);
    }

    [Fact]
    public void Guides_AreNotShapes_TheyAreNotHitOrListed()
    {
        var document = Document.Create("L").AddGuide(Horizontal(10));

        Assert.Empty(document.HitTest(0, 10));
        Assert.Empty(document.Layers[0].Shapes);
    }
}
