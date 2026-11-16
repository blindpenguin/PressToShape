using System.Collections.Generic;
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
        var toolbox = new ToolboxViewModel(new EditorState(), FakeLocalizer.Create());

        Assert.Equal(ToolDefinitions.All.Select(d => d.Tool), toolbox.Tools.Select(t => t.Definition.Tool));
    }

    [Fact]
    public void Select_IsActiveByDefault_AndOnlyOneToolIsActive()
    {
        var toolbox = new ToolboxViewModel(new EditorState(), FakeLocalizer.Create());

        var active = toolbox.Tools.Where(t => t.IsActive).ToList();

        Assert.Single(active);
        Assert.Equal(Tool.Select, active[0].Definition.Tool);
    }

    [Fact]
    public void SelectToolCommand_ChangesActiveTool_AndHighlight()
    {
        var state = new EditorState();
        var toolbox = new ToolboxViewModel(state, FakeLocalizer.Create());

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
        var toolbox = new ToolboxViewModel(state, FakeLocalizer.Create());

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
        var toolbox = new ToolboxViewModel(state, FakeLocalizer.Create());

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
        var toolbox = new ToolboxViewModel(state, FakeLocalizer.Create());

        var handled = toolbox.TrySelectByKey(key);

        Assert.False(handled);
        Assert.Equal(Tool.Hand, state.ActiveTool);
    }

    [Fact]
    public void ToolbarItem_ToolTip_ContainsNameAndShortcut()
    {
        var toolbox = new ToolboxViewModel(new EditorState(), FakeLocalizer.Create());

        Assert.Equal("Select (V)", toolbox.Tools[0].ToolTip);
    }

    [Fact]
    public void SwatchCommands_ExistButCannotExecute()
    {
        var toolbox = new ToolboxViewModel(new EditorState(), FakeLocalizer.Create());

        Assert.False(toolbox.SwapColorsCommand.CanExecute(null));
        Assert.False(toolbox.ResetColorsCommand.CanExecute(null));
    }

    [Fact]
    public void ToolNamesAndTooltips_AreTranslated_AndRefreshOnLanguageChange()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string>
        {
            ["Select"] = "Auswählen",
        });
        var toolbox = new ToolboxViewModel(new EditorState(), localizer);
        var select = toolbox.Tools.Single(t => t.Definition.Tool == Tool.Select);
        var raised = new List<string?>();
        select.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        Assert.Equal("Select", select.Name);
        Assert.Equal("Select (V)", select.ToolTip);

        localizer.SetLanguage("de");

        Assert.Equal("Auswählen", select.Name);
        Assert.Equal("Auswählen (V)", select.ToolTip);
        Assert.Contains(string.Empty, raised);
    }
}
