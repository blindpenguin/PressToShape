using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasSceneGroupTests
{
    private static RectangleShape Rect(double x) => new() { X = x, Width = 10, Height = 10 };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static double[] Xs(Document document, Guid? entered = null) =>
        CanvasScene.Build(document, 100, 0, 0, entered).Select(s => s.X).ToArray();

    [Fact]
    public void Build_Groups_AreFlattenedInDrawingOrder()
    {
        var document = Build(Rect(0), Group(Rect(10), Group(Rect(20)), Rect(30)), Rect(40));

        Assert.Equal([0.0, 10, 20, 30, 40], Xs(document));
    }

    [Fact]
    public void Build_HiddenShapeHiddenGroupAndItsChildren_AreLeftOut()
    {
        var document = Build(
            Rect(0) with { IsVisible = false },
            Group(Rect(10), Rect(20)) with { IsVisible = false },
            Group(Rect(30) with { IsVisible = false }, Rect(40)),
            Rect(50));

        Assert.Equal([40.0, 50], Xs(document));
    }

    [Fact]
    public void Build_HiddenGroupFlagDoesNotChangeTheChildrensOwnFlag_ButTheyAreStillLeftOut()
    {
        var child = Rect(10);
        var document = Build(Group(child) with { IsVisible = false });

        Assert.True(document.FindShape(child.Id)!.Shape.IsVisible);
        Assert.Empty(CanvasScene.Build(document, 100, 0, 0));
    }

    [Fact]
    public void Build_LockedShapesAndGroups_AreDrawn()
    {
        var document = Build(Rect(0) with { IsLocked = true }, Group(Rect(10)) with { IsLocked = true });

        Assert.Equal([0.0, 10], Xs(document));
    }

    [Fact]
    public void Build_NoEnteredGroup_DimsNothing()
    {
        var document = Build(Rect(0), Group(Rect(10)));

        Assert.All(CanvasScene.Build(document, 100, 0, 0), shape => Assert.False(shape.IsDimmed));
    }

    [Fact]
    public void Build_EnteredGroup_DimsEverythingOutsideIt()
    {
        var group = Group(Rect(10), Rect(20));
        var document = Build(Rect(0), group, Rect(40));

        var scene = CanvasScene.Build(document, 100, 0, 0, group.Id);

        Assert.Equal([true, false, false, true], scene.Select(s => s.IsDimmed).ToArray());
    }

    [Fact]
    public void Build_EnteredNestedGroup_DimsSiblingsOfItsParentsToo()
    {
        var inner = Group(Rect(20));
        var outer = Group(Rect(10), inner, Rect(30));
        var document = Build(outer);

        var scene = CanvasScene.Build(document, 100, 0, 0, inner.Id);

        Assert.Equal([true, false, true], scene.Select(s => s.IsDimmed).ToArray());
    }

    [Fact]
    public void Build_EnteredOuterGroup_DimsNothingInsideIt()
    {
        var outer = Group(Rect(10), Group(Rect(20)));

        var scene = CanvasScene.Build(Build(outer, Rect(50)), 100, 0, 0, outer.Id);

        Assert.Equal([false, false, true], scene.Select(s => s.IsDimmed).ToArray());
    }

    [Fact]
    public void Build_RotatedLeafInGroup_KeepsItsRotationAndPosition()
    {
        var rotated = Rect(10) with { Rotation = 30 };

        var scene = CanvasScene.Build(Build(Group(rotated)), 200, 5, 7);

        var shape = Assert.Single(scene);
        Assert.Equal(30, shape.RotationDegrees);
        Assert.Equal(5 + 10 * 2, shape.X);
    }
}
