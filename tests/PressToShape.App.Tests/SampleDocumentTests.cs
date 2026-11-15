using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SampleDocumentTests
{
    private static readonly Dictionary<string, string> German = new()
    {
        ["Background"] = "Hintergrund",
        ["Shapes"] = "Formen",
    };

    private static IEnumerable<Shape> AllShapes(PressToShape.Core.Documents.Document document) =>
        document.Layers.SelectMany(l => l.Shapes);

    [Fact]
    public void Create_English_HasBackgroundBelowShapes()
    {
        var document = SampleDocument.Create(FakeLocalizer.Create("en", German));

        Assert.Equal(new[] { "Background", "Shapes" }, document.Layers.Select(l => l.Name));
    }

    [Fact]
    public void Create_German_NamesTheLayersInGerman()
    {
        var document = SampleDocument.Create(FakeLocalizer.Create("de", German));

        Assert.Equal(new[] { "Hintergrund", "Formen" }, document.Layers.Select(l => l.Name));
    }

    [Fact]
    public void Create_Always_KeepsTheDefaultArtboard()
    {
        var document = SampleDocument.Create(FakeLocalizer.Create());

        Assert.Equal((1200, 800), (document.ArtboardWidth, document.ArtboardHeight));
    }

    [Fact]
    public void Create_Always_HasRectanglesAndEllipsesOnBothLayers()
    {
        var document = SampleDocument.Create(FakeLocalizer.Create());

        Assert.All(document.Layers, layer => Assert.NotEmpty(layer.Shapes));
        Assert.Contains(AllShapes(document), s => s is RectangleShape);
        Assert.Contains(AllShapes(document), s => s is EllipseShape);
    }

    [Fact]
    public void Create_Always_HasAStrokeAnAlphaFreeFillAndAnOpacityBelowOne()
    {
        var shapes = AllShapes(SampleDocument.Create(FakeLocalizer.Create())).ToList();

        Assert.Contains(shapes, s => s.Appearance.Stroke is not null && s.Appearance.StrokeWidth > 0);
        Assert.Contains(shapes, s => s.Appearance.Opacity < 1);
        Assert.Contains(shapes, s => s.Appearance.Fill is not null);
        Assert.Contains(shapes, s => s.Appearance.Stroke is null);
        Assert.True(shapes.Select(s => s.Appearance.Fill).Distinct().Count() > 1);
    }

    [Fact]
    public void Create_Always_KeepsEveryShapeInsideTheArtboard()
    {
        var document = SampleDocument.Create(FakeLocalizer.Create());

        Assert.All(AllShapes(document), s =>
        {
            Assert.InRange(s.X, 0, document.ArtboardWidth - s.Width);
            Assert.InRange(s.Y, 0, document.ArtboardHeight - s.Height);
        });
    }

    [Fact]
    public void Create_Always_HasShapesThatOverlapSoStackingIsVisible()
    {
        var document = SampleDocument.Create(FakeLocalizer.Create());

        var hits = document.HitTest(480, 400);

        Assert.True(hits.Count >= 2);
        Assert.Contains(hits, s => s is RectangleShape);
        Assert.Contains(hits, s => s is EllipseShape);
    }

    [Fact]
    public void Create_Always_GivesEveryShapeAndLayerAUniqueId()
    {
        var document = SampleDocument.Create(FakeLocalizer.Create());

        var ids = document.Layers.Select(l => l.Id).Concat(AllShapes(document).Select(s => s.Id)).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Create_NullLocalizer_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SampleDocument.Create(null!));
    }
}
