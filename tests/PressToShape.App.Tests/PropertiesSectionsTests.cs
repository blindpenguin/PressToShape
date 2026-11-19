using System;
using System.Linq;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesSectionsTests
{
    private const PropertiesSection Selection =
        PropertiesSection.Transform | PropertiesSection.Corners | PropertiesSection.Appearance | PropertiesSection.Text | PropertiesSection.Align
        | PropertiesSection.PathOperations | PropertiesSection.QuickActions;

    [Fact]
    public void For_SelectTool_ShowsTransformAppearanceAlignPathOperationsAndQuickActions()
    {
        Assert.Equal(Selection, PropertiesSections.For(Tool.Select));
    }

    [Fact]
    public void For_DirectSelect_ShowsTheSelectionSectionsAndAlsoPathAndNode()
    {
        Assert.Equal(Selection | PropertiesSection.Path | PropertiesSection.Node, PropertiesSections.For(Tool.DirectSelect));
    }

    [Fact]
    public void For_ShapeTools_ShowAppearanceOnly()
    {
        foreach (var tool in new[] { Tool.Rectangle, Tool.Ellipse })
        {
            Assert.Equal(PropertiesSection.Appearance, PropertiesSections.For(tool));
        }
    }

    [Fact]
    public void For_TextTool_ShowsAppearanceAndText()
    {
        Assert.Equal(PropertiesSection.Appearance | PropertiesSection.Text, PropertiesSections.For(Tool.Text));
    }

    [Fact]
    public void For_Pen_ShowsAppearanceAndPath()
    {
        Assert.Equal(PropertiesSection.Appearance | PropertiesSection.Path, PropertiesSections.For(Tool.Pen));
    }

    [Fact]
    public void For_NodeTool_ShowsAppearancePathAndNode()
    {
        Assert.Equal(PropertiesSection.Appearance | PropertiesSection.Path | PropertiesSection.Node, PropertiesSections.For(Tool.Node));
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
    public void For_EveryTool_IsDefinedAndShowsPathForPenDirectSelectAndNode_AndNodeForDirectSelectAndNode()
    {
        var tools = Enum.GetValues<Tool>();

        var withPath = tools.Where(tool => PropertiesSections.For(tool).HasFlag(PropertiesSection.Path)).ToArray();
        var withNode = tools.Where(tool => PropertiesSections.For(tool).HasFlag(PropertiesSection.Node)).ToArray();

        Assert.Equal([Tool.DirectSelect, Tool.Pen, Tool.Node], withPath);
        Assert.Equal([Tool.DirectSelect, Tool.Node], withNode);
    }

    [Fact]
    public void For_EveryTool_ShowsTheCornersSectionOnlyForSelectAndDirectSelect()
    {
        var withCorners = Enum.GetValues<Tool>().Where(tool => PropertiesSections.For(tool).HasFlag(PropertiesSection.Corners)).ToArray();

        Assert.Equal([Tool.Select, Tool.DirectSelect], withCorners);
    }

    [Fact]
    public void For_EveryTool_ShowsThePathOperationsSectionOnlyForSelectAndDirectSelect()
    {
        var withPathOperations = Enum.GetValues<Tool>().Where(tool => PropertiesSections.For(tool).HasFlag(PropertiesSection.PathOperations)).ToArray();

        Assert.Equal([Tool.Select, Tool.DirectSelect], withPathOperations);
    }

    [Fact]
    public void For_UndefinedTool_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PropertiesSections.For((Tool)999));
    }
}
