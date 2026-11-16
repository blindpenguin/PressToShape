using System;
using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class ToolGroupsTests
{
    [Fact]
    public void All_HasTheSixGroupsInOrder()
    {
        var groups = ToolGroups.All.Select(g => g.Tools.ToArray()).ToArray();

        Assert.Equal(
            [
                [Tool.Select, Tool.DirectSelect],
                [Tool.Pen, Tool.Node],
                [Tool.Text],
                [Tool.Rectangle, Tool.Ellipse],
                [Tool.Eyedropper],
                [Tool.Hand, Tool.Zoom],
            ],
            groups);
    }

    [Fact]
    public void All_ContainsEveryToolExactlyOnce()
    {
        var tools = ToolGroups.All.SelectMany(g => g.Tools).ToList();

        Assert.Equal(Enum.GetValues<Tool>().Length, tools.Count);
        Assert.Equal(Enum.GetValues<Tool>().OrderBy(t => t), tools.OrderBy(t => t));
    }

    [Fact]
    public void Of_ForEveryTool_ReturnsTheGroupThatHoldsIt()
    {
        foreach (var tool in Enum.GetValues<Tool>())
        {
            Assert.Contains(tool, ToolGroups.Of(tool).Tools);
        }
    }

    [Fact]
    public void DefaultTool_IsTheFirstToolOfTheGroup()
    {
        Assert.Equal(Tool.Select, ToolGroups.Of(Tool.DirectSelect).DefaultTool);
        Assert.Equal(Tool.Rectangle, ToolGroups.Of(Tool.Ellipse).DefaultTool);
        Assert.Equal(Tool.Hand, ToolGroups.Of(Tool.Zoom).DefaultTool);
    }

    [Fact]
    public void IsGroup_IsFalseOnlyForTextAndEyedropper()
    {
        var singles = ToolGroups.All.Where(g => !g.IsGroup).Select(g => g.DefaultTool).ToArray();

        Assert.Equal([Tool.Text, Tool.Eyedropper], singles);
    }
}
