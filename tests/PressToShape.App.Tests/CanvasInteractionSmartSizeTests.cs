using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSmartSizeTests
{
    // D is a 50 x 50 square at (200, 0). O is 100 wide and 80 high, far away at (500, 300).
    private sealed record Scene(EditorState State, CanvasInteraction Interaction, RectangleShape D);

    private static Scene Create(Tool tool = Tool.Select, double rotation = 0)
    {
        var d = new RectangleShape { X = 200, Y = 0, Width = 50, Height = 50, Rotation = rotation };
        var other = new RectangleShape { X = 500, Y = 300, Width = 100, Height = 80 };
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [d, other]);
        var state = new EditorState(document) { ActiveTool = tool };
        state.Snapping.SmartGuides = true;
        if (tool == Tool.Select)
        {
            state.Select(d.Id);
        }

        return new Scene(state, new CanvasInteraction(state), d);
    }

    private static Shape Find(Scene scene) => scene.State.Document.FindShape(scene.D.Id)!.Shape;

    private static Shape Drawn(Scene scene) => scene.State.Document.Layers[0].Shapes.Last();

    private static void Resize(Scene scene, double fromX, double fromY, double toX, double toY, bool shift = false, bool alt = false, bool ctrl = false)
    {
        scene.Interaction.PointerPressed(fromX, fromY);
        scene.Interaction.PointerMoved(toX, toY, shift, alt, ctrl);
        scene.Interaction.PointerReleased(toX, toY, shift, alt, ctrl);
    }

    [Fact]
    public void Resize_CornerSnapsWidthAndHeightToAnotherObject()
    {
        var scene = Create();

        Resize(scene, 250, 50, 297, 77);

        Assert.Equal((100.0, 80.0), (Find(scene).Width, Find(scene).Height));
    }

    [Fact]
    public void Resize_ShowsSizeMarkersWhileDragging_AndClearsThemAfter()
    {
        var scene = Create();
        scene.Interaction.PointerPressed(250, 50);

        scene.Interaction.PointerMoved(297, 55);

        var sizes = scene.State.SmartMarkers.Where(m => m.Type == SmartMarkerType.EqualSize).ToList();
        Assert.Equal(2, sizes.Count);
        Assert.All(sizes, m => Assert.Equal(100, m.Value));
        scene.Interaction.PointerReleased(297, 55);
        Assert.Empty(scene.State.SmartMarkers);
    }

    [Fact]
    public void Resize_WithKeepProportions_OnlyTheNearerAxisSnaps_AndTheOtherFollows()
    {
        var scene = Create();

        // The width snaps from 97 to 100. The height is 55 and far from 80; the proportion of the square makes it 100 too.
        Resize(scene, 250, 50, 297, 55, shift: true);

        Assert.Equal((100.0, 100.0), (Find(scene).Width, Find(scene).Height));
    }

    [Fact]
    public void Resize_FromCenter_SnapsWithHalfTheOffset_AndKeepsTheCenter()
    {
        var scene = Create();

        // The center is at x 225. The right handle at 272 makes the width 94, which snaps to 100 (the pointer goes to 275).
        Resize(scene, 250, 25, 272, 25, alt: true);

        Assert.Equal(100, Find(scene).Width, 9);
        Assert.Equal(225, Find(scene).X + Find(scene).Width / 2, 9);
    }

    [Fact]
    public void Resize_OutsideTolerance_DoesNotSnap()
    {
        var scene = Create();

        Resize(scene, 250, 50, 285, 50);

        Assert.Equal(85, Find(scene).Width, 9);
    }

    [Fact]
    public void Resize_OffCtrlOrARotatedFrame_DoesNotSnap()
    {
        var off = Create();
        off.State.Snapping.SmartGuides = false;
        Resize(off, 250, 50, 297, 77);
        Assert.Equal((97.0, 77.0), (Find(off).Width, Find(off).Height));

        var ctrl = Create();
        Resize(ctrl, 250, 50, 297, 77, ctrl: true);
        Assert.Equal((97.0, 77.0), (Find(ctrl).Width, Find(ctrl).Height));

        var turned = Create(rotation: 30);
        var frame = SelectionGeometry.FrameOf([Find(turned)])!;
        var (hx, hy) = SelectionGeometry.HandlePosition(frame, HandleType.Right, 100);
        turned.Interaction.PointerPressed(hx, hy);
        turned.Interaction.PointerMoved(hx + 47, hy);
        Assert.DoesNotContain(turned.State.SmartMarkers, m => m.Type == SmartMarkerType.EqualSize);
    }

    [Fact]
    public void Resize_CancelRestoresTheShape_AndOneUndoStepOtherwise()
    {
        var cancelled = Create();
        cancelled.Interaction.PointerPressed(250, 50);
        cancelled.Interaction.PointerMoved(297, 77);
        cancelled.Interaction.Abort();
        Assert.Equal(50, Find(cancelled).Width, 9);

        var done = Create();
        var steps = done.State.History.Steps.Count;
        Resize(done, 250, 50, 297, 77);
        Assert.Equal(steps + 1, done.State.History.Steps.Count);
    }

    [Fact]
    public void Draw_WidthAndHeightSnapToAnotherObject()
    {
        var scene = Create(Tool.Rectangle);

        Resize(scene, 700, 200, 797, 277);

        Assert.Equal((100.0, 80.0), (Drawn(scene).Width, Drawn(scene).Height));
        Assert.Equal((700.0, 200.0), (Drawn(scene).X, Drawn(scene).Y));
    }

    [Fact]
    public void Draw_WithShift_TheSquareSnapsItsLargerSide()
    {
        var scene = Create(Tool.Rectangle);

        Resize(scene, 700, 200, 797, 260, shift: true);

        Assert.Equal((100.0, 100.0), (Drawn(scene).Width, Drawn(scene).Height));
    }

    [Fact]
    public void Draw_WithAlt_GrowsFromThePressPointAndSnaps()
    {
        var scene = Create(Tool.Rectangle);

        // From the press point (800, 600) 47 to the right means a width of 94, which snaps to 100 (the pointer goes to 850).
        Resize(scene, 800, 600, 847, 640, alt: true);

        Assert.Equal(100, Drawn(scene).Width, 9);
        Assert.Equal(800, Drawn(scene).X + Drawn(scene).Width / 2, 9);
    }

    [Fact]
    public void Draw_ShowsMarkersAndLabelsWhileDragging_AndClearsThemOnCancel()
    {
        var scene = Create(Tool.Rectangle);
        scene.Interaction.PointerPressed(700, 200);

        scene.Interaction.PointerMoved(797, 277);

        Assert.Contains(scene.State.SmartMarkers, m => m.Type == SmartMarkerType.EqualSize);
        scene.Interaction.Cancel();
        Assert.Empty(scene.State.SmartMarkers);
    }

    [Fact]
    public void Draw_Labels_ShowTheDistanceToNeighbours()
    {
        var scene = Create(Tool.Rectangle);
        scene.Interaction.PointerPressed(700, 320);

        scene.Interaction.PointerMoved(750, 350);

        // The new shape is right of O (it ends at x 600) and overlaps it vertically: the gap on the left is 100.
        Assert.Contains(scene.State.SmartMarkers, m => m.Type == SmartMarkerType.Distance && m.Axis == SnapAxis.X && Math.Abs(m.Value - 100) < 1e-9);
    }

    [Fact]
    public void Draw_WithSmartGuidesOffOrCtrl_DoesNotSnap()
    {
        var off = Create(Tool.Rectangle);
        off.State.Snapping.SmartGuides = false;
        Resize(off, 700, 200, 797, 277);
        Assert.Equal((97.0, 77.0), (Drawn(off).Width, Drawn(off).Height));

        var ctrl = Create(Tool.Rectangle);
        Resize(ctrl, 700, 200, 797, 277, ctrl: true);
        Assert.Equal((97.0, 77.0), (Drawn(ctrl).Width, Drawn(ctrl).Height));
    }

    [Fact]
    public void Draw_Ellipse_Works()
    {
        var scene = Create(Tool.Ellipse);

        Resize(scene, 700, 200, 797, 277);

        Assert.IsType<EllipseShape>(Drawn(scene));
        Assert.Equal((100.0, 80.0), (Drawn(scene).Width, Drawn(scene).Height));
    }
}
