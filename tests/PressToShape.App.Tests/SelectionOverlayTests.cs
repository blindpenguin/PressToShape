using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionOverlayTests
{
    private static readonly RectangleShape A = new() { Id = Guid.NewGuid(), X = 10, Y = 20, Width = 100, Height = 40 };
    private static readonly EllipseShape B = new() { Id = Guid.NewGuid(), X = 200, Y = 100, Width = 50, Height = 50, Rotation = 90 };

    private static Document Doc()
    {
        var document = Document.Create("x");
        return document.AddShape(document.Layers[0].Id, A).AddShape(document.Layers[0].Id, B);
    }

    private static void Near(ScreenPoint actual, double x, double y)
    {
        Assert.Equal(x, actual.X, 6);
        Assert.Equal(y, actual.Y, 6);
    }

    [Fact]
    public void OneShape_HasAnOutline_ABoxAndNineHandles()
    {
        var overlay = SelectionOverlay.Build(Doc(), [A.Id], Tool.Select, 100, 0, 0);

        var outline = Assert.Single(overlay.Outlines);
        Near(outline.Points[0], 10, 20);
        Near(outline.Points[2], 110, 60);
        Assert.NotNull(overlay.Box);
        Assert.Equal(9, overlay.Handles.Count);
        Assert.NotNull(overlay.RotateLink);
        Assert.Null(overlay.SelectionBox);
    }

    [Fact]
    public void ZoomAndOrigin_MapTheOutline()
    {
        var overlay = SelectionOverlay.Build(Doc(), [A.Id], Tool.Select, 200, 50, 70);

        var outline = overlay.Outlines[0];
        Near(outline.Points[0], 70, 110);
        Near(outline.Points[2], 270, 190);
    }

    [Fact]
    public void RotatedShape_HasATurnedOutline()
    {
        var overlay = SelectionOverlay.Build(Doc(), [B.Id], Tool.Select, 100, 0, 0);

        var outline = overlay.Outlines[0]; // an ellipse box of 50 x 50 turned 90 degrees around (225, 125)
        Near(outline.Points[0], 250, 100);
        Near(outline.Points[2], 200, 150);
    }

    [Fact]
    public void RotatedNonSquareShape_TurnsItsCorners()
    {
        var turned = new RectangleShape { Id = Guid.NewGuid(), X = 0, Y = 0, Width = 100, Height = 20, Rotation = 90 };
        var document = Document.Create("x");
        document = document.AddShape(document.Layers[0].Id, turned);

        var overlay = SelectionOverlay.Build(document, [turned.Id], Tool.Select, 100, 0, 0);

        // Center (50, 10). The top left corner (-50, -10) turns to (10, -50) relative: (60, -40).
        Near(overlay.Outlines[0].Points[0], 60, -40);
        Near(overlay.Box!.Points[0], 60, -40);
    }

    [Fact]
    public void SeveralShapes_HaveOneOutlineEachAndAnUprightBoxAroundAll()
    {
        var overlay = SelectionOverlay.Build(Doc(), [A.Id, B.Id], Tool.Select, 100, 0, 0);

        Assert.Equal(2, overlay.Outlines.Count);
        Near(overlay.Box!.Points[0], 10, 20);
        Near(overlay.Box.Points[2], 250, 150);
        Assert.Equal(9, overlay.Handles.Count);
    }

    [Fact]
    public void HandleSizeStaysEightScreenPixelsAtEveryZoom()
    {
        foreach (var zoom in new[] { 25.0, 100, 400 })
        {
            var overlay = SelectionOverlay.Build(Doc(), [A.Id], Tool.Select, zoom, 0, 0);

            Assert.All(overlay.Handles, h => Assert.Equal(8, h.Size));
        }
    }

    [Fact]
    public void HandlesSitOnTheBoxAndTheRotateHandleIs24ScreenPixelsAboveIt()
    {
        foreach (var zoom in new[] { 25.0, 100, 400 })
        {
            var overlay = SelectionOverlay.Build(Doc(), [A.Id], Tool.Select, zoom, 0, 0);
            var scale = zoom / 100;
            var top = overlay.Handles.Single(h => h.Type == HandleType.Top).Center;
            var rotate = overlay.Handles.Single(h => h.Type == HandleType.Rotate).Center;
            var topLeft = overlay.Handles.Single(h => h.Type == HandleType.TopLeft).Center;

            Assert.Equal(60 * scale, top.X, 6);
            Assert.Equal(20 * scale, top.Y, 6);
            Assert.Equal(top.X, rotate.X, 6);
            Assert.Equal(top.Y - 24, rotate.Y, 6);
            Assert.Equal(10 * scale, topLeft.X, 6);
            Assert.Equal(overlay.RotateLink!.Value.From, top);
            Assert.Equal(overlay.RotateLink.Value.To, rotate);
        }
    }

    [Fact]
    public void NoSelection_GivesNothing()
    {
        var overlay = SelectionOverlay.Build(Doc(), [], Tool.Select, 100, 0, 0);

        Assert.Empty(overlay.Outlines);
        Assert.Null(overlay.Box);
        Assert.Empty(overlay.Handles);
        Assert.Null(overlay.RotateLink);
        Assert.Null(overlay.SelectionBox);
    }

    [Fact]
    public void OtherTools_GiveOutlinesOnly()
    {
        foreach (var tool in new[] { Tool.Rectangle, Tool.Ellipse, Tool.Pen, Tool.Zoom })
        {
            var overlay = SelectionOverlay.Build(Doc(), [A.Id], tool, 100, 0, 0, new SelectionBox(0, 0, 10, 10));

            Assert.Single(overlay.Outlines);
            Assert.Null(overlay.Box);
            Assert.Empty(overlay.Handles);
            Assert.Null(overlay.RotateLink);
            Assert.Null(overlay.SelectionBox);
        }
    }

    [Fact]
    public void TheDraggedBox_IsMappedToScreenPixels_EvenWithoutASelection()
    {
        var overlay = SelectionOverlay.Build(Doc(), [], Tool.Select, 200, 10, 20, new SelectionBox(5, 6, 30, 40));

        Assert.Equal(new ScreenRect(20, 32, 60, 80), overlay.SelectionBox);
        Assert.Empty(overlay.Outlines);
    }

    [Fact]
    public void HiddenOrLockedShapes_AndUnknownIds_GiveNothing()
    {
        var document = Doc();
        var layer = document.Layers[0].Id;
        var hidden = document.UpdateLayer(layer, l => l with { IsVisible = false });
        var locked = document.UpdateLayer(layer, l => l with { IsLocked = true });

        Assert.Empty(SelectionOverlay.Build(hidden, [A.Id], Tool.Select, 100, 0, 0).Outlines);
        Assert.Empty(SelectionOverlay.Build(locked, [A.Id], Tool.Select, 100, 0, 0).Handles);
        Assert.Empty(SelectionOverlay.Build(Doc(), [Guid.NewGuid()], Tool.Select, 100, 0, 0).Outlines);
    }

    [Fact]
    public void NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => SelectionOverlay.Build(null!, [], Tool.Select, 100, 0, 0));
        Assert.Throws<ArgumentNullException>(() => SelectionOverlay.Build(Doc(), null!, Tool.Select, 100, 0, 0));
    }
}
