using System;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSnapResizeTests
{
    // A 100 x 100 square A at (100, 100) is selected. Its bottom right handle is at (200, 200), its right handle at (200, 150).
    private sealed record Scene(EditorState State, CanvasInteraction Interaction, RectangleShape A);

    private static Scene Create(double rotation = 0, params Shape[] others)
    {
        var a = new RectangleShape { X = 100, Y = 100, Width = 100, Height = 100, Rotation = rotation };
        var document = Document.Create("Layer");
        document = document.AddShapes(document.Layers[0].Id, [a, .. others]);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        state.Select(a.Id);
        return new Scene(state, new CanvasInteraction(state), a);
    }

    private static Shape Find(Scene scene) => scene.State.Document.FindShape(scene.A.Id)!.Shape;

    private static void Drag(Scene scene, double fromX, double fromY, double toX, double toY, bool shift = false, bool alt = false, bool ctrl = false)
    {
        scene.Interaction.PointerPressed(fromX, fromY);
        scene.Interaction.PointerMoved(toX, toY, shift, alt, ctrl);
        scene.Interaction.PointerReleased(toX, toY, shift, alt, ctrl);
    }

    [Fact]
    public void Resize_CornerSnapsToTheGridOnBothAxes()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;

        Drag(scene, 200, 200, 233, 247);

        var shape = Find(scene);
        Assert.Equal((100, 100, 130, 150), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Resize_SideHandleSnapsOnlyItsOwnAxis()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;

        Drag(scene, 200, 150, 217, 191);

        var shape = Find(scene);
        Assert.Equal(120, shape.Width, 9);
        Assert.Equal(100, shape.Height, 9);
    }

    [Fact]
    public void Resize_SnapToObjects_EdgeLinesUpWithAnotherShape()
    {
        var scene = Create(0, new RectangleShape { X = 330, Y = 400, Width = 40, Height = 40 });
        scene.State.Snapping.SnapToObjects = true;

        Drag(scene, 200, 150, 327, 150);

        Assert.Equal(230, Find(scene).Width, 9);
    }

    [Fact]
    public void Resize_OppositeEdgesStayFixed()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;

        Drag(scene, 200, 200, 233, 247);

        Assert.Equal(100, Find(scene).X, 9);
        Assert.Equal(100, Find(scene).Y, 9);
    }

    [Fact]
    public void Resize_KeepProportions_OnlyTheNearerAxisSnaps()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;

        // x 233 is 3 from 230, y 249 is 1 from 250: y snaps, and the square stays a square.
        Drag(scene, 200, 200, 233, 249, shift: true);

        var shape = Find(scene);
        Assert.Equal(150, shape.Height, 9);
        Assert.Equal(150, shape.Width, 9);
    }

    [Fact]
    public void Resize_FromCenterWithAlt_StillWorksWithSnapping()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;

        Drag(scene, 200, 200, 233, 233, alt: true);

        var shape = Find(scene);
        Assert.Equal(160, shape.Width, 9);
        Assert.Equal(150, shape.CenterX, 9);
    }

    [Fact]
    public void Resize_CtrlSuspendsSnapping()
    {
        var scene = Create();
        scene.State.Snapping.SnapToGrid = true;

        Drag(scene, 200, 200, 233, 247, ctrl: true);

        Assert.Equal(133, Find(scene).Width, 9);
    }

    [Fact]
    public void Resize_RotatedFrame_DoesNotSnap()
    {
        var snapped = Create(30);
        snapped.State.Snapping.SnapToGrid = true;
        snapped.State.Snapping.SnapToObjects = true;
        var plain = Create(30);

        var (x, y) = HandleOfRotated(snapped);
        Drag(snapped, x, y, x + 33, y + 47);
        Drag(plain, x, y, x + 33, y + 47);

        Assert.Equal(Find(plain) with { Id = Find(snapped).Id }, Find(snapped));
        Assert.Empty(snapped.State.SnapGuides);
    }

    // The bottom right handle of A turned by 30 degrees around its center (150, 150).
    private static (double X, double Y) HandleOfRotated(Scene scene)
    {
        var frame = SelectionGeometry.FrameOf([Find(scene)])!;
        return SelectionGeometry.HandlePosition(frame, HandleType.BottomRight, 100);
    }

    [Fact]
    public void Rotate_StillUsesTheFifteenDegreeSnapOnly()
    {
        var snapped = Create();
        snapped.State.Snapping.SnapToGrid = true;
        snapped.State.Snapping.SnapToObjects = true;
        var plain = Create();
        var frame = SelectionGeometry.FrameOf([snapped.A])!;
        var (hx, hy) = SelectionGeometry.HandlePosition(frame, HandleType.Rotate, 100);

        Drag(snapped, hx, hy, hx + 60, hy + 10);
        Drag(plain, hx, hy, hx + 60, hy + 10);

        Assert.Equal(Find(plain) with { Id = Find(snapped).Id }, Find(snapped));
        Assert.Empty(snapped.State.SnapGuides);
    }

    [Fact]
    public void Resize_ShowsAGuideWhileDraggingAndClearsItOnRelease()
    {
        var scene = Create(0, new RectangleShape { X = 330, Y = 400, Width = 40, Height = 40 });
        scene.State.Snapping.SnapToObjects = true;
        scene.Interaction.PointerPressed(200, 150);

        scene.Interaction.PointerMoved(327, 150);

        Assert.Contains(scene.State.SnapGuides, g => g.Axis == SnapAxis.X && Math.Abs(g.Position - 330) < 1e-9);
        scene.Interaction.PointerReleased(327, 150);
        Assert.Empty(scene.State.SnapGuides);
    }

    [Fact]
    public void Resize_Aborted_RestoresTheShapeAndRemovesTheGuides()
    {
        var scene = Create(0, new RectangleShape { X = 330, Y = 400, Width = 40, Height = 40 });
        scene.State.Snapping.SnapToObjects = true;
        scene.Interaction.PointerPressed(200, 150);
        scene.Interaction.PointerMoved(327, 150);

        scene.Interaction.Abort();

        Assert.Equal(100, Find(scene).Width, 9);
        Assert.Empty(scene.State.SnapGuides);
    }

    [Fact]
    public void Resize_BothSnapsOff_BehavesAsBefore()
    {
        var scene = Create();

        Drag(scene, 200, 200, 233, 247);

        var shape = Find(scene);
        Assert.Equal((133, 147), (shape.Width, shape.Height));
    }
}
