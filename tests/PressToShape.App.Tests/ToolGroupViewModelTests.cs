using System.Collections.Generic;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class ToolGroupViewModelTests
{
    private static (ToolGroupViewModel Group, TestShell Shell) Make(Tool inGroup, Dictionary<string, string>? german = null)
    {
        var shell = new TestShell(FakeLocalizer.Create(german: german));
        var group = new ToolGroupViewModel(ToolGroups.Of(inGroup), shell.Toolbox, shell.State, shell.LocalizerForTests);
        return (group, shell);
    }

    [Fact]
    public void New_StartsWithTheFirstToolOfTheGroup()
    {
        var (group, _) = Make(Tool.Ellipse);

        Assert.Equal(Tool.Rectangle, group.Current.Definition.Tool);
        Assert.Equal("Rectangle", group.Name);
        Assert.Equal(2, group.Members.Count);
    }

    [Fact]
    public void New_WhenTheActiveToolIsAMember_ShowsItAndIsActive()
    {
        var shell = new TestShell();
        shell.State.ActiveTool = Tool.Node;

        var group = new ToolGroupViewModel(ToolGroups.Of(Tool.Pen), shell.Toolbox, shell.State, shell.LocalizerForTests);

        Assert.True(group.IsActive);
        Assert.Equal(Tool.Node, group.Current.Definition.Tool);
    }

    [Fact]
    public void SelectCurrent_MakesTheCurrentToolActive()
    {
        var (group, shell) = Make(Tool.Rectangle);

        group.SelectCurrentCommand.Execute(null);

        Assert.Equal(Tool.Rectangle, shell.State.ActiveTool);
        Assert.True(group.IsActive);
    }

    [Fact]
    public void SelectMember_SetsTheActiveToolMakesItCurrentAndClosesTheFlyout()
    {
        var (group, shell) = Make(Tool.Rectangle);
        group.OpenFlyoutCommand.Execute(null);

        group.SelectMemberCommand.Execute(group.Members[1]);

        Assert.Equal(Tool.Ellipse, shell.State.ActiveTool);
        Assert.Equal(Tool.Ellipse, group.Current.Definition.Tool);
        Assert.False(group.IsFlyoutOpen);
        Assert.Equal("Ellipse", group.Name);
    }

    [Fact]
    public void SelectCurrent_AfterAChoiceInTheFlyout_UsesThatTool()
    {
        var (group, shell) = Make(Tool.Rectangle);
        group.SelectMemberCommand.Execute(group.Members[1]);
        shell.State.ActiveTool = Tool.Hand;

        group.SelectCurrentCommand.Execute(null);

        Assert.Equal(Tool.Ellipse, shell.State.ActiveTool);
    }

    [Fact]
    public void ActiveTool_ChangedFromOutsideToAMember_UpdatesCurrentAndTheIcon()
    {
        var (group, shell) = Make(Tool.Rectangle);
        var changed = new List<string?>();
        group.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        shell.Toolbox.TrySelectByKey('N');
        shell.State.ActiveTool = Tool.Ellipse;

        Assert.True(group.IsActive);
        Assert.Equal(Tool.Ellipse, group.Current.Definition.Tool);
        Assert.Contains(nameof(ToolGroupViewModel.Icon), changed);
        Assert.Contains(nameof(ToolGroupViewModel.Name), changed);
    }

    [Fact]
    public void ActiveTool_ChangedToAToolOfAnotherGroup_IsNotActiveAndKeepsCurrent()
    {
        var (group, shell) = Make(Tool.Rectangle);
        shell.State.ActiveTool = Tool.Ellipse;

        shell.State.ActiveTool = Tool.Pen;

        Assert.False(group.IsActive);
        Assert.Equal(Tool.Ellipse, group.Current.Definition.Tool);
    }

    [Fact]
    public void Name_FollowsTheLanguage()
    {
        var (group, shell) = Make(Tool.Rectangle, new Dictionary<string, string> { ["Rectangle"] = "Rechteck" });

        shell.LocalizerForTests.SetLanguage("de");

        Assert.Equal("Rechteck", group.Name);
    }

    [Fact]
    public void IsGroup_ForASingleTool_IsFalseAndOpenFlyoutDoesNothing()
    {
        var (group, _) = Make(Tool.Text);

        group.OpenFlyoutCommand.Execute(null);

        Assert.False(group.IsGroup);
        Assert.False(group.IsFlyoutOpen);
    }

    [Fact]
    public void OpenFlyout_ForAGroup_OpensIt()
    {
        var (group, _) = Make(Tool.Pen);

        group.OpenFlyoutCommand.Execute(null);

        Assert.True(group.IsGroup);
        Assert.True(group.IsFlyoutOpen);
    }
}
