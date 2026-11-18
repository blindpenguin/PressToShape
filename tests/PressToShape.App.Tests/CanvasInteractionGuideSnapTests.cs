using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionGuideSnapTests
{
    // A is a 50 x 50 square at (100, 100). V is a vertical guide at x 300, H a horizontal one at y 400.
    private sealed record Scene(EditorState State, CanvasInteraction Interaction, RectangleShape A, Guide V, Guide H);

    private static Scene Create(Tool tool = Tool.Select, params Shape[] extra)
    {
        var a = new RectangleShape { X = 100, Y = 100, Width = 50, Height = 50 };
        var v = new Guide { Orientation = GuideOrientation.Vertical, Position = 300 };
        var h = new Guide { Orientation = GuideOrientation.Horizontal, Position = 400 };
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [a, .. extra]).AddGuide(v).AddGuide(h);
        var state = new EditorState(document) { ActiveTool = tool };
        state.Snapping.SnapToObjects = true;
        return new Scene(state, new CanvasInteraction(state), a, v, h);
    }

    private static Shape Find(Scene scene) => scene.State.Document.FindShape(scene.A.Id)!.Shape;

    [Fact]
    public void Move_SnapsAnEdgeToAGuide_AndTheGuideIsActiveWhileDragging()
    {
        var scene = Create();
        scene.Interaction.PointerPressed(125, 125);

        // The right edge arrives at 298: the vertical guide at 300 is 2 away.
        scene.Interaction.PointerMoved(125 + 148, 125);

        Assert.Equal([scene.V.Id], scene.State.ActiveGuideIds.ToArray());
        Assert.Empty(scene.State.SnapGuides);
        scene.Interaction.PointerReleased(125 + 148, 125);
        Assert.Equal(250, Find(scene).X, 9);
        Assert.Empty(scene.State.ActiveGuideIds);
    }

    [Fact]
    public void Move_SnapsToAHorizontalGuide()
    {
        var scene = Create();
        scene.Interaction.PointerPressed(125, 125);

        scene.Interaction.PointerMoved(125, 125 + 248);
        scene.Interaction.PointerReleased(125, 125 + 248);

        // The bottom edge arrives at 398 and snaps to 400.
        Assert.Equal(350, Find(scene).Y, 9);
    }

    [Fact]
    public void AGuideWinsATieAgainstAnObjectEdge()
    {
        var other = new RectangleShape { X = 300, Y = 600, Width = 50, Height = 50 };
        var scene = Create(Tool.Select, other);
        scene.Interaction.PointerPressed(125, 125);

        scene.Interaction.PointerMoved(125 + 148, 125);

        Assert.Equal([scene.V.Id], scene.State.ActiveGuideIds.ToArray());
        Assert.Empty(scene.State.SnapGuides.Where(g => g.Axis == SnapAxis.X));
    }

    [Fact]
    public void ANearerObjectEdge_WinsOverAGuide()
    {
        var other = new RectangleShape { X = 299, Y = 600, Width = 50, Height = 50 };
        var scene = Create(Tool.Select, other);
        scene.Interaction.PointerPressed(125, 125);

        scene.Interaction.PointerMoved(125 + 148, 125);

        Assert.Empty(scene.State.ActiveGuideIds);
        scene.Interaction.PointerReleased(125 + 148, 125);
        Assert.Equal(249, Find(scene).X, 9);
    }

    [Fact]
    public void NoSnap_WithSnapToObjectsOff_ShowGuidesOff_OrCtrl()
    {
        var off = Create();
        off.State.Snapping.SnapToObjects = false;
        off.Interaction.PointerPressed(125, 125);
        off.Interaction.PointerMoved(125 + 148, 125);
        off.Interaction.PointerReleased(125 + 148, 125);
        Assert.Equal(248, Find(off).X, 9);

        var hidden = Create();
        hidden.State.Snapping.ShowGuides = false;
        hidden.Interaction.PointerPressed(125, 125);
        hidden.Interaction.PointerMoved(125 + 148, 125);
        hidden.Interaction.PointerReleased(125 + 148, 125);
        Assert.Equal(248, Find(hidden).X, 9);

        var ctrl = Create();
        ctrl.Interaction.PointerPressed(125, 125);
        ctrl.Interaction.PointerMoved(125 + 148, 125, suspendSnap: true);
        Assert.Empty(ctrl.State.ActiveGuideIds);
        ctrl.Interaction.PointerReleased(125 + 148, 125, suspendSnap: true);
        Assert.Equal(248, Find(ctrl).X, 9);
    }

    [Fact]
    public void Resize_SnapsTheMovingEdgeToAGuide()
    {
        var scene = Create();
        scene.State.Select(scene.A.Id);

        scene.Interaction.PointerPressed(150, 125);
        scene.Interaction.PointerMoved(297, 125);
        scene.Interaction.PointerReleased(297, 125);

        Assert.Equal(300, Find(scene).X + Find(scene).Width, 9);
    }

    [Fact]
    public void Draw_SnapsTheCornersToGuides()
    {
        var scene = Create(Tool.Rectangle);

        scene.Interaction.PointerPressed(500, 500);
        scene.Interaction.PointerMoved(297, 403);
        scene.Interaction.PointerReleased(297, 403);

        var drawn = scene.State.Document.Layers[0].Shapes.Last();
        Assert.Equal((300.0, 400.0), (drawn.X, drawn.Y));
    }

    [Fact]
    public void Pen_PointsSnapToGuides()
    {
        var scene = Create(Tool.Pen);

        // 5 pixels from both guides: outside the press distance of a guide (4), inside the snap distance (8).
        scene.Interaction.PointerPressed(305, 395);
        scene.Interaction.PointerReleased(305, 395);
        scene.Interaction.PointerPressed(600, 700);
        scene.Interaction.PointerReleased(600, 700);
        scene.Interaction.FinishPen();

        var path = scene.State.Document.Layers[0].Shapes.OfType<PathShape>().Single();
        var node = path.ToDocumentNodes()[0];
        Assert.Equal((300.0, 400.0), (Math.Round(node.X, 6), Math.Round(node.Y, 6)));
    }

    [Fact]
    public void Nodes_SnapToGuides()
    {
        var path = PathShape.Create([new DocumentNode(0, 100), new DocumentNode(100, 100), new DocumentNode(200, 100)]);
        var scene = Create(Tool.DirectSelect, path);
        scene.State.Select(path.Id);

        scene.Interaction.PointerPressed(100, 100);
        scene.Interaction.PointerMoved(297, 103);
        scene.Interaction.PointerReleased(297, 103);

        var node = Assert.IsType<PathShape>(scene.State.Document.FindShape(path.Id)!.Shape).ToDocumentNodes()[1];
        Assert.Equal(300, node.X, 6);
    }

    [Fact]
    public void ADraggedGuide_DoesNotSnap_ToOtherGuides()
    {
        var scene = Create();

        scene.Interaction.PointerPressed(300, 50);
        scene.Interaction.PointerMoved(398, 50);
        scene.Interaction.PointerReleased(398, 50);

        Assert.Equal(398, scene.State.Document.FindGuide(scene.V.Id)!.Position, 9);
        Assert.Empty(scene.State.ActiveGuideIds);
    }

    [Fact]
    public void Abort_ClearsTheActiveGuides()
    {
        var scene = Create();
        scene.Interaction.PointerPressed(125, 125);
        scene.Interaction.PointerMoved(125 + 148, 125);

        scene.Interaction.Abort();

        Assert.Empty(scene.State.ActiveGuideIds);
    }

    [Fact]
    public void DeleteSelectedGuide_IsOneStep_AndUndoBringsItBack()
    {
        var scene = Create();
        scene.State.SelectGuide(scene.V.Id);
        var steps = scene.State.History.Steps.Count;

        scene.Interaction.DeleteSelectedGuide();

        Assert.Null(scene.State.Document.FindGuide(scene.V.Id));
        Assert.Equal(steps + 1, scene.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.DeleteGuide, scene.State.History.Steps[^1]);
        scene.State.Undo();
        Assert.NotNull(scene.State.Document.FindGuide(scene.V.Id));
    }

    [Fact]
    public void DeleteSelectedGuide_WithoutASelection_DoesNothing()
    {
        var scene = Create();
        var steps = scene.State.History.Steps.Count;

        scene.Interaction.DeleteSelectedGuide();

        Assert.Equal(steps, scene.State.History.Steps.Count);
        Assert.Equal(2, scene.State.Document.Guides.Length);
    }

    [Fact]
    public void DeleteSelection_DeletesASelectedGuide_ButAShapeSelectionKeepsPriority()
    {
        var scene = Create();
        scene.State.SelectGuide(scene.H.Id);
        scene.Interaction.DeleteSelection();
        Assert.Null(scene.State.Document.FindGuide(scene.H.Id));
        Assert.NotNull(scene.State.Document.FindShape(scene.A.Id));

        scene.State.Select(scene.A.Id);
        scene.Interaction.DeleteSelection();
        Assert.Null(scene.State.Document.FindShape(scene.A.Id));
        Assert.NotNull(scene.State.Document.FindGuide(scene.V.Id));
    }
}
