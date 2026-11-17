using System;
using System.Linq;
using System.Text;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

// The editing features that exist for rectangles and ellipses must work for a path too. These tests run them on a path and check that
// nothing throws and that the picture ends where the feature says.
public class PathEditingFeaturesTests
{
    private static readonly Guid LayerId = new("aaaaaaaa-0000-0000-0000-000000000001");

    private static PathShape Triangle() =>
        PathShape.Create([new DocumentNode(100, 100), new DocumentNode(200, 100), new DocumentNode(150, 200)], isClosed: true);

    private static (EditorState State, PathShape Path) Create(Tool tool = Tool.Select)
    {
        var path = Triangle();
        var document = (Document.Create("x") with { Layers = [new Layer { Id = LayerId, Name = "A" }] }).AddShape(LayerId, path);
        return (new EditorState(document) { ActiveTool = tool }, path);
    }

    private static PathShape Current(EditorState state, Guid id) => Assert.IsType<PathShape>(state.Document.FindShape(id)!.Shape);

    [Fact]
    public void SelectTool_ClickInsideThePath_SelectsIt()
    {
        var (state, path) = Create();
        var interaction = new CanvasInteraction(state);

        interaction.PointerPressed(150, 130);
        interaction.PointerReleased(150, 130);

        Assert.Equal(path.Id, state.SelectedShapeId);
    }

    [Fact]
    public void SelectTool_ClickOutsideThePath_SelectsNothing()
    {
        var (state, _) = Create();
        var interaction = new CanvasInteraction(state);

        interaction.PointerPressed(110, 190);
        interaction.PointerReleased(110, 190);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void SelectTool_RubberBandAroundThePath_SelectsIt()
    {
        var (state, path) = Create();
        var interaction = new CanvasInteraction(state);

        interaction.PointerPressed(50, 50);
        interaction.PointerMoved(300, 300);
        interaction.PointerReleased(300, 300);

        Assert.Equal(path.Id, state.SelectedShapeId);
    }

    [Fact]
    public void SelectTool_DraggingThePath_MovesItAsOneStepAndKeepsItsShape()
    {
        var (state, path) = Create();
        var interaction = new CanvasInteraction(state);
        var steps = state.History.Steps.Count;

        interaction.PointerPressed(150, 130);
        interaction.PointerMoved(170, 140);
        interaction.PointerReleased(190, 160);

        var moved = Current(state, path.Id);
        Assert.Equal((path.X + 40, path.Y + 30), (moved.X, moved.Y));
        Assert.Equal(path.SubPaths[0].Nodes, moved.SubPaths[0].Nodes);
        Assert.Equal(steps + 1, state.History.Steps.Count);
    }

    [Fact]
    public void SelectTool_DraggingAResizeHandle_ScalesThePath()
    {
        var (state, path) = Create();
        state.Select(path.Id);
        var interaction = new CanvasInteraction(state);

        // The bottom right handle sits on the corner of the box.
        var cornerX = path.X + path.Width;
        var cornerY = path.Y + path.Height;
        interaction.PointerPressed(cornerX, cornerY);
        interaction.PointerMoved(cornerX + 50, cornerY + 50);
        interaction.PointerReleased(cornerX + 100, cornerY + 100);

        var scaled = Current(state, path.Id);
        Assert.Equal(path.Width + 100, scaled.Width, 6);
        Assert.Equal(path.Height + 100, scaled.Height, 6);
        Assert.Equal(path.SubPaths[0].Nodes, scaled.SubPaths[0].Nodes);
    }

    [Fact]
    public void SetSizeAndSetRotation_OnAPath_ChangeTheBoxAndKeepTheNodes()
    {
        var (_, path) = Create();
        var frame = SelectionGeometry.FrameOf([path])!;

        var resized = (PathShape)SelectionTransforms.SetSize([path], frame, 200, null, keepProportions: false)[0];
        var rotated = (PathShape)SelectionTransforms.SetRotation([path], frame, 45)[0];

        Assert.Equal(200, resized.Width, 6);
        Assert.Equal(path.SubPaths[0].Nodes, resized.SubPaths[0].Nodes);
        Assert.Equal(45, rotated.Rotation, 6);
        Assert.Equal(path.SubPaths[0].Nodes, rotated.SubPaths[0].Nodes);
    }

    [Fact]
    public void CopyAndPaste_APath_AddsACopyWithANewIdAndTheSameNodes()
    {
        var (state, path) = Create();
        state.Select(path.Id);
        var editor = new SelectionEditor(state);

        editor.Copy();
        editor.Paste(inPlace: false);

        var paths = state.Document.Layers[0].Shapes.OfType<PathShape>().ToList();
        Assert.Equal(2, paths.Count);
        var copy = paths.Single(p => p.Id != path.Id);
        Assert.Equal(path.SubPaths[0].Nodes, copy.SubPaths[0].Nodes);
        Assert.Equal(path.X + 10, copy.X);
        Assert.Equal(copy.Id, state.SelectedShapeId);
    }

    [Fact]
    public void Duplicate_APath_AddsACopy()
    {
        var (state, path) = Create();
        state.Select(path.Id);

        new SelectionEditor(state).Duplicate();

        Assert.Equal(2, state.Document.Layers[0].Shapes.OfType<PathShape>().Count());
    }

    [Fact]
    public void CutAndDelete_APath_RemoveIt()
    {
        var (state, path) = Create();
        state.Select(path.Id);

        new SelectionEditor(state).Cut();
        Assert.Empty(state.Document.Layers[0].Shapes);

        state.Undo();
        state.Select(path.Id);
        new CanvasInteraction(state).DeleteSelection();
        Assert.Empty(state.Document.Layers[0].Shapes);
    }

    [Fact]
    public void Nudge_APath_MovesIt()
    {
        var (state, path) = Create();
        state.Select(path.Id);

        new SelectionEditor(state).Nudge(5, -3);

        var moved = Current(state, path.Id);
        Assert.Equal((path.X + 5, path.Y - 3), (moved.X, moved.Y));
    }

    [Fact]
    public void GroupAndUngroup_APathWithARectangle_KeepsThePath()
    {
        var (state, path) = Create();
        var rectangle = new RectangleShape { X = 300, Y = 300, Width = 20, Height = 20 };
        state.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(LayerId, rectangle));
        state.SetSelection([path.Id, rectangle.Id]);
        var objects = new ObjectEditor(state);

        objects.Group();
        var group = Assert.IsType<GroupShape>(Assert.Single(state.Document.Layers[0].Shapes));
        Assert.Equal(path.X, group.X);
        objects.Ungroup();

        Assert.Equal(2, state.Document.Layers[0].Shapes.Count());
        Assert.Equal(path.SubPaths[0].Nodes, Current(state, path.Id).SubPaths[0].Nodes);
    }

    [Fact]
    public void Arrange_BringToFrontAndSendToBack_ReorderAPath()
    {
        var (state, path) = Create();
        var rectangle = new RectangleShape { Width = 20, Height = 20 };
        state.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(LayerId, rectangle));
        state.Select(path.Id);
        var objects = new ObjectEditor(state);

        objects.Arrange(ArrangeType.ToFront);
        Assert.Equal(path.Id, state.Document.Layers[0].Shapes[^1].Id);

        objects.Arrange(ArrangeType.ToBack);
        Assert.Equal(path.Id, state.Document.Layers[0].Shapes[0].Id);
    }

    [Fact]
    public void HideAndLock_APath_ChangeItsFlags()
    {
        var (state, path) = Create();
        state.Select(path.Id);
        var objects = new ObjectEditor(state);

        objects.LockSelection();
        Assert.True(Current(state, path.Id).IsLocked);

        objects.SetHidden([path.Id], true);
        Assert.False(Current(state, path.Id).IsVisible);
    }

    [Fact]
    public void Eyedropper_OnAPath_TakesItsFill()
    {
        var (state, path) = Create(Tool.Eyedropper);
        Color? picked = null;
        state.ColorPicked += (_, color) => picked = color;
        var interaction = new CanvasInteraction(state);

        interaction.PointerPressed(150, 130);

        Assert.Equal(path.Appearance.Fill, picked);
    }

    [Fact]
    public void PenToSaveAndExport_ADrawnPath_SurvivesTheFileAndShowsInTheSvg()
    {
        var state = new EditorState(Document.Create("x")) { ActiveTool = Tool.Pen };
        var interaction = new CanvasInteraction(state);
        foreach (var (x, y) in new[] { (0.0, 0.0), (100.0, 0.0), (100.0, 100.0) })
        {
            interaction.PointerPressed(x, y);
            interaction.PointerReleased(x, y);
        }

        interaction.FinishPen();

        var loaded = ProjectFile.Read(ProjectFile.Write(state.Document));
        Assert.True(loaded.Succeeded, loaded.Detail);
        var path = Assert.IsType<PathShape>(Assert.Single(loaded.Document!.Layers.SelectMany(l => l.Shapes)));
        Assert.Equal(3, path.SubPaths[0].Nodes.Length);
        var svg = Encoding.UTF8.GetString(SvgExport.ToUtf8(state.Document));
        Assert.Contains("<path", svg);
        Assert.Contains("d=\"M 0 0 L 100 0 L 100 100\"", svg);
    }

    [Fact]
    public void UndoAndRedo_OfEveryPenStep_RestoreThePath()
    {
        var state = new EditorState(Document.Create("x")) { ActiveTool = Tool.Pen };
        var interaction = new CanvasInteraction(state);
        foreach (var (x, y) in new[] { (0.0, 0.0), (100.0, 0.0), (100.0, 100.0) })
        {
            interaction.PointerPressed(x, y);
            interaction.PointerReleased(x, y);
        }

        interaction.FinishPen();
        var drawn = Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));

        state.Undo();
        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));

        state.Redo();
        Assert.Equal(drawn, Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes)));
    }
}
