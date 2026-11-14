using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class ToolDefinitionsTests
{
    [Fact]
    public void All_ListsEveryTool_InMockupOrder()
    {
        var expected = new[]
        {
            Tool.Select, Tool.DirectSelect, Tool.Pen, Tool.Node, Tool.Text, Tool.Rectangle,
            Tool.Ellipse, Tool.Star, Tool.Spiral, Tool.Eyedropper, Tool.Hand, Tool.Zoom,
        };

        Assert.Equal(expected, ToolDefinitions.All.Select(d => d.Tool));
    }

    [Fact]
    public void All_CoversEveryEnumValue()
    {
        var allTools = System.Enum.GetValues<Tool>();

        Assert.Equal(allTools.Length, ToolDefinitions.All.Count);
    }

    [Fact]
    public void ShortcutKeys_AreUnique()
    {
        var keys = ToolDefinitions.All.Where(d => d.ShortcutKey.HasValue).Select(d => d.ShortcutKey!.Value).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Theory]
    [InlineData('v', "Select")]
    [InlineData('V', "Select")]
    [InlineData('a', "DirectSelect")]
    [InlineData('p', "Pen")]
    [InlineData('n', "Node")]
    [InlineData('t', "Text")]
    [InlineData('m', "Rectangle")]
    [InlineData('l', "Ellipse")]
    [InlineData('i', "Eyedropper")]
    [InlineData('h', "Hand")]
    [InlineData('z', "Zoom")]
    public void FindByShortcut_KnownKey_ReturnsTool(char key, string expectedTool)
    {
        var expected = System.Enum.Parse<Tool>(expectedTool);

        Assert.Equal(expected, ToolDefinitions.FindByShortcut(key)?.Tool);
    }

    [Theory]
    [InlineData('q')]
    [InlineData('1')]
    [InlineData(' ')]
    public void FindByShortcut_UnknownKey_ReturnsNull(char key)
    {
        Assert.Null(ToolDefinitions.FindByShortcut(key));
    }

    [Theory]
    [InlineData("Star")]
    [InlineData("Spiral")]
    public void StarAndSpiral_HaveNoShortcut(string toolName)
    {
        var tool = System.Enum.Parse<Tool>(toolName);

        Assert.Null(ToolDefinitions.Get(tool).ShortcutKey);
    }

    [Fact]
    public void ToolTip_ShowsShortcutWhenPresent()
    {
        Assert.Equal("Select (V)", ToolDefinitions.Get(Tool.Select).ToolTip);
        Assert.Equal("Star", ToolDefinitions.Get(Tool.Star).ToolTip);
    }
}
