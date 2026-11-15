using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionQueriesTests
{
    private static readonly Guid Bottom = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Top = new("aaaaaaaa-0000-0000-0000-000000000002");

    private static readonly RectangleShape A = new() { Id = new("bbbbbbbb-0000-0000-0000-00000000000a"), X = 0, Y = 0, Width = 100, Height = 100 };
    private static readonly EllipseShape B = new() { Id = new("bbbbbbbb-0000-0000-0000-00000000000b"), X = 200, Y = 0, Width = 100, Height = 100 };
    private static readonly RectangleShape C = new() { Id = new("bbbbbbbb-0000-0000-0000-00000000000c"), X = 0, Y = 300, Width = 100, Height = 100 };

    private static Document Doc() =>
        (Document.Create("x") with { Layers = [new Layer { Id = Bottom, Name = "Bottom" }, new Layer { Id = Top, Name = "Top" }] })
            .AddShape(Bottom, A)
            .AddShape(Top, B)
            .AddShape(Bottom, C);

    [Fact]
    public void IsSelectable_VisibleUnlockedLayer_IsTrue()
    {
        Assert.True(SelectionQueries.IsSelectable(Doc(), A.Id));
        Assert.True(SelectionQueries.IsSelectable(Doc(), B.Id));
    }

    [Fact]
    public void IsSelectable_HiddenLockedOrUnknown_IsFalse()
    {
        var hidden = Doc().UpdateLayer(Bottom, l => l with { IsVisible = false });
        var locked = Doc().UpdateLayer(Top, l => l with { IsLocked = true });

        Assert.False(SelectionQueries.IsSelectable(hidden, A.Id));
        Assert.True(SelectionQueries.IsSelectable(hidden, B.Id));
        Assert.False(SelectionQueries.IsSelectable(locked, B.Id));
        Assert.False(SelectionQueries.IsSelectable(Doc(), Guid.NewGuid()));
    }

    [Fact]
    public void Selectable_ListsShapesInDocumentOrder_AndSkipsHiddenAndLockedLayers()
    {
        Assert.Equal(new[] { A.Id, C.Id, B.Id }, SelectionQueries.Selectable(Doc()).Select(s => s.Id));

        var hidden = Doc().UpdateLayer(Bottom, l => l with { IsVisible = false });
        Assert.Equal(new[] { B.Id }, SelectionQueries.Selectable(hidden).Select(s => s.Id));

        var locked = Doc().UpdateLayer(Top, l => l with { IsLocked = true });
        Assert.Equal(new[] { A.Id, C.Id }, SelectionQueries.Selectable(locked).Select(s => s.Id));
    }

    [Fact]
    public void ShapesTouching_RectangleInsideAndOutside()
    {
        Assert.Equal(new[] { A.Id }, SelectionQueries.ShapesTouching(Doc(), 10, 10, 20, 20).Select(s => s.Id));
        Assert.Empty(SelectionQueries.ShapesTouching(Doc(), 120, 120, 50, 50));
    }

    [Fact]
    public void ShapesTouching_PartlyInside_Counts()
    {
        Assert.Equal(new[] { A.Id, B.Id }, SelectionQueries.ShapesTouching(Doc(), 90, 40, 120, 10).Select(s => s.Id));
    }

    [Fact]
    public void ShapesTouching_EdgeContact_Counts()
    {
        Assert.Equal(new[] { A.Id }, SelectionQueries.ShapesTouching(Doc(), 100, 0, 50, 50).Select(s => s.Id));
        Assert.Empty(SelectionQueries.ShapesTouching(Doc(), 100.001, 0, 50, 50));
    }

    [Fact]
    public void ShapesTouching_EverythingInsideAndTheBoxAroundAll()
    {
        Assert.Equal(new[] { A.Id, C.Id, B.Id }, SelectionQueries.ShapesTouching(Doc(), -50, -50, 1000, 1000).Select(s => s.Id));
    }

    [Fact]
    public void ShapesTouching_NegativeWidthAndHeight_AreNormalized()
    {
        Assert.Equal(new[] { A.Id }, SelectionQueries.ShapesTouching(Doc(), 30, 30, -20, -20).Select(s => s.Id));
    }

    [Fact]
    public void ShapesTouching_RotatedShape_UsesTheTurnedBounds()
    {
        var turned = new RectangleShape { Id = new("bbbbbbbb-0000-0000-0000-0000000000d0"), X = 500, Y = 0, Width = 200, Height = 20, Rotation = 90 };
        var doc = Doc().AddShape(Top, turned); // turned: 20 wide, 200 tall around (600, 10): x 590..610, y -90..110

        Assert.Equal(new[] { turned.Id }, SelectionQueries.ShapesTouching(doc, 595, 100, 5, 5).Select(s => s.Id));
        Assert.Empty(SelectionQueries.ShapesTouching(doc, 500, 100, 50, 50)); // inside the unturned box, outside the turned one
    }

    [Fact]
    public void ShapesTouching_ZeroSizeRectangle_StillFindsAShapeUnderThePoint()
    {
        Assert.Equal(new[] { A.Id }, SelectionQueries.ShapesTouching(Doc(), 50, 50, 0, 0).Select(s => s.Id));
    }

    [Fact]
    public void ShapesTouching_SkipsHiddenAndLockedLayers()
    {
        var hidden = Doc().UpdateLayer(Bottom, l => l with { IsVisible = false });
        var locked = Doc().UpdateLayer(Top, l => l with { IsLocked = true });

        Assert.Equal(new[] { B.Id }, SelectionQueries.ShapesTouching(hidden, -50, -50, 1000, 1000).Select(s => s.Id));
        Assert.DoesNotContain(B.Id, SelectionQueries.ShapesTouching(locked, -50, -50, 1000, 1000).Select(s => s.Id));
    }

    [Fact]
    public void SelectedShapes_AreInDocumentOrder_AndUnknownIdsAreIgnored()
    {
        var shapes = SelectionQueries.SelectedShapes(Doc(), [B.Id, Guid.NewGuid(), A.Id, B.Id]);

        Assert.Equal(new[] { A.Id, B.Id }, shapes.Select(s => s.Id));
    }

    [Fact]
    public void EmptyDocumentAndEmptyInputs_GiveEmptyResults()
    {
        var empty = Document.Create("x");

        Assert.Empty(SelectionQueries.Selectable(empty));
        Assert.Empty(SelectionQueries.ShapesTouching(empty, 0, 0, 100, 100));
        Assert.Empty(SelectionQueries.SelectedShapes(empty, []));
    }

    [Fact]
    public void ThousandShapes_AreHandledInOnePass()
    {
        var empty = Document.Create("L");
        var shapes = Enumerable.Range(0, 1000).Select(i => (Shape)new RectangleShape { X = i * 5, Width = 4, Height = 4 }).ToArray();
        var doc = empty with { Layers = [empty.Layers[0] with { Shapes = [.. shapes] }] };

        Assert.Equal(1000, SelectionQueries.Selectable(doc).Count);
        Assert.Equal(11, SelectionQueries.ShapesTouching(doc, 0, 0, 50, 4).Count);
        Assert.Equal(1000, SelectionQueries.SelectedShapes(doc, shapes.Select(s => s.Id)).Count);
    }

    [Fact]
    public void Arguments_Null_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => SelectionQueries.IsSelectable(null!, Guid.NewGuid()));
        Assert.Throws<ArgumentNullException>(() => SelectionQueries.Selectable(null!));
        Assert.Throws<ArgumentNullException>(() => SelectionQueries.ShapesTouching(null!, 0, 0, 1, 1));
        Assert.Throws<ArgumentNullException>(() => SelectionQueries.SelectedShapes(Doc(), null!));
    }
}
