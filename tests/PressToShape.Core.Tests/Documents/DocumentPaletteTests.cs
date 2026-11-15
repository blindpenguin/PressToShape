using System;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class DocumentPaletteTests
{
    private static readonly Color Red = Color.FromRgb(255, 0, 0);
    private static readonly Color Green = Color.FromRgb(0, 255, 0);

    [Fact]
    public void Create_NewDocument_HasEmptyPalette() => Assert.Empty(Document.Create("x").Palette);

    [Fact]
    public void AddSwatch_NewColors_AppendsInOrder()
    {
        var result = Document.Create("x").AddSwatch(Red).AddSwatch(Green);

        Assert.Equal([Red, Green], result.Palette.ToArray());
    }

    [Fact]
    public void AddSwatch_ColorAlreadyInPalette_ReturnsSameInstance()
    {
        var document = Document.Create("x").AddSwatch(Red);

        Assert.Same(document, document.AddSwatch(Red));
    }

    [Fact]
    public void AddSwatch_SameRgbWithOtherAlpha_IsAnotherSwatch()
    {
        var result = Document.Create("x").AddSwatch(Red).AddSwatch(Red with { A = 128 });

        Assert.Equal(2, result.Palette.Length);
    }

    [Fact]
    public void RemoveSwatchAt_ValidIndex_RemovesOnlyThatSwatch()
    {
        var document = Document.Create("x").AddSwatch(Red).AddSwatch(Green);

        var result = document.RemoveSwatchAt(0);

        Assert.Equal([Green], result.Palette.ToArray());
        Assert.Equal(2, document.Palette.Length);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void RemoveSwatchAt_BadIndex_Throws(int index)
    {
        var document = Document.Create("x").AddSwatch(Red);

        Assert.Throws<ArgumentOutOfRangeException>(() => document.RemoveSwatchAt(index));
    }

    [Fact]
    public void AddSwatch_Document_KeepsLayersAndShapes()
    {
        var document = Document.Create("x");
        var layerId = document.Layers[0].Id;
        document = document.AddShape(layerId, new RectangleShape());

        var result = document.AddSwatch(Red);

        Assert.Same(document.Layers[0], result.Layers[0]);
    }

    [Fact]
    public void Palette_DefaultArray_BecomesEmpty()
    {
        var document = Document.Create("x") with { Palette = default };

        Assert.True(document.Palette.IsEmpty);
        Assert.False(document.Palette.IsDefault);
    }

    [Fact]
    public void Palette_DuplicateColorsInInit_AreRemoved()
    {
        var document = Document.Create("x") with { Palette = [Red, Green, Red] };

        Assert.Equal([Red, Green], document.Palette.ToArray());
    }
}
