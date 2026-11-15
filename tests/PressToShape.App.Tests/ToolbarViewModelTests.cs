using System.Collections.Generic;
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
        var toolbar = new ToolbarViewModel(new EditorState(), FakeLocalizer.Create(), new SwitchableCommand(false), new SwitchableCommand(false));

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
        var toolbar = new ToolbarViewModel(new EditorState(), FakeLocalizer.Create(), new SwitchableCommand(false), new SwitchableCommand(false));

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
        var toolbar = new ToolbarViewModel(state, FakeLocalizer.Create(), new SwitchableCommand(false), new SwitchableCommand(false));

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
        var toolbar = new ToolbarViewModel(state, FakeLocalizer.Create(), new SwitchableCommand(false), new SwitchableCommand(false));

        toolbar.IsNodeToolActive = true;

        Assert.Equal(Tool.Node, state.ActiveTool);
    }

    [Fact]
    public void UncheckingTheActiveToggle_KeepsItChecked()
    {
        var state = new EditorState();
        var toolbar = new ToolbarViewModel(state, FakeLocalizer.Create(), new SwitchableCommand(false), new SwitchableCommand(false));
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
        var toolbar = new ToolbarViewModel(state, FakeLocalizer.Create(), new SwitchableCommand(false), new SwitchableCommand(false));
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
        var toolbar = new ToolbarViewModel(state, FakeLocalizer.Create(), new SwitchableCommand(false), new SwitchableCommand(false));

        Assert.False(toolbar.HasSelection);
        Assert.Equal(string.Empty, toolbar.X);
        Assert.Equal(string.Empty, toolbar.Y);
        Assert.Equal(string.Empty, toolbar.Width);
        Assert.Equal(string.Empty, toolbar.Height);

        state.HasSelection = true;

        Assert.True(toolbar.HasSelection);
    }

    [Fact]
    public void ToolbarButtonNames_AreTranslated_AndRefreshOnLanguageChange()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string>
        {
            ["Undo"] = "Rückgängig",
            ["Align Left"] = "Links ausrichten",
        });
        var toolbar = new ToolbarViewModel(new EditorState(), localizer, new SwitchableCommand(false), new SwitchableCommand(false));
        var undo = toolbar.HistoryActions[0];
        var raised = new List<string?>();
        undo.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        Assert.Equal("Undo", undo.Name);

        localizer.SetLanguage("de");

        Assert.Equal("Rückgängig", undo.Name);
        Assert.Equal("Links ausrichten", toolbar.HorizontalAlignActions[0].Name);
        Assert.Equal("Redo", toolbar.HistoryActions[1].Name); // not translated: English fallback
        Assert.Contains(string.Empty, raised);
    }

    [Fact]
    public void Button_FollowsTheCommandsCanExecute()
    {
        var command = new SwitchableCommand(false);
        var button = new ToolbarButtonViewModel(FakeLocalizer.Create(), "Undo", Material.Icons.MaterialIconKind.Undo, command);
        var raised = new List<string?>();
        button.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        Assert.False(button.IsEnabled);

        command.Set(true);
        Assert.True(button.IsEnabled);
        Assert.Contains(nameof(ToolbarButtonViewModel.IsEnabled), raised);

        command.Set(false);
        Assert.False(button.IsEnabled);
    }

    [Fact]
    public void Button_WithoutACommand_IsDisabled()
    {
        var button = new ToolbarButtonViewModel(FakeLocalizer.Create(), "Undo", Material.Icons.MaterialIconKind.Undo);

        Assert.False(button.IsEnabled);
    }

    [Fact]
    public void UndoAndRedoButtons_UseTheGivenCommands()
    {
        var undo = new SwitchableCommand(true);
        var redo = new SwitchableCommand(false);
        var toolbar = new ToolbarViewModel(new EditorState(), FakeLocalizer.Create(), undo, redo);

        Assert.True(toolbar.HistoryActions[0].IsEnabled);
        Assert.False(toolbar.HistoryActions[1].IsEnabled);

        toolbar.HistoryActions[0].Command!.Execute(null);
        Assert.Equal(1, undo.Executions);
    }
}
