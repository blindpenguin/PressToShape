using System;
using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesSectionsTests
{
    private const PropertiesSection Selection =
        PropertiesSection.Transform | PropertiesSection.Appearance | PropertiesSection.Align | PropertiesSection.QuickActions;

    [Fact]
    public void For_SelectionTool_ShowsTransformAppearanceAlignAndQuickActions()
    {
        foreach (var tool in new[] { Tool.Select, Tool.DirectSelect })
        {
            Assert.Equal(Selection, PropertiesSections.For(tool));
        }
    }

    [Fact]
    public void For_ShapeOrTextTool_ShowsAppearanceOnly()
    {
        foreach (var tool in new[] { Tool.Rectangle, Tool.Ellipse, Tool.Star, Tool.Spiral, Tool.Text })
        {
            Assert.Equal(PropertiesSection.Appearance, PropertiesSections.For(tool));
        }
    }

    [Fact]
    public void For_PathTool_ShowsAppearanceAndPath()
    {
        foreach (var tool in new[] { Tool.Pen, Tool.Node })
        {
            Assert.Equal(PropertiesSection.Appearance | PropertiesSection.Path, PropertiesSections.For(tool));
        }
    }

    [Fact]
    public void For_ToolWithoutProperties_ShowsNothing()
    {
        foreach (var tool in new[] { Tool.Eyedropper, Tool.Hand, Tool.Zoom })
        {
            Assert.Equal(PropertiesSection.None, PropertiesSections.For(tool));
        }
    }

    [Fact]
    public void For_EveryTool_IsDefinedAndShowsPathOnlyForPenAndNode()
    {
        var tools = Enum.GetValues<Tool>();

        var withPath = tools.Where(tool => PropertiesSections.For(tool).HasFlag(PropertiesSection.Path)).ToArray();

        Assert.Equal([Tool.Pen, Tool.Node], withPath);
    }

    [Fact]
    public void For_UndefinedTool_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PropertiesSections.For((Tool)999));
    }
}
