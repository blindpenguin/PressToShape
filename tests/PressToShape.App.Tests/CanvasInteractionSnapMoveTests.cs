using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSnapMoveTests
{
    // A 50 x 50 square A at (100, 100) is dragged by its center (125, 125).
    private sealed record Scene(EditorState State, CanvasInteraction Interaction, RectangleShape A);

    private static Scene Create(params Shape[] others)
    {
        var a = new RectangleShape { X = 100, Y = 100, Width = 50, Height = 50 };
        var document = Document.Create("Layer");
        document = document.AddShapes(document.Layers[0].Id, [a, .. others]);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        return new Scene(state, new CanvasInteraction(state), a);
    }

    private static RectangleShape Square(double x, double y = 100, bool visible = true, bool locked = false) =>
        new() { X = x, Y = y, Width = 50, Height = 50, IsVisible = visible, IsLocked = locked };

    private static void Press(Scene scene) => scene.Interaction.PointerPressed(125, 125);

    private static void DragBy(Scene scene, double dx, double dy, bool ctrl = false)
    {
        Press(scene);
        scene.Interaction.PointerMoved(125 + dx, 125 + dy, suspendSnap: ctrl);
        scene.Interaction.PointerReleased(125 + dx, 125 + dy, suspendSnap: ctrl);
    }

    private static Shape Find(Scene scene, Guid id) => scene.State.Document.FindShape(id)!.Shape;

    [Fact]
    public void Move_SnapToGridOn_EdgeJumpsOntoTheLine()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;

        DragBy(scene, 21, 0);

        Assert.Equal(120, Find(scene, scene.A.Id).X, 9);
        Assert.Equal(100, Find(scene, scene.A.Id).Y, 9);
    }

    [Fact]
    public void Move_SnapToGridOn_NearestOfEdgeAndCenterWins()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;

        // left 123 is 3 from 120, the center 148 is 2 from 150.
        DragBy(scene, 23, 0);

        Assert.Equal(125, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_BothSnapsOff_DoesNotSnap()
    {
        var scene = Create();

        DragBy(scene, 21, 0);

        Assert.Equal(121, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_SnapToObjects_RightEdgeLinesUpWithTheEdgeOfAnotherShape()
    {
        var scene = Create(Square(300));
        scene.State.Snapping.SnapToObjects = true;

        DragBy(scene, 148, 0);

        Assert.Equal(250, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_SnapToObjects_ShowsAGuideWhileDraggingAndRemovesItOnRelease()
    {
        var scene = Create(Square(300));
        scene.State.Snapping.SnapToObjects = true;
        Press(scene);

        scene.Interaction.PointerMoved(125 + 148, 125);

        Assert.Contains(scene.State.SnapGuides, g => g.Axis == SnapAxis.X && Math.Abs(g.Position - 300) < 1e-9);
        scene.Interaction.PointerReleased(125 + 148, 125);
        Assert.Empty(scene.State.SnapGuides);
    }

    [Fact]
    public void Move_Cancelled_RemovesTheGuides()
    {
        var scene = Create(Square(300));
        scene.State.Snapping.SnapToObjects = true;
        Press(scene);
        scene.Interaction.PointerMoved(125 + 148, 125);

        scene.Interaction.Cancel();

        Assert.Empty(scene.State.SnapGuides);
    }

    [Fact]
    public void Move_Aborted_RemovesTheGuidesAndRestoresTheShape()
    {
        var scene = Create(Square(300));
        scene.State.Snapping.SnapToObjects = true;
        Press(scene);
        scene.Interaction.PointerMoved(125 + 148, 125);

        scene.Interaction.Abort();

        Assert.Empty(scene.State.SnapGuides);
        Assert.Equal(100, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_ObjectTargetBeatsANearerGridLine()
    {
        var scene = Create(Square(305));
        scene.State.Snapping.SnapToGrid = true;
        scene.State.Snapping.SnapToObjects = true;

        // The right edge arrives at 302: the grid line 300 is 2 away, the edge of the other shape at 305 is 3 away.
        DragBy(scene, 152, 0);

        Assert.Equal(255, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_CtrlHeld_SuspendsSnappingAndShowsNoGuide()
    {
        var scene = Create(Square(300));
        scene.State.Snapping.SnapToGrid = true;
        scene.State.Snapping.SnapToObjects = true;
        Press(scene);

        scene.Interaction.PointerMoved(125 + 148, 125, suspendSnap: true);

        Assert.Empty(scene.State.SnapGuides);
        scene.Interaction.PointerReleased(125 + 148, 125, suspendSnap: true);
        Assert.Equal(248, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_CtrlReleasedDuringTheDrag_SnapsAgainFromTheNextMove()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;
        Press(scene);

        scene.Interaction.PointerMoved(125 + 21, 125, suspendSnap: true);
        Assert.Equal(121, Find(scene, scene.A.Id).X, 9);
        scene.Interaction.PointerMoved(125 + 21, 125);
        scene.Interaction.PointerReleased(125 + 21, 125);

        Assert.Equal(120, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_SnapsToTheArtboardEdge()
    {
        var scene = Create();
        scene.State.Snapping.SnapToObjects = true;

        DragBy(scene, -96, 0);

        Assert.Equal(0, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_LockedShapeIsATarget_HiddenShapeIsNot()
    {
        var locked = Create(Square(300, locked: true));
        locked.State.Snapping.SnapToObjects = true;
        var hidden = Create(Square(300, visible: false));
        hidden.State.Snapping.SnapToObjects = true;

        DragBy(locked, 148, 0);
        DragBy(hidden, 148, 0);

        Assert.Equal(250, Find(locked, locked.A.Id).X, 9);
        Assert.Equal(248, Find(hidden, hidden.A.Id).X, 9);
    }

    [Fact]
    public void Move_TheDraggedShapeIsNoTargetOfItself()
    {
        var scene = Create();
        scene.State.Snapping.SnapToObjects = true;

        // Its own edge at 100 is 3 away from where it is now, it must not pull itself back.
        DragBy(scene, 103 - 100, 0);

        Assert.Equal(103, Find(scene, scene.A.Id).X, 9);
    }

    [Fact]
    public void Move_RotatedShape_SnapsByTheBoundsOfItsOutline()
    {
        // 100 x 20 turned by 90 degrees: the outline is 20 wide, 100 high, around the center (150, 110).
        var turned = new RectangleShape { X = 100, Y = 100, Width = 100, Height = 20, Rotation = 90 };
        var document = Document.Create("Layer");
        document = document.AddShape(document.Layers[0].Id, turned);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        state.Snapping.SnapToGrid = true;
        var interaction = new CanvasInteraction(state);

        interaction.PointerPressed(150, 110);
        interaction.PointerMoved(153, 110);
        interaction.PointerReleased(153, 110);

        var moved = state.Document.FindShape(turned.Id)!.Shape;
        Assert.Equal(140, moved.RotatedBounds.X, 9);
    }

    [Fact]
    public void Move_OneUndoStepForTheWholeDrag()
    {
        var scene = Create(Square(300));
        scene.State.Snapping.SnapToObjects = true;
        var steps = scene.State.History.Steps.Count;
        Press(scene);

        scene.Interaction.PointerMoved(125 + 100, 125);
        scene.Interaction.PointerMoved(125 + 148, 125);
        scene.Interaction.PointerReleased(125 + 148, 125);

        Assert.Equal(steps + 1, scene.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.MoveShape, scene.State.History.Steps.Last());
    }

    [Fact]
    public void Move_InsideAGroup_SnapsToTheSiblingsButNotToTheGroupBox()
    {
        var a = Square(100);
        var b = Square(160);
        var group = new GroupShape { Children = [a, b] };
        var document = Document.Create("Layer");
        document = document.AddShape(document.Layers[0].Id, group);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        state.Snapping.SnapToObjects = true;
        var interaction = new CanvasInteraction(state);
        interaction.DoubleClicked(110, 110);

        // a is 50 wide, its right edge is at 150 and b starts at 160: dragging by 8 puts it at 158, 2 from b.
        interaction.PointerPressed(110, 110);
        interaction.PointerMoved(118, 110);
        interaction.PointerReleased(118, 110);

        var moved = state.Document.FindShape(a.Id)!.Shape;
        Assert.Equal(110, moved.X, 9);
    }
}
