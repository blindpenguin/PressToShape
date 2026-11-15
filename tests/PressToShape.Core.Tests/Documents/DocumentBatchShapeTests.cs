using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class DocumentBatchShapeTests
{
    private static readonly Guid LayerA = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid LayerB = new("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly RectangleShape S1 = new() { Id = new("bbbbbbbb-0000-0000-0000-000000000001"), X = 1 };
    private static readonly RectangleShape S2 = new() { Id = new("bbbbbbbb-0000-0000-0000-000000000002"), X = 2 };
    private static readonly RectangleShape S3 = new() { Id = new("bbbbbbbb-0000-0000-0000-000000000003"), X = 3 };

    private static Document Doc() =>
        (Document.Create("x") with { Layers = [new Layer { Id = LayerA, Name = "A" }, new Layer { Id = LayerB, Name = "B" }] })
            .AddShape(LayerA, S1).AddShape(LayerB, S2).AddShape(LayerA, S3);

    private static IEnumerable<Guid> Order(Document d) => d.Layers.SelectMany(l => l.Shapes).Select(s => s.Id);

    [Fact]
    public void UpdateShapes_ReplacesSeveralShapes_KeepingTheirPlaces()
    {
        var original = Doc();

        var result = original.UpdateShapes(new Dictionary<Guid, Shape> { [S1.Id] = S1 with { X = 10 }, [S2.Id] = S2 with { X = 20 } });

        Assert.Equal(Order(original), Order(result));
        Assert.Equal(new[] { 10.0, 3 }, result.Layers[0].Shapes.Select(s => s.X));
        Assert.Equal(20, result.Layers[1].Shapes[0].X);
        Assert.Equal(1, original.Layers[0].Shapes[0].X);
    }

    [Fact]
    public void UpdateShapes_NothingToReplace_ReturnsTheSameDocument_AndSharesUnchangedLayers()
    {
        var original = Doc();

        Assert.Same(original, original.UpdateShapes(new Dictionary<Guid, Shape>()));
        var result = original.UpdateShapes(new Dictionary<Guid, Shape> { [S1.Id] = S1 with { X = 9 } });
        Assert.Same(original.Layers[1], result.Layers[1]);
    }

    [Fact]
    public void UpdateShapes_UnknownIdOrWrongId_Throws()
    {
        var original = Doc();

        Assert.Throws<KeyNotFoundException>(() => original.UpdateShapes(new Dictionary<Guid, Shape> { [Guid.NewGuid()] = S1 }));
        Assert.Throws<InvalidOperationException>(() => original.UpdateShapes(new Dictionary<Guid, Shape> { [S1.Id] = S2 }));
        Assert.Throws<ArgumentNullException>(() => original.UpdateShapes(null!));
    }

    [Fact]
    public void RemoveShapes_RemovesSeveral_AcrossLayers()
    {
        var result = Doc().RemoveShapes(new HashSet<Guid> { S1.Id, S2.Id });

        Assert.Equal(new[] { S3.Id }, Order(result));
        Assert.Empty(result.Layers[1].Shapes);
    }

    [Fact]
    public void RemoveShapes_EmptyAndUnknown()
    {
        var original = Doc();

        Assert.Same(original, original.RemoveShapes(new HashSet<Guid>()));
        Assert.Throws<KeyNotFoundException>(() => original.RemoveShapes(new HashSet<Guid> { Guid.NewGuid() }));
    }

    [Fact]
    public void AddShapes_AddsOnTopInOrder_AndRejectsDuplicateIdsAndUnknownLayers()
    {
        var extra1 = new RectangleShape();
        var extra2 = new RectangleShape();
        var original = Doc();

        var result = original.AddShapes(LayerB, [extra1, extra2]);

        Assert.Equal(new[] { S2.Id, extra1.Id, extra2.Id }, result.Layers[1].Shapes.Select(s => s.Id));
        Assert.Same(original, original.AddShapes(LayerB, []));
        Assert.Throws<ArgumentException>(() => original.AddShapes(LayerB, [S1]));
        Assert.Throws<KeyNotFoundException>(() => original.AddShapes(Guid.NewGuid(), [extra1]));
    }
}
