using System;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class PathShapeOperationsReverseTests
{
    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static PathShape Open(double x = 0) =>
        PathShape.Create([new DocumentNode(x, 0), new DocumentNode(x + 50, 0), new DocumentNode(x + 100, 30)]);

    private static (double X, double Y) FirstNode(Shape shape)
    {
        var node = ((PathShape)shape).ToDocumentSubPaths()[0].Nodes[0];
        return (Math.Round(node.X, 6), Math.Round(node.Y, 6));
    }

    [Fact]
    public void ReverseDirections_Path_IsReversedWithTheSameIdAndPlace()
    {
        var before = new RectangleShape { Width = 5, Height = 5 };
        var path = Open();
        var after = new RectangleShape { X = 500, Width = 5, Height = 5 };
        var document = Build(before, path, after);

        var result = PathShapeOperations.ReverseDirections(document, [path.Id]);

        Assert.Equal([before.Id, path.Id, after.Id], result.Layers[0].Shapes.Select(s => s.Id));
        Assert.Equal((100.0, 30.0), FirstNode(result.Layers[0].Shapes[1]));
    }

    [Fact]
    public void ReverseDirections_PathInASelectedGroup_IsReversed()
    {
        var path = Open();
        var group = new GroupShape { Children = [path, new RectangleShape { X = 300, Width = 5, Height = 5 }] };
        var document = Build(group);

        var result = PathShapeOperations.ReverseDirections(document, [group.Id]);

        var kept = Assert.IsType<GroupShape>(Assert.Single(result.Layers[0].Shapes));
        Assert.Equal((100.0, 30.0), FirstNode(kept.Children[0]));
        Assert.IsType<RectangleShape>(kept.Children[1]);
    }

    [Fact]
    public void ReverseDirections_RectanglesEllipsesLockedAndHidden_AreIgnored_AndTheSameDocumentComesBack()
    {
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var ellipse = new EllipseShape { X = 50, Width = 10, Height = 10 };
        var locked = Open() with { IsLocked = true };
        var hidden = Open(200) with { IsVisible = false };
        var document = Build(rectangle, ellipse, locked, hidden);

        Assert.Same(document, PathShapeOperations.ReverseDirections(document, [rectangle.Id, ellipse.Id, locked.Id, hidden.Id, Guid.NewGuid()]));
        Assert.Empty(PathShapeOperations.ReversibleShapeIds(document, [rectangle.Id, ellipse.Id, locked.Id, hidden.Id]));
    }

    [Fact]
    public void ReversibleShapeIds_ListsEachPathOnce_AndSelectedGroupsGiveTheirPaths()
    {
        var a = Open();
        var b = Open(200);
        var group = new GroupShape { Children = [b] };
        var document = Build(a, group);

        var ids = PathShapeOperations.ReversibleShapeIds(document, [a.Id, group.Id, b.Id]);

        Assert.Equal([a.Id, b.Id], ids);
    }

    [Fact]
    public void ReverseDirections_Twice_GivesTheOriginalFirstNode()
    {
        var path = Open();
        var document = Build(path);

        var twice = PathShapeOperations.ReverseDirections(PathShapeOperations.ReverseDirections(document, [path.Id]), [path.Id]);

        Assert.Equal((0.0, 0.0), FirstNode(twice.Layers[0].Shapes[0]));
    }

    [Fact]
    public void ReverseDirections_DoesNotChangeTheInput_AndNullsThrow()
    {
        var path = Open();
        var document = Build(path);

        PathShapeOperations.ReverseDirections(document, [path.Id]);

        Assert.Equal((0.0, 0.0), FirstNode(document.Layers[0].Shapes[0]));
        Assert.Throws<ArgumentNullException>(() => PathShapeOperations.ReverseDirections(null!, []));
        Assert.Throws<ArgumentNullException>(() => PathShapeOperations.ReverseDirections(document, null!));
    }
}
