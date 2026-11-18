using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSmartLabelTests
{
    // A 50 x 50 square A at (100, 100) is dragged by its center (125, 125). B is at (300, 100) and C at (100, 300), both 50 x 50.
    private sealed record Scene(EditorState State, CanvasInteraction Interaction, RectangleShape A);

    private static Scene Create(bool smart = true, params Shape[] extra)
    {
        var a = new RectangleShape { X = 100, Y = 100, Width = 50, Height = 50 };
        var b = new RectangleShape { X = 300, Y = 100, Width = 50, Height = 50 };
        var c = new RectangleShape { X = 100, Y = 300, Width = 50, Height = 50 };
        var document = Document.Create("Layer");
        document = document.AddShapes(document.Layers[0].Id, [a, b, c, .. extra]);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        state.Snapping.SmartGuides = smart;
        return new Scene(state, new CanvasInteraction(state), a);
    }

    private static void Press(Scene scene) => scene.Interaction.PointerPressed(125, 125);

    private static Shape Find(Scene scene) => scene.State.Document.FindShape(scene.A.Id)!.Shape;

    [Fact]
    public void Move_WithSmartGuidesOn_ShowsTheDistanceToTheNeighbours()
    {
        var scene = Create();
        Press(scene);

        scene.Interaction.PointerMoved(145, 125);

        var labels = scene.State.SmartMarkers;
        Assert.All(labels, l => Assert.Equal(SmartMarkerType.Distance, l.Type));
        Assert.Equal(2, labels.Length);
        Assert.Contains(labels, l => l.Axis == SnapAxis.X && Math.Abs(l.Value - 130) < 1e-9);
        Assert.Contains(labels, l => l.Axis == SnapAxis.Y && Math.Abs(l.Value - 150) < 1e-9);
    }

    [Fact]
    public void Move_WithSmartGuidesOff_ShowsNoLabels()
    {
        var scene = Create(smart: false);
        Press(scene);

        scene.Interaction.PointerMoved(145, 125);

        Assert.Empty(scene.State.SmartMarkers);
    }

    [Fact]
    public void Move_WithCtrl_ShowsNoLabels_AndCtrlReleasedShowsThemAgain()
    {
        var scene = Create();
        Press(scene);

        scene.Interaction.PointerMoved(145, 125, suspendSnap: true);
        Assert.Empty(scene.State.SmartMarkers);

        scene.Interaction.PointerMoved(146, 125);
        Assert.NotEmpty(scene.State.SmartMarkers);
    }

    [Fact]
    public void Labels_DisappearOnReleaseCancelAndAbort()
    {
        var release = Create();
        Press(release);
        release.Interaction.PointerMoved(145, 125);
        release.Interaction.PointerReleased(145, 125);
        Assert.Empty(release.State.SmartMarkers);

        var cancel = Create();
        Press(cancel);
        cancel.Interaction.PointerMoved(145, 125);
        cancel.Interaction.Cancel();
        Assert.Empty(cancel.State.SmartMarkers);

        var abort = Create();
        Press(abort);
        abort.Interaction.PointerMoved(145, 125);
        abort.Interaction.Abort();
        Assert.Empty(abort.State.SmartMarkers);
        Assert.Equal(100, Find(abort).X, 9);
    }

    [Fact]
    public void Labels_UseThePositionAfterTheSnap()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;
        Press(scene);

        // A arrives at x 108..158: the left edge is 2 from the grid line 110 and snaps (offset 2), so A is at x 110..160 and the gap to B is 140.
        scene.Interaction.PointerMoved(133, 125);

        Assert.Contains(scene.State.SmartMarkers, l => l.Axis == SnapAxis.X && Math.Abs(l.Value - 140) < 1e-9);
    }

    [Fact]
    public void Labels_WorkWithBothSnapsOff()
    {
        var scene = Create();
        Press(scene);

        scene.Interaction.PointerMoved(135, 125);

        Assert.NotEmpty(scene.State.SmartMarkers);
        Assert.Equal(110, Find(scene).X, 9);
    }

    [Fact]
    public void Labels_ARotatedShapeIsMeasuredFromTheBoundsOfItsOutline()
    {
        var turned = new RectangleShape { X = 500, Y = 100, Width = 100, Height = 20, Rotation = 90 };
        var scene = Create(true, turned);
        scene.Interaction.PointerPressed(550, 110);

        scene.Interaction.PointerMoved(551, 110);

        // The outline of the turned shape is 20 wide and 100 high around the center (550, 110): it spans x 540..560 and y 60..160.
        // B ends at x 350, so the gap on the left is 540 + 1 - 350 = 191.
        Assert.Contains(scene.State.SmartMarkers, l => l.Axis == SnapAxis.X && Math.Abs(l.Value - 191) < 1e-6);
    }

    [Fact]
    public void Move_IsStillOneUndoStep()
    {
        var scene = Create();
        var steps = scene.State.History.Steps.Count;
        Press(scene);

        scene.Interaction.PointerMoved(135, 125);
        scene.Interaction.PointerMoved(145, 125);
        scene.Interaction.PointerReleased(145, 125);

        Assert.Equal(steps + 1, scene.State.History.Steps.Count);
        Assert.Equal(120, Find(scene).X, 9);
    }

    [Fact]
    public void TheDraggedShapesAreNoNeighbours()
    {
        var scene = Create();
        Press(scene);

        scene.Interaction.PointerMoved(125, 125);

        // Nothing is left or above A, and A itself is not a neighbour of itself: only B (right) and C (below) give labels.
        Assert.Equal(2, scene.State.SmartMarkers.Length);
    }

    [Fact]
    public void SettingChangedAfterThePress_AppliesFromTheNextDrag()
    {
        var scene = Create(smart: false);
        Press(scene);
        scene.State.Snapping.SmartGuides = true;
        scene.Interaction.PointerMoved(145, 125);
        Assert.Empty(scene.State.SmartMarkers);
        scene.Interaction.PointerReleased(145, 125);

        scene.Interaction.PointerPressed(145, 125);
        scene.Interaction.PointerMoved(150, 125);

        Assert.NotEmpty(scene.State.SmartMarkers);
    }
}
