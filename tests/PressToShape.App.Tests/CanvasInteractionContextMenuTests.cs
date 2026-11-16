using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasInteractionContextMenuTests
{
    // A group G of two squares at x 100..130 and 160..190, a plain square P at x 400, a hidden square at x 600 and a locked one at x 700.
    private sealed record Scene(
        EditorState State,
        CanvasInteraction Interaction,
        GroupShape Group,
        RectangleShape A,
        RectangleShape Plain);

    private static RectangleShape Square(double x, bool visible = true, bool locked = false) =>
        new() { X = x, Y = 100, Width = 30, Height = 30, IsVisible = visible, IsLocked = locked };

    private static Scene Create()
    {
        var a = Square(100);
        var group = new GroupShape { Children = [a, Square(160)] };
        var plain = Square(400);
        var document = Document.Create("Layer");
        document = document.AddShapes(document.Layers[0].Id, [group, plain, Square(600, visible: false), Square(700, locked: true)]);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        return new Scene(state, new CanvasInteraction(state), group, a, plain);
    }

    [Fact]
    public void SelectForContextMenu_OnASelectedObject_KeepsTheWholeSelection()
    {
        var scene = Create();
        scene.State.SetSelection([scene.Group.Id, scene.Plain.Id]);

        var mode = scene.Interaction.SelectForContextMenu(410, 110);

        Assert.Equal(RingMenuMode.Selection, mode);
        Assert.Equal([scene.Group.Id, scene.Plain.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void SelectForContextMenu_OnAnotherObject_SelectsOnlyIt()
    {
        var scene = Create();
        scene.State.SelectOnly(scene.Plain.Id);

        var mode = scene.Interaction.SelectForContextMenu(110, 110);

        Assert.Equal(RingMenuMode.Selection, mode);
        Assert.Equal([scene.Group.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void SelectForContextMenu_OnAMemberOfAGroup_SelectsTheGroup()
    {
        var scene = Create();

        scene.Interaction.SelectForContextMenu(110, 110);

        Assert.Equal([scene.Group.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void SelectForContextMenu_InAnEnteredGroup_SelectsTheMember()
    {
        var scene = Create();
        scene.State.EnterGroup(scene.Group.Id);

        scene.Interaction.SelectForContextMenu(110, 110);

        Assert.Equal([scene.A.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void SelectForContextMenu_OnEmptyCanvas_ClearsTheSelection()
    {
        var scene = Create();
        scene.State.SelectOnly(scene.Plain.Id);

        var mode = scene.Interaction.SelectForContextMenu(300, 300);

        Assert.Equal(RingMenuMode.EmptyCanvas, mode);
        Assert.False(scene.State.HasSelection);
    }

    [Theory]
    [InlineData(610)]
    [InlineData(710)]
    public void SelectForContextMenu_OnAHiddenOrLockedObject_CountsAsEmptyCanvas(double x)
    {
        var scene = Create();
        scene.State.SelectOnly(scene.Plain.Id);

        var mode = scene.Interaction.SelectForContextMenu(x, 110);

        Assert.Equal(RingMenuMode.EmptyCanvas, mode);
        Assert.False(scene.State.HasSelection);
    }

    [Fact]
    public void SelectForContextMenu_OnAHandleOfTheSelection_KeepsTheSelection()
    {
        var scene = Create();
        scene.State.SelectOnly(scene.Plain.Id);

        // Just outside the top left corner of the selected square, inside the grab area of its handle.
        var mode = scene.Interaction.SelectForContextMenu(396, 96);

        Assert.Equal(RingMenuMode.Selection, mode);
        Assert.Equal([scene.Plain.Id], scene.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void SelectForContextMenu_AddsNoHistoryStep()
    {
        var scene = Create();
        var steps = scene.State.History.Steps.Count;

        scene.Interaction.SelectForContextMenu(110, 110);
        scene.Interaction.SelectForContextMenu(410, 110);
        scene.Interaction.SelectForContextMenu(300, 300);

        Assert.Equal(steps, scene.State.History.Steps.Count);
    }
}
