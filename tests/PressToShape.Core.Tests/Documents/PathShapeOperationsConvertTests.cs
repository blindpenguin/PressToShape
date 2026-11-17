using System;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class PathShapeOperationsConvertTests
{
    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static RectangleShape Rect(double x = 0) => new() { X = x, Y = 0, Width = 100, Height = 50 };

    private static Guid[] Ids(params Shape[] shapes) => [.. shapes.Select(shape => shape.Id)];

    [Fact]
    public void ConvertToPaths_RectangleAndEllipse_BecomePathsWithTheSameIdsAndPlaces()
    {
        var a = Rect(0) with { Name = "A", Appearance = new Appearance { Fill = Color.FromRgb(1, 2, 3) } };
        var between = new RectangleShape { X = 300, Width = 10, Height = 10, IsLocked = false, Name = "Keep" };
        var b = new EllipseShape { X = 100, Y = 100, Width = 80, Height = 40, Rotation = 20 };
        var document = Build(a, between, b);

        var result = PathShapeOperations.ConvertToPaths(document, Ids(a, b));

        var shapes = result.Layers[0].Shapes;
        Assert.Equal([a.Id, between.Id, b.Id], shapes.Select(s => s.Id));
        var first = Assert.IsType<PathShape>(shapes[0]);
        Assert.Equal("A", first.Name);
        Assert.Equal(a.Appearance, first.Appearance);
        Assert.IsType<PathShape>(shapes[2]);
        Assert.Equal(20, shapes[2].Rotation);
        Assert.IsType<RectangleShape>(shapes[1]);
    }

    [Fact]
    public void ConvertToPaths_RoundedRectangle_LooksTheSame()
    {
        var rounded = new RectangleShape { X = 0, Y = 0, Width = 100, Height = 60, Radii = CornerRadii.Uniform(20), Appearance = new Appearance { Fill = Color.FromRgb(1, 2, 3) } };
        var document = Build(rounded);

        var path = Assert.IsType<PathShape>(PathShapeOperations.ConvertToPaths(document, Ids(rounded)).Layers[0].Shapes[0]);

        foreach (var (x, y) in new[] { (50.0, 30.0), (2.0, 2.0), (98.0, 58.0), (50.0, 1.0), (30.0, 30.0) })
        {
            Assert.Equal(rounded.Contains(x, y), path.Contains(x, y));
        }
    }

    [Fact]
    public void ConvertToPaths_SelectedGroup_ConvertsItsShapesAndKeepsTheGroup()
    {
        var a = Rect(0);
        var b = new EllipseShape { X = 200, Width = 50, Height = 50 };
        var group = new GroupShape { Children = [a, b], Name = "Group" };
        var document = Build(group);

        var result = PathShapeOperations.ConvertToPaths(document, Ids(group));

        var kept = Assert.IsType<GroupShape>(Assert.Single(result.Layers[0].Shapes));
        Assert.Equal(group.Id, kept.Id);
        Assert.Equal("Group", kept.Name);
        Assert.Equal([a.Id, b.Id], kept.Children.Select(c => c.Id));
        Assert.All(kept.Children, child => Assert.IsType<PathShape>(child));
    }

    [Fact]
    public void ConvertToPaths_NestedGroups_AreReached()
    {
        var inner = Rect(0);
        var outer = new GroupShape { Children = [new GroupShape { Children = [inner] }] };

        var result = PathShapeOperations.ConvertToPaths(Build(outer), Ids(outer));

        var path = LeavesOf(result).Single();
        Assert.IsType<PathShape>(path);
        Assert.Equal(inner.Id, path.Id);
    }

    private static Shape[] LeavesOf(Document document) =>
        [.. document.Layers.SelectMany(layer => layer.Shapes).SelectMany(shape => shape.Leaves())];

    [Fact]
    public void ConvertToPaths_LockedAndHiddenShapes_AreSkipped()
    {
        var locked = Rect(0) with { IsLocked = true };
        var hidden = Rect(200) with { IsVisible = false };
        var plain = Rect(400);
        var document = Build(locked, hidden, plain);

        var result = PathShapeOperations.ConvertToPaths(document, Ids(locked, hidden, plain));

        var shapes = result.Layers[0].Shapes;
        Assert.IsType<RectangleShape>(shapes[0]);
        Assert.IsType<RectangleShape>(shapes[1]);
        Assert.IsType<PathShape>(shapes[2]);
    }

    [Fact]
    public void ConvertToPaths_ShapesInALockedGroupOrLayer_AreSkipped()
    {
        var inLockedGroup = Rect(0);
        var lockedGroup = new GroupShape { Children = [inLockedGroup], IsLocked = true };
        var document = Build(lockedGroup);

        Assert.Same(document, PathShapeOperations.ConvertToPaths(document, Ids(lockedGroup)));

        var inLockedLayer = Rect(0);
        var layerDocument = Build(inLockedLayer);
        layerDocument = layerDocument.UpdateLayer(layerDocument.Layers[0].Id, layer => layer with { IsLocked = true });
        Assert.Same(layerDocument, PathShapeOperations.ConvertToPaths(layerDocument, Ids(inLockedLayer)));
    }

    [Fact]
    public void ConvertToPaths_PathsGroupsWithoutShapesAndUnknownIds_AreIgnored_AndTheSameDocumentComesBack()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var group = new GroupShape { Children = [path] };
        var document = Build(group);

        Assert.Same(document, PathShapeOperations.ConvertToPaths(document, [group.Id, path.Id, Guid.NewGuid()]));
        Assert.Same(document, PathShapeOperations.ConvertToPaths(document, []));
    }

    [Fact]
    public void ConvertibleShapeIds_ListsEachConvertibleShapeOnce()
    {
        var a = Rect(0);
        var b = new EllipseShape { X = 200, Width = 50, Height = 50 };
        var group = new GroupShape { Children = [b] };
        var document = Build(a, group);

        var ids = PathShapeOperations.ConvertibleShapeIds(document, [a.Id, group.Id, b.Id, a.Id]);

        Assert.Equal([a.Id, b.Id], ids);
    }

    [Fact]
    public void ConvertToPaths_DoesNotChangeTheInput_AndNullsThrow()
    {
        var a = Rect(0);
        var document = Build(a);

        PathShapeOperations.ConvertToPaths(document, Ids(a));

        Assert.IsType<RectangleShape>(document.Layers[0].Shapes[0]);
        Assert.Throws<ArgumentNullException>(() => PathShapeOperations.ConvertToPaths(null!, []));
        Assert.Throws<ArgumentNullException>(() => PathShapeOperations.ConvertToPaths(document, null!));
    }
}
