using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class LayerReorderTests
{
    private static Layer[] Layers(params string[] names) => names.Select(n => new Layer { Name = n }).ToArray();

    // A document with the layers A (bottom), B, C (top). The first layer of Create is A.
    private static (Document Document, Layer A, Layer B, Layer C) Build()
    {
        var document = Document.Create("A");
        var b = new Layer { Name = "B", IsLocked = true };
        var c = new Layer { Name = "C", IsVisible = false, Shapes = [new RectangleShape { Width = 5, Height = 5 }] };
        document = document.AddLayer(b).AddLayer(c);
        return (document, document.Layers[0], b, c);
    }

    private static IEnumerable<string> Names(Document document) => document.Layers.Select(l => l.Name);

    [Theory]
    [InlineData(0, new[] { "X", "A", "B", "C" })]
    [InlineData(1, new[] { "A", "X", "B", "C" })]
    [InlineData(3, new[] { "A", "B", "C", "X" })]
    public void InsertLayer_AtIndex_PlacesTheLayer(int index, string[] expected)
    {
        var (document, _, _, _) = Build();

        var result = document.InsertLayer(index, new Layer { Name = "X" });

        Assert.Equal(expected, Names(result));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InsertLayer_BadIndex_Throws(int index)
    {
        var (document, _, _, _) = Build();

        Assert.Throws<ArgumentOutOfRangeException>(() => document.InsertLayer(index, new Layer { Name = "X" }));
    }

    [Fact]
    public void InsertLayer_ExistingId_Throws()
    {
        var (document, a, _, _) = Build();

        Assert.Throws<ArgumentException>(() => document.InsertLayer(1, new Layer { Id = a.Id, Name = "X" }));
    }

    [Fact]
    public void MoveLayerToIndex_Up_SwapsWithNeighbour()
    {
        var (document, _, b, _) = Build();

        var result = document.MoveLayerToIndex(b.Id, 2);

        Assert.Equal(["A", "C", "B"], Names(result));
    }

    [Fact]
    public void MoveLayerToIndex_ToBottom_PlacesAtBottom()
    {
        var (document, _, _, c) = Build();

        var result = document.MoveLayerToIndex(c.Id, 0);

        Assert.Equal(["C", "A", "B"], Names(result));
    }

    [Fact]
    public void MoveLayerToIndex_SameIndex_ReturnsSameDocument()
    {
        var (document, _, b, _) = Build();

        Assert.Same(document, document.MoveLayerToIndex(b.Id, 1));
    }

    [Fact]
    public void MoveLayerToIndex_UnknownId_Throws()
    {
        var (document, _, _, _) = Build();

        Assert.Throws<KeyNotFoundException>(() => document.MoveLayerToIndex(Guid.NewGuid(), 0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void MoveLayerToIndex_BadIndex_Throws(int index)
    {
        var (document, a, _, _) = Build();

        Assert.Throws<ArgumentOutOfRangeException>(() => document.MoveLayerToIndex(a.Id, index));
    }

    [Fact]
    public void MoveLayerAbove_LayerBelowTarget_GoesAboveTarget()
    {
        var (document, a, _, c) = Build();

        var result = document.MoveLayerAbove(a.Id, c.Id);

        Assert.Equal(["B", "C", "A"], Names(result));
    }

    [Fact]
    public void MoveLayerAbove_LayerAboveTarget_GoesDirectlyAboveTarget()
    {
        var (document, a, _, c) = Build();

        var result = document.MoveLayerAbove(c.Id, a.Id);

        Assert.Equal(["A", "C", "B"], Names(result));
    }

    [Fact]
    public void MoveLayerAbove_AlreadyDirectlyAbove_ReturnsSameDocument()
    {
        var (document, a, b, _) = Build();

        Assert.Same(document, document.MoveLayerAbove(b.Id, a.Id));
    }

    [Fact]
    public void MoveLayerAbove_SameLayer_Throws()
    {
        var (document, a, _, _) = Build();

        Assert.Throws<InvalidOperationException>(() => document.MoveLayerAbove(a.Id, a.Id));
    }

    [Fact]
    public void MoveLayerAbove_UnknownId_Throws()
    {
        var (document, a, _, _) = Build();

        Assert.Throws<KeyNotFoundException>(() => document.MoveLayerAbove(a.Id, Guid.NewGuid()));
        Assert.Throws<KeyNotFoundException>(() => document.MoveLayerAbove(Guid.NewGuid(), a.Id));
    }

    [Fact]
    public void MoveLayerToIndex_Moved_KeepsLayersFlagsAndShapesAndOriginal()
    {
        var (document, _, b, c) = Build();

        var result = document.MoveLayerToIndex(c.Id, 0);

        Assert.Same(c, result.Layers[0]);
        Assert.Same(b, result.Layers[2]);
        Assert.Equal(["A", "B", "C"], Names(document));
    }
}
