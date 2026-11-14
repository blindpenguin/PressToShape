using System.Linq;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class ToolbarViewModelTests
{
    [Fact]
    public void Groups_ContainTheMockupButtons_InOrder()
    {
        var toolbar = new ToolbarViewModel(new EditorState());

        Assert.Equal(new[] { "New", "Open", "Save" }, toolbar.FileActions.Select(b => b.Name));
        Assert.Equal(new[] { "Undo", "Redo" }, toolbar.HistoryActions.Select(b => b.Name));
        Assert.Equal(new[] { "Align Left", "Align Center", "Align Right" }, toolbar.HorizontalAlignActions.Select(b => b.Name));
        Assert.Equal(new[] { "Rotate 90°", "Flip Horizontal" }, toolbar.TransformActions.Select(b => b.Name));
        Assert.Equal(new[] { "Align Top", "Align Middle", "Align Bottom" }, toolbar.VerticalAlignActions.Select(b => b.Name));
        Assert.Equal(new[] { "Distribute", "Export" }, toolbar.OtherActions.Select(b => b.Name));
    }

    [Fact]
    public void AllButtons_AreDisabledForNow()
    {
        var toolbar = new ToolbarViewModel(new EditorState());

        var all = toolbar.FileActions
            .Concat(toolbar.HistoryActions)
            .Concat(toolbar.HorizontalAlignActions)
            .Concat(toolbar.TransformActions)
            .Concat(toolbar.VerticalAlignActions)
            .Concat(toolbar.OtherActions);

        Assert.All(all, b => Assert.False(b.IsEnabled, b.Name));
    }

    [Fact]
    public void ToolToggles_FollowTheActiveTool()
    {
        var state = new EditorState();
        var toolbar = new ToolbarViewModel(state);

        Assert.True(toolbar.IsSelectToolActive);
        Assert.False(toolbar.IsNodeToolActive);

        state.ActiveTool = Tool.Node;

        Assert.False(toolbar.IsSelectToolActive);
        Assert.True(toolbar.IsNodeToolActive);
    }

    [Fact]
    public void CheckingAToggle_ChangesTheActiveTool()
    {
        var state = new EditorState();
        var toolbar = new ToolbarViewModel(state);

        toolbar.IsNodeToolActive = true;

        Assert.Equal(Tool.Node, state.ActiveTool);
    }

    [Fact]
    public void UncheckingTheActiveToggle_KeepsItChecked()
    {
        var state = new EditorState();
        var toolbar = new ToolbarViewModel(state);
        var raised = false;
        toolbar.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(ToolbarViewModel.IsSelectToolActive);

        toolbar.IsSelectToolActive = false;

        Assert.Equal(Tool.Select, state.ActiveTool);
        Assert.True(toolbar.IsSelectToolActive);
        Assert.True(raised); // makes the view re-check the toggle
    }

    [Fact]
    public void ToolToggleChange_RaisesNotifications()
    {
        var state = new EditorState();
        var toolbar = new ToolbarViewModel(state);
        var raised = new System.Collections.Generic.List<string?>();
        toolbar.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.ActiveTool = Tool.Pen;

        Assert.Contains(nameof(ToolbarViewModel.IsSelectToolActive), raised);
        Assert.Contains(nameof(ToolbarViewModel.IsNodeToolActive), raised);
    }

    [Fact]
    public void TransformFields_AreEmpty_AndDisabledWithoutSelection()
    {
        var state = new EditorState();
        var toolbar = new ToolbarViewModel(state);

        Assert.False(toolbar.HasSelection);
        Assert.Equal(string.Empty, toolbar.X);
        Assert.Equal(string.Empty, toolbar.Y);
        Assert.Equal(string.Empty, toolbar.Width);
        Assert.Equal(string.Empty, toolbar.Height);

        state.HasSelection = true;

        Assert.True(toolbar.HasSelection);
    }
}
