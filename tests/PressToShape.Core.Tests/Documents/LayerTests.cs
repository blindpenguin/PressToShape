using System;
using System.Collections.Immutable;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class LayerTests
{
    [Fact]
    public void Constructor_Defaults_VisibleUnlockedAndEmpty()
    {
        var layer = new Layer { Name = "Layer 1" };

        Assert.Equal("Layer 1", layer.Name);
        Assert.True(layer.IsVisible);
        Assert.False(layer.IsLocked);
        Assert.Empty(layer.Shapes);
    }

    [Fact]
    public void Constructor_NoId_GetsNonEmptyUniqueIds()
    {
        var a = new Layer();
        var b = new Layer();

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void Name_EmptyString_IsAllowed()
    {
        Assert.Equal(string.Empty, new Layer { Name = string.Empty }.Name);
    }

    [Fact]
    public void Name_Null_ThrowsOnCreationAndWith()
    {
        Assert.Throws<ArgumentNullException>(() => new Layer { Name = null! });
        Assert.Throws<ArgumentNullException>(() => new Layer() with { Name = null! });
    }

    [Fact]
    public void Id_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Layer { Id = Guid.Empty });
    }

    [Fact]
    public void Shapes_DefaultArray_BecomesEmpty()
    {
        var layer = new Layer { Shapes = default };

        Assert.False(layer.Shapes.IsDefault);
        Assert.Empty(layer.Shapes);
    }

    [Fact]
    public void With_NameVisibilityAndLock_ChangesCopyAndLeavesOriginalUnchanged()
    {
        var original = new Layer { Name = "A" };

        var changed = original with { Name = "B", IsVisible = false, IsLocked = true };

        Assert.Equal("A", original.Name);
        Assert.True(original.IsVisible);
        Assert.False(original.IsLocked);
        Assert.Equal("B", changed.Name);
        Assert.False(changed.IsVisible);
        Assert.True(changed.IsLocked);
        Assert.Equal(original.Id, changed.Id);
    }

    [Fact]
    public void With_Shapes_KeepsGivenShapesInOrder()
    {
        var first = new RectangleShape();
        var second = new EllipseShape();

        var layer = new Layer { Shapes = ImmutableArray.Create<Shape>(first, second) };

        Assert.Equal(new Shape[] { first, second }, layer.Shapes);
    }

    [Fact]
    public void ShapeLocation_LayerAndShape_AreKept()
    {
        var shape = new RectangleShape();
        var layer = new Layer();

        var location = new ShapeLocation(layer, shape);

        Assert.Same(layer, location.Layer);
        Assert.Same(shape, location.Shape);
    }
}
