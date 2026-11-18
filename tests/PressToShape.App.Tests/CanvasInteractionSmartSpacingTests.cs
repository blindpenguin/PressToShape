using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSmartSpacingTests
{
    private sealed record Scene(EditorState State, CanvasInteraction Interaction, RectangleShape D);

    private static RectangleShape Box(double x, double y = 0, double rotation = 0) => new() { X = x, Y = y, Width = 50, Height = 50, Rotation = rotation };

    // The dragged box D starts at x 250. R1 and R2 are 50 apart. N is a neighbour on the right at x 400.
    private static Scene Create(double dx = 250, bool withFirst = true, double rotation = 0, params Shape[] extra)
    {
        var d = Box(dx, rotation: rotation);
        var others = new List<Shape> { Box(100), Box(400) };
        if (withFirst)
        {
            others.Insert(0, Box(0));
        }

        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [d, .. others, .. extra]);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        state.Snapping.SmartGuides = true;
        return new Scene(state, new CanvasInteraction(state), d);
    }

    private static Shape Find(Scene scene) => scene.State.Document.FindShape(scene.D.Id)!.Shape;

    private static void MoveBy(Scene scene, double dx, bool ctrl = false)
    {
        var (px, py) = (scene.D.X + 25, 25);
        scene.Interaction.PointerPressed(px, py);
        scene.Interaction.PointerMoved(px + dx, py, suspendSnap: ctrl);
        scene.Interaction.PointerReleased(px + dx, py, suspendSnap: ctrl);
    }

    [Fact]
    public void Move_SnapsToAnExistingGap_AndShowsTheEqualGaps()
    {
        var scene = Create();
        scene.Interaction.PointerPressed(275, 25);

        // D at x 203: the gap to R2 (ends at 150) is 53, the gap between R1 and R2 is 50.
        scene.Interaction.PointerMoved(228, 25);

        var markers = scene.State.SmartMarkers.Where(m => m.Type == SmartMarkerType.EqualGap).ToList();
        Assert.NotEmpty(markers);
        Assert.Contains(markers, m => m.From == 50 && m.To == 100 && m.Value == 50);
        Assert.Contains(markers, m => m.From == 150 && Math.Abs(m.To - 200) < 1e-9);
        scene.Interaction.PointerReleased(228, 25);
        Assert.Equal(200, Find(scene).X, 9);
    }

    [Fact]
    public void Move_SnapsToTheMiddleBetweenTwoNeighbours()
    {
        // R2 ends at 150 and N starts at 400: a 50 wide box is centered at x 250 (gaps of 100). At x 256 the gaps are 106 and 94.
        var scene = Create(withFirst: false);

        MoveBy(scene, 6);

        Assert.Equal(250, Find(scene).X, 9);
    }

    [Fact]
    public void Move_ShowsTheMarkersWhileDraggingAndClearsThemAfter()
    {
        var scene = Create(withFirst: false);
        scene.Interaction.PointerPressed(275, 25);

        scene.Interaction.PointerMoved(281, 25);
        Assert.Equal(2, scene.State.SmartMarkers.Count(m => m.Type == SmartMarkerType.EqualGap));

        scene.Interaction.PointerReleased(281, 25);
        Assert.Empty(scene.State.SmartMarkers);
    }

    [Fact]
    public void Move_OutsideTheTolerance_DoesNotSnap()
    {
        var scene = Create();

        MoveBy(scene, -40);

        Assert.Equal(210, Find(scene).X, 9);
    }

    [Fact]
    public void Move_WithSmartGuidesOffOrCtrl_DoesNotSnap()
    {
        var off = Create();
        off.State.Snapping.SmartGuides = false;
        MoveBy(off, -47);
        Assert.Equal(203, Find(off).X, 9);

        var ctrl = Create();
        MoveBy(ctrl, -47, ctrl: true);
        Assert.Equal(203, Find(ctrl).X, 9);
    }

    [Fact]
    public void Move_AnEdgeSnapThatIsNearer_WinsOverEqualSpacing()
    {
        // A box far below whose left edge is at 204: the edge snap is 1 away, the equal spacing 3 away.
        var scene = Create(extra: Box(204, 500));
        scene.State.Snapping.SnapToObjects = true;

        MoveBy(scene, -47);

        Assert.Equal(204, Find(scene).X, 9);
    }

    [Fact]
    public void Move_EqualSpacingThatIsNearer_WinsOverAnEdge()
    {
        var scene = Create(extra: Box(207, 500));
        scene.State.Snapping.SnapToObjects = true;

        MoveBy(scene, -47);

        Assert.Equal(200, Find(scene).X, 9);
    }

    [Fact]
    public void Move_AbortRestoresTheShape_AndOneUndoStepOtherwise()
    {
        var aborted = Create();
        aborted.Interaction.PointerPressed(275, 25);
        aborted.Interaction.PointerMoved(228, 25);
        aborted.Interaction.Abort();
        Assert.Equal(250, Find(aborted).X, 9);
        Assert.Empty(aborted.State.SmartMarkers);

        var done = Create();
        var steps = done.State.History.Steps.Count;
        MoveBy(done, -47);
        Assert.Equal(steps + 1, done.State.History.Steps.Count);
    }

    [Fact]
    public void ResizeRightEdge_SnapsTheGapAfterTheBox()
    {
        // D is at x 250 with width 50. N starts at 400. The right edge dragged to 353 makes the gap 47... use the left side instead.
        var scene = Create(dx: 200);
        scene.State.Select(scene.D.Id);

        // D spans 200..250 and N starts at 400: dragging the right edge to 353 leaves a gap of 47 to N, near the existing gap of 50.
        scene.Interaction.PointerPressed(250, 25);
        scene.Interaction.PointerMoved(353, 25);
        scene.Interaction.PointerReleased(353, 25);

        Assert.Equal(350, Find(scene).X + Find(scene).Width, 9);
        Assert.Equal(200, Find(scene).X, 9);
    }

    [Fact]
    public void ResizeLeftEdge_SnapsTheGapBeforeTheBox()
    {
        var scene = Create(dx: 250);
        scene.State.Select(scene.D.Id);

        // The left edge dragged to 203 leaves a gap of 53 to R2 (it ends at 150): it snaps to the gap of 50, the left edge goes to 200.
        scene.Interaction.PointerPressed(250, 25);
        scene.Interaction.PointerMoved(203, 25);
        scene.Interaction.PointerReleased(203, 25);

        Assert.Equal(200, Find(scene).X, 9);
        Assert.Equal(300, Find(scene).X + Find(scene).Width, 9);
    }

    [Fact]
    public void ResizeOfARotatedFrame_DoesNotSnapOrShowMarkers()
    {
        var scene = Create(dx: 200, rotation: 30);
        scene.State.Select(scene.D.Id);
        var frame = SelectionGeometry.FrameOf([Find(scene)])!;
        var (hx, hy) = SelectionGeometry.HandlePosition(frame, HandleType.Right, 100);

        scene.Interaction.PointerPressed(hx, hy);
        scene.Interaction.PointerMoved(hx + 103, hy);

        Assert.Empty(scene.State.SmartMarkers);
        scene.Interaction.PointerReleased(hx + 103, hy);
    }

    [Fact]
    public void Resize_WithCtrl_DoesNotSnap()
    {
        var scene = Create(dx: 200);
        scene.State.Select(scene.D.Id);

        scene.Interaction.PointerPressed(250, 25);
        scene.Interaction.PointerMoved(353, 25, suspendSnap: true);
        scene.Interaction.PointerReleased(353, 25, suspendSnap: true);

        Assert.Equal(353, Find(scene).X + Find(scene).Width, 9);
    }
}
