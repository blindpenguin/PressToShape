using System.Linq;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class TabletToolColumnViewModelTests
{
    private static TabletToolColumnViewModel Make(out TestShell shell)
    {
        shell = new TestShell();
        var panel = new TabletPanelViewModel(shell.Inspector, shell.LocalizerForTests);
        return new TabletToolColumnViewModel(shell.Toolbox, shell.State, shell.LocalizerForTests, new TabletColorSwatchViewModel(shell.Inspector.Properties, panel, shell.LocalizerForTests));
    }

    [Fact]
    public void Groups_AreTheSixGroupsInOrder()
    {
        var column = Make(out _);

        var firstTools = column.Groups.Select(g => g.Members[0].Definition.Tool).ToArray();

        Assert.Equal([Tool.Select, Tool.Pen, Tool.Text, Tool.Rectangle, Tool.Eyedropper, Tool.Hand], firstTools);
    }

    [Fact]
    public void ColorSwatch_IsGiven_AndNullIsRefused()
    {
        var column = Make(out var shell);

        Assert.NotNull(column.ColorSwatch);
        Assert.Throws<System.ArgumentNullException>(() => new TabletToolColumnViewModel(shell.Toolbox, shell.State, shell.LocalizerForTests, null!));
    }

    [Fact]
    public void OpeningAFlyout_ClosesTheOtherOne()
    {
        var column = Make(out _);
        column.Groups[0].OpenFlyoutCommand.Execute(null);

        column.Groups[3].OpenFlyoutCommand.Execute(null);

        Assert.False(column.Groups[0].IsFlyoutOpen);
        Assert.True(column.Groups[3].IsFlyoutOpen);
    }

    [Fact]
    public void HasOpenFlyout_FollowsTheGroups()
    {
        var column = Make(out _);
        Assert.False(column.HasOpenFlyout);

        column.Groups[1].OpenFlyoutCommand.Execute(null);
        Assert.True(column.HasOpenFlyout);

        column.Groups[1].IsFlyoutOpen = false;
        Assert.False(column.HasOpenFlyout);
    }

    [Fact]
    public void CloseFlyouts_ClosesTheOpenOne()
    {
        var column = Make(out _);
        column.Groups[5].OpenFlyoutCommand.Execute(null);

        column.CloseFlyouts();

        Assert.False(column.HasOpenFlyout);
        Assert.False(column.Groups[5].IsFlyoutOpen);
    }
}
