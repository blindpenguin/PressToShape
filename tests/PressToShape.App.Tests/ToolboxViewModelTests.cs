using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class ToolboxViewModelTests
{
    [Fact]
    public void Tools_AreListedInMockupOrder()
    {
        var toolbox = new ToolboxViewModel(new EditorState());

        Assert.Equal(ToolDefinitions.All.Select(d => d.Tool), toolbox.Tools.Select(t => t.Definition.Tool));
    }

    [Fact]
    public void Tools_AreSplitIntoPairedGridAndSingleColumn()
    {
        var toolbox = new ToolboxViewModel(new EditorState());

        Assert.Equal(8, toolbox.PairedTools.Count);
        Assert.Equal(4, toolbox.SingleTools.Count);
        Assert.Equal(toolbox.Tools, toolbox.PairedTools.Concat(toolbox.SingleTools));
        Assert.Equal(Tool.Spiral, toolbox.SingleTools[0].Definition.Tool);
    }

    [Fact]
    public void Select_IsActiveByDefault_AndOnlyOneToolIsActive()
    {
        var toolbox = new ToolboxViewModel(new EditorState());

        var active = toolbox.Tools.Where(t => t.IsActive).ToList();

        Assert.Single(active);
        Assert.Equal(Tool.Select, active[0].Definition.Tool);
    }

    [Fact]
    public void SelectToolCommand_ChangesActiveTool_AndHighlight()
    {
        var state = new EditorState();
        var toolbox = new ToolboxViewModel(state);

        toolbox.Tools.Single(t => t.Definition.Tool == Tool.Pen).SelectCommand.Execute(null);

        Assert.Equal(Tool.Pen, state.ActiveTool);
        var active = toolbox.Tools.Where(t => t.IsActive).ToList();
        Assert.Single(active);
        Assert.Equal(Tool.Pen, active[0].Definition.Tool);
    }

    [Fact]
    public void ToolChangedElsewhere_IsReflected()
    {
        var state = new EditorState();
        var toolbox = new ToolboxViewModel(state);

        state.ActiveTool = Tool.Ellipse;

        Assert.Equal(Tool.Ellipse, toolbox.ActiveTool);
        Assert.Equal(Tool.Ellipse, toolbox.Tools.Single(t => t.IsActive).Definition.Tool);
    }

    [Theory]
    [InlineData('v', "Select")]
    [InlineData('P', "Pen")]
    [InlineData('z', "Zoom")]
    public void TrySelectByKey_KnownKey_SelectsTool(char key, string expectedTool)
    {
        var state = new EditorState();
        var toolbox = new ToolboxViewModel(state);

        var handled = toolbox.TrySelectByKey(key);

        Assert.True(handled);
        Assert.Equal(System.Enum.Parse<Tool>(expectedTool), state.ActiveTool);
    }

    [Theory]
    [InlineData('q')]
    [InlineData('5')]
    [InlineData(' ')]
    public void TrySelectByKey_UnknownKey_ReturnsFalse_AndKeepsTool(char key)
    {
        var state = new EditorState { ActiveTool = Tool.Hand };
        var toolbox = new ToolboxViewModel(state);

        var handled = toolbox.TrySelectByKey(key);

        Assert.False(handled);
        Assert.Equal(Tool.Hand, state.ActiveTool);
    }

    [Fact]
    public void ToolbarItem_ToolTip_ContainsNameAndShortcut()
    {
        var toolbox = new ToolboxViewModel(new EditorState());

        Assert.Equal("Select (V)", toolbox.Tools[0].ToolTip);
    }

    [Fact]
    public void SwatchCommands_ExistButCannotExecute()
    {
        var toolbox = new ToolboxViewModel(new EditorState());

        Assert.False(toolbox.SwapColorsCommand.CanExecute(null));
        Assert.False(toolbox.ResetColorsCommand.CanExecute(null));
    }
}
