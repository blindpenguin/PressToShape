using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Documents;

public class ArrangeOperationsTests
{
    private static RectangleShape Rect(string name) => new() { Name = name, Width = 10, Height = 10 };

    private static GroupShape Group(string name, params Shape[] children) => new() { Name = name, Children = [.. children] };

    private static Document Build(params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static string Order(Document document) => string.Join(string.Empty, document.Layers[0].Shapes.Select(s => s.Name));

    private static string Order(GroupShape group) => string.Join(string.Empty, group.Children.Select(s => s.Name));

    private static IReadOnlySet<Guid> Ids(params Shape[] shapes) => shapes.Select(s => s.Id).ToHashSet();

    private static (Document Document, Shape A, Shape B, Shape C, Shape D) Four()
    {
        Shape a = Rect("a");
        Shape b = Rect("b");
        Shape c = Rect("c");
        Shape d = Rect("d");
        return (Build(a, b, c, d), a, b, c, d);
    }

    [Theory]
    [InlineData(ArrangeType.ToFront, "bcda")]
    [InlineData(ArrangeType.Forward, "bacd")]
    public void Arrange_BottomShape_MovesUp(ArrangeType type, string expected)
    {
        var (document, a, _, _, _) = Four();

        Assert.Equal(expected, Order(document.Arrange(Ids(a), type)));
    }

    [Theory]
    [InlineData(ArrangeType.ToBack, "dabc")]
    [InlineData(ArrangeType.Backward, "abdc")]
    public void Arrange_TopShape_MovesDown(ArrangeType type, string expected)
    {
        var (document, _, _, _, d) = Four();

        Assert.Equal(expected, Order(document.Arrange(Ids(d), type)));
    }

    [Theory]
    [InlineData(ArrangeType.ToFront, "bdac")]
    [InlineData(ArrangeType.ToBack, "acbd")]
    public void Arrange_SeveralSelected_KeepRelativeOrder(ArrangeType type, string expected)
    {
        var (document, a, _, c, _) = Four();

        Assert.Equal(expected, Order(document.Arrange(Ids(c, a), type)));
    }

    [Fact]
    public void Arrange_Forward_EachSelectedMovesOnePlace()
    {
        var (document, a, _, c, _) = Four();

        Assert.Equal("badc", Order(document.Arrange(Ids(a, c), ArrangeType.Forward)));
    }

    [Fact]
    public void Arrange_Backward_EachSelectedMovesOnePlace()
    {
        var (document, _, b, _, d) = Four();

        Assert.Equal("badc", Order(document.Arrange(Ids(b, d), ArrangeType.Backward)));
    }

    [Fact]
    public void Arrange_AdjacentSelected_MoveTogetherPastOneSibling()
    {
        var (document, a, b, _, _) = Four();

        Assert.Equal("cabd", Order(document.Arrange(Ids(a, b), ArrangeType.Forward)));
    }

    [Fact]
    public void Arrange_SelectedAlreadyAtTheEnd_Stays()
    {
        var (document, a, b, c, d) = Four();

        Assert.Same(document, document.Arrange(Ids(d), ArrangeType.ToFront));
        Assert.Same(document, document.Arrange(Ids(d), ArrangeType.Forward));
        Assert.Same(document, document.Arrange(Ids(a), ArrangeType.ToBack));
        Assert.Same(document, document.Arrange(Ids(a), ArrangeType.Backward));
        Assert.Same(document, document.Arrange(Ids(c, d), ArrangeType.Forward));
        Assert.Same(document, document.Arrange(Ids(a, b), ArrangeType.Backward));
    }

    [Fact]
    public void Arrange_EverythingSelected_ReturnsSameDocument()
    {
        var (document, a, b, c, d) = Four();

        Assert.Same(document, document.Arrange(Ids(a, b, c, d), ArrangeType.ToFront));
    }

    [Fact]
    public void Arrange_NoIds_ReturnsSameDocument()
    {
        var (document, _, _, _, _) = Four();

        Assert.Same(document, document.Arrange(new HashSet<Guid>(), ArrangeType.ToFront));
    }

    [Fact]
    public void Arrange_UnknownId_Throws()
    {
        var (document, _, _, _, _) = Four();

        Assert.Throws<KeyNotFoundException>(() => document.Arrange(new HashSet<Guid> { Guid.NewGuid() }, ArrangeType.ToFront));
    }

    [Fact]
    public void Arrange_InsideGroup_MovesOnlyAmongTheGroupsChildren()
    {
        Shape x = Rect("x");
        Shape y = Rect("y");
        Shape z = Rect("z");
        var group = Group("G", x, y, z);
        var document = Build(Rect("a"), group, Rect("b"));

        var result = document.Arrange(Ids(x), ArrangeType.ToFront);

        Assert.Equal("aGb", Order(result));
        var changed = (GroupShape)result.Layers[0].Shapes[1];
        Assert.Equal("yzx", Order(changed));
        Assert.Equal(group.Id, changed.Id);
        Assert.Equal(group.Bounds, changed.Bounds);
    }

    [Fact]
    public void Arrange_ChildAndTopLevelShape_AreArrangedAmongTheirOwnSiblings()
    {
        Shape x = Rect("x");
        Shape y = Rect("y");
        Shape a = Rect("a");
        var document = Build(a, Group("G", x, y), Rect("b"));

        var result = document.Arrange(Ids(x, a), ArrangeType.ToFront);

        Assert.Equal("Gba", Order(result));
        Assert.Equal("yx", Order((GroupShape)result.Layers[0].Shapes[0]));
    }

    [Fact]
    public void Arrange_TwoNestedLevels_BothChange()
    {
        Shape x = Rect("x");
        Shape y = Rect("y");
        Shape p = Rect("p");
        Shape q = Rect("q");
        var inner = Group("I", x, y);
        var outer = Group("O", inner, p, q);
        var document = Build(outer);

        var result = document.Arrange(Ids(x, inner, p), ArrangeType.ToFront);

        var changedOuter = (GroupShape)result.Layers[0].Shapes[0];
        Assert.Equal("qIp", Order(changedOuter));
        Assert.Equal("yx", Order((GroupShape)changedOuter.Children[1]));
    }

    [Fact]
    public void Arrange_KeepsShapesInTheirLayer()
    {
        var a = Rect("a");
        var b = Rect("b");
        var document = Build(a);
        document = document.AddLayer(new Layer { Name = "Two", Shapes = [b] });

        var result = document.Arrange(Ids(a), ArrangeType.ToFront);

        Assert.Same(a, Assert.Single(result.Layers[0].Shapes));
        Assert.Same(b, Assert.Single(result.Layers[1].Shapes));
    }

    [Fact]
    public void Arrange_Result_LeavesOriginalUnchanged()
    {
        var (document, a, _, _, _) = Four();

        _ = document.Arrange(Ids(a), ArrangeType.ToFront);

        Assert.Equal("abcd", Order(document));
    }

    [Fact]
    public void MoveAbove_ShapeBelowTarget_LandsDirectlyAboveIt()
    {
        var (document, a, _, c, _) = Four();

        Assert.Equal("bcad", Order(document.MoveAbove(a.Id, c.Id)));
    }

    [Fact]
    public void MoveAbove_ShapeAboveTarget_LandsDirectlyAboveIt()
    {
        var (document, a, _, _, d) = Four();

        Assert.Equal("adbc", Order(document.MoveAbove(d.Id, a.Id)));
    }

    [Fact]
    public void MoveAbove_AlreadyThere_ReturnsSameDocument()
    {
        var (document, a, b, _, _) = Four();

        Assert.Same(document, document.MoveAbove(b.Id, a.Id));
    }

    [Fact]
    public void MoveAbove_InsideGroup_Works()
    {
        Shape x = Rect("x");
        Shape y = Rect("y");
        Shape z = Rect("z");
        var document = Build(Group("G", x, y, z));

        var result = document.MoveAbove(x.Id, y.Id);

        Assert.Equal("yxz", Order((GroupShape)result.Layers[0].Shapes[0]));
    }

    [Fact]
    public void MoveAbove_DifferentParentsOrSameShape_Throws()
    {
        Shape x = Rect("x");
        Shape a = Rect("a");
        var document = Build(a, Group("G", x, Rect("y")));

        Assert.Throws<InvalidOperationException>(() => document.MoveAbove(x.Id, a.Id));
        Assert.Throws<InvalidOperationException>(() => document.MoveAbove(a.Id, a.Id));
        Assert.Throws<KeyNotFoundException>(() => document.MoveAbove(a.Id, Guid.NewGuid()));
    }
}
