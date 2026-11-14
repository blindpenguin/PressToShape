using System;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class DocumentLayerTests
{
    private static readonly Guid LayerA = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid LayerB = new("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid ShapeA = new("bbbbbbbb-0000-0000-0000-000000000001");

    private static Document TwoLayers()
    {
        var document = Document.Create("ignored");
        return document with { Layers = [new Layer { Id = LayerA, Name = "A" }, new Layer { Id = LayerB, Name = "B" }] };
    }

    [Fact]
    public void Create_FirstLayerName_Has1200By800AndOneVisibleUnlockedLayer()
    {
        var document = Document.Create("Layer 1");

        Assert.Equal(1200, document.ArtboardWidth);
        Assert.Equal(800, document.ArtboardHeight);
        var layer = Assert.Single(document.Layers);
        Assert.Equal("Layer 1", layer.Name);
        Assert.True(layer.IsVisible);
        Assert.False(layer.IsLocked);
    }

    [Fact]
    public void Create_NullName_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Document.Create(null!));
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(double.NaN, 100)]
    [InlineData(double.PositiveInfinity, 100)]
    public void WithArtboardSize_InvalidWidth_Throws(double width, double height)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Document.Create("L").WithArtboardSize(width, height));

        Assert.Equal("ArtboardWidth", ex.ParamName);
    }

    [Theory]
    [InlineData(100, 0)]
    [InlineData(100, -1)]
    [InlineData(100, double.NaN)]
    [InlineData(100, double.NegativeInfinity)]
    public void WithArtboardSize_InvalidHeight_Throws(double width, double height)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Document.Create("L").WithArtboardSize(width, height));

        Assert.Equal("ArtboardHeight", ex.ParamName);
    }

    [Fact]
    public void WithArtboardSize_Valid_ReturnsNewDocumentAndKeepsOriginal()
    {
        var original = Document.Create("L");

        var changed = original.WithArtboardSize(500, 300);

        Assert.Equal((1200, 800), (original.ArtboardWidth, original.ArtboardHeight));
        Assert.Equal((500, 300), (changed.ArtboardWidth, changed.ArtboardHeight));
        Assert.Equal(original.Layers, changed.Layers);
    }

    [Fact]
    public void Layers_EmptyOrDefault_Throws()
    {
        var document = Document.Create("L");

        Assert.Throws<ArgumentException>(() => document with { Layers = ImmutableArray<Layer>.Empty });
        Assert.Throws<ArgumentException>(() => document with { Layers = default });
    }

    [Fact]
    public void AddLayer_NewLayer_IsPlacedOnTopAndOriginalStaysUnchanged()
    {
        var original = Document.Create("Bottom");
        var top = new Layer { Name = "Top" };

        var changed = original.AddLayer(top);

        Assert.Equal(new[] { "Bottom", "Top" }, changed.Layers.Select(l => l.Name));
        Assert.Single(original.Layers);
    }

    [Fact]
    public void AddLayer_NewLayer_SharesExistingLayerInstances()
    {
        var original = Document.Create("Bottom");

        var changed = original.AddLayer(new Layer { Name = "Top" });

        Assert.Same(original.Layers[0], changed.Layers[0]);
    }

    [Fact]
    public void AddLayer_DuplicateId_ThrowsAndLeavesDocumentUnchanged()
    {
        var document = TwoLayers();

        Assert.Throws<ArgumentException>(() => document.AddLayer(new Layer { Id = LayerA, Name = "Again" }));

        Assert.Equal(2, document.Layers.Length);
    }

    [Fact]
    public void AddLayer_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Document.Create("L").AddLayer(null!));
    }

    [Fact]
    public void RemoveLayer_ExistingLayer_RemovesItAndKeepsTheOrderOfTheOthers()
    {
        var document = TwoLayers().AddLayer(new Layer { Name = "C" });

        var changed = document.RemoveLayer(LayerA);

        Assert.Equal(new[] { "B", "C" }, changed.Layers.Select(l => l.Name));
        Assert.Equal(3, document.Layers.Length);
    }

    [Fact]
    public void RemoveLayer_LayerWithShapes_RemovesItsShapes()
    {
        var shapeLayer = new Layer
        {
            Id = LayerB,
            Name = "B",
            Shapes = ImmutableArray.Create<Shape>(new RectangleShape { Id = ShapeA }),
        };
        var document = Document.Create("A").AddLayer(shapeLayer);

        var changed = document.RemoveLayer(LayerB);

        Assert.DoesNotContain(changed.Layers.SelectMany(l => l.Shapes), s => s.Id == ShapeA);
        Assert.Contains(document.Layers.SelectMany(l => l.Shapes), s => s.Id == ShapeA);
    }

    [Fact]
    public void RemoveLayer_LastLayer_Throws()
    {
        var document = Document.Create("Only");

        Assert.Throws<InvalidOperationException>(() => document.RemoveLayer(document.Layers[0].Id));
    }

    [Fact]
    public void RemoveLayer_UnknownId_Throws()
    {
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => TwoLayers().RemoveLayer(Guid.NewGuid()));
    }

    [Fact]
    public void UpdateLayer_NameVisibilityAndLock_ChangesOnlyThatLayer()
    {
        var original = TwoLayers();

        var changed = original.UpdateLayer(LayerA, l => l with { Name = "Renamed", IsVisible = false, IsLocked = true });

        Assert.Equal("Renamed", changed.Layers[0].Name);
        Assert.False(changed.Layers[0].IsVisible);
        Assert.True(changed.Layers[0].IsLocked);
        Assert.Same(original.Layers[1], changed.Layers[1]);
        Assert.Equal("A", original.Layers[0].Name);
        Assert.True(original.Layers[0].IsVisible);
        Assert.False(original.Layers[0].IsLocked);
    }

    [Fact]
    public void UpdateLayer_UnknownId_Throws()
    {
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => TwoLayers().UpdateLayer(Guid.NewGuid(), l => l));
    }

    [Fact]
    public void UpdateLayer_ChangeReturnsOtherId_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => TwoLayers().UpdateLayer(LayerA, l => l with { Id = Guid.NewGuid() }));
    }

    [Fact]
    public void UpdateLayer_ChangeReturnsNull_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => TwoLayers().UpdateLayer(LayerA, _ => null!));
    }

    [Fact]
    public void UpdateLayer_NullChange_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => TwoLayers().UpdateLayer(LayerA, null!));
    }
}
