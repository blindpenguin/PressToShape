using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionOverlayRadiusTests
{
    private static readonly RectangleShape Rect = new() { Id = Guid.NewGuid(), X = 100, Y = 50, Width = 200, Height = 100 };

    private static Document Doc(params Shape[] shapes)
    {
        var document = Document.Create("x");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static SelectionOverlay Build(Document document, Guid[] ids, Tool tool, double zoom = 100) =>
        SelectionOverlay.Build(document, ids, tool, zoom, 0, 0);

    [Fact]
    public void Build_OneRectangle_HasFourRadiusHandlesAtTheirPositions()
    {
        foreach (var tool in new[] { Tool.Select, Tool.DirectSelect })
        {
            var overlay = Build(Doc(Rect), [Rect.Id], tool);

            Assert.Equal(4, overlay.RadiusHandles.Count);
            Assert.Equal(new[] { Corner.TopLeft, Corner.TopRight, Corner.BottomRight, Corner.BottomLeft }, overlay.RadiusHandles.Select(h => h.Corner));
            foreach (var handle in overlay.RadiusHandles)
            {
                var (x, y) = RadiusHandles.Position(Rect, handle.Corner, 100);
                Assert.Equal(x, handle.Center.X, 6);
                Assert.Equal(y, handle.Center.Y, 6);
                Assert.Equal(SelectionOverlay.HandleSize, handle.Size);
            }
        }
    }

    [Fact]
    public void Build_RadiusHandles_FollowTheZoomAndTheOrigin()
    {
        var overlay = SelectionOverlay.Build(Doc(Rect), [Rect.Id], Tool.Select, 200, 50, 70);

        var (x, y) = RadiusHandles.Position(Rect, Corner.TopLeft, 200);
        Assert.Equal(50 + x * 2, overlay.RadiusHandles[0].Center.X, 6);
        Assert.Equal(70 + y * 2, overlay.RadiusHandles[0].Center.Y, 6);
    }

    [Fact]
    public void Build_SelectKeepsItsResizeHandlesAndDirectSelectHasNoneOfThem()
    {
        var select = Build(Doc(Rect), [Rect.Id], Tool.Select);
        var direct = Build(Doc(Rect), [Rect.Id], Tool.DirectSelect);

        Assert.Equal(9, select.Handles.Count);
        Assert.NotNull(select.Box);
        Assert.Empty(direct.Handles);
        Assert.Null(direct.Box);
        Assert.Single(direct.Outlines);
    }

    [Fact]
    public void Build_OtherTools_HaveNoRadiusHandles()
    {
        foreach (var tool in Enum.GetValues<Tool>().Where(tool => tool is not (Tool.Select or Tool.DirectSelect)))
        {
            Assert.Empty(Build(Doc(Rect), [Rect.Id], tool).RadiusHandles);
        }
    }

    [Fact]
    public void Build_TwoShapes_HaveNoRadiusHandles()
    {
        var other = new RectangleShape { Id = Guid.NewGuid(), X = 400, Y = 50, Width = 200, Height = 100 };

        Assert.Empty(Build(Doc(Rect, other), [Rect.Id, other.Id], Tool.Select).RadiusHandles);
    }

    [Fact]
    public void Build_EllipseAndPath_HaveNoRadiusHandles()
    {
        var ellipse = new EllipseShape { Id = Guid.NewGuid(), X = 0, Y = 0, Width = 200, Height = 100 };
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(200, 0), new DocumentNode(100, 100)], isClosed: true);

        Assert.Empty(Build(Doc(ellipse), [ellipse.Id], Tool.Select).RadiusHandles);
        Assert.Empty(Build(Doc(path), [path.Id], Tool.Select).RadiusHandles);
    }

    [Fact]
    public void Build_GroupWithOneRectangle_HasNoRadiusHandles()
    {
        var other = new RectangleShape { Id = Guid.NewGuid(), X = 400, Y = 50, Width = 200, Height = 100 };
        var grouped = Doc(Rect, other).Group([Rect.Id, other.Id], Guid.NewGuid());
        var groupId = grouped.Layers[0].Shapes[0].Id;

        Assert.Empty(Build(grouped, [groupId], Tool.Select).RadiusHandles);
    }

    [Fact]
    public void Build_LockedOrHiddenRectangle_HasNoRadiusHandles()
    {
        var locked = Rect with { IsLocked = true };
        var hidden = Rect with { IsVisible = false };

        Assert.Empty(Build(Doc(locked), [locked.Id], Tool.Select).RadiusHandles);
        Assert.Empty(Build(Doc(hidden), [hidden.Id], Tool.Select).RadiusHandles);
    }

    [Fact]
    public void Build_SmallRectangleOnTheScreen_HasNoRadiusHandlesUntilItIsBigEnough()
    {
        var small = Rect with { Height = 63 };
        var big = Rect with { Height = 64 };

        Assert.Empty(Build(Doc(small), [small.Id], Tool.Select).RadiusHandles);
        Assert.Equal(4, Build(Doc(big), [big.Id], Tool.Select).RadiusHandles.Count);
        Assert.Equal(4, Build(Doc(small), [small.Id], Tool.Select, 200).RadiusHandles.Count);
    }

    [Fact]
    public void Build_RoundedAndRotatedRectangle_PutsTheHandlesOnTheRotatedPositions()
    {
        var rounded = Rect with { Radii = CornerRadii.Uniform(30), Rotation = 90 };

        var overlay = Build(Doc(rounded), [rounded.Id], Tool.Select);

        var (x, y) = RadiusHandles.Position(rounded, Corner.BottomRight, 100);
        Assert.Equal(x, overlay.RadiusHandles[2].Center.X, 6);
        Assert.Equal(y, overlay.RadiusHandles[2].Center.Y, 6);
    }

    [Fact]
    public void Empty_HasNoRadiusHandles()
    {
        Assert.Empty(SelectionOverlay.Empty.RadiusHandles);
    }
}
